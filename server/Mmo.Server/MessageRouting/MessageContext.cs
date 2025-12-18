using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.Entities;
using Mmo.Server.GameLoop;
using Mmo.Server.Messages;
using Mmo.Server.Zones;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.System;
using Mmo.Shared.Network;
using Mmo.Shared.Records;

namespace Mmo.Server.Networking;

/// <summary>
///     Server-Implementation von IMessageContext.
///     Wraps ServerPlayer und ClientConnection und stellt alle Informationen
///     bereit die ein Handler braucht.
///     WICHTIG:
///     - Send-Methoden queuen Messages für die Output-Phase
///     - Es wird NICHTS sofort gesendet!
///     - Das tatsächliche Senden passiert im GameServer während der Output-Phase
///     Zusätzlich bietet diese Klasse Server-only Erweiterungen wie
///     Broadcast-Methoden die nicht im Shared-Interface sind.
/// </summary>
public sealed class MessageContext : IMessageContext
{
    private readonly GameServer _gameServer;
    private readonly ServerPlayerCharacter? _serverPlayer;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public MessageContext(
        ClientConnection connection,
        GameServer gameServer,
        ZoneManager zoneManager,
        IServiceProvider services)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
        ZoneManager = zoneManager ?? throw new ArgumentNullException(nameof(zoneManager));
        Services = services ?? throw new ArgumentNullException(nameof(services));

        // Hole ServerPlayer aus ZoneManager (falls bereits eingeloggt)
        ZoneManager.TryGetPlayerByConnectionId(connection.Id, out _serverPlayer);

        // Wrap ServerPlayer in IPlayerInfo (Shared-kompatibel)
        if (_serverPlayer != null) PlayerInfo = new ServerPlayerInfo(_serverPlayer);
    }

    // ═══════════════════════════════════════════════════════════════
    // SERVER-ONLY:  Direct Access (nicht in IMessageContext!)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Direkter Zugriff auf den ServerPlayer.
    ///     NUR FÜR SERVER-INTERNE VERWENDUNG!
    ///     Nicht über IMessageContext exponiert.
    /// </summary>
    internal ServerPlayerCharacter? ServerPlayer => _serverPlayer;

    /// <summary>
    ///     Direkter Zugriff auf die ClientConnection.
    ///     NUR FÜR SERVER-INTERNE VERWENDUNG!
    /// </summary>
    internal ClientConnection Connection { get; }

    /// <summary>
    ///     Direkter Zugriff auf den ZoneManager.
    ///     NUR FÜR SERVER-INTERNE VERWENDUNG!
    /// </summary>
    internal ZoneManager ZoneManager { get; }

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - CONNECTION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Guid ConnectionId => Connection.Id;

    /// <inheritdoc />
    public string RemoteEndPoint => Connection.RemoteEndPoint;

    /// <inheritdoc />
    public DateTimeOffset ConnectedAt => _serverPlayer?.ConnectedAt ?? Connection.ConnectedAt;

    /// <inheritdoc />
    public DateTimeOffset LastActivity => _serverPlayer?.LastActivity ?? DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public int LatencyMs => _serverPlayer?.LatencyMs ?? 0;

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - AUTHENTICATION STATE
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsAuthenticated => _serverPlayer?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public bool HasCharacter => _serverPlayer?.Entity != null;

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - PLAYER INFO
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IPlayerInfo? PlayerInfo { get; }

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - PERMISSIONS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsGameMaster => _serverPlayer?.AccountFlags.HasFlag(AccountFlags.GameMaster) ?? false;

    /// <inheritdoc />
    public bool IsAdmin => _serverPlayer?.AccountFlags.HasFlag(AccountFlags.Admin) ?? false;

    /// <inheritdoc />
    public bool IsModerator => _serverPlayer?.AccountFlags.HasFlag(AccountFlags.Moderator) ?? false;

    /// <inheritdoc />
    public bool IsPremium => _serverPlayer?.AccountFlags.HasFlag(AccountFlags.Premium) ?? false;

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - SOCIAL STATE
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsMuted => _serverPlayer?.IsMuted ?? false;

    /// <inheritdoc />
    public bool IsAfk => _serverPlayer?.IsAfk ?? false;

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - SERVICES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IServiceProvider Services { get; }

    /// <inheritdoc />
    public T GetService<T>() where T : notnull => Services.GetRequiredService<T>();

    /// <inheritdoc />
    public T? GetOptionalService<T>() where T : class => Services.GetService<T>();

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - SEND METHODS (QUEUED!)
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void Send(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.ToClient(Connection, message);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <inheritdoc />
    public void SendError(string code, string message)
    {
        if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));

        Send(new ErrorMessage
        {
            Code = code,
            Message = message ?? string.Empty
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - CONNECTION CONTROL
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void Disconnect(string? reason = null)
    {
        Send(new Disconnect
        {
            Reason = DisconnectReason.ServerShutdown,
            Message = reason
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // SERVER-ONLY: BROADCAST METHODS (QUEUED!)
    //
    // Diese Methoden sind NICHT im IMessageContext Interface,
    // da sie Server-spezifisch sind.
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Queued eine Nachricht für alle Spieler in der aktuellen Zone.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    public void BroadcastToZone(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer == null) return;

        var outgoing = OutgoingMessage.BroadcastToZone(message, _serverPlayer.RuntimeId.ZoneId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Spieler in einer bestimmten Zone.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    /// <param name="zoneId">Die Ziel-Zone. </param>
    public void BroadcastToZone(INetworkMessage message, ushort zoneId)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToZone(message, zoneId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Spieler in der aktuellen Zone,
    ///     außer an den Sender selbst.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    public void BroadcastToZoneExceptSelf(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer == null) return;

        var outgoing = OutgoingMessage.BroadcastToZoneExcept(
            message,
            _serverPlayer.RuntimeId.ZoneId,
            Connection.Id
        );
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Spieler in Reichweite.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    /// <param name="radius">Maximale Distanz. </param>
    public void BroadcastToNearby(INetworkMessage message, float radius)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer == null) return;

        var outgoing = OutgoingMessage.BroadcastToNearby(
            message,
            _serverPlayer.RuntimeId.ZoneId,
            _serverPlayer.Entity.Position,
            radius,
            Connection.Id
        );
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Spieler in Reichweite (inklusive sich selbst).
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    /// <param name="radius">Maximale Distanz.</param>
    public void BroadcastToNearbyIncludingSelf(INetworkMessage message, float radius)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer == null) return;

        var outgoing = OutgoingMessage.BroadcastToNearby(
            message,
            _serverPlayer.RuntimeId.ZoneId,
            _serverPlayer.Entity.Position,
            radius,
            null // Keinen ausschließen
        );
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Party-Mitglieder.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    public void BroadcastToParty(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer?.PartyId == null) return;

        var outgoing = OutgoingMessage.BroadcastToParty(message, _serverPlayer.PartyId.Value);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Party-Mitglieder außer dem Sender.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    public void BroadcastToPartyExceptSelf(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer?.PartyId == null) return;

        var outgoing = OutgoingMessage.BroadcastToPartyExcept(
            message,
            _serverPlayer.PartyId.Value,
            Connection.Id
        );
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Guild-Mitglieder.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    public void BroadcastToGuild(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer?.GuildId == null) return;

        var outgoing = OutgoingMessage.BroadcastToGuild(message, _serverPlayer.GuildId.Value);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Guild-Mitglieder außer dem Sender.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    public void BroadcastToGuildExceptSelf(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer?.GuildId == null) return;

        var outgoing = OutgoingMessage.BroadcastToGuildExcept(
            message,
            _serverPlayer.GuildId.Value,
            Connection.Id
        );
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für alle Spieler auf dem Server.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    public void BroadcastToAll(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToAll(message);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queued eine Nachricht für einen bestimmten Spieler (by PersistentId).
    /// </summary>
    /// <param name="targetId">PersistentId des Ziel-Spielers.</param>
    /// <param name="message">Die zu sendende Nachricht. </param>
    /// <returns>True wenn der Spieler gefunden wurde. </returns>
    public bool SendToPlayer(Guid targetId, INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        if (ZoneManager.TryGetPlayerByPersistentId(targetId, out ServerPlayerCharacter? targetPlayer))
        {
            var outgoing = OutgoingMessage.ToClient(targetPlayer.Connection, message);
            _gameServer.QueueOutgoingMessage(outgoing);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Queued eine Nachricht für einen bestimmten Spieler (by Name).
    /// </summary>
    /// <param name="targetName">Name des Ziel-Spielers.</param>
    /// <param name="message">Die zu sendende Nachricht.</param>
    /// <returns>True wenn der Spieler gefunden wurde. </returns>
    public bool SendToPlayer(string targetName, INetworkMessage message)
    {
        if (string.IsNullOrEmpty(targetName)) return false;
        if (message == null) throw new ArgumentNullException(nameof(message));

        ServerPlayerCharacter? targetPlayer = ZoneManager
            .GetAllServerPlayers()
            .FirstOrDefault(p => p.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

        if (targetPlayer != null)
        {
            var outgoing = OutgoingMessage.ToClient(targetPlayer.Connection, message);
            _gameServer.QueueOutgoingMessage(outgoing);
            return true;
        }

        return false;
    }

    // ═══════════════════════════════════════════════════════════════
    // SERVER-ONLY: UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Prüft ob ein Spieler online ist (by PersistentId).
    /// </summary>
    public bool IsPlayerOnline(Guid playerId) => ZoneManager.TryGetPlayerByPersistentId(playerId, out _);

    /// <summary>
    ///     Prüft ob ein Spieler online ist (by Name).
    /// </summary>
    public bool IsPlayerOnline(string playerName)
    {
        if (string.IsNullOrEmpty(playerName)) return false;

        return ZoneManager
            .GetAllServerPlayers()
            .Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Holt die Zone-ID eines Spielers (by PersistentId).
    /// </summary>
    /// <returns>Zone-ID oder null wenn nicht gefunden.</returns>
    public ushort? GetPlayerZone(Guid playerId)
    {
        if (ZoneManager.TryGetPlayerByPersistentId(playerId, out ServerPlayerCharacter? player))
            return player.RuntimeId.ZoneId;

        return null;
    }

    /// <summary>
    ///     Prüft ob der aktuelle Spieler in der gleichen Zone wie ein anderer ist.
    /// </summary>
    public bool IsInSameZone(Guid otherPlayerId)
    {
        if (_serverPlayer == null) return false;

        ushort? otherZone = GetPlayerZone(otherPlayerId);
        return otherZone.HasValue && otherZone.Value == _serverPlayer.RuntimeId.ZoneId;
    }

    /// <summary>
    ///     Berechnet die Distanz zu einem anderen Spieler.
    /// </summary>
    /// <returns>Distanz oder null wenn nicht in gleicher Zone.</returns>
    public float? GetDistanceToPlayer(Guid otherPlayerId)
    {
        if (_serverPlayer == null) return null;

        if (ZoneManager.TryGetPlayerByPersistentId(otherPlayerId, out ServerPlayerCharacter? otherPlayer))
        {
            if (otherPlayer.RuntimeId.ZoneId != _serverPlayer.RuntimeId.ZoneId)
                return null;

            return CalculateDistance(_serverPlayer.Entity.Position, otherPlayer.Entity.Position);
        }

        return null;
    }

    /// <summary>
    ///     Prüft ob ein Spieler in Reichweite ist.
    /// </summary>
    public bool IsPlayerInRange(Guid otherPlayerId, float range)
    {
        float? distance = GetDistanceToPlayer(otherPlayerId);
        return distance.HasValue && distance.Value <= range;
    }

    // ═══════════════════════════════════════════════════════════════
    // ASYNC TASK SUPPORT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Startet einen async Task und queued das Result für den nächsten Tick.
    ///     Der Task läuft im Hintergrund, blockiert NICHT den Game-Loop.
    ///     Wenn der Task fertig ist, wird der Callback im Game-Loop ausgeführt.
    /// </summary>
    /// <typeparam name="T">Result-Type des Tasks</typeparam>
    /// <param name="task">Der async Task</param>
    /// <param name="onCompleted">Callback wenn Task fertig (wird im Game-Loop ausgeführt)</param>
    /// <param name="onError">Optional:  Callback bei Fehler</param>
    public void RunAsync<T>(
        Task<T> task,
        Action<MessageContext, T> onCompleted,
        Action<MessageContext, Exception>? onError = null)
    {
        Guid connectionId = ConnectionId;

        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                // Fehler-Callback queuen
                if (onError != null)
                    _gameServer.QueueCompletion(connectionId, ctx => onError(ctx, t.Exception!.InnerException!));
                else
                    // Default:  Error-Message senden
                    _gameServer.QueueCompletion(connectionId, ctx =>
                        ctx.SendError("INTERNAL_ERROR", "An error occurred"));
            }
            else if (t.IsCompletedSuccessfully)
            {
                // Success-Callback queuen
                _gameServer.QueueCompletion(connectionId, ctx => onCompleted(ctx, t.Result));
            }
            // Cancelled wird ignoriert
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    /// <summary>
    ///     Vereinfachte Version ohne Result.
    /// </summary>
    public void RunAsync(
        Task task,
        Action<MessageContext> onCompleted,
        Action<MessageContext, Exception>? onError = null)
    {
        Guid connectionId = ConnectionId;

        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                if (onError != null)
                    _gameServer.QueueCompletion(connectionId, ctx => onError(ctx, t.Exception!.InnerException!));
                else
                    _gameServer.QueueCompletion(connectionId, ctx =>
                        ctx.SendError("INTERNAL_ERROR", "An error occurred"));
            }
            else if (t.IsCompletedSuccessfully)
            {
                _gameServer.QueueCompletion(connectionId, ctx => onCompleted(ctx));
            }
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static float CalculateDistance(Position a, Position b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
