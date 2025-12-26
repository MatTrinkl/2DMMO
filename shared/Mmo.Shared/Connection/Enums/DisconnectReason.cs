namespace Mmo.Shared.Connection.Enums;

/// <summary>
///     Reasons why a client might disconnect.
/// </summary>
public enum DisconnectReason
{
    /// <summary>Client closed connection gracefully.</summary>
    ClientDisconnected,

    /// <summary>Connection timed out (no heartbeat).</summary>
    Timeout,

    /// <summary>Network error occurred.</summary>
    NetworkError,

    /// <summary>Server is shutting down.</summary>
    ServerShutdown,

    /// <summary>Client was kicked by server.</summary>
    Kicked,

    /// <summary>Protocol violation or invalid data.</summary>
    ProtocolError,
    /// <summary>
    /// Client was banned from the server.
    /// </summary>
    Banned,
    /// <summary>
    /// The server is shutting down for maintenance.
    /// </summary>
    Maintenance,
    /// <summary>
    /// There is another session with the same account.
    /// </summary>
    DuplicateLogin,
    /// <summary>
    /// The client have an incapable version of the game.
    /// </summary>
    VersionMismatch
}
