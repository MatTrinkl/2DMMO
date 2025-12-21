using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Records;

namespace Mmo.Shared.Entities.Interfaces;

public interface IEntityService
{
    // Entity Queries
    IEntity?  GetEntity(Guid persistentId);
    IEnumerable<IEntity> GetEntitiesInZone(ushort zoneId);
    IEnumerable<IEntity> GetEntitiesInRange(ushort zoneId, Position center, float radius);

    // Entity Lifecycle
    SpawnResult SpawnEntity(IEntity entity, ushort zoneId);
    bool DespawnEntity(Guid persistentId);

    // Visibility
    IEnumerable<IEntity> GetVisibleEntities(Guid playerId);
}
