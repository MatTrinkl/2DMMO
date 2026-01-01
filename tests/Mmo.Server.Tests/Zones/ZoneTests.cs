using Mmo.Server.Entities;
using Mmo.Server.Zones;
using Mmo.Server.Zones.Configurations;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Zones.Records;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.Tests.Zones;

[Collection("IdRegistry")]
public class ZoneTests : IDisposable
{
    public ZoneTests()
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
    public void Constructor_SetsAllProperties()
    {
        var bounds = new ZoneBounds(10, 20, 100, 200);
        var zoneConfig = new ZoneConfig
        {
            ZoneId = 42,
            InternalName = "TestZone",
            DisplayName = "Test Zone Display",
            Bounds = new ZoneBoundsConfig(10, 20, 100, 200)
        };
        var zone = new Zone(zoneConfig, new ZoneContext(zoneConfig));

        Assert.Equal(42, zone.ZoneId);
        Assert.Equal("TestZone", zone.ZoneName);
        Assert.Equal(bounds, zone.Config.Bounds.FromConfig());
        Assert.Empty(zone.GetEntityIds());
    }

    [Fact]
    public void AddEntity_AddsEntityIdToZone()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var player = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(50, 50),EntityIdentity.Unassigned(0));

        zone.AddEntity(player.PersistentId);

        Assert.True(zone.HasEntity(player.PersistentId));
        Assert.Equal(1, zone.EntityCount);
    }

    [Fact]
    public void AddEntity_MultipleEntities_AddsAllIds()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var player1 = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(10, 10),EntityIdentity.Unassigned(0));
        var player2 = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(20, 20),EntityIdentity.Unassigned(0));
        var player3 = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player3", new Position(30, 30),EntityIdentity.Unassigned(0));

        zone.AddEntity(player1.PersistentId);
        zone.AddEntity(player2.PersistentId);
        zone.AddEntity(player3.PersistentId);

        Assert.True(zone.HasEntity(player1.PersistentId));
        Assert.True(zone.HasEntity(player2.PersistentId));
        Assert.True(zone.HasEntity(player3.PersistentId));
        Assert.Equal(3, zone.EntityCount);
    }

    [Fact]
    public void AddEntity_SameEntityTwice_DoesNotDuplicate()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var playerId = Guid.NewGuid();

        zone.AddEntity(playerId);
        zone.AddEntity(playerId); // HashSet ignores duplicates

        Assert.Equal(1, zone.EntityCount);
    }

    [Fact]
    public void AddEntity_EmptyGuid_CanBeAdded()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));

        // Guid.Empty is a valid Guid, not null
        zone.AddEntity(Guid.Empty);

        Assert.True(zone.HasEntity(Guid.Empty));
    }

    [Fact]
    public void RemoveEntity_RemovesFromZone()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var playerId = Guid.NewGuid();
        zone.AddEntity(playerId);

        zone.RemoveEntity(playerId);

        Assert.Equal(0, zone.EntityCount);
        Assert.False(zone.HasEntity(playerId));
    }

    [Fact]
    public void RemoveEntity_NonExistent_DoesNotThrow()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));

        // Should not throw
        zone.RemoveEntity(Guid.NewGuid());

        Assert.Equal(0, zone.EntityCount);
    }

    [Fact]
    public void RemoveEntity_ThenAddNewEntity_UpdatesCount()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var player3Id = Guid.NewGuid();

        zone.AddEntity(player1Id);
        zone.AddEntity(player2Id);
        zone.AddEntity(player3Id);
        Assert.Equal(3, zone.EntityCount);

        zone.RemoveEntity(player1Id);
        Assert.Equal(2, zone.EntityCount);

        var player4Id = Guid.NewGuid();
        zone.AddEntity(player4Id);

        Assert.Equal(3, zone.EntityCount);
        Assert.True(zone.HasEntity(player4Id));
        Assert.False(zone.HasEntity(player1Id));
    }

    [Fact]
    public void HasEntity_WithEntityId_ReturnsTrue()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var playerId = Guid.NewGuid();
        zone.AddEntity(playerId);

        Assert.True(zone.HasEntity(playerId));
    }

    [Fact]
    public void HasEntity_AfterRemoval_ReturnsFalse()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var playerId = Guid.NewGuid();
        zone.AddEntity(playerId);
        zone.RemoveEntity(playerId);

        Assert.False(zone.HasEntity(playerId));
    }

    [Fact]
    public void HasEntity_NotInZone_ReturnsFalse()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var playerId = Guid.NewGuid();

        Assert.False(zone.HasEntity(playerId));
    }

    [Fact]
    public void GetEntityIds_ReturnsAllIds()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        zone.AddEntity(id1);
        zone.AddEntity(id2);
        zone.AddEntity(id3);

        var ids = zone.GetEntityIds().ToList();

        Assert.Equal(3, ids.Count);
        Assert.Contains(id1, ids);
        Assert.Contains(id2, ids);
        Assert.Contains(id3, ids);
    }

    [Fact]
    public void GetEntityIds_EmptyZone_ReturnsEmpty()
    {
        var zoneConfig =  new ZoneConfig();
        var zone = new Zone(new ZoneConfig(),new ZoneContext(zoneConfig));

        var ids = zone.GetEntityIds().ToList();

        Assert.Empty(ids);
    }
}
