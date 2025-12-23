namespace Mmo.Shared.Movement.Enums;

/// <summary>
///     Types of movement violations (Anti-Cheat).
/// </summary>
public enum MovementViolationType
{
    None,
    SpeedHack, // Moved too fast
    Teleport, // Position jump without teleport request
    WallClip, // Moved through wall
    FlyHack, // Flying without permission
    NoClip, // Through terrain
    OutOfBounds, // Outside zone boundaries
    InvalidPosition // Invalid coordinates (NaN, etc.)
}
