using Mmo.Server.Zones.Configurations;
using Mmo.Shared.Core;
using Mmo.Shared.Entities.Dtos;
using Mmo.Shared.Zones.Interfaces;
using Mmo.Shared.Zones.Messages.Server_Client;

namespace Mmo.Server.Zones;

public struct Zone(ZoneConfig config, ZoneContext context)
{
    private readonly HashSet<Guid> _entityIds = []; // Nur IDs!

    // ═══ Config ═══
    public ZoneContext Context { get; } = context;
    public ZoneConfig Config { get; } = config;

    public ushort ZoneId => Config.ZoneId;
    public string ZoneName => Config.InternalName;

    public IEnumerable<Guid> GetEntityIds() => _entityIds;

    public bool HasEntity(Guid persistentId) => _entityIds.Contains(persistentId);

    public void AddEntity(Guid persistentId) => _entityIds.Add(persistentId);

    public void RemoveEntity(Guid persistentId) => _entityIds.Remove(persistentId);

    public int EntityCount => _entityIds.Count;

    public ZoneState GetState() => new()
    {
        ZoneContext = Context.ToDto(),
        ZoneConfig = Config.ToDto(),
        Entities = IdRegistry.Instance.GetEntities(_entityIds).ToUnionDtoList()
    };
}
