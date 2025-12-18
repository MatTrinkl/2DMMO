using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     This class indicates a heartbeat of a client.
/// </summary>
[MessagePackObject]
public class Heartbeat : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public Heartbeat()
    {
    }

    /// <summary>
    ///     Creates a new Heartbeat Message.
    /// </summary>
    /// <param name="timestamp">The timestamp when this Heartbeat happened.</param>
    /// <param name="playerId">The Player who gives the heartbeat.</param>
    public Heartbeat(long timestamp, Guid playerId)
    {
        Timestamp = timestamp;
        PlayerId = playerId;
    }

    /// <summary>
    ///     The Player who gives the heartbeat.
    /// </summary>
    [Key(2)]
    public Guid PlayerId { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.Heartbeat;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; set; }
}
