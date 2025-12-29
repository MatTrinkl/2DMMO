using Mmo.Shared.Entities.Enums;

namespace Mmo.Shared.Entities.Interfaces;

/// <summary>
///     Interface for objects that support automatic dirty tracking.
///     Implementers can track which properties have changed since the last synchronization.
/// </summary>
/// <remarks>
///     This interface is typically implemented by generated code via the [GenerateDirtyTracking] attribute.
///     The dirty-tracking system uses DirtyFlags to optimize network updates by sending only changed properties.
/// </remarks>
public interface IDirtyTrackable
{
    /// <summary>
    ///     Gets the flags indicating which properties have changed.
    /// </summary>
    DirtyFlags DirtyFlags { get; }
    
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
    void MarkDirty(DirtyFlags flags);
}
