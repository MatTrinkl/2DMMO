using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     The parent class of all entities for now. Todo: Split this into Prefab and Living.
/// </summary>
[MessagePackObject]
[Union(0, typeof(PlayerEntity))]
[Union(1, typeof(MobEntity))]
public abstract class Entity : IEntity
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public Entity()
    {
    }

    /// <summary>
    ///     Constructor for persistent entities (Players, static NPCs).
    /// </summary>
    /// <param name="persistentId">The ID that never changes.</param>
    /// <param name="position">The current position of this entity.</param>
    /// <param name="isTrulyPersistent">Whether this entity is saved to DB.</param>
    protected Entity(Guid persistentId, Position position, bool isTrulyPersistent = true)
    {
        PersistentId = persistentId;
        Position = position;
        IsTrulyPersistent = isTrulyPersistent;
    }

    /// <summary>
    ///     Constructor for runtime entities (spawned mobs, projectiles).
    ///     Automatically generates a new PersistentId.
    /// </summary>
    /// <param name="position">The current position of this entity.</param>
    protected Entity(Position position)
    {
        PersistentId = Guid.NewGuid();
        Position = position;
        IsTrulyPersistent = false;
    }

    /// <summary>
    ///     This identifies the entity everywhere.
    /// </summary>
    [Key(0)]
    public EntityIdentity EntityId { get; protected set; }

    /// <summary>
    ///     Persistent identity - never changes.
    ///     • Players: CharacterId from database
    ///     • Static NPCs/Objects: From zone config
    ///     • Spawned Mobs/Projectiles: Generated at creation
    /// </summary>
    [Key(1)]
    public Guid PersistentId { get; protected init; }

    /// <summary>
    ///     The current position of this entity in its current zone..
    /// </summary>
    [Key(2)]
    public Position Position { get; set; } = new(0, 0);

    /// <summary>
    ///     Whether this entity is truly persistent (saved to DB)
    ///     or just runtime-persistent (exists only this session).
    /// </summary>
    [Key(3)]
    public bool IsTrulyPersistent { get; protected init; }

    /// <summary>
    ///     Type of the entity.
    /// </summary>
    [IgnoreMember]
    public abstract EntityType Type { get; }

    /// <summary>
    ///     Role of the entity.
    /// </summary>
    [IgnoreMember]
    public abstract EntityRole Role { get; }

    /// <summary>
    ///     This method will be called when this entity changes zone.
    /// </summary>
    /// <param name="newZoneId">ID of the zone.</param>
    public abstract void ChangeZone(ushort newZoneId);
    
    /// <summary>
    ///     Internal method to update the EntityId when zone changes.
    ///     Called by Zone.AddEntity() and ZoneManager.TransferEntity().
    /// </summary>
    /// <param name="newEntityId">New entity ID within the zone.</param>
    /// <param name="newZoneId">New zone ID.</param>
    internal void UpdateEntityId(int newEntityId, ushort newZoneId)
    {
        var currentId = EntityId;
        currentId.ZoneTransfer(newEntityId, newZoneId);
        EntityId = currentId;
    }
}
