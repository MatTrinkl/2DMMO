using MessagePack;
using Mmo.Shared.Character.Messages.Client_Server;
using Mmo.Shared.Character.Records;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Character.Messages.Server_Client;

/// <summary>
///     This message is a response to <see cref="CharacterListRequest"/>.
///     Server -> Client
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterListResponse)]
public class CharacterListResponse : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterListResponse;

    /// <summary>
    /// The reason the client needs to disconnect.
    /// </summary>
    [Key(1)]
    public List<CharacterInfo> Characters { get; set; } = [];

}
