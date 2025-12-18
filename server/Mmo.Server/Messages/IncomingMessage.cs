using Mmo.Server.Networking;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Messages;

/// <summary>
///     Repräsentiert eine eingehende Message in der Input-Queue.
/// </summary>
public readonly struct IncomingMessage
{
    /// <summary>Die Connection von der die Message kam.</summary>
    public ClientConnection Connection { get; init; }

    /// <summary>Der Typ der Message. </summary>
    public MessageType MessageType { get; init; }

    /// <summary>Die deserialisierte Message.</summary>
    public INetworkMessage Message { get; init; }

    /// <summary>Zeitpunkt wann die Message empfangen wurde. </summary>
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
