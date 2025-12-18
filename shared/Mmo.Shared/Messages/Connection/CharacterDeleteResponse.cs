using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Response to character deletion request.
/// </summary>
[MessagePackObject]
public class CharacterDeleteResponse : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public CharacterDeleteResponse()
    {
    }

    /// <summary>
    ///     Creates a new CharacterDeleteResponse message.
    /// </summary>
    /// <param name="success">Whether the deletion was successful.</param>
    /// <param name="error">Optional error message if failed.</param>
    public CharacterDeleteResponse(bool success, string? error = null)
    {
        Success = success;
        Error = error;
    }

    /// <summary>
    ///     Whether the deletion was successful.
    /// </summary>
    [Key(1)]
    public bool Success { get; set; }

    /// <summary>
    ///     Optional error message if deletion failed.
    /// </summary>
    [Key(2)]
    public string? Error { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterDelete;
}
