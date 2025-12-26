namespace Mmo.Shared.Character.Enums;

/// <summary>
///     Represents the character class selection for players.
///     Todo: not really implemented
/// </summary>
public enum CharacterClass : byte
{
    /// <summary>
    ///     Melee tank specializing in physical combat and defense.
    /// </summary>
    Warrior = 0,

    /// <summary>
    ///     Ranged spellcaster specializing in magical damage.
    /// </summary>
    Mage = 1,

    /// <summary>
    ///     Agile melee class specializing in stealth and critical strikes.
    /// </summary>
    Rogue = 2,

    /// <summary>
    ///     Support class specializing in healing and buffs.
    /// </summary>
    Priest = 3
    // ... expand as needed
}
