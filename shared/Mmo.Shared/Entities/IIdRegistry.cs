using System.Diagnostics.CodeAnalysis;

namespace Mmo.Shared.Entities;

/// <summary>
///     Interface for the central ID registry.
///     Provides ID allocation and entity lookup functionality.
/// </summary>
public interface IIdRegistry
{
    /// <summary>
    ///     Generates a new unique PersistentId (GUID).
    ///     All GUIDs in the system should be generated through this method.
    /// </summary>
    /// <returns>A new unique Guid.</returns>
    Guid GeneratePersistentId();

    /// <summary>
    ///     Gets the next available LocalId for a zone/shard combination.
    ///     Reuses freed IDs when available.
    /// </summary>
    /// <param name="zoneId">The zone ID.</param>
    /// <param name="shardId">The shard ID (default: 0).</param>
    /// <returns>The next available LocalId.</returns>
    int GetNextLocalId(ushort zoneId, ushort shardId = 0);

    /// <summary>
    ///     Releases a LocalId for reuse.
    ///     Called when an entity is removed from a zone.
    /// </summary>
    /// <param name="zoneId">The zone ID.</param>
    /// <param name="shardId">The shard ID.</param>
    /// <param name="localId">The LocalId to release.</param>
    void ReleaseLocalId(ushort zoneId, ushort shardId, int localId);

    /// <summary>
    ///     Looks up an entity by its PersistentId.
    /// </summary>
    /// <param name="persistentId">The PersistentId to search for.</param>
    /// <param name="entity">The found entity, or null.</param>
    /// <returns>True if the entity was found.</returns>
    bool TryGetEntity(Guid persistentId, [NotNullWhen(true)] out IEntity? entity);

    /// <summary>
    ///     Looks up an entity by its GlobalKey (runtime ID).
    /// </summary>
    /// <param name="globalKey">The GlobalKey to search for.</param>
    /// <param name="entity">The found entity, or null.</param>
    /// <returns>True if the entity was found.</returns>
    bool TryGetEntity(long globalKey, [NotNullWhen(true)] out IEntity? entity);

    /// <summary>
    ///     Looks up an entity by connection ID.
    /// </summary>
    /// <param name="connectionId">The connection ID.</param>
    /// <param name="entity">The found entity, or null.</param>
    /// <returns>True if the entity was found.</returns>
    bool TryGetEntityByConnection(Guid connectionId, [NotNullWhen(true)] out IEntity? entity);

    /// <summary>
    ///     Looks up a connection ID by entity PersistentId.
    /// </summary>
    /// <param name="persistentId">The entity's PersistentId.</param>
    /// <param name="connectionId">The found connection ID, or empty.</param>
    /// <returns>True if the connection was found.</returns>
    bool TryGetConnectionByEntity(Guid persistentId, out Guid connectionId);

    /// <summary>
    ///     Registers an entity in the registry.
    /// </summary>
    /// <param name="entity">The entity to register.</param>
    void RegisterEntity(IEntity entity);

    /// <summary>
    ///     Unregisters an entity from the registry.
    /// </summary>
    /// <param name="persistentId">The PersistentId of the entity to unregister.</param>
    void UnregisterEntity(Guid persistentId);

    /// <summary>
    ///     Registers a connection-to-entity mapping.
    /// </summary>
    /// <param name="connectionId">The connection ID.</param>
    /// <param name="persistentId">The entity's PersistentId.</param>
    void RegisterConnection(Guid connectionId, Guid persistentId);

    /// <summary>
    ///     Unregisters a connection mapping.
    /// </summary>
    /// <param name="connectionId">The connection ID to unregister.</param>
    void UnregisterConnection(Guid connectionId);

}
