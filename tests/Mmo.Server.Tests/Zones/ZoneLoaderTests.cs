using System.Text.Json;
using Mmo.Server.Zones.Configurations;
using Mmo.Server.Zones.Serialization;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Prefab;

namespace Mmo.Server.Tests.Zones;

public class ZoneLoaderTests
{
    private readonly string _testDirectory;

    public ZoneLoaderTests()
    {
        // Create temporary folder
        _testDirectory = Path.Combine(Path.GetTempPath(), "ZoneLoaderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    #region LoadZoneConfig Tests

    [Fact]
    public void LoadZoneConfig_ValidJson_ReturnsZoneConfig()
    {
        // Arrange
        const string json = """
                            {
                              "zoneId":  1,
                              "displayName":  "Startzone",
                              "internalName": "startzone",
                              "bounds": {
                                "minX": 0,
                                "maxX": 1600,
                                "minY": 0,
                                "maxY": 1600
                              },
                              "playerSpawnPoints": [
                                {
                                  "x": 800,
                                  "y": 800,
                                  "radius": 50,
                                  "isDefault": true
                                }
                              ]
                            }
                            """;
        string filePath = CreateTestFile("startzone.json", json);

        // Act
        ZoneConfig result = ZoneLoader.LoadZoneConfig(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((ushort)1, result.ZoneId);
        Assert.Equal("Startzone", result.DisplayName);
        Assert.Equal("startzone", result.InternalName);

        // Bounds
        Assert.NotNull(result.Bounds);
        Assert.Equal(0, result.Bounds.MinX);
        Assert.Equal(1600, result.Bounds.MaxX);
        Assert.Equal(0, result.Bounds.MinY);
        Assert.Equal(1600, result.Bounds.MaxY);

        // SpawnPoints
        Assert.Single(result.PlayerSpawnPoints);
        Assert.Equal(800, result.PlayerSpawnPoints[0].X);
        Assert.Equal(800, result.PlayerSpawnPoints[0].Y);
        Assert.Equal(50, result.PlayerSpawnPoints[0].Radius);
        Assert.True(result.PlayerSpawnPoints[0].IsDefault);
    }

    [Fact]
    public void LoadZoneConfig_MultipleSpawnPoints_AllLoaded()
    {
        // Arrange
        const string json = """
                            {
                              "zoneId": 2,
                              "displayName": "Hauptstadt",
                              "internalName": "hauptstadt",
                              "bounds": { "minX": 0, "maxX": 2048, "minY": 0, "maxY": 2048 },
                              "playerSpawnPoints": [
                                { "x": 1024, "y": 1024, "radius": 100, "isDefault":  true },
                                { "x":  500, "y": 1800, "radius": 40, "isDefault": false },
                                { "x": 1800, "y": 500, "radius": 40, "isDefault":  false }
                              ]
                            }
                            """;
        string filePath = CreateTestFile("hauptstadt.json", json);

        // Act
        ZoneConfig result = ZoneLoader.LoadZoneConfig(filePath);

        // Assert
        Assert.Equal(3, result.PlayerSpawnPoints.Count);
        Assert.Single(result.PlayerSpawnPoints, sp => sp.IsDefault);
    }

    [Fact]
    public void LoadZoneConfig_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_testDirectory, "nonexistent.json");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => ZoneLoader.LoadZoneConfig(nonExistentPath));
    }

    [Fact]
    public void LoadZoneConfig_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        const string invalidJson = "{ invalid json }";
        string filePath = CreateTestFile("invalid.json", invalidJson);

        // Act & Assert
        Assert.Throws<JsonException>(() => ZoneLoader.LoadZoneConfig(filePath));
    }

    [Fact]
    public void LoadZoneConfig_EmptyFile_ThrowsException()
    {
        // Arrange
        string filePath = CreateTestFile("empty.json", "");

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => ZoneLoader.LoadZoneConfig(filePath));
    }

    #endregion

    #region LoadAllZones Tests

    [Fact]
    public void LoadAllZones_MultipleFiles_ReturnsAllZones()
    {
        // Arrange
        CreateTestFile("zone1.json", """
                                     {
                                       "zoneId": 1,
                                       "displayName": "Zone 1",
                                       "internalName": "zone1",
                                       "bounds": { "minX": 0, "maxX": 100, "minY": 0, "maxY": 100 },
                                       "playerSpawnPoints": []
                                     }
                                     """);

        CreateTestFile("zone2.json", """
                                     {
                                       "zoneId": 2,
                                       "displayName": "Zone 2",
                                       "internalName": "zone2",
                                       "bounds":  { "minX": 0, "maxX": 200, "minY": 0, "maxY": 200 },
                                       "playerSpawnPoints": []
                                     }
                                     """);

        // Act
        var results = ZoneLoader.LoadAllZones(_testDirectory).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, z => z.DisplayName == "Zone 1");
        Assert.Contains(results, z => z.DisplayName == "Zone 2");
    }

    [Fact]
    public void LoadAllZones_EmptyDirectory_ReturnsEmpty()
    {
        // Arrange - Leeres Verzeichnis

        // Act
        var results = ZoneLoader.LoadAllZones(_testDirectory).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void LoadAllZones_DirectoryNotFound_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        string nonExistentDir = Path.Combine(_testDirectory, "nonexistent");

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => ZoneLoader.LoadAllZones(nonExistentDir).ToList());
    }

    [Fact]
    public void LoadAllZones_OnlyOneDefaultZone_Exists()
    {
        // Arrange
        CreateTestFile("default.json", """
                                       {
                                         "zoneId": 0,
                                         "displayName": "Default",
                                         "internalName": "default",
                                         "bounds": { "minX": 0, "maxX": 100, "minY": 0, "maxY": 100 },
                                         "playerSpawnPoints": []
                                       }
                                       """);

        CreateTestFile("other.json", """
                                     {
                                       "zoneId": 2,
                                       "displayName": "Other",
                                       "internalName": "other",
                                       "bounds": { "minX": 0, "maxX": 100, "minY": 0, "maxY": 100 },
                                       "playerSpawnPoints": []
                                     }
                                     """);

        // Act
        var results = ZoneLoader.LoadAllZones(_testDirectory).ToList();

        // Assert
        Assert.Single(results, z => z.ZoneId == 0);
    }

    #endregion

    #region Helper Methods

    private string CreateTestFile(string fileName, string content)
    {
        string filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    internal void Dispose()
    {
        if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, true);
    }

    #endregion
}
