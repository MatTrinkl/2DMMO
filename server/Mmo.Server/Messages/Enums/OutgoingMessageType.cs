namespace Mmo.Server.Messages.Enums;

/// <summary>
///     Type of outgoing message.
///     Determines how the GameServer processes the message in the output phase.
/// </summary>
public enum OutgoingMessageType : byte
{
    /// <summary>To a single client.</summary>
    ToClient = 0,

    /// <summary>To all players in a zone.</summary>
    BroadcastToZone = 1,

    /// <summary>To all players in a zone, except one.</summary>
    BroadcastToZoneExcept = 2,

    /// <summary>To all players within range.</summary>
    BroadcastToNearby = 3,

    /// <summary>To all party members.</summary>
    BroadcastToParty = 4,

    /// <summary>To all party members, except one.</summary>
    BroadcastToPartyExcept = 5,

    /// <summary>To all guild members.</summary>
    BroadcastToGuild = 6,

    /// <summary>To all guild members, except one.</summary>
    BroadcastToGuildExcept = 7,

    /// <summary>To ALL players on the server.</summary>
    BroadcastToAll = 8,

    /// <summary>
    ///     To All players, except one.
    /// </summary>
    BroadcastToAllExcept = 9
}
