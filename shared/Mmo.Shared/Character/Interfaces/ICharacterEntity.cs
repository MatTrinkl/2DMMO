using Mmo.Shared.Character.Enums;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Movement.Enums;

namespace Mmo.Shared.Character.Interfaces;

/// <summary>
///     Interface für Spieler-Charaktere.
/// </summary>
public interface ICharacterEntity : ICombatEntity
{
    // Character Identity
    Guid CharacterId { get; }
    Guid AccountId { get; }
    Race Race { get; }
    CharacterClass Class { get; }
    Gender Gender { get; }
    string? Title { get; set; }

    // Progression
    long Experience { get; set; }
    long ExperienceToNextLevel { get; }

    // PvP
    bool IsPvpFlagged { get; set; }
    int HonorPoints { get; set; }

    // State
    CharacterState State { get; set; }
    MovementFlags MovementFlags { get; set; }

    // Currency
    long Gold { get; set; }

    // Methods
    bool GainExperience(long amount);
    void LevelUp();
    void Respawn(Position position);
}
