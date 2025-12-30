namespace Mmo.Shared.Generators;

/// <summary>
///     Marks a property as the identifier field for delta DTOs.
///     This allows the dirty-tracking system to recognize which field contains the entity ID,
///     regardless of the property name (EntityId, PlayerId, QuestId, etc.).
/// </summary>
/// <remarks>
///     <para>
///     Use this attribute when creating custom delta DTOs where the ID field name varies.
///     The system can automatically use this field when generating delta update methods.
///     </para>
///     
///     <para>
///     <strong>Example Usage:</strong>
///     </para>
///     <code>
///     [MessagePackObject]
///     public class PlayerInventoryDelta
///     {
///         [Key(0)]
///         [DeltaId]  // Marks this as the identifier
///         public Guid PlayerId { get; set; }
///         
///         [Key(1)]
///         public Item? Slot1 { get; set; }  // null if unchanged
///     }
///     
///     [MessagePackObject]
///     public class QuestProgressDelta
///     {
///         [Key(0)]
///         [DeltaId]  // Different name, same purpose
///         public int QuestId { get; set; }
///         
///         [Key(1)]
///         public int? Progress { get; set; }  // null if unchanged
///     }
///     </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class DeltaIdAttribute : Attribute
{
}
