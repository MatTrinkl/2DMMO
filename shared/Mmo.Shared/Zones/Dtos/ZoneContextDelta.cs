using MessagePack;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Dtos;

/// <summary>
///     Delta DTO containing only zone-context changes (weather, time, etc.).
///     This is used to update the environmental state of the zone.
/// </summary>
/// <remarks>
///     <para>
///     This DTO is used in the ZoneDelta message system to efficiently transmit
///     zone-wide environmental changes. All fields are nullable - only changed properties are included.
///     </para>
///     
///     <para>
///     <strong>Extensibility for Context Deltas:</strong>
///     This pattern can be extended for other context-level changes:
///     - ServerContextDelta (server-wide events, maintenance warnings)
///     - GuildContextDelta (guild-wide announcements, bank access)
///     - PartyContextDelta (party leader change, loot mode)
///     - RaidContextDelta (raid warnings, boss phase changes)
///     </para>
///     
///     <para>
///     The nullable pattern ensures minimal bandwidth usage by only sending
///     properties that have actually changed.
///     </para>
/// </remarks>
/// <example>
/// <code>
/// // Send only weather change
/// var delta = new ZoneContextDelta
/// {
///     CurrentWeather = WeatherType.Rain,
///     TimeOfDay = null // hasn't changed
/// };
/// 
/// // Send only time change
/// var delta2 = new ZoneContextDelta
/// {
///     CurrentWeather = null, // hasn't changed
///     TimeOfDay = 18.5f // 6:30 PM
/// };
/// </code>
/// </example>
[MessagePackObject]
public class ZoneContextDelta
{
    /// <summary>
    ///     Gets or sets the current weather type (nullable).
    ///     Null if weather hasn't changed.
    /// </summary>
    [Key(0)]
    public WeatherType? CurrentWeather { get; set; }
    
    /// <summary>
    ///     Gets or sets the time of day in hours (0.0 - 24.0, nullable).
    ///     Null if time hasn't changed.
    /// </summary>
    /// <remarks>
    ///     Examples: 
    ///     - 0.0 = midnight
    ///     - 6.0 = 6:00 AM
    ///     - 12.0 = noon
    ///     - 18.5 = 6:30 PM
    ///     - 23.99 = 11:59 PM
    /// </remarks>
    [Key(1)]
    public float? TimeOfDay { get; set; }
}
