using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.System.Enums;

namespace Mmo.Shared.System.Messages;

[MessagePackObject]
[NetworkMessage(MessageType.ServerAnnouncement)]
public class ServerAnnouncement : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ServerAnnouncement()
    {
        Message = null!;
    }

    /// <summary>
    ///     Creates a new ServerAnnouncement Message. Server->Client
    /// </summary>
    /// <param name="timestamp">The timestamp when this ping happened.</param>
    /// <param name="announcementType"></param>
    /// <param name="message"></param>
    /// <param name="details"></param>
    public ServerAnnouncement(long timestamp, AnnouncementType announcementType, string message, string? details)
    {
        Timestamp = timestamp;
        Message = message;
        Details = details;
        AnnouncementType = announcementType;
    }

    [Key(2)] public string Message { get; set; }

    [Key(3)] public string? Details { get; set; }

    [Key(4)] public AnnouncementType AnnouncementType { get; set; }

    [Key(0)] public MessageType Type => MessageType.ServerAnnouncement;

    [Key(1)] public long Timestamp { get; set; }
}
