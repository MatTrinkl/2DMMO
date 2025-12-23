using Mmo.Shared.Character.Enums;
using Mmo.Shared.Zones.Configurations;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Server.Zones.Interfaces;

/// <summary>
///     Runtime context of a zone. Server-only.
///     Contains dynamic state that constantly changes.
/// </summary>
public interface IZoneContext
{
    // ═══════════════════════════════════════════════════════════════
    // CONFIG (statisch - Referenz auf ZoneConfig)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Static zone configuration.</summary>
    ZoneConfig Config { get; }

    // ═══════════════════════════════════════════════════════════════
    // SHORTCUTS zu Config (Convenience)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Zone-ID.</summary>
    ushort ZoneId => Config.ZoneId;

    /// <summary>Zone-Name.</summary>
    string Name => Config.DisplayName;

    /// <summary>PvP-Typ.</summary>
    PvpZoneType PvpType => Config.PvpType;

    /// <summary>Is Sanctuary?</summary>
    bool IsSanctuary => Config.IsSanctuary;

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
    bool IsNight => TimeOfDay >= 20f || TimeOfDay < 6f;

    /// <summary>Which faction currently controls the zone?</summary>
    Faction? ControllingFaction { get; }

    /// <summary>Is the zone currently locked? (Maintenance, Event, etc.)</summary>
    bool IsLocked { get; }

    /// <summary>Reason for lock.</summary>
    string? LockReason { get; }

    // ═══════════════════════════════════════════════════════════════
    // INSTANZ-SPEZIFISCH (nur wenn IsInstance)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Instance ID (only for instances).</summary>
    Guid? InstanceId { get; }

    /// <summary>Instance owner (Group/Raid Leader).</summary>
    Guid? InstanceOwnerId { get; }

    /// <summary>When was the instance created?</summary>
    DateTime? InstanceCreatedAt { get; }

    /// <summary>When does the instance expire?</summary>
    DateTime? InstanceExpiresAt { get; }
}
