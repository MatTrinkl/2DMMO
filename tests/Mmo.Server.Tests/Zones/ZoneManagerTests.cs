using Mmo.Server.Zones;
using Mmo.Shared.Entities;
using Mmo.Shared.Zones;
using Moq;

namespace Mmo.Server.Tests.Zones;

public class ZoneManagerTests
{
    [Fact]
    public void AddZonesToZoneManager()
    {
        var zoneManager = new ZoneManager(0, new Zone(0, "default", new ZoneBounds(0, 0, 0, 0)));
        zoneManager.RegisterZone(1, new Zone(1, "main", new ZoneBounds(0, 0, 0, 0)));
        Assert.NotNull(zoneManager.GetZone(1));
        Assert.True(zoneManager.GetDefaultZone()!.ZoneId == 0);
    }

    [Fact]
    public void RemoveZonesFromZoneManager()
    {
        var zoneManager = new ZoneManager(0, new Zone(0, "default", new ZoneBounds(0, 0, 0, 0)));

        Assert.True(zoneManager.UnregisterZone(0));
    }

    [Fact]
    public void AddZonesTwiceToZoneManager()
    {
        var defaultZone = new Zone(0, "default", new ZoneBounds(0, 0, 0, 0));
        var zoneManager = new ZoneManager(0, defaultZone);
        Assert.Throws<ArgumentException>(() => zoneManager.RegisterZone(0, defaultZone));
    }

    [Fact]
    public void TransferEntity()
    {
        var mockEntity = new Mock<IEntity>();
        var zoneManager = new ZoneManager(0, new Zone(0, "default", new ZoneBounds(0, 0, 0, 0)));
        zoneManager.RegisterZone(1, new Zone(1, "main", new ZoneBounds(0, 0, 0, 0)));
        zoneManager.GetDefaultZone()!.AddEntity(mockEntity.Object);
        zoneManager.TransferEntity(mockEntity.Object, 0, 1);
        Assert.True(zoneManager.GetZone(1)!.HasEntity(mockEntity.Object));
    }

    [Fact]
    public void TransferEntityWhichIsNotInZone()
    {
        var mockEntity = new Mock<IEntity>();
        var zoneManager = new ZoneManager(0, new Zone(0, "default", new ZoneBounds(0, 0, 0, 0)));
        zoneManager.RegisterZone(1, new Zone(1, "main", new ZoneBounds(0, 0, 0, 0)));

        Assert.Throws<ArgumentException>(() => zoneManager.TransferEntity(mockEntity.Object, 1, 0));
    }

    [Fact]
    public void TransferNullEntity()
    {
        var zoneManager = new ZoneManager(0, new Zone(0, "default", new ZoneBounds(0, 0, 0, 0)));
        zoneManager.RegisterZone(1, new Zone(1, "main", new ZoneBounds(0, 0, 0, 0)));

        Assert.Throws<ArgumentNullException>(() => zoneManager.TransferEntity(null!, 0, 1));
    }
}
