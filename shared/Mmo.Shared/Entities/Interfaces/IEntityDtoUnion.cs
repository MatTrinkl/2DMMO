using MessagePack;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Npc.Interfaces;

namespace Mmo.Shared.Entities.Interfaces;

[Union(0, typeof(CharacterEntityDto))]
[Union(1, typeof(NpcEntityDto))]
public interface EntityDtoUnion
{
}
