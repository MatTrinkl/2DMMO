using MessagePack;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Shared.Messages.Movement;

/// <summary>
///     This class is sent to update a position of an entity.
/// </summary>
[MessagePackObject]
public class PositionUpdate : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
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
    public PositionUpdate(long timestamp, EntityState entityOldPosition, Position newPosition)
    {
        Timestamp = timestamp;
        EntityOldPosition = entityOldPosition;
        NewPosition = newPosition;
    }

    /// <summary>
    ///     Entity to updates the position.
    /// </summary>
    [Key(2)]
    public EntityState? EntityOldPosition { get; set; }

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
