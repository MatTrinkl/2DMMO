using Mmo.Shared.Interfaces;

namespace Mmo.Server.Tests.GameServer;

internal sealed class SlowGameServer : Server.GameLoop.GameServer
{
    private readonly TimeSpan _tickWorkDuration;

    public SlowGameServer(ILog log, TimeSpan tickWorkDuration)
        : base(log)
    {
        _tickWorkDuration = tickWorkDuration;
    }

    // Slow input phase.
    protected override async Task InputPhaseAsync(CancellationToken cancellationToken) =>
        await Task.Delay(_tickWorkDuration);

    protected override Task UpdatePhaseAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    protected override Task OutputPhaseAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
