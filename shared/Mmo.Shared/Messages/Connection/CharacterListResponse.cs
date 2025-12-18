using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Character list item for display in character selection.
/// </summary>
[MessagePackObject]
public class CharacterListItem
{
    [Key(0)]
    public Guid CharacterId { get; set; }

    [Key(1)]
    public string Name { get; set; } = "";

    [Key(2)]
    public int Level { get; set; }

    [Key(3)]
    public Race Race { get; set; }

    [Key(4)]
    public CharacterClass Class { get; set; }

    [Key(5)]
    public Gender Gender { get; set; }

    [Key(6)]
    public ushort ZoneId { get; set; }

    [Key(7)]
    public Position LastPosition { get; set; }
}

/// <summary>
///     Response containing the character list.
/// </summary>
[MessagePackObject]
public class CharacterListResponse : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public CharacterListResponse()
    {
        Characters = new List<CharacterListItem>();
    }

    /// <summary>
    ///     Creates a new CharacterListResponse message.
    /// </summary>
    /// <param name="characters">List of characters.</param>
    public CharacterListResponse(List<CharacterListItem> characters)
    {
        Characters = characters;
    }

    /// <summary>
    ///     List of characters for this account.
    /// </summary>
    [Key(1)]
    public List<CharacterListItem> Characters { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterListResponse;
}
