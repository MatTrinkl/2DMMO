using Mmo.Shared.Core.Records;

namespace Mmo.Server.MovementService.Records;

/// <summary>
///     Result of movement validation.
/// </summary>
public record MovementValidationResult(
    bool IsValid,
    Position? CorrectedPosition = null);
