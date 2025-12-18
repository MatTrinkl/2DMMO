namespace Mmo.Shared.Enums.Messages;

/// <summary>
///     Represents movement state flags (can be combined).
/// </summary>
[Flags]
public enum MovementFlags : byte
{
    /// <summary>
    ///     No movement flags set.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Character is walking.
    /// </summary>
    Walking = 1,

    /// <summary>
    ///     Character is running.
    /// </summary>
    Running = 2,

    /// <summary>
    ///     Character is moving backwards.
    /// </summary>
    Backwards = 4,

    /// <summary>
    ///     Character is strafing (moving sideways).
    /// </summary>
    Strafing = 8,

    /// <summary>
    ///     Character is jumping.
    /// </summary>
    Jumping = 16,

    /// <summary>
    ///     Character is falling.
    /// </summary>
    Falling = 32,

    /// <summary>
    ///     Character is swimming.
    /// </summary>
    Swimming = 64,

    /// <summary>
    ///     Character is mounted on a mount.
    /// </summary>
    Mounted = 128
}
