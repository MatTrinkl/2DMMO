namespace Mmo.Server.Networking.NetworkEvents;

/// <summary>
///     Event arguments for network errors.
/// </summary>
public class NetworkErrorEventArgs(Guid? clientId, Exception exception, string context) : EventArgs
{
    /// <summary>
    ///     The client associated with the error (null if server-level error).
    /// </summary>
    public Guid? ClientId { get; } = clientId;

    /// <summary>
    ///     The exception that occurred.
    /// </summary>
    public Exception Exception { get; } = exception;

    /// <summary>
    ///     Context describing where the error occurred.
    /// </summary>
    public string Context { get; } = context;

    /// <summary>
    ///     Timestamp when the error occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
