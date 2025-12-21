using MessagePack;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages;

/// <summary>
///     This class is send to all clients to inform them that a player has joined a zone. Todo: ZoneId
/// </summary>
[MessagePackObject]
public class PlayerJoinedZone : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public PlayerJoinedZone()
    {
    }

    /// <summary>
    ///     Creates a new Player Join Zone Message.
    /// </summary>
    /// <param name="player">The player who joins the zone.</param>
    public PlayerJoinedZone(PlayerEntity player)
    {
        Player = player;
    }

    /// <summary>
    ///     Player who joined the zone.
    /// </summary>
    [Key(1)]
    public PlayerEntity Player { get; set; } = new();

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.PlayerJoinedZone;
}
