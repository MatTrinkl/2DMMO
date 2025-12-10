using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     The parent class of all entities for now. Todo: Split this into Prefab and Living.
/// </summary>
[MessagePackObject]
[Union(0, typeof(PlayerEntity))]
public abstract class Entity : IEntity
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public Entity()
    {
    }

    /// <summary>
    ///     Sets the parameters which are shared over all children.
    ///     Can only be called by children.
    /// </summary>
    /// <param name="entityId">The id of this entity.</param>
    /// <param name="persistentId">The ID that never chances.</param>
    /// <param name="position">The current position of this entity.</param>
    protected Entity(EntityIdentity entityId, Guid? persistentId, Position position)
    {
        EntityId = entityId;
        PersistentId = persistentId;
        Position = position;
    }

    /// <summary>
    ///     Whether this entity has a persistent identity.
    /// </summary>
    [IgnoreMember]
    public bool IsPersistent => PersistentId.HasValue;

    /// <summary>
    ///     This identifies the entity everywhere.
    /// </summary>
    [Key(0)]
    public EntityIdentity EntityId { get; protected init; }

    /// <summary>
    ///     Persistent identity - never changes.
    ///     • Players: CharacterId from database
    ///     • Static NPCs/Objects: From zone config
    ///     • Dynamic Mobs/Drops: null
    /// </summary>
    [Key(1)]
    public Guid? PersistentId { get; protected init; }

    /// <summary>
    ///     The current position of this entity in its current zone..
    /// </summary>
    [Key(2)]
    public Position Position { get; set; } = new(0, 0);

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
}
