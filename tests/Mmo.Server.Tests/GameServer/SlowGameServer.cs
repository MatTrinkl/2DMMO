using Mmo.Server.Networking;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Tests.GameServer;

/// <summary>
///     A test implementation of GameServer that simulates slow tick phases
///     for testing tick overrun handling.
/// </summary>
internal sealed class SlowGameServer(
    ILog log,
    INetworkServer networkServer,
    TimeSpan? inputPhaseDuration = null,
    TimeSpan? updatePhaseDuration = null,
    TimeSpan? outputPhaseDuration = null)
    : GameLoop.GameServer(log, networkServer)
{
    private readonly TimeSpan _inputPhaseDuration = inputPhaseDuration ?? TimeSpan.Zero;
    private readonly TimeSpan _outputPhaseDuration = outputPhaseDuration ?? TimeSpan.Zero;
    private readonly TimeSpan _updatePhaseDuration = updatePhaseDuration ?? TimeSpan.Zero;

    /// <summary>
    ///     Convenience constructor for simple slow tick simulation.
    /// </summary>
    public SlowGameServer(ILog log, INetworkServer networkServer, TimeSpan tickWorkDuration)
        : this(log, networkServer, inputPhaseDuration: tickWorkDuration)
    {
    }

    protected override async Task InputPhaseAsync(CancellationToken cancellationToken)
    {
        if (_inputPhaseDuration > TimeSpan.Zero)
            try
            {
                await Task.Delay(_inputPhaseDuration, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when test cancels - just return
            }
    }

    protected override async Task UpdatePhaseAsync(CancellationToken cancellationToken)
    {
        if (_updatePhaseDuration > TimeSpan.Zero)
            try
            {
                await Task.Delay(_updatePhaseDuration, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when test cancels - still increment tick
            }

        // Call base to increment CurrentTick
        await base.UpdatePhaseAsync(cancellationToken);
    }

    protected override async Task OutputPhaseAsync(CancellationToken cancellationToken)
    {
        if (_outputPhaseDuration > TimeSpan.Zero)
            try
            {
                await Task.Delay(_outputPhaseDuration, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when test cancels - just return
            }
        // Don't call base - we don't want to broadcast in tests
    }
}
