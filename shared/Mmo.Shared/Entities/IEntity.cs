using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     This interface represents an entity in a zone in the world. This can be a player, a creature or an interactable
///     object etc.
/// </summary>
public interface IEntity
{
    /// <summary>
    ///     This identity the entity in the game, world, server, zone and later in a shard.
    /// </summary>
    EntityIdentity EntityId { get; }

    /// <summary>
    ///     Defines the type of the entity e.g. Player, Mob...
    /// </summary>
    EntityType Type { get; }

    /// <summary>
    ///     Defines the roles of the entity.
    /// </summary>
    EntityRole Role { get; }

    /// <summary>
    ///     The position of the Entity in the current zone.
    /// </summary>
    Position Position { get; set; }

    /// <summary>
    ///     Persistent identity - NEVER changes.
    ///     Null for temporary/dynamic entities (spawned mobs, projectiles, drops).
    /// </summary>
    Guid? PersistentId { get; }

    /// <summary>
    ///     This method will be called when this entity changes zones.
    /// </summary>
    /// <param name="newZoneId">The ID of the new zone.</param>
    void ChangeZone(ushort newZoneId);
}
