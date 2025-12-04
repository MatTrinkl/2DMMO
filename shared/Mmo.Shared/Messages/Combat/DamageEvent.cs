using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.Combat;

/// <summary>
/// This class is still a Placeholder
/// </summary>
[MessagePackObject]
public class DamageEvent : ITimestampedMessage
{
    /// <summary>
    /// The constructor used bei <see cref="MessagePackSerializer"/>.
    /// </summary>
    [SerializationConstructor]
    public DamageEvent()
    {
    }
    /// <summary>
    /// WIP: Creates a new Damage Event Message.
    /// </summary>
    /// <param name="timestamp">The timestamp when this action was performed.</param>
    public DamageEvent(long timestamp)
    {
        Timestamp = timestamp;
    }
    /// <summary>
    /// The Message Type of this Message.
    /// </summary>
    [Key(0)] public MessageType Type => MessageType.DamageEvent;
    /// <summary>
    /// The timestamp of the message.
    /// </summary>
    [Key(1)] public long Timestamp { get; set; }
}
