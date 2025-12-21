using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Npc.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Npc.Interfaces;

/// <summary>
///     Interface für NPCs und Monster.
/// </summary>
public interface INpcEntity : ICombatEntity
{
    // NPC Identity
    int NpcTemplateId { get; }

    int SpawnId { get; }

    // WAS ist der NPC?
    CreatureType CreatureType { get; }

    // WIE STARK ist der NPC?
    CombatRank CombatRank { get; }

    // WAS MACHT der NPC?
    NpcFunction Function { get; }

    // AI
    NpcAiState AiState { get; set; }
    float AggroRadius { get; }
    float LeashRadius { get; }

    // Spawn
    Position SpawnPosition { get; }
    int RespawnTimeSeconds { get; }

    // Loot
    int? LootTableId { get; }
    int ExperienceReward { get; }

    // Threat
    Guid? HighestThreatEntityId { get; }
    void AddThreat(Guid entityId, int amount);
    void ClearThreat();

    // Methods
    void Reset();
    void Respawn();
    void Evade();
}
