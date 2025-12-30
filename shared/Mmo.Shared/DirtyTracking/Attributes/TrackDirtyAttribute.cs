namespace Mmo.Shared.DirtyTracking.Attributes;

/// <summary>
///     Marks a property for automatic dirty tracking with a named flag.
///     The flag name should correspond to a value in the domain-specific DirtyFlags enum.
/// </summary>
/// <remarks>
///     <para>
///     This attribute uses string-based flag names to support generic dirty tracking across different domains.
///     The flag name should match a value in your domain's flags enum (e.g., "Position" in EntityDirtyFlags).
///     </para>
///     
///     <para>
///     <strong>Examples:</strong>
///     </para>
///     <code>
///     // Entity domain
///     [TrackDirty("Position")]
///     public Position Position { get; set; }
///     
///     // Zone domain
///     [TrackDirty("Weather")]
///     public WeatherType Weather { get; set; }
///     
///     // Inventory domain
///     [TrackDirty("Slot1")]
///     public Item? Slot1 { get; set; }
///     </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackDirtyAttribute : Attribute
{
    /// <summary>
    ///     Gets the name of the dirty flag that will be set when this property changes.
    /// </summary>
    /// <remarks>
    ///     This name should match a value in the domain-specific DirtyFlags enum.
    ///     For example, "Position" for EntityDirtyFlags.Position or "Weather" for ZoneDirtyFlags.Weather.
    /// </remarks>
    public string FlagName { get; }
    
    /// <summary>
    ///     Initializes a new instance of the <see cref="TrackDirtyAttribute"/> class.
    /// </summary>
    /// <param name="flagName">
    ///     The name of the flag in the domain's DirtyFlags enum.
    ///     This should be the exact name of the enum value (e.g., "Position", "Health", "Weather").
    /// </param>
    public TrackDirtyAttribute(string flagName)
    {
        if (string.IsNullOrWhiteSpace(flagName))
            throw new ArgumentException("Flag name cannot be null or whitespace.", nameof(flagName));
            
        FlagName = flagName;
    }
}
