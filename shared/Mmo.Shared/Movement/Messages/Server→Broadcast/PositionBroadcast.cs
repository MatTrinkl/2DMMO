using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Movement.Records;

namespace Mmo.Shared.Movement.Messages.Server_Broadcast;

/// <summary>
///     This class is sent to all clients for updating a position of an entity.
///     Uses PersistentId for stable entity identification across zone transfers.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.PositionBroadcast)]
public class PositionBroadcast : IServerMessage, ITimestampedMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.PositionBroadcast;


    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; }

    /// <summary>
    ///     The PersistentId of the entity (stable across zone transfers).
    /// </summary>
    [Key(2)]
    public Guid EntityId { get; set; }

    /// <summary>
    ///     The new position of the entity.
    /// </summary>
    [Key(3)]
    public required Position NewPosition { get; set; }
    [Key(4)]
    public required Velocity NewVelocity { get; set; }
}
