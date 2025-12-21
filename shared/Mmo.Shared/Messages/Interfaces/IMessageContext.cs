using Mmo.Shared.Account.Interfaces;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Network;

/// <summary>
///     Kontext für Message-Handling.
///     Enthält alle Informationen die ein Handler braucht um eine Message zu verarbeiten.
///     WICHTIG:
///     - Keine Server-spezifischen Typen (ServerPlayer, ClientConnection, etc.)!
///     - Send-Methoden sind NICHT async!  Sie queuen nur für die Output-Phase.
///     - Diese Klasse ist im Shared-Projekt und muss auch vom Client nutzbar sein.
/// </summary>
public interface IMessageContext
{
    // ═══════════════════════════════════════════════════════════════
    // CONNECTION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Eindeutige ID dieser Verbindung.
    ///     Wird bei TCP-Connect vergeben und bleibt konstant.
    /// </summary>
    Guid ConnectionId { get; }

    /// <summary>
    ///     Remote-Endpoint (IP:Port) des Clients.
    ///     Für Logging, Rate-Limiting, IP-Bans.
    /// </summary>
    string RemoteEndPoint { get; }

    /// <summary>
    ///     Zeitpunkt wann die Verbindung hergestellt wurde.
    /// </summary>
    DateTimeOffset ConnectedAt { get; }

    /// <summary>
    ///     Zeitpunkt der letzten Aktivität (Message empfangen).
    /// </summary>
    DateTimeOffset LastActivity { get; }

    /// <summary>
    ///     Aktuelle Latenz in Millisekunden (aus Heartbeat berechnet).
    /// </summary>
    int LatencyMs { get; }

    // ═══════════════════════════════════════════════════════════════
    // AUTHENTICATION STATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Ist der Client authentifiziert (Login erfolgreich)?
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    ///     Hat der Spieler einen aktiven Charakter ausgewählt und gespawnt?
    /// </summary>
    bool HasCharacter { get; }

    // ═══════════════════════════════════════════════════════════════
    // PLAYER INFO (abstrahiert, ohne Server-Typen)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Informationen über den Spieler.
    ///     Null wenn nicht authentifiziert oder kein Charakter ausgewählt.
    /// </summary>
    IPlayerInfo? PlayerInfo { get; }

    // ═══════════════════════════════════════════════════════════════
    // SHORTCUTS (Convenience Properties)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Spieler-ID (PersistentId des Charakters).</summary>
    Guid? PlayerId => PlayerInfo?.PersistentId;

    /// <summary>Account-ID. </summary>
    Guid? AccountId => PlayerInfo?.AccountId;

    /// <summary>Charakter-Name.</summary>
    string? PlayerName => PlayerInfo?.Name;

    /// <summary>Aktuelle Zone-ID.</summary>
    ushort? ZoneId => PlayerInfo?.ZoneId;

    /// <summary>Aktueller Shard-ID.</summary>
    ushort? ShardId => PlayerInfo?.ShardId;

    /// <summary>Spieler-Level.</summary>
    int? PlayerLevel => PlayerInfo?.Level;

    // ═══════════════════════════════════════════════════════════════
    // PERMISSIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Ist der Spieler ein Game Master?</summary>
    bool IsGameMaster { get; }

    /// <summary>Ist der Spieler ein Admin? </summary>
    bool IsAdmin { get; }

    /// <summary>Ist der Spieler ein Moderator? </summary>
    bool IsModerator { get; }

    /// <summary>Hat der Spieler Premium-Status?</summary>
    bool IsPremium { get; }

    // ═══════════════════════════════════════════════════════════════
    // SOCIAL STATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Ist der Spieler in einer Party?</summary>
    bool IsInParty => PlayerInfo?.PartyId != null;

    /// <summary>Ist der Spieler in einer Guild?</summary>
    bool IsInGuild => PlayerInfo?.GuildId != null;

    /// <summary>Ist der Spieler gemutet?</summary>
    bool IsMuted { get; }

    /// <summary>Ist der Spieler AFK?</summary>
    bool IsAfk { get; }

    // ═══════════════════════════════════════════════════════════════
    // SERVICES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Zugriff auf den DI-Container.
    /// </summary>
    IServiceProvider Services { get; }

    // ═══════════════════════════════════════════════════════════════
    // SEND METHODS (QUEUED - nicht async!)
    //
    // WICHTIG: Diese Methoden senden NICHT sofort!
    // Sie fügen Messages zur Output-Queue hinzu.
    // Das tatsächliche Senden passiert in der OUTPUT PHASE des Game-Loops.
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Queued eine Nachricht für diesen Client.
    ///     Wird in der OUTPUT PHASE gesendet, NICHT sofort!
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht.</param>
    void Send(INetworkMessage message);

    /// <summary>
    ///     Queued eine Fehlermeldung für diesen Client.
    /// </summary>
    /// <param name="code">Fehler-Code (z.B. "AUTH_FAILED", "INVALID_INPUT").</param>
    /// <param name="message">Benutzerfreundliche Fehlermeldung.</param>
    void SendError(string code, string message);

    // ═══════════════════════════════════════════════════════════════
    // CONNECTION CONTROL
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Markiert die Verbindung zum Trennen.
    ///     Der Disconnect wird am Ende des aktuellen Ticks ausgeführt.
    /// </summary>
    /// <param name="reason">Optionaler Grund (wird an Client gesendet).</param>
    void Disconnect(string? reason = null);

    /// <summary>
    ///     Holt einen Service aus dem DI-Container.
    /// </summary>
    T GetService<T>() where T : notnull;

    /// <summary>
    ///     Holt einen optionalen Service aus dem DI-Container.
    /// </summary>
    T? GetOptionalService<T>() where T : class;
}
