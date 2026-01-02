using Mmo.Shared.Zones.Interfaces;

namespace Mmo.Shared.Zones.Extensions;

/// <summary>
///     Extension methods for delta DTOs to enable efficient O(1) lookups.
/// </summary>
public static class DeltaDtoExtensions
{
    /// <summary>
    ///     Converts a collection of delta DTOs to a dictionary for O(1) lookup by ID.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <typeparam name="TDelta">The type of the delta DTO.</typeparam>
    /// <param name="deltas">The collection of delta DTOs.</param>
    /// <returns>A dictionary keyed by delta ID for O(1) lookup.</returns>
    /// <remarks>
    ///     <para>
    ///         This method enables efficient lookup of delta DTOs by their ID.
    ///         Typical use case: Server sends a list of deltas, client converts to dictionary for fast access.
    ///     </para>
    ///     <para>
    ///         <strong>Performance:</strong>
    ///         - Conversion: O(n) where n is the number of deltas
    ///         - Lookup: O(1) after conversion
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Receive delta list from server (using generated Delta DTO)
    /// List&lt;IEntityPositionDelta&gt; positionDeltas = zoneDelta.PositionUpdates;
    /// 
    /// // Convert to dictionary for O(1) lookup
    /// var deltaDict = positionDeltas.ToDeltaDictionary();
    /// 
    /// // Fast lookup by entity ID
    /// if (deltaDict.TryGetValue(myEntityId, out var delta))
    /// {
    ///     // Apply position delta
    ///     entity.X = delta.X;
    ///     entity.Y = delta.Y;
    /// }
    /// </code>
    /// </example>
    public static Dictionary<TId, TDelta> ToDeltaDictionary<TId, TDelta>(this IEnumerable<TDelta> deltas)
        where TDelta : IDeltaDto<TId>
        where TId : notnull =>
        deltas.ToDictionary(delta => delta.GetId());

    /// <summary>
    ///     Tries to get a delta DTO from a collection by ID.
    ///     This is a convenience method that combines dictionary creation and lookup.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <typeparam name="TDelta">The type of the delta DTO.</typeparam>
    /// <param name="deltas">The collection of delta DTOs.</param>
    /// <param name="id">The identifier to search for.</param>
    /// <param name="delta">The found delta DTO, or null if not found.</param>
    /// <returns>True if the delta was found; otherwise, false.</returns>
    /// <remarks>
    ///     <para>
    ///         <strong>Performance Note:</strong>
    ///         If you need to perform multiple lookups, use <see cref="ToDeltaDictionary{TId,TDelta}" /> first
    ///         to avoid repeated linear searches. This method is O(n) for each call.
    ///     </para>
    /// </remarks>
    public static bool TryGetDelta<TId, TDelta>(this IEnumerable<TDelta> deltas, TId id, out TDelta? delta)
        where TDelta : class, IDeltaDto<TId>
        where TId : notnull
    {
        delta = deltas.FirstOrDefault(d => EqualityComparer<TId>.Default.Equals(d.GetId(), id));
        return delta != null;
    }
}
