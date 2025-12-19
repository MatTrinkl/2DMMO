using System.Text.Json;
using Mmo.Shared.Configurations.Zones;

namespace Mmo.Shared.Zones;

/// <summary>
///     Loads Zones from JSON Config.
/// </summary>
public static class ZoneLoader
{
    /// <summary>
    ///     The option of the deserialization.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true
    };

    /// <summary>
    ///     Load a <see cref="ZoneConfig" /> from a JSON file.
    /// </summary>
    /// <param name="configPath">Path to the JSON File.</param>
    /// <returns>Returns the loaded Configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the path leads to a file that does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown when the path doesn't end with .json.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file isn't a <see cref="ZoneConfig" />.</exception>
    public static ZoneConfig LoadZoneConfig(string configPath)
    {
        if (!File.Exists(configPath))
            throw new FileNotFoundException($"File not found: {configPath}");
        if (!string.Equals(Path.GetExtension(configPath), ".json", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only .json files are supported.", nameof(configPath));


        string json = File.ReadAllText(configPath);
        ZoneConfig? config = JsonSerializer.Deserialize<ZoneConfig>(json, _options);

        return config ?? throw new InvalidOperationException($"Invalid Zone-Config: {configPath}");
    }

    /// <summary>
    ///     Returns an IEnumerable with all Zones loaded from a directory.
    /// </summary>
    /// <param name="zonesDirectory">Directory to load from</param>
    /// <returns>Returns all zones from the directory.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory is not found.</exception>
    public static IEnumerable<ZoneConfig> LoadAllZones(string zonesDirectory)
    {
        if (!Directory.Exists(zonesDirectory))
            throw new DirectoryNotFoundException($"Zones-Directory not found: {zonesDirectory}");

        string[] zoneFiles = Directory.GetFiles(zonesDirectory, "*.json");

        foreach (string file in zoneFiles) yield return LoadZoneConfig(file);
    }

    /// <summary>
    ///     Creates a Zone from its configuration.
    /// </summary>
    /// <param name="config">The configuration of the zone.</param>
    /// <returns>The zone from the config.</returns>
    public static Zone CreateZoneFromConfig(ZoneConfig config)
    {
        var bounds = ZoneBounds.FromConfig(config.Bounds);
        var newZone = new Zone(config.ZoneId, config.InternalName, bounds);
        return newZone;
    }
}
