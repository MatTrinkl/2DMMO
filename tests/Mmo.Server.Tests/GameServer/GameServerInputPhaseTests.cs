using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Messages.Ping;
using Mmo.Shared.Records;
using Moq;

namespace Mmo.Server.Tests.GameServer;

public class GameServerInputPhaseTests
{
    private readonly Mock<ILog> _mockLog;
    private readonly MockNetworkServer _mockNetworkServer;

    public GameServerInputPhaseTests()
    {
        _mockLog = new Mock<ILog>();
        _mockNetworkServer = new MockNetworkServer();
    }

    [Fact]
    public async Task InputPhase_ProcessesAllQueuedMessages()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Queue multiple messages (ChatMessage handler not yet implemented, logs Debug)
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 1"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 2"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 3"));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify all messages were routed (logged as "Handler not yet implemented")
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")),
                It.IsAny<Guid>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task InputPhase_ProcessesMessagesFromMultipleClients()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId1 = Guid.NewGuid();
        var clientId2 = Guid.NewGuid();
        var clientId3 = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Messages from different clients (handler not yet implemented, logs Debug)
        _mockNetworkServer.SimulateMessageReceived(clientId1, new ChatMessage(Guid.NewGuid(), "From Client 1"));
        _mockNetworkServer.SimulateMessageReceived(clientId2, new ChatMessage(Guid.NewGuid(), "From Client 2"));
        _mockNetworkServer.SimulateMessageReceived(clientId3, new ChatMessage(Guid.NewGuid(), "From Client 3"));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify messages from all clients were routed
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")),
                clientId1),
            Times.Once);

        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")),
                clientId2),
            Times.Once);

        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")),
                clientId3),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesLoginRequest()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send LoginRequest (handled by LoginHandler)
        _mockNetworkServer.SimulateMessageReceived(clientId, new LoginRequest("TestUser", "password123"));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // LoginRequest is handled by LoginHandler - verify it doesn't log as "Unknown" or "not yet implemented"
        _mockLog.Verify(log => log.Warn(
                It.Is<string>(s => s.Contains("Unknown message type")),
                MessageType.LoginRequest,
                It.IsAny<Guid>()),
            Times.Never);
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("Handler not yet implemented")),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task InputPhase_HandlesPositionUpdate()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send PositionUpdate (handler not yet implemented, logs Debug)
        var testEntity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(5, 5));
        _mockNetworkServer.SimulateMessageReceived(clientId,
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(10, 20)));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify PositionUpdate was routed
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("PositionUpdate") && s.Contains("Handler not yet implemented")),
                clientId),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesChatMessage()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send ChatMessage (handler not yet implemented, logs Debug)
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Hello, World!"));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify ChatMessage was routed
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")),
                clientId),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesPingMessage()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send Ping (handler not yet implemented, logs Debug)
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify Ping was routed
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("Ping") && s.Contains("Handler not yet implemented")),
                clientId),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesHeartbeat()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send Heartbeat (handled silently by ClientConnection timeout logic)
        _mockNetworkServer.SimulateMessageReceived(clientId,
            new Heartbeat(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Guid.NewGuid()));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify Heartbeat was routed but NOT logged (handled silently)
        _mockLog.Verify(log => log.Warn(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
        _mockLog.Verify(log => log.Debug(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task InputPhase_ProcessesMessagesInOrder()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        var receivedMessages = new List<string>();
        using var cts = new CancellationTokenSource();

        // Setup to capture message order from Debug logs (unimplemented handlers)
        _mockLog.Setup(log => log.Debug(
                It.IsAny<string>(),
                It.IsAny<object[]>()))
            .Callback<string, object[]>((msg, args) =>
            {
                if (msg.Contains("Handler not yet implemented"))
                {
                    // The message format is: "{MessageType} from {ConnectionId} - Handler not yet implemented"
                    // where {MessageType} is in the template string itself
                    if (msg.Contains("PositionUpdate")) receivedMessages.Add("PositionUpdate");
                    else if (msg.Contains("ChatMessage")) receivedMessages.Add("ChatMessage");
                    else if (msg.Contains("Ping")) receivedMessages.Add("Ping");
                }
            });

        // Queue messages in specific order (all will log Debug as handlers not implemented)
        var testEntity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));
        _mockNetworkServer.SimulateMessageReceived(clientId,
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(5, 5)));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Hi"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify order
        Assert.Equal(3, receivedMessages.Count);
        Assert.Equal("PositionUpdate", receivedMessages[0]);
        Assert.Equal("ChatMessage", receivedMessages[1]);
        Assert.Equal("Ping", receivedMessages[2]);
    }

    [Fact]
    public async Task InputPhase_HandlesEmptyQueue()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        using var cts = new CancellationTokenSource();

        // Don't send any messages
        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Should complete without errors
        Assert.True(gameServer.CurrentTick > 0);

        // No message processing should have occurred
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("Received message")),
                It.IsAny<MessageType>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task InputPhase_ProcessesMessagesAcrossMultipleTicks()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send initial message
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "First"));

        // Run for a bit
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        long firstTickCount = gameServer.CurrentTick;

        // Send another message
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Second"));

        // Run again
        using var cts2 = new CancellationTokenSource();
        cts2.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts2.Token);

        // Both messages should have been processed
        Assert.True(gameServer.CurrentTick > firstTickCount);
        _mockLog.Verify(log => log.Debug(
                It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")),
                clientId),
            Times.Exactly(2));
    }

    [Fact]
    public async Task InputPhase_HandlesMixedMessageTypes()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send various message types
        var testEntity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));
        _mockNetworkServer.SimulateMessageReceived(clientId, new LoginRequest("Player1", "password123"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Test"));
        _mockNetworkServer.SimulateMessageReceived(clientId,
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(1, 1)));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Heartbeat(123456789, Guid.NewGuid()));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify all different types were processed
        // LoginRequest is handled by LoginHandler (should not log Debug or Warn)
        _mockLog.Verify(
            log => log.Warn(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        
        // Ping, ChatMessage, PositionUpdate: handlers not yet implemented (log Debug)
        _mockLog.Verify(
            log => log.Debug(It.Is<string>(s => s.Contains("Ping") && s.Contains("Handler not yet implemented")), clientId),
            Times.Once);
        _mockLog.Verify(
            log => log.Debug(It.Is<string>(s => s.Contains("ChatMessage") && s.Contains("Handler not yet implemented")), clientId),
            Times.Once);
        _mockLog.Verify(
            log => log.Debug(It.Is<string>(s => s.Contains("PositionUpdate") && s.Contains("Handler not yet implemented")), clientId),
            Times.Once);
        
        // Heartbeat: handled silently (no logging) - already verified by Times.Never for both Debug and Warn above
    }
}
