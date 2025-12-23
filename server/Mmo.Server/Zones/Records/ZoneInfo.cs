namespace Mmo.Server.Zones.Records;

/// <summary>
///     Static information about a zone.
/// </summary>
/// <param name="ZoneId">Unique zone identifier.</param>
/// <param name="Name">Display name of the zone.</param>
/// <param name="RecommendedLevel">Recommended player level for this zone.</param>
/// <param name="IsPvP">Whether PvP is enabled in this zone.</param>
/// <param name="IsInstance">Whether this is an instanced zone.</param>
public record ZoneInfo(
    ushort ZoneId,
    string Name,
    int RecommendedLevel,
    bool IsPvP,
    bool IsInstance);
