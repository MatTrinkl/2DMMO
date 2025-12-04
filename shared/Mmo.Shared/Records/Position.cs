using MessagePack;

namespace Mmo.Shared.Records;

/// <summary>
/// Represents a Position on the map.
/// </summary>
/// <param name="X">The horizontal coordinate X.</param>
/// <param name="Y">The vertical coordinate Y.</param>
[MessagePackObject]
public record Position(
    [property: Key(0)] float X,
    [property: Key(1)] float Y
);
