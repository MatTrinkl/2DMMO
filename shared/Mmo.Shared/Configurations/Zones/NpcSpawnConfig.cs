using Mmo.Shared.Records;

namespace Mmo.Shared.Configurations.Zones;

/// <summary>
///     NPC-Spawn Definition.
/// </summary>
public class NpcSpawnConfig
{
    public int SpawnId { get; set; }
    public int NpcTemplateId { get; set; }
    public Position Position { get; set; }
    public int RespawnSeconds { get; set; } = 60;
    public int? PatrolPathId { get; set; }
}
