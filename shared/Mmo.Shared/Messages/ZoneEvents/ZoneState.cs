using MessagePack;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.ZoneEvents;

/// <summary>
///     This class updates the state of a zone. It's like a position update of all entities at ones.
/// </summary>
[MessagePackObject]
public class ZoneState : ITimestampedMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ZoneState()
    {
    }

    /// <summary>
    ///     Creates a new Zone State Message.
    /// </summary>
    /// <param name="timestamp">The timestamp of the message.</param>
    /// <param name="zoneId">The ID of the zone. (Later we need to see how shards work with that).</param>
    /// <param name="entities">All Entities in this Zone.</param>
    public ZoneState(long timestamp, Guid zoneId, List<EntityState> entities)
    {
        Timestamp = timestamp;
        ZoneId = zoneId;
        Entities = entities;
    }

    /// <summary>
    ///     ID of the zone. WIP!
    /// </summary>
    [Key(2)]
    public Guid ZoneId { get; set; }

    /// <summary>
    ///     List of all Entities in this zone.
    /// </summary>
    [Key(3)]
    public List<EntityState> Entities { get; set; } = new();

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ZoneState;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; set; }
}
