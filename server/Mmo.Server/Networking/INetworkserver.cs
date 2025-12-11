// server/Mmo.Server/Networking/INetworkServer.cs

using Mmo.Server.Networking.NetworkEvents;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Networking;

public interface INetworkServer : IDisposable
{
    int ClientCount { get; }
    bool IsListening { get; }

    event EventHandler<ClientConnectedEventArgs>? ClientConnected;
    event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

    Task RunAsync(CancellationToken cancellationToken);
    Task SendToClientAsync(Guid clientId, INetworkMessage message);
    Task BroadcastAsync(INetworkMessage message);
    Task BroadcastExceptAsync(INetworkMessage message, Guid excludeClientId);
    Task KickClientAsync(Guid clientId);
    IEnumerable<Guid> GetConnectedClientIds();
    bool IsClientConnected(Guid clientId);
}
