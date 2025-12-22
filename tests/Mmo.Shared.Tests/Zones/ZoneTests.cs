using Mmo.Shared.Character.Entities;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Shared.Tests.Zones;

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
        var zone = new Zone(42, "TestZone", bounds);

        Assert.Equal(42, zone.Id);
        Assert.Equal("TestZone", zone.Name);
        Assert.Equal(bounds, zone.Bounds);
        Assert.Empty(zone.GetEntityIds());
    }

    [Fact]
    public void AddEntity_AddsEntityIdToZone()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(50, 50));

        zone.AddEntity(player.PersistentId);

        Assert.True(zone.HasEntity(player.PersistentId));
        Assert.Equal(1, zone.EntityCount);
    }

    [Fact]
    public void AddEntity_MultipleEntities_AddsAllIds()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(10, 10));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(20, 20));
        var player3 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player3", new Position(30, 30));

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
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var playerId = Guid.NewGuid();

        zone.AddEntity(playerId);
        zone.AddEntity(playerId); // HashSet ignores duplicates

        Assert.Equal(1, zone.EntityCount);
    }

    [Fact]
    public void AddEntity_EmptyGuid_CanBeAdded()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        // Guid.Empty is a valid Guid, not null
        zone.AddEntity(Guid.Empty);

        Assert.True(zone.HasEntity(Guid.Empty));
    }

    [Fact]
    public void RemoveEntity_RemovesFromZone()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var playerId = Guid.NewGuid();
        zone.AddEntity(playerId);

        zone.RemoveEntity(playerId);

        Assert.Equal(0, zone.EntityCount);
        Assert.False(zone.HasEntity(playerId));
    }

    [Fact]
    public void RemoveEntity_NonExistent_DoesNotThrow()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        // Should not throw
        zone.RemoveEntity(Guid.NewGuid());

        Assert.Equal(0, zone.EntityCount);
    }

    [Fact]
    public void RemoveEntity_ThenAddNewEntity_UpdatesCount()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
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
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var playerId = Guid.NewGuid();
        zone.AddEntity(playerId);

        Assert.True(zone.HasEntity(playerId));
    }

    [Fact]
    public void HasEntity_AfterRemoval_ReturnsFalse()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var playerId = Guid.NewGuid();
        zone.AddEntity(playerId);
        zone.RemoveEntity(playerId);

        Assert.False(zone.HasEntity(playerId));
    }

    [Fact]
    public void HasEntity_NotInZone_ReturnsFalse()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var playerId = Guid.NewGuid();

        Assert.False(zone.HasEntity(playerId));
    }

    [Fact]
    public void GetEntityIds_ReturnsAllIds()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
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
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        var ids = zone.GetEntityIds().ToList();

        Assert.Empty(ids);
    }
}
