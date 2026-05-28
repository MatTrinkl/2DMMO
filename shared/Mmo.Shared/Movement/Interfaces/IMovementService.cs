using System.Numerics;
using Mmo.Shared.Movement.Records;

namespace Mmo.Shared.Movement.Interfaces;

public interface IMovementService
{
    // Movement Validation (Anti-Cheat!)
    MovementValidationResult ValidateMovement(
        Guid playerId,
        Position currentPosition,
        Position targetPosition,
        float deltaTime);

    void ApplyMovement(Guid entityId, Position newPosition);

    // Broadcasting
    void BroadcastMovement(Guid entityId, Position position, Vector2 velocity);

    // Special Movement
    TeleportResult Teleport(Guid playerId, Position targetPosition, bool crossZone = false);
    void ApplyKnockback(Guid entityId, Vector2 direction, float force);
}
