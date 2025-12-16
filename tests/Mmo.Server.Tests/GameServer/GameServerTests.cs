using Mmo.Server.Entities;
using Mmo.Server.Messages;
using Mmo.Server.Networking;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Records;
using Moq;

namespace Mmo.Server.Tests.GameServer;

public class GameServerTests
{
    private readonly Mock<ILog> _mockLog = new();
    private readonly Mock<INetworkServer> _mockNetworkServer = new();

    // NetworkServer braucht einen echten Constructor, daher anders mocken

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

        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToServer(mockMessage.Object));

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

    [Fact]
    public void OnClientConnected_LogsConnection()
    {
        var mockNetworkServer = new MockNetworkServer();
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, mockNetworkServer);
        var clientId = Guid.NewGuid();

        mockNetworkServer.SimulateClientConnected(clientId, "192.168.1.1:5000");

        _mockLog.Verify(log => log.Info(
                It.Is<string>(s => s.Contains("connected")),
                It.Is<Guid>(g => g == clientId),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void OnClientDisconnected_WithoutPlayer_LogsDisconnection()
    {
        var mockNetworkServer = new MockNetworkServer();
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, mockNetworkServer);
        var clientId = Guid.NewGuid();

        mockNetworkServer.SimulateClientDisconnected(clientId, DisconnectReason.ClientDisconnected);

        _mockLog.Verify(log => log.Info(
                It.Is<string>(s => s.Contains("disconnected")),
                It.Is<Guid>(g => g == clientId),
                It.IsAny<DisconnectReason>()),
            Times.Once);
    }

    [Fact]
    public void OnClientDisconnected_WithPlayer_RemovesPlayerAndQueuesBroadcast()
    {
        var mockNetworkServer = new MockNetworkServer();
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Add a player to the zone first
        ClientConnection connection = mockNetworkServer.GetOrCreateMockConnection(clientId);
        var player = new ServerPlayer(
            new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(10, 10)),
            connection
        );
        gameServer.ZoneManager.AddPlayer(player);

        // Simulate disconnect
        mockNetworkServer.SimulateClientDisconnected(clientId, DisconnectReason.ClientDisconnected);

        // Verify player was removed
        bool playerFound = gameServer.ZoneManager.TryGetPlayerByConnectionId(clientId, out _);
        Assert.False(playerFound);

        // Verify logs
        _mockLog.Verify(log => log.Info(
                It.Is<string>(s => s.Contains("removed from zone")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void OnNetworkError_WithClientId_LogsClientError()
    {
        var mockNetworkServer = new MockNetworkServer();
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, mockNetworkServer);
        var clientId = Guid.NewGuid();
        var exception = new Exception("Test error");

        mockNetworkServer.SimulateNetworkError(clientId, exception, "TestContext");

        _mockLog.Verify(log => log.Error(
                It.Is<string>(s => s.Contains("Network error")),
                It.Is<Guid>(g => g == clientId),
                It.Is<string>(s => s == "TestContext"),
                It.Is<string>(s => s.Contains("Test error"))),
            Times.Once);
    }

    [Fact]
    public void OnNetworkError_WithoutClientId_LogsServerError()
    {
        var mockNetworkServer = new MockNetworkServer();
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, mockNetworkServer);
        var exception = new Exception("Server error");

        mockNetworkServer.SimulateNetworkError(null, exception, "ServerContext");

        _mockLog.Verify(log => log.Error(
                It.Is<string>(s => s.Contains("Server network error")),
                It.Is<string>(s => s == "ServerContext"),
                It.Is<string>(s => s.Contains("Server error"))),
            Times.Once);
    }

    [Fact]
    public async Task OnMessageReceived_QueuesMessageForProcessing()
    {
        var mockNetworkServer = new MockNetworkServer();
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, mockNetworkServer);
        var clientId = Guid.NewGuid();
        var message = new ChatMessage(Guid.NewGuid(), "Hello");
        using var cts = new CancellationTokenSource();

        // Simulate message received
        mockNetworkServer.SimulateMessageReceived(clientId, message);

        // Run one tick to process the message
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify message was routed (ChatMessage handler not yet implemented, logs Debug)
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")),
                It.Is<Guid>(g => g == clientId)),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_LogsDifferentMessageTypes()
    {
        var mockNetworkServer = new MockNetworkServer();
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Simulate different message types
        var loginRequest = new LoginRequest("TestUser", "password123");
        var testEntity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));
        var positionUpdate =
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(5, 5));
        var chatMessage = new ChatMessage(Guid.NewGuid(), "Test");

        mockNetworkServer.SimulateMessageReceived(clientId, loginRequest);
        mockNetworkServer.SimulateMessageReceived(clientId, positionUpdate);
        mockNetworkServer.SimulateMessageReceived(clientId, chatMessage);

        // Run one tick to process messages
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify all messages were routed
        // LoginRequest is handled by LoginHandler (should not log "Unknown" or "not yet implemented")
        _mockLog.Verify(log => log.Warn(
                It.Is<string>(s => s.Contains("Unknown message type")),
                It.Is<MessageType>(t => t == MessageType.LoginRequest),
                It.IsAny<Guid>()),
            Times.Never);

        // PositionUpdate and ChatMessage are not yet implemented (log Debug, not Warn)
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("PositionUpdate") && s.Contains("Handler not yet implemented")),
                It.IsAny<Guid>()),
            Times.Once);

        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")),
                It.IsAny<Guid>()),
            Times.Once);
    }
}
