using MessagePack;

namespace Mmo.Shared.Movement.Records;

/// <summary>
///     Represents a Position on the map.
/// </summary>
/// <param name="X">The horizontal coordinate X.</param>
/// <param name="Y">The vertical coordinate Y.</param>
[MessagePackObject]
public record Position(
    [property: Key(0)] float X,
    [property: Key(1)] float Y
)
{
    /// <summary>
    ///     Calculates the distance from this position to another one.
    /// </summary>
    /// <param name="otherPosition">The distance to get the distance from.</param>
    /// <returns>The distance between this position and <see cref="otherPosition" />.</returns>
    public float CalculateDistance(Position otherPosition)
    {
        float dx = X - otherPosition.X;
        float dy = Y - otherPosition.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
