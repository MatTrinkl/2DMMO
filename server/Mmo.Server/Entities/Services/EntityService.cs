using Mmo.Server.Zones;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Records;

namespace Mmo.Server.Entities.Services;

/// <summary>
///     Service for managing and querying entities across zones.
///     Provides spatial queries and entity lifecycle management.
/// </summary>
public class EntityService : IEntityService
{
    private readonly ILog _log;
    private readonly ZoneManager _zoneManager;

    public EntityService(ZoneManager zoneManager, ILog log)
    {
        _zoneManager = zoneManager;
        _log = log;
    }

    public IEnumerable<BaseEntity> GetEntitiesInRange(ushort zoneId, Position center, float radius)
    {
        IEnumerable<BaseEntity> entitiesInZone = _zoneManager.GetEntitiesInZone(zoneId);

        // Filter entities by distance
        return entitiesInZone.Where(entity =>
        {
            float dx = entity.Position.X - center.X;
            float dy = entity.Position.Y - center.Y;
            float distanceSquared = dx * dx + dy * dy;
            return distanceSquared <= radius * radius;
        });
    }

    public SpawnResult SpawnEntity(BaseEntity entity, ushort zoneId)
    {
        Zone? zone = _zoneManager.GetZone(zoneId);
        if (zone == null) return SpawnResult.Failed("ZONE_NOT_FOUND");

        // 1. LocalId vergeben + RuntimeId setzen
        ushort localId = IdRegistry.Instance.GetNextLocalId(zoneId);
        entity.SetEntityId(localId, zoneId);

        // 2. In IdRegistry registrieren (Entity-Lookups)
        IdRegistry.Instance.RegisterEntity(entity);

        // 3. In Zone registrieren (nur ID)
        zone?.AddEntity(entity.PersistentId);

        _log.Debug("Entity spawned: {Type} {Id} in Zone {Zone}",
            entity.GetType().Name, entity.PersistentId, zoneId);

        return SpawnResult.Succeeded(entity.RuntimeId.LocalId, zoneId);
    }

    public bool DespawnEntity(Guid persistentId)
    {
        if (!IdRegistry.Instance.TryGetEntity(persistentId, out BaseEntity? entity))
            return false;

        ushort zoneId = entity.RuntimeId.ZoneId;

        // 1. Aus Zone entfernen
        Zone? zone = _zoneManager.GetZone(zoneId);
        zone?.RemoveEntity(persistentId);

        // 2. LocalId freigeben
        IdRegistry.Instance.ReleaseLocalId(zoneId, 0, entity.RuntimeId.LocalId);

        // 3. Aus IdRegistry entfernen
        IdRegistry.Instance.UnregisterEntity(persistentId);

        return true;
    }

    public IEnumerable<BaseEntity> GetVisibleEntities(Guid playerId)
    {
        // Get the player entity
        if (!IdRegistry.Instance.TryGetEntity(playerId, out BaseEntity? playerEntity))
            return [];

        // Get all entities in the same zone
        IEnumerable<BaseEntity> entitiesInZone = _zoneManager.GetEntitiesInZone(playerEntity.RuntimeId.ZoneId);

        // Return all entities except the player itself
        // In a full implementation, this could include visibility checks, distance, etc.
        return entitiesInZone.Where(e => e.PersistentId != playerId);
    }

    // Lookups delegieren an IdRegistry
    public BaseEntity? GetEntity(Guid persistentId)
    {
        IdRegistry.Instance.TryGetEntity(persistentId, out BaseEntity? entity);
        return entity;
    }

    public IEnumerable<BaseEntity> GetEntitiesInZone(ushort zoneId)
        => _zoneManager.GetEntitiesInZone(zoneId);
}
