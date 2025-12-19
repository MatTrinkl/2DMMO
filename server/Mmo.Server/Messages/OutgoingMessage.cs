using Mmo.Server.Networking;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Server.Messages;

/// <summary>
///     Represents an outgoing message that will be sent in the output phase.
///     Created by MessageContext and processed by GameServer in the output phase.
/// </summary>
public readonly struct OutgoingMessage
{
    /// <summary>Type of message (ToClient, Broadcast, etc.).</summary>
    public OutgoingMessageType Type { get; init; }

    /// <summary>The message to be sent.</summary>
    public INetworkMessage Message { get; init; }

    // ═══ For ToClient ═══

    /// <summary>Target connection (only for ToClient).</summary>
    public ClientConnection? TargetConnection { get; init; }

    // ═══ For Broadcasts ═══

    /// <summary>Target zone (for zone broadcasts).</summary>
    public ushort? ZoneId { get; init; }

    /// <summary>Target party (for party broadcasts).</summary>
    public Guid? PartyId { get; init; }

    /// <summary>Target guild (for guild broadcasts).</summary>
    public Guid? GuildId { get; init; }

    /// <summary>Connection to exclude from broadcast.</summary>
    public Guid? ExcludeConnectionId { get; init; }

    // ═══ For BroadcastToNearby ═══

    /// <summary>Origin position (for radius broadcasts).</summary>
    public Position? Origin { get; init; }

    /// <summary>Radius (for radius broadcasts).</summary>
    public float? Radius { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Creates a message for a single client.
    /// </summary>
    public static OutgoingMessage ToClient(ClientConnection connection, INetworkMessage message)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.ToClient,
            Message = message,
            TargetConnection = connection
        };
    }

    /// <summary>
    ///     Creates a broadcast for all players in a zone.
    /// </summary>
    public static OutgoingMessage BroadcastToZone(INetworkMessage message, ushort zoneId)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.BroadcastToZone,
            Message = message,
            ZoneId = zoneId
        };
    }

    /// <summary>
    ///     Creates a broadcast for all players in a zone, except one.
    /// </summary>
    public static OutgoingMessage BroadcastToZoneExcept(
        INetworkMessage message,
        ushort zoneId,
        Guid excludeConnectionId)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.BroadcastToZoneExcept,
            Message = message,
            ZoneId = zoneId,
            ExcludeConnectionId = excludeConnectionId
        };
    }

    /// <summary>
    ///     Creates a broadcast for all players within range.
    /// </summary>
    public static OutgoingMessage BroadcastToNearby(
        INetworkMessage message,
        ushort zoneId,
        Position origin,
        float radius,
        Guid? excludeConnectionId = null)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.BroadcastToNearby,
            Message = message,
            ZoneId = zoneId,
            Origin = origin,
            Radius = radius,
            ExcludeConnectionId = excludeConnectionId
        };
    }

    /// <summary>
    ///     Creates a broadcast for all party members.
    /// </summary>
    public static OutgoingMessage BroadcastToParty(INetworkMessage message, Guid partyId)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.BroadcastToParty,
            Message = message,
            PartyId = partyId
        };
    }

    /// <summary>
    ///     Creates a broadcast for all party members, except one.
    /// </summary>
    public static OutgoingMessage BroadcastToPartyExcept(
        INetworkMessage message,
        Guid partyId,
        Guid excludeConnectionId)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.BroadcastToPartyExcept,
            Message = message,
            PartyId = partyId,
            ExcludeConnectionId = excludeConnectionId
        };
    }

    /// <summary>
    ///     Creates a broadcast for all guild members.
    /// </summary>
    public static OutgoingMessage BroadcastToGuild(INetworkMessage message, Guid guildId)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.BroadcastToGuild,
            Message = message,
            GuildId = guildId
        };
    }

    /// <summary>
    ///     Creates a broadcast for all guild members, except one.
    /// </summary>
    public static OutgoingMessage BroadcastToGuildExcept(
        INetworkMessage message,
        Guid guildId,
        Guid excludeConnectionId)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.BroadcastToGuildExcept,
            Message = message,
            GuildId = guildId,
            ExcludeConnectionId = excludeConnectionId
        };
    }

    /// <summary>
    ///     Creates a broadcast for ALL players on the server.
    /// </summary>
    public static OutgoingMessage BroadcastToAll(INetworkMessage message)
    {
        return new OutgoingMessage
        {
            Type = OutgoingMessageType.BroadcastToAll,
            Message = message
        };
    }
}

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
    BroadcastToAll = 8
}
