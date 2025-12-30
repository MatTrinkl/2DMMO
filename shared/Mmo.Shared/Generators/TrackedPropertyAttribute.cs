using Mmo.Shared.Entities.Enums;

namespace Mmo.Shared.Generators;

/// <summary>
///     Marks a property for automatic dirty tracking.
///     When the property is set, the specified DirtyFlags will be automatically set.
/// </summary>
/// <remarks>
///     This attribute works with the [GenerateDirtyTracking] attribute on the containing class.
///     The generator will create property wrappers that automatically update DirtyFlags when the property changes.
/// </remarks>
/// <example>
/// <code>
/// [GenerateDirtyTracking]
/// public partial class Player
/// {
///     [TrackedProperty(DirtyFlags.Position)]
///     public float X { get; set; }
///     
///     [TrackedProperty(DirtyFlags.Health)]
///     public int CurrentHP { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class TrackedPropertyAttribute : Attribute
{
    /// <summary>
    ///     Gets the dirty flag that will be set when this property changes.
    /// </summary>
    public DirtyFlags Flag { get; }
    
    /// <summary>
    ///     Initializes a new instance of the <see cref="TrackedPropertyAttribute"/> class.
    /// </summary>
    /// <param name="flag">The flag to set when this property changes.</param>
    public TrackedPropertyAttribute(DirtyFlags flag)
    {
        Flag = flag;
    }
}
