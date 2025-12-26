using MessagePack;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Messages.Client_Server;
using Mmo.Shared.Character.Records;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Character.Messages.Server_Client;

/// <summary>
///     This message is a response to <see cref="CharacterDeleteRequest"/>.
///     Server -> Client
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterDeleteResponse)]
public class CharacterDeleteResponse : IResponseMessage<CharacterDeleteResponseErrorCode>
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterDeleteResponse;

    /// <inheritdoc/>
    [Key(1)]
    public bool Success { get; init; }

    /// <inheritdoc/>
    [Key(2)]
    public GlobalErrorCode GlobalError { get; init; }

    /// <inheritdoc/>
    [Key(3)]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The specific error when <see cref="Success"/> is false and <see cref="GlobalError"/> = <see cref="GlobalErrorCode.None"/>.
    /// </summary>
    [Key(4)]
    public CharacterDeleteResponseErrorCode? ErrorCode { get; init; }
    /// <summary>
    /// The ID of the deleted character (to mark in the list).
    /// </summary>
    [Key(5)]
    public long? CharacterId { get; init; }
    /// <summary>
    /// The time of deletion. It's possible to instantiate that character in the next 24 hours (Soft delete).
    /// </summary>
    [Key(6)]
    public long? DeletionTime { get; init; }
}
