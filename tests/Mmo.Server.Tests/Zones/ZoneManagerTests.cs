using Mmo.Server.Entities;
using Mmo.Server.Networking;
using Mmo.Server.Tests.Helpers;
using Mmo.Server.Zones;
using Mmo.Shared.Entities;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;

namespace Mmo.Server.Tests.Zones;

public class ZoneManagerTests : IDisposable
{
    private static readonly MockNetworkServer _sharedMockNetworkServer = new();

    public ZoneManagerTests()
    {
        // Clear IdRegistry before each test
        IdRegistry.Instance.Clear();
    }

    public void Dispose()
    {
        // Clear IdRegistry after each test
        IdRegistry.Instance.Clear();
    }

    private ZoneManager CreateZoneManager()
    {
        var defaultZone = new Zone(0, "default", new ZoneBounds(0, 0, 1000, 1000));
        return new ZoneManager(0, defaultZone);
    }

    private ServerPlayer CreateServerPlayer(Guid? persistentId = null, Guid? connectionId = null)
    {
        var entity = new PlayerEntity(
            persistentId ?? Guid.NewGuid(),
            "TestPlayer",
            new Position(100, 100)
        );
        Guid connId = connectionId ?? Guid.NewGuid();
        ClientConnection connection = _sharedMockNetworkServer.GetOrCreateMockConnection(connId);
        return new ServerPlayer(entity, connection);
    }

    // ══════════════════════════════════════════════════════════
    // ZONE TESTS
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_CreatesWithDefaultZone()
    {
        ZoneManager zoneManager = CreateZoneManager();

        Assert.Equal(1, zoneManager.ZoneCount);
        Assert.NotNull(zoneManager.GetDefaultZone());
        Assert.Equal(0, zoneManager.DefaultZoneId);
    }

    [Fact]
    public void RegisterZone_AddsNewZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var newZone = new Zone(1, "second", new ZoneBounds(0, 0, 500, 500));

        zoneManager.RegisterZone(1, newZone);

        Assert.Equal(2, zoneManager.ZoneCount);
        Assert.NotNull(zoneManager.GetZone(1));
    }

    [Fact]
    public void RegisterZone_DuplicateId_ThrowsArgumentException()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var duplicateZone = new Zone(0, "duplicate", new ZoneBounds(0, 0, 100, 100));

        Assert.Throws<ArgumentException>(() => zoneManager.RegisterZone(0, duplicateZone));
    }

    [Fact]
    public void UnregisterZone_RemovesZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
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
        ZoneManager zoneManager = CreateZoneManager();

        Assert.Null(zoneManager.GetZone(999));
    }

    // ══════════════════════════════════════════════════════════
    // PLAYER MANAGEMENT TESTS
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void AddPlayer_AddsToDefaultZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayer player = CreateServerPlayer();

        zoneManager.AddPlayer(player);

        Assert.Equal(1, zoneManager.PlayerCount);
        Assert.True(zoneManager.HasPlayerWithConnectionId(player.Connection.Id));
    }

    [Fact]
    public void AddPlayer_AddsToSpecificZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var newZone = new Zone(1, "other", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, newZone);
        ServerPlayer player = CreateServerPlayer();

        zoneManager.AddPlayer(player, 1);

        Assert.Equal(1, zoneManager.PlayerCount);
        var playersInZone = zoneManager.GetServerPlayersInZone(1).ToList();
        Assert.Single(playersInZone);
        Assert.Equal(player.Connection.Id, playersInZone[0].Connection.Id);
    }

    [Fact]
    public void AddPlayer_NonExistentZone_ThrowsInvalidOperationException()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayer player = CreateServerPlayer();

        Assert.Throws<InvalidOperationException>(() => zoneManager.AddPlayer(player, 999));
    }

    [Fact]
    public void AddPlayer_NullPlayer_ThrowsArgumentNullException()
    {
        ZoneManager zoneManager = CreateZoneManager();

        Assert.Throws<ArgumentNullException>(() => zoneManager.AddPlayer(null!));
    }

    [Fact]
    public void TryGetPlayerByConnectionId_ExistingPlayer_ReturnsTrue()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayer player = CreateServerPlayer();
        zoneManager.AddPlayer(player);

        bool found = zoneManager.TryGetPlayerByConnectionId(player.Connection.Id, out ServerPlayer? foundPlayer);

        Assert.True(found);
        Assert.NotNull(foundPlayer);
        Assert.Equal(player.Connection.Id, foundPlayer.Connection.Id);
    }

    [Fact]
    public void TryGetPlayerByConnectionId_NonExistent_ReturnsFalse()
    {
        ZoneManager zoneManager = CreateZoneManager();

        bool found = zoneManager.TryGetPlayerByConnectionId(Guid.NewGuid(), out ServerPlayer? foundPlayer);

        Assert.False(found);
        Assert.Null(foundPlayer);
    }

    [Fact]
    public void TryGetPlayerByPersistentId_ExistingPlayer_ReturnsTrue()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var persistentId = Guid.NewGuid();
        ServerPlayer player = CreateServerPlayer(persistentId);
        zoneManager.AddPlayer(player);

        bool found = zoneManager.TryGetPlayerByPersistentId(persistentId, out ServerPlayer? foundPlayer);

        Assert.True(found);
        Assert.NotNull(foundPlayer);
        Assert.Equal(persistentId, foundPlayer.Entity.PersistentId);
    }

    [Fact]
    public void RemovePlayerByConnectionId_ExistingPlayer_RemovesAndReturnsPlayer()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayer player = CreateServerPlayer();
        zoneManager.AddPlayer(player);

        ServerPlayer? removedPlayer = zoneManager.RemovePlayerByConnectionId(player.Connection.Id);

        Assert.NotNull(removedPlayer);
        Assert.Equal(player.Connection.Id, removedPlayer.Connection.Id);
        Assert.Equal(0, zoneManager.PlayerCount);
        Assert.False(zoneManager.HasPlayerWithConnectionId(player.Connection.Id));
    }

    [Fact]
    public void RemovePlayerByConnectionId_NonExistent_ReturnsNull()
    {
        ZoneManager zoneManager = CreateZoneManager();

        ServerPlayer? removedPlayer = zoneManager.RemovePlayerByConnectionId(Guid.NewGuid());

        Assert.Null(removedPlayer);
    }

    [Fact]
    public void RemovePlayerByConnectionId_AlsoRemovesFromPersistentIdLookup()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var persistentId = Guid.NewGuid();
        ServerPlayer player = CreateServerPlayer(persistentId);
        zoneManager.AddPlayer(player);

        zoneManager.RemovePlayerByConnectionId(player.Connection.Id);

        Assert.False(zoneManager.HasPlayerWithPersistentId(persistentId));
    }

    [Fact]
    public void GetServerPlayersInZone_ReturnsOnlyPlayersInThatZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var zone1 = new Zone(1, "zone1", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, zone1);

        ServerPlayer player1 = CreateServerPlayer();
        ServerPlayer player2 = CreateServerPlayer();
        ServerPlayer player3 = CreateServerPlayer();

        zoneManager.AddPlayer(player1, 0);
        zoneManager.AddPlayer(player2, 0);
        zoneManager.AddPlayer(player3, 1);

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
        ZoneManager zoneManager = CreateZoneManager();
        var persistentId = Guid.NewGuid();
        ServerPlayer player = CreateServerPlayer(persistentId);
        zoneManager.AddPlayer(player);

        bool found = zoneManager.TryGetEntityByPersistentId(persistentId, out IEntity? entity);

        Assert.True(found);
        Assert.NotNull(entity);
        Assert.Equal(persistentId, entity.PersistentId);
    }

    [Fact]
    public void GetAllEntities_ReturnsAllEntitiesInZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayer player1 = CreateServerPlayer();
        ServerPlayer player2 = CreateServerPlayer();
        zoneManager.AddPlayer(player1);
        zoneManager.AddPlayer(player2);

        List<Entity> entities = zoneManager.GetAllEntities(0);

        Assert.Equal(2, entities.Count);
    }

    // ══════════════════════════════════════════════════════════
    // TRANSFER TESTS
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void TransferPlayerByConnectionId_MovesPlayerToNewZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var zone1 = new Zone(1, "zone1", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, zone1);
        ServerPlayer player = CreateServerPlayer();
        zoneManager.AddPlayer(player, 0);

        bool result = zoneManager.TransferPlayerByConnectionId(player.Connection.Id, 1);

        Assert.True(result);
        var playersInZone0 = zoneManager.GetServerPlayersInZone(0).ToList();
        var playersInZone1 = zoneManager.GetServerPlayersInZone(1).ToList();
        Assert.Empty(playersInZone0);
        Assert.Single(playersInZone1);
    }

    [Fact]
    public void TransferPlayerByConnectionId_SameZone_ReturnsTrue()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayer player = CreateServerPlayer();
        zoneManager.AddPlayer(player, 0);

        bool result = zoneManager.TransferPlayerByConnectionId(player.Connection.Id, 0);

        Assert.True(result);
    }

    [Fact]
    public void TransferPlayerByConnectionId_NonExistentPlayer_ReturnsFalse()
    {
        ZoneManager zoneManager = CreateZoneManager();

        bool result = zoneManager.TransferPlayerByConnectionId(Guid.NewGuid(), 0);

        Assert.False(result);
    }

    [Fact]
    public void TransferPlayerByPersistentId_MovesPlayerToNewZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var zone1 = new Zone(1, "zone1", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, zone1);
        var persistentId = Guid.NewGuid();
        ServerPlayer player = CreateServerPlayer(persistentId);
        zoneManager.AddPlayer(player, 0);

        bool result = zoneManager.TransferPlayerByPersistentId(persistentId, 1);

        Assert.True(result);
        // Player should still be findable by PersistentId after transfer
        Assert.True(zoneManager.TryGetPlayerByPersistentId(persistentId, out _));
    }

    // ══════════════════════════════════════════════════════════
    // MOB ENTITY TESTS
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void AddEntity_MobEntity_AddsToZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var mob = new MobEntity("Goblin", new Position(50, 50));

        zoneManager.AddEntity(mob, 0);

        Assert.True(zoneManager.TryGetEntityByPersistentId(mob.PersistentId, out IEntity? foundEntity));
        Assert.Equal(mob, foundEntity);
        Assert.Equal(1, zoneManager.PersistentEntityCount);
    }

    [Fact]
    public void RemoveEntity_MobEntity_RemovesFromZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var mob = new MobEntity("Wolf", new Position(100, 100));
        zoneManager.AddEntity(mob, 0);

        IEntity? removed = zoneManager.RemoveEntity(mob.PersistentId);

        Assert.NotNull(removed);
        Assert.Equal(mob.PersistentId, removed.PersistentId);
        Assert.False(zoneManager.TryGetEntityByPersistentId(mob.PersistentId, out _));
        Assert.Equal(0, zoneManager.PersistentEntityCount);
    }

    [Fact]
    public void AddEntity_MultipleMobs_AllHaveUniquePersistentIds()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var mob1 = new MobEntity("Goblin", new Position(10, 10));
        var mob2 = new MobEntity("Orc", new Position(20, 20));
        var mob3 = new MobEntity("Troll", new Position(30, 30));

        zoneManager.AddEntity(mob1, 0);
        zoneManager.AddEntity(mob2, 0);
        zoneManager.AddEntity(mob3, 0);

        Assert.NotEqual(mob1.PersistentId, mob2.PersistentId);
        Assert.NotEqual(mob2.PersistentId, mob3.PersistentId);
        Assert.NotEqual(mob1.PersistentId, mob3.PersistentId);
        Assert.Equal(3, zoneManager.PersistentEntityCount);
    }

    [Fact]
    public void GetAllEntities_IncludesMobsAndPlayers()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayer player = CreateServerPlayer();
        var mob = new MobEntity("Spider", new Position(75, 75));

        zoneManager.AddPlayer(player, 0);
        zoneManager.AddEntity(mob, 0);

        List<Entity> entities = zoneManager.GetAllEntities(0);

        Assert.Equal(2, entities.Count);
        Assert.Contains(entities, e => e is PlayerEntity);
        Assert.Contains(entities, e => e is MobEntity);
    }

    [Fact]
    public void MobEntity_IsTrulyPersistent_IsFalse()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var mob = new MobEntity("Skeleton", new Position(0, 0));

        zoneManager.AddEntity(mob, 0);

        Assert.False(mob.IsTrulyPersistent);
    }

    [Fact]
    public void PlayerEntity_IsTrulyPersistent_IsTrue()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayer player = CreateServerPlayer();

        zoneManager.AddPlayer(player, 0);

        Assert.True(player.Entity.IsTrulyPersistent);
    }
}
