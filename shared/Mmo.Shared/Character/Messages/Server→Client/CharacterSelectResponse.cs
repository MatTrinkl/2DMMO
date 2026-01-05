using MessagePack;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Messages.Client_Server;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Character.Messages.Server_Client;

/// <summary>
///     This message is a response to <see cref="CharacterSelectRequest" />.
///     Server -> Client
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterSelectResponse)]
public class CharacterSelectResponse : IResponseMessage<CharacterSelectResponseErrorCode>
{
    /// <summary>
    ///     Only if <see cref="Success" />==true.
    ///     The ID of the selected character.
    /// </summary>
    [Key(5)]
    public long? CharacterId { get; init; } = null;

    /// <summary>
    ///     Only if <see cref="Success" />==true.
    ///     The ID of the zone where the character will spawn.
    /// </summary>
    [Key(6)]
    public ushort? SpawnZoneId { get; init; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterSelectResponse;

    /// <inheritdoc />
    [Key(1)]
    public bool Success { get; init; }

    /// <inheritdoc />
    [Key(2)]
    public GlobalErrorCode GlobalError { get; init; } = GlobalErrorCode.None;

    /// <inheritdoc />
    [Key(3)]
    public string? ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    ///     The specific error when <see cref="Success" /> is false and <see cref="GlobalError" /> =
    ///     <see cref="GlobalErrorCode.None" />.
    /// </summary>
    [Key(4)]
    public CharacterSelectResponseErrorCode? ErrorCode { get; init; } = null;
}
