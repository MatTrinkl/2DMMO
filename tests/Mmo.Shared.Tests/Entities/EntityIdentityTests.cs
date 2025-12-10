using Mmo.Shared.Entities;

namespace Mmo.Shared.Tests.Entities;

public class EntityIdentityTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var identity = new EntityIdentity(42, 100, 5, 7);

        Assert.Equal(42, identity.Id);
        Assert.Equal(100, identity.ZoneId);
        Assert.Equal(5, identity.ShardId);
        Assert.Equal(7, identity.PrefabId);
    }

    [Fact]
    public void GlobalKey_CalculatesCorrectly()
    {
        var identity = new EntityIdentity(42, 100, 5, 7);

        // GlobalKey = ((long)ShardId << 48) | ((long)ZoneId << 32) | (uint)Id
        long expected = ((long)5 << 48) | ((long)100 << 32) | 42;

        Assert.Equal(expected, identity.GlobalKey);
    }

    [Fact]
    public void GlobalKey_WithMaxValues_DoesNotOverflow()
    {
        var identity = new EntityIdentity(int.MaxValue, ushort.MaxValue, ushort.MaxValue, 0);

        // Should not throw or overflow
        long globalKey = identity.GlobalKey;

        Assert.NotEqual(0, globalKey);
    }

    [Fact]
    public void ZoneTransfer_ChangesIdAndZoneId()
    {
        var identity = new EntityIdentity(42, 100, 5, 7);

        identity.ZoneTransfer(99, 200);

        Assert.Equal(99, identity.Id);
        Assert.Equal(200, identity.ZoneId);
        Assert.Equal(0, identity.ShardId); // ShardId is hardcoded to 0
        Assert.Equal(7, identity.PrefabId); // PrefabId remains unchanged
    }

    [Fact]
    public void ZoneTransfer_UpdatesGlobalKey()
    {
        var identity = new EntityIdentity(42, 100, 5, 7);
        long originalGlobalKey = identity.GlobalKey;

        identity.ZoneTransfer(99, 200);
        long newGlobalKey = identity.GlobalKey;

        Assert.NotEqual(originalGlobalKey, newGlobalKey);
    }

    [Fact]
    public void Equals_SameGlobalKeyAndPrefabId_ReturnsTrue()
    {
        var identity1 = new EntityIdentity(42, 100, 5, 7);
        var identity2 = new EntityIdentity(42, 100, 5, 7);

        Assert.True(identity1.Equals(identity2));
        Assert.True(identity1 == identity2);
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var identity1 = new EntityIdentity(42, 100, 5, 7);
        var identity2 = new EntityIdentity(43, 100, 5, 7);

        Assert.False(identity1.Equals(identity2));
        Assert.True(identity1 != identity2);
    }

    [Fact]
    public void Equals_DifferentZoneId_ReturnsFalse()
    {
        var identity1 = new EntityIdentity(42, 100, 5, 7);
        var identity2 = new EntityIdentity(42, 101, 5, 7);

        Assert.False(identity1.Equals(identity2));
    }

    [Fact]
    public void Equals_DifferentPrefabId_ReturnsFalse()
    {
        var identity1 = new EntityIdentity(42, 100, 5, 7);
        var identity2 = new EntityIdentity(42, 100, 5, 8);

        Assert.False(identity1.Equals(identity2));
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var identity1 = new EntityIdentity(42, 100, 5, 7);
        var identity2 = new EntityIdentity(42, 100, 5, 7);

        Assert.Equal(identity1.GetHashCode(), identity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_MayReturnDifferentHash()
    {
        var identity1 = new EntityIdentity(42, 100, 5, 7);
        var identity2 = new EntityIdentity(43, 101, 5, 7);

        // Note: Hash codes can collide, but they should differ for different values most of the time
        // We just verify it doesn't throw
        int hash1 = identity1.GetHashCode();
        int hash2 = identity2.GetHashCode();

        Assert.NotEqual(0, hash1);
        Assert.NotEqual(0, hash2);
    }

    [Fact]
    public void DecodeGlobalKey_ReturnsCorrectValues()
    {
        var identity = new EntityIdentity(42, 100, 5, 7);
        long globalKey = identity.GlobalKey;

        (ushort shardId, ushort zoneId, int id) = EntityIdentity.DecodeGlobalKey(globalKey);

        Assert.Equal(5, shardId);
        Assert.Equal(100, zoneId);
        Assert.Equal(42, id);
    }

    [Fact]
    public void DecodeGlobalKey_WithZeroValues_ReturnsZeros()
    {
        var identity = new EntityIdentity(0, 0, 0, 0);
        long globalKey = identity.GlobalKey;

        (ushort shardId, ushort zoneId, int id) = EntityIdentity.DecodeGlobalKey(globalKey);

        Assert.Equal(0, shardId);
        Assert.Equal(0, zoneId);
        Assert.Equal(0, id);
    }

    [Fact]
    public void DecodeGlobalKey_WithMaxValues_ReturnsCorrectValues()
    {
        var identity = new EntityIdentity(int.MaxValue, ushort.MaxValue, ushort.MaxValue, 0);
        long globalKey = identity.GlobalKey;

        (ushort shardId, ushort zoneId, int id) = EntityIdentity.DecodeGlobalKey(globalKey);

        Assert.Equal(ushort.MaxValue, shardId);
        Assert.Equal(ushort.MaxValue, zoneId);
        Assert.Equal(int.MaxValue, id);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var identity = new EntityIdentity(42, 100, 5, 7);

        string result = identity.ToString();

        Assert.Contains("100", result); // ZoneId
        Assert.Contains("42", result); // Id
        Assert.Contains("7", result); // PrefabId
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var identity = new EntityIdentity(42, 100, 5, 7);

        Assert.False(identity.Equals(null));
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var identity = new EntityIdentity(42, 100, 5, 7);
        object other = "not an EntityIdentity";

        Assert.False(identity.Equals(other));
    }
}
