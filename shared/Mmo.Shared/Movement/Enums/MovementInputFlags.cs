namespace Mmo.Shared.Movement.Enums;

/// <summary>
/// Bit flags for movement input state, sent with PositionUpdate.
/// </summary>
[Flags]
public enum MovementInputFlags : byte
{
    None = 0,
    Forward = 1,      // W key
    Backward = 2,     // S key
    Left = 4,         // A key
    Right = 8,        // D key
    Jump = 16,        // Space key
    Sprint = 32       // Shift key
}
