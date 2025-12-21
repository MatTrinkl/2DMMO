using Mmo.Server.Connections;
using Mmo.Server.Core;
using Mmo.Server.Infrastructure.Interfaces;
using Mmo.Server.Messages;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.Infrastructure.Services;

/// <summary>
///     Implementation of IBroadcastService.
///     Delegates to GameServer's output queue.
/// </summary>
public class BroadcastService : IBroadcastService
{
    private readonly GameServer _gameServer;

    public BroadcastService(GameServer gameServer)
    {
        _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
    }

    /// <inheritdoc />
    public void Send(ClientConnection connection, INetworkMessage message)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.ToClient(connection, message);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToZone(ushort zoneId, INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToZone(message, zoneId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToZoneExcept(ushort zoneId, Guid excludeConnectionId, INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToZoneExcept(message, zoneId, excludeConnectionId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToNearby(ushort zoneId, Position position, float radius, INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToNearby(message, zoneId, position, radius);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToNearbyExcept(
        ushort zoneId,
        Position position,
        float radius,
        Guid excludeConnectionId,
        INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToNearby(message, zoneId, position, radius, excludeConnectionId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToParty(Guid partyId, INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToParty(message, partyId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToPartyExcept(Guid partyId, Guid excludeConnectionId, INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToPartyExcept(message, partyId, excludeConnectionId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToGuild(Guid guildId, INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToGuild(message, guildId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToGuildExcept(Guid guildId, Guid excludeConnectionId, INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToGuildExcept(message, guildId, excludeConnectionId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void BroadcastToAll(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToAll(message);
        _gameServer.QueueOutgoingMessage(outgoing);
    }
}
