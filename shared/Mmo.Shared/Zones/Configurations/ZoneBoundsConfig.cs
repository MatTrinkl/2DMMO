namespace Mmo.Shared.Zones.Configurations;

/// <summary>
///     A configuration of a Border of a Zone. This is used when saving them to JSON.
/// </summary>
/// <param name="minX">The left side of the X-Axis.</param>
/// <param name="minY">The bottom side of the Y-Axis.</param>
/// <param name="maxX">The right side of the X-Axis.</param>
/// <param name="maxY">The top side of the Y-Axis.</param>
public class ZoneBoundsConfig(float minX, float minY, float maxX, float maxY)
{
    /// <summary>The left side of the X-Axis.</summary>
    public float MinX { get; init; } = minX;

    /// <summary>The right side of the X-Axis.</summary>
    public float MaxX { get; init; } = maxX;

    /// <summary>The bottom side of the Y-Axis.</summary>
    public float MinY { get; init; } = minY;

    /// <summary>The top side of the Y-Axis.</summary>
    public float MaxY { get; init; } = maxY;
}
