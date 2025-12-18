namespace Mmo.Shared.Enums.Messages;

[Flags]
public enum MovementFlags : byte
{
    None = 0,
    Walking = 1,
    Running = 2,
    Backwards = 4,
    Strafing = 8,
    Jumping = 16,
    Falling = 32,
    Swimming = 64,
    Mounted = 128,
}
