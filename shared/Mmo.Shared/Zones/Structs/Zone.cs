namespace Mmo.Shared.Zones.Structs;

public readonly struct Zone (ushort zoneId, string name, ZoneBounds bounds)
{
    private readonly HashSet<Guid> _entityIds = new();  // Nur IDs!

    public ushort Id { get; } =  zoneId;
    public string Name { get; }= name;
    public ZoneBounds Bounds { get; }= bounds;

    public int EntityCount => _entityIds.Count;

    public IEnumerable<Guid> GetEntityIds() => _entityIds;

    public bool HasEntity(Guid persistentId) => _entityIds.Contains(persistentId);

    public void AddEntity(Guid persistentId) => _entityIds.Add(persistentId);

    public void RemoveEntity(Guid persistentId) => _entityIds.Remove(persistentId);
}
