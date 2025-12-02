using MessagePack;
using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages;

[MessagePackObject]
public class ChatMessage : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.ChatMessage;

    [Key(0)]
    public Guid SenderId { get; set; }

    [Key(1)]
    public string SenderName { get; set; } = string.Empty;

    [Key(2)]
    public string Message { get; set; } = string.Empty;

    [Key(3)]
    public long Timestamp { get; set; }
}
