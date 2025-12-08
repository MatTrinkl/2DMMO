using Microsoft.Extensions.Logging;
using Mmo.Server;
using Mmo.Server.GameLoop;
using Mmo.Shared.Interfaces;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddSimpleConsole(options => { options.TimestampFormat = "[HH:mm:ss] "; })
                .SetMinimumLevel(LogLevel.Information);
        });
        ILogger coreLogger = loggerFactory.CreateLogger("GameServer");
        ILog log = new LoggerAdapter(coreLogger);

        var server = new GameServer(log);

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            log.Info("Ctrl+C received. Shutting down game server...");
            cts.Cancel();
        };

        try
        {
            await server.StartServerAsync(cts.Token);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Unhandled exception in GameServer.");
        }
    }
}
