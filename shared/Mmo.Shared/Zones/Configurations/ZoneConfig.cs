using Mmo.Shared.Character.Enums;
using Mmo.Shared.Records;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Configurations;

/// <summary>
///     Statische Zone-Definition.  Wird aus Config geladen.
///     Ändert sich NICHT zur Laufzeit.
/// </summary>
public class ZoneConfig
{
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Eindeutige Zone-ID.</summary>
    public ushort ZoneId { get; set; }

    /// <summary>Interner Name (für Code).</summary>
    public string InternalName { get; set; } = "";

    /// <summary>Anzeigename (für UI).</summary>
    public string DisplayName { get; set; } = "";

    /// <summary>Beschreibung. </summary>
    public string Description { get; set; } = "";

    // ═══════════════════════════════════════════════════════════════
    // REGELN (statisch)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>PvP-Regeln dieser Zone.</summary>
    public PvpZoneType PvpType { get; set; } = PvpZoneType.Normal;

    /// <summary>Zone-Flags (NoMount, IsCapital, etc.).</summary>
    public ZoneFlags Flags { get; set; } = ZoneFlags.None;

    /// <summary>Empfohlenes Mindest-Level.</summary>
    public int MinLevel { get; set; } = 1;

    /// <summary>Empfohlenes Max-Level.</summary>
    public int MaxLevel { get; set; } = 60;

    /// <summary>Max. Spieler (0 = unbegrenzt).</summary>
    public int MaxPlayers { get; set; } = 0;

    /// <summary>Welche Fraktion "besitzt" die Zone? (null = neutral).</summary>
    public Faction? OwningFaction { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // BOUNDS & GEOMETRY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Zone-Grenzen.</summary>
    public ZoneBoundsConfig Bounds { get; set; } = new(0, 0, 1000, 1000);

    // ═══════════════════════════════════════════════════════════════
    // SPAWNS (statisch definiert)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Spieler-Spawn-Punkte.</summary>
    public List<SpawnPointConfig> PlayerSpawnPoints { get; set; } = new();

    /// <summary>NPC-Spawns.</summary>
    public List<NpcSpawnConfig> NpcSpawns { get; set; } = new();

    /// <summary>Spirit Healer Positionen.</summary>
    public List<Position> GraveyardPositions { get; set; } = new();

    // ═══════════════════════════════════════════════════════════════
    // ENVIRONMENT (Defaults)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Standard-Wetter.</summary>
    public WeatherType DefaultWeather { get; set; } = WeatherType.Clear;

    /// <summary>Musik-ID.</summary>
    public int? MusicId { get; set; }

    /// <summary>Ambiente-Sound-ID.</summary>
    public int? AmbienceId { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED (aus Flags)
    // ═══════════════════════════════════════════════════════════════

    public bool IsPvpEnabled => PvpType != PvpZoneType.Sanctuary;
    public bool IsSanctuary => PvpType == PvpZoneType.Sanctuary;
    public bool IsInstance => Flags.HasFlag(ZoneFlags.IsInstance);
    public bool IsRaid => Flags.HasFlag(ZoneFlags.IsRaid);
    public bool IsCapital => Flags.HasFlag(ZoneFlags.IsCapital);
    public bool HasRestXp => Flags.HasFlag(ZoneFlags.HasRestXp);
    public bool AllowsMounting => !Flags.HasFlag(ZoneFlags.NoMounting);
    public bool AllowsFlying => !Flags.HasFlag(ZoneFlags.NoFlying);
}
