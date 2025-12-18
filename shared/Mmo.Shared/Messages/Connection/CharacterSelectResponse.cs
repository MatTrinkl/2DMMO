using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Response to character selection.
/// </summary>
[MessagePackObject]
public class CharacterSelectResponse : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public CharacterSelectResponse()
    {
    }

    /// <summary>
    ///     Creates a new CharacterSelectResponse message.
    /// </summary>
    /// <param name="success">Whether the selection was successful.</param>
    /// <param name="error">Optional error message if failed.</param>
    public CharacterSelectResponse(bool success, string? error = null)
    {
        Success = success;
        Error = error;
    }

    /// <summary>
    ///     Whether the selection was successful.
    /// </summary>
    [Key(1)]
    public bool Success { get; set; }

    /// <summary>
    ///     Optional error message if selection failed.
    /// </summary>
    [Key(2)]
    public string? Error { get; set; }

    /// <summary>
    ///     Character ID.
    /// </summary>
    [Key(3)]
    public Guid CharacterId { get; set; }

    /// <summary>
    ///     Character name.
    /// </summary>
    [Key(4)]
    public string? Name { get; set; }

    /// <summary>
    ///     Character level.
    /// </summary>
    [Key(5)]
    public int Level { get; set; }

    /// <summary>
    ///     Zone ID where the character is located.
    /// </summary>
    [Key(6)]
    public ushort ZoneId { get; set; }

    /// <summary>
    ///     Character position.
    /// </summary>
    [Key(7)]
    public Position Position { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterSelect;
}
