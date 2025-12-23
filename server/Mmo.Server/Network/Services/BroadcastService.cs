using Mmo.Server.Connections;
using Mmo.Server.Core;
using Mmo.Server.Messages;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.PlayerService;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.System.Messages;

namespace Mmo.Server.Network.Services;

/// <summary>
///     Service for broadcasting messages to players in zones or specific connections.
///     Handles both targeted and area-based message distribution.
/// </summary>
public class BroadcastService(GameServer gameServer)
    : IBroadcastService
{
    public void BroadcastToZone<T>(ushort zoneId, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToZone(message, zoneId);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToZoneExcept<T>(ushort zoneId, Guid excludedClientId, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToZoneExcept(message, zoneId, excludedClientId);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastInRange<T>(ushort zoneId, Position center, float radius, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToNearby(message, zoneId, center, radius);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastInRangeExcept<T>(ushort zoneId, Position center, float radius, Guid excludedClientId,
        T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToNearby(message, zoneId, center, radius, excludedClientId);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void SendToPlayer<T>(ClientConnection client, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.ToClient(client, message);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void SendToPlayers<T>(IEnumerable<ClientConnection> clients, T message) where T : INetworkMessage
    {
        foreach (ClientConnection client in clients)
            SendToPlayer(client, message);
    }

    public void SendError(ClientConnection client, string code, string message, string? details = null,
        string? field = null)
    {
        if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));
        if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

        SendToPlayer(client, new ErrorMessage(code, message, details, field));
    }

    public void BroadcastGlobal<T>(T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToAll(message);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastGlobalExcept<T>(Guid excludedClientId, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToAllExcept(message, excludedClientId);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToParty<T>(ServerPlayerCharacter characterInParty, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);
        if (characterInParty.PartyId == null) return;

        var outgoing = OutgoingMessage.BroadcastToParty(message, characterInParty.PartyId.Value);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToPartyExcept<T>(ServerPlayerCharacter characterInPartyAndToExcluded, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);
        if (characterInPartyAndToExcluded.PartyId == null) return;

        var outgoing = OutgoingMessage.BroadcastToPartyExcept(message, characterInPartyAndToExcluded.PartyId.Value,
            characterInPartyAndToExcluded.Connection.Id);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToGuild<T>(ServerPlayerCharacter clientInGuild, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);
        if (clientInGuild?.GuildId == null) return;

        var outgoing = OutgoingMessage.BroadcastToGuild(message, clientInGuild.GuildId.Value);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToGuildExcept<T>(ServerPlayerCharacter clientInGuildAndToExcluded, T message) where T : INetworkMessage
    {
        ArgumentNullException.ThrowIfNull(message);
        if (clientInGuildAndToExcluded?.GuildId == null) return;

        var outgoing = OutgoingMessage.BroadcastToGuildExcept(message, clientInGuildAndToExcluded.GuildId.Value,
            clientInGuildAndToExcluded.Connection.Id);
        gameServer.QueueOutgoingMessage(outgoing);
    }
}
