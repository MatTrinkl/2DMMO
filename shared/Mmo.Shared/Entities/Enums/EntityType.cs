namespace Mmo.Shared.Entities.Enums;

/// <summary>
///     Represents the type of entity in the game world.
/// </summary>
public enum EntityType : byte
{
    /// <summary>
    ///     No entity type.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Player-controlled character.
    /// </summary>
    Player = 1,

    /// <summary>
    ///     Non-player character (NPC).
    /// </summary>
    Npc = 2,

    /// <summary>
    ///     Interactive object (chests, doors, etc.). Planned for future phases.
    /// </summary>
    Object = 3,

    /// <summary>
    ///     Projectile entity (fireballs, arrows, etc.). Planned for future phases.
    /// </summary>
    Projectile = 4,

    /// <summary>
    ///     Loot container entity. Planned for future phases.
    /// </summary>
    Loot = 5
}
