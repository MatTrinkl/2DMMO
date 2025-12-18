using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Response to a heartbeat message (for latency measurement).
/// </summary>
[MessagePackObject]
public class HeartbeatResponse : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public HeartbeatResponse()
    {
    }

    /// <summary>
    ///     Creates a new HeartbeatResponse message.
    /// </summary>
    /// <param name="serverTimestamp">The timestamp from the server's heartbeat.</param>
    /// <param name="clientTimestamp">The timestamp when the client received the heartbeat.</param>
    public HeartbeatResponse(long serverTimestamp, long clientTimestamp)
    {
        ServerTimestamp = serverTimestamp;
        ClientTimestamp = clientTimestamp;
    }

    /// <summary>
    ///     The timestamp from the server's heartbeat.
    /// </summary>
    [Key(1)]
    public long ServerTimestamp { get; set; }

    /// <summary>
    ///     The timestamp when the client received the heartbeat.
    /// </summary>
    [Key(2)]
    public long ClientTimestamp { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.Heartbeat;
}
