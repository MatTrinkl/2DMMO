using Mmo.Server.Zones;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Entities.Records;

namespace Mmo.Server.Entities.Services;

public class EntityService :  IEntityService
{
    private readonly ZoneManager _zoneManager;
    private readonly ILog _log;

    public EntityService(ZoneManager zoneManager, ILog log)
    {
        _zoneManager = zoneManager;
        _log = log;
    }

    public IEnumerable<IEntity> GetEntitiesInRange(ushort zoneId, Position center, float radius)
    {
        var entitiesInZone = _zoneManager.GetEntitiesInZone(zoneId);
        
        // Filter entities by distance
        return entitiesInZone.Where(entity =>
        {
            float dx = entity.Position.X - center.X;
            float dy = entity.Position.Y - center.Y;
            float distanceSquared = dx * dx + dy * dy;
            return distanceSquared <= radius * radius;
        });
    }

    public SpawnResult SpawnEntity(IEntity entity, ushort zoneId)
    {
        var zone = _zoneManager.GetZone(zoneId);
        if (zone == null)
        {
            return SpawnResult.Failed("ZONE_NOT_FOUND");
        }

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
        if (!IdRegistry.Instance.TryGetEntity(persistentId, out var entity))
            return false;

        ushort zoneId = entity.RuntimeId.ZoneId;

        // 1. Aus Zone entfernen
        var zone = _zoneManager.GetZone(zoneId);
        zone?.RemoveEntity(persistentId);

        // 2. LocalId freigeben
        IdRegistry.Instance.ReleaseLocalId(zoneId, 0, entity.RuntimeId.LocalId);

        // 3. Aus IdRegistry entfernen
        IdRegistry.Instance.UnregisterEntity(persistentId);

        return true;
    }

    public IEnumerable<IEntity> GetVisibleEntities(Guid playerId)
    {
        // Get the player entity
        if (!IdRegistry.Instance.TryGetEntity(playerId, out var playerEntity))
            return [];

        // Get all entities in the same zone
        var entitiesInZone = _zoneManager.GetEntitiesInZone(playerEntity.RuntimeId.ZoneId);
        
        // Return all entities except the player itself
        // In a full implementation, this could include visibility checks, distance, etc.
        return entitiesInZone.Where(e => e.PersistentId != playerId);
    }

    // Lookups delegieren an IdRegistry
    public IEntity? GetEntity(Guid persistentId)
    {
        IdRegistry.Instance.TryGetEntity(persistentId, out var entity);
        return entity;
    }

    public IEnumerable<IEntity> GetEntitiesInZone(ushort zoneId)
        => _zoneManager.GetEntitiesInZone(zoneId);
}
