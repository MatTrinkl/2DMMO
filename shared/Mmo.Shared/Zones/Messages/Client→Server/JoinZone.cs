using MessagePack;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Client_Server;

/// <summary>
///     This class is sent when a player requests to join a zone. TODO: ZoneId.
/// </summary>
[MessagePackObject]
public class JoinZone : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public JoinZone()
    {
    }

    /// <summary>
    ///     Creates a new Join Zone Message.
    /// </summary>
    /// <param name="playerId">The player who wants to join the zone.</param>
    public JoinZone(Guid playerId)
    {
        PlayerId = playerId;
    }

    /// <summary>
    ///     Player who requests to join.
    /// </summary>
    [Key(1)]
    public Guid PlayerId { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.JoinZone;
}
