using MessagePack;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Client_Server;

/// <summary>
///     This class sends a request to switch server.
///     Client -> Server
///     Response: <see cref="ServerSelectResponse"/> with Success=true or ErrorCode.
///     Todo: We dont have different server currently but later at sometime.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.RealmListRequest)]
public class RealmListRequest : IClientMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.RealmListRequest;
}
