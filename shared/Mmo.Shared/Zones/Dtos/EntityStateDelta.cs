using MessagePack;
using Mmo.Shared.Generators;
using Mmo.Shared.Zones.Interfaces;

namespace Mmo.Shared.Zones.Dtos;

/// <summary>
///     Delta DTO for state/stat-related changes.
///     This is a manual example - in production, use [GenerateDirtyTracking] on your entity class.
/// </summary>
/// <remarks>
///     This DTO demonstrates the pattern that the DirtyTrackingGenerator will create.
///     For automatic generation, mark your entity with [GenerateDirtyTracking(IdPropertyName = "...")] 
///     and properties with [TrackedProperty(DirtyFlags.Health)], etc.
/// </remarks>
[MessagePackObject]
public class EntityStateDelta : IDeltaDto<Guid>
{
    [Key(0)]
    [DeltaId]
    public Guid EntityId { get; set; }
    
    [Key(1)]
    public int? CurrentHP { get; set; }
    
    [Key(2)]
    public int? MaxHP { get; set; }
    
    [Key(3)]
    public byte? State { get; set; }
    
    [Key(4)]
    public uint? ModelId { get; set; }
    
    [Key(5)]
    public int? Level { get; set; }
    
    public Guid GetId() => EntityId;
}
