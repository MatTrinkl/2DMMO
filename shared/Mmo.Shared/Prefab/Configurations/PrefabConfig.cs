namespace Mmo.Shared.Prefab.Configurations;

/// <summary>
///     Configuration for a single prefab definition.
/// </summary>
/// <param name="id">The unique ID of the prefab.</param>
/// <param name="name">The name/identifier of the prefab.</param>
/// <param name="category">The category this prefab belongs to (e.g., "player", "npc", "mob").</param>
/// <param name="description">Optional description of the prefab.</param>
public class PrefabConfig(ushort id, string name, string category, string? description = null)
{
    /// <summary>
    ///     The unique ID of the prefab.
    /// </summary>
    public ushort Id { get; init; } = id;

    /// <summary>
    ///     The name/identifier of the prefab (e.g., "playerDefault", "goblin").
    /// </summary>
    public string Name { get; init; } = name;

    /// <summary>
    ///     The category this prefab belongs to (e.g., "player", "npc", "mob", "object").
    /// </summary>
    public string Category { get; init; } = category;

    /// <summary>
    ///     Optional description of the prefab.
    /// </summary>
    public string? Description { get; init; } = description;
}
