namespace Mmo.Shared.Zones.Interfaces;

/// <summary>
///     Interface for delta DTOs that support efficient ID-based lookup.
///     Enables O(1) lookup when delta DTOs are stored in dictionaries or hash sets.
/// </summary>
/// <typeparam name="TId">The type of the identifier (e.g., Guid, int, long).</typeparam>
/// <remarks>
///     <para>
///         This interface allows delta DTOs to be efficiently looked up by their ID.
///         When combined with Dictionary or HashSet, it enables O(1) lookup performance.
///     </para>
///     <para>
///         <strong>Usage Pattern:</strong>
///     </para>
///     <code>
///     // Convert list to dictionary for O(1) lookup
///     var deltaDict = positionDeltas.ToDictionary(d => d.GetId());
///     
///     // O(1) lookup by entity ID
///     if (deltaDict.TryGetValue(entityId, out var delta))
///     {
///         // Apply delta
///     }
///     </code>
/// </remarks>
public interface IDeltaDto<out TId>
{
    /// <summary>
    ///     Gets the identifier value from the delta DTO.
    ///     This is used for O(1) lookups in collections.
    /// </summary>
    /// <returns>The identifier value.</returns>
    TId GetId();
}
