namespace Mmo.Server.Core.Structs;

/// <summary>
///     Server statistics.
/// </summary>
public readonly record struct ServerStats(
    long TickCount,
    TimeSpan Uptime,
    double LastTickDurationsMs,
    double AverageTickDurationsMs,
    int PlayerCount,
    int ConnectionCount,
    int InputQueueSize,
    int OutputQueueSize,
    int CompletionQueueSize)
{
    /// <summary>
    ///     The current tickcount of the server.
    /// </summary>
    public long TickCount { get; init; } = TickCount;

    /// <summary>
    ///     The Uptime of the server.
    /// </summary>
    public TimeSpan Uptime { get; init; } = Uptime;

    /// <summary>
    ///     The time the last tick needed. Should be 1/20 of a second.
    /// </summary>
    public double LastTickDurationMs { get; init; } = LastTickDurationsMs;

    /// <summary>
    ///     The time a average tick needs. Should be max. 1/20 of a second.
    /// </summary>
    public double AverageTickDurationMs { get; init; } = AverageTickDurationsMs;

    /// <summary>
    ///     Current active player count.
    /// </summary>
    public int PlayerCount { get; init; } = PlayerCount;

    /// <summary>
    ///     Current Connection Count. Should be the same as player count.
    /// </summary>
    public int ConnectionCount { get; init; } = ConnectionCount;

    /// <summary>
    ///     Current size of the input queue.
    /// </summary>
    public int InputQueueSize { get; init; } = InputQueueSize;

    /// <summary>
    ///     Current size of the output queue.
    /// </summary>
    public int OutputQueueSize { get; init; } = OutputQueueSize;

    /// <summary>
    ///     Current size of the async task (outside the tick) queue.
    /// </summary>
    public int CompletionQueueSize { get; init; } = CompletionQueueSize;
}
