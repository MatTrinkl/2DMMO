using Mmo.Shared.Character.Entities;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Entities.Structs;

namespace Mmo.Shared.Zones.Structs;

/// <summary>
///     This class represents a zone in the world.
/// </summary>
/// <param name="Id">Id of the zone.</param>
/// <param name="ZoneName">Display name of the zone.</param>
/// <param name="Bounds">The outer border of the zone.</param>
/// <param name="_idRegistry">Optional IdRegistry for ID management. If null, uses IdRegistry.Instance.</param>
public readonly record struct Zone(ushort Id, string ZoneName, ZoneBounds Bounds, IIdRegistry? _idRegistry = null)
{
    /// <summary>
    ///     The IdRegistry used for LocalId allocation.
    /// </summary>
    private readonly IIdRegistry _idRegistry = _idRegistry ?? IdRegistry.Instance;

    /// <summary>
    ///     ID of this zone.
    /// </summary>
    public ushort ZoneId { get; } = Id;

    /// <summary>
    ///     Display Name of this Zone.
    /// </summary>
    public string ZoneName { get; } = ZoneName;

    /// <summary>
    ///     The outer border of the zone.
    /// </summary>
    public ZoneBounds Bounds { get; } = Bounds;

    /// <summary>
    ///     All entities in this zone. Access it with <see cref="EntityIdentity.LocalId" />.
    /// </summary>
    public Dictionary<int, IEntity> Entities { get; } = new();

    /// <summary>ID of the zone.</summary>
    public ushort Id { get; init; } = Id;


    public int PlayerCount() => GetPlayers().Count();

    /// <summary>
    ///     Add a new Entity to this zone.
    ///     Uses IdRegistry for LocalId allocation.
    /// </summary>
    /// <param name="entity">Entity to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if entity is null.</exception>
    /// <exception cref="ArgumentException">Thrown when entity is already in this zone.</exception>
    public void AddEntity(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (HasEntity(entity))
            throw new ArgumentException($"Entity {entity} is already registered in Zone {ZoneId} ({ZoneName})");

        // Get next LocalId from IdRegistry
        int newEntityId = _idRegistry.GetNextLocalId(ZoneId);

        entity.SetEntityId(newEntityId, ZoneId);

        Entities.Add(newEntityId, entity);
    }

    /// <summary>
    ///     Remove a entity from this zone.
    ///     Releases the LocalId back to IdRegistry for reuse.
    /// </summary>
    /// <param name="entityId">Entity to remove.</param>
    public void RemoveEntity(int entityId)
    {
        if (Entities.Remove(entityId))
            // Release LocalId back to IdRegistry for reuse
            // Todo:Hardcodes ShardID for prototype!!!
            _idRegistry.ReleaseLocalId(ZoneId, 0, entityId);
    }

    /// <summary>
    ///     Get a IEnumerable with all Player Entities.
    /// </summary>
    /// <returns>Return all player entities.</returns>
    public IEnumerable<PlayerEntity> GetPlayers() => Entities.Values.OfType<PlayerEntity>();

    /// <summary>
    ///     Checks if an entity is registered in this zone.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>Returns true if the entity exists in this zone.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
    public bool HasEntity(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Entities.ContainsValue(entity);
    }

    /// <summary>
    ///     Checks if an entity is registered in this zone.
    /// </summary>
    /// <param name="entityZoneId">The ID of entity in this zone.</param>
    /// <returns>Returns true if the entity exists in this zone.</returns>
    public bool HasEntity(int entityZoneId) => Entities.Keys.Contains(entityZoneId);

    /// <summary>
    ///     Checks if the Position is inside the ZoneBounds.
    /// </summary>
    /// <param name="pos">The Position to check.</param>
    /// <returns>Returns true if the position is inside the border.</returns>
    /// <exception cref="ArgumentNullException">Thrown if position is null.</exception>
    public bool IsPositionInBounds(Position pos)
    {
        ArgumentNullException.ThrowIfNull(pos);
        return Bounds.Contains(pos);
    }
}
