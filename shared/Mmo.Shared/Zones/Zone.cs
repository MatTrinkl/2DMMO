using Mmo.Shared.Character.Enums;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Generators;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Interfaces;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Shared.Zones;

[GenerateDto(InheritInterfaces = true)]
public class Zone(ushort zoneId, string name, ZoneBounds bounds) : IZoneData
{
    private readonly HashSet<Guid> _entityIds = new(); // Nur IDs!

    public ushort Id { get; } = zoneId;
    public string Name { get; } = name;

    public int EntityCount => _entityIds.Count;
    public ZoneBounds Boarder { get; } = bounds;
    public ushort ZoneId { get; }
    public string ZoneName { get; }
    public ZoneFlags ZoneFlags { get; }
    public int RecommendedMinLevel { get; }
    public int RecommendedMaxLevel { get; }
    public bool IsPvpEnabled { get; }
    public bool IsContested { get; }
    public Faction? ControllingFaction { get; }
    public bool IsSanctuary { get; }
    public Position DefaultSpawnPoint { get; }
    public Position? GraveyardPosition { get; }
    public string MusicId { get; }
    public string AmbienceId { get; }

    public IEnumerable<Guid> GetEntityIds() => _entityIds;

    public bool HasEntity(Guid persistentId) => _entityIds.Contains(persistentId);

    public void AddEntity(Guid persistentId) => _entityIds.Add(persistentId);

    public void RemoveEntity(Guid persistentId) => _entityIds.Remove(persistentId);
}
