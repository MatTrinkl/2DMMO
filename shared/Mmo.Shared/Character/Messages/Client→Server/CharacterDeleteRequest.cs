using MessagePack;
using Mmo.Shared.Character.Messages.Server_Client;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Character.Messages.Client_Server;

/// <summary>
///     This class sends a delete request to the server.
///     Client -> Server
///     Response: <see cref="CharacterDeleteResponse" />
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterDeleteRequest)]
public class CharacterDeleteRequest : IClientMessage
{
    /// <summary>
    ///     The ID of the character to delete.
    /// </summary>
    [Key(1)]
    public long CharacterId { get; set; }

    /// <summary>
    ///     The confirmation of the deletion. This needs to be "DELETE"
    /// </summary>
    [Key(2)]
    public string Confirmation { get; set; } = "";

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterDeleteRequest;
}
