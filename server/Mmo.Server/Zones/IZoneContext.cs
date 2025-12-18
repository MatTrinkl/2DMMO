using Mmo.Shared.Configurations.Zones;
using Mmo.Shared.Enums;

namespace Mmo.Server.Zones;

/// <summary>
///     Runtime-Kontext einer Zone.  Nur auf dem Server.
///     Enthält dynamischen Zustand der sich ständig ändert.
/// </summary>
public interface IZoneContext
{
    // ═══════════════════════════════════════════════════════════════
    // CONFIG (statisch - Referenz auf ZoneConfig)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Statische Zone-Konfiguration.</summary>
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

    /// <summary>Ist Sanctuary?</summary>
    bool IsSanctuary => Config.IsSanctuary;

    // ═══════════════════════════════════════════════════════════════
    // RUNTIME STATE (dynamisch)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Aktuelle Shard-ID.</summary>
    ushort ShardId { get; }

    /// <summary>Aktuelle Spieleranzahl.</summary>
    int PlayerCount { get; }

    /// <summary>Ist die Zone voll?</summary>
    bool IsFull => Config.MaxPlayers > 0 && PlayerCount >= Config.MaxPlayers;

    /// <summary>Aktuelles Wetter.</summary>
    WeatherType CurrentWeather { get; }

    /// <summary>Aktuelle Tageszeit (0.0 - 24.0).</summary>
    float TimeOfDay { get; }

    /// <summary>Ist es Nacht?  (20: 00 - 06:00)</summary>
    bool IsNight => TimeOfDay >= 20f || TimeOfDay < 6f;

    /// <summary>Welche Fraktion kontrolliert die Zone aktuell?</summary>
    Faction? ControllingFaction { get; }

    /// <summary>Ist die Zone gerade gesperrt?  (Wartung, Event, etc. )</summary>
    bool IsLocked { get; }

    /// <summary>Grund für Sperre.</summary>
    string? LockReason { get; }

    // ═══════════════════════════════════════════════════════════════
    // INSTANZ-SPEZIFISCH (nur wenn IsInstance)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Instanz-ID (nur bei Instanzen).</summary>
    Guid? InstanceId { get; }

    /// <summary>Instanz-Besitzer (Gruppe/Raid-Leader).</summary>
    Guid? InstanceOwnerId { get; }

    /// <summary>Wann wurde die Instanz erstellt?</summary>
    DateTime? InstanceCreatedAt { get; }

    /// <summary>Wann läuft die Instanz ab? </summary>
    DateTime? InstanceExpiresAt { get; }
}
