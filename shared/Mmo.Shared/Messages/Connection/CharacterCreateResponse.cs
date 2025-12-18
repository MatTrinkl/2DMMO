using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Response to character creation request.
/// </summary>
[MessagePackObject]
public class CharacterCreateResponse : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public CharacterCreateResponse()
    {
    }

    /// <summary>
    ///     Creates a new CharacterCreateResponse message.
    /// </summary>
    /// <param name="success">Whether the creation was successful.</param>
    /// <param name="characterId">The new character ID if successful.</param>
    /// <param name="error">Optional error message if failed.</param>
    public CharacterCreateResponse(bool success, Guid? characterId = null, string? error = null)
    {
        Success = success;
        CharacterId = characterId;
        Error = error;
    }

    /// <summary>
    ///     Whether the creation was successful.
    /// </summary>
    [Key(1)]
    public bool Success { get; set; }

    /// <summary>
    ///     The new character ID if successful.
    /// </summary>
    [Key(2)]
    public Guid? CharacterId { get; set; }

    /// <summary>
    ///     Optional error message if creation failed.
    /// </summary>
    [Key(3)]
    public string? Error { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterCreate;
}
