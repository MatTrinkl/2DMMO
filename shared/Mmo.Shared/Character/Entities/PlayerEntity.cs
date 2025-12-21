using MessagePack;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Combat.Records;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums.Entities;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Prefab;
using Mmo.Shared.Records;

namespace Mmo.Shared.Character.Entities;

/// <summary>
///     Represents a player character entity in the game world.
/// </summary>
[MessagePackObject]
public class PlayerEntity : CombatEntity, ICharacterEntity
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    [SerializationConstructor]
    public PlayerEntity()
    {
    }

    public PlayerEntity(Guid characterId, Guid accountId, string displayName, Position position)
        : base(characterId, position, PrefabIds.PlayerDefault)
    {
        CharacterId = characterId;
        AccountId = accountId;
        DisplayName = displayName;
    }

    [IgnoreMember]
    public float LevelProgress => ExperienceToNextLevel > 0
        ? (float)Experience / ExperienceToNextLevel
        : 0f;

    [Key(27)] public DateTime? PvpFlagExpires { get; set; }

    [Key(29)] public int PvpKills { get; set; }

    [Key(30)] public int PvpDeaths { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Locations
    // ═══════════════════════════════════════════════════════════════

    [Key(34)] public BindLocation? HearthstoneLocation { get; set; }

    [Key(35)] public Position? LastSafePosition { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // IEntity Overrides
    // ═══════════════════════════════════════════════════════════════

    [IgnoreMember] public override EntityType Type => EntityType.Player;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity Overrides
    // ═══════════════════════════════════════════════════════════════

    [Key(18)] public override CombatResourceType CombatResourceType { get; set; } = CombatResourceType.Mana;

    [IgnoreMember] public override bool IsDead => State == CharacterState.Dead || CurrentHealth <= 0;

    [IgnoreMember] public override bool IsAttackable => !IsDead && State != CharacterState.Ghost;

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Identity
    // ═══════════════════════════════════════════════════════════════

    [Key(19)] public Guid CharacterId { get; }

    [Key(20)] public Guid AccountId { get; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Character Info
    // ═══════════════════════════════════════════════════════════════

    [Key(21)] public Race Race { get; set; } = Race.Human;

    [Key(22)] public CharacterClass Class { get; set; } = CharacterClass.Warrior;

    [Key(23)] public Gender Gender { get; set; } = Gender.Male;

    [Key(24)] public string? Title { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Progression
    // ═══════════════════════════════════════════════════════════════

    [Key(25)] public long Experience { get; set; }

    [IgnoreMember] public long ExperienceToNextLevel => CalculateXpForLevel(Level + 1);

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - PvP
    // ═══════════════════════════════════════════════════════════════

    [Key(26)] public bool IsPvpFlagged { get; set; }

    [Key(28)] public int HonorPoints { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - State
    // ═══════════════════════════════════════════════════════════════

    [Key(31)] public CharacterState State { get; set; } = CharacterState.Alive;

    [Key(32)] public MovementFlags MovementFlags { get; set; } = MovementFlags.None;

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Currency
    // ═══════════════════════════════════════════════════════════════

    [Key(33)] public long Gold { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════════
    // OVERRIDES
    // ═══════════════════════════════════════════════════════════════

    public override bool IsHostileTo(ICombatEntity other)
    {
        // Basis-Check
        if (base.IsHostileTo(other))
            return true;

        // PvP-Check für andere Spieler
        if (other is ICharacterEntity otherPlayer)
            // Beide müssen PvP-flagged sein
            return IsPvpFlagged && otherPlayer.IsPvpFlagged;

        return false;
    }

    public override void ChangeZone(ushort newZoneId)
    {
        LastSafePosition = Position;
        base.ChangeZone(newZoneId);
    }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity METHODS
    // ═══════════════════════════════════════════════════════════════

    public bool GainExperience(long amount)
    {
        if (amount <= 0) return false;

        Experience += amount;

        if (Experience >= ExperienceToNextLevel)
        {
            LevelUp();
            return true;
        }

        return false;
    }

    public void LevelUp()
    {
        Experience -= ExperienceToNextLevel;
        Level++;

        // Stats erhöhen
        int healthGain = 10 + Level * 2;
        int resourceGain = 5 + Level;

        MaxHealth += healthGain;
        MaxResource += resourceGain;

        // Full heal on level up
        CurrentHealth = MaxHealth;
        CurrentResource = MaxResource;

        AttackPower += 2;
    }

    public void Respawn(Position position)
    {
        State = CharacterState.Alive;
        Position = position;
        CurrentHealth = MaxHealth / 2;
        CurrentResource = MaxResource / 2;
        IsInCombat = false;
        TargetEntityId = null;
    }

    protected override void OnDeath(ICombatEntity? killer) => State = CharacterState.Dead;

    public void EnablePvpFlag(int durationSeconds = 300)
    {
        IsPvpFlagged = true;
        PvpFlagExpires = durationSeconds > 0
            ? DateTime.UtcNow.AddSeconds(durationSeconds)
            : null;
    }

    public void DisablePvpFlag()
    {
        IsPvpFlagged = false;
        PvpFlagExpires = null;
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static long CalculateXpForLevel(int level)
    {
        // XP Curve: 100 * level^2
        return 100L * level * level;
    }
}
