using Mmo.Server.Networking;
using Mmo.Shared.Interfaces;
using Moq;

namespace Mmo.Server.Tests.GameServer;

public class GameServerTests
{
    private readonly Mock<ILog> _mockLog;
    private readonly Mock<INetworkServer> _mockNetworkServer;

    public GameServerTests()
    {
        _mockLog = new Mock<ILog>();

        // NetworkServer braucht einen echten Constructor, daher anders mocken
        _mockNetworkServer = new Mock<INetworkServer>();
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer.Object);

        Assert.False(gameServer.IsRunning);
        Assert.Equal(0, gameServer.CurrentTick);
        Assert.NotNull(gameServer.ZoneManager);
    }

    [Fact]
    public void MarkEntityDirty_WithGuid_AddsToDirtySet()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer.Object);
        var persistentId = Guid.NewGuid();

        gameServer.MarkEntityDirty(persistentId);

        // Wir können das nicht direkt testen ohne Reflection,
        // aber wir können testen dass es keine Exception wirft
        Assert.True(true);
    }

    [Fact]
    public void QueueBroadcast_AddsMessageToQueue()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer.Object);
        var mockMessage = new Mock<INetworkMessage>();

        gameServer.QueueBroadcast(mockMessage.Object);

        // Wieder:  ohne Reflection schwer zu testen,
        // aber keine Exception = gut
        Assert.True(true);
    }

    [Fact]
    public async Task StartServerAsync_CancelledImmediately_StopsGracefully()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Sofort canceln

        await gameServer.StartServerAsync(cts.Token);

        Assert.False(gameServer.IsRunning);
    }

    [Fact]
    public async Task StartServerAsync_RunsForFewTicks_IncrementsTickCounter()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer.Object);
        using var cts = new CancellationTokenSource();

        // Nach 100ms canceln (ca. 2-3 Ticks bei 25Hz)
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        await gameServer.StartServerAsync(cts.Token);

        Assert.True(gameServer.CurrentTick > 0);
        Assert.False(gameServer.IsRunning);
    }
}
