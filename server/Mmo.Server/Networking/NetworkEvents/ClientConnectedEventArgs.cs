namespace Mmo.Server.Networking.NetworkEvents;

/// <summary>
///     Event arguments for when a client connects to the server.
/// </summary>
public class ClientConnectedEventArgs(Guid clientId, string remoteEndPoint) : EventArgs
{
    /// <summary>
    ///     Unique identifier for this client connection.
    /// </summary>
    public Guid ClientId { get; } = clientId;

    /// <summary>
    ///     The remote IP: Port of the connected client.
    /// </summary>
    public string RemoteEndPoint { get; } = remoteEndPoint;

    /// <summary>
    ///     Timestamp when the client connected.
    /// </summary>
    public DateTimeOffset ConnectedAt { get; } = DateTimeOffset.UtcNow;
}
