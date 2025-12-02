using MessagePack;
using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages;

[MessagePackObject]
public class PlayerJoined : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.PlayerJoined;

    [Key(0)]
    public Guid PlayerId { get; set; }

    [Key(1)]
    public string Username { get; set; } = string.Empty;

    [Key(2)]
    public float X { get; set; }

    [Key(3)]
    public float Y { get; set; }
}
