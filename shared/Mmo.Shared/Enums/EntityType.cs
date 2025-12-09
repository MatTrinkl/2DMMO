namespace Mmo.Shared.Enums;

/// <summary>
///     Basic category of an entity.
///     Values should not be changed after release (network/protocol).
///     Underlying type: byte for lower bandwidth/memory consumption.
/// </summary>
public enum EntityType : byte
{
    /// <summary>
    ///     Fallback.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     Player or Character.
    /// </summary>
    Player = 1,

    /// <summary>
    ///     generic NPC (QuestGiver/Vendor etc. are <see cref="EntityRole" />.
    /// </summary>
    Npc = 2,

    /// <summary>
    ///     Mob is neutral, only aggressive when attacked.
    /// </summary>
    Mob = 10,

    /// <summary>
    ///     Monster is always aggressive
    /// </summary>
    Monster = 11,

    /// <summary>
    ///     Boss of a dungeon or raid or world boss.
    /// </summary>
    Boss = 12,

    /// <summary>
    ///     An item is collectable and will despawn.
    /// </summary>
    Item = 20,

    /// <summary>
    ///     A resource node is interactable and consumed.
    /// </summary>
    ResourceNode = 21,

    /// <summary>
    ///     Represents a door or portal in the world.
    /// </summary>
    Door = 50,

    /// <summary>
    ///     Represents an object which the player can interact. (e.g. a sign with a quest).
    /// </summary>
    InteractiveObject = 51,

    /// <summary>
    ///     A decorative entity not part of the solid map.
    /// </summary>
    Decorative = 52,

    /// <summary>
    ///     A moving bullet or arrow.
    /// </summary>
    Projectile = 80,

    /// <summary>
    ///     A companion of the player.
    /// </summary>
    Pet = 81,

    /// <summary>
    ///     Spawnpointsystem is WIP.
    /// </summary>
    SpawnPointPlayer = 100,

    /// <summary>
    ///     Spawnpointsystem is WIP.
    /// </summary>
    SpawnPointMob = 101,

    /// <summary>
    ///     Spawnpointsystem is WIP.
    /// </summary>
    SpawnPointMonster = 102,

    /// <summary>
    ///     Spawnpointsystem is WIP.
    /// </summary>
    SpawnPointTreasure = 103

    // Reserved (200..255) for later
}
