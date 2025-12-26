using MessagePack;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Records;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Server_Client;

/// <summary>
///     This class sends a response to a <see cref="RealmListRequest" /> to the client.
///     Server -> Client
///     Todo: We dont have different server currently but later at sometime.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.RealmListResponse)]
public class RealmListResponse : IServerMessage
{
    /// <summary>
    ///     A list of all realms.
    /// </summary>
    [Key(1)]
    public List<RealmInfo> Realms { get; set; } = [];

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.RealmListResponse;
}
