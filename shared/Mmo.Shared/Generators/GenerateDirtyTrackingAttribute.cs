namespace Mmo.Shared.Generators;

/// <summary>
///     Marks a class or interface for automatic dirty tracking code generation.
///     The generator will implement IDirtyTrackable and create property wrappers for [TrackDirty] members.
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
/// [GenerateDirtyTracking(IdPropertyName = "PersistentId", FlagsEnumType = "Mmo.Shared.Entities.Enums.EntityDirtyFlags")]
/// public partial class PlayerEntity
/// {
///     public Guid PersistentId { get; set; }
///     
///     [TrackDirty("Position")]
///     public float X { get; set; }
///     
///     [TrackDirty("Position")]
///     public float Y { get; set; }
///     
///     [TrackDirty("Health")]
///     public int CurrentHP { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class GenerateDirtyTrackingAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the property that serves as the entity identifier.
    ///     If not specified, no IDeltaDto interface will be implemented (for singleton objects like ZoneContext).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Set this for objects that need lookup capability (entities in lists).
    ///     Leave null/empty for singleton objects where no lookup is needed.
    ///     </para>
    ///     
    ///     <para>
    ///     When specified, the generator will:
    ///     <list type="bullet">
    ///         <item>Include this property in all generated delta DTOs</item>
    ///         <item>Mark it with [DeltaId] attribute for automatic recognition</item>
    ///         <item>Implement IDeltaDto&lt;TId&gt; interface for O(1) lookup capabilities</item>
    ///         <item>Generate GetId() method for dictionary-based lookups</item>
    ///     </list>
    ///     </para>
    ///     
    ///     <para>Examples:</para>
    ///     <list type="bullet">
    ///         <item>Entities: IdPropertyName = "PersistentId" → generates IDeltaDto&lt;Guid&gt;</item>
    ///         <item>ZoneContext: IdPropertyName = null → generates plain Delta DTO</item>
    ///     </list>
    /// </remarks>
    public string? IdPropertyName { get; set; }

    /// <summary>
    ///     Gets or sets the fully qualified name of the flags enum type to use for dirty tracking.
    ///     If not specified, defaults to "Mmo.Shared.Entities.Enums.EntityDirtyFlags".
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This allows different domains to use their own flag enums:
    ///     <list type="bullet">
    ///         <item>Entity domain: "Mmo.Shared.Entities.Enums.EntityDirtyFlags"</item>
    ///         <item>Zone domain: "Mmo.Shared.Zones.Enums.ZoneDirtyFlags"</item>
    ///         <item>Inventory domain: "Mmo.Shared.Inventory.Enums.InventoryDirtyFlags"</item>
    ///     </list>
    ///     </para>
    ///     
    ///     The enum type must:
    ///     - Be a [Flags] enum
    ///     - Have a backing type of uint or ulong
    ///     - Define at least a "None" value
    /// </remarks>
    public string? FlagsEnumType { get; set; }
}
