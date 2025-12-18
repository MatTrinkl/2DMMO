using Mmo.Shared.Enums;

namespace Mmo.Shared. Interfaces;

/// <summary>
///     Die Session eines eingeloggten Spielers.
///     Enthält Account-Daten, aktiven Charakter und Session-Status.
/// </summary>
public interface IPlayerSession
{
    // ════════════════════════════════════════════════════════════════
    // SESSION INFO
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    ///     Eindeutige Session-ID (für Reconnect).
    /// </summary>
    Guid SessionId { get; }

    /// <summary>
    ///     Zeitpunkt wann die Session erstellt wurde (Login).
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    ///     Zeitpunkt der letzten Aktivität (für Timeout).
    /// </summary>
    DateTime LastActivityAt { get; }

    /// <summary>
    ///     Aktualisiert den LastActivityAt Timestamp.
    /// </summary>
    void UpdateLastActivity();

    // ════════════════════════════════════════════════════════════════
    // ACCOUNT INFO
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    ///     Account-ID aus der Datenbank.
    /// </summary>
    Guid AccountId { get; }

    /// <summary>
    ///     Account-Email (für Logging, nicht an Client senden! ).
    /// </summary>
    string AccountEmail { get; }

    /// <summary>
    ///     Account-Flags (Premium, GM, Banned, etc.).
    /// </summary>
    AccountFlags AccountFlags { get; }

    /// <summary>
    ///     Ist dies ein Premium/VIP Account?
    /// </summary>
    bool IsPremium => AccountFlags.HasFlag(AccountFlags.Premium);

    /// <summary>
    ///     Ist dies ein Game Master Account?
    /// </summary>
    bool IsGameMaster => AccountFlags.HasFlag(AccountFlags.GameMaster);

    /// <summary>
    ///     Ist dies ein Admin Account?
    /// </summary>
    bool IsAdmin => AccountFlags.HasFlag(AccountFlags.Admin);

    // ════════════════════════════════════════════════════════════════
    // CHARACTER INFO
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    ///     Der aktuell aktive Charakter.  Null wenn keiner ausgewählt.
    /// </summary>
    ICharacterEntity? Character { get; }

    /// <summary>
    ///     Hat der Spieler einen aktiven Charakter?
    /// </summary>
    bool HasCharacter => Character != null;

    /// <summary>
    ///     Setzt den aktiven Charakter.
    /// </summary>
    void SetCharacter(ICharacterEntity character);

    /// <summary>
    ///     Entfernt den aktiven Charakter (Logout to Character Select).
    /// </summary>
    void ClearCharacter();

    // ════════════════════════════════════════════════════════════════
    // SOCIAL INFO
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    ///     ID der Gruppe/Party (null wenn nicht in Gruppe).
    /// </summary>
    Guid? PartyId { get; set; }

    /// <summary>
    ///     ID der Gilde (null wenn nicht in Gilde).
    /// </summary>
    Guid? GuildId { get; set; }

    /// <summary>
    ///     ID des Raids (null wenn nicht in Raid).
    /// </summary>
    Guid? RaidId { get; set; }

    /// <summary>
    ///     Ist der Spieler in einer Gruppe?
    /// </summary>
    bool IsInParty => PartyId != null;

    /// <summary>
    ///     Ist der Spieler in einer Gilde?
    /// </summary>
    bool IsInGuild => GuildId != null;

    /// <summary>
    ///     Ist der Spieler in einem Raid?
    /// </summary>
    bool IsInRaid => RaidId != null;

    // ════════════════════════════════════════════════════════════════
    // STATUS FLAGS
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    ///     Ist der Spieler AFK?
    /// </summary>
    bool IsAfk { get; set; }

    /// <summary>
    ///     AFK-Nachricht.
    /// </summary>
    string?  AfkMessage { get; set; }

    /// <summary>
    ///     Ist der Spieler auf DND (Do Not Disturb)?
    /// </summary>
    bool IsDnd { get; set; }

    /// <summary>
    ///     DND-Nachricht.
    /// </summary>
    string? DndMessage { get; set; }

    /// <summary>
    ///     Ist der Spieler gemutet (kann nicht chatten)?
    /// </summary>
    bool IsMuted { get; set; }

    /// <summary>
    ///     Bis wann ist der Spieler gemutet?
    /// </summary>
    DateTime? MutedUntil { get; set; }

    // ════════════════════════════════════════════════════════════════
    // SESSION DATA (Key-Value Store)
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    ///     Speichert einen Wert in der Session.
    /// </summary>
    void Set<T>(string key, T value);

    /// <summary>
    ///     Holt einen Wert aus der Session.
    /// </summary>
    T?  Get<T>(string key);

    /// <summary>
    ///     Prüft ob ein Wert existiert.
    /// </summary>
    bool Has(string key);

    /// <summary>
    ///     Entfernt einen Wert aus der Session.
    /// </summary>
    void Remove(string key);
}
