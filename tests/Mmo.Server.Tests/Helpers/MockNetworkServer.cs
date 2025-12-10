using Mmo.Server.Networking;
using Mmo.Server.Networking.NetworkEvents;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Tests.Helpers;

/// <summary>
///     A mock implementation of INetworkServer for testing.
///     Tracks sent messages and allows simulation of network events.
/// </summary>
public class MockNetworkServer : INetworkServer
{
    private readonly List<Guid> _connectedClients = new();
    private bool _isDisposed;

    public List<(Guid ClientId, INetworkMessage Message)> SentMessages { get; } = new();
    public List<INetworkMessage> BroadcastMessages { get; } = new();
    public List<(INetworkMessage Message, Guid ExcludedClient)> BroadcastExceptMessages { get; } = new();
    public List<Guid> KickedClients { get; } = new();

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

    public Task SendToClientAsync(Guid clientId, INetworkMessage message)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        SentMessages.Add((clientId, message));
        return Task.CompletedTask;
    }

    public Task BroadcastAsync(INetworkMessage message)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        BroadcastMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task BroadcastExceptAsync(INetworkMessage message, Guid excludeClientId)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        BroadcastExceptMessages.Add((message, excludeClientId));
        return Task.CompletedTask;
    }

    public Task KickClientAsync(Guid clientId)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MockNetworkServer));

        KickedClients.Add(clientId);
        _connectedClients.Remove(clientId);
        return Task.CompletedTask;
    }

    public IEnumerable<Guid> GetConnectedClientIds() => _connectedClients.ToList();

    public bool IsClientConnected(Guid clientId) => _connectedClients.Contains(clientId);

    public void Dispose()
    {
        _isDisposed = true;
        _connectedClients.Clear();
    }

    // ══════════════════════════════════════════════════════════
    // TEST HELPER METHODS
    // ══════════════════════════════════════════════════════════

    /// <summary>
    ///     Simulates a client connecting to the server.
    /// </summary>
    public void SimulateClientConnected(Guid clientId, string remoteEndPoint = "127.0.0.1:12345")
    {
        if (!_connectedClients.Contains(clientId)) _connectedClients.Add(clientId);
        ClientConnected?.Invoke(this, new ClientConnectedEventArgs(clientId, remoteEndPoint));
    }

    /// <summary>
    ///     Simulates a client disconnecting from the server.
    /// </summary>
    public void SimulateClientDisconnected(Guid clientId, DisconnectReason reason)
    {
        _connectedClients.Remove(clientId);
        ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(clientId, reason));
    }

    /// <summary>
    ///     Simulates receiving a message from a client.
    /// </summary>
    public void SimulateMessageReceived(Guid clientId, INetworkMessage message) =>
        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(clientId, message));

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
}
