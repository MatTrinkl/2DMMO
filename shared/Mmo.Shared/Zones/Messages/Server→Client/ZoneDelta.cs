using MessagePack;
using Mmo.Shared.Entities.Dtos;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Dtos;

namespace Mmo.Shared.Zones.Messages.Server_Client;

/// <summary>
///     Delta message containing only entity and zone changes since the last update.
///     This message is sent every tick (40ms) and uses batching to minimize bandwidth.
/// </summary>
/// <remarks>
///     <para>
///     <strong>High-Frequency Message:</strong>
///     This message is sent at 25Hz (every 40ms tick) when there are changes.
///     Only changed properties are included to minimize bandwidth usage.
///     </para>
///     
///     <para>
///     <strong>Extensibility:</strong>
///     Additional delta types can be added by:
///     1. Creating a new Delta DTO (following EntityPositionDelta/EntityStateDelta pattern)
///     2. Adding a nullable List property here
///     3. Populating it server-side based on DirtyFlags
///     </para>
///     
///     <para>
///     <strong>Null Fields:</strong>
///     All collection fields are nullable. Null = no changes of that type.
///     This minimizes message size when only specific aspects have changed.
///     </para>
/// </remarks>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneDelta)]
public class ZoneDelta : IServerMessage, ITimestampedMessage
{
    /// <summary>
    ///     Gets the message type (ZoneDelta).
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ZoneDelta;
    
    /// <summary>
    ///     Gets or sets the server timestamp (Unix milliseconds).
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; }
    
    /// <summary>
    ///     Gets or sets the zone ID.
    /// </summary>
    [Key(2)]
    public ushort ZoneId { get; init; }
    
    /// <summary>
    ///     Gets or sets the list of newly spawned entities (nullable).
    ///     Null if no entities spawned this tick.
    /// </summary>
    [Key(3)]
    public List<EntityDtoUnion>? SpawnedEntities { get; init; }
    
    /// <summary>
    ///     Gets or sets the list of despawned entity IDs (nullable).
    ///     Null if no entities despawned this tick.
    /// </summary>
    [Key(4)]
    public List<Guid>? DespawnedEntityIds { get; init; }
    
    /// <summary>
    ///     Gets or sets the list of position updates (nullable).
    ///     Null if no position changes this tick.
    /// </summary>
    /// <remarks>
    ///     Uses EntityPositionDelta for compact transmission of position data.
    /// </remarks>
    [Key(5)]
    public List<EntityPositionDelta>? PositionUpdates { get; init; }
    
    /// <summary>
    ///     Gets or sets the list of state/stat updates (nullable).
    ///     Null if no state changes this tick.
    /// </summary>
    /// <remarks>
    ///     Uses EntityStateDelta with nullable fields - only changed properties are set.
    /// </remarks>
    [Key(6)]
    public List<EntityStateDelta>? StateUpdates { get; init; }
    
    /// <summary>
    ///     Gets or sets the zone context delta (nullable).
    ///     Null if no zone-wide changes (weather, time) this tick.
    /// </summary>
    [Key(7)]
    public ZoneContextDelta? ContextDelta { get; init; }
}
