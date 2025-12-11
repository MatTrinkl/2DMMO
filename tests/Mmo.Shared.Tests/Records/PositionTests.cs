using MessagePack;
using Mmo.Shared.Records;

namespace Mmo.Shared.Tests.Records;

public class PositionTests
{
    [Fact]
    public void Constructor_SetsXAndY()
    {
        var position = new Position(123.45f, 678.90f);

        Assert.Equal(123.45f, position.X);
        Assert.Equal(678.90f, position.Y);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var position1 = new Position(100, 200);
        var position2 = new Position(100, 200);

        Assert.Equal(position1, position2);
        Assert.True(position1 == position2);
        Assert.False(position1 != position2);
    }

    [Fact]
    public void Equality_DifferentX_AreNotEqual()
    {
        var position1 = new Position(100, 200);
        var position2 = new Position(101, 200);

        Assert.NotEqual(position1, position2);
        Assert.False(position1 == position2);
        Assert.True(position1 != position2);
    }

    [Fact]
    public void Equality_DifferentY_AreNotEqual()
    {
        var position1 = new Position(100, 200);
        var position2 = new Position(100, 201);

        Assert.NotEqual(position1, position2);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var position1 = new Position(50, 75);
        var position2 = new Position(50, 75);

        Assert.Equal(position1.GetHashCode(), position2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_MayReturnDifferentHash()
    {
        var position1 = new Position(50, 75);
        var position2 = new Position(51, 76);

        // Hash codes can collide, but we verify they're computed
        int hash1 = position1.GetHashCode();
        int hash2 = position2.GetHashCode();

        Assert.NotEqual(0, hash1);
        Assert.NotEqual(0, hash2);
    }

    [Fact]
    public void Position_WithZeroValues_IsValid()
    {
        var position = new Position(0, 0);

        Assert.Equal(0, position.X);
        Assert.Equal(0, position.Y);
    }

    [Fact]
    public void Position_WithNegativeValues_IsValid()
    {
        var position = new Position(-100, -200);

        Assert.Equal(-100, position.X);
        Assert.Equal(-200, position.Y);
    }

    [Fact]
    public void Position_WithMaxValues_IsValid()
    {
        var position = new Position(float.MaxValue, float.MaxValue);

        Assert.Equal(float.MaxValue, position.X);
        Assert.Equal(float.MaxValue, position.Y);
    }

    [Fact]
    public void Position_WithMinValues_IsValid()
    {
        var position = new Position(float.MinValue, float.MinValue);

        Assert.Equal(float.MinValue, position.X);
        Assert.Equal(float.MinValue, position.Y);
    }

    [Fact]
    public void MessagePack_Serialization_RoundTrip()
    {
        var original = new Position(123.456f, 789.012f);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        Position deserialized = MessagePackSerializer.Deserialize<Position>(serialized);

        Assert.Equal(original.X, deserialized.X);
        Assert.Equal(original.Y, deserialized.Y);
    }

    [Fact]
    public void MessagePack_Serialization_WithZeroValues_RoundTrip()
    {
        var original = new Position(0, 0);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        Position deserialized = MessagePackSerializer.Deserialize<Position>(serialized);

        Assert.Equal(0, deserialized.X);
        Assert.Equal(0, deserialized.Y);
    }

    [Fact]
    public void MessagePack_Serialization_WithNegativeValues_RoundTrip()
    {
        var original = new Position(-50.5f, -100.25f);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        Position deserialized = MessagePackSerializer.Deserialize<Position>(serialized);

        Assert.Equal(original.X, deserialized.X);
        Assert.Equal(original.Y, deserialized.Y);
    }

    [Fact]
    public void ToString_ReturnsReadableFormat()
    {
        var position = new Position(42.5f, 99.9f);

        string result = position.ToString();

        // Records have automatic ToString implementation
        Assert.Contains("42", result);
        Assert.Contains("99", result);
    }

    [Fact]
    public void Deconstruct_ExtractsXAndY()
    {
        var position = new Position(10, 20);

        (float x, float y) = position;

        Assert.Equal(10, x);
        Assert.Equal(20, y);
    }
}
