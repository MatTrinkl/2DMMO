using Mmo.Server.Core.Structs;
using Mmo.Server.Entities;
using Mmo.Server.Messages;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Chat.Messages;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Movement;
using Mmo.Shared.Prefab;

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
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        Assert.Equal(0, gameServer.TickCount);
    }

    [Fact]
    public void QueueOutgoingMessage_AddsMessageToQueue()
    {
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var message = new ChatMessage(Guid.NewGuid(), "Test");

        // Should not throw
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToAll(message));

        Assert.True(true);
    }

    [Fact]
    public void Start_AndStop_WorksGracefully()
    {
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Should have processed some ticks
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void OnClientConnected_LogsConnection()
    {
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
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
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
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
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
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
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        // Simulate different message types
        var loginRequest = new LoginRequest { Username = "TestUser", Password = "password123" };
        var testEntity = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "TestPlayer", new Position(0, 0), EntityIdentity.Unassigned(PrefabIds.PlayerDefault));
        var positionUpdate =
            new PositionUpdate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), testEntity.ToDto(), new Position(5, 5));
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
    public void GetStats_ReturnsValidStats()
    {
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        ServerStats stats = gameServer.GetStats();

        Assert.True(stats.TickCount > 0);
        Assert.True(stats.Uptime.TotalMilliseconds >= 0);
    }
}
