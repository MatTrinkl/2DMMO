using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Records;

namespace Mmo.Shared.Interfaces;

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
