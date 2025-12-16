using System.Collections.Concurrent;
using Mmo.Server.Networking;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Messages;

// Statt zwei Queues - eine einheitliche Struktur
public readonly struct OutgoingMessage
{
    public INetworkMessage Message { get; private init; }
    public ClientConnection? TargetClient { get; private init; } // null = Broadcast an alle
    public ushort? ZoneId { get; private init; } // ZoneId

    // Factory Methods für Klarheit
    public static OutgoingMessage BroadcastToServer(INetworkMessage message)
        => new() { Message = message, TargetClient = null, ZoneId = null };

    public static OutgoingMessage BroadcastToZone(INetworkMessage message, ushort zoneId) => new OutgoingMessage()
        { Message = message, TargetClient = null, ZoneId = zoneId };

    public static OutgoingMessage ToClient(ClientConnection client, INetworkMessage message)
        => new() { Message = message, TargetClient = client, ZoneId = null };
}
