namespace Mmo.Shared.Enums;

/// <summary>
///     Represents the function/interaction type of an NPC.
///     Defines what the player can do with this NPC. Flags can be combined.
/// </summary>
[Flags]
public enum NpcFunction : ushort
{
    /// <summary>
    ///     No function assigned.
    /// </summary>
    None = 0,

    // Combat Behavior
    /// <summary>
    ///     NPC is hostile and will attack players on sight.
    /// </summary>
    Hostile = 1 << 0,

    /// <summary>
    ///     NPC is neutral and only attacks when provoked.
    /// </summary>
    Neutral = 1 << 1,

    /// <summary>
    ///     NPC is friendly and cannot be attacked.
    /// </summary>
    Friendly = 1 << 2,

    // Services (combinable!)
    /// <summary>
    ///     NPC sells items to players.
    /// </summary>
    Vendor = 1 << 3,

    /// <summary>
    ///     NPC provides and completes quests.
    /// </summary>
    QuestGiver = 1 << 4,

    /// <summary>
    ///     NPC teaches skills and abilities.
    /// </summary>
    Trainer = 1 << 5,

    /// <summary>
    ///     NPC manages flight paths.
    /// </summary>
    FlightMaster = 1 << 6,

    /// <summary>
    ///     NPC allows players to bind their hearthstone.
    /// </summary>
    Innkeeper = 1 << 7,

    /// <summary>
    ///     NPC provides access to bank storage.
    /// </summary>
    Banker = 1 << 8,

    /// <summary>
    ///     NPC manages the auction house.
    /// </summary>
    Auctioneer = 1 << 9,

    /// <summary>
    ///     NPC repairs equipment.
    /// </summary>
    Repairer = 1 << 10,

    /// <summary>
    ///     NPC provides resurrection services (respawn point).
    /// </summary>
    SpiritHealer = 1 << 11
}
