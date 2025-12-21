using Mmo.Server.EntityService.Records;
using Mmo.Shared.Entities.Interfaces;

namespace Mmo.Server.EntityService.Interfaces;

/// <summary>
///     Service for entity lifecycle management (spawning, despawning).
/// </summary>
public interface IEntityService
{
    /// <summary>
    ///     Spawns an entity in a zone.
    /// </summary>
    /// <param name="entity">The entity to spawn.</param>
    /// <param name="zoneId">The target zone ID.</param>
    /// <returns>Spawn result with LocalId and ZoneId on success.</returns>
    Task<SpawnResult> SpawnEntityAsync(IEntity entity, ushort zoneId);

    /// <summary>
    ///     Despawns an entity from its current zone.
    /// </summary>
    /// <param name="persistentId">The entity's persistent ID.</param>
    /// <returns>True if despawned successfully.</returns>
    Task<bool> DespawnEntityAsync(Guid persistentId);
}
