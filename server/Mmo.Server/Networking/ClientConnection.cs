using System.Net.Sockets;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Serialization;

namespace Mmo.Server.Networking;

/// <summary>
///     Represents a single client connection, handling sending and receiving of messages.
/// </summary>
public class ClientConnection(TcpClient tcpClient, ILog log) : IDisposable
{
    private readonly NetworkStream _stream = tcpClient.GetStream();
    private bool _isDisconnecting;
    private bool _isDisposed;

    /// <summary>
    ///     Unique identifier for this connection.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    ///     Returns true if the underlying TCP connection is still active.
    /// </summary>
    public bool IsConnected => !_isDisposed && tcpClient.Connected;

    /// <summary>
    ///     The remote endpoint (IP: Port) of this connection.
    /// </summary>
    public string RemoteEndPoint => tcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown";

    public Guid PlayerId { get; set; }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        try
        {
            _stream.Dispose();
            tcpClient.Dispose();
        }
        catch
        {
            // Ignore dispose errors
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Fired when a message is received from this client.
    /// </summary>
    public event Action<INetworkMessage>? MessageReceived;

    /// <summary>
    ///     Fired when this client disconnects (includes reason).
    /// </summary>
    public event Action<DisconnectReason>? Disconnected;

    /// <summary>
    ///     Sends a message to this client.
    /// </summary>
    public async Task SendAsync(INetworkMessage message) // ← Nicht mehr generisch!
    {
        if (_isDisposed || _isDisconnecting)
        {
            log.Warn("Cannot send to disposed/disconnecting client {ClientId}", Id);
            return;
        }

        try
        {
            // Serialize - verwende dynamic dispatch um den konkreten Typ zu bekommen
            byte[] payload = SerializeMessage(message);

            // Frame:  [4 Bytes Length][Payload]
            byte[] lengthBytes = BitConverter.GetBytes((uint)payload.Length);

            await _stream.WriteAsync(lengthBytes);
            await _stream.WriteAsync(payload);
            await _stream.FlushAsync();
        }
        catch (ObjectDisposedException)
        {
            log.Debug("Stream already disposed for client {ClientId}", Id);
        }
        catch (IOException ex)
        {
            log.Error("Send failed for client {ClientId}: {Error}", Id, ex.Message);
            await DisconnectAsync(DisconnectReason.NetworkError);
        }
        catch (Exception ex)
        {
            log.Error("Unexpected send error for client {ClientId}:  {Error}", Id, ex.Message);
            await DisconnectAsync(DisconnectReason.NetworkError);
        }
    }

    /// <summary>
    ///     Serializes a message using its runtime type, not the interface type.
    /// </summary>
    private static byte[] SerializeMessage(INetworkMessage message)
    {
        // Verwende dynamic um den KONKRETEN Typ zu serialisieren
        return MessageSerializer.Serialize((dynamic)message);
    }

    /// <summary>
    ///     Starts the receiving loop for this connection.
    ///     This method runs until the connection is closed or canceled.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop receiving.</param>
    public async Task StartReceivingAsync(CancellationToken cancellationToken)
    {
        DisconnectReason disconnectReason = DisconnectReason.ClientDisconnected;

        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                // 1. Read length (4 bytes)
                byte[] lengthBuffer = new byte[4];
                await ReadExactlyAsync(lengthBuffer, 4, cancellationToken);
                uint length = BitConverter.ToUInt32(lengthBuffer);

                // Sanity check:  prevent excessive memory allocation
                if (length > 10 * 1024 * 1024) // 10 MB max
                {
                    log.Warn("Client {ClientId} sent oversized message: {Length} bytes", Id, length);
                    disconnectReason = DisconnectReason.ProtocolError;
                    break;
                }

                // 2. Read payload (dynamic size)
                byte[] payload = new byte[length];
                await ReadExactlyAsync(payload, (int)length, cancellationToken);

                // 3. Deserialize with MessageSerializer
                INetworkMessage message = MessageSerializer.Deserialize(payload);

                // 4. Raise event
                MessageReceived?.Invoke(message);
            }

            // If canceled, it's a server shutdown
            if (cancellationToken.IsCancellationRequested) disconnectReason = DisconnectReason.ServerShutdown;
        }
        catch (OperationCanceledException)
        {
            disconnectReason = DisconnectReason.ServerShutdown;
        }
        catch (IOException ex)
        {
            // Connection closed by remote host or network error
            log.Warn("Client {ClientId} connection closed: {Error}", Id, ex.Message);
            disconnectReason = DisconnectReason.ClientDisconnected;
        }
        catch (Exception ex)
        {
            log.Warn("Client {ClientId} read loop error: {Error}", Id, ex.Message);
            disconnectReason = DisconnectReason.NetworkError;
        }
        finally
        {
            await DisconnectAsync(disconnectReason);
        }
    }

    /// <summary>
    ///     Reads exactly the specified number of bytes from the stream.
    ///     TCP doesn't guarantee all bytes arrive at once, so we loop.
    /// </summary>
    private async Task ReadExactlyAsync(byte[] buffer, int count, CancellationToken ct)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int bytesRead = await _stream.ReadAsync(
                buffer.AsMemory(totalRead, count - totalRead), ct);

            if (bytesRead == 0) throw new IOException("Connection closed by remote host");

            totalRead += bytesRead;
        }
    }

    /// <summary>
    ///     Disconnects this client with the specified reason.
    /// </summary>
    /// <param name="reason">The reason for disconnection.</param>
    public Task DisconnectAsync(DisconnectReason reason = DisconnectReason.ClientDisconnected)
    {
        // Prevent multiple disconnect calls
        if (_isDisconnecting) return Task.CompletedTask;

        _isDisconnecting = true;

        log.Debug("Disconnecting client {ClientId}:  {Reason}", Id, reason);

        try
        {
            _stream.Close();
            tcpClient.Close();
        }
        catch
        {
            // Ignore cleanup errors
        }

        // Raise event with reason
        Disconnected?.Invoke(reason);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Kicks this client with the Kicked reason.
    /// </summary>
    public Task KickAsync() => DisconnectAsync(DisconnectReason.Kicked);
}
