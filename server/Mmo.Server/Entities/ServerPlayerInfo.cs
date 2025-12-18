using Mmo.Server.Entities;
using Mmo.Shared.Interfaces;
using Mmo. Shared.Network;
using Mmo.Shared. Records;

namespace Mmo.Server.Networking;

/// <summary>
///     Wrapper um ServerPlayer der IPlayerInfo implementiert.
///
///     Erlaubt Zugriff auf Spieler-Daten ohne Server-spezifische Typen
///     in das Shared-Projekt zu exponieren.
/// </summary>
internal sealed class ServerPlayerInfo : IPlayerInfo
{
    private readonly ServerPlayer _player;

    public ServerPlayerInfo(ServerPlayer player)
    {
        _player = player ??  throw new ArgumentNullException(nameof(player));
    }

    // ═══ Identity ═══
    public Guid PersistentId => _player. Entity.PersistentId;
    public Guid AccountId => _player.AccountId ??  Guid.Empty;
    public string Name => _player.Name;

    // ═══ Location ═══
    public ushort ZoneId => _player.RuntimeId.ZoneId;
    public ushort ShardId => _player.RuntimeId.ShardId;
    public Position Position => _player.Entity.Position;

    // ═══ Character Info ═══
    public int Level => _player.Entity.Level;

    // ═══ Social ═══
    public Guid?  PartyId => _player.PartyId;
    public Guid?  GuildId => _player.GuildId;

    // ═══ State ═══
    public bool IsPvpFlagged => _player.Entity.IsPvpFlagged;
    public bool IsInCombat => _player.Entity. IsInCombat;
    public bool IsMuted => _player.IsMuted;
    public bool IsAfk => _player.IsAfk;
}
