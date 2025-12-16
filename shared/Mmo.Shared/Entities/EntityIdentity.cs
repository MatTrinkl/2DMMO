using MessagePack;

namespace Mmo.Shared.Entities;

/// <summary>
///     This class is used to identify an entity.
/// </summary>
[MessagePackObject]
public struct EntityIdentity : IEquatable<EntityIdentity>
{
    /// <summary>
    ///     Server ID - identifies the game server/region (for multi-region support).
    ///     Prototype default: 1
    /// </summary>
    [Key(0)]
    public byte ServerId { get; private set; }

    /// <summary>
    ///     The ID of the Zone where the Entity exists currently.
    /// </summary>
    [Key(1)]
    public ushort ZoneId { get; private set; }

    /// <summary>
    ///     Shard ID within the zone (for horizontal scaling).
    ///     Prototype default: 0
    /// </summary>
    [Key(2)]
    public ushort ShardId { get; private set; }

    /// <summary>
    ///     Local ID - unique within a zone/shard pair. Assigned by the zone.
    /// </summary>
    [Key(3)]
    public int LocalId { get; private set; }

    /// <summary>
    ///     Reference to the prefab of this entity. Defines the type/template.
    ///     See <see cref="PrefabIds" /> for available prefabs.
    /// </summary>
    [Key(4)]
    public ushort PrefabId { get; }

    /// <summary>
    ///     Creates a new EntityIdentity struct.
    /// </summary>
    /// <param name="serverId">The ID of the server/region.</param>
    /// <param name="zoneId">The ID of the zone.</param>
    /// <param name="shardId">The ID of the shard of the zone.</param>
    /// <param name="localId">The local ID of the Entity within the zone/shard.</param>
    /// <param name="prefabId">The ID of the prefab of this entity.</param>
    [SerializationConstructor]
    public EntityIdentity(byte serverId, ushort zoneId, ushort shardId, int localId, ushort prefabId)
    {
        ServerId = serverId;
        ZoneId = zoneId;
        ShardId = shardId;
        LocalId = localId;
        PrefabId = prefabId;
    }

    /// <summary>
    ///     The global ID of the entity. This is unique at every time. This ID will change when a zone or shard is changed.
    ///     Format: (ServerId << 56) | (ZoneId << 40) | (ShardId << 24) | (LocalId & 0 xFFFFFF)
    /// </summary>
    [IgnoreMember]
    public long GlobalKey =>
        ((long)ServerId << 56) | ((long)ZoneId << 40) | ((long)ShardId << 24) | (LocalId & 0xFFFFFF);


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
        => HashCode.Combine(ServerId, ZoneId, ShardId, LocalId);

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
    /// <returns>
    ///     Returns a string with format: "Entity[Server:{ServerId} Zone:{ZoneId} Shard:{ShardId} Local:{LocalId}
    ///     Prefab:{PrefabId}]".
    /// </returns>
    public override string ToString()
        => $"Entity[Server:{ServerId} Zone:{ZoneId} Shard:{ShardId} Local:{LocalId} Prefab:{PrefabId}]";

    /// <summary>
    ///     Decodes the global key into <see cref="ServerId" />, <see cref="ZoneId" />, <see cref="ShardId" /> and
    ///     <see cref="LocalId" />.
    /// </summary>
    /// <param name="globalKey">The key to decode.</param>
    /// <returns>A 4-Tuple with the Ids.</returns>
    public static (byte ServerId, ushort ZoneId, ushort ShardId, int LocalId) DecodeGlobalKey(long globalKey)
    {
        return (
            (byte)(globalKey >> 56),
            (ushort)(globalKey >> 40),
            (ushort)(globalKey >> 24),
            (int)(globalKey & 0xFFFFFF)
        );
    }

    /// <summary>
    ///     This will be called when the Entity is transferred into a new zone.
    /// </summary>
    /// <param name="newLocalId">The new local ID in the target zone.</param>
    /// <param name="zoneId">The ID of the new zone.</param>
    public void ZoneTransfer(int newLocalId, ushort zoneId)
    {
        ZoneId = zoneId;
        LocalId = newLocalId;
        ShardId = 0; // hardcode because no sharding for prototype
    }

    /// <summary>
    ///     Creates an unassigned EntityIdentity (for entities not yet added to a zone).
    /// </summary>
    /// <param name="prefabId">The prefab ID of the entity.</param>
    /// <returns>An EntityIdentity with all location fields set to 0.</returns>
    public static EntityIdentity Unassigned(ushort prefabId) => new(0, 0, 0, 0, prefabId);

    /// <summary>
    ///     Checks if this EntityIdentity has been assigned to a zone.
    /// </summary>
    [IgnoreMember]
    public bool IsAssigned => ZoneId > 0 && LocalId > 0;

    /// <summary>
    ///     Transfers this entity to a different server.
    /// </summary>
    /// <param name="newServerId">The target server ID.</param>
    public void TransferToServer(byte newServerId) => ServerId = newServerId;

    /// <summary>
    ///     Transfers this entity to a different zone on the same server.
    /// </summary>
    /// <param name="newZoneId">The target zone ID.</param>
    /// <param name="newLocalId">The new local ID in the target zone.</param>
    public void TransferToZone(ushort newZoneId, int newLocalId)
    {
        ZoneId = newZoneId;
        LocalId = newLocalId;
    }

    /// <summary>
    ///     Transfers this entity to a different shard within the same zone.
    /// </summary>
    /// <param name="newShardId">The target shard ID.</param>
    /// <param name="newLocalId">The new local ID in the target shard.</param>
    public void TransferToShard(ushort newShardId, int newLocalId)
    {
        ShardId = newShardId;
        LocalId = newLocalId;
    }
}
