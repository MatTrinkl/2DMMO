using MessagePack;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Character.Messages.Client_Server;

/// <summary>
///     This class requests a list of all characters of the client.
///     Client -> Server
///     Response:
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterListRequest)]
public class CharacterListRequest : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterListRequest;
}
