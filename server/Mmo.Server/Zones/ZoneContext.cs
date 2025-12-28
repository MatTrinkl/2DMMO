using Mmo.Server.Zones.Configurations;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Interfaces;

namespace Mmo.Server.Zones;

/// <summary>
///     Concrete implementation of IZoneContext.
/// </summary>
public class ZoneContext(ZoneConfig config, ushort shardId = 0) : IZoneContext
{
    // ═══════════════════════════════════════════════════════════════
    // INSTANCE-SPECIFIC (only when IsInstance)
    // ═══════════════════════════════════════════════════════════════
    public Guid? InstanceId { get; set; }
    public Guid? InstanceOwnerId { get; set; }
    public DateTime? InstanceCreatedAt { get; set; }

    public DateTime? InstanceExpiresAt { get; set; }

    // ═══ Runtime State ═══
    public ushort ShardId { get; } = shardId;
    public WeatherType CurrentWeather { get; private set; } = config.DefaultWeather;
    public float TimeOfDay { get; set; } = 12f; // Default: Mittag
    public Faction? ControllingFaction { get; set; } = config.OwningFaction;
    public bool IsLocked { get; private set; }
    public string? LockReason { get; private set; }
    public bool IsInstance { get; init; }


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

    public static ZoneContext CreateFromConfig(ZoneConfig config)
    {
        var context = new ZoneContext(config)
        {
            ControllingFaction = config.OwningFaction, CurrentWeather = config.DefaultWeather,
            IsInstance = config.IsInstance
        };
        return context;
    }
}
