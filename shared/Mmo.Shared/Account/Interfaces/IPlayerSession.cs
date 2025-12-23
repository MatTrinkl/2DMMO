using Mmo.Shared.Account.Enums;
using Mmo.Shared.Character.Interfaces;

namespace Mmo.Shared.Account.Interfaces;

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
    ///     The currently active character. Null if none selected.
    /// </summary>
    ICharacterEntity? Character { get; }

    /// <summary>
    ///     Does the player have an active character?
    /// </summary>
    bool HasCharacter => Character != null;

    // ════════════════════════════════════════════════════════════════
    // SOCIAL INFO
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    ///     ID of the group/party (null if not in group).
    /// </summary>
    Guid? PartyId { get; set; }

    /// <summary>
    ///     ID of the guild (null if not in guild).
    /// </summary>
    Guid? GuildId { get; set; }

    /// <summary>
    ///     ID of the raid (null if not in raid).
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
    string? AfkMessage { get; set; }

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

    /// <summary>
    ///     Aktualisiert den LastActivityAt Timestamp.
    /// </summary>
    void UpdateLastActivity();

    /// <summary>
    ///     Setzt den aktiven Charakter.
    /// </summary>
    void SetCharacter(ICharacterEntity character);

    /// <summary>
    ///     Entfernt den aktiven Charakter (Logout to Character Select).
    /// </summary>
    void ClearCharacter();

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
    T? Get<T>(string key);

    /// <summary>
    ///     Checks if a value exists.
    /// </summary>
    bool Has(string key);

    /// <summary>
    ///     Removes a value from the session.
    /// </summary>
    /// </summary>
    void Remove(string key);
}
