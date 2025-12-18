namespace Mmo.Server.GameLoop;

/// <summary>
///     Server-Statistiken.
/// </summary>
public class ServerStats
{
    public long TickCount { get; init; }
    public TimeSpan Uptime { get; init; }
    public int TargetTickRate { get; init; }
    public double LastTickDurationMs { get; init; }
    public double AverageTickDurationMs { get; init; }
    public int PlayerCount { get; init; }
    public int ConnectionCount { get; init; }
    public int InputQueueSize { get; init; }
    public int OutputQueueSize { get; init; }
    public int CompletionQueueSize { get; init; }
}
