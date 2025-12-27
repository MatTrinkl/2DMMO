using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Messages.Client_Server;

/// <summary>
///     This class is sent when a player requests to leave a zone.
///     Client → Server
///     There are multiple responses: Broadcast to all players in the current Zone that the player left,
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.LeaveZone)]
public class LeaveZone : IClientMessage
{
    /// <summary>
    ///     Reason for the leave.
    /// </summary>
    [Key(1)]
    public ZoneLeaveReason Reason { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LeaveZone;
}
