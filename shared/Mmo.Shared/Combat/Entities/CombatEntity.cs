using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Combat.Records;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Entities.Structs;

namespace Mmo.Shared.Combat.Entities;

/// <summary>
///     Abstract base class for all entities that can fight.
///     Implements the shared combat logic.
/// </summary>
public abstract class CombatEntity(Guid persistentId, Position position, ushort prefabId) : ICombatEntity
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════


    /// <summary>
    ///     Prefab type identifier - Public setter required for MessagePack deserialization.
    ///     Should be immutable after creation in production code.
    /// </summary>
    public ushort PrefabId { get; set; } = prefabId;

    public float BaseMovementSpeed { get; set; } = 5.0f;

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    public float HealthPercent => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    public float ResourcePercent => MaxResource > 0 ? (float)CurrentResource / MaxResource : 0f;
    // ═══════════════════════════════════════════════════════════════
    // IEntity Implementation
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Runtime identity - Public setter required for MessagePack deserialization.
    ///     Should only be modified via SetEntityId() or ChangeZone() in production code.
    /// </summary>
    public EntityIdentity RuntimeId { get; set; } = new(1, 0, 0, 0, prefabId);

    /// <summary>
    ///     Persistent GUID - Public setter required for MessagePack deserialization.
    ///     Should be immutable after creation in production code.
    /// </summary>
    public Guid PersistentId { get; set; } = persistentId;

    public Position Position { get; set; } = position;
    public abstract EntityType Type { get; }
    public virtual bool IsTrulyPersistent => true;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity - Identity
    // ═══════════════════════════════════════════════════════════════
    public string DisplayName { get; set; } = "";
    public int Level { get; set; } = 1;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity - Health & Resource
    // ═══════════════════════════════════════════════════════════════
    public int CurrentHealth { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int CurrentResource { get; set; } = 100;
    public int MaxResource { get; set; } = 100;
    public virtual CombatResourceType CombatResourceType { get; set; } = CombatResourceType.None;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity - State
    // ═══════════════════════════════════════════════════════════════
    public bool IsInCombat { get; set; }
    public Guid? TargetEntityId { get; set; }
    public Faction Faction { get; set; } = Faction.Neutral;
    public virtual bool IsDead => CurrentHealth <= 0;
    public virtual bool IsAttackable => !IsDead;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity - Stats
    // ═══════════════════════════════════════════════════════════════
    public int AttackPower { get; set; } = 10;
    public int Armor { get; set; } = 0;
    public float MovementSpeed { get; set; } = 5.0f;

    // ═══════════════════════════════════════════════════════════════
    // IEntity METHODS
    // ═══════════════════════════════════════════════════════════════

    public void SetEntityId(ushort localId, ushort zoneId)
    {
        RuntimeId = new EntityIdentity(
            RuntimeId.ServerId,
            zoneId,
            RuntimeId.ShardId,
            localId,
            PrefabId
        );
    }


    public virtual void ChangeZone(ushort newZoneId)
    {
        RuntimeId = new EntityIdentity(
            RuntimeId.ServerId,
            newZoneId,
            0,
            0,
            PrefabId
        );
    }

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity METHODS - Gemeinsame Implementierung!
    // ═══════════════════════════════════════════════════════════════

    public virtual bool IsHostileTo(ICombatEntity other)
    {
        if (other.Faction == Faction.Monster && this is ICharacterEntity)
            return true;
        if (Faction == Faction.Monster && other is ICharacterEntity)
            return true;
        if (Faction == other.Faction)
            return false;

        return Faction != other.Faction && Faction != Faction.Neutral && other.Faction != Faction.Neutral;
    }

    public virtual DamageResult TakeDamage(int damage, DamageType damageType, ICombatEntity? source)
    {
        if (IsDead)
            return new DamageResult(
                damage,
                0,
                0,
                0,
                0,
                false,
                false,
                false,
                false,
                false,
                true,
                damageType
            );

        // Armor Mitigation (only for Physical)
        int mitigated = damageType == DamageType.Physical
            ? CalculateArmorMitigation(damage)
            : damage;

        // Actual damage dealt
        int actualDamage = Math.Min(mitigated, CurrentHealth);
        int overkill = Math.Max(0, mitigated - CurrentHealth);

        CurrentHealth -= actualDamage;

        // Enter combat
        if (source != null) EnterCombat(source);

        // Check death
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die(source);
        }

        return new DamageResult(
            damage,
            mitigated,
            0,
            actualDamage,
            overkill,
            false,
            false,
            false,
            false,
            false,
            false,
            damageType
        );
    }

    public virtual HealResult ReceiveHeal(int amount, ICombatEntity? source)
    {
        if (IsDead)
            return new HealResult(
                amount,
                0,
                amount,
                false
            );

        int actualHeal = Math.Min(amount, MaxHealth - CurrentHealth);
        int overheal = amount - actualHeal;

        CurrentHealth += actualHeal;

        return new HealResult(
            amount,
            actualHeal,
            overheal,
            false
        );
    }

    public virtual void Die(ICombatEntity? killer)
    {
        CurrentHealth = 0;
        IsInCombat = false;
        TargetEntityId = null;
        OnDeath(killer);
    }

    public virtual void EnterCombat(ICombatEntity? enemy)
    {
        if (!IsInCombat)
        {
            IsInCombat = true;
            OnEnterCombat(enemy);
        }
    }

    public virtual void LeaveCombat()
    {
        if (IsInCombat)
        {
            IsInCombat = false;
            OnLeaveCombat();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PROTECTED HELPERS (for subclasses to override)
    // ═══════════════════════════════════════════════════════════════

    protected virtual int CalculateArmorMitigation(int rawDamage)
    {
        // Simple formula:  Damage * (1 - Armor / (Armor + 100 * Level))
        float mitigation = (float)Armor / (Armor + 100 * Math.Max(1, Level));
        return (int)(rawDamage * (1 - mitigation));
    }

    protected virtual void OnDeath(ICombatEntity? killer)
    {
    }

    protected virtual void OnEnterCombat(ICombatEntity? enemy)
    {
    }

    protected virtual void OnLeaveCombat()
    {
    }
}
