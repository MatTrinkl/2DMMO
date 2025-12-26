using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Client_Server;

/// <summary>
///     This class sends a logout request to the server.
///     Client -> Server
///     Ones per Session.
///     No Response is coming only the graceful Session closed signal via TCP.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.LogoutRequest)]
public class LogoutRequest : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LogoutRequest;


    /// <summary>
    ///     The reason for the logout. Todo: make this to an enum
    /// </summary>
    [Key(2)]
    public string? Reason { get; set; } = "";
}
