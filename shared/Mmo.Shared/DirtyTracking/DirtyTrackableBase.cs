namespace Mmo.Shared.DirtyTracking;

/// <summary>
///     Abstract base class for dirty-trackable objects with generic flag type support.
///     Provides a complete implementation of IDirtyTrackable with bitwise flag operations.
/// </summary>
/// <typeparam name="TFlags">The type of the dirty flags enum (must be a flags enum).</typeparam>
/// <remarks>
///     This class uses Convert.ToUInt64 for bitwise operations, supporting enums up to 64 bits.
///     For ulong-based enums (InventoryDirtyFlags), use the full 64-bit range.
/// </remarks>
public abstract class DirtyTrackableBase<TFlags> : IDirtyTrackable<TFlags>
    where TFlags : struct, Enum
{
    /// <inheritdoc />
    public TFlags DirtyFlags { get; private set; }

    /// <inheritdoc />
    public bool IsDirty => !EqualityComparer<TFlags>.Default.Equals(DirtyFlags, default);

    /// <inheritdoc />
    public void ClearDirtyFlags() => DirtyFlags = default;

    /// <inheritdoc />
    public void MarkDirty(TFlags flags)
    {
        ulong current = Convert.ToUInt64(DirtyFlags);
        ulong toSet = Convert.ToUInt64(flags);
        DirtyFlags = (TFlags)Enum.ToObject(typeof(TFlags), current | toSet);
    }

    /// <inheritdoc />
    public bool HasFlag(TFlags flags)
    {
        ulong current = Convert.ToUInt64(DirtyFlags);
        ulong toCheck = Convert.ToUInt64(flags);
        return (current & toCheck) == toCheck;
    }

    /// <summary>
    ///     Sets a field value and marks the appropriate dirty flag if the value changed.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="field">Reference to the backing field.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="flag">The dirty flag to set if the value changed.</param>
    /// <returns>True if the value was changed, false if it was already equal.</returns>
    /// <remarks>
    ///     This helper method can be used in property setters to automatically track changes:
    ///     <code>
    ///     private float _x;
    ///     public float X
    ///     {
    ///         get => _x;
    ///         set => SetField(ref _x, value, EntityDirtyFlags.Position);
    ///     }
    ///     </code>
    /// </remarks>
    protected bool SetField<T>(ref T field, T value, TFlags flag)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        MarkDirty(flag);
        return true;
    }
}
