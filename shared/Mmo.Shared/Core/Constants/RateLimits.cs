namespace Mmo.Shared.Core.Constants;

/// <summary>
/// This class references the limit of maximum requests a client can send to the server.
/// </summary>
public static class RateLimits
{
    // ═══════════════════════════════════════════════════════
    // MOVEMENT (highest frequency)
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Maximum Position Updates per Second.
    /// Even if the Client is at a higher framerate (e.g. 60 FPS) the server is only ticking with <see cref="SharedConstants.TickRate"/>.
    /// The server cannot process more inputs than this.
    /// </summary>
    public const int PositionUpdatePerSecond = 20;

    // ═══════════════════════════════════════════════════════
    // COMBAT
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Maximum Action Requests per Second.
    /// Normally each action (spell, attack, etc.) has a cooldown. But there are e.g. Instant Casts so the Limit needs to be a bit higher.
    /// </summary>
    public const int ActionRequestPerSecond = 10;

    /// <summary>
    /// Maximum Target Select Requests per Second.
    /// </summary>
    public const int TargetSelectPerSecond = 10;

    // ═══════════════════════════════════════════════════════
    // CHAT
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Maximum Send Chat Messages Per Second.
    /// </summary>
    public const int ChatMessagePerSecond = 3;
    /// <summary>
    /// Maximum Send Chat Messages Per Minute.
    /// </summary>
    public const int ChatMessagePerMinute = 30;

    // ═══════════════════════════════════════════════════════
    // AUTH / CRITICAL
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Maximum Login Requests Per Second.
    /// </summary>
    public const int LoginRequestPerMinute = 3;
    /// <summary>
    /// The cooldown after 3 failed attempts to login.
    /// </summary>
    public const int LoginRequestCooldownSeconds = 60;

    // ═══════════════════════════════════════════════════════
    // SYSTEM
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// The constant interval of the Ping Request.
    /// </summary>
    public const int PingPerSecond = 1;
    /// <summary>
    /// The constant interval of the Heartbeat Request.
    /// </summary>
    public const int HeartbeatIntervalSeconds = 5;

    // ═══════════════════════════════════════════════════════
    // GLOBAL FALLBACK
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// No special rule. Only the maximum requests per second.
    /// </summary>
    public const int GlobalPerSecond = 100;
}
