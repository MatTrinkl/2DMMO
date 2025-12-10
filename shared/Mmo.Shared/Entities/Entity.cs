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
    }

    /// <summary>
    ///     Constructor for persistent entities (Players, NPCs from config).
    /// </summary>
    protected Entity(Guid persistentId, Position position, bool isTrulyPersistent = true)
    {
        PersistentId = persistentId;
        Position = position;
        IsTrulyPersistent = isTrulyPersistent;
    }

    /// <summary>
    ///     Constructor for runtime entities (Spawned Mobs, Projectiles).
    ///     Generates a new PersistentId automatically.
    /// </summary>
    protected Entity(Position position)
    {
        PersistentId = Guid.NewGuid();
        Position = position;
        IsTrulyPersistent = false;
    }

    /// <summary>
    ///     Runtime identity - changes on zone transfer.
    /// </summary>
    [Key(0)]
    public EntityIdentity EntityId { get; protected set; }

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
    ///     Sets the runtime EntityId. Called by Zone when entity is added.
    ///     Handles struct copy correctly.
    /// </summary>
    public void SetEntityId(int id, ushort zoneId)
    {
        EntityIdentity identity = EntityId;
        identity.ZoneTransfer(id, zoneId);
        EntityId = identity;
    }

    public abstract void ChangeZone(ushort newZoneId);
}
