using Mmo.Shared.Enums;

namespace Mmo.Shared.Configurations.Zones;

/// <summary>
///     TODO:will be used in issue #123
/// </summary>
public class SpawnPointConfig
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Radius { get; init; }
    public Faction? ForFaction { get; set; } // null = alle
    public bool IsDefault { get; init; }
}
