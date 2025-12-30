using Mmo.Shared.Character.Enums;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.DirtyTracking.Attributes;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Generators;

namespace Mmo.Shared.Entities.Interfaces;

/// <summary>
///     Interface für alles was kämpfen kann.
/// </summary>
[GenerateDto(InheritInterfaces = false, DtoName = "CombatEntityDto")]
[GenerateDirtyTracking(IdPropertyName = "PersistentId")]
public interface ICombatEntity : IEntity
{
    // Identity
    string DisplayName { get; init; }
    
    [TrackDirty("Level")]
    int Level { get; set; }

    // Health & Resource
    [TrackDirty("Health")]
    int CurrentHealth { get; set; }
    
    [TrackDirty("MaxHealth")]
    int MaxHealth { get; init; }
    
    [TrackDirty("Resource")]
    int CurrentResource { get; set; }
    
    [TrackDirty("MaxResource")]
    int MaxResource { get; init; }
    
    CombatResourceType CombatResourceType { get; init; }

    // State
    [TrackDirty("State")]
    bool IsInCombat { get; set; }
    
    [TrackDirty("State")]
    Guid? TargetEntityId { get; set; }
    
    Faction Faction { get; init; }

    // Stats
    [TrackDirty("Velocity")]
    float MovementSpeed { get; set; }
}
