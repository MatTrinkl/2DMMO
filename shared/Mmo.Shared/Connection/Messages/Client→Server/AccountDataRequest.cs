using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Networking;

namespace Mmo.Shared.Connection.Messages.Client_Server;

/// <summary>
///     This class requests detailed account information.
///     Client->Server
///     Response is <see cref="AccountDataResponse"/>
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.AccountDataRequest)]
public class AccountDataRequest : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.AccountDataRequest;
}
