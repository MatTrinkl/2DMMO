using System.Reflection;
using System.Runtime.CompilerServices;
using Mmo.Server.Networking;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Tests.Helpers;

/// <summary>
///     A mock implementation of NetworkServer for testing.
///     Tracks sent messages and allows simulation of network events.
///     Uses the new event-based pattern from the refactored architecture.
/// </summary>
public class MockNetworkServer(ILog log, bool isDisposed, int port = 7777) :NetworkServer(log, port)
{
    private readonly Dictionary<Guid, ClientConnection> _connections = new();
    private bool _isDisposed = isDisposed;

    public List<(ClientConnection Client, INetworkMessage Message)> SentMessages { get; } = new();

    public int ConnectionCount => _connections.Count;

    /// <summary>Event that fires when a message is received from a client.</summary>
    public event Action<ClientConnection, MessageType, INetworkMessage>? OnMessageReceived;

    /// <summary>Event that fires when a client connects.</summary>
    public event Action<ClientConnection>? OnClientConnected;

    /// <summary>Event that fires when a client disconnects.</summary>
    public event Action<ClientConnection, string?>? OnClientDisconnected;

    // ══════════════════════════════════════════════════════════
    // METHODS CALLED BY GAMESERVER
    // ══════════════════════════════════════════════════════════

    public void Send(ClientConnection connection, INetworkMessage message)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        SentMessages.Add((connection, message));
    }

    public bool TryGetConnection(Guid connectionId, out ClientConnection? connection)
    {
        return _connections.TryGetValue(connectionId, out connection);
    }

    public void RemoveConnection(Guid connectionId, string? reason)
    {
        if (_connections.Remove(connectionId, out var connection))
        {
            // Trigger disconnect event
            OnClientDisconnected?.Invoke(connection, reason);
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
        _connections.Clear();
    }

    // ══════════════════════════════════════════════════════════
    // TEST HELPER METHODS
    // ══════════════════════════════════════════════════════════

    /// <summary>
    ///     Simulates a client connecting to the server.
    /// </summary>
    public void SimulateClientConnected(Guid clientId, string remoteEndPoint = "127.0.0.1:12345")
    {
        var connection = GetOrCreateMockConnection(clientId);
        OnClientConnected?.Invoke(connection);
    }

    /// <summary>
    ///     Simulates a client disconnecting from the server.
    /// </summary>
    public void SimulateClientDisconnected(Guid clientId, string? reason = null)
    {
        if (_connections.TryGetValue(clientId, out var connection))
        {
            OnClientDisconnected?.Invoke(connection, reason);
            _connections.Remove(clientId);
        }
    }

    /// <summary>
    ///     Simulates receiving a message from a client.
    /// </summary>
    public void SimulateMessageReceived(Guid clientId, INetworkMessage message)
    {
        var connection = GetOrCreateMockConnection(clientId);
        var messageType = message.Type;
        OnMessageReceived?.Invoke(connection, messageType, message);
    }

    /// <summary>
    ///     Clears all tracked messages and events.
    /// </summary>
    public void Clear()
    {
        SentMessages.Clear();
    }

    /// <summary>
    ///     Gets or creates a mock ClientConnection for the given Guid.
    /// </summary>
    public ClientConnection GetOrCreateMockConnection(Guid clientId)
    {
        if (!_connections.TryGetValue(clientId, out var connection))
        {
            connection = CreateMockClientConnection(clientId);
            _connections[clientId] = connection;
        }

        return connection;
    }

    private static ClientConnection CreateMockClientConnection(Guid clientId)
    {
        // Use FormatterServices to create an instance without calling the constructor
        // This avoids the need for a real TcpClient
        var connection = (ClientConnection)RuntimeHelpers
            .GetUninitializedObject(typeof(ClientConnection));

        // Set the Id using reflection
        var idField = typeof(ClientConnection).GetField("<Id>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        idField?.SetValue(connection, clientId);

        // Set RemoteEndPoint using reflection
        var endpointField = typeof(ClientConnection).GetField("<RemoteEndPoint>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        endpointField?.SetValue(connection, "127.0.0.1:12345");

        // Set ConnectedAt using reflection
        var connectedAtField = typeof(ClientConnection).GetField("<ConnectedAt>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        connectedAtField?.SetValue(connection, DateTimeOffset.UtcNow);

        return connection;
    }
}
