// shared/Mmo.Shared/Entities/IEntity.cs

using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

public interface IEntity
{
    /// <summary>
    ///     Runtime identity - changes on zone transfer!
    ///     Use for zone-internal lookups only.
    /// </summary>
    EntityIdentity RuntimeId { get; }

    /// <summary>
    ///     Stable identity for network communication.
    ///     Never changes, even on zone transfer.
    /// </summary>
    Guid PersistentId { get; }

    /// <summary>
    ///     Whether this entity is truly persistent (saved to DB)
    ///     or just runtime-persistent (exists only this session).
    /// </summary>
    bool IsTrulyPersistent { get; }

    EntityType Type { get; }
    EntityRole Role { get; }
    Position Position { get; set; }

    /// <summary>
    ///     Sets the runtime identity. Called by Zone when entity is added/transferred.
    /// </summary>
    /// <param name="localId">The new local ID within the zone.</param>
    /// <param name="zoneId">The zone ID.</param>
    void SetEntityId(int localId, ushort zoneId);

    /// <summary>
    ///     Called when this entity changes zones.
    /// </summary>
    /// <param name="newZoneId">The ID of the new zone.</param>
    void ChangeZone(ushort newZoneId);
}
