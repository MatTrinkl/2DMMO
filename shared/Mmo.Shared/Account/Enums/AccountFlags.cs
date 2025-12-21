namespace Mmo.Shared.Account.Enums;

/// <summary>
///     Flags for account permissions and status.
/// </summary>
[Flags]
public enum AccountFlags : uint
{
    /// <summary>
    ///     No special flags set.
    /// </summary>
    None = 0,

    // Status
    /// <summary>
    ///     Email has been verified.
    /// </summary>
    Verified = 1 << 0,

    /// <summary>
    ///     Premium/VIP account status.
    /// </summary>
    Premium = 1 << 1,

    /// <summary>
    ///     Founder status.
    /// </summary>
    Founder = 1 << 2,

    // Moderation
    /// <summary>
    ///     Account is muted (cannot chat).
    /// </summary>
    Muted = 1 << 4,

    /// <summary>
    ///     Account is permanently banned.
    /// </summary>
    Banned = 1 << 5,

    /// <summary>
    ///     Account is temporarily suspended.
    /// </summary>
    Suspended = 1 << 6,

    // Staff
    /// <summary>
    ///     Has moderator privileges.
    /// </summary>
    Moderator = 1 << 8,

    /// <summary>
    ///     Has Game Master (GM) privileges.
    /// </summary>
    GameMaster = 1 << 9,

    /// <summary>
    ///     Has administrator privileges.
    /// </summary>
    Admin = 1 << 10,

    /// <summary>
    ///     Has developer privileges.
    /// </summary>
    Developer = 1 << 11,

    // Special
    /// <summary>
    ///     Streamer mode (hide name, etc.).
    /// </summary>
    Streamer = 1 << 16,

    /// <summary>
    ///     Content creator status.
    /// </summary>
    ContentCreator = 1 << 17,

    /// <summary>
    ///     Beta tester status.
    /// </summary>
    BetaTester = 1 << 18
}
