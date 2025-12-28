using Mmo.Shared.Character.Enums;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Generators;

namespace Mmo.Shared.Entities.Interfaces;

/// <summary>
///     Interface für alles was kämpfen kann.
/// </summary>
[GenerateDto(InheritInterfaces = false, DtoName = "CombatEntityDto")]
public interface ICombatEntity : IEntity
{
    // Identity
    string DisplayName { get; init; }
    int Level { get; set; }

    // Health & Resource
    int CurrentHealth { get; set; }
    int MaxHealth { get; init; }
    int CurrentResource { get; set; }
    int MaxResource { get; init; }
    CombatResourceType CombatResourceType { get; init; }

    // State
    bool IsInCombat { get; set; }
    Guid? TargetEntityId { get; set; }
    Faction Faction { get; init; }

    // Stats
    float MovementSpeed { get; set; }
}
