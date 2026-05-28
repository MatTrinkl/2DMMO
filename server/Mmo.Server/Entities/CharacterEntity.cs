using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Combat.Records;
using Mmo.Shared.Movement.Records;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Generators;
using Mmo.Shared.Movement.Enums;
using Mmo.Shared.Prefab;

namespace Mmo.Server.Entities;

/// <summary>
///     Represents a player character entity in the game world.
/// </summary>
public class CharacterEntity : CombatEntity, ICharacterEntity
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════


    public CharacterEntity(Guid characterId, Guid accountId, string displayName, Position position,
        EntityIdentity runtimeId)
        : base(characterId, position, PrefabIds.PlayerDefault, runtimeId)
    {
        CharacterId = characterId;
        AccountId = accountId;
        DisplayName = displayName;
    }

    public float LevelProgress => ExperienceToNextLevel > 0
        ? (float)Experience / ExperienceToNextLevel
        : 0f;

    protected override bool IsDead => State == CharacterState.Dead || CurrentHealth <= 0;

    public override bool IsAttackable => !IsDead && State != CharacterState.Ghost;

    public Guid AccountId { get; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Progression
    // ═══════════════════════════════════════════════════════════════

    [ServerOnly] public long Experience { get; set; }

    public long ExperienceToNextLevel => CalculateXpForLevel(Level + 1);

    public int HonorPoints { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Currency
    // ═══════════════════════════════════════════════════════════════

    [ServerOnly] public long Gold { get; set; } = 0;

    public DateTime? PvpFlagExpires { get; set; }

    public int PvpKills { get; set; }

    public int PvpDeaths { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Locations
    // ═══════════════════════════════════════════════════════════════

    public BindLocation? HearthstoneLocation { get; set; }

    public Position? LastSafePosition { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // IEntity Overrides
    // ═══════════════════════════════════════════════════════════════

    public override EntityType Type => EntityType.Player;

    // ═══════════════════════════════════════════════════════════════
    // ICombatEntity Overrides
    // ═══════════════════════════════════════════════════════════════

    public override CombatResourceType CombatResourceType { get; init; } = CombatResourceType.Mana;

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Identity
    // ═══════════════════════════════════════════════════════════════

    public Guid CharacterId { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - Character Info
    // ═══════════════════════════════════════════════════════════════

    public Race Race { get; init; } = Race.Human;

    public CharacterClass Class { get; init; } = CharacterClass.Warrior;

    public Gender Gender { get; init; } = Gender.Male;

    public string? Title { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - PvP
    // ═══════════════════════════════════════════════════════════════

    public bool IsPvpFlagged { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ICharacterEntity - State
    // ═══════════════════════════════════════════════════════════════

    public CharacterState State { get; set; } = CharacterState.Alive;

    public MovementFlags MovementFlags { get; set; } = MovementFlags.None;

    // ═══════════════════════════════════════════════════════════════
    // OVERRIDES
    // ═══════════════════════════════════════════════════════════════

    protected override bool IsHostileTo(ICombatEntity other)
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
            //LevelUp();
            return true;

        return false;
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
