using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Network;

/// <summary>
///     Manages all client connections.
///     IMPORTANT:
///     - Contains NO game logic!
///     - Creates ClientConnections and listens for their events
///     - Forwards events to the game layer
/// </summary>
public class NetworkServer(
    ILog log,
    int port = 7777) : IDisposable
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly ConcurrentDictionary<Guid, ClientConnection> _connections = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly TcpListener _listener = new(IPAddress.Any, port);

    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Number of active connections.
    /// </summary>
    public int ConnectionCount => _connections.Count;

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════
    // EVENTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Fired when a message is received from a client.</summary>
    public event Action<ClientConnection, MessageType, INetworkMessage>? OnMessageReceived;

    /// <summary>Fired when a client connects.</summary>
    public event Action<ClientConnection>? OnClientConnected;

    /// <summary>Fired when a client disconnects.</summary>
    public event Action<ClientConnection, string?>? OnClientDisconnected;

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════

    public void Start()
    {
        _listener.Start();
        log.Info("NetworkServer started on port {Port}",
            ((IPEndPoint)_listener.LocalEndpoint).Port);

        // Start accept loop
        _ = AcceptClientsAsync();
    }

    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();

        foreach (ClientConnection connection in _connections.Values) connection.Dispose();

        _connections.Clear();
        log.Info("NetworkServer stopped");
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    private async Task AcceptClientsAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
            try
            {
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token);

                // Create connection
                var connection = new ClientConnection(tcpClient, log);

                if (_connections.TryAdd(connection.Id, connection))
                {
                    log.Info("Client connected: {ConnectionId} from {Endpoint}",
                        connection.Id, connection.RemoteEndPoint);

                    // Listen for connection events
                    connection.OnMessageReceived += HandleConnectionMessage;
                    connection.OnDisconnected += HandleConnectionDisconnected;

                    // Fire event to game layer
                    OnClientConnected?.Invoke(connection);

                    // Connection starts receiving on its own!
                    connection.StartReceivingAsync();
                }
                else
                {
                    connection.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                // Server was stopped
                break;
            }
            catch (ObjectDisposedException)
            {
                // Server was stopped
                break;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error accepting client");
            }
    }

    /// <summary>
    ///     Called by ClientConnection when a message is received.
    /// </summary>
    private void HandleConnectionMessage(
        ClientConnection connection,
        MessageType type,
        INetworkMessage message)
    {
        // Forward event to game layer (GameServer)
        OnMessageReceived?.Invoke(connection, type, message);
    }

    /// <summary>
    ///     Called by ClientConnection when the connection is disconnected.
    /// </summary>
    private void HandleConnectionDisconnected(ClientConnection connection, string? reason)
    {
        // Unregister events
        connection.OnMessageReceived -= HandleConnectionMessage;
        connection.OnDisconnected -= HandleConnectionDisconnected;

        // Remove from dictionary
        _connections.TryRemove(connection.Id, out _);

        log.Info("Client disconnected: {ConnectionId} - {Reason}",
            connection.Id, reason ?? "Unknown");

        // Fire event to game layer
        OnClientDisconnected?.Invoke(connection, reason);

        // Cleanup connection
        connection.Dispose();
    }

    /// <summary>
    ///     Removes and disconnects a connection.
    /// </summary>
    public void RemoveConnection(Guid connectionId, string? reason = null)
    {
        if (_connections.TryRemove(connectionId, out ClientConnection? connection))
        {
            // Unregister events
            connection.OnMessageReceived -= HandleConnectionMessage;
            connection.OnDisconnected -= HandleConnectionDisconnected;

            log.Info("Removing connection: {ConnectionId} - {Reason}",
                connectionId, reason ?? "Unknown");

            // Fire event BEFORE dispose
            OnClientDisconnected?.Invoke(connection, reason);

            // Cleanup
            connection.Dispose();
        }
    }

    /// <summary>
    ///     Gets a connection by ID.
    /// </summary>
    public bool TryGetConnection(Guid connectionId, out ClientConnection? connection) =>
        _connections.TryGetValue(connectionId, out connection);

    /// <summary>
    ///     Gets all active connections.
    /// </summary>
    public IEnumerable<ClientConnection> GetAllConnections() => _connections.Values;

    /// <summary>
    ///     Sends a message to a connection.
    /// </summary>
    public void Send(ClientConnection connection, INetworkMessage message)
    {
        if (connection == null) return;

        try
        {
            connection.Send(message);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error sending to {ConnectionId}", connection.Id);
            RemoveConnection(connection.Id, "Send error");
        }
    }

    /// <summary>
    ///     Sends a message to a connection by ID.
    /// </summary>
    public bool Send(Guid connectionId, INetworkMessage message)
    {
        if (_connections.TryGetValue(connectionId, out ClientConnection? connection))
        {
            Send(connection, message);
            return true;
        }

        return false;
    }
}
