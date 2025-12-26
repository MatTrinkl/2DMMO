using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.System.Messages;

[MessagePackObject]
[NetworkMessage(MessageType.Pong)]
public class Pong : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public Pong()
    {
    }

    /// <summary>
    ///     Creates a new Pong Message. Server->Client
    /// </summary>
    /// <param name="timestamp">The timestamp when this pong happened.</param>
    public Pong(long timestamp)
    {
        Timestamp = timestamp;
    }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.Pong;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; }
}
