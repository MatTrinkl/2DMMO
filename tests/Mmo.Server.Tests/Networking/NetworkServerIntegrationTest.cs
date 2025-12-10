using System.Net.Sockets;
using MessagePack;
using Mmo.Server.Networking;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.ZoneEvents;
using Mmo.Shared.Records;
using Mmo.Shared.Serialization;
using Xunit.Abstractions;

namespace Mmo.Server.Tests.Networking;

/// <summary>
///     Integration tests that use real TCP connections.
/// </summary>
public class NetworkServerIntegrationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private const int _testPort = 17777;
    private readonly MockLog _log = new();
    private NetworkServer _server = null!;
    private CancellationTokenSource _serverCts = null!;
    private Task _serverTask = null!;

    public NetworkServerIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public async Task InitializeAsync()
    {
        _server = new NetworkServer(_testPort, _log);
        _serverCts = new CancellationTokenSource();
        _serverTask = _server.RunAsync(_serverCts.Token);

        // Wait for server to start listening
        await Task.Delay(200);
    }

    public async Task DisposeAsync()
    {
        _serverCts.Cancel();

        try
        {
            await _serverTask.WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        _server.Dispose();
        _serverCts.Dispose();
    }

    [Fact]
    public async Task Server_AcceptsClientConnection()
    {
        // Arrange
        var clientConnectedTcs = new TaskCompletionSource<Guid>();
        _server.ClientConnected += (_, args) => clientConnectedTcs.TrySetResult(args.ClientId);

        // Act
        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync("127.0.0.1", _testPort);

        // Assert
        var clientId = await clientConnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.NotEqual(Guid.Empty, clientId);
        Assert.Equal(1, _server.ClientCount);
    }

    [Fact]
    public async Task Server_DetectsClientDisconnection()
    {
        // Arrange
        var clientConnectedTcs = new TaskCompletionSource<Guid>();
        var clientDisconnectedTcs = new TaskCompletionSource<Guid>();

        _server.ClientConnected += (_, args) => clientConnectedTcs.TrySetResult(args.ClientId);
        _server.ClientDisconnected += (_, args) => clientDisconnectedTcs.TrySetResult(args.ClientId);

        // Act
        var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync("127.0.0.1", _testPort);

        var connectedClientId = await clientConnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(2));

        tcpClient.Close();

        // Assert
        var disconnectedClientId = await clientDisconnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(connectedClientId, disconnectedClientId);
    }

    [Fact]
    public async Task Server_BroadcastsToAllClients()
    {
        // Arrange:  Connect 3 clients and wait for each connection to be confirmed
        var clients = new List<TcpClient>();
        var connectedCount = 0;
        var allConnectedTcs = new TaskCompletionSource();

        _server.ClientConnected += (_, _) =>
        {
            if (Interlocked.Increment(ref connectedCount) == 3)
            {
                allConnectedTcs.TrySetResult();
            }
        };

        for (int i = 0; i < 3; i++)
        {
            var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", _testPort);
            clients.Add(client);
        }

        // Wait until all 3 clients are connected on server side
        await allConnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(3, _server.ClientCount);

        // Act: Broadcast message
        var broadcast = new PlayerJoinedZone(
            new PlayerEntity(Guid.NewGuid(), "Broadcaster", new Position(0, 0))
        );
        await _server.BroadcastAsync(broadcast);

        // Small delay to ensure message is sent
        await Task.Delay(100);

        // Assert: All clients receive the message
        foreach (var client in clients)
        {
            var message = await ReadMessageWithTimeoutAsync(client, TimeSpan.FromSeconds(2));
            Assert.NotNull(message);
            Assert.IsType<PlayerJoinedZone>(message);
        }

        // Cleanup
        foreach (var client in clients)
        {
            client.Close();
        }
    }

    [Fact]
    public async Task Server_SendsMessageToSpecificClient()
    {
        // Arrange
        var clientConnectedTcs = new TaskCompletionSource<Guid>();
        _server.ClientConnected += (_, args) => clientConnectedTcs.TrySetResult(args.ClientId);

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync("127.0.0.1", _testPort);

        var clientId = await clientConnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(2));

        // Act: Server sends message to client
        var loginResponse = new LoginResponse
        {
            ZoneId = 0,
            Success = true,
            PlayerId = Guid.NewGuid()
        };

        await _server.SendToClientAsync(clientId, loginResponse);

        // Assert: Client receives message
        INetworkMessage? receivedMessage = await ReadMessageWithTimeoutAsync(tcpClient, TimeSpan.FromSeconds(2));

        Assert.NotNull(receivedMessage);
        Assert.IsType<LoginResponse>(receivedMessage);
        Assert.True(((LoginResponse)receivedMessage).Success);
    }

    [Fact]
    public async Task Server_ReceivesMessageFromClient()
    {
        // Arrange
        var messageReceivedTcs = new TaskCompletionSource<INetworkMessage>();
        _server.MessageReceived += (_, args) => messageReceivedTcs.TrySetResult(args.Message);

        var clientConnectedTcs = new TaskCompletionSource<Guid>();
        _server.ClientConnected += (_, args) => clientConnectedTcs.TrySetResult(args.ClientId);

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync("127.0.0.1", _testPort);

        // Wait for connection to be fully established
        await clientConnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Task.Delay(50);

        // Act: Send a message from client to server
        var loginRequest = new LoginRequest("TestUser", "TestPassword");


        await SendMessageAsync(tcpClient, loginRequest);

        // Assert
        var receivedMessage = await messageReceivedTcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.IsType<LoginRequest>(receivedMessage);
        Assert.Equal("TestUser", ((LoginRequest)receivedMessage).Username);
    }

    [Fact]
    public async Task Server_BroadcastExcept_ExcludesSpecifiedClient()
    {
        // Arrange:  Connect 2 clients
        var client1ConnectedTcs = new TaskCompletionSource<Guid>();
        var client2ConnectedTcs = new TaskCompletionSource<Guid>();
        var connectionCount = 0;

        _server.ClientConnected += (_, args) =>
        {
            var count = Interlocked.Increment(ref connectionCount);
            if (count == 1)
                client1ConnectedTcs.TrySetResult(args.ClientId);
            else if (count == 2)
                client2ConnectedTcs.TrySetResult(args.ClientId);
        };

        using var client1 = new TcpClient();
        await client1.ConnectAsync("127.0.0.1", _testPort);
        var client1Id = await client1ConnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(2));

        using var client2 = new TcpClient();
        await client2.ConnectAsync("127.0.0.1", _testPort);
        var client2Id = await client2ConnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(2, _server.ClientCount);

        // Act: Broadcast except client1 (so only client2 should receive)
        var broadcast = new PlayerJoinedZone(
            new PlayerEntity(Guid.NewGuid(), "Test", new Position(0, 0))
        );
        await _server.BroadcastExceptAsync(broadcast, client1Id);

        // Give time for message to be sent
        await Task.Delay(200);

        // Assert: Client2 SHOULD receive message
        var message2 = await ReadMessageWithTimeoutAsync(client2, TimeSpan.FromSeconds(2));
        Assert.NotNull(message2);
        Assert.IsType<PlayerJoinedZone>(message2);

        // Assert: Client1 should NOT receive message (expect null/timeout)
        var message1 = await ReadMessageWithTimeoutAsync(client1, TimeSpan.FromMilliseconds(300));
        Assert.Null(message1); // This is expected - client1 was excluded
    }


    // ══════════════════════════════════════════════════════════
    // HELPER METHODS
    // ══════════════════════════════════════════════════════════

    /// <summary>
    ///     Reads a length-prefixed message from a TcpClient with timeout.
    ///     Returns null if timeout or connection closed.
    /// </summary>
    /// <summary>
    ///     Sends a length-prefixed message to the server.
    /// </summary>
    private static async Task SendMessageAsync<T>(TcpClient client, T message)
        where T : INetworkMessage
    {
        var stream = client.GetStream();

        // Verwende MessageSerializer statt MessagePackSerializer!
        byte[] messageBytes = MessageSerializer.Serialize(message);
        byte[] lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

        await stream.WriteAsync(lengthPrefix);
        await stream.WriteAsync(messageBytes);
        await stream.FlushAsync();
    }

    private static async Task<INetworkMessage? > ReadMessageWithTimeoutAsync(
        TcpClient client,
        TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);
            var stream = client.GetStream();

            // Wait for data
            var waitStart = DateTime.UtcNow;
            while (! stream.DataAvailable && DateTime.UtcNow - waitStart < timeout)
            {
                await Task.Delay(10, cts.Token);
            }

            if (!stream.DataAvailable)
                return null;

            // Read length prefix (4 bytes) - als UINT wie der Server es sendet!
            byte[] lengthBuffer = new byte[4];
            int bytesRead = 0;
            while (bytesRead < 4)
            {
                int read = await stream.ReadAsync(
                    lengthBuffer. AsMemory(bytesRead, 4 - bytesRead),
                    cts.Token);

                if (read == 0)
                    return null;

                bytesRead += read;
            }

            // ← Verwende ToUInt32 wie der Server!
            uint messageLength = BitConverter.ToUInt32(lengthBuffer);

            if (messageLength == 0 || messageLength > 1024 * 1024)
                return null;

            // Read message
            byte[] messageBuffer = new byte[messageLength];
            bytesRead = 0;
            while (bytesRead < (int)messageLength)
            {
                int read = await stream. ReadAsync(
                    messageBuffer. AsMemory(bytesRead, (int)messageLength - bytesRead),
                    cts.Token);

                if (read == 0)
                    return null;

                bytesRead += read;
            }

            return MessageSerializer.Deserialize(messageBuffer);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
