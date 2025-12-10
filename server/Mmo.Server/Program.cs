using Microsoft.Extensions.Logging;
using Mmo.Server.GameLoop;
using Mmo.Server.Networking;
using Mmo.Shared;

namespace Mmo.Server;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        // ══════════════════════════════════════════════════════════
        // LOGGING SETUP
        // ══════════════════════════════════════════════════════════
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                . AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "[HH:mm:ss] ";
                    options.SingleLine = true;
                })
                .SetMinimumLevel(LogLevel. Debug);
        });

        ILogger coreLogger = loggerFactory.CreateLogger("Mmo.Server");
        var log = new LoggerAdapter(coreLogger);

        log.Info("═══════════════════════════════════════════════════");
        log.Info("  2DMMO Server v0.1.0");
        log.Info("  Port:   {Port} | Tick Rate: {TickRate} Hz",
            SharedConstants.DefaultPort, SharedConstants. TickRate);
        log.Info("═══════════════════════════════════════════════════");

        // ══════════════════════════════════════════════════════════
        // SERVER SETUP
        // ══════════════════════════════════════════════════════════
        var networkServer = new NetworkServer(SharedConstants.DefaultPort, log);
        var gameServer = new GameServer(log, networkServer);

        // ══════════════════════════════════════════════════════════
        // CANCELLATION
        // ══════════════════════════════════════════════════════════
        using var cts = new CancellationTokenSource();

        // Flag to prevent double-cancellation
        var shutdownRequested = false;
        var shutdownLock = new object();

        void RequestShutdown(string source)
        {
            lock (shutdownLock)
            {
                if (shutdownRequested) return;
                shutdownRequested = true;

                log.Info("{Source} received.   Initiating graceful shutdown...", source);

                try
                {
                    if (! cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed, ignore
                }
            }
        }

        // Handle Ctrl+C
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;  // Prevent immediate termination
            RequestShutdown("Ctrl+C");
        };

        // Handle SIGTERM (for Docker/Kubernetes) - use weak reference pattern
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            RequestShutdown("SIGTERM");
        };

        // ══════════════════════════════════════════════════════════
        // RUN SERVER
        // ══════════════════════════════════════════════════════════
        try
        {
            log.Info("Starting server...");

            await Task.WhenAll(
                networkServer.  RunAsync(cts. Token),
                gameServer. StartServerAsync(cts.Token)
            );

            log.Info("Server shutdown completed gracefully.");
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
            log.Info("Server shutdown completed gracefully.");
        }
        catch (Exception ex)
        {
            log.Error("Unhandled exception in server: {Error}", ex. Message);
            log.Error("Stack trace: {StackTrace}", ex.StackTrace ??  "N/A");
            Environment.ExitCode = 1;
        }
        finally
        {
            networkServer.Dispose();
            log.Info("Server resources cleaned up.   Goodbye!");
        }
    }
}
