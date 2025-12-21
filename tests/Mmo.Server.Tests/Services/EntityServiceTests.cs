using Mmo.Server.Entities.Services;
using Mmo.Server.Tests.Helpers;
using Mmo.Server.Zones;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.Tests.Services;

[Collection("IdRegistry")]
public class EntityServiceTests : IDisposable
{
    private readonly MockLog _log = new();
    private readonly ZoneManager _zoneManager;

    public EntityServiceTests()
    {
        IdRegistry.Instance.Clear();
        _zoneManager = new ZoneManager(0);
        
        // Register default zone
        var defaultZone = new Zone(0, "default", new ZoneBounds(0, 0, 1000, 1000));
        _zoneManager.RegisterZone(defaultZone);
    }

    public void Dispose()
    {
        IdRegistry.Instance.Clear();
    }

    [Fact]
    public void SpawnEntity_ValidZone_SucceedsAndAssignsLocalId()
    {
        var service = new EntityService(_zoneManager, _log);
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "TestPlayer", new Position(100, 100));

        var result = service.SpawnEntity(player, 0);

        Assert.True(result.Success);
        Assert.NotNull(result.LocalId);
        Assert.NotNull(result.ZoneId);
        Assert.Equal((ushort)0, result.LocalId.Value); // First entity gets ID 0
        Assert.Equal((ushort)0, result.ZoneId.Value);
        Assert.Equal(0, player.RuntimeId.LocalId);
        Assert.Equal(0, player.RuntimeId.ZoneId);
    }

    [Fact]
    public void SpawnEntity_InvalidZone_ReturnsFailure()
    {
        var service = new EntityService(_zoneManager, _log);
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "TestPlayer", new Position(100, 100));

        var result = service.SpawnEntity(player, 999);

        Assert.False(result.Success);
        Assert.Equal("ZONE_NOT_FOUND", result.Error);
    }

    [Fact]
    public void SpawnEntity_MultipleEntities_AssignsSequentialIds()
    {
        var service = new EntityService(_zoneManager, _log);
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(100, 100));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(200, 200));
        var player3 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player3", new Position(300, 300));

        var result1 = service.SpawnEntity(player1, 0);
        var result2 = service.SpawnEntity(player2, 0);
        var result3 = service.SpawnEntity(player3, 0);

        Assert.NotNull(result1.LocalId);
        Assert.NotNull(result2.LocalId);
        Assert.NotNull(result3.LocalId);
        Assert.Equal((ushort)0, result1.LocalId.Value);
        Assert.Equal((ushort)1, result2.LocalId.Value);
        Assert.Equal((ushort)2, result3.LocalId.Value);
    }

    [Fact]
    public void SpawnEntity_RegistersInIdRegistry()
    {
        var service = new EntityService(_zoneManager, _log);
        var playerId = Guid.NewGuid();
        var player = new PlayerEntity(playerId, Guid.NewGuid(), "TestPlayer", new Position(100, 100));

        service.SpawnEntity(player, 0);

        Assert.True(IdRegistry.Instance.HasEntity(playerId));
        Assert.True(IdRegistry.Instance.TryGetEntity(playerId, out var retrieved));
        Assert.Equal(player, retrieved);
    }

    [Fact]
    public void SpawnEntity_AddsToZone()
    {
        var service = new EntityService(_zoneManager, _log);
        var playerId = Guid.NewGuid();
        var player = new PlayerEntity(playerId, Guid.NewGuid(), "TestPlayer", new Position(100, 100));

        service.SpawnEntity(player, 0);

        var zone = _zoneManager.GetZone(0);
        Assert.NotNull(zone);
        Assert.True(zone.Value.HasEntity(playerId));
    }

    [Fact]
    public void DespawnEntity_ExistingEntity_SucceedsAndCleanup()
    {
        var service = new EntityService(_zoneManager, _log);
        var playerId = Guid.NewGuid();
        var player = new PlayerEntity(playerId, Guid.NewGuid(), "TestPlayer", new Position(100, 100));
        
        service.SpawnEntity(player, 0);

        var result = service.DespawnEntity(playerId);

        Assert.True(result);
        Assert.False(IdRegistry.Instance.HasEntity(playerId));
        
        var zone = _zoneManager.GetZone(0);
        Assert.NotNull(zone);
        Assert.False(zone.Value.HasEntity(playerId));
    }

    [Fact]
    public void DespawnEntity_NonExistentEntity_ReturnsFalse()
    {
        var service = new EntityService(_zoneManager, _log);

        var result = service.DespawnEntity(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public void DespawnEntity_ReleasesLocalId()
    {
        var service = new EntityService(_zoneManager, _log);
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(100, 100));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(200, 200));

        service.SpawnEntity(player1, 0); // Gets ID 0
        service.SpawnEntity(player2, 0); // Gets ID 1
        service.DespawnEntity(player1.PersistentId); // Releases ID 0

        var player3 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player3", new Position(300, 300));
        var result = service.SpawnEntity(player3, 0); // Should reuse ID 0

        Assert.NotNull(result.LocalId);
        Assert.Equal((ushort)0, result.LocalId.Value);
    }

    [Fact]
    public void GetEntitiesInZone_ReturnsAllEntities()
    {
        var service = new EntityService(_zoneManager, _log);
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(100, 100));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(200, 200));

        service.SpawnEntity(player1, 0);
        service.SpawnEntity(player2, 0);

        var entities = service.GetEntitiesInZone(0).ToList();

        Assert.Equal(2, entities.Count);
        Assert.Contains(player1, entities);
        Assert.Contains(player2, entities);
    }

    [Fact]
    public void GetEntity_ExistingEntity_ReturnsEntity()
    {
        var service = new EntityService(_zoneManager, _log);
        var playerId = Guid.NewGuid();
        var player = new PlayerEntity(playerId, Guid.NewGuid(), "TestPlayer", new Position(100, 100));
        
        service.SpawnEntity(player, 0);

        var result = service.GetEntity(playerId);

        Assert.NotNull(result);
        Assert.Equal(player, result);
    }

    [Fact]
    public void GetEntity_NonExistentEntity_ReturnsNull()
    {
        var service = new EntityService(_zoneManager, _log);

        var result = service.GetEntity(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void GetEntitiesInRange_ReturnsOnlyEntitiesWithinRadius()
    {
        var service = new EntityService(_zoneManager, _log);
        
        // Spawn entities at different positions
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(100, 100));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(105, 105)); // ~7 units away
        var player3 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player3", new Position(200, 200)); // ~141 units away

        service.SpawnEntity(player1, 0);
        service.SpawnEntity(player2, 0);
        service.SpawnEntity(player3, 0);

        // Get entities within 10 units of (100, 100)
        var entitiesInRange = service.GetEntitiesInRange(0, new Position(100, 100), 10).ToList();

        Assert.Equal(2, entitiesInRange.Count); // player1 and player2
        Assert.Contains(player1, entitiesInRange);
        Assert.Contains(player2, entitiesInRange);
        Assert.DoesNotContain(player3, entitiesInRange);
    }

    [Fact]
    public void GetEntitiesInRange_EmptyZone_ReturnsEmpty()
    {
        var service = new EntityService(_zoneManager, _log);

        var entitiesInRange = service.GetEntitiesInRange(0, new Position(100, 100), 50).ToList();

        Assert.Empty(entitiesInRange);
    }

    [Fact]
    public void GetVisibleEntities_ReturnsOtherEntitiesInSameZone()
    {
        var service = new EntityService(_zoneManager, _log);
        
        var playerId = Guid.NewGuid();
        var player = new PlayerEntity(playerId, Guid.NewGuid(), "Player", new Position(100, 100));
        var other1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Other1", new Position(200, 200));
        var other2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Other2", new Position(300, 300));

        service.SpawnEntity(player, 0);
        service.SpawnEntity(other1, 0);
        service.SpawnEntity(other2, 0);

        var visibleEntities = service.GetVisibleEntities(playerId).ToList();

        Assert.Equal(2, visibleEntities.Count); // other1 and other2, not player itself
        Assert.Contains(other1, visibleEntities);
        Assert.Contains(other2, visibleEntities);
        Assert.DoesNotContain(player, visibleEntities);
    }

    [Fact]
    public void GetVisibleEntities_NonExistentPlayer_ReturnsEmpty()
    {
        var service = new EntityService(_zoneManager, _log);

        var visibleEntities = service.GetVisibleEntities(Guid.NewGuid()).ToList();

        Assert.Empty(visibleEntities);
    }

    [Fact]
    public void GetVisibleEntities_PlayerAloneInZone_ReturnsEmpty()
    {
        var service = new EntityService(_zoneManager, _log);
        
        var playerId = Guid.NewGuid();
        var player = new PlayerEntity(playerId, Guid.NewGuid(), "Player", new Position(100, 100));

        service.SpawnEntity(player, 0);

        var visibleEntities = service.GetVisibleEntities(playerId).ToList();

        Assert.Empty(visibleEntities); // No other entities
    }
}
