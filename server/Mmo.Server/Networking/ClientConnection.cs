using System.Buffers;
using System.Net.Sockets;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Serialization;

namespace Mmo.Server.Networking;

/// <summary>
///     Repräsentiert eine Verbindung zu einem Client.
///     Verantwortlichkeiten:
///     - TCP-Stream verwalten
///     - Messages empfangen und Events feuern
///     - Messages senden
///     - Connection-State tracken
///     KEINE Game-Logik hier! Alles geht über Events zum GameServer.
/// </summary>
public sealed class ClientConnection : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ILog _log;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly NetworkStream _stream;
    private readonly TcpClient _tcpClient;

    private bool _disposed;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public ClientConnection(
        TcpClient tcpClient,
        ILog log)
    {
        _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        _log = log ?? throw new ArgumentNullException(nameof(log));

        _stream = tcpClient.GetStream();
        RemoteEndPoint = tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";

        _tcpClient.NoDelay = true;
        _tcpClient.ReceiveTimeout = 30000;
        _tcpClient.SendTimeout = 10000;
    }

    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    public Guid Id { get; } = Guid.NewGuid();
    public string RemoteEndPoint { get; }
    public DateTimeOffset ConnectedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastActivity { get; private set; } = DateTimeOffset.UtcNow;
    public bool IsConnected => !_disposed && _tcpClient.Connected;

    // ═══════════════════════════════════════════════════════════════
    // AUTH STATE (wird vom Handler gesetzt)
    // ═══════════════════════════════════════════════════════════════

    public ConnectionState State { get; private set; } = ConnectionState.Connected;
    public Guid? AccountId { get; private set; }
    public AccountFlags AccountFlags { get; private set; } = AccountFlags.None;
    public string? Username { get; private set; }
    public Guid? SessionToken { get; private set; }

    public bool IsAuthenticated => State >= ConnectionState.Authenticated;
    public bool IsInGame => State == ConnectionState.InGame;

    // ═══════════════════════════════════════════════════════════════
    // METRICS
    // ═══════════════════════════════════════════════════════════════

    public int LatencyMs { get; set; }
    public int SmoothedLatencyMs { get; set; }
    public long MessagesReceived { get; private set; }
    public long MessagesSent { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // DISPOSE
    // ═══════════════════════════════════════════════════════════════

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _cts.Cancel();
            _stream.Dispose();
            _tcpClient.Dispose();
            _sendLock.Dispose();
            _cts.Dispose();
        }
        catch
        {
            // ignore errors in cleanup for now
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // EVENTS
    // ═══════════════════════════════════════════════════════════════

    public event Action<ClientConnection, MessageType, INetworkMessage>? OnMessageReceived;
    public event Action<ClientConnection, string?>? OnDisconnected;

    // ═══════════════════════════════════════════════════════════════
    // RECEIVE LOOP
    // ═══════════════════════════════════════════════════════════════

    public void StartReceivingAsync() => _ = ReceiveLoopAsync();

    private async Task ReceiveLoopAsync()
    {
        byte[] headerBuffer = new byte[4];

        try
        {
            while (!_cts.Token.IsCancellationRequested && IsConnected)
            {
                // Header lesen
                int bytesRead = await ReadExactAsync(headerBuffer, 4);
                if (bytesRead == 0) break;

                int messageLength = BitConverter.ToInt32(headerBuffer, 0);

                if (messageLength <= 0 || messageLength > 1024 * 1024)
                {
                    _log.Warn("Invalid message length {Length} from {ConnectionId}", messageLength, Id);
                    break;
                }

                // Body lesen
                byte[] bodyBuffer = ArrayPool<byte>.Shared.Rent(messageLength);
                try
                {
                    bytesRead = await ReadExactAsync(bodyBuffer, messageLength);
                    if (bytesRead == 0) break;

                    // Deserialisieren
                    INetworkMessage message = MessageSerializer.Deserialize(
                        new ReadOnlyMemory<byte>(bodyBuffer, 0, messageLength));

                    LastActivity = DateTimeOffset.UtcNow;
                    MessagesReceived++;

                    // ALLE Messages gehen zum GameServer!
                    OnMessageReceived?.Invoke(this, message.Type, message);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(bodyBuffer);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (IOException ex)
        {
            _log.Debug("Connection {ConnectionId} IO error: {Message}", Id, ex.Message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Connection {ConnectionId} receive error", Id);
        }
        finally
        {
            OnDisconnected?.Invoke(this, "Connection closed");
        }
    }

    private async Task<int> ReadExactAsync(byte[] buffer, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int bytesRead = await _stream.ReadAsync(
                buffer.AsMemory(totalRead, count - totalRead),
                _cts.Token);

            if (bytesRead == 0) return 0;
            totalRead += bytesRead;
        }

        return totalRead;
    }

    // ═══════════════════════════════════════════════════════════════
    // SEND
    // ═══════════════════════════════════════════════════════════════

    public void Send(INetworkMessage message)
    {
        if (_disposed || !IsConnected) return;

        try
        {
            byte[] data = MessageSerializer.Serialize(message);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            _sendLock.Wait();
            try
            {
                _stream.Write(lengthPrefix, 0, 4);
                _stream.Write(data, 0, data.Length);
                MessagesSent++;
            }
            finally
            {
                _sendLock.Release();
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error sending to {ConnectionId}", Id);
            Dispose();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // STATE MANAGEMENT (vom Handler aufgerufen)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Setzt den Auth-State.  Wird vom ConnectionHandler aufgerufen.
    /// </summary>
    public void SetAuthenticated(Guid accountId, string username, AccountFlags flags, Guid sessionToken)
    {
        AccountId = accountId;
        Username = username;
        AccountFlags = flags;
        SessionToken = sessionToken;
        State = ConnectionState.Authenticated;

        _log.Debug("Connection {ConnectionId} authenticated as {Username}", Id, username);
    }

    /// <summary>
    ///     Setzt State auf InGame.  Wird vom ConnectionHandler aufgerufen.
    /// </summary>
    public void SetInGame()
    {
        if (State == ConnectionState.Authenticated)
        {
            State = ConnectionState.InGame;
            _log.Debug("Connection {ConnectionId} is now InGame", Id);
        }
    }

    /// <summary>
    ///     Reset auf unauthenticated (Logout).
    /// </summary>
    public void ResetAuth()
    {
        AccountId = null;
        Username = null;
        AccountFlags = AccountFlags.None;
        SessionToken = null;
        State = ConnectionState.Connected;
    }

    /// <summary>
    ///     Aktualisiert die Latenz.
    /// </summary>
    public void UpdateLatency(int latencyMs)
    {
        if (latencyMs < 0 || latencyMs > 10000) return;

        LatencyMs = latencyMs;
        SmoothedLatencyMs = SmoothedLatencyMs == 0
            ? latencyMs
            : (int)(SmoothedLatencyMs * 0.8f + latencyMs * 0.2f);

        LastActivity = DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     Prüft ob Connection "tot" ist.
    /// </summary>
    public bool IsConnectionDead(TimeSpan timeout) => DateTimeOffset.UtcNow - LastActivity > timeout;
}
