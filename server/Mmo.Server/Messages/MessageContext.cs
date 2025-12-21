using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.Connections;
using Mmo.Server.Core;
using Mmo.Server.PlayerService;
using Mmo.Server.Zones;
using Mmo.Shared.Account.Enums;
using Mmo.Shared.Account.Interfaces;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.Messages;

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
    // IMessageContext - Connection CONTROL
    // ═══════════════════════════════════════════════════════════════

    public void Disconnect(string? reason = null)
    {
        //todo: to PlayerService
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
        //Todo: to PlayerService
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
        //Todo: to PlayerService
        if (ZoneManager.TryGetPlayerByPersistentId(playerId, out ServerPlayerCharacter? player))
            return player.RuntimeId.ZoneId;

        return null;
    }

    /// <summary>
    ///     Checks if the current player is in the same zone as another.
    /// </summary>
    public bool IsInSameZone(Guid otherPlayerId)
    {
        //Todo: to PlayerService
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
        //Todo: to PlayerService
        if (_serverPlayer == null) return null;

        if (ZoneManager.TryGetPlayerByPersistentId(otherPlayerId, out ServerPlayerCharacter? otherPlayer))
        {
            if (otherPlayer.RuntimeId.ZoneId != _serverPlayer.RuntimeId.ZoneId)
                return null;

            return _serverPlayer.Entity.Position.CalculateDistance(otherPlayer.Entity.Position);
        }

        return null;
    }

    /// <summary>
    ///     Checks if a player is in range.
    /// </summary>
    public bool IsPlayerInRange(Guid otherPlayerId, float range)
    {
        //Todo: to PlayerService
        float? distance = GetDistanceToPlayer(otherPlayerId);
        return distance.HasValue && distance.Value <= range;
    }
}
