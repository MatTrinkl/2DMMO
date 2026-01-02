using MessagePack;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Networking;

namespace Mmo.Shared.Character.Records;

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
/// <param name="LastPlayed">
///     The time in milliseconds when the character was played the last time. Use
///     <see cref="NetworkTime.ToDateTime(long)" />
/// </param>
/// for conversion.
/// <param name="PendingDeletion">Is true if the character is currently soft deleted.</param>
/// <param name="DeletionTime">
///     The time in milliseconds when the character was deleted (24 hours soft deletion). Use
///     <see cref="NetworkTime.ToDateTime(long)" />
/// </param>
/// for conversion.
/// </param>
[MessagePackObject]
public record CharacterInfo(
    [property: Key(0)] Guid CharacterId,
    [property: Key(1)] string Name,
    [property: Key(2)] int Level,
    [property: Key(3)] Race Race,
    [property: Key(4)] CharacterClass Class,
    [property: Key(5)] ushort ZoneId,
    [property: Key(6)] Position LastPosition,
    [property: Key(7)] long LastPlayed,
    [property: Key(8)] bool PendingDeletion,
    [property: Key(9)] long DeletionTime
);
