using Mmo.Server.Networking;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;

namespace Mmo.Server.Entities;

/// <summary>
///     Server-side wrapper for PlayerEntity with connection info.
/// </summary>
public class ServerPlayer(PlayerEntity entity, ClientConnection connection)
{
    /// <summary>
    ///     The shared PlayerEntity (position, stats, etc.).
    /// </summary>
    public PlayerEntity Entity { get; } = entity;

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
    ///     The runtime ID of the player. This includes the LocalId in the Zone, the ZoneId and the ShardID. All combined are
    ///     the
    ///     <see cref="EntityIdentity.GlobalKey" />.
    /// </summary>
    public EntityIdentity RuntimeId => Entity.RuntimeId;

    /// <summary>
    ///     The name of the player.
    /// </summary>
    public string Name => Entity.DisplayName;

    /// <summary>Account-ID (nach Login gesetzt).</summary>
    public Guid?  AccountId { get; set; }

    /// <summary>Session-Token für Reconnect.</summary>
    public Guid SessionToken { get; set; } = Guid.NewGuid();

    /// <summary>Ist der Spieler vollständig authentifiziert?</summary>
    public bool IsAuthenticated => AccountId != null;

    /// <summary>Party-ID (null wenn nicht in Party).</summary>
    public Guid? PartyId { get; set; }

    /// <summary>Guild-ID (null wenn nicht in Guild).</summary>
    public Guid? GuildId { get; set; }

    /// <summary>Ist DND?</summary>
    public bool IsDnd { get; set; }

    /// <summary>Ist gemutet?</summary>
    public bool IsMuted { get; set; }

    /// <summary>Account-Flags (Premium, GM, etc.).</summary>
    public AccountFlags AccountFlags { get; set; } = AccountFlags.None;

    /// <summary>Shortcut:  Ist GM?</summary>
    public bool IsGameMaster => AccountFlags.HasFlag(AccountFlags.GameMaster);

    /// <summary>Rate-Limiting Tracker.</summary>
    private readonly Dictionary<string, List<DateTime>> _rateLimits = new();
    /// <summary>Rolle in der aktuellen Gruppe (Tank, Healer, DPS).</summary>
    public PlayerRole GroupRole { get; set; } = PlayerRole.None;

    public bool IsRateLimited(string action, int maxPerMinute)
    {
        if (! _rateLimits.TryGetValue(action, out List<DateTime>? times)) return false;
        int recent = times.Count(t => t > DateTime.UtcNow.AddMinutes(-1));
        return recent >= maxPerMinute;
    }

    public void TrackAction(string action)
    {
        if (!_rateLimits.ContainsKey(action))
            _rateLimits[action] = new List<DateTime>();
        _rateLimits[action]. Add(DateTime.UtcNow);
    }

    /// <summary>
    ///     Aktuelle Latenz in Millisekunden (Round-Trip-Time).
    ///     Wird aus Heartbeat-Responses berechnet.
    /// </summary>
    public int LatencyMs { get; set; }

    /// <summary>
    ///     Geglättete Latenz (Moving Average).
    ///     Weniger sprunghaft als LatencyMs.
    /// </summary>
    public int SmoothedLatencyMs { get; private set; }

    /// <summary>
    ///     Aktualisiert die Latenz mit Glättung.
    /// </summary>
    public void UpdateLatency(int newLatencyMs)
    {
        LatencyMs = newLatencyMs;

        // Exponential Moving Average (Glättung)
        // SmoothedLatency = 0.8 * SmoothedLatency + 0.2 * NewLatency
        if (SmoothedLatencyMs == 0)
        {
            SmoothedLatencyMs = newLatencyMs;
        }
        else
        {
            SmoothedLatencyMs = (int)(SmoothedLatencyMs * 0.8f + newLatencyMs * 0.2f);
        }

        LastActivity = DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     Prüft ob der Spieler als AFK gilt (keine Aktivität seit X Sekunden).
    /// </summary>
    public bool IsAfk => (DateTimeOffset.UtcNow - LastActivity).TotalSeconds > 300; // 5 Minuten

    /// <summary>
    ///     Prüft ob die Verbindung als "tot" gilt (kein Heartbeat seit X Sekunden).
    /// </summary>
    public bool IsConnectionDead => (DateTimeOffset. UtcNow - LastActivity).TotalSeconds > 30;
}
