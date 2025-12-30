namespace Mmo.Shared.Generators;

/// <summary>
///     Marks a class or interface for automatic dirty tracking code generation.
///     The generator will implement IDirtyTrackable and create property wrappers for [TrackedProperty] members.
/// </summary>
/// <remarks>
///     <para>
///     When applied to a class or interface, the DtoGenerator will:
///     <list type="bullet">
///         <item>Implement IDirtyTrackable interface</item>
///         <item>Generate backing fields and property wrappers for tracked properties</item>
///         <item>Generate extension methods for creating delta DTOs (e.g., ToPositionDelta())</item>
///         <item>Automatically set DirtyFlags when tracked properties change</item>
///         <item>Transfer the ID property to generated delta DTOs (when IdPropertyName is specified)</item>
///     </list>
///     </para>
///     
///     <para>
///     This attribute is designed to be extensible - it can be applied to any entity type,
///     not just the predefined ones. Delta DTO generation can be customized via additional
///     attributes or conventions.
///     </para>
/// </remarks>
/// <example>
/// <code>
/// [GenerateDirtyTracking(IdPropertyName = "PersistentId")]
/// public partial class PlayerEntity
/// {
///     public Guid PersistentId { get; set; }
///     
///     [TrackedProperty(DirtyFlags.Position)]
///     public float X { get; set; }
///     
///     [TrackedProperty(DirtyFlags.Position)]
///     public float Y { get; set; }
///     
///     [TrackedProperty(DirtyFlags.Health)]
///     public int CurrentHP { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class GenerateDirtyTrackingAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the property that serves as the entity identifier.
    ///     This ID will be automatically included in generated delta DTOs with the [DeltaId] attribute.
    /// </summary>
    /// <remarks>
    ///     When specified, the generator will:
    ///     <list type="bullet">
    ///         <item>Include this property in all generated delta DTOs</item>
    ///         <item>Mark it with [DeltaId] attribute for automatic recognition</item>
    ///         <item>Enable O(1) lookup capabilities in delta collections</item>
    ///     </list>
    ///     
    ///     Examples: "PersistentId", "EntityId", "PlayerId", "QuestId"
    /// </remarks>
    public string? IdPropertyName { get; set; }
}
