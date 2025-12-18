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
///     Server implementation of IMessageContext.
///     Wraps ServerPlayer and ClientConnection and provides all information
///     that a handler needs.
///     IMPORTANT:
///     - Send methods queue messages for the Output phase
///     - Nothing is sent immediately!
///     - Actual sending happens in GameServer during the Output phase
///     Additionally, this class provides server-only extensions like
///     broadcast methods that are not in the Shared interface.
/// </summary>
public sealed class MessageContext : IMessageContext
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly GameServer _gameServer;
    private readonly ServerPlayerCharacter? _serverPlayer;

    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES - Server-only (not in IMessageContext!)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Direct access to the ServerPlayer.
    ///     FOR SERVER-INTERNAL USE ONLY!
    ///     Not exposed via IMessageContext.
    /// </summary>
    internal ServerPlayerCharacter? ServerPlayer => _serverPlayer;

    /// <summary>
    ///     Direct access to the ClientConnection.
    ///     FOR SERVER-INTERNAL USE ONLY!
    /// </summary>
    internal ClientConnection Connection { get; }

    /// <summary>
    ///     Direct access to the ZoneManager.
    ///     FOR SERVER-INTERNAL USE ONLY!
    /// </summary>
    internal ZoneManager ZoneManager { get; }

    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES - IMessageContext - Connection
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
    // IMessageContext - Authentication State
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsAuthenticated => _serverPlayer?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public bool HasCharacter => _serverPlayer?.Entity != null;

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - Player Info
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IPlayerInfo? PlayerInfo { get; }

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - Permissions
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
    // IMessageContext - Social State
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsMuted => _serverPlayer?.IsMuted ?? false;

    /// <inheritdoc />
    public bool IsAfk => _serverPlayer?.IsAfk ?? false;

    // ═══════════════════════════════════════════════════════════════
    // IMessageContext - Services
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IServiceProvider Services { get; }

    /// <inheritdoc />
    public T GetService<T>() where T : notnull => Services.GetRequiredService<T>();

    /// <inheritdoc />
    public T? GetOptionalService<T>() where T : class => Services.GetService<T>();

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

        // Get ServerPlayer from ZoneManager (if already logged in)
        ZoneManager.TryGetPlayerByConnectionId(connection.Id, out _serverPlayer);

        // Wrap ServerPlayer in IPlayerInfo (Shared-compatible)
        if (_serverPlayer != null) PlayerInfo = new ServerPlayerInfo(_serverPlayer);
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS - IMessageContext - Send Methods (Queued)
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
    // IMessageContext - Connection CONTROL
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
    ///     Queues a message for all players in the current zone.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void BroadcastToZone(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer == null) return;

        var outgoing = OutgoingMessage.BroadcastToZone(message, _serverPlayer.RuntimeId.ZoneId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queues a message for all players in a specific zone.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="zoneId">The target zone. </param>
    public void BroadcastToZone(INetworkMessage message, ushort zoneId)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToZone(message, zoneId);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queues a message for all players in the current zone,
    ///     except the sender.
    /// </summary>
    /// <param name="message">The message to send.</param>
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
    ///     Queues a message for all players in range.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="radius">Maximum distance. </param>
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
    ///     Queues a message for all players in range (including self).
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="radius">Maximum distance.</param>
    public void BroadcastToNearbyIncludingSelf(INetworkMessage message, float radius)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer == null) return;

        var outgoing = OutgoingMessage.BroadcastToNearby(
            message,
            _serverPlayer.RuntimeId.ZoneId,
            _serverPlayer.Entity.Position,
            radius,
            null // Don't exclude anyone
        );
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queues a message for all party members.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void BroadcastToParty(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer?.PartyId == null) return;

        var outgoing = OutgoingMessage.BroadcastToParty(message, _serverPlayer.PartyId.Value);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queues a message for all party members except the sender.
    /// </summary>
    /// <param name="message">The message to send.</param>
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
    ///     Queues a message for all guild members.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void BroadcastToGuild(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (_serverPlayer?.GuildId == null) return;

        var outgoing = OutgoingMessage.BroadcastToGuild(message, _serverPlayer.GuildId.Value);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queues a message for all guild members except the sender.
    /// </summary>
    /// <param name="message">The message to send.</param>
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
    ///     Queues a message for all players on the server.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void BroadcastToAll(INetworkMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var outgoing = OutgoingMessage.BroadcastToAll(message);
        _gameServer.QueueOutgoingMessage(outgoing);
    }

    /// <summary>
    ///     Queues a message for a specific player (by PersistentId).
    /// </summary>
    /// <param name="targetId">PersistentId of the target player.</param>
    /// <param name="message">The message to send. </param>
    /// <returns>True if the player was found. </returns>
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
    ///     Queues a message for a specific player (by name).
    /// </summary>
    /// <param name="targetName">Name of the target player.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>True if the player was found. </returns>
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
    ///     Checks if a player is online (by PersistentId).
    /// </summary>
    public bool IsPlayerOnline(Guid playerId) => ZoneManager.TryGetPlayerByPersistentId(playerId, out _);

    /// <summary>
    ///     Checks if a player is online (by name).
    /// </summary>
    public bool IsPlayerOnline(string playerName)
    {
        if (string.IsNullOrEmpty(playerName)) return false;

        return ZoneManager
            .GetAllServerPlayers()
            .Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Gets the zone ID of a player (by PersistentId).
    /// </summary>
    /// <returns>Zone ID or null if not found.</returns>
    public ushort? GetPlayerZone(Guid playerId)
    {
        if (ZoneManager.TryGetPlayerByPersistentId(playerId, out ServerPlayerCharacter? player))
            return player.RuntimeId.ZoneId;

        return null;
    }

    /// <summary>
    ///     Checks if the current player is in the same zone as another.
    /// </summary>
    public bool IsInSameZone(Guid otherPlayerId)
    {
        if (_serverPlayer == null) return false;

        ushort? otherZone = GetPlayerZone(otherPlayerId);
        return otherZone.HasValue && otherZone.Value == _serverPlayer.RuntimeId.ZoneId;
    }

    /// <summary>
    ///     Calculates the distance to another player.
    /// </summary>
    /// <returns>Distance or null if not in the same zone.</returns>
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
    ///     Checks if a player is in range.
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
    ///     Starts an async task and queues the result for the next tick.
    ///     The task runs in the background and does NOT block the Game Loop.
    ///     When the task completes, the callback is executed in the Game Loop.
    /// </summary>
    /// <typeparam name="T">Result type of the task</typeparam>
    /// <param name="task">The async task</param>
    /// <param name="onCompleted">Callback when task completes (executed in Game Loop)</param>
    /// <param name="onError">Optional: Callback on error</param>
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
                // Queue error callback
                if (onError != null)
                    _gameServer.QueueCompletion(connectionId, ctx => onError(ctx, t.Exception!.InnerException!));
                else
                    // Default: Send error message
                    _gameServer.QueueCompletion(connectionId, ctx =>
                        ctx.SendError("INTERNAL_ERROR", "An error occurred"));
            }
            else if (t.IsCompletedSuccessfully)
            {
                // Queue success callback
                _gameServer.QueueCompletion(connectionId, ctx => onCompleted(ctx, t.Result));
            }
            // Cancelled is ignored
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    /// <summary>
    ///     Simplified version without result.
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
