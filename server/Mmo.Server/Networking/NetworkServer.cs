using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Mmo.Server.Networking.NetworkEvents;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Networking;

/// <summary>
///     TCP server that manages client connections and routes messages.
/// </summary>
public sealed class NetworkServer(int port, ILog log) : INetworkServer
{
    private readonly ConcurrentDictionary<Guid, ClientConnection> _clients = new();
    private readonly ConcurrentDictionary<Guid, Guid> _playerToConnection = new();  // PlayerId → ConnId
    private bool _isDisposed;
    private TcpListener? _listener;

    /// <summary>
    ///     Number of currently connected clients.
    /// </summary>
    public int ClientCount => _clients.Count;

    /// <summary>
    ///     Returns true if the server is listening.
    /// </summary>
    public bool IsListening => _listener?.Server.IsBound ?? false;

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _listener?.Stop();

        foreach (ClientConnection connection in _clients.Values) connection.Dispose();

        _clients.Clear();
    }


    /// <summary>Fired when a new client connects. </summary>
    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    /// <summary>Fired when a client disconnects.</summary>
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

    /// <summary>Fired when a message is received from any client.</summary>
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>Fired when a network error occurs.</summary>
    public event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;


    /// <summary>
    ///     Starts the server and listens for incoming connections.
    /// </summary>
    /// <param name="cancellationToken">Token to stop the server.</param>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        log.Info("NetworkServer listening on port {Port}", port);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Await new connection
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);

                // Create ClientConnection
                var connection = new ClientConnection(tcpClient, log);
                _clients.TryAdd(connection.Id, connection);

                // Setup event handlers
                SetupConnectionEvents(connection);

                // Fire connected event
                OnClientConnected(new ClientConnectedEventArgs(connection.Id, connection.RemoteEndPoint));
                log.Info("Client {ClientId} connected from {EndPoint}", connection.Id, connection.RemoteEndPoint);

                // Start receive loop (fire and forget)
                _ = connection.StartReceivingAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            log.Info("NetworkServer shutting down.. .");
        }
        catch (Exception ex)
        {
            log.Error("NetworkServer error: {Error}", ex.Message);
            OnErrorOccurred(new NetworkErrorEventArgs(null, ex, "AcceptLoop"));
        }
        finally
        {
            await ShutdownAsync();
        }
    }

    /// <summary>
    ///     Sends a message to a specific client.
    /// </summary>
    /// <param name="clientId">The target client ID.</param>
    /// <param name="message">The message to send.</param>
    public async Task SendToClientAsync(Guid clientId, INetworkMessage message)
    {
        if (_clients.TryGetValue(clientId, out ClientConnection? connection))
            await connection.SendAsync(message);
        else
            log.Debug("Cannot send to unknown client {ClientId}", clientId);
    }

    /// <summary>
    ///     Broadcasts a message to all connected clients.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    public async Task BroadcastAsync(INetworkMessage message)
    {
        if (_clients.IsEmpty) return;

        IEnumerable<Task> tasks = _clients.Values.Select(c => c.SendAsync(message));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    ///     Broadcasts a message to all clients except one (e.g., the sender).
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <param name="excludeClientId">The client ID to exclude.</param>
    public async Task BroadcastExceptAsync(INetworkMessage message, Guid excludeClientId)
    {
        IEnumerable<Task> tasks = _clients
            .Where(kvp => kvp.Key != excludeClientId)
            .Select(kvp => kvp.Value.SendAsync(message));

        await Task.WhenAll(tasks);
    }

    /// <summary>
    ///     Kicks a client from the server.
    /// </summary>
    /// <param name="clientId">The client to kick.</param>
    public async Task KickClientAsync(Guid clientId)
    {
        if (_clients.TryGetValue(clientId, out ClientConnection? connection)) await connection.KickAsync();
    }

    /// <summary>
    ///     Gets all connected client IDs.
    /// </summary>
    public IEnumerable<Guid> GetConnectedClientIds() => _clients.Keys.ToList();

    /// <summary>
    ///     Checks if a client is connected.
    /// </summary>
    /// <param name="clientId">The client ID to check.</param>
    public bool IsClientConnected(Guid clientId) => _clients.ContainsKey(clientId);

    public void AssociatePlayer(Guid connectionId, Guid playerId)
    {
        _playerToConnection[playerId] = connectionId;
        if (_clients.TryGetValue(connectionId, out ClientConnection? conn))
        {
            conn. PlayerId = playerId;
        }
    }

    public void RemovePlayer(Guid playerId) => _playerToConnection.TryRemove(playerId, out _);

    // ══════════════════════════════════════════════════════════
    // EVENT INVOKERS
    // ══════════════════════════════════════════════════════════

    private void OnClientConnected(ClientConnectedEventArgs e) => ClientConnected?.Invoke(this, e);

    private void OnClientDisconnected(ClientDisconnectedEventArgs e) => ClientDisconnected?.Invoke(this, e);

    internal void OnMessageReceived(Guid connectionId, INetworkMessage message) => MessageReceived?.Invoke(this, new MessageReceivedEventArgs(connectionId, message, DateTimeOffset.UtcNow));

    private void OnErrorOccurred(NetworkErrorEventArgs e) => ErrorOccurred?.Invoke(this, e);

    /// <summary>
    ///     Sets up event handlers for a client connection.
    /// </summary>
    private void SetupConnectionEvents(ClientConnection connection)
    {
        // Message received
        connection.MessageReceived += message =>
        {
            OnMessageReceived(connection.Id, message);
        };

        // Client disconnected (with reason from ClientConnection)
        connection.Disconnected += reason => { HandleDisconnect(connection.Id, reason); };
    }

    /// <summary>
    ///     Handles client disconnection.
    /// </summary>
    private void HandleDisconnect(Guid clientId, DisconnectReason reason)
    {
        if (_clients.TryRemove(clientId, out ClientConnection? connection)) connection.Dispose();

        OnClientDisconnected(new ClientDisconnectedEventArgs(clientId, reason));
        log.Info("Client {ClientId} disconnected:  {Reason}", clientId, reason);
    }

    /// <summary>
    ///     Gracefully shuts down the server and disconnects all clients.
    /// </summary>
    private async Task ShutdownAsync()
    {
        log.Info("Shutting down NetworkServer, disconnecting {Count} clients...", _clients.Count);

        // Stop listening
        _listener?.Stop();

        // Disconnect all clients
        IEnumerable<Task> disconnectTasks = _clients.Values
            .Select(c => c.DisconnectAsync(DisconnectReason.ServerShutdown));

        await Task.WhenAll(disconnectTasks);

        // Dispose all connections
        foreach (ClientConnection connection in _clients.Values) connection.Dispose();

        _clients.Clear();

        log.Info("NetworkServer shutdown complete");
    }
}
