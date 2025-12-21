namespace Mmo.Shared.Zones.Enums;

/// <summary>
///     Represents the PvP rules of a zone.
/// </summary>
public enum PvpZoneType : byte
{
    /// <summary>
    ///     Safe area - no PvP possible (cities, starting zones).
    /// </summary>
    Sanctuary = 0,

    /// <summary>
    ///     PvP only when both players are flagged.
    /// </summary>
    Normal = 1,

    /// <summary>
    ///     Free-for-all PvP - anyone can attack anyone.
    /// </summary>
    FreeForAll = 2,

    /// <summary>
    ///     Faction PvP - automatically flagged against opposing faction.
    /// </summary>
    FactionWarfare = 3,

    /// <summary>
    ///     Arena/Battleground - organized PvP.
    /// </summary>
    Arena = 4,

    /// <summary>
    ///     Contested zone - PvP flag is automatically activated.
    /// </summary>
    Contested = 5
}
