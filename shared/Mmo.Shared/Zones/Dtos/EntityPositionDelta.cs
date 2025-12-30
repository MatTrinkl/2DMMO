using MessagePack;
using Mmo.Shared.Generators;
using Mmo.Shared.Zones.Interfaces;

namespace Mmo.Shared.Zones.Dtos;

/// <summary>
///     Delta DTO containing only position and movement-related changes for an entity.
///     This is a lightweight alternative to sending the full entity DTO.
/// </summary>
/// <remarks>
///     <para>
///     This DTO is used in the ZoneDelta message system to efficiently transmit
///     position updates. Only the changed properties need to be sent.
///     </para>
///     
///     <para>
///     <strong>Extensibility Pattern:</strong>
///     This DTO demonstrates the delta pattern with nullable fields.
///     You can create similar delta DTOs for other entity aspects:
///     - Use nullable types for optional fields that may not change
///     - Include only the EntityId as non-nullable identifier
///     - Keep DTOs focused on a specific aspect (position, stats, equipment, etc.)
///     - Implement IDeltaDto&lt;TId&gt; for O(1) lookup support
///     </para>
/// </remarks>
/// <example>
/// <code>
/// // Send only position change
/// var delta = new EntityPositionDelta
/// {
///     EntityId = entity.PersistentId,
///     X = entity.X,
///     Y = entity.Y,
///     VelocityX = entity.VelocityX,
///     VelocityY = entity.VelocityY,
///     Rotation = entity.Rotation // nullable, only if changed
/// };
/// 
/// // O(1) lookup in collections
/// var deltaDict = deltas.ToDictionary(d => d.GetId());
/// if (deltaDict.TryGetValue(entityId, out var found))
/// {
///     // Apply delta
/// }
/// </code>
/// </example>
[MessagePackObject]
public class EntityPositionDelta : IDeltaDto<Guid>
{
    /// <summary>
    ///     Gets or sets the unique identifier of the entity.
    /// </summary>
    [Key(0)]
    [DeltaId]
    public Guid EntityId { get; set; }
    
    /// <summary>
    ///     Gets or sets the X coordinate.
    /// </summary>
    [Key(1)]
    public float X { get; set; }
    
    /// <summary>
    ///     Gets or sets the Y coordinate.
    /// </summary>
    [Key(2)]
    public float Y { get; set; }
    
    /// <summary>
    ///     Gets or sets the velocity on the X axis.
    /// </summary>
    [Key(3)]
    public float VelocityX { get; set; }
    
    /// <summary>
    ///     Gets or sets the velocity on the Y axis.
    /// </summary>
    [Key(4)]
    public float VelocityY { get; set; }
    
    /// <summary>
    ///     Gets or sets the rotation (optional).
    ///     Null if rotation hasn't changed.
    /// </summary>
    [Key(5)]
    public float? Rotation { get; set; }
    
    /// <summary>
    ///     Gets the identifier value for O(1) lookup in collections.
    /// </summary>
    /// <returns>The entity identifier.</returns>
    public Guid GetId() => EntityId;
}
