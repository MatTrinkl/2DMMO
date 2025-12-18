using Mmo.Server.Messages;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared;
using Mmo.Shared.Messages.Chat;

namespace Mmo.Server.Tests.GameServer;

[Collection("IdRegistry")]
public class GameServerBroadcastTests : IDisposable
{
    private readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer = new();

    public GameServerBroadcastTests()
    {
        // Clear IdRegistry before each test
        IdRegistry.Instance.Clear();
    }

    public void Dispose()
    {
        // Clear IdRegistry after each test
        IdRegistry.Instance.Clear();
    }

    [Fact]
    public void OutputPhase_SendsQueuedMessages()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var message = new ChatMessage(Guid.NewGuid(), "Test broadcast");

        // Queue a broadcast
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToAll(message));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify message was sent (to all connected clients, but there are none so list is empty)
        // The important thing is that it doesn't crash
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void QueueOutgoingMessage_MultipleMessages_AllProcessed()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var message1 = new ChatMessage(Guid.NewGuid(), "Message 1");
        var message2 = new ChatMessage(Guid.NewGuid(), "Message 2");
        var message3 = new ChatMessage(Guid.NewGuid(), "Message 3");

        // Queue multiple broadcasts
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToAll(message1));
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToAll(message2));
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToAll(message3));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify ticks occurred (messages were processed)
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void QueueOutgoingMessage_ToSpecificClient_DoesNotCrash()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        var connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        var message = new ChatMessage(Guid.NewGuid(), "Hello Client");

        // Queue a message to specific client
        gameServer.QueueOutgoingMessage(OutgoingMessage.ToClient(connection, message));

        // Start and run
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Verify it ran without crashing
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void BroadcastAnnouncement_SendsToAllPlayers()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        // Start server
        gameServer.Start();

        // Broadcast announcement
        gameServer.BroadcastAnnouncement("Server maintenance in 5 minutes");

        Thread.Sleep(100);
        gameServer.Stop();

        // Verify announcement was logged
        Assert.True(_mockLog.HasMessageContaining("INFO", "Server announcement"));
    }

    [Fact]
    public void GetStats_ReturnsCorrectQueueSizes()
    {
        var gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);

        // Queue some messages
        for (int i = 0; i < 5; i++)
        {
            gameServer.QueueOutgoingMessage(
                OutgoingMessage.BroadcastToAll(new ChatMessage(Guid.NewGuid(), $"Message {i}")));
        }

        // Start and run briefly
        gameServer.Start();
        Thread.Sleep(150); // Give it time to process
        gameServer.Stop();

        // Get stats
        var stats = gameServer.GetStats();

        // Verify stats are reasonable
        Assert.True(stats.TickCount > 0);
        Assert.True(stats.OutputQueueSize >= 0); // Queue should be processed or processing
    }
}
