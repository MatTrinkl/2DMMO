using MessagePack;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Messages.Server_Client;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Character.Messages.Client_Server;

/// <summary>
///     This class sends a request to create a new character on the server.
///     Client -> Server
///     Response: <see cref="CharacterCreateResponse"/>.
///     Todo: complete this when doing the character system
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterCreateRequest)]
public class CharacterCreateRequest : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterCreateRequest;

    /// <summary>
    ///     The ID of the selected character.
    /// </summary>
    [Key(1)]
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// The race of the character.
    /// </summary>
    [Key(2)] public Race Race { get; set; }
    /// <summary>
    /// The class of the character.
    /// </summary>
    [Key(3)] public CharacterClass Class { get; set; }
    /// <summary>
    /// The Appearance Data of the character. Todo: not implemented yet.
    /// </summary>
    [Key(4)] public byte[] AppearanceData { get; set; } = new byte[4];
}
