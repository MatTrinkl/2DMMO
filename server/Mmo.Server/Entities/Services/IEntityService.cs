using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Records;

namespace Mmo.Server.Entities.Services;

public interface IEntityService
{
    // Entity Queries
    BaseEntity? GetEntity(Guid persistentId);
    IEnumerable<BaseEntity> GetEntitiesInZone(ushort zoneId);
    IEnumerable<BaseEntity> GetEntitiesInRange(ushort zoneId, Position center, float radius);

    // Entity Lifecycle
    SpawnResult SpawnEntity(BaseEntity entity, ushort zoneId);
    bool DespawnEntity(Guid persistentId);

    // Visibility
    IEnumerable<BaseEntity> GetVisibleEntities(Guid playerId);
}
