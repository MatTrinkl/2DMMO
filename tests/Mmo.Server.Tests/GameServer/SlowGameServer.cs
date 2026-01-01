using Mmo.Server.MessageRouting;
using Mmo.Server.Tests.Helpers;
using Mmo.Server.Zones;
using Mmo.Shared.Core.Interfaces;

namespace Mmo.Server.Tests.GameServer;

/// <summary>
///     A test implementation of GameServer that simulates slow tick phases
///     for testing tick overrun handling.
///     Note: The new GameServer runs in a background thread and doesn't expose
///     protected methods to override. This class now simulates slowness by
///     adding delay in the Tick method via interception.
/// </summary>
internal sealed class SlowGameServer : Core.GameServer
{
    private readonly TimeSpan _tickDelay;

    public SlowGameServer(
        ILog log,
        MockNetworkServer networkServer,
        TimeSpan tickDelay,
        ZoneManager? zoneManager = null,
        MessageRouter? messageRouter = null,
        IServiceProvider? services = null)
        : base(
            networkServer,
            messageRouter ??
            TestHelpers.CreateMessageRouter(log, zoneManager ?? TestHelpers.CreateDefaultZoneManager()),
            zoneManager ?? TestHelpers.CreateDefaultZoneManager(),
            services ?? TestHelpers.CreateTestServices(log, zoneManager ?? TestHelpers.CreateDefaultZoneManager()),
            log)
    {
        _tickDelay = tickDelay;
    }

    /// <summary>
    ///     Overrides Tick to add artificial delay for testing tick overruns.
    /// </summary>
    protected override void Tick(float deltaTime)
    {
        if (_tickDelay > TimeSpan.Zero) Thread.Sleep(_tickDelay);

        base.Tick(deltaTime);
    }
}
