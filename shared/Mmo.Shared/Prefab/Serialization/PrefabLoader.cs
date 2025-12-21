using System.Text.Json;
using Mmo.Shared.Prefab.Configurations;

namespace Mmo.Shared.Prefab.Serialization;

/// <summary>
///     Loads Prefab configurations from JSON files.
/// </summary>
public static class PrefabLoader
{
    /// <summary>
    ///     The JSON serializer options.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true
    };

    /// <summary>
    ///     Load prefab configuration from a JSON file.
    /// </summary>
    /// <param name="configPath">Path to the JSON file.</param>
    /// <returns>Returns the loaded configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the path leads to a file that does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown when the path doesn't end with .json.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file isn't a valid prefab config.</exception>
    public static PrefabsConfig LoadPrefabConfig(string configPath)
    {
        if (!File.Exists(configPath))
            throw new FileNotFoundException($"File not found: {configPath}");
        if (!string.Equals(Path.GetExtension(configPath), ".json", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only .json files are supported.", nameof(configPath));

        string json = File.ReadAllText(configPath);
        PrefabsConfig? config = JsonSerializer.Deserialize<PrefabsConfig>(json, _options);

        return config ?? throw new InvalidOperationException($"Invalid Prefab-Config: {configPath}");
    }
}
