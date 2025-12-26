using System.Net.Sockets;
using Mmo.Shared.Connection.Messages;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Messaging.Serialization;

namespace Mmo.Integration.Tests.Utilities;

/// <summary>
/// Simple TCP client for integration testing.
/// This client connects to the MMO server and can send/receive messages.
/// </summary>
public class TestClient : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;

    private readonly List<INetworkMessage> _receivedMessages = new();
    private readonly object _lock = new();

    public bool IsConnected => _tcpClient?.Connected ?? false;
    public Guid AccountId { get; private set; }
    public string Username { get; private set; }
    public string AccountName { get; private set; } = "";
    public bool IsPremium { get; private set; } = false;
    public long Timestamp { get; private set; } = 0;

    public TestClient(string host, int port)
    {
        _host = host;
        _port = port;
    }

    /// <summary>
    /// Connects to the server.
    /// </summary>
    public async Task<bool> ConnectAsync(TimeSpan timeout)
    {
        try
        {
            _tcpClient = new TcpClient();
            using var connectCts = new CancellationTokenSource(timeout);
            await _tcpClient.ConnectAsync(_host, _port, connectCts.Token);

            _stream = _tcpClient.GetStream();
            _cts = new CancellationTokenSource();

            // Start receiving messages in background
            _receiveTask = Task.Run(() => ReceiveLoop(_cts.Token));

            return true;
        }
        catch
        {
            Dispose();
            return false;
        }
    }

    /// <summary>
    /// Sends a login request and waits for response.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(string username, string password, TimeSpan timeout)
    {
        Username = username;
        var loginRequest = new LoginRequest(){Username = username, Password = password};
        await SendMessageAsync(loginRequest);

        // Wait for LoginResponse
        var response = await WaitForMessageAsync<LoginResponse>(timeout);

        if (response?.Success == true)
        {
            AccountId = (Guid)response.AccountId!;
            AccountName = response.AccountName!;
            IsPremium = (bool)response.IsPremium!;
            Timestamp = response.Timestamp;
        }

        return response;
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    public async Task SendMessageAsync<T>(T message) where T : INetworkMessage
    {
        if (_stream == null || !IsConnected)
            throw new InvalidOperationException("Not connected to server");

        byte[] payload = MessageSerializer.Serialize(message);

        // Frame format: [4 Bytes Length][N Bytes Payload]
        // Note: MessageType is already in the payload at Key(0)
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

        await _stream.WriteAsync(lengthPrefix);
        await _stream.WriteAsync(payload);
        await _stream.FlushAsync();
    }

    /// <summary>
    /// Waits for a specific message type.
    /// </summary>
    public async Task<T?> WaitForMessageAsync<T>(TimeSpan timeout) where T : class, INetworkMessage
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            lock (_lock)
            {
                for (int i = 0; i < _receivedMessages.Count; i++)
                {
                    if (_receivedMessages[i] is T message)
                    {
                        _receivedMessages.RemoveAt(i);
                        return message;
                    }
                }
            }

            await Task.Delay(50);
        }

        return null;
    }

    /// <summary>
    /// Gets all messages of a specific type that have been received.
    /// </summary>
    public List<T> GetMessages<T>() where T : class, INetworkMessage
    {
        lock (_lock)
        {
            return _receivedMessages.OfType<T>().ToList();
        }
    }

    /// <summary>
    /// Clears all received messages.
    /// </summary>
    public void ClearMessages()
    {
        lock (_lock)
        {
            _receivedMessages.Clear();
        }
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        if (_stream == null) return;

        var headerBuffer = new byte[4];

        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                // Read frame header: [4 Bytes Length]
                int headerBytesRead = 0;
                while (headerBytesRead < 4)
                {
                    int read = await _stream.ReadAsync(headerBuffer.AsMemory(headerBytesRead, 4 - headerBytesRead), cancellationToken);
                    if (read == 0)
                        return; // Connection closed

                    headerBytesRead += read;
                }

                int payloadLength = BitConverter.ToInt32(headerBuffer, 0);

                if (payloadLength <= 0 || payloadLength > 1024 * 1024)
                {
                    Console.WriteLine($"Invalid message length: {payloadLength}");
                    return;
                }

                // Read payload
                byte[] payloadBuffer = new byte[payloadLength];
                int payloadBytesRead = 0;

                while (payloadBytesRead < payloadLength)
                {
                    int read = await _stream.ReadAsync(
                        payloadBuffer.AsMemory(payloadBytesRead, payloadLength - payloadBytesRead),
                        cancellationToken);

                    if (read == 0)
                        return; // Connection closed

                    payloadBytesRead += read;
                }

                // Deserialize message
                try
                {
                    var message = MessageSerializer.Deserialize(payloadBuffer);

                    lock (_lock)
                    {
                        _receivedMessages.Add(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing message: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in receive loop: {ex.Message}");
        }
    }

    public void Dispose()
    {
        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // CTS already disposed, ignore
        }

        _receiveTask?.Wait(TimeSpan.FromSeconds(2));

        _stream?.Dispose();
        _tcpClient?.Dispose();
        _cts?.Dispose();
    }
}
