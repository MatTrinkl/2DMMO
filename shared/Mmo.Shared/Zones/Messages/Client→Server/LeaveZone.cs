using MessagePack;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Client_Server;

/// <summary>
///     This class is sent when a player requests to leave a zone. TODO: ZoneId.
/// </summary>
[MessagePackObject]
public class LeaveZone : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public LeaveZone()
    {
    }

    /// <summary>
    ///     Creates a new Leave Zone Message.
    /// </summary>
    /// <param name="playerId">The player who wants to leave the zone.</param>
    public LeaveZone(Guid playerId)
    {
        PlayerId = playerId;
    }

    /// <summary>
    ///     Player who requests to leave.
    /// </summary>
    [Key(1)]
    public Guid PlayerId { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LeaveZone;
}
