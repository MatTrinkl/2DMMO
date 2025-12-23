using MessagePack;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Movement;

/// <summary>
///     This class is sent to update a position of an entity.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.PositionUpdate)]
public class PositionUpdate : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public PositionUpdate()
    {
    }

    /// <summary>
    ///     Creates a new Position Update Message.
    /// </summary>
    /// <param name="timestamp">The timestamp when this position update happened.</param>
    /// <param name="entityOldPosition">The Entity which changed the position.</param>
    /// <param name="newPosition">The new Position of the Entity.</param>
    public PositionUpdate(long timestamp, IEntity entityOldPosition, Position newPosition)
    {
        Timestamp = timestamp;
        EntityOldPosition = entityOldPosition;
        NewPosition = newPosition;
    }

    /// <summary>
    ///     Entity to updates the position.
    /// </summary>
    [Key(2)]
    public IEntity? EntityOldPosition { get; set; }

    /// <summary>
    ///     New Position of the entity.
    /// </summary>
    [Key(3)]
    public Position? NewPosition { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.PositionUpdate;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; set; }
}
