// server/Mmo.Server/Networking/INetworkServer.cs

using Mmo.Server.Networking.NetworkEvents;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Networking;

/// <summary>
///     Defines the contract for a network server that manages client connections and message transmission.
/// </summary>
/// <remarks>
///     The network server handles TCP connections, message serialization/deserialization,
///     and provides events for connection state changes and incoming messages.
///     It operates on a separate thread from the Game Loop to avoid blocking game logic.
/// </remarks>
public interface INetworkServer : IDisposable
{
    /// <summary>
    ///     Gets the current number of connected clients.
    /// </summary>
    int ClientCount { get; }

    /// <summary>
    ///     Gets a value indicating whether the server is currently listening for connections.
    /// </summary>
    bool IsListening { get; }

    /// <summary>
    ///     Occurs when a new client connects to the server.
    /// </summary>
    event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    /// <summary>
    ///     Occurs when a client disconnects from the server.
    /// </summary>
    event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

    /// <summary>
    ///     Occurs when a message is received from a client.
    /// </summary>
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    ///     Occurs when a network error is encountered.
    /// </summary>
    event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

    /// <summary>
    ///     Starts the network server and begins listening for client connections.
    /// </summary>
    /// <param name="cancellationToken">Token to signal server shutdown.</param>
    /// <returns>A task that completes when the server stops.</returns>
    Task RunAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Sends a message to a specific client.
    /// </summary>
    /// <param name="client">The target client connection.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>A task that completes when the message is sent.</returns>
    Task SendToClientAsync(ClientConnection client, INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all connected clients.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <returns>A task that completes when the message is sent to all clients.</returns>
    Task BroadcastAsync(INetworkMessage message);

    /// <summary>
    ///     Broadcasts a message to all connected clients except one.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <param name="excludeClientId">The client to exclude from the broadcast.</param>
    /// <returns>A task that completes when the message is sent to all applicable clients.</returns>
    Task BroadcastExceptAsync(INetworkMessage message, ClientConnection excludeClientId);

    /// <summary>
    ///     Disconnects a client from the server.
    /// </summary>
    /// <param name="clientId">The client to disconnect.</param>
    /// <returns>A task that completes when the client is disconnected.</returns>
    Task KickClientAsync(ClientConnection clientId);

    /// <summary>
    ///     Gets the collection of all connected client IDs.
    /// </summary>
    /// <returns>An enumerable of connected client GUIDs.</returns>
    IEnumerable<Guid> GetConnectedClientIds();

    /// <summary>
    ///     Determines whether a specific client is currently connected.
    /// </summary>
    /// <param name="clientId">The client connection to check.</param>
    /// <returns>True if the client is connected; otherwise, false.</returns>
    bool IsClientConnected(ClientConnection clientId);

    /// <summary>
    ///     Associates a client connection with a player's persistent ID.
    /// </summary>
    /// <param name="connectionId">The client connection.</param>
    /// <param name="playerId">The player's persistent ID.</param>
    /// <remarks>
    ///     This mapping is used to track which player entity corresponds to which network connection.
    /// </remarks>
    public void AssociatePlayer(ClientConnection connectionId, Guid playerId);

    /// <summary>
    ///     Removes a player association from the server.
    /// </summary>
    /// <param name="playerId">The client connection whose player association should be removed.</param>
    public void RemovePlayer(ClientConnection playerId);
}
