namespace Mmo.Shared.Enums;

/// <summary>
///     Represents weather types for zones.
/// </summary>
public enum WeatherType : byte
{
    /// <summary>
    ///     Clear weather (no precipitation).
    /// </summary>
    Clear = 0,

    /// <summary>
    ///     Cloudy weather.
    /// </summary>
    Cloudy = 1,

    /// <summary>
    ///     Light rain.
    /// </summary>
    Rain = 2,

    /// <summary>
    ///     Heavy rain.
    /// </summary>
    HeavyRain = 3,

    /// <summary>
    ///     Storm with lightning.
    /// </summary>
    Storm = 4,

    /// <summary>
    ///     Light snow.
    /// </summary>
    Snow = 5,

    /// <summary>
    ///     Heavy snow storm (blizzard).
    /// </summary>
    Blizzard = 6,

    /// <summary>
    ///     Foggy weather (reduced visibility).
    /// </summary>
    Fog = 7,

    /// <summary>
    ///     Desert sandstorm.
    /// </summary>
    Sandstorm = 8
}
