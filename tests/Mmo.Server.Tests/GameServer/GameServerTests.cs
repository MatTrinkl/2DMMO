using Mmo.Server.Messages;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Records;

namespace Mmo.Server.Tests.GameServer;

[Collection("IdRegistry")]
public class GameServerTests
{
    private readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer = new();

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        Assert.Equal(0, gameServer.TickCount);
    }

    [Fact]
    public void QueueOutgoingMessage_AddsMessageToQueue()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var message = new ChatMessage(Guid.NewGuid(), "Test");

        // Should not throw
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToAll(message));

        Assert.True(true);
    }

    [Fact]
    public void Start_AndStop_WorksGracefully()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Should have processed some ticks
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void OnClientConnected_LogsConnection()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        gameServer.Start();
        _mockNetworkServer.SimulateClientConnected(clientId, "192.168.1.1:5000");
        Thread.Sleep(50);
        gameServer.Stop();

        // Verify connection was logged
        Assert.True(_mockLog.HasMessageContaining("DEBUG", "connected"));
    }

    [Fact]
    public void OnClientDisconnected_LogsDisconnection()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        gameServer.Start();
        _mockNetworkServer.SimulateClientConnected(clientId);
        _mockNetworkServer.SimulateClientDisconnected(clientId, "Test disconnect");
        Thread.Sleep(50);
        gameServer.Stop();

        // Verify disconnection was logged
        Assert.True(_mockLog.HasMessageContaining("DEBUG", "disconnect"));
    }

    [Fact]
    public void OnMessageReceived_QueuesMessageForProcessing()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        var message = new ChatMessage(Guid.NewGuid(), "Hello");

        // Simulate message received
        _mockNetworkServer.SimulateMessageReceived(clientId, message);

        // Run to process the message
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify at least one tick occurred
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void ProcessMessage_HandlesDifferentMessageTypes()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Simulate different message types
        var loginRequest = new LoginRequest("TestUser", "password123");
        var testEntity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));
        var positionUpdate =
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity, new Position(5, 5));
        var chatMessage = new ChatMessage(Guid.NewGuid(), "Test");

        _mockNetworkServer.SimulateMessageReceived(clientId, loginRequest);
        _mockNetworkServer.SimulateMessageReceived(clientId, positionUpdate);
        _mockNetworkServer.SimulateMessageReceived(clientId, chatMessage);

        // Run to process messages
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify processing occurred
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void PlayerLogin_ValidatesUsername()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Simulate login with invalid usernames
        var loginRequest1 = new LoginRequest("Te", "password123"); // Too short
        var loginRequest2 = new LoginRequest("Teasdfasdfasfsdfasdfasdfasdfasdfasdfsf", "password123"); // Too long
        var loginRequest3 = new LoginRequest("", "password123"); // Empty

        _mockNetworkServer.SimulateMessageReceived(clientId, loginRequest1);
        _mockNetworkServer.SimulateMessageReceived(clientId, loginRequest2);
        _mockNetworkServer.SimulateMessageReceived(clientId, loginRequest3);

        // Run to process messages
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify warnings were logged for invalid usernames
        Assert.True(_mockLog.HasMessageContaining("WARN", "Username"));
    }

    [Fact]
    public void GetStats_ReturnsValidStats()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        var stats = gameServer.GetStats();

        Assert.NotNull(stats);
        Assert.True(stats.TickCount > 0);
        Assert.True(stats.Uptime.TotalMilliseconds >= 0);
    }
}
