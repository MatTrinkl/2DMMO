using MessagePack;
using Mmo.Shared.Generators;
using Mmo.Shared.Zones.Interfaces;

namespace Mmo.Shared.Zones.Dtos;

/// <summary>
///     Delta DTO containing only state and stat-related changes for an entity.
///     This is a lightweight alternative to sending the full entity DTO.
/// </summary>
/// <remarks>
///     <para>
///     This DTO is used in the ZoneDelta message system to efficiently transmit
///     state and stat updates. All fields are nullable - only changed properties are included.
///     </para>
///     
///     <para>
///     <strong>Nullable Pattern for Deltas:</strong>
///     - null = property hasn't changed, don't update
///     - non-null = property has changed, update to this value
///     This pattern minimizes bandwidth by only sending what actually changed.
///     </para>
///     
///     <para>
///     <strong>Extensibility:</strong>
///     Create similar delta DTOs for other aspects:
///     - InventoryDelta (equipped items, bag slots)
///     - BuffDelta (active buffs/debuffs)
///     - QuestDelta (quest progress)
///     - AchievementDelta (achievement progress)
///     - Implement IDeltaDto&lt;TId&gt; for O(1) lookup support
///     </para>
/// </remarks>
/// <example>
/// <code>
/// // Send only HP change (other fields remain null)
/// var delta = new EntityStateDelta
/// {
///     EntityId = entity.PersistentId,
///     CurrentHP = entity.CurrentHP,
///     MaxHP = null, // hasn't changed
///     State = null, // hasn't changed
///     ModelId = null, // hasn't changed
///     Level = null // hasn't changed
/// };
/// 
/// // O(1) lookup in collections
/// var deltaDict = stateDeltas.ToDictionary(d => d.GetId());
/// if (deltaDict.TryGetValue(entityId, out var delta))
/// {
///     // Apply delta
/// }
/// </code>
/// </example>
[MessagePackObject]
public class EntityStateDelta : IDeltaDto<Guid>
{
    /// <summary>
    ///     Gets or sets the unique identifier of the entity.
    /// </summary>
    [Key(0)]
    [DeltaId]
    public Guid EntityId { get; set; }
    
    /// <summary>
    ///     Gets or sets the current health points (nullable).
    ///     Null if HP hasn't changed.
    /// </summary>
    [Key(1)]
    public int? CurrentHP { get; set; }
    
    /// <summary>
    ///     Gets or sets the maximum health points (nullable).
    ///     Null if MaxHP hasn't changed.
    /// </summary>
    [Key(2)]
    public int? MaxHP { get; set; }
    
    /// <summary>
    ///     Gets or sets the entity state as a byte (nullable).
    ///     Null if state hasn't changed.
    ///     Cast to/from EntityState enum on client/server.
    /// </summary>
    /// <remarks>
    ///     Uses byte instead of EntityState enum to avoid circular dependencies
    ///     and allow for flexibility in state definitions.
    /// </remarks>
    [Key(3)]
    public byte? State { get; set; }
    
    /// <summary>
    ///     Gets or sets the model/appearance ID (nullable).
    ///     Null if model hasn't changed.
    ///     Used for polymorphs, disguises, mounts, etc.
    /// </summary>
    [Key(4)]
    public uint? ModelId { get; set; }
    
    /// <summary>
    ///     Gets or sets the level (nullable).
    ///     Null if level hasn't changed.
    /// </summary>
    [Key(5)]
    public int? Level { get; set; }
    
    /// <summary>
    ///     Gets the identifier value for O(1) lookup in collections.
    /// </summary>
    /// <returns>The entity identifier.</returns>
    public Guid GetId() => EntityId;
}
