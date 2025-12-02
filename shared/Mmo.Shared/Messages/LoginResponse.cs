using MessagePack;
using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages;

[MessagePackObject]
public class LoginResponse : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.LoginResponse;

    [Key(0)]
    public bool Success { get; set; }

    [Key(1)]
    public Guid PlayerId { get; set; }

    [Key(2)]
    public string? ErrorMessage { get; set; }
}
