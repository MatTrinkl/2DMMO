using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Movement.Enums;
using Mmo.Shared.Movement.Records;

namespace Mmo.Shared.Movement.Messages.Client_Server;

/// <summary>
///     Message sent to update the position of an entity.
///     Client → Server
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.PositionUpdate)]
public class PositionUpdate : IClientMessage, ITimestampedMessage
{
    /// <summary>
    ///     The message type identifier.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.PositionUpdate;

    /// <summary>
    ///     The timestamp of the message in Unix milliseconds.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; }

    /// <summary>
    /// The concurrent number of the input for this position update.
    /// </summary>
    [Key(2)]
    public uint SequenceNumber { get; init; }

    /// <summary>
    ///     The new position of the entity.
    /// </summary>
    [Key(3)]
    public required Position NewPosition { get; set; }

    /// <summary>
    /// The new velocity of the entity
    /// </summary>
    [Key(4)]
    public required Velocity NewVelocity { get; set; }

    /// <summary>
    /// Which input was pressed?
    /// </summary>
    [Key(5)]
    public MovementInputFlags MovementInputFlags { get; init; }
}
