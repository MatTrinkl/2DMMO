using Mmo.Shared.Entities;
using Mmo.Shared.Zones;
using Moq;

namespace Mmo.Shared.Tests.Zones;

public class ZoneTests
{
    [Fact]
    public void AddEntity_SameEntityTwice_ThrowsArgumentException()
    {
        var z = new Zone(1, "main", new ZoneBounds(0, 0, 0, 0));
        var entityMock = new Mock<IEntity>();
        z.AddEntity(entityMock.Object);
        Assert.Throws<ArgumentException>(() => z.AddEntity(entityMock.Object));
    }

    [Fact]
    public void RemoveEntity_ThenAddNewEntity_ReusesEntityId()
    {
        var z = new Zone(1, "main", new ZoneBounds(0, 0, 0, 0));
        var entityMock = new Mock<IEntity>();
        var entityMock2 = new Mock<IEntity>();
        var entityMock3 = new Mock<IEntity>();
        z.AddEntity(entityMock.Object);
        z.AddEntity(entityMock2.Object);
        z.AddEntity(entityMock3.Object);
        int idOld = entityMock.Object.EntityId.Id;
        z.RemoveEntity(idOld);
        z.AddEntity(entityMock.Object);
        Assert.True(entityMock.Object.EntityId.Id == idOld);
        Assert.True(z.HasEntity(idOld));
    }
}
