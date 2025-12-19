using Mmo.Shared.Entities;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;
using Moq;

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

        Assert.Equal(42, zone.ZoneId);
        Assert.Equal("TestZone", zone.ZoneName);
        Assert.Equal(bounds, zone.Bounds);
        Assert.Empty(zone.Entities);
    }

    [Fact]
    public void AddEntity_AssignsCorrectEntityId()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(50, 50));

        zone.AddEntity(player);

        Assert.Equal(0, player.RuntimeId.LocalId); // First entity gets ID 0
        Assert.Equal(1, player.RuntimeId.ZoneId); // ZoneId matches the zone
        Assert.Equal(0, player.RuntimeId.ShardId); // ShardId is 0 (hardcoded for prototype)
    }

    [Fact]
    public void AddEntity_MultipleEntities_AssignsSequentialIds()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(10, 10));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(20, 20));
        var player3 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player3", new Position(30, 30));

        zone.AddEntity(player1);
        zone.AddEntity(player2);
        zone.AddEntity(player3);

        Assert.Equal(0, player1.RuntimeId.LocalId);
        Assert.Equal(1, player2.RuntimeId.LocalId);
        Assert.Equal(2, player3.RuntimeId.LocalId);
        Assert.Equal(3, zone.Entities.Count);
    }

    [Fact]
    public void AddEntity_SameEntityTwice_ThrowsArgumentException()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 0, 0));
        var entityMock = new Mock<IEntity>();
        zone.AddEntity(entityMock.Object);
        Assert.Throws<ArgumentException>(() => zone.AddEntity(entityMock.Object));
    }

    [Fact]
    public void AddEntity_NullEntity_ThrowsArgumentNullException()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        Assert.Throws<ArgumentNullException>(() => zone.AddEntity(null!));
    }

    [Fact]
    public void RemoveEntity_RemovesFromDictionary()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(50, 50));
        zone.AddEntity(player);
        int entityId = player.RuntimeId.LocalId;

        zone.RemoveEntity(entityId);

        Assert.Empty(zone.Entities);
        Assert.False(zone.HasEntity(entityId));
    }

    [Fact]
    public void RemoveEntity_NonExistent_DoesNotThrow()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        // Should not throw
        zone.RemoveEntity(999);
    }

    [Fact]
    public void RemoveEntity_ThenAddNewEntity_ReusesEntityId()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(10, 10));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(20, 20));
        var player3 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player3", new Position(30, 30));

        zone.AddEntity(player1); // Gets ID 0
        zone.AddEntity(player2); // Gets ID 1
        zone.AddEntity(player3); // Gets ID 2

        int reusedId = player1.RuntimeId.LocalId;
        zone.RemoveEntity(reusedId); // Free ID 0

        var player4 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player4", new Position(40, 40));
        zone.AddEntity(player4); // Should reuse ID 0

        Assert.Equal(reusedId, player4.RuntimeId.LocalId);
        Assert.True(zone.HasEntity(reusedId));
    }

    [Fact]
    public void HasEntity_WithEntity_ReturnsTrue()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(50, 50));
        zone.AddEntity(player);

        Assert.True(zone.HasEntity(player));
    }

    [Fact]
    public void HasEntity_WithEntityId_ReturnsTrue()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(50, 50));
        zone.AddEntity(player);

        Assert.True(zone.HasEntity(player.RuntimeId.LocalId));
    }

    [Fact]
    public void HasEntity_NotInZone_ReturnsFalse()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(50, 50));

        Assert.False(zone.HasEntity(player));
    }

    [Fact]
    public void HasEntity_WithNullEntity_ThrowsArgumentNullException()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        Assert.Throws<ArgumentNullException>(() => zone.HasEntity(null!));
    }

    [Fact]
    public void IsPositionInBounds_InsideBounds_ReturnsTrue()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var position = new Position(50, 50);

        Assert.True(zone.IsPositionInBounds(position));
    }

    [Fact]
    public void IsPositionInBounds_OutsideBounds_ReturnsFalse()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var position = new Position(150, 150);

        Assert.False(zone.IsPositionInBounds(position));
    }

    [Fact]
    public void IsPositionInBounds_NullPosition_ThrowsArgumentNullException()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        Assert.Throws<ArgumentNullException>(() => zone.IsPositionInBounds(null!));
    }

    [Fact]
    public void ZoneId_CanBeUpdated()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        zone.ZoneId = 42;

        Assert.Equal(42, zone.ZoneId);
    }

    [Fact]
    public void Entities_IsAccessible()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(50, 50));
        zone.AddEntity(player);

        Assert.NotEmpty(zone.Entities);
        Assert.Contains(player, zone.Entities.Values);
    }

    [Fact]
    public void GetPlayers_ReturnsOnlyPlayerEntities()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(10, 10));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(20, 20));
        zone.AddEntity(player1);
        zone.AddEntity(player2);

        var players = zone.GetPlayers().ToList();

        Assert.Equal(2, players.Count);
        Assert.Contains(player1, players);
        Assert.Contains(player2, players);
    }

    [Fact]
    public void GetPlayers_EmptyZone_ReturnsEmpty()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        var players = zone.GetPlayers().ToList();

        Assert.Empty(players);
    }

    [Fact]
    public void PlayerCount_ReturnsCorrectCount()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(10, 10));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(20, 20));

        Assert.Equal(0, zone.PlayerCount());

        zone.AddEntity(player1);
        Assert.Equal(1, zone.PlayerCount());

        zone.AddEntity(player2);
        Assert.Equal(2, zone.PlayerCount());

        zone.RemoveEntity(player1.RuntimeId.LocalId);
        Assert.Equal(1, zone.PlayerCount());
    }

    [Fact]
    public void IsPositionInBounds_OnBoundary_ReturnsTrue()
    {
        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100));

        // Corners
        Assert.True(zone.IsPositionInBounds(new Position(0, 0)));
        Assert.True(zone.IsPositionInBounds(new Position(100, 0)));
        Assert.True(zone.IsPositionInBounds(new Position(0, 100)));
        Assert.True(zone.IsPositionInBounds(new Position(100, 100)));

        // Edges
        Assert.True(zone.IsPositionInBounds(new Position(50, 0)));
        Assert.True(zone.IsPositionInBounds(new Position(50, 100)));
        Assert.True(zone.IsPositionInBounds(new Position(0, 50)));
        Assert.True(zone.IsPositionInBounds(new Position(100, 50)));
    }

    [Fact]
    public void CustomIdRegistry_IsUsedForIdAllocation()
    {
        var mockIdRegistry = new Mock<IIdRegistry>();
        mockIdRegistry.Setup(r => r.GetNextLocalId(It.IsAny<ushort>(), It.IsAny<ushort>())).Returns(42);

        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100), mockIdRegistry.Object);
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(50, 50));

        zone.AddEntity(player);

        Assert.Equal(42, player.RuntimeId.LocalId);
        mockIdRegistry.Verify(r => r.GetNextLocalId(1, 0), Times.Once);
    }

    [Fact]
    public void CustomIdRegistry_ReleaseIdCalledOnRemove()
    {
        var mockIdRegistry = new Mock<IIdRegistry>();
        mockIdRegistry.Setup(r => r.GetNextLocalId(It.IsAny<ushort>(), It.IsAny<ushort>())).Returns(42);

        var zone = new Zone(1, "main", new ZoneBounds(0, 0, 100, 100), mockIdRegistry.Object);
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(50, 50));
        zone.AddEntity(player);

        zone.RemoveEntity(42);

        mockIdRegistry.Verify(r => r.ReleaseLocalId(1, 0, 42), Times.Once);
    }
}
