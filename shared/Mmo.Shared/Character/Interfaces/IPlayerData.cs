using Mmo.Shared.Character.Enums;
using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Combat.Records;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Movement.Enums;

namespace Mmo.Shared.Character.Interfaces;

/// <summary>
///     Interface for synchronized player data between PlayerEntity and PlayerEntityDto.
///     Contains all properties that are sent to the client.
///     Excludes server-only properties like Experience and Gold.
/// </summary>
public interface IPlayerData
{
    // From CombatEntity (Keys 0-17)
    EntityIdentity RuntimeId { get; }
    Guid PersistentId { get; }
    Position Position { get; }
    ushort PrefabId { get; }
    string DisplayName { get; }
    int Level { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
    int CurrentResource { get; }
    int MaxResource { get; }
    CombatResourceType CombatResourceType { get; }
    bool IsInCombat { get; }
    Guid? TargetEntityId { get; }
    Faction Faction { get; }
    int AttackPower { get; }
    int Armor { get; }
    float MovementSpeed { get; }
    float BaseMovementSpeed { get; }

    // From PlayerEntity (Keys 18-35, excluding ServerOnly: Experience=25, Gold=33)
    Guid CharacterId { get; }
    Guid AccountId { get; }
    Race Race { get; }
    CharacterClass Class { get; }
    Gender Gender { get; }
    string? Title { get; }
    bool IsPvpFlagged { get; }
    DateTime? PvpFlagExpires { get; }
    int HonorPoints { get; }
    int PvpKills { get; }
    int PvpDeaths { get; }
    CharacterState State { get; }
    MovementFlags MovementFlags { get; }
    BindLocation? HearthstoneLocation { get; }
    Position? LastSafePosition { get; }
}
