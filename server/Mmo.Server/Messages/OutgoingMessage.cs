using Mmo.Server.Networking;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Server.Messages;

/// <summary>
///     Repräsentiert eine ausgehende Nachricht die in der Output-Phase gesendet wird.
///     Wird vom MessageContext erstellt und vom GameServer in der Output-Phase verarbeitet.
/// </summary>
public readonly struct OutgoingMessage
{
    /// <summary>Art der Nachricht (ToClient, Broadcast, etc.).</summary>
    public OutgoingMessageType Type { get; init; }

    /// <summary>Die zu sendende Nachricht.</summary>
    public INetworkMessage Message { get; init; }

    // ═══ Für ToClient ═══

    /// <summary>Ziel-Connection (nur bei ToClient).</summary>
    public ClientConnection? TargetConnection { get; init; }

    // ═══ Für Broadcasts ═══

    /// <summary>Ziel-Zone (für Zone-Broadcasts).</summary>
    public ushort? ZoneId { get; init; }

    /// <summary>Ziel-Party (für Party-Broadcasts).</summary>
    public Guid? PartyId { get; init; }

    /// <summary>Ziel-Guild (für Guild-Broadcasts).</summary>
    public Guid? GuildId { get; init; }

    /// <summary>Connection die ausgeschlossen werden soll.</summary>
    public Guid? ExcludeConnectionId { get; init; }

    // ═══ Für BroadcastToNearby ═══

    /// <summary>Ursprungsposition (für Radius-Broadcasts).</summary>
    public Position? Origin { get; init; }

    /// <summary>Radius (für Radius-Broadcasts).</summary>
    public float? Radius { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Erstellt eine Nachricht für einen einzelnen Client.
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
    ///     Erstellt einen Broadcast für alle Spieler in einer Zone.
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
    ///     Erstellt einen Broadcast für alle Spieler in einer Zone, außer einem.
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
    ///     Erstellt einen Broadcast für alle Spieler in Reichweite.
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
    ///     Erstellt einen Broadcast für alle Party-Mitglieder.
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
    ///     Erstellt einen Broadcast für alle Party-Mitglieder, außer einem.
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
    ///     Erstellt einen Broadcast für alle Guild-Mitglieder.
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
    ///     Erstellt einen Broadcast für alle Guild-Mitglieder, außer einem.
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
    ///     Erstellt einen Broadcast für ALLE Spieler auf dem Server.
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
///     Typ der ausgehenden Nachricht.
///     Bestimmt wie der GameServer die Nachricht in der Output-Phase verarbeitet.
/// </summary>
public enum OutgoingMessageType : byte
{
    /// <summary>An einen einzelnen Client. </summary>
    ToClient = 0,

    /// <summary>An alle Spieler in einer Zone. </summary>
    BroadcastToZone = 1,

    /// <summary>An alle Spieler in einer Zone, außer einem.</summary>
    BroadcastToZoneExcept = 2,

    /// <summary>An alle Spieler in Reichweite.</summary>
    BroadcastToNearby = 3,

    /// <summary>An alle Party-Mitglieder.</summary>
    BroadcastToParty = 4,

    /// <summary>An alle Party-Mitglieder, außer einem. </summary>
    BroadcastToPartyExcept = 5,

    /// <summary>An alle Guild-Mitglieder.</summary>
    BroadcastToGuild = 6,

    /// <summary>An alle Guild-Mitglieder, außer einem.</summary>
    BroadcastToGuildExcept = 7,

    /// <summary>An ALLE Spieler auf dem Server. </summary>
    BroadcastToAll = 8
}
