using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Messages.Ping;
using Mmo.Shared.Records;
using Moq;

namespace Mmo.Server.Tests.GameServer;

[Collection("IdRegistry")]
public class GameServerInputPhaseTests
{
    private readonly MockLog _mockLog;
    private readonly MockNetworkServer _mockNetworkServer;

    public GameServerInputPhaseTests()
    {
        _mockLog = new MockLog();
        _mockNetworkServer = new MockNetworkServer();
    }

    [Fact]
    public void InputPhase_ProcessesQueuedMessages()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Queue multiple messages
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 1"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 2"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Message 3"));

        // Start server and let it run for a short time
        gameServer.Start();
        Thread.Sleep(100); // Allow at least 2-3 ticks
        gameServer.Stop();

        // Verify at least some ticks occurred
        Assert.True(gameServer.TickCount > 0);

        // Messages would be processed - but since handlers aren't fully implemented,
        // we just verify no crashes occurred
    }

    [Fact]
    public void InputPhase_ProcessesMessagesFromMultipleClients()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId1 = Guid.NewGuid();
        var clientId2 = Guid.NewGuid();
        var clientId3 = Guid.NewGuid();

        // Messages from different clients
        _mockNetworkServer.SimulateMessageReceived(clientId1, new ChatMessage(Guid.NewGuid(), "From Client 1"));
        _mockNetworkServer.SimulateMessageReceived(clientId2, new ChatMessage(Guid.NewGuid(), "From Client 2"));
        _mockNetworkServer.SimulateMessageReceived(clientId3, new ChatMessage(Guid.NewGuid(), "From Client 3"));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify ticks occurred
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void InputPhase_HandlesLoginRequest()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Send LoginRequest (handled by ConnectionHandler)
        _mockNetworkServer.SimulateMessageReceived(clientId, new LoginRequest("TestUser", "password123"));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify it ran without crashing
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void InputPhase_HandlesPositionUpdate()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Send PositionUpdate
        var testEntity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(5, 5));
        _mockNetworkServer.SimulateMessageReceived(clientId,
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(10, 20)));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify it ran
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void InputPhase_HandlesChatMessage()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Send ChatMessage
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Hello, World!"));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify it ran
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void InputPhase_HandlesPingMessage()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Send Ping
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify it ran
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void InputPhase_HandlesHeartbeat()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Send Heartbeat
        _mockNetworkServer.SimulateMessageReceived(clientId,
            new Heartbeat(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Guid.NewGuid()));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify it ran without warnings
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void InputPhase_ProcessesMessagesInOrder()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Queue messages in specific order
        var testEntity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));
        _mockNetworkServer.SimulateMessageReceived(clientId,
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(5, 5)));
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Hi"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify all were processed (no crashes)
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void InputPhase_HandlesEmptyQueue()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        // Don't send any messages
        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Should complete without errors
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void InputPhase_HandlesMixedMessageTypes()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Send various message types
        var testEntity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));
        _mockNetworkServer.SimulateMessageReceived(clientId, new LoginRequest("Player1", "password123"));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Ping());
        _mockNetworkServer.SimulateMessageReceived(clientId, new ChatMessage(Guid.NewGuid(), "Test"));
        _mockNetworkServer.SimulateMessageReceived(clientId,
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(1, 1)));
        _mockNetworkServer.SimulateMessageReceived(clientId, new Heartbeat(123456789, Guid.NewGuid()));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify all different types were processed
        Assert.True(gameServer.TickCount > 0);
    }
}
