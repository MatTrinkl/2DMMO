using Mmo.Shared.Entities;
using Mmo.Shared.Zones;

namespace Mmo.Server.Zones;

/// <summary>
///     Manages all Zones of the Game aka the world.
/// </summary>
public class ZoneManager
{
    /// <summary>
    ///     The ID of the default Zone.
    /// </summary>
    private readonly ushort _defaultZoneId;

    /// <summary>
    ///     All Zones of the world. Access them with <see cref="Zone.ZoneId" />.
    /// </summary>
    private readonly Dictionary<ushort, Zone> _zones;

    /// <summary>
    ///     Creates a new Manager.
    /// </summary>
    /// <param name="defaultZoneId">ID of the default Zone.</param>
    /// <param name="defaultZone">The Default Zone object.</param>
    public ZoneManager(ushort defaultZoneId, Zone defaultZone)
    {
        _defaultZoneId = defaultZoneId;
        _zones = new Dictionary<ushort, Zone> { { _defaultZoneId, defaultZone } };
    }

    /// <summary>
    ///     Register a Zone.
    /// </summary>
    /// <param name="zoneId">The ID of the Zone to register.</param>
    /// <param name="zone">The Zone to register.</param>
    /// <exception cref="ArgumentException">Thrown when the zone is already registered.</exception>
    public void RegisterZone(ushort zoneId, Zone zone)
    {
        if (_zones.ContainsKey(zoneId)) throw new ArgumentException($"Zone with ID {zoneId} exists already.");

        zone.ZoneId = zoneId;
        _zones[zoneId] = zone;
    }

    /// <summary>
    ///     Removes a Zone from the world. (Needed for instantiation and shards later.
    /// </summary>
    /// <param name="zoneId">The ID of the Zone to remove.</param>
    /// <returns>Returns true if the Zone was removed successfully.</returns>
    public bool UnregisterZone(ushort zoneId) => _zones.Remove(zoneId);

    /// <summary>
    ///     Get the Zone by ID.
    /// </summary>
    /// <param name="zoneId">The ID of the Zone.</param>
    /// <returns>Returns the Zone or null.</returns>
    public Zone? GetZone(ushort zoneId) => _zones.GetValueOrDefault(zoneId);

    /// <summary>
    ///     Get the Default Zone.
    /// </summary>
    /// <returns>Returns the Default Zone or null.</returns>
    public Zone? GetDefaultZone() => _zones.GetValueOrDefault(_defaultZoneId);

    /// <summary>
    ///     WIP: Not Complete.
    ///     Transfer an Entity from one zone to another.
    /// </summary>
    /// <param name="entity">Entity to transfer.</param>
    /// <param name="fromZoneId">ID of the old Zone.</param>
    /// <param name="toZoneId">ID of the new Zone.</param>
    /// <exception cref="ArgumentException">Thrown when a parameter or the Zone from the IDs are null. </exception>
    /// <exception cref="ArgumentNullException">Thrown when the Entity is not in the old zone.</exception>
    public void TransferEntity(IEntity entity, ushort fromZoneId, ushort toZoneId)
    {
        Zone? oldZone = _zones.GetValueOrDefault(fromZoneId);
        Zone? newZone = _zones.GetValueOrDefault(toZoneId);

        ArgumentNullException.ThrowIfNull(oldZone);
        ArgumentNullException.ThrowIfNull(newZone);
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.EntityId.ZoneId != fromZoneId)
            throw new ArgumentException($"Entity with ID {entity.EntityId} does not belong to source Zone with ID {fromZoneId}.");

        if (oldZone == newZone) return;

        oldZone.RemoveEntity(entity.EntityId.Id);
        newZone.AddEntity(entity);
        entity.ChangeZone(toZoneId);
    }
}
