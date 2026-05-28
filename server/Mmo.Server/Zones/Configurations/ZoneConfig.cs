using Mmo.Shared.Character.Enums;
using Mmo.Shared.Movement.Records;
using Mmo.Shared.Zones.Configurations;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Interfaces;
using Mmo.Shared.Zones.Records;

namespace Mmo.Server.Zones.Configurations;

/// <summary>
///     Static zone definition. Loaded from config.
///     Does NOT change at runtime.
/// </summary>
public record ZoneConfig : IZoneConfig
{
    /// <summary>Description.</summary>
    public string Description { get; init; } = "";

    /// <summary>Max. Spieler (0 = unbegrenzt).</summary>
    public int MaxPlayers { get; init; } = 0;

    public bool Instantiable { get; init; } = false;

    // ═══════════════════════════════════════════════════════════════
    // SPAWNS (statisch definiert)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Spieler-Spawn-Punkte.</summary>
    public List<SpawnPointConfig> PlayerSpawnPoints { get; init; } = new();

    /// <summary>NPC-Spawns.</summary>
    public List<NpcSpawnConfig> NpcSpawns { get; init; } = new();

    /// <summary>Spirit Healer Positionen.</summary>
    public List<Position> GraveyardPositions { get; init; } = new();

    // ═══════════════════════════════════════════════════════════════
    // ENVIRONMENT (Defaults)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Standard-Wetter.</summary>
    public WeatherType DefaultWeather { get; init; } = WeatherType.Clear;
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Unique zone ID.</summary>
    public ushort ZoneId { get; init; }

    /// <summary>Internal name (for code).</summary>
    public string InternalName { get; init; } = "";

    /// <summary>Display name (for UI).</summary>
    public string DisplayName { get; init; } = "";

    /// <summary>
    ///     Is this zone contestable and the faction who is controlling it can change?
    /// </summary>
    public bool Contestable { get; init; } = false;

    // ═══════════════════════════════════════════════════════════════
    // REGELN (statisch)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>PvP-Regeln dieser Zone.</summary>
    public PvpZoneType PvpType { get; init; } = PvpZoneType.Normal;

    /// <summary>Zone-Flags (NoMount, IsCapital, etc.).</summary>
    public ZoneFlags ZoneFlags { get; init; } = ZoneFlags.None;

    /// <summary>Empfohlenes Mindest-Level.</summary>
    public int MinLevel { get; init; } = 1;

    /// <summary>Empfohlenes Max-Level.</summary>
    public int MaxLevel { get; init; } = 60;

    /// <summary>Which faction "owns" the zone? This can be the city of faction xyz. (null = neutral).</summary>
    public Faction? OwningFaction { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // BOUNDS & GEOMETRY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Zone-Grenzen.</summary>
    public ZoneBoundsConfig Bounds { get; init; } = new(0, 0, 1000, 1000);

    /// <summary>Musik-ID.</summary>
    public int? MusicId { get; init; }

    /// <summary>Ambiente-Sound-ID.</summary>
    public int? AmbienceId { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED (aus Flags)
    // ═══════════════════════════════════════════════════════════════

    public bool IsPvpEnabled => PvpType != PvpZoneType.Sanctuary;
    public bool IsSanctuary => PvpType == PvpZoneType.Sanctuary;
    public bool IsInstance => ZoneFlags.HasFlag(ZoneFlags.IsInstance);
    public bool IsRaid => ZoneFlags.HasFlag(ZoneFlags.IsRaid);
    public bool IsCapital => ZoneFlags.HasFlag(ZoneFlags.IsCapital);
    public bool HasRestXp => ZoneFlags.HasFlag(ZoneFlags.HasRestXp);
    public bool AllowsMounting => !ZoneFlags.HasFlag(ZoneFlags.NoMounting);
    public bool AllowsFlying => !ZoneFlags.HasFlag(ZoneFlags.NoFlying);
}
