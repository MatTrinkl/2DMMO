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
    ///     Stable identity for network communication.
    ///     Never changes, even on zone transfer.
    ///     
    ///     • Players: CharacterId from database
    ///     • Static NPCs: From zone config
    ///     • Spawned Mobs: Generated at spawn time
    ///     • Projectiles: Generated at creation
    /// </summary>
    Guid PersistentId { get; }

    /// <summary>
    ///     Whether this entity is truly persistent (saved to DB)
    ///     or just runtime-persistent (exists only this session).
    /// </summary>
    bool IsTrulyPersistent { get; }

    /// <summary>
    ///     This method will be called when this entity changes zones.
    /// </summary>
    /// <param name="newZoneId">The ID of the new zone.</param>
    void ChangeZone(ushort newZoneId);
}
