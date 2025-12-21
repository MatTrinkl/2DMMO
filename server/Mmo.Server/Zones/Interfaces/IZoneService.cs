using Mmo.Server.Zones.Records;
using Mmo.Shared.Core.Records;

namespace Mmo.Server.Zones.Interfaces;

public interface IZoneService
{
    // Zone Queries
    ZoneInfo? GetZoneInfo(ushort zoneId);
    IEnumerable<ZoneInfo> GetAllZones();
    bool ZoneExists(ushort zoneId);

    // Zone Transitions (mit Validierung!)
    ZoneTransferResult RequestZoneTransferAsync(Guid playerId,
        ushort targetZoneId,
        Position? targetPosition = null);

    // Zone Population
    int GetPlayerCount(ushort zoneId);
    IEnumerable<Guid> GetPlayersInZone(ushort zoneId);
}
