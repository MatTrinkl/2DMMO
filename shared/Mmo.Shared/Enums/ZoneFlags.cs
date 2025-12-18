namespace Mmo.Shared.Enums;

/// <summary>
///     Allgemeine Zone-Eigenschaften.
/// </summary>
[Flags]
public enum ZoneFlags : ushort
{
    None = 0,

    // PvP
    PvpEnabled = 1 << 0, // PvP grundsätzlich erlaubt
    AutoFlagPvp = 1 << 1, // Automatisch PvP-Flag beim Betreten

    // Restrictions
    NoMounting = 1 << 2, // Kein Reiten erlaubt
    NoFlying = 1 << 3, // Kein Fliegen erlaubt
    NoCombat = 1 << 4, // Kein Kampf möglich (auch nicht PvE)
    NoSpellCast = 1 << 5, // Keine Zauber
    NoSummon = 1 << 6, // Keine Beschwörung/Teleport hierher

    // Special
    IsCapital = 1 << 7, // Hauptstadt
    IsInstance = 1 << 8, // Instanzierte Zone
    IsRaid = 1 << 9, // Raid-Instanz
    IsBattleground = 1 << 10, // Battleground
    IsArena = 1 << 11, // Arena

    // Environment
    IsIndoor = 1 << 12, // Innenraum
    IsUnderwater = 1 << 13, // Unterwasser-Zone

    // Rest
    HasRestXp = 1 << 14 // Rest-XP in dieser Zone
}
