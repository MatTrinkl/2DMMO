namespace Mmo.Shared.Enums;

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
    ProtocolError
}
