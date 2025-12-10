using Mmo.Server.Networking. NetworkEvents;
using Mmo.Shared.Enums;

namespace Mmo.Server.Tests. Networking;

public class NetworkEventsTests
{
    [Fact]
    public void ClientConnectedEventArgs_SetsAllProperties()
    {
        var clientId = Guid.NewGuid();
        var endPoint = "192.168.1.1:12345";

        var args = new ClientConnectedEventArgs(clientId, endPoint);

        Assert.Equal(clientId, args.ClientId);
        Assert.Equal(endPoint, args.RemoteEndPoint);
        Assert.True(args.ConnectedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void ClientDisconnectedEventArgs_SetsAllProperties()
    {
        var clientId = Guid.NewGuid();
        var reason = DisconnectReason.ClientDisconnected;

        var args = new ClientDisconnectedEventArgs(clientId, reason);

        Assert.Equal(clientId, args.ClientId);
        Assert.Equal(reason, args.Reason);
        Assert.True(args.DisconnectedAt <= DateTimeOffset. UtcNow);
    }

    [Theory]
    [InlineData(DisconnectReason.ClientDisconnected)]
    [InlineData(DisconnectReason. Timeout)]
    [InlineData(DisconnectReason.NetworkError)]
    [InlineData(DisconnectReason.ServerShutdown)]
    [InlineData(DisconnectReason. Kicked)]
    [InlineData(DisconnectReason. ProtocolError)]
    public void ClientDisconnectedEventArgs_AllReasonsWork(DisconnectReason reason)
    {
        var args = new ClientDisconnectedEventArgs(Guid.NewGuid(), reason);

        Assert.Equal(reason, args.Reason);
    }

    [Fact]
    public void NetworkErrorEventArgs_WithClientId_SetsCorrectly()
    {
        var clientId = Guid.NewGuid();
        var exception = new Exception("Test error");
        var context = "ReadLoop";

        var args = new NetworkErrorEventArgs(clientId, exception, context);

        Assert.Equal(clientId, args.ClientId);
        Assert.Equal(exception, args.Exception);
        Assert.Equal(context, args.Context);
    }

    [Fact]
    public void NetworkErrorEventArgs_WithoutClientId_ClientIdIsNull()
    {
        var exception = new Exception("Server error");
        var context = "AcceptLoop";

        var args = new NetworkErrorEventArgs(null, exception, context);

        Assert.Null(args.ClientId);
        Assert.Equal(exception, args.Exception);
    }
}
