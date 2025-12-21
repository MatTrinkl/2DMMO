using Mmo.Server.Connections;
using Mmo.Server.Network;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.Messages;

/// <summary>
///     Represents an incoming message in the input queue.
/// </summary>
public readonly struct IncomingMessage
{
    /// <summary>The connection from which the message came.</summary>
    public ClientConnection Connection { get; init; }

    /// <summary>The type of the message.</summary>
    public MessageType MessageType { get; init; }

    /// <summary>The deserialized message.</summary>
    public INetworkMessage Message { get; init; }

    /// <summary>Timestamp when the message was received.</summary>
    public DateTimeOffset ReceivedAt { get; init; }

    public IncomingMessage(
        ClientConnection connection,
        MessageType messageType,
        INetworkMessage message)
    {
        Connection = connection;
        MessageType = messageType;
        Message = message;
        ReceivedAt = DateTimeOffset.UtcNow;
    }
}
