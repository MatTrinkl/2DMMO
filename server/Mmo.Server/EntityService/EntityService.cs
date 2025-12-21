using Mmo.Server.EntityService.Interfaces;
using Mmo.Server.EntityService.Records;
using Mmo.Server.Zones;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.EntityService;

/// <summary>
///     Implementation of IEntityService.
///     Handles entity lifecycle (spawn/despawn) using ZoneManager and IdRegistry.
/// </summary>
public class EntityService : IEntityService
{
    private readonly IdRegistry _idRegistry;
    private readonly ILog _log;
    private readonly ZoneManager _zoneManager;

    public EntityService(ZoneManager zoneManager, ILog log)
    {
        _zoneManager = zoneManager ?? throw new ArgumentNullException(nameof(zoneManager));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _idRegistry = IdRegistry.Instance;
    }

    /// <inheritdoc />
    public Task<SpawnResult> SpawnEntityAsync(IEntity entity, ushort zoneId)
    {
        if (entity == null)
            return Task.FromResult(new SpawnResult(false, Error: "Entity is null"));

        try
        {
            // Get zone
            Zone? zone = _zoneManager.GetZone(zoneId);
            if (zone == null)
                return Task.FromResult(new SpawnResult(false, Error: $"Zone {zoneId} not found"));

            // Add to zone (this allocates LocalId via SetEntityId)
            _zoneManager.AddEntity(entity, zoneId);

            // Register entity in IdRegistry
            _idRegistry.RegisterEntity(entity);

            int localId = entity.RuntimeId.LocalId;

            _log.Debug("Spawned entity {PersistentId} as LocalId {LocalId} in Zone {ZoneId}",
                entity.PersistentId, localId, zoneId);

            return Task.FromResult(new SpawnResult(true, localId, zoneId));
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to spawn entity {PersistentId}", entity.PersistentId);
            return Task.FromResult(new SpawnResult(false, Error: ex.Message));
        }
    }

    /// <inheritdoc />
    public Task<bool> DespawnEntityAsync(Guid persistentId)
    {
        try
        {
            // Get entity
            if (!_idRegistry.TryGetEntity(persistentId, out IEntity? entity))
            {
                _log.Warn("Cannot despawn entity {PersistentId} - not found", persistentId);
                return Task.FromResult(false);
            }

            // Remove from zone
            IEntity? removed = _zoneManager.RemoveEntity(persistentId);
            if (removed == null)
            {
                _log.Warn("Cannot despawn entity {PersistentId} - not in any zone", persistentId);
            }

            // Release LocalId
            if (entity.RuntimeId.IsAssigned)
                _idRegistry.ReleaseLocalId(entity.RuntimeId.ZoneId, entity.RuntimeId.ShardId,
                    entity.RuntimeId.LocalId);

            // Unregister entity
            _idRegistry.UnregisterEntity(persistentId);

            _log.Debug("Despawned entity {PersistentId} from zone {ZoneId}",
                persistentId, entity.RuntimeId.ZoneId);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to despawn entity {PersistentId}", persistentId);
            return Task.FromResult(false);
        }
    }
}
