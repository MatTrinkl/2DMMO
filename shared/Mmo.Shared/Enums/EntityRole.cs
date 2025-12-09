namespace Mmo.Shared.Enums;

/// <summary>
///     Roles/attributes that can be set in addition to the main category (<see cref="EntityType" />).
///     Flag approach allows hybrid entities (e.g., Type=NPC + Role=Vendor).
///     Underlying type: ushort, to leave room for extensions.
/// </summary>
[Flags]
public enum EntityRole : ushort
{
    /// <summary>
    ///     Default, no extra roles.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Entity is a vendor.
    /// </summary>
    Vendor = 1 << 0,

    /// <summary>
    ///     Entity can give a quest.
    /// </summary>
    QuestGiver = 1 << 1,

    /// <summary>
    ///     Describes the behavior against a player.
    ///     Hostile means always attacking a player when possible.
    /// </summary>
    Hostile = 1 << 2,

    /// <summary>
    ///     Describes the behavior against a player.
    ///     Passive means only attacking a player when the player attacks.
    /// </summary>
    Passive = 1 << 3,

    /// <summary>
    ///     Describes the behavior against a player.
    ///     Friend means its never attacking a player.
    /// </summary>
    Friend = 1 << 4,

    /// <summary>
    ///     The entity is interactable.
    /// </summary>
    Interactable = 1 << 5,

    /// <summary>
    ///     The entity is a lootable.
    /// </summary>
    Lootable = 1 << 6,

    /// <summary>
    ///     Can be used when for e.g. a door is a spawn point.
    /// </summary>
    SpawnPoint = 1 << 7,

    /// <summary>
    ///     When the entity is a collectable.
    /// </summary>
    Collectible = 1 << 8,

    /// <summary>
    ///     When entity is a companion of a player.
    /// </summary>
    Companion = 1 << 9

    // Reserve-Bits for later
    // 1 << 10 ... 1 << 15 are free to use
}
