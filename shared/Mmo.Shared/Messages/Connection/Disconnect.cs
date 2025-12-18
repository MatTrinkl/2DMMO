using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     This class informs all clients when a player is disconnected.
/// </summary>
[MessagePackObject]
public class Disconnect : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public Disconnect()
    {
    }

    /// <summary>
    ///     Creates a new Disconnect Message.
    /// </summary>
    /// <param name="playerId">The Player who disconnects.</param>
    /// <param name="reason">The reasen why the player is disconnecting.</param>
    public Disconnect(Guid playerId, DisconnectReason reason)
    {
        PlayerId = playerId;
        Reason = reason;
    }
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.Disconnect;

    /// <summary>
    ///     The player who is disconnected.
    /// </summary>
    [Key(1)]
    public Guid PlayerId { get; set; }
    [Key(2)]
    public DisconnectReason Reason { get; init; }
    [Key(3)]
    public string? Message { get; set; }


}
