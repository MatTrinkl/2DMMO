namespace Mmo.Shared.Configurations.Zones;

/// <summary>
///     A configuration of a Zone. This is used when saving them to JSON.
/// </summary>
/// <param name="zoneId">The ID of the zone.</param>
/// <param name="zoneName">The Name of the zone.</param>
/// <param name="bounds">The border of the zone.</param>
/// <param name="spawnPoints">WIP: The SpawnPoints of the Zone.</param>
/// <param name="isDefault">Is this Zone the default zone.</param>
public class ZoneConfig(
    ushort zoneId,
    string zoneName,
    ZoneBoundsConfig bounds,
    SpawnPointConfig[] spawnPoints,
    bool isDefault)
{
    /// <summary>
    ///     The ID of the Zone.
    /// </summary>
    public ushort ZoneId { get; init; } = zoneId;

    /// <summary>
    ///     The Name of the Zone.
    /// </summary>
    public string ZoneName { get; init; } = zoneName;

    /// <summary>
    ///     The outer border of the zone.
    /// </summary>
    public ZoneBoundsConfig Bounds { get; init; } = bounds;

    /// <summary>
    ///     WIP: The spawn points of the Zone.
    /// </summary>
    public SpawnPointConfig[] SpawnPoints { get; init; } = spawnPoints;

    /// <summary>
    ///     Is this zone the default zone.
    /// </summary>
    public bool IsDefault { get; init; } = isDefault;
}
