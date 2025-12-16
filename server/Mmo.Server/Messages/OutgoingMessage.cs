using System.Collections.Concurrent;
using Mmo.Server.Networking;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Messages;

/// <summary>
/// Represents an outgoing network message with its destination information.
/// Used by the GameServer to queue messages for the Output phase.
/// </summary>
/// <remarks>
/// This unified structure replaces separate queues for different message types.
/// Messages can be targeted to a specific client, broadcast to all clients, or broadcast to a specific zone.
/// </remarks>
public readonly struct OutgoingMessage
{
    /// <summary>
    /// The network message to be sent.
    /// </summary>
    public INetworkMessage Message { get; private init; }
    
    /// <summary>
    /// The target client connection. If null, the message will be broadcast.
    /// </summary>
    public ClientConnection? TargetClient { get; private init; }
    
    /// <summary>
    /// The target zone ID for zone-specific broadcasts. If null, broadcasts to all zones.
    /// </summary>
    public ushort? ZoneId { get; private init; }

    /// <summary>
    /// Creates an outgoing message that broadcasts to all clients on the server.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <returns>An OutgoingMessage configured for server-wide broadcast.</returns>
    public static OutgoingMessage BroadcastToServer(INetworkMessage message)
        => new() { Message = message, TargetClient = null, ZoneId = null };

    /// <summary>
    /// Creates an outgoing message that broadcasts to all clients in a specific zone.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <param name="zoneId">The zone ID to broadcast to.</param>
    /// <returns>An OutgoingMessage configured for zone-specific broadcast.</returns>
    public static OutgoingMessage BroadcastToZone(INetworkMessage message, ushort zoneId) => new OutgoingMessage()
        { Message = message, TargetClient = null, ZoneId = zoneId };

    /// <summary>
    /// Creates an outgoing message targeted to a specific client.
    /// </summary>
    /// <param name="client">The target client connection.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>An OutgoingMessage configured for single-client delivery.</returns>
    public static OutgoingMessage ToClient(ClientConnection client, INetworkMessage message)
        => new() { Message = message, TargetClient = client, ZoneId = null };
}
