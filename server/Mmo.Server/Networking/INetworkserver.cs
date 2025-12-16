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
    Task SendToClientAsync(ClientConnection client, INetworkMessage message);
    Task BroadcastAsync(INetworkMessage message);
    Task BroadcastExceptAsync(INetworkMessage message, ClientConnection excludeClientId);
    Task KickClientAsync(ClientConnection clientId);
    IEnumerable<Guid> GetConnectedClientIds();
    bool IsClientConnected(ClientConnection clientId);
    public void AssociatePlayer(ClientConnection connectionId, Guid playerId);
    public void RemovePlayer(ClientConnection playerId);
}
