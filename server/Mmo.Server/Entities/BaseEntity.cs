using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Entities.Structs;

namespace Mmo.Server.Entities;

public abstract class BaseEntity(
    EntityIdentity runtimeId,
    Guid persistentId,
    Position position)
    : IEntity
{
    public EntityIdentity RuntimeId { get; init; } = runtimeId;
    public abstract bool IsTrulyPersistent { get; }
    public Guid PersistentId { get; init; } = persistentId;
    public Position Position { get; set; } = position;
    public abstract EntityType Type { get; }
    public abstract void SetEntityId(ushort localId, ushort zoneId);

    public abstract void ChangeZone(ushort newZoneId);
}
