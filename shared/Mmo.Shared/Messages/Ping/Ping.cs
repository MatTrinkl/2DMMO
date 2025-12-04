using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.Ping;

[MessagePackObject]
public class Ping:ITimestampedMessage
{
    /// <summary>
    /// The constructor used bei <see cref="MessagePackSerializer"/>.
    /// </summary>
    [SerializationConstructor]
    public Ping()
    {
    }

    /// <summary>
    /// Creates a new Ping Message. Client->Server
    /// </summary>
    /// <param name="timestamp">The timestamp when this ping happened.</param>
    public Ping(long timestamp)
    {
        Timestamp = timestamp;
    }
    /// <summary>
    /// The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.Ping;
    /// <summary>
    /// The timestamp of the message.
    /// </summary>
    [Key(1)] public long Timestamp { get; set; }
}
