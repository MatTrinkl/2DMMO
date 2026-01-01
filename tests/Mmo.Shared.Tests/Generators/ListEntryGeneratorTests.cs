using Mmo.Shared.Tests.Generators.TestData;
using Mmo.Shared.Tests.Generators.TestData.Generated;

namespace Mmo.Shared.Tests.Generators;

/// <summary>
///     Tests for the ListEntryGenerator to verify generated ListEntry classes.
/// </summary>
public class ListEntryGeneratorTests
{
    [Fact]
    public void GenerateListEntry_ZoneListEntry_HasCorrectProperties()
    {
        // Arrange & Act
        var entry = new ZoneListEntry
        {
            Id = 1,
            Name = "Test Zone",
            RecommendedLevel = 10,
            Cost = 100
        };

        // Assert
        Assert.Equal((ushort)1, entry.Id);
        Assert.Equal("Test Zone", entry.Name);
        Assert.Equal((byte)10, entry.RecommendedLevel);
        Assert.Equal(100, entry.Cost);
    }

    [Fact]
    public void GenerateListEntry_ZoneMapEntry_HasCorrectProperties()
    {
        // Arrange & Act
        var entry = new ZoneMapEntry
        {
            Id = 2,
            Name = "Map Zone",
            IsDiscovered = true
        };

        // Assert
        Assert.Equal((ushort)2, entry.Id);
        Assert.Equal("Map Zone", entry.Name);
        Assert.True(entry.IsDiscovered);
    }

    [Fact]
    public void GenerateListEntry_ZoneDungeonEntry_HasCorrectProperties()
    {
        // Arrange & Act
        var entry = new ZoneDungeonEntry
        {
            Id = 3,
            Name = "Dungeon Zone",
            RecommendedLevel = 20,
            MaxPlayers = 5
        };

        // Assert
        Assert.Equal((ushort)3, entry.Id);
        Assert.Equal("Dungeon Zone", entry.Name);
        Assert.Equal((byte)20, entry.RecommendedLevel);
        Assert.Equal((ushort)5, entry.MaxPlayers);
    }

    [Fact]
    public void GenerateListEntry_ExtensionMethod_ToZoneListEntry_Works()
    {
        // Arrange
        var testData = new ZoneTestData
        {
            Id = 1,
            Name = "Test Zone",
            RecommendedLevel = 10,
            Cost = 100
        };

        // Act
        var entry = testData.ToZoneListEntry();

        // Assert
        Assert.Equal((ushort)1, entry.Id);
        Assert.Equal("Test Zone", entry.Name);
        Assert.Equal((byte)10, entry.RecommendedLevel);
        Assert.Equal(100, entry.Cost);
    }

    [Fact]
    public void GenerateListEntry_OptionalProperties_AreNullable()
    {
        // Arrange & Act - Creating an entry without setting optional properties
        var entry = new ZoneListEntry
        {
            Id = 1,
            Name = "Test Zone"
        };

        // Assert - Optional properties should be nullable
        Assert.Null(entry.RecommendedLevel);
        Assert.Null(entry.Cost);
    }

    [Fact]
    public void GenerateListEntry_BaseDataProperties_AreRequired()
    {
        // This test verifies that base data properties exist and are not nullable
        var entry = new ZoneListEntry();
        
        // These properties should exist and not be marked as nullable reference types
        Assert.IsType<ushort>(entry.Id);
        Assert.NotNull(entry.Name); // string is nullable by default, but should be present
    }
}
