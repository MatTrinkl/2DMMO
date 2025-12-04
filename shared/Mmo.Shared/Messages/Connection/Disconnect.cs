using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
/// This class informs all clients when a player is disconnected.
/// </summary>
[MessagePackObject]
public class Disconnect  : INetworkMessage
{
    /// <summary>
    /// The constructor used bei <see cref="MessagePackSerializer"/>.
    /// </summary>
    [SerializationConstructor]
    public Disconnect()
    {
    }
    /// <summary>
    /// Creates a new Disconnect Message.
    /// </summary>
    /// <param name="playerId">The Player who disconnects.</param>
    public Disconnect(Guid playerId)
    {
        PlayerId = playerId;
    }
    /// <summary>
    /// The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.Disconnect;
    /// <summary>
    /// The player who is disconnected.
    /// </summary>
    [Key(1)]
    public Guid PlayerId { get; set; }
}
