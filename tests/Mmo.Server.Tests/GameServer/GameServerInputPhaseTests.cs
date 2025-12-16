using Mmo.Server.Entities;
using Mmo.Server.Tests.Helpers;
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

        // Queue multiple messages
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 1"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 2"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 3"));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify all messages were logged (processed)
        _mockLog.Verify(log => log.Debug(
            It.Is<string>(s => s.Contains("Received message")),
            It.Is<MessageType>(t => t == MessageType.ChatMessage),
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

        // Messages from different clients
        _mockNetworkServer.SimulateMessageReceived(clientId1, new ChatMessage(Guid.NewGuid(), "From Client 1"));
        _mockNetworkServer.SimulateMessageReceived(clientId2, new ChatMessage(Guid.NewGuid(), "From Client 2"));
        _mockNetworkServer.SimulateMessageReceived(clientId3, new ChatMessage(Guid.NewGuid(), "From Client 3"));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify messages from all clients were processed
        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            It.IsAny<MessageType>(),
            clientId1),
            Times.Once);

        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            It.IsAny<MessageType>(),
            clientId2),
            Times.Once);

        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            It.IsAny<MessageType>(),
            clientId3),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesLoginRequest()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send LoginRequest
        _mockNetworkServer.SimulateMessageReceived(clientId, new LoginRequest("TestUser", "password123"));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify LoginRequest was logged
        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            MessageType.LoginRequest,
            clientId),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesPositionUpdate()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send PositionUpdate
        var testEntity = new Shared.Entities.PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(5, 5));
        _mockNetworkServer.SimulateMessageReceived(clientId, new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(10, 20)));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify PositionUpdate was logged
        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            MessageType.PositionUpdate,
            clientId),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesChatMessage()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send ChatMessage
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Hello, World!"));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify ChatMessage was logged
        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            MessageType.ChatMessage,
            clientId),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesPingMessage()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send Ping
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify Ping was logged
        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            MessageType.Ping,
            clientId),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_HandlesHeartbeat()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Send Heartbeat
        _mockNetworkServer.SimulateMessageReceived(clientId, new Heartbeat(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Guid.NewGuid()));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify Heartbeat was logged
        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            MessageType.Heartbeat,
            clientId),
            Times.Once);
    }

    [Fact]
    public async Task InputPhase_ProcessesMessagesInOrder()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        var receivedMessages = new List<MessageType>();
        using var cts = new CancellationTokenSource();

        // Setup to capture message order
        _mockLog.Setup(log => log.Debug(
            It.IsAny<string>(),
            It.IsAny<object[]>()))
            .Callback<string, object[]>((msg, args) => 
            {
                if (args.Length >= 1 && args[0] is MessageType msgType)
                {
                    receivedMessages.Add(msgType);
                }
            });

        // Queue messages in specific order
        var testEntity = new Shared.Entities.PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));
        _mockNetworkServer.SimulateMessageReceived(clientId, new LoginRequest("User", "password123"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(5, 5)));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Hi"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify order
        Assert.Equal(4, receivedMessages.Count);
        Assert.Equal(MessageType.LoginRequest, receivedMessages[0]);
        Assert.Equal(MessageType.PositionUpdate, receivedMessages[1]);
        Assert.Equal(MessageType.ChatMessage, receivedMessages[2]);
        Assert.Equal(MessageType.Ping, receivedMessages[3]);
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

        var firstTickCount = gameServer.CurrentTick;

        // Send another message
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Second"));

        // Run again
        using var cts2 = new CancellationTokenSource();
        cts2.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts2.Token);

        // Both messages should have been processed
        Assert.True(gameServer.CurrentTick > firstTickCount);
        _mockLog.Verify(log => log.Debug(
            It.IsAny<string>(),
            MessageType.ChatMessage,
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
        var testEntity = new Shared.Entities.PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));
        _mockNetworkServer.SimulateMessageReceived(clientId, new LoginRequest("Player1", "password123"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Test"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(1, 1)));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Heartbeat(123456789, Guid.NewGuid()));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify all different types were processed
        _mockLog.Verify(log => log.Debug(It.IsAny<string>(), MessageType.LoginRequest, clientId), Times.Once);
        _mockLog.Verify(log => log.Debug(It.IsAny<string>(), MessageType.Ping, clientId), Times.Once);
        _mockLog.Verify(log => log.Debug(It.IsAny<string>(), MessageType.ChatMessage, clientId), Times.Once);
        _mockLog.Verify(log => log.Debug(It.IsAny<string>(), MessageType.PositionUpdate, clientId), Times.Once);
        _mockLog.Verify(log => log.Debug(It.IsAny<string>(), MessageType.Heartbeat, clientId), Times.Once);
    }
}
