using Mmo.Server.Connections;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.Infrastructure.Interfaces;

/// <summary>
///     Service for broadcasting messages to clients.
///     Provides clean methods for sending messages to different scopes
///     (zone, nearby, party, guild, all, etc.).
/// </summary>
public interface IBroadcastService
{
    /// <summary>
    ///     Sends a message to a specific client.
    /// </summary>
    void Send(ClientConnection connection, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all players in a zone.
    /// </summary>
    void BroadcastToZone(ushort zoneId, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all players in a zone except one.
    /// </summary>
    void BroadcastToZoneExcept(ushort zoneId, Guid excludeConnectionId, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all players within range of a position.
    /// </summary>
    void BroadcastToNearby(ushort zoneId, Position position, float radius, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all players within range, excluding one.
    /// </summary>
    void BroadcastToNearbyExcept(
        ushort zoneId,
        Position position,
        float radius,
        Guid excludeConnectionId,
        INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all players in a party.
    /// </summary>
    void BroadcastToParty(Guid partyId, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all players in a party except one.
    /// </summary>
    void BroadcastToPartyExcept(Guid partyId, Guid excludeConnectionId, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all players in a guild.
    /// </summary>
    void BroadcastToGuild(Guid guildId, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all players in a guild except one.
    /// </summary>
    void BroadcastToGuildExcept(Guid guildId, Guid excludeConnectionId, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all connected players.
    /// </summary>
    void BroadcastToAll(INetworkMessage message);
}
