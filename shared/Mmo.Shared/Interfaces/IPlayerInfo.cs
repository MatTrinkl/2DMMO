using Mmo.Shared.Records;

namespace Mmo.Shared.Interfaces;

/// <summary>
///     Abstrakte Spieler-Informationen für Message-Handling.
///
///     WICHTIG: Enthält keine Server-spezifischen Typen!
///     Ist ein Read-Only View auf die Spieler-Daten.
/// </summary>
public interface IPlayerInfo
{
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Persistente Charakter-ID (aus Datenbank).</summary>
    Guid PersistentId { get; }

    /// <summary>Account-ID.</summary>
    Guid AccountId { get; }

    /// <summary>Charakter-Name.</summary>
    string Name { get; }

    // ═══════════════════════════════════════════════════════════════
    // LOCATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Aktuelle Zone-ID.</summary>
    ushort ZoneId { get; }

    /// <summary>Aktueller Shard-ID.</summary>
    ushort ShardId { get; }

    /// <summary>Aktuelle Position.</summary>
    Position Position { get; }

    // ═══════════════════════════════════════════════════════════════
    // CHARACTER INFO
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Charakter-Level.</summary>
    int Level { get; }

    // ═══════════════════════════════════════════════════════════════
    // SOCIAL
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Party-ID (null wenn nicht in Party).</summary>
    Guid? PartyId { get; }

    /// <summary>Guild-ID (null wenn nicht in Guild).</summary>
    Guid? GuildId { get; }

    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Ist PvP-Flag aktiv?</summary>
    bool IsPvpFlagged { get; }

    /// <summary>Ist der Spieler im Kampf?</summary>
    bool IsInCombat { get; }

    /// <summary>Ist der Spieler gemutet?</summary>
    bool IsMuted { get; }

    /// <summary>Ist der Spieler AFK?</summary>
    bool IsAfk { get; }
}
