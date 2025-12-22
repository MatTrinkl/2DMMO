namespace Mmo.Shared.Movement.Enums;

/// <summary>
///     Arten von Movement-Verletzungen (Anti-Cheat).
/// </summary>
public enum MovementViolationType
{
    None,
    SpeedHack, // Zu schnell bewegt
    Teleport, // Position-Sprung ohne Teleport-Request
    WallClip, // Durch Wand bewegt
    FlyHack, // Fliegen ohne Erlaubnis
    NoClip, // Durch Terrain
    OutOfBounds, // Außerhalb der Zone-Grenzen
    InvalidPosition // Ungültige Koordinaten (NaN, etc.)
}
