namespace Mmo. Shared.Enums;

/// <summary>
///     Flags für Account-Berechtigungen und -Status.
/// </summary>
[Flags]
public enum AccountFlags :  uint
{
    None = 0,

    // Status
    Verified = 1 << 0,           // Email verifiziert
    Premium = 1 << 1,            // Premium/VIP Account
    Founder = 1 << 2,            // Founder-Status

    // Moderation
    Muted = 1 << 4,              // Account ist gemutet
    Banned = 1 << 5,             // Account ist gebannt
    Suspended = 1 << 6,          // Account temporär gesperrt

    // Staff
    Moderator = 1 << 8,          // Moderator-Rechte
    GameMaster = 1 << 9,         // GM-Rechte
    Admin = 1 << 10,             // Admin-Rechte
    Developer = 1 << 11,         // Developer-Rechte

    // Special
    Streamer = 1 << 16,          // Streamer-Modus (Name verstecken etc.)
    ContentCreator = 1 << 17,    // Content Creator
    BetaTester = 1 << 18,        // Beta-Tester
}
