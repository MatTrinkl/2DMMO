using Mmo.Server.Zones.Records;
using Mmo.Shared.Movement.Records;

namespace Mmo.Server.Zones.Interfaces;

/// <summary>
///     Service interface for zone management operations.
///     Provides methods for querying zones, handling transitions, and tracking population.
/// </summary>
public interface IZoneService
{
    /// <summary>Gets information about a specific zone.</summary>
    /// <param name="zoneId">The zone ID to query.</param>
    /// <returns>Zone information if found, null otherwise.</returns>
    ZoneInfo? GetZoneInfo(ushort zoneId);

    /// <summary>Gets information about all available zones.</summary>
    /// <returns>Collection of all zone information.</returns>
    IEnumerable<ZoneInfo> GetAllZones();

    /// <summary>Checks if a zone exists.</summary>
    /// <param name="zoneId">The zone ID to check.</param>
    /// <returns>True if the zone exists, false otherwise.</returns>
    bool ZoneExists(ushort zoneId);

    // Zone Transitions (with validation!)
    /// <summary>Requests a zone transfer for a player.</summary>
    /// <param name="playerId">The persistent ID of the player.</param>
    /// <param name="targetZoneId">The target zone ID.</param>
    /// <param name="targetPosition">Optional target position in the new zone.</param>
    /// <returns>Result of the transfer operation.</returns>
    ZoneTransferResult RequestZoneTransferAsync(Guid playerId,
        ushort targetZoneId,
        Position? targetPosition = null);

    // Zone Population
    /// <summary>Gets the current player count in a zone.</summary>
    /// <param name="zoneId">The zone ID to query.</param>
    /// <returns>Number of players currently in the zone.</returns>
    int GetPlayerCount(ushort zoneId);

    /// <summary>Gets all players currently in a zone.</summary>
    /// <param name="zoneId">The zone ID to query.</param>
    /// <returns>Collection of player persistent IDs in the zone.</returns>
    IEnumerable<Guid> GetPlayersInZone(ushort zoneId);
}
