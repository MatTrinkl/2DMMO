using Mmo.Shared.Zones.Messages.Server_Client;

namespace Mmo.Shared.Zones.Enums;

/// <summary>
/// This Error Code is send in a <see cref="ZoneTransferResponse"/>
/// </summary>
public enum ZoneTransferResponseErrorCode : byte
{
    /// <summary>
    /// Requested Zone with the given ZoneID doesn't exist.
    /// </summary>
    ZoneNotFound,

    /// <summary>
    /// Requested Zone is locked.
    /// </summary>
    ZoneLocked,

    /// <summary>
    /// Requested Zone requires a specific quest completed to enter.
    /// </summary>
    QuestRequired,

    /// <summary>
    /// Requested player is still in combat and cannot enter the new Zone.
    /// </summary>
    InCombat,

    /// <summary>
    /// There is an active cooldown preventing the player to enter the zone. (e.g. Hearthstone)
    /// </summary>
    CooldownActive,

    /// <summary>
    /// The maximum Player count is reached (should not happen with Sharding).
    /// </summary>
    InstanceFull,

    /// <summary>
    /// This zone requires a party.
    /// </summary>
    NotInParty,

    /// <summary>
    /// This zone requires a specific faction.
    /// </summary>
    WrongFaction,

    /// <summary>
    /// The level of the player is too high. (Only for starting zones)
    /// </summary>
    LevelTooHigh,

    /// <summary>
    /// The level of the player is too low.
    /// </summary>
    LevelTooLow,

    /// <summary>
    /// The target position is invalid.
    /// </summary>
    InvalidTargetPosition
}
