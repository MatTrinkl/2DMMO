using MessagePack;

namespace Mmo.Shared.Entities;

/// <summary>
///     This class is used to identify an entity.
/// </summary>
[MessagePackObject]
public struct EntityIdentity : IEquatable<EntityIdentity>
{
    /// <summary>
    ///     Unique in a zone. Is set from the server.
    /// </summary>
    [Key(0)]
    public int Id { get; private set; }

    /// <summary>
    ///     The ID of the Zone where the Entity exits currently.
    /// </summary>
    [Key(1)]
    public ushort ZoneId { get; private set; }

    /// <summary>
    ///     WIP: The Entity of the shard of the zone.
    /// </summary>
    [Key(2)]
    public ushort ShardId { get; private set; }

    /// <summary>
    ///     Reference to the prefab of this entity.
    /// </summary>
    [Key(3)] public readonly ushort PrefabId;

    /// <summary>
    ///     Creates a new EntityIdentity struct.
    /// </summary>
    /// <param name="id">The ID of the Entity in a zone.</param>
    /// <param name="zoneId">The ID of the zone.</param>
    /// <param name="shardId">The ID of the shard of the zone.</param>
    /// <param name="prefabId">The ID of the prefab of this entity.</param>
    public EntityIdentity(int id, ushort zoneId, ushort shardId, ushort prefabId)
    {
        Id = id;
        ZoneId = zoneId;
        ShardId = shardId;
        PrefabId = prefabId;
    }

    /// <summary>
    ///     The global ID of the entity. This is unique at every time. This ID will change when a zone or shard is changed.
    /// </summary>
    [IgnoreMember]
    public long GlobalKey => ((long)ShardId << 48) | ((long)ZoneId << 32) | (uint)Id;


    /// <summary>
    ///     Compares two EntityIdentities.
    /// </summary>
    /// <param name="other">Other EntityIdentity to compare.</param>
    /// <returns>True if the GlobalKey and the PrefabID are the same.</returns>
    public bool Equals(EntityIdentity other)
        => GlobalKey == other.GlobalKey && PrefabId == other.PrefabId;

    /// <summary>
    ///     Compares this to an object.
    /// </summary>
    /// <param name="obj">Object to compare.</param>
    /// <returns>True if <see cref="obj" /> is <see cref="EntityIdentity" /> and the GlobalKey and the PrefabID are the same.</returns>
    public override bool Equals(object? obj)
        => obj is EntityIdentity other && Equals(other);

    /// <summary>
    ///     Generates the Hashcode of this struct.
    /// </summary>
    /// <returns>Returns the hashcode.</returns>
    public override int GetHashCode()
        => HashCode.Combine(Id, ZoneId);

    /// <summary>
    ///     Compares two EntityIdentities if they are equal.
    /// </summary>
    /// <param name="left">EntityIdentities on the left of the operator.</param>
    /// <param name="right">EntityIdentities on the right of the operator.</param>
    /// <returns>Returns true if the GlobalKey and the PrefabID are the same.</returns>
    public static bool operator ==(EntityIdentity left, EntityIdentity right)
        => left.Equals(right);

    /// <summary>
    ///     Compares two EntityIdentities if they are not equal.
    /// </summary>
    /// <param name="left">EntityIdentities on the left of the operator.</param>
    /// <param name="right">EntityIdentities on the right of the operator.</param>
    /// <returns>Returns true if the GlobalKey and the PrefabID are not the same.</returns>
    public static bool operator !=(EntityIdentity left, EntityIdentity right)
        => !left.Equals(right);

    /// <summary>
    ///     Turns this EntityIdentity into a string.
    /// </summary>
    /// <returns>Returns a string with format: "Entity[{ZoneId}:{Id}|Prefab:{PrefabId}]".</returns>
    public override string ToString()
        => $"Entity[{ZoneId}:{Id}|Prefab:{PrefabId}]";

    /// <summary>
    ///     Decodes the global key into <see cref="ShardId" />, <see cref="ZoneId" /> and <see cref="Id" />.
    /// </summary>
    /// <param name="globalKey">The key to decode.</param>
    /// <returns>A 3-Tuple with the Ids.</returns>
    public static (ushort ShardId, ushort ZoneId, int Id) DecodeGlobalKey(long globalKey)
    {
        return (
            (ushort)(globalKey >> 48),
            (ushort)(globalKey >> 32),
            (int)globalKey
        );
    }

    /// <summary>
    ///     This will be called when the Entity is transferred into a new zone.
    /// </summary>
    /// <param name="newEntityId">The ID of the entity in the new zone.</param>
    /// <param name="zoneId">The ID of the new zone.</param>
    public void ZoneTransfer(int newEntityId, ushort zoneId)
    {
        ZoneId = zoneId;
        Id = newEntityId;
        ShardId = 0; // hardcode because no sharding for prototype
    }
}
