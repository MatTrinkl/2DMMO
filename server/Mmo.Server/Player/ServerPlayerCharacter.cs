using Mmo.Server.Connections;
using Mmo.Server.Entities;
using Mmo.Shared.Account.Enums;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Constants;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Groups.Enums;

namespace Mmo.Server.Player;

/// <summary>
///     Server-side wrapper for PlayerEntity with connection info.
/// </summary>
public class ServerPlayerCharacter(CharacterEntity entity, ClientConnection connection)
{
    /// <summary>Rate-Limiting Tracker..</summary>
    private readonly Dictionary<string, List<DateTime>> _rateLimits = new();

    /// <summary>
    ///     The shared PlayerEntity (position, stats, etc.).
    /// </summary>
    public CharacterEntity Entity { get; } = entity;

    /// <summary>
    ///     The ClientConnection ID for this player.
    /// </summary>
    public ClientConnection Connection { get; } = connection;

    /// <summary>
    ///     When this player connected.
    /// </summary>
    public DateTimeOffset ConnectedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Last received input timestamp (for timeout detection).
    /// </summary>
    public DateTimeOffset LastActivity { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Last received input timestamp (for timeout detection).
    /// </summary>
    public DateTimeOffset LastHeartbeat { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The runtime ID of the player. This includes the LocalId in the Zone, the ZoneId and the ShardID. All combined are
    ///     the
    ///     <see cref="EntityIdentity.GlobalKey" />.
    /// </summary>
    public EntityIdentity RuntimeId => Entity.RuntimeId;

    /// <summary>
    ///     The name of the player.
    /// </summary>
    public string Name => Entity.DisplayName;

    /// <summary>Account-ID (set after Login).</summary>
    public Guid? AccountId { get; init; }

    /// <summary>Session-Token for Reconnection.</summary>
    public Guid SessionToken { get; set; } = IdRegistry.Instance.GeneratePersistentId();

    /// <summary>Is the player fully authenticated?</summary>
    public bool IsAuthenticated => AccountId != null;

    /// <summary>The ID of the party (null when character is not in a party).</summary>
    public Guid? PartyId { get; set; } = null;

    /// <summary>The ID of the guild (null when character is not in a guild).</summary>
    public Guid? GuildId { get; set; } = null;

    /// <summary>When true then the Player has set "Do not disturb".</summary>
    public bool IsDnd { get; set; }

    /// <summary>When true then the Player is muted.</summary>
    public bool IsMuted { get; set; } = false;

    /// <summary>Account-Flags (Premium, GM, etc.).</summary>
    public AccountFlags AccountFlags { get; set; } = AccountFlags.None;

    /// <summary>Is the account of this character a GameMaster.</summary>
    public bool IsGameMaster => AccountFlags.HasFlag(AccountFlags.GameMaster);

    /// <summary>Role in the current group (Tank, Healer, DPS).</summary>
    public PlayerRole GroupRole { get; set; } = PlayerRole.None;

    /// <summary>
    ///     Current latency in milliseconds (Round-Trip-Time).
    ///     Is calculated from the heartbeat response.
    /// </summary>
    public int LatencyMs { get; private set; }

    /// <summary>
    ///     Smoothed latency (Moving Average).
    ///     Less jumpy as LatencyMs.
    /// </summary>
    public int SmoothedLatencyMs { get; private set; }

    /// <summary>
    ///     Checks if the player is Away from Keyboard. (no activity for x seconds).
    /// </summary>
    public bool IsAfk =>
        (DateTimeOffset.UtcNow - LastActivity).TotalSeconds > SharedConstants.TimeToAfkInSeconds; // 5 Minuten

    /// <summary>
    ///     Checks if the connection to the client is dead. (no Heartbeat since X seconds).
    /// </summary>
    public bool IsConnectionDead => (DateTimeOffset.UtcNow - LastHeartbeat).TotalSeconds >
                                    SharedConstants.TimeToConnectionDeadInSeconds;

    /// <summary>
    ///     Checks if the player have a limited rate for an action.
    ///     Todo: This is only a placeholder and not fully implemented.
    /// </summary>
    /// <param name="action">The action to check.</param>
    /// <param name="maxPerMinute">The maximum of actions per minute allowed.</param>
    /// <returns>True is the rate is limited.</returns>
    public bool IsRateLimited(string action, int maxPerMinute)
    {
        if (!_rateLimits.TryGetValue(action, out List<DateTime>? times)) return false;
        int recent = times.Count(t => t > DateTime.UtcNow.AddMinutes(-1));
        return recent >= maxPerMinute;
    }

    /// <summary>
    ///     This is tracking the action of a player (for rate limits).
    ///     Todo: not implemented yet.
    /// </summary>
    /// <param name="action"></param>
    public void TrackAction(string action)
    {
        if (!_rateLimits.ContainsKey(action))
            _rateLimits[action] = new List<DateTime>();
        _rateLimits[action].Add(DateTime.UtcNow);
    }

    /// <summary>
    ///     Updates the latency.
    /// </summary>
    public void UpdateLatency(int newLatencyMs)
    {
        LatencyMs = newLatencyMs;

        // Exponential Moving Average (Smoothing)
        // SmoothedLatency = 0.8 * SmoothedLatency + 0.2 * NewLatency
        SmoothedLatencyMs = SmoothedLatencyMs == 0
            ? newLatencyMs
            : (int)(SmoothedLatencyMs * 0.8f + newLatencyMs * 0.2f);

        LastActivity = DateTimeOffset.UtcNow;
    }
}
