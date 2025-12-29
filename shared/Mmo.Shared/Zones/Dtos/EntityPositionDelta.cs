using MessagePack;

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
/// </code>
/// </example>
[MessagePackObject]
public class EntityPositionDelta
{
    /// <summary>
    ///     Gets or sets the unique identifier of the entity.
    /// </summary>
    [Key(0)]
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
}
