using MessagePack;
using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages;

[MessagePackObject]
public class PlayerLeft : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.PlayerLeft;

    [Key(0)]
    public Guid PlayerId { get; set; }
}
