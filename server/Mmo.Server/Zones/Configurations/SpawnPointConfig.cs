using Mmo.Shared.Character.Enums;

namespace Mmo.Shared.Zones.Configurations;

/// <summary>
///     TODO:will be used in issue #123
/// </summary>
public record SpawnPointConfig
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Radius { get; init; }
    public Faction? ForFaction { get; set; } // null = alle
    public bool IsDefault { get; init; }
}
