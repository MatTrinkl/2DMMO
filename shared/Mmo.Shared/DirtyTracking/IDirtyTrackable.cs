namespace Mmo.Shared.DirtyTracking;

/// <summary>
///     Generic interface for objects that support automatic dirty tracking.
///     Implementers can track which properties have changed since the last synchronization.
/// </summary>
/// <typeparam name="TFlags">The type of the dirty flags enum (e.g., EntityDirtyFlags, ZoneDirtyFlags).</typeparam>
/// <remarks>
///     This interface enables domain-specific dirty tracking with type safety.
///     Different data structures can use their own flag enums while sharing the same tracking infrastructure.
/// </remarks>
public interface IDirtyTrackable<TFlags> where TFlags : struct, Enum
{
    /// <summary>
    ///     Gets the flags indicating which properties have changed.
    /// </summary>
    TFlags DirtyFlags { get; }
    
    /// <summary>
    ///     Gets a value indicating whether any properties have changed.
    /// </summary>
    bool IsDirty { get; }
    
    /// <summary>
    ///     Clears all dirty flags, marking all properties as synchronized.
    /// </summary>
    /// <remarks>
    ///     Call this method after successfully sending delta updates to clients.
    /// </remarks>
    void ClearDirtyFlags();
    
    /// <summary>
    ///     Marks specific properties as dirty.
    /// </summary>
    /// <param name="flags">The flags to set.</param>
    /// <remarks>
    ///     This method uses bitwise OR to combine flags with existing dirty state.
    /// </remarks>
    void MarkDirty(TFlags flags);
    
    /// <summary>
    ///     Checks if the specified flags are set.
    /// </summary>
    /// <param name="flags">The flags to check.</param>
    /// <returns>True if all specified flags are set, false otherwise.</returns>
    bool HasFlag(TFlags flags);
}
