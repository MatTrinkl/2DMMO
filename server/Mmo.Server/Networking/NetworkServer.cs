using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Networking;

/// <summary>
///     Verwaltet alle Client-Connections.
///
///     WICHTIG:
///     - Kennt KEINE Game-Logik!
///     - Erstellt ClientConnections und hört auf deren Events
///     - Leitet Events nach außen weiter
/// </summary>
public class NetworkServer(
    ILog log,
    int port = 7777) : IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Any, port);
    private readonly CancellationTokenSource _cts = new();

    // ═══ CONNECTIONS ═══
    private readonly ConcurrentDictionary<Guid, ClientConnection> _connections = new();

    // ═══ EVENTS (für Game-Layer) ═══

    /// <summary>Wird gefeuert wenn eine Message empfangen wurde.</summary>
    public event Action<ClientConnection, MessageType, INetworkMessage>? OnMessageReceived;

    /// <summary>Wird gefeuert wenn ein Client sich verbindet.</summary>
    public event Action<ClientConnection>? OnClientConnected;

    /// <summary>Wird gefeuert wenn ein Client die Verbindung trennt.</summary>
    public event Action<ClientConnection, string?>? OnClientDisconnected;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    // ═══════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════

    public void Start()
    {
        _listener.Start();
        log.Info("NetworkServer started on port {Port}",
            ((IPEndPoint)_listener.LocalEndpoint).Port);

        // Accept-Loop starten
        _ = AcceptClientsAsync();
    }

    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();

        foreach (var connection in _connections.Values)
        {
            connection.Dispose();
        }

        _connections.Clear();
        log.Info("NetworkServer stopped");
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════
    // ACCEPT LOOP
    // ═══════════════════════════════════════════════════════════════

    private async Task AcceptClientsAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token);

                // Connection erstellen
                var connection = new ClientConnection(tcpClient, log);

                if (_connections.TryAdd(connection.Id, connection))
                {
                    log.Info("Client connected: {ConnectionId} from {Endpoint}",
                        connection.Id, connection.RemoteEndPoint);

                    // Auf Connection-Events hören
                    connection.OnMessageReceived += HandleConnectionMessage;
                    connection.OnDisconnected += HandleConnectionDisconnected;

                    // Event nach außen feuern
                    OnClientConnected?.Invoke(connection);

                    // Connection startet selbst das Empfangen!
                    connection.StartReceivingAsync();
                }
                else
                {
                    connection.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                // Server wurde gestoppt
                break;
            }
            catch (ObjectDisposedException)
            {
                // Server wurde gestoppt
                break;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error accepting client");
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CONNECTION EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Wird von ClientConnection aufgerufen wenn Message empfangen.
    /// </summary>
    private void HandleConnectionMessage(
        ClientConnection connection,
        MessageType type,
        INetworkMessage message)
    {
        // Event nach außen weiterleiten (an GameServer)
        OnMessageReceived?.Invoke(connection, type, message);
    }

    /// <summary>
    ///     Wird von ClientConnection aufgerufen wenn Verbindung getrennt.
    /// </summary>
    private void HandleConnectionDisconnected(ClientConnection connection, string? reason)
    {
        // Events abmelden
        connection.OnMessageReceived -= HandleConnectionMessage;
        connection.OnDisconnected -= HandleConnectionDisconnected;

        // Aus Dictionary entfernen
        _connections.TryRemove(connection.Id, out _);

        log.Info("Client disconnected: {ConnectionId} - {Reason}",
            connection.Id, reason ?? "Unknown");

        // Event nach außen feuern
        OnClientDisconnected?.Invoke(connection, reason);

        // Connection aufräumen
        connection.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONNECTION MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Entfernt und trennt eine Connection.
    /// </summary>
    public void RemoveConnection(Guid connectionId, string? reason = null)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            // Events abmelden
            connection.OnMessageReceived -= HandleConnectionMessage;
            connection.OnDisconnected -= HandleConnectionDisconnected;

            log.Info("Removing connection: {ConnectionId} - {Reason}",
                connectionId, reason ?? "Unknown");

            // Event feuern BEVOR dispose
            OnClientDisconnected?.Invoke(connection, reason);

            // Aufräumen
            connection.Dispose();
        }
    }

    /// <summary>
    ///     Holt eine Connection by ID.
    /// </summary>
    public bool TryGetConnection(Guid connectionId, out ClientConnection? connection)
    {
        return _connections.TryGetValue(connectionId, out connection);
    }

    /// <summary>
    ///     Alle aktiven Connections.
    /// </summary>
    public IEnumerable<ClientConnection> GetAllConnections()
    {
        return _connections.Values;
    }

    /// <summary>
    ///     Anzahl aktiver Connections.
    /// </summary>
    public int ConnectionCount => _connections.Count;

    // ═══════════════════════════════════════════════════════════════
    // SEND (delegiert an Connection)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Sendet eine Message an eine Connection.
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
    ///     Sendet eine Message an eine Connection by ID.
    /// </summary>
    public bool Send(Guid connectionId, INetworkMessage message)
    {
        if (_connections.TryGetValue(connectionId, out var connection))
        {
            Send(connection, message);
            return true;
        }

        return false;
    }
}
