using Mmo.Shared.Interfaces;

namespace Mmo.Server.Networking.NetworkEvents;

/// <summary>
///     Event arguments for when a message is received from a client.
/// </summary>
public class MessageReceivedEventArgs(Guid clientId, INetworkMessage message) : EventArgs
{
    /// <summary>
    ///     The client who sent the message.
    /// </summary>
    public Guid ClientId { get; } = clientId;

    /// <summary>
    ///     The deserialized network message.
    /// </summary>
    public INetworkMessage Message { get; } = message;

    /// <summary>
    ///     Timestamp when the message was received.
    /// </summary>
    public DateTimeOffset ReceivedAt { get; } = DateTimeOffset.UtcNow;
}
