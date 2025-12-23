using Mmo.Shared.Core.Records;

namespace Mmo.Shared.Account.Interfaces;

/// <summary>
///     Abstrakte Spieler-Informationen für Message-Handling.
///     WICHTIG: Enthält keine Server-spezifischen Typen!
///     Ist ein Read-Only View auf die Spieler-Daten.
/// </summary>
public interface IPlayerInfo
{
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Persistent character ID (from database).</summary>
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

    /// <summary>Party ID (null if not in party).</summary>
    Guid? PartyId { get; }

    /// <summary>Guild ID (null if not in guild).</summary>
    Guid? GuildId { get; }

    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Is PvP flag active?</summary>
    bool IsPvpFlagged { get; }

    /// <summary>Is the player in combat?</summary>
    bool IsInCombat { get; }

    /// <summary>Is the player muted?</summary>
    bool IsMuted { get; }

    /// <summary>Is the player AFK?</summary>
    bool IsAfk { get; }
}
