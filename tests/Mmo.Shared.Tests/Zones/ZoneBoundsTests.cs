using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones;
using Mmo.Shared.Zones.Configurations;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Shared.Tests.Zones;

public class ZoneBoundsTests
{
    private readonly ZoneBounds _zoneBounds = new(0f, 0f, 100f, 100f);

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldSetBoundsCorrectly()
    {
        // Arrange & Act
        var bounds = new ZoneBounds(-10f, -20f, 30f, 40f);

        // Assert
        Assert.Equal(-10f, bounds.MinX);
        Assert.Equal(-20f, bounds.MinY);
        Assert.Equal(30f, bounds.MaxX);
        Assert.Equal(40f, bounds.MaxY);
    }

    #endregion

    #region FromConfig Tests

    [Fact]
    public void FromConfig_ShouldCreateZoneBoundsFromConfig()
    {
        // Arrange
        var config = new ZoneBoundsConfig(5, 10, 50, 60);

        // Act
        var bounds = ZoneBounds.FromConfig(config);

        // Assert
        Assert.Equal(config.MinX, bounds.MinX);
        Assert.Equal(config.MinY, bounds.MinY);
        Assert.Equal(config.MaxX, bounds.MaxX);
        Assert.Equal(config.MaxY, bounds.MaxY);
    }

    #endregion

    #region Contains (float x, float y) Tests

    [Theory]
    [InlineData(50f, 50f, true)] // Center - inside
    [InlineData(0f, 0f, true)] // Min corner - inclusive
    [InlineData(100f, 100f, true)] // Max corner - inclusive
    [InlineData(0f, 50f, true)] // Left edge
    [InlineData(100f, 50f, true)] // Right edge
    [InlineData(50f, 0f, true)] // Bottom edge
    [InlineData(50f, 100f, true)] // Top edge
    [InlineData(-1f, 50f, false)] // Outside left
    [InlineData(101f, 50f, false)] // Outside right
    [InlineData(50f, -1f, false)] // Outside bottom
    [InlineData(50f, 101f, false)] // Outside top
    [InlineData(-1f, -1f, false)] // Outside both min
    [InlineData(101f, 101f, false)] // Outside both max
    public void Contains_WithCoordinates_ShouldReturnExpectedResult(float x, float y, bool expected)
    {
        // Act
        bool result = _zoneBounds.Contains(x, y);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Contains (Position) Tests

    [Theory]
    [InlineData(50f, 50f, true)] // Center - inside
    [InlineData(0f, 0f, true)] // Min corner - inclusive
    [InlineData(100f, 100f, true)] // Max corner - inclusive
    [InlineData(-1f, 50f, false)] // Outside left
    [InlineData(101f, 50f, false)] // Outside right
    [InlineData(50f, -1f, false)] // Outside bottom
    [InlineData(50f, 101f, false)] // Outside top
    public void Contains_WithPosition_ShouldReturnExpectedResult(float x, float y, bool expected)
    {
        // Arrange
        var position = new Position(x, y);

        // Act
        bool result = _zoneBounds.Contains(position);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Clamp Tests

    [Theory]
    [InlineData(50f, 50f, 50f, 50f)] // Inside - no change
    [InlineData(-10f, 50f, 0f, 50f)] // Clamp X to MinX
    [InlineData(110f, 50f, 100f, 50f)] // Clamp X to MaxX
    [InlineData(50f, -10f, 50f, 0f)] // Clamp Y to MinY
    [InlineData(50f, 110f, 50f, 100f)] // Clamp Y to MaxY
    [InlineData(-10f, -10f, 0f, 0f)] // Clamp both to Min
    [InlineData(110f, 110f, 100f, 100f)] // Clamp both to Max
    [InlineData(-10f, 110f, 0f, 100f)] // Clamp X to Min, Y to Max
    [InlineData(0f, 0f, 0f, 0f)] // On MinX, MinY edge
    [InlineData(100f, 100f, 100f, 100f)] // On MaxX, MaxY edge
    public void Clamp_ShouldClampPositionToBounds(float inputX, float inputY, float expectedX, float expectedY)
    {
        // Arrange
        var position = new Position(inputX, inputY);

        // Act
        Position result = _zoneBounds.Clamp(position);

        // Assert
        Assert.Equal(expectedX, result.X);
        Assert.Equal(expectedY, result.Y);
    }

    #endregion

    #region IsNearEdge (Position, float threshold) Tests

    [Theory]
    [InlineData(100f, 100f, 5f, true)] // At max corner
    [InlineData(0f, 0f, 5f, true)] // At min corner
    [InlineData(3f, 50f, 5f, true)] // Near left edge (MinX)
    [InlineData(97f, 50f, 5f, true)] // Near right edge (MaxX)
    [InlineData(50f, 3f, 5f, true)] // Near bottom edge (MinY)
    [InlineData(50f, 97f, 5f, true)] // Near top edge (MaxY)
    [InlineData(50f, 50f, 5f, false)] // Center - not near any edge
    [InlineData(10f, 10f, 5f, false)] // Inside but not near edge
    public void IsNearEdge_WithPosition_ShouldReturnExpectedResult(float x, float y, float threshold, bool expected)
    {
        // Arrange
        var position = new Position(x, y);

        // Act
        bool result = _zoneBounds.IsNearEdge(position, threshold);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region IsNearEdge (float x, float y, float threshold) Tests

    [Theory]
    [InlineData(100f, 100f, 5f, true)] // At max corner
    [InlineData(0f, 0f, 5f, true)] // At min corner
    [InlineData(95f, 50f, 5f, true)] // Near right edge (MaxX)
    [InlineData(5f, 50f, 5f, true)] // Near left edge (MinX)
    [InlineData(50f, 95f, 5f, true)] // Near top edge (MaxY)
    [InlineData(50f, 5f, 5f, true)] // Near bottom edge (MinY)
    [InlineData(3f, 50f, 5f, true)] // Within threshold of MinX
    [InlineData(97f, 50f, 5f, true)] // Within threshold of MaxX
    [InlineData(50f, 3f, 5f, true)] // Within threshold of MinY
    [InlineData(50f, 97f, 5f, true)] // Within threshold of MaxY
    [InlineData(50f, 50f, 5f, false)] // Center - not near any edge
    [InlineData(10f, 10f, 5f, false)] // Inside but not near edge
    [InlineData(90f, 90f, 5f, false)] // Inside but not near edge
    [InlineData(6f, 6f, 5f, false)] // Just outside threshold from min
    [InlineData(94f, 94f, 5f, false)] // Just outside threshold from max
    public void IsNearEdge_WithCoordinates_ShouldReturnExpectedResult(float x, float y, float threshold, bool expected)
    {
        // Act
        bool result = _zoneBounds.IsNearEdge(x, y, threshold);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsNearEdge_WithZeroThreshold_ShouldOnlyReturnTrueAtExactEdge()
    {
        // Act & Assert
        Assert.True(_zoneBounds.IsNearEdge(0f, 50f, 0f)); // Exactly at MinX
        Assert.True(_zoneBounds.IsNearEdge(100f, 50f, 0f)); // Exactly at MaxX
        Assert.True(_zoneBounds.IsNearEdge(50f, 0f, 0f)); // Exactly at MinY
        Assert.True(_zoneBounds.IsNearEdge(50f, 100f, 0f)); // Exactly at MaxY
        Assert.False(_zoneBounds.IsNearEdge(50f, 50f, 0f)); // Center
    }

    [Fact]
    public void IsNearEdge_NearSingleEdge_ShouldReturnTrue()
    {
        // Near only one edge should still return true
        Assert.True(_zoneBounds.IsNearEdge(2f, 50f, 5f)); // Near MinX only
        Assert.True(_zoneBounds.IsNearEdge(98f, 50f, 5f)); // Near MaxX only
        Assert.True(_zoneBounds.IsNearEdge(50f, 2f, 5f)); // Near MinY only
        Assert.True(_zoneBounds.IsNearEdge(50f, 98f, 5f)); // Near MaxY only
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void ZoneBounds_WithNegativeCoordinates_ShouldWorkCorrectly()
    {
        // Arrange
        var bounds = new ZoneBounds(-100f, -100f, -50f, -50f);

        // Act & Assert
        Assert.True(bounds.Contains(-75f, -75f));
        Assert.True(bounds.Contains(-100f, -100f));
        Assert.True(bounds.Contains(-50f, -50f));
        Assert.False(bounds.Contains(-101f, -75f));
        Assert.False(bounds.Contains(-49f, -75f));
    }

    [Fact]
    public void ZoneBounds_WithZeroSize_ShouldContainOnlyOnePoint()
    {
        // Arrange
        var bounds = new ZoneBounds(10f, 10f, 10f, 10f);

        // Act & Assert
        Assert.True(bounds.Contains(10f, 10f));
        Assert.False(bounds.Contains(10.1f, 10f));
        Assert.False(bounds.Contains(10f, 10.1f));
        Assert.False(bounds.Contains(9.9f, 10f));
    }

    [Fact]
    public void Clamp_WithNegativeBounds_ShouldClampCorrectly()
    {
        // Arrange
        var bounds = new ZoneBounds(-50f, -50f, 50f, 50f);
        var outsidePosition = new Position(-100f, 100f);

        // Act
        Position result = bounds.Clamp(outsidePosition);

        // Assert
        Assert.Equal(-50f, result.X);
        Assert.Equal(50f, result.Y);
    }

    #endregion
}
