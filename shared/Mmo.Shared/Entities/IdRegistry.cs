using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Mmo.Shared.Entities;

/// <summary>
///     Central registry for ID allocation and entity lookups.
///     Singleton pattern for global access across the application.
/// </summary>
public sealed class IdRegistry : IIdRegistry
{
    private static readonly Lazy<IdRegistry> LazyInstance = new(() => new IdRegistry());

    /// <summary>
    ///     LocalId counters per zone/shard combination.
    ///     Key = (ZoneId << 16) | ShardId
    /// </summary>
    private readonly ConcurrentDictionary<uint, int> _localIdCounters = new();

    /// <summary>
    ///     Freed LocalIds per zone/shard for reuse.
    ///     Key = (ZoneId << 16) | ShardId
    /// </summary>
    private readonly ConcurrentDictionary<uint, ConcurrentQueue<int>> _freedLocalIds = new();

    /// <summary>
    ///     PersistentId → IEntity lookup.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, IEntity> _entitiesByPersistentId = new();

    /// <summary>
    ///     GlobalKey → IEntity lookup (for runtime lookups).
    /// </summary>
    private readonly ConcurrentDictionary<long, IEntity> _entitiesByGlobalKey = new();

    /// <summary>
    ///     ConnectionId → PersistentId mapping.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Guid> _connectionToEntity = new();

    /// <summary>
    ///     PersistentId → ConnectionId mapping.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Guid> _entityToConnection = new();

    /// <summary>
    ///     Private constructor for singleton pattern.
    /// </summary>
    private IdRegistry()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the IdRegistry.
    /// </summary>
    public static IdRegistry Instance => LazyInstance.Value;

    /// <summary>
    ///     Number of registered entities.
    /// </summary>
    public int EntityCount => _entitiesByPersistentId.Count;

    /// <summary>
    ///     Number of registered connections.
    /// </summary>
    public int ConnectionCount => _connectionToEntity.Count;

    /// <inheritdoc />
    public int GetNextLocalId(ushort zoneId, ushort shardId = 0)
    {
        uint key = GetZoneShardKey(zoneId, shardId);

        // Try to reuse a freed ID first
        if (_freedLocalIds.TryGetValue(key, out ConcurrentQueue<int>? freedQueue) &&
            freedQueue.TryDequeue(out int freedId))
        {
            return freedId;
        }

        // Otherwise, get the next ID from the counter (starts at 0)
        return _localIdCounters.AddOrUpdate(key, 0, (_, current) => current + 1);
    }

    /// <inheritdoc />
    public void ReleaseLocalId(ushort zoneId, ushort shardId, int localId)
    {
        uint key = GetZoneShardKey(zoneId, shardId);
        ConcurrentQueue<int> freedQueue = _freedLocalIds.GetOrAdd(key, _ => new ConcurrentQueue<int>());
        freedQueue.Enqueue(localId);
    }

    /// <inheritdoc />
    public bool TryGetEntity(Guid persistentId, [NotNullWhen(true)] out IEntity? entity) =>
        _entitiesByPersistentId.TryGetValue(persistentId, out entity);

    /// <inheritdoc />
    public bool TryGetEntity(long globalKey, [NotNullWhen(true)] out IEntity? entity) =>
        _entitiesByGlobalKey.TryGetValue(globalKey, out entity);

    /// <inheritdoc />
    public bool TryGetEntityByConnection(Guid connectionId, [NotNullWhen(true)] out IEntity? entity)
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
    public void RegisterEntity(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _entitiesByPersistentId.TryAdd(entity.PersistentId, entity);

        // Only register by GlobalKey if entity is assigned to a zone
        if (entity.RuntimeId.IsAssigned)
        {
            _entitiesByGlobalKey.TryAdd(entity.RuntimeId.GlobalKey, entity);
        }
    }

    /// <inheritdoc />
    public void UnregisterEntity(Guid persistentId)
    {
        if (_entitiesByPersistentId.TryRemove(persistentId, out IEntity? entity))
        {
            // Also remove from GlobalKey lookup if it was assigned
            if (entity.RuntimeId.IsAssigned)
            {
                _entitiesByGlobalKey.TryRemove(entity.RuntimeId.GlobalKey, out _);
            }
        }

        // Also clean up any connection mapping
        if (_entityToConnection.TryRemove(persistentId, out Guid connectionId))
        {
            _connectionToEntity.TryRemove(connectionId, out _);
        }
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
        {
            _entityToConnection.TryRemove(persistentId, out _);
        }
    }

    /// <summary>
    ///     Updates the GlobalKey lookup when an entity changes zones.
    ///     Should be called after the entity's RuntimeId has been updated.
    /// </summary>
    /// <param name="entity">The entity that changed zones.</param>
    /// <param name="oldGlobalKey">The previous GlobalKey of the entity.</param>
    public void UpdateEntityGlobalKey(IEntity entity, long oldGlobalKey)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Remove old GlobalKey mapping
        _entitiesByGlobalKey.TryRemove(oldGlobalKey, out _);

        // Add new GlobalKey mapping if assigned
        if (entity.RuntimeId.IsAssigned)
        {
            _entitiesByGlobalKey.TryAdd(entity.RuntimeId.GlobalKey, entity);
        }
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
    public IEnumerable<IEntity> GetAllEntities() => _entitiesByPersistentId.Values;

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
}
