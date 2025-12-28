using Mmo.Shared.Character.Enums;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Generators;
using Mmo.Shared.Movement.Enums;

namespace Mmo.Shared.Character.Interfaces;

/// <summary>
///     Interface für Spieler-Charaktere.
/// </summary>
[GenerateDto(InheritInterfaces = false, DtoName = "CharacterEntityDto")]
[DtoUnionMember(0, typeof(IEntity))]
public interface ICharacterEntity : ICombatEntity
{
    // Character Identity
    Guid CharacterId { get; init; }
    Race Race { get; init; }
    CharacterClass Class { get; init; }
    Gender Gender { get; init; }
    string? Title { get; init; }

    // PvP
    bool IsPvpFlagged { get; set; }

    // State
    CharacterState State { get; set; }
    MovementFlags MovementFlags { get; set; }
}
