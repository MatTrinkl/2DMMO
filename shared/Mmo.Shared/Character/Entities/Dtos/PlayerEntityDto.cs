using MessagePack;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Combat.Records;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Movement.Enums;

namespace Mmo.Shared.Character.Entities.Dtos;

/// <summary>
///     DTO for PlayerEntity network synchronization.
///     Contains all client-visible properties, excluding server-only data like Experience and Gold.
/// </summary>
[MessagePackObject]
public sealed class PlayerEntityDto : IPlayerData
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    [SerializationConstructor]
    public PlayerEntityDto()
    {
        Position = null!; // Will be set by MessagePack or FromEntity
    }

    // ═══════════════════════════════════════════════════════════════
    // From CombatEntity (Keys 0-17)
    // ═══════════════════════════════════════════════════════════════

    [Key(0)] public EntityIdentity RuntimeId { get; set; }

    [Key(1)] public Guid PersistentId { get; set; }

    [Key(2)] public Position Position { get; set; }

    [Key(3)] public ushort PrefabId { get; set; }

    [Key(4)] public string DisplayName { get; set; } = "";

    [Key(5)] public int Level { get; set; }

    [Key(6)] public int CurrentHealth { get; set; }

    [Key(7)] public int MaxHealth { get; set; }

    [Key(8)] public int CurrentResource { get; set; }

    [Key(9)] public int MaxResource { get; set; }

    [Key(10)] public CombatResourceType CombatResourceType { get; set; }

    [Key(11)] public bool IsInCombat { get; set; }

    [Key(12)] public Guid? TargetEntityId { get; set; }

    [Key(13)] public Faction Faction { get; set; }

    [Key(14)] public int AttackPower { get; set; }

    [Key(15)] public int Armor { get; set; }

    [Key(16)] public float MovementSpeed { get; set; }

    [Key(17)] public float BaseMovementSpeed { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // From PlayerEntity (Keys 19-35, excluding 25=Experience, 33=Gold)
    // Note: PlayerEntity has Key(18) for CombatResourceType override,
    // but this DTO is a flat structure (not inheriting), so we skip
    // Key(18) and continue with PlayerEntity-specific keys starting at 19.
    // ═══════════════════════════════════════════════════════════════

    [Key(19)] public Guid CharacterId { get; set; }

    [Key(20)] public Guid AccountId { get; set; }

    [Key(21)] public Race Race { get; set; }

    [Key(22)] public CharacterClass Class { get; set; }

    [Key(23)] public Gender Gender { get; set; }

    [Key(24)] public string? Title { get; set; }

    // Key(25) = Experience - SKIPPED (ServerOnly)

    [Key(26)] public bool IsPvpFlagged { get; set; }

    [Key(27)] public DateTime? PvpFlagExpires { get; set; }

    [Key(28)] public int HonorPoints { get; set; }

    [Key(29)] public int PvpKills { get; set; }

    [Key(30)] public int PvpDeaths { get; set; }

    [Key(31)] public CharacterState State { get; set; }

    [Key(32)] public MovementFlags MovementFlags { get; set; }

    // Key(33) = Gold - SKIPPED (ServerOnly)

    [Key(34)] public BindLocation? HearthstoneLocation { get; set; }

    [Key(35)] public Position? LastSafePosition { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // MAPPING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Creates a DTO from a PlayerEntity, excluding server-only properties.
    /// </summary>
    public static PlayerEntityDto FromEntity(PlayerEntity entity)
    {
        return new PlayerEntityDto
        {
            // From CombatEntity
            RuntimeId = entity.RuntimeId,
            PersistentId = entity.PersistentId,
            Position = entity.Position,
            PrefabId = entity.PrefabId,
            DisplayName = entity.DisplayName,
            Level = entity.Level,
            CurrentHealth = entity.CurrentHealth,
            MaxHealth = entity.MaxHealth,
            CurrentResource = entity.CurrentResource,
            MaxResource = entity.MaxResource,
            CombatResourceType = entity.CombatResourceType,
            IsInCombat = entity.IsInCombat,
            TargetEntityId = entity.TargetEntityId,
            Faction = entity.Faction,
            AttackPower = entity.AttackPower,
            Armor = entity.Armor,
            MovementSpeed = entity.MovementSpeed,
            BaseMovementSpeed = entity.BaseMovementSpeed,

            // From PlayerEntity
            CharacterId = entity.CharacterId,
            AccountId = entity.AccountId,
            Race = entity.Race,
            Class = entity.Class,
            Gender = entity.Gender,
            Title = entity.Title,
            // Experience - SKIPPED (ServerOnly)
            IsPvpFlagged = entity.IsPvpFlagged,
            PvpFlagExpires = entity.PvpFlagExpires,
            HonorPoints = entity.HonorPoints,
            PvpKills = entity.PvpKills,
            PvpDeaths = entity.PvpDeaths,
            State = entity.State,
            MovementFlags = entity.MovementFlags,
            // Gold - SKIPPED (ServerOnly)
            HearthstoneLocation = entity.HearthstoneLocation,
            LastSafePosition = entity.LastSafePosition
        };
    }
}
