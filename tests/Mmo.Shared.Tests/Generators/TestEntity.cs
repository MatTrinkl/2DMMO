using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Generators;

namespace Mmo.Shared.Tests.Generators;

/// <summary>
///     Example entity demonstrating automatic dirty-tracking generation.
///     The DirtyTrackingGenerator will automatically create:
///     - IDirtyTrackable implementation
///     - TestEntityPositionDelta.g.cs
///     - TestEntityStateDelta.g.cs (if there are health/state properties)
///     - Extension methods (ToPositionDelta(), ToStateDelta())
/// </summary>
[GenerateDirtyTracking(IdPropertyName = "EntityId")]
public partial class TestEntity
{
    public Guid EntityId { get; set; }
    
    [TrackedProperty(DirtyFlags.Position)]
    public float X { get; set; }
    
    [TrackedProperty(DirtyFlags.Position)]
    public float Y { get; set; }
    
    [TrackedProperty(DirtyFlags.Velocity)]
    public float VelocityX { get; set; }
    
    [TrackedProperty(DirtyFlags.Velocity)]
    public float VelocityY { get; set; }
    
    [TrackedProperty(DirtyFlags.Health)]
    public int CurrentHP { get; set; }
    
    [TrackedProperty(DirtyFlags.MaxHealth)]
    public int MaxHP { get; set; }
    
    public string Name { get; set; } = "";
}
