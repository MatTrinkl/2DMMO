using Mmo.Shared.Character.Enums;
using Mmo.Shared.Generators;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Interfaces;

/// <summary>
///     Runtime context of a zone. Server-only.
///     Contains dynamic state that constantly changes.
/// </summary>
[GenerateDto(DtoName = "ZoneContextDto")]
public interface IZoneContext
{
    // ═══════════════════════════════════════════════════════════════
    // RUNTIME STATE (dynamisch)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Current shard ID.</summary>
    ushort ShardId { get; }

    /// <summary>Current weather.</summary>
    WeatherType CurrentWeather { get; }

    /// <summary>Current time of day (0.0 - 24.0).</summary>
    float TimeOfDay { get; }

    /// <summary>Is it night? (20:00 - 06:00)</summary>
    bool IsNight => TimeOfDay is >= 20f or < 6f;

    /// <summary>Which faction currently controls the zone?</summary>
    Faction? ControllingFaction { get; }

    /// <summary>Is the zone currently locked? (Maintenance, Event, etc.)</summary>
    bool IsLocked { get; }

    /// <summary>Reason for lock.</summary>
    string? LockReason { get; }

    bool IsInstance { get; }
}
