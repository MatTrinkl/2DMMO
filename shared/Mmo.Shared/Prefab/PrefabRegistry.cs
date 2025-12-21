using Mmo.Shared.Entities;
using Mmo.Shared.Prefab.Configurations;
using Mmo.Shared.Prefab.Serialization;

namespace Mmo.Shared.Prefab;

/// <summary>
///     Registry for managing prefab IDs loaded from configuration.
///     Provides both ID-based and name-based lookups.
/// </summary>
public class PrefabRegistry
{
    private readonly Dictionary<ushort, PrefabConfig> _prefabsById = new();
    private readonly Dictionary<string, PrefabConfig> _prefabsByName = new();

    /// <summary>
    ///     Creates a new instance of PrefabRegistry.
    ///     Use Instance for the global singleton.
    /// </summary>
    public PrefabRegistry()
    {
        // Public constructor for testing
    }

    /// <summary>
    ///     Gets the singleton instance of the PrefabRegistry.
    /// </summary>
    public static PrefabRegistry Instance { get; } = new();

    /// <summary>
    ///     Number of registered prefabs.
    /// </summary>
    public int Count => _prefabsById.Count;

    /// <summary>
    ///     Loads prefabs from a configuration file.
    /// </summary>
    /// <param name="configPath">Path to the prefabs.json file.</param>
    public void LoadFromFile(string configPath)
    {
        PrefabsConfig config = PrefabLoader.LoadPrefabConfig(configPath);
        LoadFromConfig(config);
    }

    /// <summary>
    ///     Loads prefabs from a configuration object.
    /// </summary>
    /// <param name="config">The prefabs configuration.</param>
    public void LoadFromConfig(PrefabsConfig config)
    {
        _prefabsById.Clear();
        _prefabsByName.Clear();

        foreach (PrefabConfig prefab in config.Prefabs)
        {
            if (_prefabsById.ContainsKey(prefab.Id))
                throw new InvalidOperationException($"Duplicate prefab ID: {prefab.Id}");

            string key = $"{prefab.Category}:{prefab.Name}";
            if (_prefabsByName.ContainsKey(key))
                throw new InvalidOperationException($"Duplicate prefab name: {key}");

            _prefabsById[prefab.Id] = prefab;
            _prefabsByName[key] = prefab;
        }
    }

    /// <summary>
    ///     Gets a prefab ID by category and name.
    /// </summary>
    /// <param name="category">The category (e.g., "player", "npc", "mob").</param>
    /// <param name="name">The name of the prefab.</param>
    /// <returns>The prefab ID.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the prefab is not found.</exception>
    public ushort GetId(string category, string name)
    {
        string key = $"{category}:{name}";
        if (_prefabsByName.TryGetValue(key, out PrefabConfig? prefab))
            return prefab.Id;

        throw new KeyNotFoundException($"Prefab not found: {key}");
    }

    /// <summary>
    ///     Gets a prefab configuration by ID.
    /// </summary>
    /// <param name="id">The prefab ID.</param>
    /// <returns>The prefab configuration.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the prefab is not found.</exception>
    public PrefabConfig GetConfig(ushort id)
    {
        if (_prefabsById.TryGetValue(id, out PrefabConfig? prefab))
            return prefab;

        throw new KeyNotFoundException($"Prefab ID not found: {id}");
    }

    /// <summary>
    ///     Tries to get a prefab ID by category and name.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="name">The name of the prefab.</param>
    /// <param name="id">The prefab ID if found.</param>
    /// <returns>True if the prefab was found, false otherwise.</returns>
    public bool TryGetId(string category, string name, out ushort id)
    {
        string key = $"{category}:{name}";
        if (_prefabsByName.TryGetValue(key, out PrefabConfig? prefab))
        {
            id = prefab.Id;
            return true;
        }

        id = 0;
        return false;
    }

    /// <summary>
    ///     Checks if a prefab ID exists in the registry.
    /// </summary>
    /// <param name="id">The prefab ID to check.</param>
    /// <returns>True if the prefab exists, false otherwise.</returns>
    public bool HasPrefab(ushort id) => _prefabsById.ContainsKey(id);

    /// <summary>
    ///     Gets all registered prefabs.
    /// </summary>
    /// <returns>Collection of all prefab configurations.</returns>
    public IEnumerable<PrefabConfig> GetAllPrefabs() => _prefabsById.Values;

    /// <summary>
    ///     Gets all prefabs in a specific category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>Collection of prefabs in the category.</returns>
    public IEnumerable<PrefabConfig> GetPrefabsByCategory(string category) =>
        _prefabsById.Values.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
}
