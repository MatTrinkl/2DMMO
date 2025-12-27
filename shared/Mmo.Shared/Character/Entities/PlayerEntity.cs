using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Combat.Entities;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Combat.Records;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Generators;
using Mmo.Shared.Movement.Enums;
using Mmo.Shared.Prefab;

namespace Mmo.Shared.Character.Entities;

/// <summary>
///     Represents a player character entity in the game world.
/// </summary>
[GenerateDto]
[DtoImplements(typeof(IPlayerData))]
public class PlayerEntity : CombatEntity, ICharacterEntity, IPlayerData
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════


    public PlayerEntity(Guid characterId, Guid accountId, string displayName, Position position)
        : base(characterId, position, PrefabIds.PlayerDefault)
    {
        CharacterId = characterId;
        AccountId = accountId;
        DisplayName = displayName;
    }

    public float LevelProgress => ExperienceToNextLevel > 0
        ? (float)Experience / ExperienceToNextLevel
        : 0f;

    // ═══════════════════════════════════════════════════════════════
    // IEntity Overrides
    // ═══════════════════════════════════════════════════════════════

    public override EntityType Type => EntityType.Player;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity Overrides
    // ═══════════════════════════════════════════════════════════════

    public override CombatResourceType CombatResourceType { get; set; } = CombatResourceType.Mana;

    public override bool IsDead => State == CharacterState.Dead || CurrentHealth <= 0;

    public override bool IsAttackable => !IsDead && State != CharacterState.Ghost;

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Identity
    // ═══════════════════════════════════════════════════════════════

    public Guid CharacterId { get; }

    public Guid AccountId { get; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Character Info
    // ═══════════════════════════════════════════════════════════════

    public Race Race { get; set; } = Race.Human;

    public CharacterClass Class { get; set; } = CharacterClass.Warrior;

    public Gender Gender { get; set; } = Gender.Male;

    public string? Title { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Progression
    // ═══════════════════════════════════════════════════════════════

    [ServerOnly] public long Experience { get; set; }

    public long ExperienceToNextLevel => CalculateXpForLevel(Level + 1);

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - PvP
    // ═══════════════════════════════════════════════════════════════

    public bool IsPvpFlagged { get; set; }

    public int HonorPoints { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - State
    // ═══════════════════════════════════════════════════════════════

    public CharacterState State { get; set; } = CharacterState.Alive;

    public MovementFlags MovementFlags { get; set; } = MovementFlags.None;

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Currency
    // ═══════════════════════════════════════════════════════════════

    [ServerOnly] public long Gold { get; set; } = 0;

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

    public DateTime? PvpFlagExpires { get; set; }

    public int PvpKills { get; set; }

    public int PvpDeaths { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Locations
    // ═══════════════════════════════════════════════════════════════

    public BindLocation? HearthstoneLocation { get; set; }

    public Position? LastSafePosition { get; set; }

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
