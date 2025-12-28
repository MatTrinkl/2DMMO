namespace Mmo.Shared.Zones.Records;

/// <summary>
///     A configuration of a Border of a Zone. This is used when saving them to JSON.
/// </summary>
/// <param name="MinX">The left side of the X-Axis.</param>
/// <param name="MinY">The bottom side of the Y-Axis.</param>
/// <param name="MaxX">The right side of the X-Axis.</param>
/// <param name="MaxY">The top side of the Y-Axis.</param>
public record ZoneBoundsConfig(float MinX, float MinY, float MaxX, float MaxY)
{
}
