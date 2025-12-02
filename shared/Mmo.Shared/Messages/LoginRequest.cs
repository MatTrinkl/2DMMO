using MessagePack;
using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages;

[MessagePackObject]
public class LoginRequest : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.LoginRequest;

    [Key(0)]
    public string Username { get; set; } = string.Empty;
}
