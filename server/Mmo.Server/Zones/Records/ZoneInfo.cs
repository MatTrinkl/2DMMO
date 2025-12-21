namespace Mmo.Server.Zones.Records;

public record ZoneInfo(
    ushort ZoneId,
    string Name,
    int RecommendedLevel,
    bool IsPvP,
    bool IsInstance);
