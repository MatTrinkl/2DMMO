using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Shared.Messages.Movement;

/// <summary>
///     This class is sent to all clients for updating a position of an entity.
///     Uses PersistentId for stable entity identification across zone transfers.
/// </summary>
[MessagePackObject]
public class PositionBroadcast : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public PositionBroadcast()
    {
    }

    /// <summary>
    ///     Creates a new Position Broadcast Message.
    /// </summary>
    /// <param name="timestamp">The timestamp when this position update happened.</param>
    /// <param name="entityId">The PersistentId of the entity (stable across zone transfers).</param>
    /// <param name="newPosition">The new Position of the Entity.</param>
    public PositionBroadcast(long timestamp, Guid entityId, Position newPosition)
    {
        Timestamp = timestamp;
        EntityId = entityId;
        NewPosition = newPosition;
    }

    /// <summary>
    ///     The PersistentId of the entity (stable across zone transfers).
    /// </summary>
    [Key(2)]
    public Guid EntityId { get; set; }

    /// <summary>
    ///     The new position of the entity.
    /// </summary>
    [Key(3)]
    public Position? NewPosition { get; set; }

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
