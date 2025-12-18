using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     Abstrakte Basisklasse für alle Entities die kämpfen können.
///     Implementiert die gemeinsame Combat-Logik.
/// </summary>
[MessagePackObject]
public abstract class CombatEntity : ICombatEntity
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    [SerializationConstructor]
    protected CombatEntity()
    {
    }

    protected CombatEntity(Guid persistentId, Position position, ushort prefabId)
    {
        PersistentId = persistentId;
        Position = position;
        PrefabId = prefabId;
        RuntimeId = new EntityIdentity(1, 0, 0, 0, prefabId);
    }

    /// <summary>
    ///     Prefab type identifier - Public setter required for MessagePack deserialization.
    ///     Should be immutable after creation in production code.
    /// </summary>
    [Key(3)]
    public ushort PrefabId { get; set; }

    [Key(17)] public float BaseMovementSpeed { get; set; } = 5.0f;

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    [IgnoreMember] public float HealthPercent => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    [IgnoreMember] public float ResourcePercent => MaxResource > 0 ? (float)CurrentResource / MaxResource : 0f;
    // ═══════════════════════════════════════════════════════════════
    // IEntity Implementation
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Runtime identity - Public setter required for MessagePack deserialization.
    ///     Should only be modified via SetEntityId() or ChangeZone() in production code.
    /// </summary>
    [Key(0)]
    public EntityIdentity RuntimeId { get; set; }

    /// <summary>
    ///     Persistent GUID - Public setter required for MessagePack deserialization.
    ///     Should be immutable after creation in production code.
    /// </summary>
    [Key(1)]
    public Guid PersistentId { get; set; }

    [Key(2)] public Position Position { get; set; }

    [IgnoreMember] public abstract EntityType Type { get; }

    [IgnoreMember] public virtual bool IsTrulyPersistent => true;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity - Identity
    // ═══════════════════════════════════════════════════════════════

    [Key(4)] public string DisplayName { get; set; } = "";

    [Key(5)] public int Level { get; set; } = 1;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity - Health & Resource
    // ═══════════════════════════════════════════════════════════════

    [Key(6)] public int CurrentHealth { get; set; } = 100;

    [Key(7)] public int MaxHealth { get; set; } = 100;

    [Key(8)] public int CurrentResource { get; set; } = 100;

    [Key(9)] public int MaxResource { get; set; } = 100;

    [Key(10)] public virtual ResourceType ResourceType { get; set; } = ResourceType.None;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity - State
    // ═══════════════════════════════════════════════════════════════

    [Key(11)] public bool IsInCombat { get; set; }

    [Key(12)] public Guid? TargetEntityId { get; set; }

    [Key(13)] public Faction Faction { get; set; } = Faction.Neutral;

    [IgnoreMember] public virtual bool IsDead => CurrentHealth <= 0;

    [IgnoreMember] public virtual bool IsAttackable => !IsDead;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity - Stats
    // ═══════════════════════════════════════════════════════════════

    [Key(14)] public int AttackPower { get; set; } = 10;

    [Key(15)] public int Armor { get; set; } = 0;

    [Key(16)] public float MovementSpeed { get; set; } = 5.0f;

    // ═══════════════════════════════════════════════════════════════
    // IEntity METHODS
    // ═══════════════════════════════════════════════════════════════

    public void SetEntityId(int localId, ushort zoneId)
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

        // Armor Mitigation (nur für Physical)
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
    // PROTECTED HELPERS (für Subklassen zum Überschreiben)
    // ═══════════════════════════════════════════════════════════════

    protected virtual int CalculateArmorMitigation(int rawDamage)
    {
        // Simple formula:  Damage * (1 - Armor / (Armor + 100 * Level))
        float mitigation = Armor / (float)(Armor + 100 * Math.Max(1, Level));
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
