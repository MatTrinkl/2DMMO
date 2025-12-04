using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Shared.Messages.Movement;

/// <summary>
///     This class is sent to all clients for updating a position of an entity.
/// </summary>
[MessagePackObject]
public class PositionBroadcast : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public PositionBroadcast()
    {
    }

    /// <summary>
    ///     Creates a new Position Broadcast Message.
    /// </summary>
    /// <param name="timestamp">The timestamp when this position update happened.</param>
    /// <param name="entityId">The Entity which changed the position.</param>
    /// <param name="newPosition">The new Position of the Entity.</param>
    public PositionBroadcast(long timestamp, Guid entityId, Position newPosition)
    {
        Timestamp = timestamp;
        EntityId = entityId;
        NewPosition = newPosition;
    }

    /// <summary>
    ///     Entity which has changed position
    /// </summary>
    [Key(2)]
    public Guid EntityId { get; set; }

    /// <summary>
    ///     The new position of the entity.
    /// </summary>
    [Key(3)]
    public Position NewPosition { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.PositionBroadcast;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; set; }
}
