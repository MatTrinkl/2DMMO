namespace Mmo.Shared.Enums;

/// <summary>
///     Represents the type of trainer NPCs.
/// </summary>
public enum TrainerType : byte
{
    /// <summary>
    ///     Class trainer (teaches class-specific skills).
    /// </summary>
    Class = 0,

    /// <summary>
    ///     Profession trainer (teaches crafting/gathering skills).
    /// </summary>
    Profession = 1,

    /// <summary>
    ///     Weapon trainer (teaches weapon proficiencies).
    /// </summary>
    Weapon = 2,

    /// <summary>
    ///     Pet trainer (teaches pet abilities).
    /// </summary>
    Pet = 3,

    /// <summary>
    ///     Mount trainer (teaches riding skills and mounts).
    /// </summary>
    Mount = 4
}
