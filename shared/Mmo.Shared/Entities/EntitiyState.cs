using MessagePack;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     The parent class of all entities for now. Todo: Split this into Prefab and Living.
/// </summary>
[MessagePackObject]
[Union(0, typeof(PlayerState))]
public abstract class EntityState
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public EntityState()
    {
    }

    /// <summary>
    ///     Sets the parameters which are shared over all children.
    ///     Can only be called by children.
    /// </summary>
    /// <param name="entityId">The id of this entity.</param>
    /// <param name="position">The current position of this entity.</param>
    protected EntityState(Guid entityId, Position position)
    {
        EntityId = entityId;
        Position = position;
    }

    /// <summary>
    ///     The unique id of this entity. Used for finding the entity in lists etc.
    /// </summary>
    [Key(0)]
    public Guid EntityId { get; set; }

    /// <summary>
    ///     The current positon of this entity in its current zone.
    /// </summary>
    [Key(1)]
    public Position Position { get; set; } = new(0,0);
}
