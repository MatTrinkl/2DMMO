using System.Collections.Concurrent;
using System.Diagnostics;
using Mmo.Server.MessageRouting;
using Mmo.Server.Messages;
using Mmo.Server.Networking;
using Mmo.Server.Zones;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.System;
using Mmo.Shared.Messages.ZoneEvents;

namespace Mmo.Server.GameLoop;

/// <summary>
///     Der zentrale Game-Server.
///
///     Verantwortlichkeiten:
///     - Game-Loop (Tick) verwalten
///     - Input/Output/Completion Queues verarbeiten
///     - Messages an Handler routen
///     - Broadcasts ausführen
///
///     WICHTIG:
///     - NetworkServer kennt GameServer NICHT (nur Events)
///     - GameServer registriert sich auf NetworkServer-Events
///     - Alle Send-Operationen sind gequeued (Output-Phase)
/// </summary>
public class GameServer : IDisposable
{
    private readonly NetworkServer _networkServer;
    private readonly MessageRouter _messageRouter;
    private readonly ZoneManager _zoneManager;
    private readonly IServiceProvider _services;
    private readonly ILog _log;

    // ═══════════════════════════════════════════════════════════════
    // QUEUES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Eingehende Messages von Clients.
    ///     Gefüllt vom NetworkServer-Event, verarbeitet in ProcessInputQueue().
    /// </summary>
    private readonly ConcurrentQueue<IncomingMessage> _inputQueue = new();

    /// <summary>
    ///     Ausgehende Messages an Clients.
    ///     Gefüllt von Handlern via ctx.Send(), verarbeitet in ProcessOutputQueue().
    /// </summary>
    private readonly ConcurrentQueue<OutgoingMessage> _outputQueue = new();

    /// <summary>
    ///     Callbacks von fertigen async Tasks.
    ///     Gefüllt von ctx.RunAsync(), verarbeitet in ProcessCompletionQueue().
    /// </summary>
    private readonly ConcurrentQueue<(Guid ConnectionId, Action<MessageContext> Callback)> _completionQueue = new();

    /// <summary>
    ///     Connections die getrennt werden sollen.
    ///     Verarbeitet in ProcessDisconnectQueue().
    /// </summary>
    private readonly ConcurrentQueue<(Guid ConnectionId, string? Reason)> _disconnectQueue = new();

    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════

    private bool _isRunning;
    private bool _disposed;
    private Thread? _gameLoopThread;
    private readonly CancellationTokenSource _cts = new();

    // ═══════════════════════════════════════════════════════════════
    // CONFIGURATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Target Tick-Rate (Ticks pro Sekunde).</summary>
    public int TargetTickRate { get; set; } = 20;

    /// <summary>Target Tick-Zeit in Millisekunden.</summary>
    private double TargetTickTimeMs => 1000.0 / TargetTickRate;

    // ═══════════════════════════════════════════════════════════════
    // METRICS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Aktueller Tick-Counter.</summary>
    public long TickCount { get; private set; }

    /// <summary>Letzte Tick-Dauer in Millisekunden.</summary>
    public double LastTickDurationMs { get; private set; }

    /// <summary>Durchschnittliche Tick-Dauer in Millisekunden. </summary>
    public double AverageTickDurationMs { get; private set; }

    /// <summary>Server-Startzeit.</summary>
    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>Server-Uptime.</summary>
    public TimeSpan Uptime => DateTimeOffset.UtcNow - StartedAt;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public GameServer(
        NetworkServer networkServer,
        MessageRouter messageRouter,
        ZoneManager zoneManager,
        IServiceProvider services,
        ILog log)
    {
        _networkServer = networkServer ?? throw new ArgumentNullException(nameof(networkServer));
        _messageRouter = messageRouter ?? throw new ArgumentNullException(nameof(messageRouter));
        _zoneManager = zoneManager ?? throw new ArgumentNullException(nameof(zoneManager));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _log = log ?? throw new ArgumentNullException(nameof(log));

        // ════════════════════════════════════════════════════════════
        // Auf NetworkServer-Events registrieren
        // NetworkServer weiß NICHT dass GameServer existiert!
        // ════════════════════════════════════════════════════════════
        _networkServer.OnMessageReceived += HandleNetworkMessage;
        _networkServer.OnClientConnected += HandleClientConnected;
        _networkServer.OnClientDisconnected += HandleClientDisconnected;
    }

    // ═══════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Startet den Game-Server.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            _log.Warn("GameServer is already running");
            return;
        }

        _isRunning = true;
        StartedAt = DateTimeOffset.UtcNow;

        // Game-Loop in eigenem Thread starten
        _gameLoopThread = new Thread(GameLoopThread)
        {
            Name = "GameLoop",
            IsBackground = false,
            Priority = ThreadPriority.AboveNormal
        };
        _gameLoopThread.Start();

        _log.Info("GameServer started (TickRate: {TickRate}/s)", TargetTickRate);
    }

    /// <summary>
    ///     Stoppt den Game-Server.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _log.Info("GameServer stopping...");

        _isRunning = false;
        _cts.Cancel();

        // Auf Game-Loop-Thread warten
        _gameLoopThread?.Join(TimeSpan.FromSeconds(5));

        // Alle Spieler disconnecten
        DisconnectAllPlayers("Server shutting down");

        _log.Info("GameServer stopped after {TickCount} ticks", TickCount);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();

        // Events abmelden
        _networkServer.OnMessageReceived -= HandleNetworkMessage;
        _networkServer.OnClientConnected -= HandleClientConnected;
        _networkServer.OnClientDisconnected -= HandleClientDisconnected;

        _cts.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════
    // GAME LOOP
    // ═══════════════════════════════════════════════════════════════

    private void GameLoopThread()
    {
        _log.Info("Game loop thread started");

        var stopwatch = new Stopwatch();
        var tickTimes = new Queue<double>(100);

        while (_isRunning && !_cts.Token.IsCancellationRequested)
        {
            stopwatch.Restart();

            try
            {
                // Delta-Time berechnen (in Sekunden)
                float deltaTime = (float)(TargetTickTimeMs / 1000.0);

                // Tick ausführen
                Tick(deltaTime);

                TickCount++;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in game loop tick {TickCount}", TickCount);
            }

            stopwatch.Stop();
            LastTickDurationMs = stopwatch.Elapsed.TotalMilliseconds;

            // Durchschnitt berechnen
            tickTimes.Enqueue(LastTickDurationMs);
            if (tickTimes.Count > 100) tickTimes.Dequeue();
            AverageTickDurationMs = tickTimes.Average();

            // Warnung wenn Tick zu lange dauert
            if (LastTickDurationMs > TargetTickTimeMs * 1.5)
            {
                _log.Warn("Tick {TickCount} took {Duration: F2}ms (target: {Target:F2}ms)",
                    TickCount, LastTickDurationMs, TargetTickTimeMs);
            }

            // Sleep bis zum nächsten Tick
            var sleepTime = TargetTickTimeMs - LastTickDurationMs;
            if (sleepTime > 0)
            {
                Thread.Sleep((int)sleepTime);
            }
        }

        _log.Info("Game loop thread ended");
    }

    /// <summary>
    ///     Führt einen Tick aus.
    /// </summary>
    public void Tick(float deltaTime)
    {
        // 1. COMPLETION PHASE - Fertige async Tasks
        ProcessCompletionQueue();

        // 2. INPUT PHASE - Neue Messages verarbeiten
        ProcessInputQueue();

        // 3. UPDATE PHASE - Game-Logik
        UpdateGameSystems(deltaTime);

        // 4. OUTPUT PHASE - Messages senden
        ProcessOutputQueue();

        // 5. CLEANUP PHASE - Disconnects verarbeiten
        ProcessDisconnectQueue();
    }

    // ═══════════════════════════════════════════════════════════════
    // NETWORK EVENT HANDLERS (vom Network-Thread aufgerufen!)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Wird vom NetworkServer aufgerufen wenn eine Message empfangen wird.
    ///     Läuft auf dem Network-Thread - nur in Queue packen!
    /// </summary>
    private void HandleNetworkMessage(ClientConnection connection, MessageType type, INetworkMessage message)
    {
        var incoming = new IncomingMessage(connection, type, message);
        _inputQueue.Enqueue(incoming);
    }

    /// <summary>
    ///     Wird vom NetworkServer aufgerufen wenn ein Client sich verbindet.
    /// </summary>
    private void HandleClientConnected(ClientConnection connection)
    {
        _log.Debug("Client connected event:  {ConnectionId} from {Endpoint}",
            connection.Id, connection.RemoteEndPoint);
    }

    /// <summary>
    ///     Wird vom NetworkServer aufgerufen wenn ein Client disconnected.
    /// </summary>
    private void HandleClientDisconnected(ClientConnection connection, string? reason)
    {
        _log.Debug("Client disconnected event: {ConnectionId} - {Reason}",
            connection.Id, reason);

        // Cleanup in Queue für Game-Loop
        _disconnectQueue.Enqueue((connection.Id, reason));
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC API (Thread-Safe, von Handlern aufgerufen)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Fügt eine ausgehende Message zur Queue hinzu.
    ///     Wird vom MessageContext aufgerufen.
    /// </summary>
    public void QueueOutgoingMessage(OutgoingMessage message)
    {
        _outputQueue.Enqueue(message);
    }

    /// <summary>
    ///     Queued einen Callback der im nächsten Tick ausgeführt wird.
    ///     Wird von ctx.RunAsync() aufgerufen wenn ein async Task fertig ist.
    /// </summary>
    public void QueueCompletion(Guid connectionId, Action<MessageContext> callback)
    {
        _completionQueue.Enqueue((connectionId, callback));
    }

    /// <summary>
    ///     Markiert eine Connection zum Trennen.
    /// </summary>
    public void QueueDisconnect(Guid connectionId, string? reason = null)
    {
        _disconnectQueue.Enqueue((connectionId, reason));
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPLETION PHASE
    // ═══════════════════════════════════════════════════════════════

    private void ProcessCompletionQueue()
    {
        int processed = 0;
        const int maxPerTick = 100; // Limit um einzelnen Tick nicht zu überlasten

        while (processed < maxPerTick && _completionQueue.TryDequeue(out var completion))
        {
            try
            {
                // Connection noch da?
                if (_networkServer.TryGetConnection(completion.ConnectionId, out var connection) &&
                    connection != null)
                {
                    var ctx = CreateMessageContext(connection);
                    completion.Callback(ctx);
                }
                else
                {
                    _log.Debug("Completion callback skipped - connection {ConnectionId} no longer exists",
                        completion.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error processing completion for {ConnectionId}", completion.ConnectionId);
            }

            processed++;
        }

        if (_completionQueue.Count > 0)
        {
            _log.Debug("Completion queue has {Count} remaining items", _completionQueue.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // INPUT PHASE
    // ═══════════════════════════════════════════════════════════════

    private void ProcessInputQueue()
    {
        int processed = 0;
        const int maxPerTick = 1000; // Limit

        while (processed < maxPerTick && _inputQueue.TryDequeue(out var incoming))
        {
            try
            {
                ProcessMessage(incoming);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error processing message {Type} from {ConnectionId}",
                    incoming.MessageType, incoming.Connection.Id);
            }

            processed++;
        }

        if (_inputQueue.Count > 0)
        {
            _log.Warn("Input queue overloaded:  {Count} messages remaining", _inputQueue.Count);
        }
    }

    /// <summary>
    ///     Verarbeitet eine einzelne eingehende Message.
    /// </summary>
    private void ProcessMessage(IncomingMessage incoming)
    {
        // MessageContext erstellen
        var ctx = CreateMessageContext(incoming.Connection);

        // An Router übergeben
        _messageRouter.Route(ctx, incoming.MessageType, incoming.Message);
    }

    /// <summary>
    ///     Erstellt einen MessageContext für eine Connection.
    /// </summary>
    private MessageContext CreateMessageContext(ClientConnection connection) =>
        new(connection, this, _zoneManager, _services);

    // ═══════════════════════════════════════════════════════════════
    // UPDATE PHASE
    // ═══════════════════════════════════════════════════════════════

    private void UpdateGameSystems(float deltaTime)
    {
        // TODO: Hier kommen die Game-Systeme rein:
        // - AI Update
        // - Combat Update
        // - Buff/Debuff Timers
        // - Respawn Timers
        // - Zone Updates
        // - Heartbeat Service
        // - AFK Detection
        // - etc.

        // Beispiel:  Heartbeat alle 5 Sekunden
        UpdateHeartbeat(deltaTime);

        // Beispiel: AFK/Dead Connection Check
        CheckDeadConnections(deltaTime);
    }

    private float _heartbeatTimer = 0;
    private const float HeartbeatInterval = 5.0f;

    private void UpdateHeartbeat(float deltaTime)
    {
        _heartbeatTimer += deltaTime;

        if (_heartbeatTimer >= HeartbeatInterval)
        {
            _heartbeatTimer = 0;

            // Heartbeat an alle authentifizierten Spieler
            var heartbeat = new Heartbeat
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            foreach (var player in _zoneManager.GetAllServerPlayers())
            {
                var outgoing = OutgoingMessage.ToClient(player.Connection, heartbeat);
                _outputQueue.Enqueue(outgoing);
            }
        }
    }

    private float _deadConnectionTimer = 0;
    private const float DeadConnectionCheckInterval = 10.0f;
    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(30);

    private void CheckDeadConnections(float deltaTime)
    {
        _deadConnectionTimer += deltaTime;

        if (_deadConnectionTimer >= DeadConnectionCheckInterval)
        {
            _deadConnectionTimer = 0;

            var deadConnections = _zoneManager
                .GetAllServerPlayers()
                .Where(p => p.Connection.IsConnectionDead(ConnectionTimeout))
                .Select(p => p.Connection.Id)
                .ToList();

            foreach (var connectionId in deadConnections)
            {
                _log.Warn("Connection {ConnectionId} timed out", connectionId);
                _disconnectQueue.Enqueue((connectionId, "Connection timeout"));
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // OUTPUT PHASE
    // ═══════════════════════════════════════════════════════════════

    private void ProcessOutputQueue()
    {
        while (_outputQueue.TryDequeue(out var outgoing))
        {
            try
            {
                SendMessage(outgoing);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error sending message {Type}", outgoing.Type);
            }
        }
    }

    private void SendMessage(OutgoingMessage outgoing)
    {
        switch (outgoing.Type)
        {
            case OutgoingMessageType.ToClient:
                SendToClient(outgoing);
                break;

            case OutgoingMessageType.BroadcastToZone:
                BroadcastToZone(outgoing);
                break;

            case OutgoingMessageType.BroadcastToZoneExcept:
                BroadcastToZoneExcept(outgoing);
                break;

            case OutgoingMessageType.BroadcastToNearby:
                BroadcastToNearby(outgoing);
                break;

            case OutgoingMessageType.BroadcastToParty:
                BroadcastToParty(outgoing, includeExcluded: true);
                break;

            case OutgoingMessageType.BroadcastToPartyExcept:
                BroadcastToParty(outgoing, includeExcluded: false);
                break;

            case OutgoingMessageType.BroadcastToGuild:
                BroadcastToGuild(outgoing, includeExcluded: true);
                break;

            case OutgoingMessageType.BroadcastToGuildExcept:
                BroadcastToGuild(outgoing, includeExcluded: false);
                break;

            case OutgoingMessageType.BroadcastToAll:
                BroadcastToAll(outgoing);
                break;

            default:
                _log.Warn("Unknown outgoing message type: {Type}", outgoing.Type);
                break;
        }
    }

    private void SendToClient(OutgoingMessage outgoing)
    {
        if (outgoing.TargetConnection != null && outgoing.TargetConnection.IsConnected)
        {
            _networkServer.Send(outgoing.TargetConnection, outgoing.Message);
        }
    }

    private void BroadcastToZone(OutgoingMessage outgoing)
    {
        if (!outgoing.ZoneId.HasValue) return;

        var players = _zoneManager.GetServerPlayersInZone(outgoing.ZoneId.Value);

        foreach (var player in players)
        {
            if (player.Connection.IsConnected)
            {
                _networkServer.Send(player.Connection, outgoing.Message);
            }
        }
    }

    private void BroadcastToZoneExcept(OutgoingMessage outgoing)
    {
        if (!outgoing.ZoneId.HasValue) return;

        var players = _zoneManager.GetServerPlayersInZone(outgoing.ZoneId.Value);

        foreach (var player in players)
        {
            if (player.Connection.Id != outgoing.ExcludeConnectionId &&
                player.Connection.IsConnected)
            {
                _networkServer.Send(player.Connection, outgoing.Message);
            }
        }
    }

    private void BroadcastToNearby(OutgoingMessage outgoing)
    {
        if (!outgoing.ZoneId.HasValue || outgoing.Origin == null || !outgoing.Radius.HasValue)
            return;

        var players = _zoneManager.GetServerPlayersInZone(outgoing.ZoneId.Value);
        var origin = outgoing.Origin;
        var radiusSquared = outgoing.Radius.Value * outgoing.Radius.Value;

        foreach (var player in players)
        {
            if (player.Connection.Id == outgoing.ExcludeConnectionId)
                continue;

            if (!player.Connection.IsConnected)
                continue;

            var pos = player.Entity.Position;
            var dx = pos.X - origin.X;
            var dy = pos.Y - origin.Y;
            var distanceSquared = dx * dx + dy * dy;

            if (distanceSquared <= radiusSquared)
            {
                _networkServer.Send(player.Connection, outgoing.Message);
            }
        }
    }

    private void BroadcastToParty(OutgoingMessage outgoing, bool includeExcluded)
    {
        if (!outgoing.PartyId.HasValue) return;

        var partyMembers = _zoneManager
            .GetAllServerPlayers()
            .Where(p => p.PartyId == outgoing.PartyId.Value);

        foreach (var player in partyMembers)
        {
            if (!includeExcluded && player.Connection.Id == outgoing.ExcludeConnectionId)
                continue;

            if (player.Connection.IsConnected)
            {
                _networkServer.Send(player.Connection, outgoing.Message);
            }
        }
    }

    private void BroadcastToGuild(OutgoingMessage outgoing, bool includeExcluded)
    {
        if (!outgoing.GuildId.HasValue) return;

        var guildMembers = _zoneManager
            .GetAllServerPlayers()
            .Where(p => p.GuildId == outgoing.GuildId.Value);

        foreach (var player in guildMembers)
        {
            if (!includeExcluded && player.Connection.Id == outgoing.ExcludeConnectionId)
                continue;

            if (player.Connection.IsConnected)
            {
                _networkServer.Send(player.Connection, outgoing.Message);
            }
        }
    }

    private void BroadcastToAll(OutgoingMessage outgoing)
    {
        var allPlayers = _zoneManager.GetAllServerPlayers();

        foreach (var player in allPlayers)
        {
            if (player.Connection.IsConnected)
            {
                _networkServer.Send(player.Connection, outgoing.Message);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // DISCONNECT PHASE
    // ═══════════════════════════════════════════════════════════════

    private void ProcessDisconnectQueue()
    {
        while (_disconnectQueue.TryDequeue(out var disconnect))
        {
            try
            {
                ProcessDisconnect(disconnect.ConnectionId, disconnect.Reason);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error processing disconnect for {ConnectionId}", disconnect.ConnectionId);
            }
        }
    }

    private void ProcessDisconnect(Guid connectionId, string? reason)
    {
        // Spieler aus ZoneManager entfernen
        var player = _zoneManager.RemovePlayerByConnectionId(connectionId);

        if (player != null)
        {
            _log.Info("Player {Name} removed from zone {ZoneId}:  {Reason}",
                player.Name, player.RuntimeId.ZoneId, reason ?? "Unknown");

            // Broadcast an Zone:  Spieler hat verlassen
            var leftMessage = new PlayerLeftZone(player.Entity);
            var outgoing = OutgoingMessage.BroadcastToZoneExcept(
                leftMessage,
                player.RuntimeId.ZoneId,
                connectionId
            );
            _outputQueue.Enqueue(outgoing);
        }

        // Connection im NetworkServer schließen
        _networkServer.RemoveConnection(connectionId, reason);
    }

    private void DisconnectAllPlayers(string reason)
    {
        var allPlayers = _zoneManager.GetAllServerPlayers().ToList();

        foreach (var player in allPlayers)
        {
            try
            {
                // Disconnect-Nachricht senden
                var disconnectMsg = new Disconnect
                {
                    Reason = DisconnectReason.ServerShutdown,
                    Message = reason
                };
                _networkServer.Send(player.Connection, disconnectMsg);

                // Aus ZoneManager entfernen
                _zoneManager.RemovePlayerByConnectionId(player.Connection.Id);

                // Connection schließen
                _networkServer.RemoveConnection(player.Connection.Id, reason);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error disconnecting player {Name}", player.Name);
            }
        }

        _log.Info("Disconnected {Count} players:  {Reason}", allPlayers.Count, reason);
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC UTILITIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Sendet eine Server-Ankündigung an alle Spieler.
    /// </summary>
    public void BroadcastAnnouncement(string message, AnnouncementType type = AnnouncementType.Info)
    {
        var announcement = new ServerAnnouncement
        {
            Message = message,
            AnnouncementType = type,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        _outputQueue.Enqueue(OutgoingMessage.BroadcastToAll(announcement));
        _log.Info("Server announcement:  {Message}", message);
    }

    /// <summary>
    ///     Holt Server-Statistiken.
    /// </summary>
    public ServerStats GetStats()
    {
        return new ServerStats
        {
            TickCount = TickCount,
            Uptime = Uptime,
            TargetTickRate = TargetTickRate,
            LastTickDurationMs = LastTickDurationMs,
            AverageTickDurationMs = AverageTickDurationMs,
            PlayerCount = _zoneManager.GetAllServerPlayers().Count(),
            ConnectionCount = _networkServer.ConnectionCount,
            InputQueueSize = _inputQueue.Count,
            OutputQueueSize = _outputQueue.Count,
            CompletionQueueSize = _completionQueue.Count
        };
    }
}
