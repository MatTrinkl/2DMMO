using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Request to create a new character.
/// </summary>
[MessagePackObject]
public class CharacterCreate : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public CharacterCreate()
    {
    }

    /// <summary>
    ///     Creates a new CharacterCreate message.
    /// </summary>
    /// <param name="name">The character name.</param>
    /// <param name="race">The character race.</param>
    /// <param name="charClass">The character class.</param>
    /// <param name="gender">The character gender.</param>
    public CharacterCreate(string name, Race race, CharacterClass charClass, Gender gender)
    {
        Name = name;
        Race = race;
        Class = charClass;
        Gender = gender;
    }

    /// <summary>
    ///     The character name.
    /// </summary>
    [Key(1)]
    public string Name { get; set; } = "";

    /// <summary>
    ///     The character race.
    /// </summary>
    [Key(2)]
    public Race Race { get; set; }

    /// <summary>
    ///     The character class.
    /// </summary>
    [Key(3)]
    public CharacterClass Class { get; set; }

    /// <summary>
    ///     The character gender.
    /// </summary>
    [Key(4)]
    public Gender Gender { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterCreate;
}
