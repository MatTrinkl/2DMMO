using MessagePack;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Messages.Client_Server;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Character.Messages.Server_Client;

/// <summary>
///     This message is a response to <see cref="CharacterCreateRequest" />.
///     Server -> Client
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterCreateResponse)]
public class CharacterCreateResponse : IResponseMessage<CharacterCreateResponseErrorCode>
{
    /// <summary>
    ///     The ID of the new character.
    /// </summary>
    [Key(5)]
    public long? CharacterId { get; init; }

    /// <summary>
    ///     The name of the new character.
    /// </summary>
    [Key(6)]
    public string? Name { get; init; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterCreateResponse;

    /// <inheritdoc />
    [Key(1)]
    public bool Success { get; init; }

    /// <inheritdoc />
    [Key(2)]
    public GlobalErrorCode GlobalError { get; init; }

    /// <inheritdoc />
    [Key(3)]
    public string? ErrorMessage { get; init; }

    /// <summary>
    ///     The specific error when <see cref="Success" /> is false and <see cref="GlobalError" /> =
    ///     <see cref="GlobalErrorCode.None" />.
    /// </summary>
    [Key(4)]
    public CharacterCreateResponseErrorCode? ErrorCode { get; init; }
}
