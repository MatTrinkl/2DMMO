namespace Mmo.Shared.Creatures.Enums;

/// <summary>
///     Represents the creature type classification for NPCs and monsters.
/// </summary>
public enum CreatureType : byte
{
    /// <summary>
    ///     Humanoid creatures (humans, elves, orcs, etc.).
    /// </summary>
    Humanoid = 0,

    /// <summary>
    ///     Beast creatures (wolves, bears, cats, etc.).
    /// </summary>
    Beast = 1,

    /// <summary>
    ///     Undead creatures (skeletons, zombies, ghosts, etc.).
    /// </summary>
    Undead = 2,

    /// <summary>
    ///     Demon creatures from dark realms.
    /// </summary>
    Demon = 3,

    /// <summary>
    ///     Elemental creatures (fire, water, earth, air).
    /// </summary>
    Elemental = 4,

    /// <summary>
    ///     Giant creatures (trolls, ogres, etc.).
    /// </summary>
    Giant = 5,

    /// <summary>
    ///     Mechanical creatures (golems, constructs, etc.).
    /// </summary>
    Mechanical = 6,

    /// <summary>
    ///     Dragonkin creatures (dragons, drakes, wyverns).
    /// </summary>
    Dragonkin = 7,

    /// <summary>
    ///     Aberration creatures (otherworldly horrors).
    /// </summary>
    Aberration = 8,

    /// <summary>
    ///     Critter creatures (rabbits, rats, ambient wildlife).
    /// </summary>
    Critter = 9
}
