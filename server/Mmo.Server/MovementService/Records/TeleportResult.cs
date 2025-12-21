using Mmo.Shared.Core.Records;

namespace Mmo.Server.MovementService.Records;

/// <summary>
///     Result of teleport operation.
/// </summary>
public record TeleportResult(
    bool Success,
    Position? FinalPosition = null,
    string? Error = null);
