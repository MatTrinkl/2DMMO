using MessagePack;
using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages;

[MessagePackObject]
public class PositionUpdate : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.PositionUpdate;

    [Key(0)]
    public Guid PlayerId { get; set; }

    [Key(1)]
    public float X { get; set; }

    [Key(2)]
    public float Y { get; set; }

    [Key(3)]
    public long Timestamp { get; set; }
}
