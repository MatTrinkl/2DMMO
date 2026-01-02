namespace Mmo.Shared.Zones.Enums;

/// <summary>
///     Represents general zone properties (flags can be combined).
/// </summary>
[Flags]
public enum ZoneFlags : ushort
{
    /// <summary>
    ///     No special flags set.
    /// </summary>
    None = 0,

    // PvP
    /// <summary>
    ///     PvP is generally allowed in this zone.
    /// </summary>
    PvpEnabled = 1 << 0,

    /// <summary>
    ///     Automatically flag players for PvP when entering.
    /// </summary>
    AutoFlagPvp = 1 << 1,

    // Restrictions
    /// <summary>
    ///     Mounting is not allowed in this zone.
    /// </summary>
    NoMounting = 1 << 2,

    /// <summary>
    ///     Flying is not allowed in this zone.
    /// </summary>
    NoFlying = 1 << 3,

    /// <summary>
    ///     No combat possible (including PvE).
    /// </summary>
    NoCombat = 1 << 4,

    /// <summary>
    ///     No spell casting allowed.
    /// </summary>
    NoSpellCast = 1 << 5,

    /// <summary>
    ///     No summoning or teleporting to this zone.
    /// </summary>
    NoSummon = 1 << 6,

    // Special
    /// <summary>
    ///     This is a capital city.
    /// </summary>
    IsCapital = 1 << 7,

    /// <summary>
    ///     This is an instanced zone.
    /// </summary>
    IsInstance = 1 << 8,

    /// <summary>
    ///     This is a raid instance.
    /// </summary>
    IsRaid = 1 << 9,

    /// <summary>
    ///     This is a battleground.
    /// </summary>
    IsBattleground = 1 << 10,

    /// <summary>
    ///     This is an arena.
    /// </summary>
    IsArena = 1 << 11,

    // Environment
    /// <summary>
    ///     This is an indoor area.
    /// </summary>
    IsIndoor = 1 << 12,

    /// <summary>
    ///     This is an underwater zone.
    /// </summary>
    IsUnderwater = 1 << 13,

    // Rest
    /// <summary>
    ///     Rest XP is gained in this zone.
    /// </summary>
    HasRestXp = 1 << 14,

    /// <summary>
    ///     This is a ghost zone.
    /// </summary>
    IsGhostZone = 1 << 15
}
