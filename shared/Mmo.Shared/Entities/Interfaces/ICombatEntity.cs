using Mmo.Shared.Character.Enums;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Combat.Records;
using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     Interface für alles was kämpfen kann.
/// </summary>
public interface ICombatEntity : IEntity
{
    // Identity
    string DisplayName { get; }
    int Level { get; set; }

    // Health & Resource
    int CurrentHealth { get; set; }
    int MaxHealth { get; set; }
    int CurrentResource { get; set; }
    int MaxResource { get; set; }
    CombatResourceType CombatResourceType { get; }

    // State
    bool IsInCombat { get; set; }
    bool IsDead { get; }
    bool IsAttackable { get; }
    Guid? TargetEntityId { get; set; }
    Faction Faction { get; set; }

    // Stats
    int AttackPower { get; }
    int Armor { get; }
    float MovementSpeed { get; set; }

    // Methods
    bool IsHostileTo(ICombatEntity other);
    DamageResult TakeDamage(int damage, DamageType damageType, ICombatEntity? source);
    HealResult ReceiveHeal(int amount, ICombatEntity? source);
    void Die(ICombatEntity? killer);
    void EnterCombat(ICombatEntity? enemy);
    void LeaveCombat();
}
