using MessagePack;
using Mmo.Shared.Generators;
using Mmo.Shared.Zones.Interfaces;

namespace Mmo.Shared.Zones.Dtos;

/// <summary>
///     Delta DTO for position-related changes.
///     This is a manual example - in production, use [GenerateDirtyTracking] on your entity class.
/// </summary>
/// <remarks>
///     This DTO demonstrates the pattern that the DirtyTrackingGenerator will create.
///     For automatic generation, mark your entity with [GenerateDirtyTracking(IdPropertyName = "...")] 
///     and properties with [TrackedProperty(DirtyFlags.Position)].
/// </remarks>
[MessagePackObject]
public class EntityPositionDelta : IDeltaDto<Guid>
{
    [Key(0)]
    [DeltaId]
    public Guid EntityId { get; set; }
    
    [Key(1)]
    public float X { get; set; }
    
    [Key(2)]
    public float Y { get; set; }
    
    [Key(3)]
    public float VelocityX { get; set; }
    
    [Key(4)]
    public float VelocityY { get; set; }
    
    [Key(5)]
    public float? Rotation { get; set; }
    
    public Guid GetId() => EntityId;
}
