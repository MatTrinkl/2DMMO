using MessagePack;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.ZoneEvents;

/// <summary>
///     This class is send to all clients to inform them that a player has left a zone. Todo: ZoneId
/// </summary>
[MessagePackObject]
public class PlayerLeftZone : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public PlayerLeftZone()
    {
    }

    /// <summary>
    ///     Creates a new Player Left Zone Message.
    /// </summary>
    /// <param name="player">The player who left the zone.</param>
    public PlayerLeftZone(PlayerEntity player)
    {
        Player = player;
    }

    /// <summary>
    ///     Player who left the zone.
    /// </summary>
    [Key(1)]
    public PlayerEntity Player { get; set; } = new();

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.PlayerLeftZone;
}
