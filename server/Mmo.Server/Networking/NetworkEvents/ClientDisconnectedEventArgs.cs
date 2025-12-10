using Mmo.Shared.Enums;

namespace Mmo.Server.Networking.NetworkEvents;

/// <summary>
///     Event arguments for when a client disconnects from the server.
/// </summary>
public class ClientDisconnectedEventArgs(Guid clientId, DisconnectReason reason) : EventArgs
{
    /// <summary>
    ///     Unique identifier for the disconnected client.
    /// </summary>
    public Guid ClientId { get; } = clientId;

    /// <summary>
    ///     The reason for disconnection.
    /// </summary>
    public DisconnectReason Reason { get; } = reason;

    /// <summary>
    ///     Timestamp when the client disconnected.
    /// </summary>
    public DateTimeOffset DisconnectedAt { get; } = DateTimeOffset.UtcNow;
}
