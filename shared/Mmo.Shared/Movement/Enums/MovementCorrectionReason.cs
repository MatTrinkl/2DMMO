namespace Mmo.Shared.Movement.Enums;

/// <summary>
/// Reason for server-side position correction.
/// </summary>
public enum MovementCorrectionReason : byte
{
    None = 0,
    Collision = 1, // Client position inside wall/object
    SpeedTooHigh = 2, // Velocity exceeds MAX_SPEED * TOLERANCE
    OutOfBounds = 3, // Position outside zone boundaries
    Stuck = 4, // Server detected stuck state
    AntiCheat = 5 // General anti-cheat correction
}
