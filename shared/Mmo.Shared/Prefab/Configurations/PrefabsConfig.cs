namespace Mmo.Shared.Prefab.Configurations;

/// <summary>
///     Root configuration object for all prefabs.
/// </summary>
/// <param name="prefabs">Array of all prefab configurations.</param>
public class PrefabsConfig(PrefabConfig[] prefabs)
{
    /// <summary>
    ///     Array of all prefab configurations.
    /// </summary>
    public PrefabConfig[] Prefabs { get; init; } = prefabs;
}
