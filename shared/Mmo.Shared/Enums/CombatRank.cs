namespace Mmo.Shared.Enums;

/// <summary>
///     Represents the combat strength of an entity (affects HP, damage, XP, loot).
/// </summary>
public enum CombatRank : byte
{
    /// <summary>
    ///     Trivial enemy (no XP, no threat - critters, grey mobs).
    /// </summary>
    Trivial = 0,

    /// <summary>
    ///     Normal enemy (standard difficulty).
    /// </summary>
    Normal = 1,

    /// <summary>
    ///     Elite enemy (~3x HP/Damage).
    /// </summary>
    Elite = 2,

    /// <summary>
    ///     Rare enemy (uncommon spawn, better loot).
    /// </summary>
    Rare = 3,

    /// <summary>
    ///     Rare elite enemy (rare + elite bonuses).
    /// </summary>
    RareElite = 4,

    /// <summary>
    ///     Dungeon boss.
    /// </summary>
    Boss = 5,

    /// <summary>
    ///     World/raid boss.
    /// </summary>
    WorldBoss = 6
}
