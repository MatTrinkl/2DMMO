using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Server.Entities;

/// <summary>
///     Wrapper um ServerPlayer der IPlayerInfo implementiert.
///     Erlaubt Zugriff auf Spieler-Daten ohne Server-spezifische Typen
///     in das Shared-Projekt zu exponieren.
/// </summary>
internal sealed class ServerPlayerInfo(ServerPlayerCharacter playerCharacter) : IPlayerInfo
{
    private readonly ServerPlayerCharacter _playerCharacter =
        playerCharacter ?? throw new ArgumentNullException(nameof(playerCharacter));

    // ═══ Identity ═══
    public Guid PersistentId => _playerCharacter.Entity.PersistentId;
    public Guid AccountId => _playerCharacter.AccountId ?? Guid.Empty;
    public string Name => _playerCharacter.Name;

    // ═══ Location ═══
    public ushort ZoneId => _playerCharacter.RuntimeId.ZoneId;
    public ushort ShardId => _playerCharacter.RuntimeId.ShardId;
    public Position Position => _playerCharacter.Entity.Position;

    // ═══ Character Info ═══
    public int Level => _playerCharacter.Entity.Level;

    // ═══ Social ═══
    public Guid? PartyId => _playerCharacter.PartyId;
    public Guid? GuildId => _playerCharacter.GuildId;

    // ═══ State ═══
    public bool IsPvpFlagged => _playerCharacter.Entity.IsPvpFlagged;
    public bool IsInCombat => _playerCharacter.Entity.IsInCombat;
    public bool IsMuted => _playerCharacter.IsMuted;
    public bool IsAfk => _playerCharacter.IsAfk;
}
