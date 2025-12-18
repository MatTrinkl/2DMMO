using Mmo.Shared;

namespace Mmo.Server.GameLoop;

/// <summary>
///     Server statistics.
/// </summary>
public class ServerStats
{
    /// <summary>
    /// The current tickcount of the server.
    /// </summary>
    public long TickCount { get; init; } = 0;

    /// <summary>
    /// The Uptime of the server.
    /// </summary>
    public TimeSpan Uptime { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// The current tick rate (should not be changed).
    /// </summary>
    public int TargetTickRate { get; init; } = SharedConstants.TickRate;

    /// <summary>
    /// The time the last tick needed. Should be 1/20 of a second.
    /// </summary>
    public double LastTickDurationMs { get; init; } = 0;

    /// <summary>
    /// The time a average tick needs. Should be max. 1/20 of a second.
    /// </summary>
    public double AverageTickDurationMs { get; init; } = 0;

    /// <summary>
    /// Current active player count.
    /// </summary>
    public int PlayerCount { get; init; } = 0;

    /// <summary>
    /// Current Connection Count. Should be the same as player count.
    /// </summary>
    public int ConnectionCount { get; init; } = 0;

    /// <summary>
    /// Current size of the input queue.
    /// </summary>
    public int InputQueueSize { get; init; } = 0;
    /// <summary>
    /// Current size of the output queue.
    /// </summary>
    public int OutputQueueSize { get; init; } = 0;
    /// <summary>
    /// Current size of the async task (outside the tick) queue.
    /// </summary>
    public int CompletionQueueSize { get; init; } = 0;
}
