using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Creatures.Enums;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Generators;
using Mmo.Shared.Npc.Enums;

namespace Mmo.Shared.Npc.Interfaces;

/// <summary>
///     Interface für NPCs und Monster.
/// </summary>
[GenerateDto(InheritInterfaces = false, DtoName = "NpcEntityDto")]
[DtoUnionMember(1, typeof(IEntity))]
public interface INpcEntity : ICombatEntity
{
    // NPC Identity
    int NpcTemplateId { get; init; }

    // WAS ist der NPC?
    CreatureType CreatureType { get; init; }

    // WIE STARK ist der NPC?
    CombatRank CombatRank { get; init; }

    // WAS MACHT der NPC?
    NpcFunction Function { get; init; }

    // AI
    NpcAiState AiState { get; set; }
}
