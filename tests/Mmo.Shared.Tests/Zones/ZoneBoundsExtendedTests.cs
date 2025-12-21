using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Shared.Tests.Zones;

public class ZoneBoundsExtendedTests
{
    [Fact]
    public void Contains_PositionInsideBounds_ReturnsTrue()
    {
        var bounds = new ZoneBounds(0, 0, 100, 100);

        Assert.True(bounds.Contains(50, 50));
        Assert.True(bounds.Contains(0, 0)); // Left-top corner
        Assert.True(bounds.Contains(100, 100)); // Right-bottom corner
        Assert.True(bounds.Contains(100, 0)); // Right-top corner
        Assert.True(bounds.Contains(0, 100)); // Left-bottom corner
    }

    [Fact]
    public void Contains_PositionOutsideBounds_ReturnsFalse()
    {
        var bounds = new ZoneBounds(0, 0, 100, 100);

        Assert.False(bounds.Contains(-1, 50)); // Left of bounds
        Assert.False(bounds.Contains(101, 50)); // Right of bounds
        Assert.False(bounds.Contains(50, -1)); // Above bounds
        Assert.False(bounds.Contains(50, 101)); // Below bounds
        Assert.False(bounds.Contains(-10, -10)); // Far outside
    }

    [Fact]
    public void Contains_PositionStruct_InsideBounds_ReturnsTrue()
    {
        var bounds = new ZoneBounds(10, 20, 50, 60);
        var position = new Position(30, 40);

        Assert.True(bounds.Contains(position));
    }

    [Fact]
    public void Contains_PositionStruct_OutsideBounds_ReturnsFalse()
    {
        var bounds = new ZoneBounds(10, 20, 50, 60);
        var position = new Position(100, 100);

        Assert.False(bounds.Contains(position));
    }

    [Fact]
    public void Clamp_PositionInsideBounds_ReturnsOriginal()
    {
        var bounds = new ZoneBounds(0, 0, 100, 100);
        var position = new Position(50, 50);

        Position clamped = bounds.Clamp(position);

        Assert.Equal(50, clamped.X);
        Assert.Equal(50, clamped.Y);
    }

    [Fact]
    public void Clamp_PositionOutsideBounds_ClampsToBounds()
    {
        var bounds = new ZoneBounds(0, 0, 100, 100);

        Position clampedLeft = bounds.Clamp(new Position(-10, 50));
        Assert.Equal(0, clampedLeft.X);
        Assert.Equal(50, clampedLeft.Y);

        Position clampedRight = bounds.Clamp(new Position(110, 50));
        Assert.Equal(100, clampedRight.X);
        Assert.Equal(50, clampedRight.Y);

        Position clampedTop = bounds.Clamp(new Position(50, -10));
        Assert.Equal(50, clampedTop.X);
        Assert.Equal(0, clampedTop.Y);

        Position clampedBottom = bounds.Clamp(new Position(50, 110));
        Assert.Equal(50, clampedBottom.X);
        Assert.Equal(100, clampedBottom.Y);
    }

    [Fact]
    public void IsNearEdge_PositionNearEdge_ReturnsTrue()
    {
        var bounds = new ZoneBounds(0, 0, 100, 100);
        float margin = 10;

        Assert.True(bounds.IsNearEdge(5, 50, margin)); // Near left edge
        Assert.True(bounds.IsNearEdge(95, 50, margin)); // Near right edge
        Assert.True(bounds.IsNearEdge(50, 5, margin)); // Near top edge
        Assert.True(bounds.IsNearEdge(50, 95, margin)); // Near bottom edge
        Assert.True(bounds.IsNearEdge(5, 5, margin)); // Near corner
    }

    [Fact]
    public void IsNearEdge_PositionFarFromEdge_ReturnsFalse()
    {
        var bounds = new ZoneBounds(0, 0, 100, 100);
        float margin = 10;

        Assert.False(bounds.IsNearEdge(50, 50, margin)); // Center
        Assert.False(bounds.IsNearEdge(20, 50, margin)); // Not near left
        Assert.False(bounds.IsNearEdge(80, 50, margin)); // Not near right
        Assert.False(bounds.IsNearEdge(50, 20, margin)); // Not near top
        Assert.False(bounds.IsNearEdge(50, 80, margin)); // Not near bottom
    }

    [Fact]
    public void IsNearEdge_PositionFarOutsideBounds_StillChecksDistance()
    {
        var bounds = new ZoneBounds(0, 0, 100, 100);
        float margin = 10;

        // Positions just outside but within margin - returns TRUE
        Assert.True(bounds.IsNearEdge(-5, 50, margin)); // Outside left by 5, within margin of 10
        Assert.True(bounds.IsNearEdge(105, 50, margin)); // Outside right by 5, within margin of 10
        Assert.True(bounds.IsNearEdge(50, -5, margin)); // Outside top by 5, within margin of 10
        Assert.True(bounds.IsNearEdge(50, 105, margin)); // Outside bottom by 5, within margin of 10
    }

    [Fact]
    public void Bounds_Dimensions_CalculateCorrectly()
    {
        var bounds = new ZoneBounds(10, 20, 50, 60);

        float width = bounds.MaxX - bounds.MinX;
        float height = bounds.MaxY - bounds.MinY;

        Assert.Equal(40, width); // 50 - 10 = 40
        Assert.Equal(40, height); // 60 - 20 = 40
    }

    [Fact]
    public void Bounds_WithNegativeCoordinates_WorksCorrectly()
    {
        var bounds = new ZoneBounds(-50, -50, 50, 50);

        Assert.True(bounds.Contains(0, 0));
        Assert.True(bounds.Contains(-25, 25));
        Assert.False(bounds.Contains(-51, 0));
        Assert.False(bounds.Contains(0, 51));

        float width = bounds.MaxX - bounds.MinX;
        float height = bounds.MaxY - bounds.MinY;
        Assert.Equal(100, width);
        Assert.Equal(100, height);
    }
}
