using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Request to select and enter the game with a character.
/// </summary>
[MessagePackObject]
public class CharacterSelect : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public CharacterSelect()
    {
    }

    /// <summary>
    ///     Creates a new CharacterSelect message.
    /// </summary>
    /// <param name="characterId">The character to select.</param>
    public CharacterSelect(Guid characterId)
    {
        CharacterId = characterId;
    }

    /// <summary>
    ///     The character ID to select.
    /// </summary>
    [Key(1)]
    public Guid CharacterId { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterSelect;
}
