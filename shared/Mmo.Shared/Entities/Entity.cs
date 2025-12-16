// shared/Mmo. Shared/Entities/Entity. cs

using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

[MessagePackObject]
[Union(0, typeof(PlayerEntity))]
// [Union(1, typeof(MobEntity))]  // Später hinzufügen
public abstract class Entity : IEntity
{
    [SerializationConstructor]
    protected Entity()
    {
        // RuntimeId will be set by MessagePack during deserialization
        // PrefabId should be part of the serialized RuntimeId
    }

    /// <summary>
    ///     Constructor for persistent entities (Players, NPCs from config).
    /// </summary>
    protected Entity(Guid persistentId, Position position, ushort prefabId, bool isTrulyPersistent = true)
    {
        PersistentId = persistentId;
        Position = position;
        IsTrulyPersistent = isTrulyPersistent;
        RuntimeId = EntityIdentity.Unassigned(prefabId);
    }

    /// <summary>
    ///     Constructor for runtime entities (Spawned Mobs, Projectiles).
    ///     Generates a new PersistentId through IdRegistry.
    /// </summary>
    protected Entity(Position position, ushort prefabId)
    {
        PersistentId = IdRegistry.Instance.GeneratePersistentId();
        Position = position;
        IsTrulyPersistent = false;
        RuntimeId = EntityIdentity.Unassigned(prefabId);
    }

    /// <summary>
    ///     Convenience property for accessing the PrefabId.
    /// </summary>
    [IgnoreMember]
    public ushort PrefabId => RuntimeId.PrefabId;

    /// <summary>
    ///     Convenience property for accessing the ZoneId.
    /// </summary>
    [IgnoreMember]
    public ushort ZoneId => RuntimeId.ZoneId;

    /// <summary>
    ///     Checks if this entity is currently assigned to a zone.
    /// </summary>
    [IgnoreMember]
    public bool IsInZone => RuntimeId.IsAssigned;

    /// <summary>
    ///     Runtime identity - changes on zone transfer.
    /// </summary>
    [Key(0)]
    public EntityIdentity RuntimeId { get; set; }

    /// <summary>
    ///     Stable identity - never changes.
    /// </summary>
    [Key(1)]
    public Guid PersistentId { get; protected init; }

    /// <summary>
    ///     The current position of this entity.
    /// </summary>
    [Key(2)]
    public Position Position { get; set; } = new(0, 0);

    /// <summary>
    ///     True if this entity is saved to database.
    /// </summary>
    [Key(3)]
    public bool IsTrulyPersistent { get; protected init; }

    [IgnoreMember] public abstract EntityType Type { get; }

    [IgnoreMember] public abstract EntityRole Role { get; }

    /// <summary>
    ///     Sets the runtime identity. Called by Zone when entity is added.
    ///     Handles struct copy correctly.
    /// </summary>
    public void SetEntityId(int localId, ushort zoneId)
    {
        EntityIdentity identity = RuntimeId;
        identity.ZoneTransfer(localId, zoneId);
        RuntimeId = identity;
    }

    public abstract void ChangeZone(ushort newZoneId);

    /// <summary>
    ///     Assigns this entity to a zone. Called internally by Zone.AddEntity().
    /// </summary>
    internal void AssignToZone(byte serverId, ushort zoneId, ushort shardId, int localId) =>
        RuntimeId = new EntityIdentity(serverId, zoneId, shardId, localId, RuntimeId.PrefabId);

    /// <summary>
    ///     Removes this entity from its current zone. Called internally by Zone.RemoveEntity().
    /// </summary>
    internal void RemoveFromZone() => RuntimeId = EntityIdentity.Unassigned(RuntimeId.PrefabId);
}
