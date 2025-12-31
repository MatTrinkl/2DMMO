using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Messages.Server_Broadcast;

/// <summary>
///     This class is send to all clients to inform them that a player has left a zone. Todo: ZoneId
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterLeftZone)]
public class CharacterLeftZone : IServerMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterLeftZone;

    /// <summary>
    /// ID of the Character who left the zone.
    /// </summary>
    [Key(1)]
    public Guid CharacterId { get; init; }

    /// <summary>
    /// The reason the player disconnected.
    /// </summary>
    [Key(2)]
    public ZoneLeaveReason LeaveReason { get; init; }

    /// <summary>
    /// The ID of the current zone to check for errors and cheating.
    /// </summary>
    [Key(3)]
    public ushort ZoneId { get; init; }
}
