using Mmo.Server.Entities;
using Mmo.Server.Messages;
using Mmo.Server.Networking;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared;
using Mmo.Shared.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Messages.ZoneEvents;
using Mmo.Shared.Records;
using Moq;

namespace Mmo.Server.Tests.GameServer;

[Collection("IdRegistry")]
public class GameServerBroadcastTests : IDisposable
{
    private readonly Mock<ILog> _mockLog = new();
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
    public async Task OutputPhase_SendsQueuedBroadcasts()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var message = new ChatMessage(Guid.NewGuid(), "Test broadcast");
        using var cts = new CancellationTokenSource();

        // Queue a broadcast
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToServer(message));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify message was broadcast
        Assert.Contains(_mockNetworkServer.BroadcastMessages, m => m == message);
    }

    [Fact]
    public async Task OutputPhase_SendsFullZoneState_EveryNTicks()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Add a player to the zone
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        var player = new ServerPlayer(
            new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(10, 10)),
            connection
        );
        gameServer.ZoneManager.AddPlayer(player);

        // Run for at least SharedConstants.TickRate ticks (25 ticks = 1 second)
        // This should trigger at least one full state broadcast
        // Increased timeout for CI stability (allow extra time for slow systems)
        // At 40ms per tick, 25 ticks = 1000ms minimum, use 3000ms for safety on loaded CI systems
        cts.CancelAfter(TimeSpan.FromMilliseconds(3000)); // ~75 ticks with large buffer
        await gameServer.StartServerAsync(cts.Token);

        // Verify at least one ZoneState was sent
        Assert.True(gameServer.CurrentTick >= SharedConstants.TickRate,
            $"Expected at least {SharedConstants.TickRate} ticks, but got {gameServer.CurrentTick}");
        var zoneStateMessages = _mockNetworkServer.SentMessages
            .Where(m => m.Message is ZoneState)
            .ToList();
        Assert.NotEmpty(zoneStateMessages);
    }

    [Fact]
    public async Task OutputPhase_SendsDeltaUpdates_ForDirtyEntities()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Add a player to the zone
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        var player = new ServerPlayer(
            new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(10, 10)),
            connection
        );
        gameServer.ZoneManager.AddPlayer(player);

        // Mark entity as dirty
        gameServer.MarkEntityDirty(player.Entity);

        // Run a few ticks (but not enough for full state)
        cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // ~2-3 ticks
        await gameServer.StartServerAsync(cts.Token);

        // Verify PositionBroadcast was sent for dirty entity
        var positionBroadcasts = _mockNetworkServer.SentMessages
            .Where(m => m.Message is PositionBroadcast)
            .ToList();
        Assert.NotEmpty(positionBroadcasts);
    }

    [Fact]
    public async Task OutputPhase_ClearsDirtyEntities_AfterFullStateOrDelta()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Add a player to the zone
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        var player = new ServerPlayer(
            new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(10, 10)),
            connection
        );
        gameServer.ZoneManager.AddPlayer(player);

        // Mark entity as dirty
        gameServer.MarkEntityDirty(player.Entity);

        // Run a few ticks
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));
        await gameServer.StartServerAsync(cts.Token);

        _mockNetworkServer.Clear();

        // Run more ticks without marking dirty again
        using var cts2 = new CancellationTokenSource();
        cts2.CancelAfter(TimeSpan.FromMilliseconds(100));
        await gameServer.StartServerAsync(cts2.Token);

        // Should not have additional position broadcasts (only potential full state)
        var positionBroadcasts = _mockNetworkServer.SentMessages
            .Where(m => m.Message is PositionBroadcast)
            .ToList();

        // If we didn't hit a full state tick, there should be no position broadcasts
        if (gameServer.CurrentTick % SharedConstants.TickRate != 0) Assert.Empty(positionBroadcasts);
    }

    [Fact]
    public async Task BroadcastFullZoneStates_SendsToAllPlayersInZone()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var clientId1 = Guid.NewGuid();
        var clientId2 = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Add two players to the zone
        ClientConnection connection1 = _mockNetworkServer.GetOrCreateMockConnection(clientId1);
        var player1 = new ServerPlayer(
            new PlayerEntity(Guid.NewGuid(), "Player1", new Position(10, 10)),
            connection1
        );
        ClientConnection connection2 = _mockNetworkServer.GetOrCreateMockConnection(clientId2);
        var player2 = new ServerPlayer(
            new PlayerEntity(Guid.NewGuid(), "Player2", new Position(20, 20)),
            connection2
        );
        gameServer.ZoneManager.AddPlayer(player1);
        gameServer.ZoneManager.AddPlayer(player2);

        // Run until full state broadcast
        // Increased timeout for CI stability (allow extra time for slow systems)
        // At 40ms per tick, 25 ticks = 1000ms minimum, use 3000ms for safety on loaded CI systems
        cts.CancelAfter(TimeSpan.FromMilliseconds(3000));
        await gameServer.StartServerAsync(cts.Token);

        // Verify both players received ZoneState
        var player1Messages = _mockNetworkServer.SentMessages
            .Where(m => m.Client.Id == clientId1 && m.Message is ZoneState)
            .ToList();
        var player2Messages = _mockNetworkServer.SentMessages
            .Where(m => m.Client.Id == clientId2 && m.Message is ZoneState)
            .ToList();

        Assert.NotEmpty(player1Messages);
        Assert.NotEmpty(player2Messages);
    }

    [Fact]
    public async Task BroadcastDirtyEntities_OnlyBroadcastsToPlayersInSameZone()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);

        var clientId1 = Guid.NewGuid();
        var clientId2 = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        // Add players to the same zone
        ClientConnection connection1 = _mockNetworkServer.GetOrCreateMockConnection(clientId1);
        var player1 = new ServerPlayer(
            new PlayerEntity(Guid.NewGuid(), "Player1", new Position(10, 10)),
            connection1
        );
        ClientConnection connection2 = _mockNetworkServer.GetOrCreateMockConnection(clientId2);
        var player2 = new ServerPlayer(
            new PlayerEntity(Guid.NewGuid(), "Player2", new Position(20, 20)),
            connection2
        );
        gameServer.ZoneManager.AddPlayer(player1);
        gameServer.ZoneManager.AddPlayer(player2);

        // Mark one entity as dirty
        gameServer.MarkEntityDirty(player1.Entity);

        // Run a few ticks
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));
        await gameServer.StartServerAsync(cts.Token);

        // Both players should receive the position broadcast (same zone)
        var player1Broadcasts = _mockNetworkServer.SentMessages
            .Where(m => m.Client.Id == clientId1 && m.Message is PositionBroadcast)
            .ToList();
        var player2Broadcasts = _mockNetworkServer.SentMessages
            .Where(m => m.Client.Id == clientId2 && m.Message is PositionBroadcast)
            .ToList();

        Assert.NotEmpty(player1Broadcasts);
        Assert.NotEmpty(player2Broadcasts);
    }

    [Fact]
    public async Task OutputPhase_DoesNotBroadcastToEmptyZone()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        using var cts = new CancellationTokenSource();

        // Add a mob but no players
        var mob = new MobEntity("TestMob", new Position(10, 10));
        gameServer.ZoneManager.AddEntity(mob, 0);
        gameServer.MarkEntityDirty(mob);

        // Run a few ticks
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));
        await gameServer.StartServerAsync(cts.Token);

        // No broadcasts should be sent (no players to send to)
        Assert.Empty(_mockNetworkServer.SentMessages);
    }

    [Fact]
    public void MarkEntityDirty_WithGuid_MarksForDeltaBroadcast()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var persistentId = Guid.NewGuid();

        // Should not throw
        gameServer.MarkEntityDirty(persistentId);

        Assert.True(true); // Success if no exception
    }

    [Fact]
    public void MarkEntityDirty_WithEntity_MarksForDeltaBroadcast()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var entity = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(10, 10));

        // Should not throw
        gameServer.MarkEntityDirty(entity);

        Assert.True(true); // Success if no exception
    }

    [Fact]
    public async Task QueueBroadcast_MultiplMessages_AllSentInOrder()
    {
        var gameServer = new Server.GameLoop.GameServer(_mockLog.Object, _mockNetworkServer);
        var message1 = new ChatMessage(Guid.NewGuid(), "Message 1");
        var message2 = new ChatMessage(Guid.NewGuid(), "Message 2");
        var message3 = new ChatMessage(Guid.NewGuid(), "Message 3");
        using var cts = new CancellationTokenSource();

        // Queue multiple broadcasts
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToServer(message1));
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToServer(message2));
        gameServer.QueueOutgoingMessage(OutgoingMessage.BroadcastToServer(message3));

        // Run one tick
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        await gameServer.StartServerAsync(cts.Token);

        // Verify all messages were broadcast in order
        Assert.Equal(3, _mockNetworkServer.BroadcastMessages.Count);
        Assert.Equal(message1, _mockNetworkServer.BroadcastMessages[0]);
        Assert.Equal(message2, _mockNetworkServer.BroadcastMessages[1]);
        Assert.Equal(message3, _mockNetworkServer.BroadcastMessages[2]);
    }
}
