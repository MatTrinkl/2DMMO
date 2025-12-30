// shared/Mmo.Shared/Entities/IEntity.cs

using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Generators;

namespace Mmo.Shared.Entities.Interfaces;

[GenerateDto(InheritInterfaces = false, DtoName = "EntityDto")]
[GenerateDtoUnion(UnionName = "EntityDtoUnion", Namespace = "Mmo.Shared.Entities.Dtos")]
[GenerateDirtyTracking(IdPropertyName = "PersistentId")]
public interface IEntity
{
    /// <summary>
    ///     Stable identity for network communication.
    ///     Never changes, even on zone transfer.
    /// </summary>
    Guid PersistentId { get; init; }

    EntityType Type { get; }

    [TrackedProperty(DirtyFlags.Position)]
    Position Position { get; set; }
}
