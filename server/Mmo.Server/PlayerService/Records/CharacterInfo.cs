using Mmo.Shared.Character.Enums;
using Mmo.Shared.Core.Records;

namespace Mmo.Server.PlayerService.Records;

/// <summary>
///     Represents basic information about a character for character selection.
/// </summary>
/// <param name="CharacterId">The unique ID of the character.</param>
/// <param name="Name">The character's name.</param>
/// <param name="Level">The character's level.</param>
/// <param name="Race">The character's race.</param>
/// <param name="Class">The character's class.</param>
/// <param name="ZoneId">The ID of the zone where the character last logged out.</param>
/// <param name="LastPosition">The character's last known position.</param>
public record CharacterInfo(
    Guid CharacterId,
    string Name,
    int Level,
    Race Race,
    CharacterClass Class,
    ushort ZoneId,
    Position LastPosition
);
