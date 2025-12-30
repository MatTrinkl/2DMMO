namespace Mmo.Shared.Zones.Enums;

/// <summary>
///     Flags indicating which properties of a zone have changed.
///     These flags are used by the dirty-tracking system to optimize zone-wide updates.
///     Use bitwise OR to combine multiple flags.
/// </summary>
/// <remarks>
///     This enum is specific to zone-level tracking. Entity-specific changes use EntityDirtyFlags.
///     The uint type provides 32 possible flags (bits 0-31).
/// </remarks>
[Flags]
public enum ZoneDirtyFlags : uint
{
    /// <summary>
    ///     No properties have changed.
    /// </summary>
    None = 0,
    
    // ═══════════════════════════════════════════════════════════════
    // ENVIRONMENT FLAGS (Bits 0-7)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     Weather conditions have changed (clear, rain, snow, etc.).
    /// </summary>
    Weather = 1 << 0,
    
    /// <summary>
    ///     Time of day has changed (day/night cycle).
    /// </summary>
    TimeOfDay = 1 << 1,
    
    /// <summary>
    ///     Fog settings have changed (density, color).
    /// </summary>
    Fog = 1 << 2,
    
    /// <summary>
    ///     Lighting settings have changed (ambient, directional).
    /// </summary>
    Lighting = 1 << 3,
    
    // ═══════════════════════════════════════════════════════════════
    // AUDIO FLAGS (Bits 8-11)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     Background music has changed.
    /// </summary>
    Music = 1 << 8,
    
    /// <summary>
    ///     Ambient sound effects have changed (wind, water, etc.).
    /// </summary>
    Ambience = 1 << 9,
    
    // ═══════════════════════════════════════════════════════════════
    // ZONE STATE FLAGS (Bits 12-19)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     Controlling faction has changed (territory ownership).
    /// </summary>
    ControllingFaction = 1 << 12,
    
    /// <summary>
    ///     PvP state has changed (enabled/disabled).
    /// </summary>
    PvPState = 1 << 13,
    
    /// <summary>
    ///     Special event is active or ended.
    /// </summary>
    EventActive = 1 << 14,
    
    /// <summary>
    ///     Zone phase has changed (phasing system for quest progression).
    /// </summary>
    Phase = 1 << 15,
    
    // ═══════════════════════════════════════════════════════════════
    // CONVENIENCE COMBINATIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     All environment-related properties (Weather, TimeOfDay, Fog, Lighting).
    /// </summary>
    AllEnvironment = Weather | TimeOfDay | Fog | Lighting,
    
    /// <summary>
    ///     All audio-related properties (Music, Ambience).
    /// </summary>
    AllAudio = Music | Ambience,
    
    /// <summary>
    ///     All zone state properties (Faction, PvP, Events, Phase).
    /// </summary>
    AllState = ControllingFaction | PvPState | EventActive | Phase,
    
    /// <summary>
    ///     All flags set.
    /// </summary>
    All = ~None
}
