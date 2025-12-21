using Mmo.Server.MovementService.Records;
using Mmo.Shared.Core.Records;

namespace Mmo.Server.MovementService.Interfaces;

/// <summary>
///     Service for movement and position validation.
/// </summary>
public interface IMovementService
{
    /// <summary>
    ///     Validates a player's movement update.
    /// </summary>
    /// <param name="persistentId">The player's persistent ID.</param>
    /// <param name="newPosition">The new position to validate.</param>
    /// <returns>Validation result with corrected position if needed.</returns>
    Task<MovementValidationResult> ValidateMovementAsync(Guid persistentId, Position newPosition);

    /// <summary>
    ///     Teleports a player to a new position.
    /// </summary>
    /// <param name="persistentId">The player's persistent ID.</param>
    /// <param name="targetPosition">The target position.</param>
    /// <returns>Teleport result with final position on success.</returns>
    Task<TeleportResult> TeleportAsync(Guid persistentId, Position targetPosition);
}
