using System.Diagnostics;
using Mmo.Shared;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.GameLoop;

public class GameServer
{
    private readonly ILog _log;

    public long CurrentTick { get; private set; }

    public GameServer(ILog log)
    {
        _log = log;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _log.Info("GameServer starting with {TickRate} Hz...", SharedConstants.TickRate);

        var stopwatch = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            var tickStart = stopwatch.Elapsed;
            CurrentTick++;

            // 1️⃣ INPUT PHASE (~5ms)
            await InputPhaseAsync(cancellationToken);

            // 2️⃣ UPDATE PHASE (~5ms)
            await UpdatePhaseAsync(cancellationToken);

            // 3️⃣ OUTPUT PHASE (~10ms)
            await OutputPhaseAsync(cancellationToken);

            var elapsed = stopwatch.Elapsed - tickStart;
            var elapsedMs = elapsed.TotalMilliseconds;
            var budgetMs = SharedConstants.TickDuration.TotalMilliseconds;

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

            // Optional: Debug-Log für erfolgreiche Ticks
           /* _log.Debug(
                "Tick {Tick} completed in {ElapsedMs:F2} ms (budget: {BudgetMs:F2} ms)",
                CurrentTick,
                elapsedMs,
                budgetMs);*/

            var remaining = SharedConstants.TickDuration - elapsed;

            try
            {
                // 4️⃣ WAIT – restliche Zeit bis 33.33ms warten
                await Task.Delay(remaining, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _log.Info("GameServer stopped after {Ticks} ticks.", CurrentTick);
    }

    // --- Phasen-Stubs -------------------------------------------------------

    private Task InputPhaseAsync(CancellationToken cancellationToken)
    {
        // TODO: MessageQueue auslesen, Messages nach Typ verteilen
        return Task.CompletedTask;
    }

    private Task UpdatePhaseAsync(CancellationToken cancellationToken)
    {
        // TODO: Positionen validieren, Kollisionen prüfen, GameState aktualisieren
        return Task.CompletedTask;
    }

    private Task OutputPhaseAsync(CancellationToken cancellationToken)
    {
        // TODO: WorldState zusammenstellen, an alle Clients senden
        return Task.CompletedTask;
    }
}
