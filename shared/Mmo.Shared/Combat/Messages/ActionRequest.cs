using MessagePack;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Combat.Messages;

/// <summary>
///     This class is still a Placeholder
/// </summary>
[MessagePackObject]
public class ActionRequest : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ActionRequest()
    {
    }

    /// <summary>
    ///     WIP: Creates a new Action Request Message.
    /// </summary>
    /// <param name="timestamp">The timestamp when this action was performed.</param>
    public ActionRequest(long timestamp)
    {
        Timestamp = timestamp;
    }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ActionRequest;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; set; }
}
