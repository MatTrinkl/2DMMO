using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Mmo.Server.Core;
using Mmo.Server.Entities;
using Mmo.Server.Entities.Interfaces;
using Mmo.Shared.Entities.Structs;

namespace Mmo.Shared.Core;

/// <summary>
///     Central registry for ID allocation and entity lookups.
///     Singleton pattern for global access across the application.
/// </summary>
public sealed class IdRegistry : IIdRegistry
{
    private static readonly Lazy<IdRegistry> _lazyInstance = new(() => new IdRegistry());

    /// <summary>
    ///     ConnectionId → PersistentId mapping.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Guid> _connectionToEntity = new();

    /// <summary>
    ///     GlobalKey → BaseEntity lookup (for runtime lookups).
    /// </summary>
    private readonly ConcurrentDictionary<long, BaseEntity> _entitiesByGlobalKey = new();

    /// <summary>
    ///     PersistentId → BaseEntity lookup.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, BaseEntity> _entitiesByPersistentId = new();

    /// <summary>
    ///     PersistentId → ConnectionId mapping.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Guid> _entityToConnection = new();

    /// <summary>
    ///     Freed LocalIds per zone/shard for reuse.
    ///     Key = (ZoneId  16) | ShardId
    /// </summary>
    private readonly ConcurrentDictionary<uint, ConcurrentQueue<ushort>> _freedLocalIds = new();

    /// <summary>
    ///     LocalId counters per zone/shard combination.
    ///     Key = (ZoneId  16) | ShardId
    /// </summary>
    private readonly ConcurrentDictionary<uint, ushort> _localIdCounters = new();

    /// <summary>
    ///     Private constructor for singleton pattern.
    /// </summary>
    private IdRegistry()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the IdRegistry.
    /// </summary>
    public static IdRegistry Instance => _lazyInstance.Value;

    /// <summary>
    ///     Number of registered entities.
    /// </summary>
    public int EntityCount => _entitiesByPersistentId.Count;

    /// <summary>
    ///     Number of registered connections.
    /// </summary>
    public int ConnectionCount => _connectionToEntity.Count;

    /// <inheritdoc />
    public Guid GeneratePersistentId() => Guid.NewGuid();

    /// <inheritdoc />
    public ushort GetNextLocalId(ushort zoneId, ushort shardId = 0)
    {
        uint key = GetZoneShardKey(zoneId, shardId);

        // Try to reuse a freed ID first
        if (_freedLocalIds.TryGetValue(key, out ConcurrentQueue<ushort>? freedQueue) &&
            freedQueue.TryDequeue(out ushort freedId))
            return freedId;

        // AddOrUpdate behavior:
        // - First call (key doesn't exist): returns addValue (0), stores 0
        // - Subsequent calls: returns current + 1, stores the new value
        // Result: IDs are 0, 1, 2, 3, ...
        return _localIdCounters.AddOrUpdate(key, 0, (_, current) => (ushort)(current + 1));
    }

    /// <inheritdoc />
    public void ReleaseLocalId(ushort zoneId, ushort shardId, ushort localId)
    {
        uint key = GetZoneShardKey(zoneId, shardId);
        ConcurrentQueue<ushort> freedQueue = _freedLocalIds.GetOrAdd(key, _ => new ConcurrentQueue<ushort>());
        freedQueue.Enqueue(localId);
    }

    /// <inheritdoc />
    public bool TryGetEntity(Guid persistentId, [NotNullWhen(true)] out BaseEntity? entity) =>
        _entitiesByPersistentId.TryGetValue(persistentId, out entity);

    /// <inheritdoc />
    public List<BaseEntity> GetEntities(IEnumerable<Guid> persistentIds)
    {
        List<BaseEntity> entities = [];
        foreach (Guid id in persistentIds) entities.AddRange(_entitiesByPersistentId[id]);

        return entities;
    }

    /// <inheritdoc />
    public bool TryGetEntity(long globalKey, [NotNullWhen(true)] out BaseEntity? entity) =>
        _entitiesByGlobalKey.TryGetValue(globalKey, out entity);

    /// <inheritdoc />
    public bool TryGetEntityByConnection(Guid connectionId, [NotNullWhen(true)] out BaseEntity? entity)
    {
        entity = null;
        if (!_connectionToEntity.TryGetValue(connectionId, out Guid persistentId))
            return false;

        return TryGetEntity(persistentId, out entity);
    }

    /// <inheritdoc />
    public bool TryGetConnectionByEntity(Guid persistentId, out Guid connectionId) =>
        _entityToConnection.TryGetValue(persistentId, out connectionId);

    /// <inheritdoc />
    /// <remarks>
    ///     Duplicate registrations are silently ignored (TryAdd returns false).
    ///     This is intentional to support scenarios like reconnecting players
    ///     or zone transfers where the entity might already be registered.
    /// </remarks>
    public void RegisterEntity(BaseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _entitiesByPersistentId.TryAdd(entity.PersistentId, entity);

        // Only register by GlobalKey if entity is assigned to a zone
        EntityIdentity runtimeId = GetRuntimeId(entity);
        if (runtimeId.IsAssigned) _entitiesByGlobalKey.TryAdd(runtimeId.GlobalKey, entity);
    }

    /// <inheritdoc />
    public void UnregisterEntity(Guid persistentId)
    {
        if (_entitiesByPersistentId.TryRemove(persistentId, out BaseEntity? entity))
        {
            // Also remove from GlobalKey lookup if it was assigned
            EntityIdentity runtimeId = GetRuntimeId(entity);
            if (runtimeId.IsAssigned)
                _entitiesByGlobalKey.TryRemove(runtimeId.GlobalKey, out _);
        }

        // Also clean up any connection mapping
        if (_entityToConnection.TryRemove(persistentId, out Guid connectionId))
            _connectionToEntity.TryRemove(connectionId, out _);
    }

    /// <inheritdoc />
    public void RegisterConnection(Guid connectionId, Guid persistentId)
    {
        _connectionToEntity.TryAdd(connectionId, persistentId);
        _entityToConnection.TryAdd(persistentId, connectionId);
    }

    /// <inheritdoc />
    public void UnregisterConnection(Guid connectionId)
    {
        if (_connectionToEntity.TryRemove(connectionId, out Guid persistentId))
            _entityToConnection.TryRemove(persistentId, out _);
    }

    /// <summary>
    ///     Updates the GlobalKey lookup when an entity changes zones.
    ///     Should be called after the entity's RuntimeId has been updated.
    /// </summary>
    /// <param name="entity">The entity that changed zones.</param>
    /// <param name="oldGlobalKey">The previous GlobalKey of the entity.</param>
    public void UpdateEntityGlobalKey(BaseEntity entity, long oldGlobalKey)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Remove old GlobalKey mapping
        _entitiesByGlobalKey.TryRemove(oldGlobalKey, out _);

        // Add new GlobalKey mapping if assigned
        EntityIdentity runtimeId = GetRuntimeId(entity);
        if (runtimeId.IsAssigned) _entitiesByGlobalKey.TryAdd(runtimeId.GlobalKey, entity);
    }

    /// <summary>
    ///     Checks if an entity with the given PersistentId is registered.
    /// </summary>
    /// <param name="persistentId">The PersistentId to check.</param>
    /// <returns>True if the entity is registered.</returns>
    public bool HasEntity(Guid persistentId) => _entitiesByPersistentId.ContainsKey(persistentId);

    /// <summary>
    ///     Checks if a connection is registered.
    /// </summary>
    /// <param name="connectionId">The ConnectionId to check.</param>
    /// <returns>True if the connection is registered.</returns>
    public bool HasConnection(Guid connectionId) => _connectionToEntity.ContainsKey(connectionId);

    /// <summary>
    ///     Gets all registered entities.
    /// </summary>
    /// <returns>An enumerable of all entities.</returns>
    public IEnumerable<BaseEntity> GetAllEntities() => _entitiesByPersistentId.Values;

    /// <summary>
    ///     Clears all registrations. Useful for testing.
    /// </summary>
    public void Clear()
    {
        _entitiesByPersistentId.Clear();
        _entitiesByGlobalKey.Clear();
        _connectionToEntity.Clear();
        _entityToConnection.Clear();
        _localIdCounters.Clear();
        _freedLocalIds.Clear();
    }

    /// <summary>
    ///     Creates a combined key for zone/shard lookup.
    ///     Uses bit-packing to create a unique 32-bit key from two 16-bit values:
    ///     - Upper 16 bits: ZoneId
    ///     - Lower 16 bits: ShardId
    ///     This allows O(1) lookup in the _localIdCounters and _freedLocalIds dictionaries.
    /// </summary>
    /// <param name="zoneId">The zone ID (0-65535).</param>
    /// <param name="shardId">The shard ID (0-65535).</param>
    /// <returns>A unique 32-bit key combining both IDs.</returns>
    private static uint GetZoneShardKey(ushort zoneId, ushort shardId) =>
        ((uint)zoneId << 16) | shardId;

    /// <summary>
    ///     Gets the RuntimeId from an entity, using the IMutableRuntimeEntity interface
    ///     when available to get the correct (hidden) RuntimeId value from derived classes.
    /// </summary>
    /// <param name="entity">The entity to get RuntimeId from.</param>
    /// <returns>The entity's RuntimeId.</returns>
    private static EntityIdentity GetRuntimeId(BaseEntity entity) =>
        entity is IMutableRuntimeEntity mutableEntity ? mutableEntity.RuntimeId : entity.RuntimeId;
}
