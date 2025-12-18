using MessagePack;
using Mmo.Shared.Entities;
using Mmo.Shared.Records;

namespace Mmo.Shared.Tests.Entities;

public class EntityIdentitySerializationTests
{
    [Fact]
    public void EntityIdentity_Serialization_PreservesPrefabId()
    {
        var original = new EntityIdentity(1, 100, 5, 42, 7);

        byte[] bytes = MessagePackSerializer.Serialize(original);
        EntityIdentity deserialized = MessagePackSerializer.Deserialize<EntityIdentity>(bytes);

        Assert.Equal(original.ServerId, deserialized.ServerId);
        Assert.Equal(original.ZoneId, deserialized.ZoneId);
        Assert.Equal(original.ShardId, deserialized.ShardId);
        Assert.Equal(original.LocalId, deserialized.LocalId);
        Assert.Equal(original.PrefabId, deserialized.PrefabId);
        Assert.Equal(7, deserialized.PrefabId);
    }

    [Fact]
    public void PlayerEntity_Serialization_PreservesRuntimeIdWithPrefabId()
    {
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "TestPlayer", new Position(10, 20));

        // Verify PrefabId is set correctly before serialization
        Assert.Equal(PrefabIds.PlayerDefault, player.RuntimeId.PrefabId);
        Assert.Equal(1, player.RuntimeId.PrefabId);

        byte[] bytes = MessagePackSerializer.Serialize(player);
        PlayerEntity deserialized = MessagePackSerializer.Deserialize<PlayerEntity>(bytes);

        // Verify PrefabId is preserved after serialization
        Assert.Equal(PrefabIds.PlayerDefault, deserialized.RuntimeId.PrefabId);
        Assert.Equal(1, deserialized.RuntimeId.PrefabId);
    }
}
