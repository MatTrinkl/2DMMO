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
/// [GenerateDirtyTracking]
/// public partial class PlayerEntity
/// {
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
}
