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
    
    [TrackedProperty(EntityDirtyFlags.Level)]
    int Level { get; set; }

    // Health & Resource
    [TrackedProperty(EntityDirtyFlags.Health)]
    int CurrentHealth { get; set; }
    
    [TrackedProperty(EntityDirtyFlags.MaxHealth)]
    int MaxHealth { get; init; }
    
    [TrackedProperty(EntityDirtyFlags.Resource)]
    int CurrentResource { get; set; }
    
    [TrackedProperty(EntityDirtyFlags.MaxResource)]
    int MaxResource { get; init; }
    
    CombatResourceType CombatResourceType { get; init; }

    // State
    [TrackedProperty(EntityDirtyFlags.State)]
    bool IsInCombat { get; set; }
    
    [TrackedProperty(EntityDirtyFlags.State)]
    Guid? TargetEntityId { get; set; }
    
    Faction Faction { get; init; }

    // Stats
    [TrackedProperty(EntityDirtyFlags.Velocity)]
    float MovementSpeed { get; set; }
}
