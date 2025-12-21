using Microsoft.Extensions.Logging;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.Zones.Interfaces;
using Mmo.Server.Zones.Records;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;

namespace Mmo.Server.Zones.Services;

public class ZoneService(ZoneManager zoneManager, IBroadcastService broadcast, ILog log)
    : IZoneService
{
    private readonly ILog _log = log;
    private readonly ZoneManager _zoneManager = zoneManager;
    private readonly IBroadcastService _broadcast = broadcast;

    public ZoneInfo? GetZoneInfo(ushort zoneId) => throw new NotImplementedException();

    public IEnumerable<ZoneInfo> GetAllZones() => throw new NotImplementedException();

    public bool ZoneExists(ushort zoneId) => throw new NotImplementedException();

    public ZoneTransferResult RequestZoneTransferAsync(Guid playerId, ushort targetZoneId,
        Position? targetPosition = null) => throw new NotImplementedException();

    public int GetPlayerCount(ushort zoneId) => throw new NotImplementedException();

    public IEnumerable<Guid> GetPlayersInZone(ushort zoneId) => throw new NotImplementedException();
}
