using System.Net.Sockets;
using Mmo.Shared.Connection.Messages;
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
    
    private readonly Queue<INetworkMessage> _receivedMessages = new();
    private readonly object _lock = new();
    
    public bool IsConnected => _tcpClient?.Connected ?? false;
    public Guid PlayerId { get; private set; }
    public ushort ZoneId { get; private set; }
    public string Username { get; private set; } = "";

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
        var loginRequest = new LoginRequest(username, password);
        await SendMessageAsync(loginRequest);
        
        // Wait for LoginResponse
        var response = await WaitForMessageAsync<LoginResponse>(timeout);
        
        if (response?.Success == true)
        {
            PlayerId = response.PlayerId;
            ZoneId = response.ZoneId;
        }
        
        return response;
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    public async Task SendMessageAsync(INetworkMessage message)
    {
        if (_stream == null || !IsConnected)
            throw new InvalidOperationException("Not connected to server");

        byte[] payload = MessageSerializer.Serialize(message);
        
        // Frame format: [1 Byte Type][4 Bytes Length][N Bytes Payload]
        byte[] frame = new byte[5 + payload.Length];
        frame[0] = (byte)message.Type;
        BitConverter.GetBytes((uint)payload.Length).CopyTo(frame, 1);
        payload.CopyTo(frame, 5);

        await _stream.WriteAsync(frame);
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
                    if (_receivedMessages.ElementAt(i) is T message)
                    {
                        // Remove from queue
                        var list = _receivedMessages.ToList();
                        list.RemoveAt(i);
                        _receivedMessages.Clear();
                        foreach (var msg in list)
                            _receivedMessages.Enqueue(msg);
                        
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

        var buffer = new byte[8192];
        
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                // Read frame header: [1 Byte Type][4 Bytes Length]
                int headerBytesRead = 0;
                while (headerBytesRead < 5)
                {
                    int read = await _stream.ReadAsync(buffer.AsMemory(headerBytesRead, 5 - headerBytesRead), cancellationToken);
                    if (read == 0)
                        return; // Connection closed
                    
                    headerBytesRead += read;
                }

                byte messageTypeByte = buffer[0];
                uint payloadLength = BitConverter.ToUInt32(buffer, 1);

                // Read payload
                byte[] payloadBuffer = new byte[payloadLength];
                int payloadBytesRead = 0;
                
                while (payloadBytesRead < payloadLength)
                {
                    int read = await _stream.ReadAsync(
                        payloadBuffer.AsMemory(payloadBytesRead, (int)payloadLength - payloadBytesRead), 
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
                        _receivedMessages.Enqueue(message);
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
        _cts?.Cancel();
        _receiveTask?.Wait(TimeSpan.FromSeconds(2));
        
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _cts?.Dispose();
    }
}
