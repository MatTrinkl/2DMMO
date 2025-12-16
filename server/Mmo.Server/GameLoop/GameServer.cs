using System.Collections.Concurrent;
using System.Diagnostics;
using Mmo.Server.Entities;
using Mmo.Server.MessageRouting;
using Mmo.Server.Networking;
using Mmo.Server.Networking.NetworkEvents;
using Mmo.Server.Zones;
using Mmo.Shared;
using Mmo.Shared.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Messages.ZoneEvents;
using Mmo.Shared.Zones;

namespace Mmo.Server.GameLoop;

/// <summary>
///     This class is the core server structure.  All communication will be done with an instance of this class.
/// </summary>
public class GameServer
{
    /// <summary>
    ///     PersistentIds of entities that changed this tick (for delta updates).
    /// </summary>
    private readonly HashSet<Guid> _dirtyEntities = new();

    /// <summary>
    ///     Incoming messages from clients, processed in InputPhase.
    /// </summary>
    private readonly ConcurrentQueue<MessageReceivedEventArgs> _incomingMessages = new();

    private readonly ILog _log;
    internal readonly INetworkServer NetworkServer;
    private readonly MessageRouter _router;

    /// <summary>
    ///     Outgoing event broadcasts (PlayerJoined, PlayerLeft, Chat, etc. ),
    ///     collected during the tick and sent in OutputPhase.
    /// </summary>
    private readonly ConcurrentQueue<INetworkMessage> _pendingBroadcasts = new();

    /// <summary>
    ///     Creates a new GameServer object.
    /// </summary>
    /// <param name="log">The logging interface.</param>
    /// <param name="networkServer">The network server for client communication.</param>
    public GameServer(ILog log, INetworkServer networkServer)
    {
        _log = log;
        NetworkServer = networkServer;
        _router = new MessageRouter(this,log);

        CurrentTick = 0;
        IsRunning = false;

        // Only temporary - later loaded from config files.
        ZoneManager = new ZoneManager(0, new Zone(0, "default", new ZoneBounds(0, 0, 0, 0)));

        // Subscribe to network events
        NetworkServer.ClientConnected += OnClientConnected;
        NetworkServer.ClientDisconnected += OnClientDisconnected;
        NetworkServer.MessageReceived += OnMessageReceived;
        NetworkServer.ErrorOccurred += OnNetworkError;
    }

    /// <summary>
    ///     True if the server is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    ///     The current tick of the server.
    /// </summary>
    public long CurrentTick { get; private set; }

    /// <summary>
    ///     Access to the ZoneManager for external usage.
    /// </summary>
    public ZoneManager ZoneManager { get; }

    /// <summary>
    ///     Starts the server and keeps its loop until the cancellation is requested.
    ///     Then the final tick will run and the server shuts down.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token. </param>
    public async Task StartServerAsync(CancellationToken cancellationToken)
    {
        _log.Info("GameServer starting with {TickRate} Hz...", SharedConstants.TickRate);
        IsRunning = true;
        var stopwatch = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            TimeSpan tickStart = stopwatch.Elapsed;

            // 1️⃣ INPUT PHASE
            await InputPhaseAsync(cancellationToken);

            // 2️⃣ UPDATE PHASE
            await UpdatePhaseAsync(cancellationToken);

            // 3️⃣ OUTPUT PHASE
            await OutputPhaseAsync(cancellationToken);

            TimeSpan elapsed = stopwatch.Elapsed - tickStart;
            double elapsedMs = elapsed.TotalMilliseconds;
            double budgetMs = SharedConstants.TickDuration.TotalMilliseconds;

            // Tick-Overrun Logging
            if (elapsed > SharedConstants.TickDuration)
            {
                _log.Warn(
                    "Tick {Tick} overrun: {ElapsedMs:F2} ms (budget: {BudgetMs: F2} ms)",
                    CurrentTick,
                    elapsedMs,
                    budgetMs);

                continue;
            }

            TimeSpan remaining = SharedConstants.TickDuration - elapsed;

            try
            {
                await Task.Delay(remaining, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        IsRunning = false;
        _log.Info("GameServer stopped after {Ticks} ticks.", CurrentTick);
    }

    /// <summary>
    ///     Input Phase: Process all incoming messages from clients.
    /// </summary>
    protected virtual Task InputPhaseAsync(CancellationToken cancellationToken)
    {
        while (_incomingMessages. TryDequeue(out MessageReceivedEventArgs? incoming))
        {
            _router.Route(incoming.Connection, incoming.Message);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Update Phase:  Validate positions, check collisions, update game state.
    /// </summary>
    protected virtual Task UpdatePhaseAsync(CancellationToken cancellationToken)
    {
        // TODO:  Position validate, check collisions, update GameState (ZoneState)
        CurrentTick++;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Output Phase: Send broadcasts to clients (events, delta updates, full state).
    /// </summary>
    protected virtual async Task OutputPhaseAsync(CancellationToken cancellationToken)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // ══════════════════════════════════════════════════════════
        // 1. EVENT BROADCASTS (highest priority)
        // ══════════════════════════════════════════════════════════
        while (_pendingBroadcasts.TryDequeue(out INetworkMessage? eventMessage))
        {
            // TODO: Send Events zone-specific if necessary.
            await NetworkServer.BroadcastAsync(eventMessage);
            _log.Debug("Event broadcast:  {MessageType}", eventMessage.Type);
        }

        // ══════════════════════════════════════════════════════════
        // 2. FULL STATE:  All n ticks - per zone
        // ══════════════════════════════════════════════════════════
        if (CurrentTick % SharedConstants.TickRate == 0)
        {
            await BroadcastFullZoneStatesAsync(timestamp);
            _dirtyEntities.Clear();
        }
        // ══════════════════════════════════════════════════════════
        // 3. DELTA:  Update dirty Entities - per zone
        // ══════════════════════════════════════════════════════════
        else if (_dirtyEntities.Count > 0)
        {
            await BroadcastDirtyEntitiesAsync(timestamp);
            _dirtyEntities.Clear();
        }
    }

    /// <summary>
    ///     Broadcasts full ZoneState to all players in each zone.
    /// </summary>
    private async Task BroadcastFullZoneStatesAsync(long timestamp)
    {
        foreach (Zone zone in ZoneManager.GetAllZones())
        {
            var playersInZone = ZoneManager.GetServerPlayersInZone(zone.ZoneId).ToList();

            if (playersInZone.Count == 0)
                continue;

            var zoneState = new ZoneState(
                timestamp,
                zone.ZoneId,
                ZoneManager.GetAllEntities(zone.ZoneId)
            );

            await BroadcastToPlayersAsync(playersInZone, zoneState);

            _log.Debug("Full ZoneState for Zone {ZoneId} → {PlayerCount} players at tick {Tick}",
                zone.ZoneId, playersInZone.Count, CurrentTick);
        }
    }

    /// <summary>
    ///     Broadcasts position updates only for dirty entities to relevant zones.
    /// </summary>
    private async Task BroadcastDirtyEntitiesAsync(long timestamp)
    {
        // Collect dirty Entities and group them by Zone
        IEnumerable<IGrouping<ushort, IEntity>> entitiesByZone = _dirtyEntities
            .Select(persistentId =>
                ZoneManager.TryGetEntityByPersistentId(persistentId, out IEntity? entity) ? entity : null)
            .OfType<IEntity>()
            .GroupBy(e => e.RuntimeId.ZoneId);

        foreach (IGrouping<ushort, IEntity> zoneGroup in entitiesByZone)
        {
            ushort zoneId = zoneGroup.Key;
            var playersInZone = ZoneManager.GetServerPlayersInZone(zoneId).ToList();

            if (playersInZone.Count == 0)
                continue;

            foreach (IEntity entity in zoneGroup)
            {
                var positionBroadcast = new PositionBroadcast(
                    timestamp,
                    entity.PersistentId,
                    entity.Position
                );

                await BroadcastToPlayersAsync(playersInZone, positionBroadcast);
            }

            _log.Debug("Delta broadcast for Zone {ZoneId}:  {EntityCount} entities → {PlayerCount} players",
                zoneId, zoneGroup.Count(), playersInZone.Count);
        }
    }

    /// <summary>
    ///     Broadcasts a message to a specific list of players.
    /// </summary>
    private async Task BroadcastToPlayersAsync(IEnumerable<ServerPlayer> players, INetworkMessage message)
    {
        IEnumerable<Task> tasks = players.Select(p => NetworkServer.SendToClientAsync(p.Connection, message));
        await Task.WhenAll(tasks);
    }

    private void OnClientConnected(object? sender, ClientConnectedEventArgs e)
    {
        _log.Info("Client {ClientId} connected from {EndPoint}",
            e.ClientId, e.RemoteEndPoint);

        // No Player yet - only after LoginRequest
    }

    private void OnClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
    {
        _log.Info("Client {ClientId} disconnected:  {Reason}",
            e.ClientId, e.Reason);

        // Remove Player from Zone via ZoneManager
        ServerPlayer? serverPlayer = ZoneManager.RemovePlayerByConnectionId(e.ClientId);

        if (serverPlayer == null) return;
        _log.Info("Player {PlayerName} removed from zone", serverPlayer.Entity.DisplayName);

        // Notify other clients via pending broadcasts
        var playerLeft = new PlayerLeftZone(serverPlayer.Entity);
        _pendingBroadcasts.Enqueue(playerLeft);
    }

    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e) => _incomingMessages.Enqueue(e);

    private void OnNetworkError(object? sender, NetworkErrorEventArgs e)
    {
        if (e.ClientId.HasValue)
            _log.Error("Network error for client {ClientId} in {Context}:  {Error}",
                e.ClientId, e.Context, e.Exception.Message);
        else
            _log.Error("Server network error in {Context}: {Error}",
                e.Context, e.Exception.Message);
    }

    /// <summary>
    ///     Mark an entity as dirty (changed) for delta broadcasts.
    ///     Uses PersistentId for stability across zone transfers.
    /// </summary>
    /// <param name="persistentId">The PersistentId of the entity.</param>
    public void MarkEntityDirty(Guid persistentId) => _dirtyEntities.Add(persistentId);

    /// <summary>
    ///     Mark an entity as dirty (changed) for delta broadcasts.
    /// </summary>
    /// <param name="entity">The entity to mark as dirty.</param>
    public void MarkEntityDirty(IEntity entity) => _dirtyEntities.Add(entity.PersistentId);

    /// <summary>
    ///     Queue an event broadcast to be sent in the next OutputPhase.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    public void QueueBroadcast(INetworkMessage message) => _pendingBroadcasts.Enqueue(message);
}
