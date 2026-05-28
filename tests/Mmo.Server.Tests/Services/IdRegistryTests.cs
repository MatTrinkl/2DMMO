using Mmo.Server.Entities;
using Mmo.Server.Entities.Interfaces;
using Mmo.Shared.Core;
using Mmo.Shared.Movement.Records;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Entities.Structs;

namespace Mmo.Shared.Tests.Entities;

[Collection("IdRegistry")]
public class IdRegistryTests : IDisposable
{
    public IdRegistryTests()
    {
        // Clear the registry before each test
        IdRegistry.Instance.Clear();
    }

    public void Dispose()
    {
        // Clean up after each test
        IdRegistry.Instance.Clear();
    }

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        IdRegistry instance1 = IdRegistry.Instance;
        IdRegistry instance2 = IdRegistry.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void GeneratePersistentId_ReturnsUniqueGuids()
    {
        Guid id1 = IdRegistry.Instance.GeneratePersistentId();
        Guid id2 = IdRegistry.Instance.GeneratePersistentId();
        Guid id3 = IdRegistry.Instance.GeneratePersistentId();

        Assert.NotEqual(Guid.Empty, id1);
        Assert.NotEqual(Guid.Empty, id2);
        Assert.NotEqual(Guid.Empty, id3);
        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id2, id3);
        Assert.NotEqual(id1, id3);
    }

    [Fact]
    public void GetNextLocalId_ReturnsIncrementingIds()
    {
        ushort zoneId = 1;

        int id1 = IdRegistry.Instance.GetNextLocalId(zoneId);
        int id2 = IdRegistry.Instance.GetNextLocalId(zoneId);
        int id3 = IdRegistry.Instance.GetNextLocalId(zoneId);

        Assert.Equal(0, id1);
        Assert.Equal(1, id2);
        Assert.Equal(2, id3);
    }

    [Fact]
    public void GetNextLocalId_DifferentZones_HaveSeparateCounters()
    {
        ushort zone1 = 1;
        ushort zone2 = 2;

        int id1Zone1 = IdRegistry.Instance.GetNextLocalId(zone1);
        int id2Zone1 = IdRegistry.Instance.GetNextLocalId(zone1);
        int id1Zone2 = IdRegistry.Instance.GetNextLocalId(zone2);

        Assert.Equal(0, id1Zone1);
        Assert.Equal(1, id2Zone1);
        Assert.Equal(0, id1Zone2);
    }

    [Fact]
    public void GetNextLocalId_DifferentShards_HaveSeparateCounters()
    {
        ushort zoneId = 1;
        ushort shard0 = 0;
        ushort shard1 = 1;

        int idShard0 = IdRegistry.Instance.GetNextLocalId(zoneId, shard0);
        int idShard1 = IdRegistry.Instance.GetNextLocalId(zoneId, shard1);

        Assert.Equal(0, idShard0);
        Assert.Equal(0, idShard1);
    }

    [Fact]
    public void ReleaseLocalId_AllowsReuse()
    {
        ushort zoneId = 1;

        ushort id1 = IdRegistry.Instance.GetNextLocalId(zoneId);
        IdRegistry.Instance.GetNextLocalId(zoneId);

        IdRegistry.Instance.ReleaseLocalId(zoneId, 0, id1);

        int id3 = IdRegistry.Instance.GetNextLocalId(zoneId);

        Assert.Equal(id1, id3); // Reused ID
    }

    [Fact]
    public void RegisterEntity_CanBeRetrievedByPersistentId()
    {
        TestEntity entity = CreateTestEntity();

        IdRegistry.Instance.RegisterEntity(entity);

        bool found = IdRegistry.Instance.TryGetEntity(entity.PersistentId, out BaseEntity? retrieved);

        Assert.True(found);
        Assert.Same(entity, retrieved);
    }

    [Fact]
    public void RegisterEntity_WithAssignedRuntimeId_CanBeRetrievedByGlobalKey()
    {
        TestEntity entity = CreateTestEntity();
        // Simulate zone assignment
        entity.SetEntityId(1, 100);

        IdRegistry.Instance.RegisterEntity(entity);

        bool found = IdRegistry.Instance.TryGetEntity(entity.RuntimeId.GlobalKey, out BaseEntity? retrieved);

        Assert.True(found);
        Assert.Same(entity, retrieved);
    }

    [Fact]
    public void RegisterEntity_WithUnassignedRuntimeId_NotFoundByGlobalKey()
    {
        TestEntity entity = CreateTestEntity();
        // Entity not assigned to zone - RuntimeId.IsAssigned is false

        IdRegistry.Instance.RegisterEntity(entity);

        bool found = IdRegistry.Instance.TryGetEntity(entity.RuntimeId.GlobalKey, out _);

        Assert.False(found);
    }

    [Fact]
    public void UnregisterEntity_RemovesFromAllLookups()
    {
        TestEntity entity = CreateTestEntity();
        entity.SetEntityId(1, 100);

        IdRegistry.Instance.RegisterEntity(entity);
        IdRegistry.Instance.UnregisterEntity(entity.PersistentId);

        bool foundByPersistent = IdRegistry.Instance.TryGetEntity(entity.PersistentId, out _);
        bool foundByGlobalKey = IdRegistry.Instance.TryGetEntity(entity.RuntimeId.GlobalKey, out _);

        Assert.False(foundByPersistent);
        Assert.False(foundByGlobalKey);
    }

    [Fact]
    public void RegisterConnection_CreatesMapping()
    {
        TestEntity entity = CreateTestEntity();
        var connectionId = Guid.NewGuid();

        IdRegistry.Instance.RegisterEntity(entity);
        IdRegistry.Instance.RegisterConnection(connectionId, entity.PersistentId);

        bool foundEntity = IdRegistry.Instance.TryGetEntityByConnection(connectionId, out BaseEntity? retrieved);
        bool foundConnection =
            IdRegistry.Instance.TryGetConnectionByEntity(entity.PersistentId, out Guid retrievedConnId);

        Assert.True(foundEntity);
        Assert.Same(entity, retrieved);
        Assert.True(foundConnection);
        Assert.Equal(connectionId, retrievedConnId);
    }

    [Fact]
    public void UnregisterConnection_RemovesBothMappings()
    {
        TestEntity entity = CreateTestEntity();
        var connectionId = Guid.NewGuid();

        IdRegistry.Instance.RegisterEntity(entity);
        IdRegistry.Instance.RegisterConnection(connectionId, entity.PersistentId);
        IdRegistry.Instance.UnregisterConnection(connectionId);

        bool foundEntity = IdRegistry.Instance.TryGetEntityByConnection(connectionId, out _);
        bool foundConnection = IdRegistry.Instance.TryGetConnectionByEntity(entity.PersistentId, out _);

        Assert.False(foundEntity);
        Assert.False(foundConnection);
    }

    [Fact]
    public void UnregisterEntity_AlsoRemovesConnectionMapping()
    {
        TestEntity entity = CreateTestEntity();
        var connectionId = Guid.NewGuid();

        IdRegistry.Instance.RegisterEntity(entity);
        IdRegistry.Instance.RegisterConnection(connectionId, entity.PersistentId);
        IdRegistry.Instance.UnregisterEntity(entity.PersistentId);

        bool foundEntity = IdRegistry.Instance.TryGetEntityByConnection(connectionId, out _);
        bool foundConnection = IdRegistry.Instance.TryGetConnectionByEntity(entity.PersistentId, out _);

        Assert.False(foundEntity);
        Assert.False(foundConnection);
    }

    [Fact]
    public void UpdateEntityGlobalKey_UpdatesLookup()
    {
        TestEntity entity = CreateTestEntity();
        entity.SetEntityId(1, 100);
        long oldGlobalKey = entity.RuntimeId.GlobalKey;

        IdRegistry.Instance.RegisterEntity(entity);

        // Simulate zone transfer
        entity.SetEntityId(2, 200);
        IdRegistry.Instance.UpdateEntityGlobalKey(entity, oldGlobalKey);

        bool foundOld = IdRegistry.Instance.TryGetEntity(oldGlobalKey, out _);
        bool foundNew = IdRegistry.Instance.TryGetEntity(entity.RuntimeId.GlobalKey, out BaseEntity? retrieved);

        Assert.False(foundOld);
        Assert.True(foundNew);
        Assert.Same(entity, retrieved);
    }

    [Fact]
    public void HasEntity_ReturnsTrueForRegisteredEntity()
    {
        TestEntity entity = CreateTestEntity();

        IdRegistry.Instance.RegisterEntity(entity);

        Assert.True(IdRegistry.Instance.HasEntity(entity.PersistentId));
    }

    [Fact]
    public void HasEntity_ReturnsFalseForUnregisteredEntity() =>
        Assert.False(IdRegistry.Instance.HasEntity(Guid.NewGuid()));

    [Fact]
    public void HasConnection_ReturnsTrueForRegisteredConnection()
    {
        var connectionId = Guid.NewGuid();

        IdRegistry.Instance.RegisterConnection(connectionId, Guid.NewGuid());

        Assert.True(IdRegistry.Instance.HasConnection(connectionId));
    }

    [Fact]
    public void HasConnection_ReturnsFalseForUnregisteredConnection() =>
        Assert.False(IdRegistry.Instance.HasConnection(Guid.NewGuid()));

    [Fact]
    public void EntityCount_ReflectsRegisteredEntities()
    {
        Assert.Equal(0, IdRegistry.Instance.EntityCount);

        TestEntity entity1 = CreateTestEntity();
        TestEntity entity2 = CreateTestEntity();

        IdRegistry.Instance.RegisterEntity(entity1);
        Assert.Equal(1, IdRegistry.Instance.EntityCount);

        IdRegistry.Instance.RegisterEntity(entity2);
        Assert.Equal(2, IdRegistry.Instance.EntityCount);

        IdRegistry.Instance.UnregisterEntity(entity1.PersistentId);
        Assert.Equal(1, IdRegistry.Instance.EntityCount);
    }

    [Fact]
    public void ConnectionCount_ReflectsRegisteredConnections()
    {
        Assert.Equal(0, IdRegistry.Instance.ConnectionCount);

        var conn1 = Guid.NewGuid();
        var conn2 = Guid.NewGuid();

        IdRegistry.Instance.RegisterConnection(conn1, Guid.NewGuid());
        Assert.Equal(1, IdRegistry.Instance.ConnectionCount);

        IdRegistry.Instance.RegisterConnection(conn2, Guid.NewGuid());
        Assert.Equal(2, IdRegistry.Instance.ConnectionCount);

        IdRegistry.Instance.UnregisterConnection(conn1);
        Assert.Equal(1, IdRegistry.Instance.ConnectionCount);
    }

    [Fact]
    public void GetAllEntities_ReturnsAllRegistered()
    {
        TestEntity entity1 = CreateTestEntity();
        TestEntity entity2 = CreateTestEntity();

        IdRegistry.Instance.RegisterEntity(entity1);
        IdRegistry.Instance.RegisterEntity(entity2);

        var all = IdRegistry.Instance.GetAllEntities().ToList();

        Assert.Equal(2, all.Count);
        Assert.Contains(entity1, all);
        Assert.Contains(entity2, all);
    }

    [Fact]
    public void Clear_RemovesEverything()
    {
        TestEntity entity = CreateTestEntity();
        var connectionId = Guid.NewGuid();

        IdRegistry.Instance.RegisterEntity(entity);
        IdRegistry.Instance.RegisterConnection(connectionId, entity.PersistentId);
        IdRegistry.Instance.GetNextLocalId(1);

        IdRegistry.Instance.Clear();

        Assert.Equal(0, IdRegistry.Instance.EntityCount);
        Assert.Equal(0, IdRegistry.Instance.ConnectionCount);
        // Counter should be reset (starts at 0)
        Assert.Equal(0, IdRegistry.Instance.GetNextLocalId(1));
    }

    private static TestEntity CreateTestEntity() =>
        new(EntityIdentity.Unassigned(0), Guid.NewGuid(), new Position(0, 0));

    /// <summary>
    ///     Simple test entity implementation.
    /// </summary>
    private class TestEntity : BaseEntity, IMutableRuntimeEntity
    {
        /// <summary>Test server ID.</summary>
        private const byte _testServerId = 1;

        /// <summary>Test shard ID (not used in prototype).</summary>
        private const ushort _testShardId = 0;

        /// <summary>Test prefab ID for player entity (corresponds to PrefabIds.PlayerDefault fallback value).</summary>
        private const ushort _testPrefabId = 1;

        public TestEntity(EntityIdentity runtimeId, Guid persistentId, Position position)
            : base(runtimeId, persistentId, position)
        {
            // Hide base RuntimeId with own settable version
            RuntimeId = runtimeId;
        }

        /// <summary>
        ///     Hides base RuntimeId to allow modification after construction.
        ///     Implements IMutableRuntimeEntity for polymorphic access.
        /// </summary>
        public new EntityIdentity RuntimeId { get; private set; }

        public override bool IsTrulyPersistent => false;
        public override EntityType Type => EntityType.Player;

        /// <summary>
        ///     Explicit interface implementation to ensure IdRegistry uses this property.
        /// </summary>
        EntityIdentity IMutableRuntimeEntity.RuntimeId => RuntimeId;

        public override void SetEntityId(ushort localId, ushort zoneId) =>
            RuntimeId = new EntityIdentity(_testServerId, zoneId, _testShardId, localId, _testPrefabId);

        public override void ChangeZone(ushort newZoneId)
        {
        }
    }
}
