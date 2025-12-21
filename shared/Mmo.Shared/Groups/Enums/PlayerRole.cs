namespace Mmo.Shared.Groups.Enums;

/// <summary>
///     Represents the player's role in groups (tank, healer, DPS).
///     Only relevant for players!
/// </summary>
public enum PlayerRole : byte
{
    /// <summary>
    ///     No role assigned.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Tank role (absorbs damage, protects group).
    /// </summary>
    Tank = 1,

    /// <summary>
    ///     Healer role (restores health to group members).
    /// </summary>
    Healer = 2,

    /// <summary>
    ///     Melee DPS role (close-range damage dealer).
    /// </summary>
    MeleeDps = 3,

    /// <summary>
    ///     Ranged DPS role (long-range damage dealer).
    /// </summary>
    RangedDps = 4
}
