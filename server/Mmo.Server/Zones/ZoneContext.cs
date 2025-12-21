using Mmo.Server.Zones.Interfaces;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Core;
using Mmo.Shared.Zones.Configurations;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.Zones;

/// <summary>
///     Konkrete Implementation von IZoneContext.
/// </summary>
public class ZoneContext(Zone zone, ZoneConfig config, ushort shardId = 0) : IZoneContext
{
    // ═══ Config ═══
    public ZoneConfig Config { get; } = config;

    // ═══ Runtime State ═══
    public ushort ShardId { get; } = shardId;
    public WeatherType CurrentWeather { get; set; } = config.DefaultWeather;
    public float TimeOfDay { get; set; } = 12f; // Default: Mittag
    public Faction? ControllingFaction { get; set; }
    public bool IsLocked { get; set; }
    public string? LockReason { get; set; }

    // ═══ Instanz ═══
    public Guid? InstanceId { get; set; }
    public Guid? InstanceOwnerId { get; set; }
    public DateTime? InstanceCreatedAt { get; set; }
    public DateTime? InstanceExpiresAt { get; set; }

    // ═══ Methods ═══

    public void SetWeather(WeatherType weather)
    {
        CurrentWeather = weather;
        // TODO: Broadcast WeatherChange to all players
    }

    public void Lock(string reason)
    {
        IsLocked = true;
        LockReason = reason;
    }

    public void Unlock()
    {
        IsLocked = false;
        LockReason = null;
    }
}
