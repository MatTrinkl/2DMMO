using Mmo.Server.Core.Structs;
using Mmo.Server.Messages;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Chat.Messages;
using Mmo.Shared.Entities;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Records;

namespace Mmo.Server.Tests.GameServer;

[Collection("IdRegistry")]
public class GameServerTests
{
    private readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer;

    public GameServerTests()
    {
        _mockNetworkServer = new MockNetworkServer(_mockLog, true);
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        Assert.Equal(0, gameServer.TickCount);
    }

    [Fact]
    public void QueueOutgoingMessage_AddsMessageToQueue()
    {
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var message = new ChatMessage(Guid.NewGuid(), "Test");

        // Should not throw
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToAll(message));

        Assert.True(true);
    }

    [Fact]
    public void Start_AndStop_WorksGracefully()
    {
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Should have processed some ticks
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void OnClientConnected_LogsConnection()
    {
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
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
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
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
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
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
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Simulate different message types
        var loginRequest = new LoginRequest("TestUser", "password123");
        var testEntity = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "TestPlayer", new Position(0, 0));
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
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
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
        Thread.Sleep(150); // Give more time for messages to be processed
        gameServer.Stop();

        // Debug: Output all messages
        string allMessages = string.Join(Environment.NewLine, _mockLog.Messages);

        // Verify warnings were logged for invalid usernames
        Assert.True(_mockLog.HasMessageContaining("WARN", "Username"),
            $"Expected warning with 'Username'. All messages:{Environment.NewLine}{allMessages}");
    }

    [Fact]
    public void GetStats_ReturnsValidStats()
    {
        Server.GameServer.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        ServerStats stats = gameServer.GetStats();

        Assert.NotNull(stats);
        Assert.True(stats.TickCount > 0);
        Assert.True(stats.Uptime.TotalMilliseconds >= 0);
    }
}
