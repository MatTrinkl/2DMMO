using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.Combat;

/// <summary>
/// This class is still a Placeholder
/// </summary>
[MessagePackObject]
public class ActionResult : ITimestampedMessage
{
    /// <summary>
    /// The constructor used bei <see cref="MessagePackSerializer"/>.
    /// </summary>
    [SerializationConstructor]
    public ActionResult()
    {
    }
    /// <summary>
    /// WIP: Creates a new Action Result Message.
    /// </summary>
    /// <param name="timestamp">The timestamp when this action was performed.</param>
    public ActionResult(long timestamp)
    {
        Timestamp = timestamp;
    }
    /// <summary>
    /// The Message Type of this Message.
    /// </summary>
    [Key(0)] public MessageType Type => MessageType.ActionResult;
    /// <summary>
    /// The timestamp of the message.
    /// </summary>
    [Key(1)] public long Timestamp { get; set; }
}
