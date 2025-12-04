using MessagePack;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.ZoneEvents;

/// <summary>
/// This class is send to all clients to inform them that a player has leaved a zone. Todo: ZoneId
/// </summary>
[MessagePackObject]
public class PlayerLeftZone : INetworkMessage
{
    /// <summary>
    /// The constructor used bei <see cref="MessagePackSerializer"/>.
    /// </summary>
    [SerializationConstructor]
    public PlayerLeftZone()
    {
    }

    /// <summary>
    /// Creates a new Player Left Zone Message.
    /// </summary>
    /// <param name="player">The player who left the zone.</param>
    public PlayerLeftZone(PlayerState player)
    {
        Player = player;
    }
    /// <summary>
    /// The Message Type of this Message.
    /// </summary>
    [Key(0)] public MessageType Type => MessageType.PlayerLeftZone;

    /// <summary>
    /// Player who leaved the zone.
    /// </summary>
    [Key(1)] public PlayerState Player { get; set; }
}
