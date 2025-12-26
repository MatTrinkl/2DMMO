using MessagePack;
using Mmo.Shared.Character.Messages.Server_Client;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Character.Messages.Client_Server;

/// <summary>
///     This class sends the selected character from the character list. The server loads the character data and spawns the character if everything is successfully.
///     Client -> Server
///     Ones per Session.
///     Response: <see cref="CharacterSelectResponse"/>.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterSelectRequest)]
public class CharacterSelectRequest : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterSelectRequest;

    /// <summary>
    ///     The ID of the selected character.
    /// </summary>
    [Key(1)]
    public long CharacterId { get; set; }
}
