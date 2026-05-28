using MessagePack;

namespace Mmo.Shared.Movement.Records;

/// <summary>
///     Represents a Velocity of an entity on the map.
/// </summary>
/// <param name="X">The horizontal velocity X.</param>
/// <param name="Y">The vertical velocity Y.</param>
[MessagePackObject]
public record Velocity(
    [property: Key(0)] float X,
    [property: Key(1)] float Y
);
