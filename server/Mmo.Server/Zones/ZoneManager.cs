using System.Collections.Concurrent;
using Mmo.Server.PlayerService;
using Mmo.Shared.Core;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.Zones;

/// <summary>
///     Manages zones and their associated entities and players.
///     Maintains mappings between connections, players, and zones.
/// </summary>
public class ZoneManager(ushort defaultZoneId)
{
    // ONLY ServerPlayerCharacter - for Connection mapping
    // IEntity lookups go through IdRegistry!
    private readonly ConcurrentDictionary<Guid, ServerPlayerCharacter> _playersByConnectionId = new();
    private readonly ConcurrentDictionary<ushort, Zone> _zones = new();

    public ushort DefaultZoneId { get; } = defaultZoneId;

    public int ZoneCount => _zones.Count;

    // ═══════════════════════════════════════════════════════════════
    // ZONE METHODS
    // ═══════════════════════════════════════════════════════════════

    public Zone? GetZone(ushort zoneId)
    {
        if (_zones.TryGetValue(zoneId, out Zone zone))
            return zone;
        return null;
    }

    public Zone? GetDefaultZone()
    {
        if (_zones.TryGetValue(DefaultZoneId, out Zone zone))
            return zone;
        return null;
    }

    public bool ZoneExists(ushort zoneId)
        => _zones.ContainsKey(zoneId);

    public bool RegisterZone(Zone zone) => _zones.TryAdd(zone.Id, zone);

    // ═══════════════════════════════════════════════════════════════
    // ENTITY METHODS - Delegiert an IdRegistry + Zone
    // ═══════════════════════════════════════════════════════════════

    public IEntity? GetEntity(Guid persistentId)
    {
        // Lookup über IdRegistry - KEINE eigene Liste!
        IdRegistry.Instance.TryGetEntity(persistentId, out IEntity? entity);
        return entity;
    }

    public IEnumerable<IEntity> GetEntitiesInZone(ushort zoneId)
    {
        Zone? zone = GetZone(zoneId);
        if (zone == null) return [];

        // Zone only has IDs, entities come from IdRegistry
        return zone.Value.GetEntityIds()
            .Select(id => IdRegistry.Instance.TryGetEntity(id, out IEntity? e) ? e : null)
            .Where(e => e != null)!;
    }

    // ═══════════════════════════════════════════════════════════════
    // PLAYER METHODS - ServerPlayerCharacter Mapping
    // ═══════════════════════════════════════════════════════════════

    public bool TryGetPlayerByConnectionId(Guid connectionId, out ServerPlayerCharacter? player)
        => _playersByConnectionId.TryGetValue(connectionId, out player);

    public bool TryGetPlayerByPersistentId(Guid persistentId, out ServerPlayerCharacter? player)
    {
        // Über IdRegistry Connection finden, dann in unserer Map nachschauen
        if (IdRegistry.Instance.TryGetConnectionByEntity(persistentId, out Guid connectionId))
            return _playersByConnectionId.TryGetValue(connectionId, out player);
        player = null;
        return false;
    }

    public IEnumerable<ServerPlayerCharacter> GetServerPlayersInZone(ushort zoneId)
    {
        Zone? zone = GetZone(zoneId);
        if (zone == null) return [];

        return zone.Value.GetEntityIds()
            .Select(id =>
            {
                if (IdRegistry.Instance.TryGetConnectionByEntity(id, out Guid connId) &&
                    _playersByConnectionId.TryGetValue(connId, out ServerPlayerCharacter? player))
                    return player;
                return null;
            })
            .Where(p => p != null)!;
    }

    public IEnumerable<ServerPlayerCharacter> GetAllServerPlayers() => _playersByConnectionId.Values;


    // ═══════════════════════════════════════════════════════════════
    // INTERNAL - Only for Services
    // ═══════════════════════════════════════════════════════════════

    internal void RegisterServerPlayer(ServerPlayerCharacter serverPlayer)
    {
        _playersByConnectionId[serverPlayer.Connection.Id] = serverPlayer;

        // Connection-Mapping in IdRegistry
        IdRegistry.Instance.RegisterConnection(
            serverPlayer.Connection.Id,
            serverPlayer.Entity.PersistentId
        );
    }

    internal ServerPlayerCharacter? RemoveServerPlayer(Guid connectionId)
    {
        if (!_playersByConnectionId.TryGetValue(connectionId, out ServerPlayerCharacter? player))
            return null;

        _playersByConnectionId.TryRemove(connectionId, out _);
        IdRegistry.Instance.UnregisterConnection(connectionId);

        return player;
    }
}
