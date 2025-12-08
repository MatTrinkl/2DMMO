using System.Diagnostics;
using Mmo.Shared;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.GameLoop;

/// <summary>
///     This class is the core server structure. All communication will be done with an instance of this class.
/// </summary>
public class GameServer
{
    /// <summary>
    ///     Reference of logging-tool.
    /// </summary>
    private readonly ILog _log;

    /// <summary>
    ///     Creates a new GameServer object.
    /// </summary>
    /// <param name="log">The logging interface.</param>
    public GameServer(ILog log)
    {
        _log = log;
        CurrentTick = 0;
        IsRunning = true;
    }

    /// <summary>
    ///     true if the server is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    ///     The current tick of the server.
    /// </summary>
    public long CurrentTick { get; private set; }

    /// <summary>
    ///     Starts the server and keep its loop until the cancellation is requested.
    ///     Then the final tick will run and then server shuts down.
    /// </summary>
    /// <param name="cancellationToken">The token contains the <see cref="CancellationToken.IsCancellationRequested" />.</param>
    public async Task StartServerAsync(CancellationToken cancellationToken)
    {
        _log.Info("GameServer starting with {TickRate} Hz...", SharedConstants.TickRate);

        var stopwatch = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            TimeSpan tickStart = stopwatch.Elapsed;
            CurrentTick++;

            // 1️⃣ INPUT PHASE (~5ms)
            await InputPhaseAsync(cancellationToken);

            // 2️⃣ UPDATE PHASE (~5ms)
            await UpdatePhaseAsync(cancellationToken);

            // 3️⃣ OUTPUT PHASE (~10ms)
            await OutputPhaseAsync(cancellationToken);

            TimeSpan elapsed = stopwatch.Elapsed - tickStart;
            double elapsedMs = elapsed.TotalMilliseconds;
            double budgetMs = SharedConstants.TickDuration.TotalMilliseconds;

            // Tick-Overrun Logging (WARNING Level)
            if (elapsed > SharedConstants.TickDuration)
            {
                _log.Warn(
                    "Tick {Tick} overrun: {ElapsedMs:F2} ms (budget: {BudgetMs:F2} ms)",
                    CurrentTick,
                    elapsedMs,
                    budgetMs);

                // No Delay - directly to next Tick
                continue;
            }

            TimeSpan remaining = SharedConstants.TickDuration - elapsed;

            try
            {
                // 4️⃣ WAIT – until the rest time of the 33.3ms is passed
                await Task.Delay(remaining, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        IsRunning = false;
        _log.Info("GameServer stopped after {Ticks} ticks.", CurrentTick);
    }


    /// <summary>
    ///     This is a placeholder.
    /// </summary>
    /// <param name="cancellationToken">The token contains the <see cref="CancellationToken.IsCancellationRequested" />.</param>
    /// <returns>Return a reference to the task</returns>
    protected virtual Task InputPhaseAsync(CancellationToken cancellationToken)
    {
        // TODO: MessageQueue auslesen, Messages nach Typ verteilen
        return Task.CompletedTask;
    }

    /// <summary>
    ///     This is a placeholder.
    /// </summary>
    /// <param name="cancellationToken">The token contains the <see cref="CancellationToken.IsCancellationRequested" />.</param>
    /// <returns>Return a reference to the task</returns>
    protected virtual Task UpdatePhaseAsync(CancellationToken cancellationToken)
    {
        // TODO: Positionen validieren, Kollisionen prüfen, GameState aktualisieren
        return Task.CompletedTask;
    }

    /// <summary>
    ///     This is a placeholder.
    /// </summary>
    /// <param name="cancellationToken">The token contains the <see cref="CancellationToken.IsCancellationRequested" />.</param>
    /// <returns>Return a reference to the task</returns>
    protected virtual Task OutputPhaseAsync(CancellationToken cancellationToken)
    {
        // TODO: WorldState zusammenstellen, an alle Clients senden
        return Task.CompletedTask;
    }
}
