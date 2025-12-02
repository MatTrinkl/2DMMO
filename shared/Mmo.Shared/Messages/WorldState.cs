using MessagePack;
using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages;

[MessagePackObject]
public class PlayerState
{
    [Key(0)]
    public Guid PlayerId { get; set; }

    [Key(1)]
    public string Username { get; set; } = string.Empty;

    [Key(2)]
    public float X { get; set; }

    [Key(3)]
    public float Y { get; set; }
}

[MessagePackObject]
public class WorldState : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.WorldState;

    [Key(0)]
    public List<PlayerState> Players { get; set; } = new();

    [Key(1)]
    public long ServerTick { get; set; }
}
