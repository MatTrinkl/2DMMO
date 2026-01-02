using MessagePack;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Server_Broadcast;

/// <summary>
///     This class is sent to all clients to inform them that a player has joined a zone.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterJoinedZone)]
public class CharacterJoinedZone : IServerMessage
{
    /// <summary>
    ///     Character Entity who joined the zone.
    /// </summary>
    [Key(1)]
    public required CharacterEntityDto JoinedPlayer { get; init; }

    /// <summary>
    ///     The ID of the current zone to check for errors and cheating.
    /// </summary>
    [Key(2)]
    public ushort ZoneId { get; init; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterJoinedZone;
}
