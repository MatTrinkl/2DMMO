using Mmo.Shared.Movement.Records;

namespace Mmo.Shared.Zones.Configurations;

/// <summary>
///     NPC-Spawn Definition.
/// </summary>
public record NpcSpawnConfig
{
    public int SpawnId { get; set; }
    public int NpcTemplateId { get; set; }
    public Position Position { get; set; } = null!;
    public int RespawnSeconds { get; set; } = 60;
    public int? PatrolPathId { get; set; }
}
