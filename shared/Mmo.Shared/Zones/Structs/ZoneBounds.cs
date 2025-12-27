using MessagePack;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones.Configurations;

namespace Mmo.Shared.Zones.Structs;

/// <summary>
///     Represents the outer border of a Zone. This is just the border no collision map!
/// </summary>
/// <param name="MinX">The left side of the X-Axis.</param>
/// <param name="MinY">The bottom side of the Y-Axis.</param>
/// <param name="MaxX">The right side of the X-Axis.</param>
/// <param name="MaxY">The top side of the Y-Axis.</param>
[MessagePackObject]
public readonly record struct ZoneBounds(float MinX, float MinY, float MaxX, float MaxY)
{
    /// <summary>
    ///     The right side of the X-Axis.
    /// </summary>
    [Key(0)] public readonly float MaxX = MaxX;

    /// <summary>
    ///     The top side of the Y-Axis.
    /// </summary>
    [Key(1)] public readonly float MaxY = MaxY;

    /// <summary>
    ///     The left side of the X-Axis.
    /// </summary>
    [Key(2)] public readonly float MinX = MinX;

    /// <summary>
    ///     The bottom side of the Y-Axis.
    /// </summary>
    [Key(3)] public readonly float MinY = MinY;

    public static ZoneBounds FromConfig(ZoneBoundsConfig config) =>
        new(config.MinX, config.MinY, config.MaxX, config.MaxY);

    /// <summary>
    ///     Min inclusive, max exclusive
    /// </summary>
    /// <param name="x">Current X Position.</param>
    /// <param name="y">Current Y Position.</param>
    /// <returns></returns>
    public bool Contains(float x, float y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;

    /// <summary>
    ///     Min inclusive, max exclusive
    /// </summary>
    /// <param name="position">Current Position.</param>
    /// <returns></returns>
    public bool Contains(Position position) =>
        position.X >= MinX && position.X <= MaxX && position.Y >= MinY && position.Y <= MaxY;

    /// <summary>
    ///     Clamps the current position in between this ZoneBounds.
    /// </summary>
    /// <param name="position">Current Position.</param>
    /// <returns>Returns the current Position clamped to this ZoneBounds.</returns>
    public Position Clamp(Position position) =>
        new(Math.Clamp(position.X, MinX, MaxX), Math.Clamp(position.Y, MinY, MaxY));


    /// <summary>
    ///     Checks if an Entity is getting too close to the edge of this ZoneBounds.
    /// </summary>
    /// <param name="x">Current X Position.</param>
    /// <param name="y">Current Y Position. </param>
    /// <param name="threshold">The distance to the edge. </param>
    /// <returns>Returns true if one coordinate is within the threshold to any edge.</returns>
    public bool IsNearEdge(float x, float y, float threshold) =>
        x - MinX <= threshold || MaxX - x <= threshold ||
        y - MinY <= threshold || MaxY - y <= threshold;

    /// <summary>
    ///     Checks if an Entity is getting too close to the edge of this ZoneBounds.
    /// </summary>
    /// <param name="pos">Current Position.</param>
    /// <param name="threshold">The distance to the edge.</param>
    /// <returns>Returns true if one coordinate is within the threshold to any edge.</returns>
    public bool IsNearEdge(Position pos, float threshold) =>
        pos.X - MinX <= threshold || MaxX - pos.X <= threshold ||
        pos.Y - MinY <= threshold || MaxY - pos.Y <= threshold;
}
