using Mmo.Shared.DirtyTracking.Attributes;
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
[GenerateDirtyTracking(IdPropertyName = "EntityId", FlagsEnumType = "Mmo.Shared.Entities.Enums.EntityDirtyFlags")]
public partial class TestEntity
{
    public Guid EntityId { get; set; }

    [TrackDirty("Position")]
    public float X { get; set; }

    [TrackDirty("Position")]
    public float Y { get; set; }

    [TrackDirty("Velocity")]
    public float VelocityX { get; set; }

    [TrackDirty("Velocity")]
    public float VelocityY { get; set; }

    [TrackDirty("Health")]
    public int CurrentHP { get; set; }

    [TrackDirty("MaxHealth")]
    public int MaxHP { get; set; }

    public string Name { get; set; } = "";
}
