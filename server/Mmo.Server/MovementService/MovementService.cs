using Mmo.Server.MovementService.Interfaces;
using Mmo.Server.MovementService.Records;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;

namespace Mmo.Server.MovementService;

/// <summary>
///     Stub implementation of IMovementService.
///     TODO: Implement movement validation and teleport logic.
/// </summary>
public class MovementService : IMovementService
{
    private readonly ILog _log;

    public MovementService(ILog log)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <inheritdoc />
    public Task<MovementValidationResult> ValidateMovementAsync(Guid persistentId, Position newPosition)
    {
        _log.Debug("MovementService.ValidateMovementAsync called for {PersistentId}", persistentId);

        // TODO: Implement movement validation (speed checks, collision, etc.)
        return Task.FromResult(new MovementValidationResult(true));
    }

    /// <inheritdoc />
    public Task<TeleportResult> TeleportAsync(Guid persistentId, Position targetPosition)
    {
        _log.Debug("MovementService.TeleportAsync called for {PersistentId}", persistentId);

        // TODO: Implement teleport logic
        return Task.FromResult(new TeleportResult(
            false,
            Error: "Teleport not yet implemented"));
    }
}
