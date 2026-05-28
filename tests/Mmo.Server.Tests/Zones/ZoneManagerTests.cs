using Mmo.Server.Connections;
using Mmo.Server.Entities;
using Mmo.Server.Player;
using Mmo.Server.Tests.Helpers;
using Mmo.Server.Zones;
using Mmo.Shared.Core;
using Mmo.Shared.Movement.Records;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Prefab;

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
        Zone defaultZone = TestHelpers.CreateTestZone(0, "default");
        var manger = new ZoneManager(0);
        manger.RegisterZone(defaultZone);
        return manger;
    }

    private ServerPlayerCharacter CreateServerPlayer(Guid? persistentId = null, Guid? connectionId = null)
    {
        var entity = new CharacterEntity(
            persistentId ?? IdRegistry.Instance.GeneratePersistentId(),
            Guid.NewGuid(),
            "TestPlayer",
            new Position(100, 100),
            EntityIdentity.Unassigned(PrefabIds.PlayerDefault)
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
        Zone newZone = TestHelpers.CreateTestZone(1, "second");

        zoneManager.RegisterZone(newZone);

        Assert.Equal(2, zoneManager.ZoneCount);
        Assert.NotNull(zoneManager.GetZone(1));
    }

    [Fact]
    public void RegisterZone_DuplicateId_ReturnsFalse()
    {
        ZoneManager zoneManager = CreateZoneManager();
        Zone duplicateZone = TestHelpers.CreateTestZone(0, "duplicate");

        // TryAdd returns false when key already exists
        bool result = zoneManager.RegisterZone(duplicateZone);
        Assert.False(result);
    }

    [Fact]
    public void GetZone_NonExistent_ReturnsDefault()
    {
        ZoneManager zoneManager = CreateZoneManager();

        // GetValueOrDefault returns default(Zone?) which is null for nullable struct
        Zone? result = zoneManager.GetZone(999);
        Assert.Null(result);
    }

    // ══════════════════════════════════════════════════════════
    // PLAYER MANAGEMENT TESTS
    // ══════════════════════════════════════════════════════════


    [Fact]
    public void TryGetPlayerByConnectionId_NonExistent_ReturnsFalse()
    {
        ZoneManager zoneManager = CreateZoneManager();

        bool found = zoneManager.TryGetPlayerByConnectionId(Guid.NewGuid(), out ServerPlayerCharacter? foundPlayer);

        Assert.False(found);
        Assert.Null(foundPlayer);
    }
}
