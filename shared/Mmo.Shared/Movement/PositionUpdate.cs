using MessagePack;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Dtos;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Movement;

/// <summary>
///     Message sent to update the position of an entity.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.PositionUpdate)]
public class PositionUpdate : ITimestampedMessage
{
    /// <summary>
    ///     Parameterless constructor used by <see cref="MessagePackSerializer" /> for deserialization.
    /// </summary>
    [SerializationConstructor]
    public PositionUpdate()
    {
    }

    /// <summary>
    ///     Creates a new Position Update Message.
    /// </summary>
    /// <param name="timestamp">The timestamp when this position update happened.</param>
    /// <param name="entity">The Entity DTO for the entity that changed position.</param>
    /// <param name="newPosition">The new position of the entity.</param>
    public PositionUpdate(long timestamp, EntityDtoUnion? entity, Position newPosition)
    {
        Timestamp = timestamp;
        Entity = entity;
        NewPosition = newPosition;
    }

    /// <summary>
    ///     The entity whose position is being updated.
    ///     Uses EntityDtoUnion for polymorphic serialization of different entity types.
    /// </summary>
    [Key(2)]
    public EntityDtoUnion? Entity { get; set; }

    /// <summary>
    ///     The new position of the entity.
    /// </summary>
    [Key(3)]
    public Position? NewPosition { get; set; }

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
}
