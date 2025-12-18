using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Server.Services.Player;

/// <summary>
///     Daten für einen Charakter (aus DB geladen).
/// </summary>
public record CharacterData(
    Guid CharacterId,
    string Name,
    int Level,
    Race Race,
    CharacterClass Class,
    Gender Gender,
    Faction Faction,
    ushort ZoneId,
    Position Position,
    int MaxHealth,
    int CurrentHealth,
    int MaxResource,
    int CurrentResource
);
