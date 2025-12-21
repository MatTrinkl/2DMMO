using System.Collections.Concurrent;
using System.Diagnostics;
using Mmo.Server.Connections;
using Mmo.Server.Core.Structs;
using Mmo.Server.MessageRouting;
using Mmo.Server.Messages;
using Mmo.Server.Messages.Enums;
using Mmo.Server.Network;
using Mmo.Server.PlayerService;
using Mmo.Server.Zones;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Connection.Messages;
using Mmo.Shared.Core.Constants;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.System.Enums;
using Mmo.Shared.System.Messages;
using Mmo.Shared.Zones.Messages.Server_Brodcast;

namespace Mmo.Server.Core;

/// <summary>
///     The central Game Server.
///     Responsibilities:
///     - Manage the Game Loop (Tick)
///     - Process Input/Output/Completion Queues
///     - Route messages to handlers
///     - Execute broadcasts
///     IMPORTANT:
///     - NetworkServer does NOT know about GameServer (only events)
///     - GameServer registers for NetworkServer events
///     - All send operations are queued (Output phase)
/// </summary>
public class GameServer : IDisposable
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    private const float _heartbeatInterval = 5.0f;
    private const float _deadConnectionCheckInterval = 10.0f;
    private static readonly TimeSpan _connectionTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Callbacks from completed async tasks.
    ///     Populated by ctx.RunAsync(), processed in ProcessCompletionQueue().
    /// </summary>
    private readonly ConcurrentQueue<(Guid ConnectionId, Action<MessageContext> Callback)> _completionQueue = new();

    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    ///     Connections that should be disconnected.
    ///     Processed in ProcessDisconnectQueue().
    /// </summary>
    private readonly ConcurrentQueue<(Guid ConnectionId, string? Reason)> _disconnectQueue = new();

    /// <summary>
    ///     Incoming messages from clients.
    ///     Populated by NetworkServer event, processed in ProcessInputQueue().
    /// </summary>
    private readonly ConcurrentQueue<IncomingMessage> _inputQueue = new();

    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly ILog _log;
    private readonly MessageRouter _messageRouter;
    private readonly NetworkServer _networkServer;

    /// <summary>
    ///     Outgoing messages to clients.
    ///     Populated by handlers via ctx.Send(), processed in ProcessOutputQueue().
    /// </summary>
    private readonly ConcurrentQueue<OutgoingMessage> _outputQueue = new();


    private readonly IServiceProvider _services;
    private readonly ZoneManager _zoneManager;

    private float _deadConnectionTimer;
    private bool _disposed;
    private Thread? _gameLoopThread;
    private float _heartbeatTimer;
    private bool _isRunning;

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
        // Register for NetworkServer events
        // NetworkServer does NOT know that GameServer exists!
        // ════════════════════════════════════════════════════════════
        _networkServer.OnMessageReceived += HandleNetworkMessage;
        _networkServer.OnClientConnected += HandleClientConnected;
        _networkServer.OnClientDisconnected += HandleClientDisconnected;
    }

    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Target tick rate (ticks per second).</summary>
    public int TargetTickRate { get; set; } = SharedConstants.TickRate;

    /// <summary>Target tick time in milliseconds.</summary>
    private double TargetTickTimeMs => 1000.0 / TargetTickRate;

    /// <summary>Current tick counter.</summary>
    public long TickCount { get; private set; }

    /// <summary>Duration of the last tick in milliseconds.</summary>
    public double LastTickDurationMs { get; private set; }

    /// <summary>Average tick duration in milliseconds.</summary>
    public double AverageTickDurationMs { get; private set; }

    /// <summary>Server start time.</summary>
    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>Server uptime.</summary>
    public TimeSpan Uptime => DateTimeOffset.UtcNow - StartedAt;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();

        // Unregister events
        _networkServer.OnMessageReceived -= HandleNetworkMessage;
        _networkServer.OnClientConnected -= HandleClientConnected;
        _networkServer.OnClientDisconnected -= HandleClientDisconnected;

        _cts.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Starts the Game Server.
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

        // Start Game Loop in separate thread
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
    ///     Stops the Game Server.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;

        _log.Info("GameServer stopping...");

        _isRunning = false;
        _cts.Cancel();

        // Wait for Game Loop thread
        _gameLoopThread?.Join(TimeSpan.FromSeconds(5));

        // Disconnect all players
        DisconnectAllPlayers("Server shutting down");

        _log.Info("GameServer stopped after {TickCount} ticks", TickCount);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS - GAME LOOP
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
                // Calculate delta time (in seconds)
                float deltaTime = (float)(TargetTickTimeMs / 1000.0);

                // Execute tick
                Tick(deltaTime);

                TickCount++;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in game loop tick {TickCount}", TickCount);
            }

            stopwatch.Stop();
            LastTickDurationMs = stopwatch.Elapsed.TotalMilliseconds;

            // Calculate average
            tickTimes.Enqueue(LastTickDurationMs);
            if (tickTimes.Count > 100) tickTimes.Dequeue();
            AverageTickDurationMs = tickTimes.Average();

            // Warn if tick takes too long
            if (LastTickDurationMs > TargetTickTimeMs * 1.5)
                _log.Warn("Tick {TickCount} took {Duration: F2}ms (target: {Target:F2}ms)",
                    TickCount, LastTickDurationMs, TargetTickTimeMs);

            // Sleep until next tick
            double sleepTime = TargetTickTimeMs - LastTickDurationMs;
            if (sleepTime > 0) Thread.Sleep((int)sleepTime);
        }

        _log.Info("Game loop thread ended");
    }

    /// <summary>
    ///     Executes one tick of the Game Loop.
    ///     The Game Loop consists of five phases that run sequentially:
    ///     1. COMPLETION - Process callbacks from completed async operations
    ///     2. INPUT - Process incoming messages from clients
    ///     3. UPDATE - Execute game logic and system updates
    ///     4. OUTPUT - Send outgoing messages to clients
    ///     5. CLEANUP - Process connection disconnects
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last tick in seconds.</param>
    protected virtual void Tick(float deltaTime)
    {
        // 1. COMPLETION PHASE - Completed async tasks
        ProcessCompletionQueue();

        // 2. INPUT PHASE - Process new messages
        ProcessInputQueue();

        // 3. UPDATE PHASE - Game logic
        UpdateGameSystems(deltaTime);

        // 4. OUTPUT PHASE - Send messages
        ProcessOutputQueue();

        // 5. CLEANUP PHASE - Process disconnects
        ProcessDisconnectQueue();
    }

    // ───────────────────────────────────────────────────────────────
    // Network Event Handlers (called from Network thread!)
    // ───────────────────────────────────────────────────────────────

    /// <summary>
    ///     Called by NetworkServer when a message is received.
    ///     Runs on Network thread - only add to queue!
    /// </summary>
    private void HandleNetworkMessage(ClientConnection connection, MessageType type, INetworkMessage message)
    {
        var incoming = new IncomingMessage(connection, type, message);
        _inputQueue.Enqueue(incoming);
    }

    /// <summary>
    ///     Called by NetworkServer when a client connects.
    /// </summary>
    private void HandleClientConnected(ClientConnection connection)
    {
        _log.Debug("Client connected event:  {ConnectionId} from {Endpoint}",
            connection.Id, connection.RemoteEndPoint);
    }

    /// <summary>
    ///     Called by NetworkServer when a client disconnects.
    /// </summary>
    private void HandleClientDisconnected(ClientConnection connection, string? reason)
    {
        _log.Debug("Client disconnected event: {ConnectionId} - {Reason}",
            connection.Id, reason ?? "No reason logged.");

        // Queue cleanup for Game Loop
        _disconnectQueue.Enqueue((connection.Id, reason));
    }

    // ───────────────────────────────────────────────────────────────
    // Public API (Thread-safe, called by handlers)
    // ───────────────────────────────────────────────────────────────

    /// <summary>
    ///     Adds an outgoing message to the queue.
    ///     Called by MessageContext.
    /// </summary>
    public void QueueOutgoingMessage(OutgoingMessage message) => _outputQueue.Enqueue(message);

    /// <summary>
    ///     Queues a callback to be executed in the next tick.
    ///     Called by ctx.RunAsync() when an async task completes.
    /// </summary>
    public void QueueCompletion(Guid connectionId, Action<MessageContext> callback) =>
        _completionQueue.Enqueue((connectionId, callback));

    /// <summary>
    ///     Marks a connection for disconnection.
    /// </summary>
    public void QueueDisconnect(Guid connectionId, string? reason = null) =>
        _disconnectQueue.Enqueue((connectionId, reason));

    // ───────────────────────────────────────────────────────────────
    // COMPLETION PHASE
    // ───────────────────────────────────────────────────────────────

    private void ProcessCompletionQueue()
    {
        int processed = 0;
        const int maxPerTick = 100; // Limit to avoid overloading a single tick

        while (processed < maxPerTick &&
               _completionQueue.TryDequeue(out (Guid ConnectionId, Action<MessageContext> Callback) completion))
        {
            try
            {
                // Connection still exists?
                if (_networkServer.TryGetConnection(completion.ConnectionId, out ClientConnection? connection) &&
                    connection != null)
                {
                    MessageContext ctx = CreateMessageContext(connection);
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
            _log.Debug("Completion queue has {Count} remaining items", _completionQueue.Count);
    }

    // ───────────────────────────────────────────────────────────────
    // INPUT PHASE
    // ───────────────────────────────────────────────────────────────

    private void ProcessInputQueue()
    {
        int processed = 0;
        const int maxPerTick = 1000; // Limit

        while (processed < maxPerTick && _inputQueue.TryDequeue(out IncomingMessage incoming))
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

        if (_inputQueue.Count > 0) _log.Warn("Input queue overloaded:  {Count} messages remaining", _inputQueue.Count);
    }

    /// <summary>
    ///     Processes a single incoming message by creating a context and routing it to the appropriate handler.
    /// </summary>
    private void ProcessMessage(IncomingMessage incoming)
    {
        // Create MessageContext
        MessageContext ctx = CreateMessageContext(incoming.Connection);

        // Route to handler
        _messageRouter.Route(ctx, incoming.MessageType, incoming.Message);
    }

    /// <summary>
    ///     Creates a MessageContext for a connection.
    ///     The context provides access to game services and enables handlers to send responses.
    /// </summary>
    private MessageContext CreateMessageContext(ClientConnection connection) =>
        new(connection, this, _zoneManager, _services);

    // ───────────────────────────────────────────────────────────────
    // UPDATE PHASE
    // ───────────────────────────────────────────────────────────────

    private void UpdateGameSystems(float deltaTime)
    {
        // TODO: Game systems will be added here:
        // - AI Update
        // - Combat Update
        // - Buff/Debuff Timers
        // - Respawn Timers
        // - Zone Updates
        // - Heartbeat Service
        // - AFK Detection
        // - etc.

        // Example: Heartbeat every 5 seconds
        UpdateHeartbeat(deltaTime);

        // Example: AFK/Dead Connection Check
        CheckDeadConnections(deltaTime);
    }

    private void UpdateHeartbeat(float deltaTime)
    {
        _heartbeatTimer += deltaTime;

        if (_heartbeatTimer < _heartbeatInterval) return;
        _heartbeatTimer = 0;

        // Send heartbeat to all authenticated players
        var heartbeat = new Heartbeat
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };


        var outgoing = OutgoingMessage.BroadcastToAll(heartbeat);
        _outputQueue.Enqueue(outgoing);
    }

    private void CheckDeadConnections(float deltaTime)
    {
        _deadConnectionTimer += deltaTime;

        if (_deadConnectionTimer < _deadConnectionCheckInterval) return;
        _deadConnectionTimer = 0;

        var deadConnections = _zoneManager
            .GetAllServerPlayers()
            .Where(p => p.Connection.IsConnectionDead(_connectionTimeout))
            .Select(p => p.Connection.Id)
            .ToList();

        foreach (Guid connectionId in deadConnections)
        {
            _log.Warn("Connection {ConnectionId} timed out", connectionId);
            _disconnectQueue.Enqueue((connectionId, "Connection timeout"));
        }
    }

    // ───────────────────────────────────────────────────────────────
    // OUTPUT PHASE
    // ───────────────────────────────────────────────────────────────

    private void ProcessOutputQueue()
    {
        while (_outputQueue.TryDequeue(out OutgoingMessage outgoing))
            try
            {
                SendMessage(outgoing);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error sending message {Type}", outgoing.Type);
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
                BroadcastToParty(outgoing, true);
                break;

            case OutgoingMessageType.BroadcastToPartyExcept:
                BroadcastToParty(outgoing, false);
                break;

            case OutgoingMessageType.BroadcastToGuild:
                BroadcastToGuild(outgoing, true);
                break;

            case OutgoingMessageType.BroadcastToGuildExcept:
                BroadcastToGuild(outgoing, false);
                break;

            case OutgoingMessageType.BroadcastToAll:
                BroadcastToAll(outgoing, true);
                break;

            case OutgoingMessageType.BroadcastToAllExcept:
                BroadcastToAll(outgoing, false);
                break;
            default:
                _log.Warn("Unknown outgoing message type: {Type}", outgoing.Type);
                break;
        }
    }

    private void SendToClient(OutgoingMessage outgoing)
    {
        if (outgoing.TargetConnection is { IsConnected: true })
            _networkServer.Send(outgoing.TargetConnection, outgoing.Message);
    }

    private void BroadcastToZone(OutgoingMessage outgoing)
    {
        if (!outgoing.ZoneId.HasValue) return;

        IEnumerable<ServerPlayerCharacter> players = _zoneManager.GetServerPlayersInZone(outgoing.ZoneId.Value);

        foreach (ServerPlayerCharacter player in players.Where(p => p.Connection.IsConnected))
            _networkServer.Send(player.Connection, outgoing.Message);
    }

    private void BroadcastToZoneExcept(OutgoingMessage outgoing)
    {
        if (!outgoing.ZoneId.HasValue) return;

        IEnumerable<ServerPlayerCharacter> players = _zoneManager.GetServerPlayersInZone(outgoing.ZoneId.Value);

        foreach (ServerPlayerCharacter player in players.Where(p => p.Connection.Id != outgoing.ExcludeConnectionId &&
                                                                    p.Connection.IsConnected))
            _networkServer.Send(player.Connection, outgoing.Message);
    }

    private void BroadcastToNearby(OutgoingMessage outgoing)
    {
        if (!outgoing.ZoneId.HasValue || outgoing.Origin == null || !outgoing.Radius.HasValue)
            return;

        IEnumerable<ServerPlayerCharacter> players = _zoneManager.GetServerPlayersInZone(outgoing.ZoneId.Value);
        Position? origin = outgoing.Origin;
        float radiusSquared = outgoing.Radius.Value * outgoing.Radius.Value;

        foreach (ServerPlayerCharacter player in players.Where(p =>
                     p.Connection.IsConnected && p.Connection.Id != outgoing.ExcludeConnectionId))
        {
            Position pos = player.Entity.Position;
            float dx = pos.X - origin.X;
            float dy = pos.Y - origin.Y;
            float distanceSquared = dx * dx + dy * dy;

            if (distanceSquared <= radiusSquared) _networkServer.Send(player.Connection, outgoing.Message);
        }
    }

    private void BroadcastToParty(OutgoingMessage outgoing, bool includeExcluded)
    {
        if (!outgoing.PartyId.HasValue) return;

        IEnumerable<ServerPlayerCharacter> partyMembers = _zoneManager
            .GetAllServerPlayers()
            .Where(p => p.PartyId == outgoing.PartyId.Value);

        foreach (ServerPlayerCharacter player in partyMembers)
        {
            if (!includeExcluded && player.Connection.Id == outgoing.ExcludeConnectionId)
                continue;

            if (player.Connection.IsConnected) _networkServer.Send(player.Connection, outgoing.Message);
        }
    }

    private void BroadcastToGuild(OutgoingMessage outgoing, bool includeExcluded)
    {
        if (!outgoing.GuildId.HasValue) return;

        IEnumerable<ServerPlayerCharacter> guildMembers = _zoneManager
            .GetAllServerPlayers()
            .Where(p => p.GuildId == outgoing.GuildId.Value);

        foreach (ServerPlayerCharacter player in guildMembers)
        {
            if (!includeExcluded && player.Connection.Id == outgoing.ExcludeConnectionId)
                continue;

            if (player.Connection.IsConnected) _networkServer.Send(player.Connection, outgoing.Message);
        }
    }

    private void BroadcastToAll(OutgoingMessage outgoing, bool includeExcluded)
    {
        IEnumerable<ServerPlayerCharacter> allPlayers = _zoneManager.GetAllServerPlayers();

        foreach (ServerPlayerCharacter player in allPlayers)
        {
            if (!includeExcluded && player.Connection.Id == outgoing.ExcludeConnectionId)
                continue;

            if (player.Connection.IsConnected) _networkServer.Send(player.Connection, outgoing.Message);
        }
    }

    // ───────────────────────────────────────────────────────────────
    // DISCONNECT PHASE
    // ───────────────────────────────────────────────────────────────

    private void ProcessDisconnectQueue()
    {
        while (_disconnectQueue.TryDequeue(out (Guid ConnectionId, string? Reason) disconnect))
            try
            {
                ProcessDisconnect(disconnect.ConnectionId, disconnect.Reason);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error processing disconnect for {ConnectionId}", disconnect.ConnectionId);
            }
    }

    private void ProcessDisconnect(Guid connectionId, string? reason)
    {
        // Remove player from ZoneManager
        ServerPlayerCharacter? player = _zoneManager.RemoveServerPlayer(connectionId);

        if (player != null)
        {
            _log.Info("Player {Name} removed from zone {ZoneId}:  {Reason}",
                player.Name, player.RuntimeId.ZoneId, reason ?? "Unknown");

            // Broadcast to zone: Player has left
            var leftMessage = new PlayerLeftZone(player.Entity);
            var outgoing = OutgoingMessage.BroadcastToZoneExcept(
                leftMessage,
                player.RuntimeId.ZoneId,
                connectionId
            );
            _outputQueue.Enqueue(outgoing);
        }

        // Close connection in NetworkServer
        _networkServer.RemoveConnection(connectionId, reason);
    }

    private void DisconnectAllPlayers(string reason)
    {
        var allPlayers = _zoneManager.GetAllServerPlayers().ToList();

        foreach (ServerPlayerCharacter player in allPlayers)
            try
            {
                // Send disconnect message
                var disconnectMsg = new Disconnect
                {
                    Reason = DisconnectReason.ServerShutdown,
                    Message = reason
                };
                _networkServer.Send(player.Connection, disconnectMsg);

                // Remove from ZoneManager
                _zoneManager.RemoveServerPlayer(player.Connection.Id);

                // Close connection
                _networkServer.RemoveConnection(player.Connection.Id, reason);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error disconnecting player {Name}", player.Name);
            }

        _log.Info("Disconnected {Count} players:  {Reason}", allPlayers.Count, reason);
    }

    // ───────────────────────────────────────────────────────────────
    // Public Utilities
    // ───────────────────────────────────────────────────────────────

    /// <summary>
    ///     Sends a server announcement to all players.
    /// </summary>
    /// <param name="message">The announcement message text.</param>
    /// <param name="type">The type/severity of the announcement (Info, Warning, etc.).</param>
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
    ///     Gets current server statistics including tick performance and queue sizes.
    /// </summary>
    /// <returns>A ServerStats object containing current metrics.</returns>
    public ServerStats GetStats()
    {
        return new ServerStats(TickCount, Uptime, LastTickDurationMs, AverageTickDurationMs,
            _zoneManager.GetAllServerPlayers().Count(), _networkServer.ConnectionCount, _inputQueue.Count,
            _outputQueue.Count, _completionQueue.Count);
    }
}
