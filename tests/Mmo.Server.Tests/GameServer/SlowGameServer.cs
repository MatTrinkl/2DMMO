using Mmo.Server.Networking;
using Mmo. Shared.Interfaces;

namespace Mmo.Server.Tests.GameServer;

/// <summary>
///     A test implementation of GameServer that simulates slow tick phases
///     for testing tick overrun handling.
/// </summary>
internal sealed class SlowGameServer : Server. GameLoop.GameServer
{
    private readonly TimeSpan _inputPhaseDuration;
    private readonly TimeSpan _updatePhaseDuration;
    private readonly TimeSpan _outputPhaseDuration;

    public SlowGameServer(
        ILog log,
        NetworkServer networkServer,
        TimeSpan?  inputPhaseDuration = null,
        TimeSpan? updatePhaseDuration = null,
        TimeSpan? outputPhaseDuration = null)
        : base(log, networkServer)
    {
        _inputPhaseDuration = inputPhaseDuration ??  TimeSpan.Zero;
        _updatePhaseDuration = updatePhaseDuration ?? TimeSpan.Zero;
        _outputPhaseDuration = outputPhaseDuration ?? TimeSpan.Zero;
    }

    /// <summary>
    ///     Convenience constructor for simple slow tick simulation.
    /// </summary>
    public SlowGameServer(ILog log, NetworkServer networkServer, TimeSpan tickWorkDuration)
        : this(log, networkServer, inputPhaseDuration: tickWorkDuration)
    {
    }

    protected override async Task InputPhaseAsync(CancellationToken cancellationToken)
    {
        if (_inputPhaseDuration > TimeSpan.Zero)
        {
            await Task.Delay(_inputPhaseDuration, cancellationToken);
        }
        // Don't call base - we don't want to process real messages in tests
    }

    protected override async Task UpdatePhaseAsync(CancellationToken cancellationToken)
    {
        if (_updatePhaseDuration > TimeSpan.Zero)
        {
            await Task.Delay(_updatePhaseDuration, cancellationToken);
        }

        // Call base to increment CurrentTick
        await base.UpdatePhaseAsync(cancellationToken);
    }

    protected override async Task OutputPhaseAsync(CancellationToken cancellationToken)
    {
        if (_outputPhaseDuration > TimeSpan.Zero)
        {
            await Task.Delay(_outputPhaseDuration, cancellationToken);
        }
        // Don't call base - we don't want to broadcast in tests
    }
}
