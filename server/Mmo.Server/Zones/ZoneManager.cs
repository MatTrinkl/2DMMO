using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Mmo.Server.Entities;
using Mmo.Shared.Entities;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;

namespace Mmo.Server.Zones;

/// <summary>
///     Manages all Zones of the Game aka the world.
///     Handles zone and player management. Entity/connection registration
///     is handled by the caller through IdRegistry directly.
/// </summary>
public class ZoneManager
{
    /// <summary>
    ///     The ID of the default Zone.
    /// </summary>
    private readonly ushort _defaultZoneId;

    /// <summary>
    ///     ConnectionId → ServerPlayer (Session-stabil, ändert sich nie während der Verbindung)
    ///     Note: This is server-specific (ServerPlayer wraps PlayerEntity with Connection info)
    /// </summary>
    private readonly ConcurrentDictionary<Guid, ServerPlayer> _playersByConnectionId = new();

    /// <summary>
    ///     PersistentId → ServerPlayer (Permanent-stabil, ändert sich nie - auch nicht bei Zonenwechsel)
    ///     Note: This is server-specific (ServerPlayer wraps PlayerEntity with Connection info)
    /// </summary>
    private readonly ConcurrentDictionary<Guid, ServerPlayer> _playersByPersistentId = new();

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
    ///     Number of connected players across all zones.
    /// </summary>
    public int PlayerCount => _playersByConnectionId.Count;

    /// <summary>
    ///     Number of registered zones.
    /// </summary>
    public int ZoneCount => _zones.Count;

    /// <summary>
    ///     The default zone ID.
    /// </summary>
    public ushort DefaultZoneId => _defaultZoneId;

    /// <summary>
    ///     Register a Zone.
    /// </summary>
    /// <param name="zoneId">The ID of the Zone to register.</param>
    /// <param name="zone">The Zone to register.</param>
    /// <exception cref="ArgumentException">Thrown when the zone is already registered.</exception>
    public void RegisterZone(ushort zoneId, Zone zone)
    {
        if (_zones.ContainsKey(zoneId))
            throw new ArgumentException($"Zone with ID {zoneId} exists already.");

        zone.ZoneId = zoneId;
        _zones[zoneId] = zone;
    }

    /// <summary>
    ///     Removes a Zone from the world.  (Needed for instantiation and shards later.)
    /// </summary>
    /// <param name="zoneId">The ID of the Zone to remove.</param>
    /// <returns>Returns true if the Zone was removed successfully. </returns>
    public bool UnregisterZone(ushort zoneId) => _zones.Remove(zoneId);

    /// <summary>
    ///     Get the Zone by ID.
    /// </summary>
    /// <param name="zoneId">The ID of the Zone. </param>
    /// <returns>Returns the Zone or null. </returns>
    public Zone? GetZone(ushort zoneId) => _zones.GetValueOrDefault(zoneId);

    /// <summary>
    ///     Get the Default Zone.
    /// </summary>
    /// <returns>Returns the Default Zone or null.</returns>
    public Zone? GetDefaultZone() => _zones.GetValueOrDefault(_defaultZoneId);

    /// <summary>
    ///     Gets all registered zones.
    /// </summary>
    public IEnumerable<Zone> GetAllZones() => _zones.Values;

    public PlayerEntity SpawnPlayer(Guid connectionId, string username)
    {
        ArgumentNullException.ThrowIfNull(username);
        //TODO: CharacterId and Position read from DB, currently its the connectionId and always spawn at 0,0
        return new PlayerEntity(connectionId, username, new Position(0, 0));
    }

    /// <summary>
    ///     Adds a player to a zone.
    ///     Note: Caller is responsible for registering entity and connection in IdRegistry.
    /// </summary>
    /// <param name="serverPlayer">The server player wrapper.</param>
    /// <param name="zoneId">The zone to add the player to.  Defaults to the default zone.</param>
    /// <exception cref="InvalidOperationException">Thrown when zone doesn't exist.</exception>
    /// <exception cref="ArgumentNullException">Thrown when serverPlayer is null.</exception>
    public void AddPlayer(ServerPlayer serverPlayer, ushort? zoneId = null)
    {
        ArgumentNullException.ThrowIfNull(serverPlayer);

        ushort targetZoneId = zoneId ?? _defaultZoneId;
        Zone? zone = GetZone(targetZoneId);

        if (zone == null)
            throw new InvalidOperationException($"Zone {targetZoneId} does not exist.");

        // Add to Zone (assigns EntityId via IdRegistry)
        zone.AddEntity(serverPlayer.Entity);

        // Add to server-specific ServerPlayer lookups
        _playersByConnectionId.TryAdd(serverPlayer.Connection.Id, serverPlayer);
        _playersByPersistentId.TryAdd(serverPlayer.Entity.PersistentId, serverPlayer);
    }

    /// <summary>
    ///     Removes a player by connection ID.
    ///     Note: Caller is responsible for unregistering from IdRegistry.
    /// </summary>
    /// <param name="connectionId">The connection ID of the player to remove.</param>
    /// <returns>The removed player, or null if not found.</returns>
    public ServerPlayer? RemovePlayerByConnectionId(Guid connectionId)
    {
        if (!_playersByConnectionId.TryRemove(connectionId, out ServerPlayer? serverPlayer))
            return null;

        // Remove from server-specific lookups
        _playersByPersistentId.TryRemove(serverPlayer.Entity.PersistentId, out _);

        // Remove from zone
        ushort zoneId = serverPlayer.Entity.RuntimeId.ZoneId;
        Zone? zone = GetZone(zoneId);
        zone?.RemoveEntity(serverPlayer.Entity.RuntimeId.LocalId);

        return serverPlayer;
    }

    /// <summary>
    ///     Removes a player by persistent ID.
    ///     Note: Caller is responsible for unregistering from IdRegistry.
    /// </summary>
    /// <param name="persistentId">The persistent ID of the player to remove.</param>
    /// <returns>The removed player, or null if not found.</returns>
    public ServerPlayer? RemovePlayerByPersistentId(Guid persistentId)
    {
        if (!_playersByPersistentId.TryRemove(persistentId, out ServerPlayer? serverPlayer))
            return null;

        // Remove from other lookups
        _playersByConnectionId.TryRemove(serverPlayer.Connection.Id, out _);

        // Remove from zone
        ushort zoneId = serverPlayer.Entity.RuntimeId.ZoneId;
        Zone? zone = GetZone(zoneId);
        zone?.RemoveEntity(serverPlayer.Entity.RuntimeId.LocalId);

        return serverPlayer;
    }

    /// <summary>
    ///     Gets a player by connection ID. O(1) lookup.
    /// </summary>
    /// <param name="connectionId">The connection ID to search for.</param>
    /// <param name="player">The found player, or null. </param>
    /// <returns>True if the player was found. </returns>
    public bool TryGetPlayerByConnectionId(Guid connectionId, [NotNullWhen(true)] out ServerPlayer? player) =>
        _playersByConnectionId.TryGetValue(connectionId, out player);

    /// <summary>
    ///     Gets a player by persistent ID. O(1) lookup.
    /// </summary>
    /// <param name="persistentId">The persistent ID to search for.</param>
    /// <param name="player">The found player, or null.</param>
    /// <returns>True if the player was found.</returns>
    public bool TryGetPlayerByPersistentId(Guid persistentId, [NotNullWhen(true)] out ServerPlayer? player) =>
        _playersByPersistentId.TryGetValue(persistentId, out player);

    /// <summary>
    ///     Gets all server players (across all zones).
    /// </summary>
    public IEnumerable<ServerPlayer> GetAllServerPlayers() => _playersByConnectionId.Values;

    /// <summary>
    ///     Gets all server players in a specific zone.
    /// </summary>
    /// <param name="zoneId">The zone ID to filter by.</param>
    public IEnumerable<ServerPlayer> GetServerPlayersInZone(ushort? zoneId)
    {
        return GetAllServerPlayers()
            .Where(p => p.Entity.RuntimeId.ZoneId == zoneId);
    }

    /// <summary>
    ///     Checks if a player with the given connection ID exists.
    /// </summary>
    /// <param name="connectionId">The connection ID to check.</param>
    public bool HasPlayerWithConnectionId(Guid connectionId) => _playersByConnectionId.ContainsKey(connectionId);

    /// <summary>
    ///     Checks if a player with the given persistent ID exists.
    /// </summary>
    /// <param name="persistentId">The persistent ID to check.</param>
    public bool HasPlayerWithPersistentId(Guid persistentId) => _playersByPersistentId.ContainsKey(persistentId);

    /// <summary>
    ///     Adds any entity to a zone (Players, NPCs, Mobs, etc.).
    ///     Note: Caller is responsible for registering in IdRegistry.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="zoneId">The zone to add the entity to.</param>
    /// <exception cref="InvalidOperationException">Thrown when zone doesn't exist.</exception>
    public void AddEntity(IEntity entity, ushort zoneId)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Zone? zone = GetZone(zoneId);
        if (zone == null)
            throw new InvalidOperationException($"Zone {zoneId} does not exist.");

        zone.AddEntity(entity);
    }

    /// <summary>
    ///     Adds a persistent entity (NPC, static object from zone config).
    ///     Deprecated: Use AddEntity instead.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="zoneId">The zone to add the entity to.</param>
    [Obsolete("Use AddEntity instead - all entities now have PersistentId")]
    public void AddPersistentEntity(IEntity entity, ushort zoneId) => AddEntity(entity, zoneId);

    /// <summary>
    ///     Adds a dynamic/temporary entity (spawned mob, projectile, drop).
    ///     Deprecated: Use AddEntity instead.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="zoneId">The zone to add the entity to.</param>
    [Obsolete("Use AddEntity instead - all entities now have PersistentId")]
    public void AddDynamicEntity(IEntity entity, ushort zoneId) => AddEntity(entity, zoneId);

    /// <summary>
    ///     Removes an entity by its PersistentId.
    ///     Uses IdRegistry for O(1) lookup if entity is registered there.
    ///     Note: Caller is responsible for unregistering from IdRegistry.
    /// </summary>
    /// <param name="persistentId">The persistent ID of the entity.</param>
    /// <returns>The removed entity, or null if not found.</returns>
    public IEntity? RemoveEntity(Guid persistentId)
    {
        // Try O(1) lookup via IdRegistry first
        if (IdRegistry.Instance.TryGetEntity(persistentId, out IEntity? entity))
        {
            // Remove from zone
            ushort zoneId = entity.RuntimeId.ZoneId;
            Zone? zone = GetZone(zoneId);
            zone?.RemoveEntity(entity.RuntimeId.LocalId);
            return entity;
        }

        // Fallback: Search through all zones (for entities not in IdRegistry)
        foreach (Zone zone in _zones.Values)
        {
            entity = zone.Entities.Values.FirstOrDefault(e => e.PersistentId == persistentId);
            if (entity != null)
            {
                zone.RemoveEntity(entity.RuntimeId.LocalId);
                return entity;
            }
        }

        return null;
    }

    /// <summary>
    ///     Removes a persistent entity by its PersistentId.
    ///     Deprecated: Use RemoveEntity instead.
    /// </summary>
    /// <param name="persistentId">The persistent ID of the entity.</param>
    /// <returns>The removed entity, or null if not found.</returns>
    [Obsolete("Use RemoveEntity instead - all entities now have PersistentId")]
    public IEntity? RemovePersistentEntity(Guid persistentId) => RemoveEntity(persistentId);

    /// <summary>
    ///     Gets any entity by PersistentId. Uses IdRegistry for O(1) lookup.
    /// </summary>
    /// <param name="persistentId">The persistent ID to search for.</param>
    /// <param name="entity">The found entity, or null.</param>
    /// <returns>True if the entity was found.</returns>
    public bool TryGetEntityByPersistentId(Guid persistentId, [NotNullWhen(true)] out IEntity? entity)
    {
        // Try O(1) lookup via IdRegistry first
        if (IdRegistry.Instance.TryGetEntity(persistentId, out entity))
            return true;

        // Fallback: Search through all zones (for entities not in IdRegistry)
        foreach (Zone zone in _zones.Values)
        {
            entity = zone.Entities.Values.FirstOrDefault(e => e.PersistentId == persistentId);
            if (entity != null)
                return true;
        }

        entity = null;
        return false;
    }

    /// <summary>
    ///     Checks if an entity with the given PersistentId exists.
    ///     Uses IdRegistry for O(1) lookup.
    /// </summary>
    /// <param name="persistentId">The persistent ID to check.</param>
    public bool HasPersistentEntity(Guid persistentId) =>
        IdRegistry.Instance.HasEntity(persistentId) ||
        _zones.Values.Any(zone => zone.Entities.Values.Any(e => e.PersistentId == persistentId));

    /// <summary>
    ///     Gets an entity by runtime EntityId from a specific zone.
    ///     Note: EntityId changes on zone transfer!
    /// </summary>
    /// <param name="zoneId">The zone ID. </param>
    /// <param name="entityId">The entity ID within the zone.</param>
    /// <returns>The entity, or null if not found.</returns>
    public IEntity? GetEntity(ushort zoneId, int entityId)
    {
        Zone? zone = GetZone(zoneId);
        return zone?.Entities.GetValueOrDefault(entityId);
    }

    /// <summary>
    ///     Gets an entity by runtime EntityId from the default zone.
    /// </summary>
    /// <param name="entityId">The entity ID within the zone.</param>
    /// <returns>The entity, or null if not found.</returns>
    public IEntity? GetEntityFromDefaultZone(int entityId) => GetEntity(_defaultZoneId, entityId);

    /// <summary>
    ///     Gets all entities from a specific zone.
    /// </summary>
    /// <param name="zoneId">The zone ID.</param>
    /// <returns>List of entities, or empty list if zone not found.</returns>
    public List<Entity> GetAllEntities(ushort zoneId)
    {
        Zone? zone = GetZone(zoneId);
        return zone?.Entities.Values.OfType<Entity>().ToList() ?? [];
    }

    /// <summary>
    ///     Gets all entities from the default zone.
    /// </summary>
    /// <returns>List of entities. </returns>
    public List<Entity> GetAllEntitiesFromDefaultZone() => GetAllEntities(_defaultZoneId);

    /// <summary>
    ///     Gets all player entities from a specific zone (shared PlayerEntity, not ServerPlayer).
    /// </summary>
    /// <param name="zoneId">The zone ID.</param>
    public IEnumerable<PlayerEntity> GetPlayerEntities(ushort zoneId)
    {
        Zone? zone = GetZone(zoneId);
        return zone?.GetPlayers() ?? Enumerable.Empty<PlayerEntity>();
    }

    /// <summary>
    ///     Transfer an Entity from one zone to another.
    ///     Note: EntityId will change, but PersistentId stays the same!
    ///     Updates GlobalKey lookup in IdRegistry if entity is registered there.
    /// </summary>
    /// <param name="entity">Entity to transfer.</param>
    /// <param name="fromZoneId">ID of the old Zone.</param>
    /// <param name="toZoneId">ID of the new Zone.</param>
    /// <exception cref="ArgumentNullException">Thrown when entity or zones are null.</exception>
    /// <exception cref="ArgumentException">Thrown when entity is not in the source zone.</exception>
    public void TransferEntity(IEntity entity, ushort fromZoneId, ushort toZoneId)
    {
        Zone? oldZone = _zones.GetValueOrDefault(fromZoneId);
        Zone? newZone = _zones.GetValueOrDefault(toZoneId);

        ArgumentNullException.ThrowIfNull(oldZone, nameof(fromZoneId));
        ArgumentNullException.ThrowIfNull(newZone, nameof(toZoneId));
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.RuntimeId.ZoneId != fromZoneId)
            throw new ArgumentException(
                $"Entity with ID {entity.RuntimeId} does not belong to source Zone with ID {fromZoneId}.");

        if (oldZone == newZone) return;

        // Save old GlobalKey for IdRegistry update
        long oldGlobalKey = entity.RuntimeId.GlobalKey;

        // Remove from old zone (releases LocalId via IdRegistry)
        oldZone.RemoveEntity(entity.RuntimeId.LocalId);

        // Add to new zone (assigns new LocalId via IdRegistry)
        newZone.AddEntity(entity);

        // Update GlobalKey lookup in IdRegistry if entity is registered
        IdRegistry.Instance.UpdateEntityGlobalKey(entity, oldGlobalKey);

        // Notify entity of zone change
        entity.ChangeZone(toZoneId);

        // PersistentId lookups don't need updating - they stay the same!
    }

    /// <summary>
    ///     Transfers a player to another zone by connection ID.
    /// </summary>
    /// <param name="connectionId">The connection ID of the player. </param>
    /// <param name="toZoneId">The target zone ID.</param>
    /// <returns>True if the transfer was successful.</returns>
    public bool TransferPlayerByConnectionId(Guid connectionId, ushort toZoneId)
    {
        if (!TryGetPlayerByConnectionId(connectionId, out ServerPlayer? serverPlayer))
            return false;

        ushort fromZoneId = serverPlayer.Entity.RuntimeId.ZoneId;

        if (fromZoneId == toZoneId)
            return true; // Already in target zone

        TransferEntity(serverPlayer.Entity, fromZoneId, toZoneId);
        return true;
    }

    /// <summary>
    ///     Transfers a player to another zone by persistent ID.
    /// </summary>
    /// <param name="persistentId">The persistent ID of the player. </param>
    /// <param name="toZoneId">The target zone ID.</param>
    /// <returns>True if the transfer was successful.</returns>
    public bool TransferPlayerByPersistentId(Guid persistentId, ushort toZoneId)
    {
        if (!TryGetPlayerByPersistentId(persistentId, out ServerPlayer? serverPlayer))
            return false;

        ushort fromZoneId = serverPlayer.Entity.RuntimeId.ZoneId;

        if (fromZoneId == toZoneId)
            return true; // Already in target zone

        TransferEntity(serverPlayer.Entity, fromZoneId, toZoneId);
        return true;
    }
}
