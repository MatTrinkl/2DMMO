using Mmo.Server.Connections;
using Mmo.Server.Core;
using Mmo.Server.Messages;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.PlayerService;
using Mmo.Server.Zones.Interfaces;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.System.Messages;

namespace Mmo.Server.Network.Services;

public class BroadcastService(GameServer gameServer)
    : IBroadcastService
{
    public void BroadcastToZone(ushort zoneId, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToZone(message, zoneId);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToZoneExcept(ushort zoneId, Guid excludedClientId, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToZoneExcept(message, zoneId, excludedClientId);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastInRange(ushort zoneId, Position center, float radius, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToNearby(message, zoneId, center, radius);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastInRangeExcept(ushort zoneId, Position center, float radius, Guid excludedClientId,
        INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToNearby(message, zoneId, center, radius, excludedClientId);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void SendToPlayer(ClientConnection client, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.ToClient(client, message);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void SendToPlayers(IEnumerable<ClientConnection> clients, INetworkMessage message)
    {
        foreach (ClientConnection client in clients)
            SendToPlayer(client, message);
    }

    public void SendError(ClientConnection client, string code, string message, string? details=null, string? field=null)
    {
        if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));
        if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

        SendToPlayer(client, new ErrorMessage(code, message, details, field));
    }

    public void BroadcastGlobal(INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToAll(message);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastGlobalExcept(Guid excludedClientId, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var outgoing = OutgoingMessage.BroadcastToAllExcept(message, excludedClientId);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToParty(ServerPlayerCharacter characterInParty, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (characterInParty.PartyId == null) return;

        var outgoing = OutgoingMessage.BroadcastToParty(message, characterInParty.PartyId.Value);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToPartyExcept(ServerPlayerCharacter characterInPartyAndToExcluded, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (characterInPartyAndToExcluded.PartyId == null) return;

        var outgoing = OutgoingMessage.BroadcastToPartyExcept(message, characterInPartyAndToExcluded.PartyId.Value,
            characterInPartyAndToExcluded.Connection.Id);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToGuild(ServerPlayerCharacter clientInGuild, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (clientInGuild?.GuildId == null) return;

        var outgoing = OutgoingMessage.BroadcastToGuild(message, clientInGuild.GuildId.Value);
        gameServer.QueueOutgoingMessage(outgoing);
    }

    public void BroadcastToGuildExcept(ServerPlayerCharacter clientInGuildAndToExcluded, INetworkMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (clientInGuildAndToExcluded?.GuildId == null) return;

        var outgoing = OutgoingMessage.BroadcastToGuildExcept(message, clientInGuildAndToExcluded.GuildId.Value,
            clientInGuildAndToExcluded.Connection.Id);
        gameServer.QueueOutgoingMessage(outgoing);
    }
}
