using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Request to delete a character.
/// </summary>
[MessagePackObject]
public class CharacterDelete : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public CharacterDelete()
    {
    }

    /// <summary>
    ///     Creates a new CharacterDelete message.
    /// </summary>
    /// <param name="characterId">The character to delete.</param>
    public CharacterDelete(Guid characterId)
    {
        CharacterId = characterId;
    }

    /// <summary>
    ///     The character ID to delete.
    /// </summary>
    [Key(1)]
    public Guid CharacterId { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterDelete;
}
