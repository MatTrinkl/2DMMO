using MessagePack;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Dtos;

/// <summary>
///     Delta DTO for zone context changes (weather, time of day, etc.).
///     This is a standalone DTO as it's not entity-specific.
/// </summary>
[MessagePackObject]
public class ZoneContextDelta
{
    [Key(0)]
    public WeatherType? CurrentWeather { get; set; }
    
    [Key(1)]
    public float? TimeOfDay { get; set; }
}
