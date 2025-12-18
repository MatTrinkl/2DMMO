using Mmo.Shared.Enums;
using Mmo.Shared.Records;

public record CharacterInfo(
    Guid CharacterId,
    string Name,
    int Level,
    Race Race,
    CharacterClass Class,
    ushort ZoneId,
    Position LastPosition
);
