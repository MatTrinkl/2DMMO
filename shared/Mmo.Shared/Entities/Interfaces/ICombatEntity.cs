using Mmo.Shared.Character.Enums;
using Mmo.Shared.Combat.Enums;
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
    
    [TrackedProperty(DirtyFlags.Level)]
    int Level { get; set; }

    // Health & Resource
    [TrackedProperty(DirtyFlags.Health)]
    int CurrentHealth { get; set; }
    
    [TrackedProperty(DirtyFlags.MaxHealth)]
    int MaxHealth { get; init; }
    
    [TrackedProperty(DirtyFlags.Resource)]
    int CurrentResource { get; set; }
    
    [TrackedProperty(DirtyFlags.MaxResource)]
    int MaxResource { get; init; }
    
    CombatResourceType CombatResourceType { get; init; }

    // State
    [TrackedProperty(DirtyFlags.State)]
    bool IsInCombat { get; set; }
    
    [TrackedProperty(DirtyFlags.State)]
    Guid? TargetEntityId { get; set; }
    
    Faction Faction { get; init; }

    // Stats
    [TrackedProperty(DirtyFlags.Velocity)]
    float MovementSpeed { get; set; }
}
