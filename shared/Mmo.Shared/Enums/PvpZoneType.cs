namespace Mmo.Shared.Enums;


/// <summary>
///     PvP-Regeln einer Zone.
/// </summary>
public enum PvpZoneType : byte
{
    /// <summary>
    ///     Sicheres Gebiet - kein PvP möglich (Städte, Startgebiete).
    /// </summary>
    Sanctuary = 0,

    /// <summary>
    ///     PvP nur wenn beide Spieler geflaggt sind.
    /// </summary>
    Normal = 1,

    /// <summary>
    ///     Freies PvP - jeder kann jeden angreifen.
    /// </summary>
    FreeForAll = 2,

    /// <summary>
    ///     Fraktions-PvP - automatisch geflaggt gegen andere Fraktion.
    /// </summary>
    FactionWarfare = 3,

    /// <summary>
    ///     Arena/Battleground - organisiertes PvP.
    /// </summary>
    Arena = 4,

    /// <summary>
    ///     Contested Zone - PvP-Flag wird automatisch aktiviert.
    /// </summary>
    Contested = 5,
}




