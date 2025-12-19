using Mmo.Server.Entities;
using Mmo.Server.Networking;
using Mmo.Server.Tests.Helpers;
using Mmo.Server.Zones;
using Mmo.Shared.Entities;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;

namespace Mmo.Server.Tests.Zones;

[Collection("IdRegistry")]
public class ZoneManagerTests : IDisposable
{
    private static readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _sharedMockNetworkServer;

    public ZoneManagerTests()
    {
        // Clear IdRegistry before each test
        IdRegistry.Instance.Clear();
        _sharedMockNetworkServer = new MockNetworkServer(_mockLog, true);
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

    private ServerPlayerCharacter CreateServerPlayer(Guid? persistentId = null, Guid? connectionId = null)
    {
        var entity = new PlayerEntity(
            persistentId ?? IdRegistry.Instance.GeneratePersistentId(),
            Guid.NewGuid(),
            "TestPlayer",
            new Position(100, 100)
        );
        Guid connId = connectionId ?? IdRegistry.Instance.GeneratePersistentId();
        ClientConnection connection = _sharedMockNetworkServer.GetOrCreateMockConnection(connId);
        return new ServerPlayerCharacter(entity, connection);
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
        ServerPlayerCharacter playerCharacter = CreateServerPlayer();

        zoneManager.AddPlayer(playerCharacter);

        Assert.Equal(1, zoneManager.PlayerCount);
        Assert.True(zoneManager.HasPlayerWithConnectionId(playerCharacter.Connection.Id));
    }

    [Fact]
    public void AddPlayer_AddsToSpecificZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var newZone = new Zone(1, "other", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, newZone);
        ServerPlayerCharacter playerCharacter = CreateServerPlayer();

        zoneManager.AddPlayer(playerCharacter, 1);

        Assert.Equal(1, zoneManager.PlayerCount);
        var playersInZone = zoneManager.GetServerPlayersInZone(1).ToList();
        Assert.Single(playersInZone);
        Assert.Equal(playerCharacter.Connection.Id, playersInZone[0].Connection.Id);
    }

    [Fact]
    public void AddPlayer_NonExistentZone_ThrowsInvalidOperationException()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayerCharacter playerCharacter = CreateServerPlayer();

        Assert.Throws<InvalidOperationException>(() => zoneManager.AddPlayer(playerCharacter, 999));
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
        ServerPlayerCharacter playerCharacter = CreateServerPlayer();
        zoneManager.AddPlayer(playerCharacter);

        bool found =
            zoneManager.TryGetPlayerByConnectionId(playerCharacter.Connection.Id,
                out ServerPlayerCharacter? foundPlayer);

        Assert.True(found);
        Assert.NotNull(foundPlayer);
        Assert.Equal(playerCharacter.Connection.Id, foundPlayer.Connection.Id);
    }

    [Fact]
    public void TryGetPlayerByConnectionId_NonExistent_ReturnsFalse()
    {
        ZoneManager zoneManager = CreateZoneManager();

        bool found = zoneManager.TryGetPlayerByConnectionId(Guid.NewGuid(), out ServerPlayerCharacter? foundPlayer);

        Assert.False(found);
        Assert.Null(foundPlayer);
    }

    [Fact]
    public void TryGetPlayerByPersistentId_ExistingPlayer_ReturnsTrue()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var persistentId = Guid.NewGuid();
        ServerPlayerCharacter playerCharacter = CreateServerPlayer(persistentId);
        zoneManager.AddPlayer(playerCharacter);

        bool found = zoneManager.TryGetPlayerByPersistentId(persistentId, out ServerPlayerCharacter? foundPlayer);

        Assert.True(found);
        Assert.NotNull(foundPlayer);
        Assert.Equal(persistentId, foundPlayer.Entity.PersistentId);
    }

    [Fact]
    public void RemovePlayerByConnectionId_ExistingPlayer_RemovesAndReturnsPlayer()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayerCharacter playerCharacter = CreateServerPlayer();
        zoneManager.AddPlayer(playerCharacter);

        ServerPlayerCharacter? removedPlayer = zoneManager.RemovePlayerByConnectionId(playerCharacter.Connection.Id);

        Assert.NotNull(removedPlayer);
        Assert.Equal(playerCharacter.Connection.Id, removedPlayer.Connection.Id);
        Assert.Equal(0, zoneManager.PlayerCount);
        Assert.False(zoneManager.HasPlayerWithConnectionId(playerCharacter.Connection.Id));
    }

    [Fact]
    public void RemovePlayerByConnectionId_NonExistent_ReturnsNull()
    {
        ZoneManager zoneManager = CreateZoneManager();

        ServerPlayerCharacter? removedPlayer = zoneManager.RemovePlayerByConnectionId(Guid.NewGuid());

        Assert.Null(removedPlayer);
    }

    [Fact]
    public void RemovePlayerByConnectionId_AlsoRemovesFromPersistentIdLookup()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var persistentId = Guid.NewGuid();
        ServerPlayerCharacter playerCharacter = CreateServerPlayer(persistentId);
        zoneManager.AddPlayer(playerCharacter);

        zoneManager.RemovePlayerByConnectionId(playerCharacter.Connection.Id);

        Assert.False(zoneManager.HasPlayerWithPersistentId(persistentId));
    }

    [Fact]
    public void GetServerPlayersInZone_ReturnsOnlyPlayersInThatZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        var zone1 = new Zone(1, "zone1", new ZoneBounds(0, 0, 500, 500));
        zoneManager.RegisterZone(1, zone1);

        ServerPlayerCharacter player1 = CreateServerPlayer();
        ServerPlayerCharacter player2 = CreateServerPlayer();
        ServerPlayerCharacter player3 = CreateServerPlayer();

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
        ServerPlayerCharacter playerCharacter = CreateServerPlayer(persistentId);
        zoneManager.AddPlayer(playerCharacter);

        bool found = zoneManager.TryGetEntityByPersistentId(persistentId, out IEntity? entity);

        Assert.True(found);
        Assert.NotNull(entity);
        Assert.Equal(persistentId, entity.PersistentId);
    }

    [Fact]
    public void GetAllEntities_ReturnsAllEntitiesInZone()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayerCharacter player1 = CreateServerPlayer();
        ServerPlayerCharacter player2 = CreateServerPlayer();
        zoneManager.AddPlayer(player1);
        zoneManager.AddPlayer(player2);

        List<IEntity> entities = zoneManager.GetAllEntities(0);

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
        ServerPlayerCharacter playerCharacter = CreateServerPlayer();
        zoneManager.AddPlayer(playerCharacter, 0);

        bool result = zoneManager.TransferPlayerByConnectionId(playerCharacter.Connection.Id, 1);

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
        ServerPlayerCharacter playerCharacter = CreateServerPlayer();
        zoneManager.AddPlayer(playerCharacter, 0);

        bool result = zoneManager.TransferPlayerByConnectionId(playerCharacter.Connection.Id, 0);

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
        ServerPlayerCharacter playerCharacter = CreateServerPlayer(persistentId);
        zoneManager.AddPlayer(playerCharacter, 0);

        bool result = zoneManager.TransferPlayerByPersistentId(persistentId, 1);

        Assert.True(result);
        // Player should still be findable by PersistentId after transfer
        Assert.True(zoneManager.TryGetPlayerByPersistentId(persistentId, out _));
    }

    [Fact]
    public void PlayerEntity_IsTrulyPersistent_IsTrue()
    {
        ZoneManager zoneManager = CreateZoneManager();
        ServerPlayerCharacter playerCharacter = CreateServerPlayer();

        zoneManager.AddPlayer(playerCharacter, 0);

        Assert.True(playerCharacter.Entity.IsTrulyPersistent);
    }
}
