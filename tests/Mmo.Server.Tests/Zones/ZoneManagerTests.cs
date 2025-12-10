using Mmo.Server.Entities;
using Mmo.Server.Zones;
using Mmo.Shared. Entities;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;

namespace Mmo.Server.Tests.Zones;

public class ZoneManagerTests
{
    private ZoneManager CreateZoneManager()
    {
        var defaultZone = new Zone(0, "default", new ZoneBounds(0, 0, 1000, 1000));
        return new ZoneManager(0, defaultZone);
    }

    private ServerPlayer CreateServerPlayer(Guid?  persistentId = null, Guid? connectionId = null)
    {
        var entity = new PlayerEntity(
            persistentId ?? Guid.NewGuid(),
            "TestPlayer",
            new Position(100, 100)
        );
        return new ServerPlayer(entity, connectionId ??  Guid.NewGuid());
    }

    // ══════════════════════════════════════════════════════════
    // ZONE TESTS
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_CreatesWithDefaultZone()
    {
        var zoneManager = CreateZoneManager();

        Assert.Equal(1, zoneManager.ZoneCount);
        Assert.NotNull(zoneManager.GetDefaultZone());
        Assert.Equal(0, zoneManager.DefaultZoneId);
    }

    [Fact]
    public void RegisterZone_AddsNewZone()
    {
        var zoneManager = CreateZoneManager();
        var newZone = new Zone(1, "second", new ZoneBounds(0, 0, 500, 500));

        zoneManager.RegisterZone(1, newZone);

        Assert.Equal(2, zoneManager.ZoneCount);
        Assert.NotNull(zoneManager. GetZone(1));
    }

    [Fact]
    public void RegisterZone_DuplicateId_ThrowsArgumentException()
    {
        var zoneManager = CreateZoneManager();
        var duplicateZone = new Zone(0, "duplicate", new ZoneBounds(0, 0, 100, 100));

        Assert.Throws<ArgumentException>(() => zoneManager.RegisterZone(0, duplicateZone));
    }

    [Fact]
    public void UnregisterZone_RemovesZone()
    {
        var zoneManager = CreateZoneManager();
        var newZone = new Zone(1, "toRemove", new ZoneBounds(0, 0, 100, 100));
        zoneManager.RegisterZone(1, newZone);

        bool result = zoneManager.UnregisterZone(1);

        Assert.True(result);
        Assert.Equal(1, zoneManager.ZoneCount);
        Assert.Null(zoneManager.GetZone(1));
    }

    [Fact]
    public void GetZone_NonExistent_ReturnsNull()
    {
        var zoneManager = CreateZoneManager();

        Assert.Null(zoneManager. GetZone(999));
    }

    // ══════════════════════════════════════════════════════════
    // PLAYER MANAGEMENT TESTS
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void AddPlayer_AddsToDefaultZone()
    {
        var zoneManager = CreateZoneManager();
        var player = CreateServerPlayer();

        zoneManager.AddPlayer(player);

        Assert.Equal(1, zoneManager.PlayerCount);
        Assert.True(zoneManager.HasPlayerWithConnectionId(player.ConnectionId));
    }

    [Fact]
    public void AddPlayer_AddsToSpecificZone()
    {
        var zoneManager = CreateZoneManager();
        var newZone = new Zone(1, "other", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, newZone);
        var player = CreateServerPlayer();

        zoneManager.AddPlayer(player, zoneId: 1);

        Assert.Equal(1, zoneManager.PlayerCount);
        var playersInZone = zoneManager. GetServerPlayersInZone(1).ToList();
        Assert.Single(playersInZone);
        Assert.Equal(player.ConnectionId, playersInZone[0].ConnectionId);
    }

    [Fact]
    public void AddPlayer_NonExistentZone_ThrowsInvalidOperationException()
    {
        var zoneManager = CreateZoneManager();
        var player = CreateServerPlayer();

        Assert.Throws<InvalidOperationException>(() => zoneManager.AddPlayer(player, zoneId: 999));
    }

    [Fact]
    public void AddPlayer_NullPlayer_ThrowsArgumentNullException()
    {
        var zoneManager = CreateZoneManager();

        Assert.Throws<ArgumentNullException>(() => zoneManager.AddPlayer(null! ));
    }

    [Fact]
    public void TryGetPlayerByConnectionId_ExistingPlayer_ReturnsTrue()
    {
        var zoneManager = CreateZoneManager();
        var player = CreateServerPlayer();
        zoneManager.AddPlayer(player);

        bool found = zoneManager.TryGetPlayerByConnectionId(player.ConnectionId, out var foundPlayer);

        Assert.True(found);
        Assert.NotNull(foundPlayer);
        Assert.Equal(player.ConnectionId, foundPlayer.ConnectionId);
    }

    [Fact]
    public void TryGetPlayerByConnectionId_NonExistent_ReturnsFalse()
    {
        var zoneManager = CreateZoneManager();

        bool found = zoneManager. TryGetPlayerByConnectionId(Guid.NewGuid(), out var foundPlayer);

        Assert.False(found);
        Assert.Null(foundPlayer);
    }

    [Fact]
    public void TryGetPlayerByPersistentId_ExistingPlayer_ReturnsTrue()
    {
        var zoneManager = CreateZoneManager();
        var persistentId = Guid.NewGuid();
        var player = CreateServerPlayer(persistentId:  persistentId);
        zoneManager.AddPlayer(player);

        bool found = zoneManager. TryGetPlayerByPersistentId(persistentId, out var foundPlayer);

        Assert.True(found);
        Assert.NotNull(foundPlayer);
        Assert.Equal(persistentId, foundPlayer.Entity.PersistentId);
    }

    [Fact]
    public void RemovePlayerByConnectionId_ExistingPlayer_RemovesAndReturnsPlayer()
    {
        var zoneManager = CreateZoneManager();
        var player = CreateServerPlayer();
        zoneManager.AddPlayer(player);

        var removedPlayer = zoneManager.RemovePlayerByConnectionId(player. ConnectionId);

        Assert.NotNull(removedPlayer);
        Assert.Equal(player.ConnectionId, removedPlayer.ConnectionId);
        Assert.Equal(0, zoneManager.PlayerCount);
        Assert.False(zoneManager.HasPlayerWithConnectionId(player.ConnectionId));
    }

    [Fact]
    public void RemovePlayerByConnectionId_NonExistent_ReturnsNull()
    {
        var zoneManager = CreateZoneManager();

        var removedPlayer = zoneManager. RemovePlayerByConnectionId(Guid.NewGuid());

        Assert.Null(removedPlayer);
    }

    [Fact]
    public void RemovePlayerByConnectionId_AlsoRemovesFromPersistentIdLookup()
    {
        var zoneManager = CreateZoneManager();
        var persistentId = Guid.NewGuid();
        var player = CreateServerPlayer(persistentId: persistentId);
        zoneManager.AddPlayer(player);

        zoneManager.RemovePlayerByConnectionId(player.ConnectionId);

        Assert.False(zoneManager.HasPlayerWithPersistentId(persistentId));
    }

    [Fact]
    public void GetServerPlayersInZone_ReturnsOnlyPlayersInThatZone()
    {
        var zoneManager = CreateZoneManager();
        var zone1 = new Zone(1, "zone1", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, zone1);

        var player1 = CreateServerPlayer();
        var player2 = CreateServerPlayer();
        var player3 = CreateServerPlayer();

        zoneManager.AddPlayer(player1, zoneId: 0);
        zoneManager.AddPlayer(player2, zoneId: 0);
        zoneManager.AddPlayer(player3, zoneId: 1);

        var playersInZone0 = zoneManager.GetServerPlayersInZone(0).ToList();
        var playersInZone1 = zoneManager.GetServerPlayersInZone(1).ToList();

        Assert.Equal(2, playersInZone0.Count);
        Assert.Single(playersInZone1);
    }

    // ══════════════════════════════════════════════════════════
    // ENTITY TESTS
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void TryGetEntityByPersistentId_PlayerEntity_ReturnsTrue()
    {
        var zoneManager = CreateZoneManager();
        var persistentId = Guid.NewGuid();
        var player = CreateServerPlayer(persistentId: persistentId);
        zoneManager.AddPlayer(player);

        bool found = zoneManager.TryGetEntityByPersistentId(persistentId, out var entity);

        Assert.True(found);
        Assert.NotNull(entity);
        Assert.Equal(persistentId, entity.PersistentId);
    }

    [Fact]
    public void GetAllEntities_ReturnsAllEntitiesInZone()
    {
        var zoneManager = CreateZoneManager();
        var player1 = CreateServerPlayer();
        var player2 = CreateServerPlayer();
        zoneManager.AddPlayer(player1);
        zoneManager.AddPlayer(player2);

        var entities = zoneManager.GetAllEntities(0);

        Assert.Equal(2, entities.Count);
    }

    // ══════════════════════════════════════════════════════════
    // TRANSFER TESTS
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void TransferPlayerByConnectionId_MovesPlayerToNewZone()
    {
        var zoneManager = CreateZoneManager();
        var zone1 = new Zone(1, "zone1", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, zone1);
        var player = CreateServerPlayer();
        zoneManager.AddPlayer(player, zoneId: 0);

        bool result = zoneManager.TransferPlayerByConnectionId(player.ConnectionId, toZoneId: 1);

        Assert.True(result);
        var playersInZone0 = zoneManager.GetServerPlayersInZone(0).ToList();
        var playersInZone1 = zoneManager.GetServerPlayersInZone(1).ToList();
        Assert.Empty(playersInZone0);
        Assert.Single(playersInZone1);
    }

    [Fact]
    public void TransferPlayerByConnectionId_SameZone_ReturnsTrue()
    {
        var zoneManager = CreateZoneManager();
        var player = CreateServerPlayer();
        zoneManager.AddPlayer(player, zoneId: 0);

        bool result = zoneManager. TransferPlayerByConnectionId(player.ConnectionId, toZoneId: 0);

        Assert.True(result);
    }

    [Fact]
    public void TransferPlayerByConnectionId_NonExistentPlayer_ReturnsFalse()
    {
        var zoneManager = CreateZoneManager();

        bool result = zoneManager. TransferPlayerByConnectionId(Guid.NewGuid(), toZoneId: 0);

        Assert.False(result);
    }

    [Fact]
    public void TransferPlayerByPersistentId_MovesPlayerToNewZone()
    {
        var zoneManager = CreateZoneManager();
        var zone1 = new Zone(1, "zone1", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, zone1);
        var persistentId = Guid.NewGuid();
        var player = CreateServerPlayer(persistentId: persistentId);
        zoneManager.AddPlayer(player, zoneId: 0);

        bool result = zoneManager.TransferPlayerByPersistentId(persistentId, toZoneId: 1);

        Assert.True(result);
        // Player should still be findable by PersistentId after transfer
        Assert.True(zoneManager.TryGetPlayerByPersistentId(persistentId, out _));
    }
}
