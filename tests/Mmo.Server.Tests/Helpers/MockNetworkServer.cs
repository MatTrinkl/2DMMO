using Mmo.Server.Networking;
using Mmo.Server.Networking.NetworkEvents;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;
using Moq;

namespace Mmo.Server.Tests.Helpers;

/// <summary>
///     A mock implementation of INetworkServer for testing.
///     Tracks sent messages and allows simulation of network events.
/// </summary>
public class MockNetworkServer : INetworkServer
{
    private readonly List<ClientConnection> _connectedClients = new();
    private readonly Dictionary<Guid, ClientConnection> _mockConnections = new();
    private bool _isDisposed;

    public List<(ClientConnection Client, INetworkMessage Message)> SentMessages { get; } = new();
    public List<INetworkMessage> BroadcastMessages { get; } = new();
    public List<(INetworkMessage Message, ClientConnection ExcludedClient)> BroadcastExceptMessages { get; } = new();
    public List<ClientConnection> KickedClients { get; } = new();

    public int ClientCount => _connectedClients.Count;
    public bool IsListening { get; set; } = true;

    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

    public Task RunAsync(CancellationToken cancellationToken)
    {
        // Mock implementation - does nothing but allows cancellation
        return Task.Delay(-1, cancellationToken).ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnCanceled);
    }

    public Task SendToClientAsync(ClientConnection client, INetworkMessage message)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        SentMessages.Add((client, message));
        return Task.CompletedTask;
    }

    public Task BroadcastAsync(INetworkMessage message)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        BroadcastMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task BroadcastExceptAsync(INetworkMessage message, ClientConnection excludeClient)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        BroadcastExceptMessages.Add((message, excludeClient));
        return Task.CompletedTask;
    }

    public Task KickClientAsync(ClientConnection client)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        KickedClients.Add(client);
        _connectedClients.Remove(client);
        return Task.CompletedTask;
    }

    public IEnumerable<Guid> GetConnectedClientIds() => _connectedClients.Select(c => c.Id).ToList();

    public bool IsClientConnected(ClientConnection client) => _connectedClients.Contains(client);

    public void AssociatePlayer(ClientConnection connection, Guid playerId)
    {
        // Mock implementation - track association if needed for tests
        connection.PlayerId = playerId;
    }

    public void RemovePlayer(ClientConnection connection)
    {
        // Mock implementation - track removal if needed for tests
        connection.PlayerId = Guid.Empty;
    }

    public void Dispose()
    {
        _isDisposed = true;
        _connectedClients.Clear();
    }

    // ══════════════════════════════════════════════════════════
    // TEST HELPER METHODS
    // ══════════════════════════════════════════════════════════

    /// <summary>
    ///     Creates a mock ClientConnection for testing.
    /// </summary>
    public ClientConnection CreateMockClient(Guid? clientId = null)
    {
        // Create a mock ClientConnection - we'll need to use reflection or a test-friendly approach
        // For now, let's return the clientId which the tests can use
        // This is a limitation - we can't easily create ClientConnection without a real TcpClient
        throw new NotImplementedException("Use SimulateClientConnected with ClientConnection instead");
    }

    /// <summary>
    ///     Simulates a client connecting to the server.
    /// </summary>
    public void SimulateClientConnected(Guid clientId, string remoteEndPoint = "127.0.0.1:12345")
    {
        // For backwards compatibility, we track by GUID but the actual implementation uses ClientConnection
        // This is a test helper limitation
        ClientConnected?.Invoke(this, new ClientConnectedEventArgs(clientId, remoteEndPoint));
    }

    /// <summary>
    ///     Simulates a client disconnecting from the server.
    /// </summary>
    public void SimulateClientDisconnected(Guid clientId, DisconnectReason reason)
    {
        ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(clientId, reason));
    }

    /// <summary>
    ///     Simulates receiving a message from a client.
    /// </summary>
    public void SimulateMessageReceived(Guid clientId, INetworkMessage message)
    {
        var connection = GetOrCreateMockConnection(clientId);
        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(connection, message, DateTime.UtcNow));
    }

    /// <summary>
    ///     Simulates a network error occurring.
    /// </summary>
    public void SimulateNetworkError(Guid? clientId, Exception exception, string context = "Test error") =>
        ErrorOccurred?.Invoke(this, new NetworkErrorEventArgs(clientId, exception, context));

    /// <summary>
    ///     Clears all tracked messages and events.
    /// </summary>
    public void Clear()
    {
        SentMessages.Clear();
        BroadcastMessages.Clear();
        BroadcastExceptMessages.Clear();
        KickedClients.Clear();
    }

    /// <summary>
    ///     Gets or creates a mock ClientConnection for the given Guid.
    /// </summary>
    public ClientConnection GetOrCreateMockConnection(Guid clientId)
    {
        if (!_mockConnections.TryGetValue(clientId, out var connection))
        {
            // Create a mock using System.Net.Sockets.TcpClient mock
            // Since we can't easily mock this, we'll use reflection to create a ClientConnection
            // For testing purposes, we'll create a null-safe version
            connection = CreateMockClientConnection(clientId);
            _mockConnections[clientId] = connection;
        }
        return connection;
    }

    private static ClientConnection CreateMockClientConnection(Guid clientId)
    {
        // Use FormatterServices to create an instance without calling the constructor
        // This avoids the need for a real TcpClient
        var connection = (ClientConnection)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(ClientConnection));
        
        // Set the Id using reflection
        var idField = typeof(ClientConnection).GetField("<Id>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        idField?.SetValue(connection, clientId);
        
        return connection;
    }
}
