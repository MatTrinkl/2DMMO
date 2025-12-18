namespace Mmo.Shared.Enums;

/// <summary>
///     Spieler-Rolle in Gruppen (Tank, Healer, DPS).
///     NUR für Spieler relevant!
/// </summary>
public enum PlayerRole : byte
{
    None = 0,
    Tank = 1,
    Healer = 2,
    MeleeDps = 3,
    RangedDps = 4,
}
