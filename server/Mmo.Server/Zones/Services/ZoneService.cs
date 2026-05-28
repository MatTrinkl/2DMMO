using Mmo.Server.Entities;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.Zones.Interfaces;
using Mmo.Server.Zones.Records;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Movement.Records;

namespace Mmo.Server.Zones.Services;

/// <summary>
///     Service for managing zone information, transitions, and population tracking.
/// </summary>
public class ZoneService(ZoneManager zoneManager, IBroadcastService broadcast, ILog log)
    : IZoneService
{
    private readonly IBroadcastService _broadcast = broadcast;
    private readonly ILog _log = log;
    private readonly ZoneManager _zoneManager = zoneManager;

    public ZoneInfo? GetZoneInfo(ushort zoneId)
    {
        Zone? zone = _zoneManager.GetZone(zoneId);
        if (zone == null)
            return null;

        // Using available data from Zone struct
        // TODO: Add RecommendedLevel, IsPvP, IsInstance to Zone configuration
        return new ZoneInfo(
            zone.Value.ZoneId,
            zone.Value.ZoneName,
            1, // Default value, should come from config
            false, // Default value, should come from config
            false // Default value, should come from config
        );
    }

    public IEnumerable<ZoneInfo> GetAllZones()
    {
        // ZoneManager doesn't expose all zones, so we can't implement this without refactoring
        // For now, return empty - this would need ZoneManager to expose a GetAllZones() method
        _log.Warn("GetAllZones called but ZoneManager doesn't expose all zones");
        return [];
    }

    public bool ZoneExists(ushort zoneId) => _zoneManager.ZoneExists(zoneId);

    public ZoneTransferResult RequestZoneTransferAsync(Guid playerId, ushort targetZoneId,
        Position? targetPosition = null)
    {
        // 1. Validate target zone exists
        if (!_zoneManager.ZoneExists(targetZoneId))
            return new ZoneTransferResult
            {
                Success = false,
                Error = "TARGET_ZONE_NOT_FOUND"
            };

        // 2. Get player entity
        if (!IdRegistry.Instance.TryGetEntity(playerId, out BaseEntity? entity))
            return new ZoneTransferResult
            {
                Success = false,
                Error = "PLAYER_NOT_FOUND"
            };

        ushort oldZoneId = entity.RuntimeId.ZoneId;

        // 3. Remove from old zone
        Zone? oldZone = _zoneManager.GetZone(oldZoneId);
        oldZone?.RemoveEntity(playerId);

        // 4. Release old LocalId
        IdRegistry.Instance.ReleaseLocalId(oldZoneId, 0, entity.RuntimeId.LocalId);

        // 5. Get new LocalId and assign to new zone
        ushort newLocalId = IdRegistry.Instance.GetNextLocalId(targetZoneId);
        entity.SetEntityId(newLocalId, targetZoneId);

        // 6. Add to new zone
        Zone? newZone = _zoneManager.GetZone(targetZoneId);
        newZone?.AddEntity(playerId);

        // 7. Update GlobalKey in IdRegistry
        IdRegistry.Instance.UpdateEntityGlobalKey(entity,
            (long)entity.RuntimeId.ServerId << 56 | (long)oldZoneId << 40 | (long)entity.RuntimeId.ShardId << 24 |
            entity.RuntimeId.LocalId);

        // 8. Set spawn position if provided
        Position spawnPos = targetPosition ?? new Position(100, 100); // Default spawn
        entity.Position = spawnPos;

        _log.Info("Player {PlayerId} transferred from zone {OldZone} to {NewZone}",
            playerId, oldZoneId, targetZoneId);

        return new ZoneTransferResult
        {
            Success = true,
            NewZoneId = targetZoneId,
            SpawnPosition = spawnPos
        };
    }

    public int GetPlayerCount(ushort zoneId)
    {
        Zone? zone = _zoneManager.GetZone(zoneId);
        return zone?.EntityCount ?? 0;
    }

    public IEnumerable<Guid> GetPlayersInZone(ushort zoneId)
    {
        Zone? zone = _zoneManager.GetZone(zoneId);
        if (zone == null)
            return [];

        return zone.Value.GetEntityIds();
    }
}
