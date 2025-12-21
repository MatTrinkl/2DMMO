using Mmo.Shared.Core.Records;
using Mmo.Shared.Movement.Enums;

namespace Mmo.Shared.Movement.Records;

/// <summary>
///     Result of a movement validation (Anti-Cheat).
/// </summary>
public record MovementValidationResult
{
    /// <summary>Ob die Bewegung gültig ist.</summary>
    public bool IsValid { get; init; }

    /// <summary>Korrigierte Position (bei Cheat-Verdacht).</summary>
    public Position?  CorrectedPosition { get; init; }

    /// <summary>Art der Verletzung (für Logging/Banning).</summary>
    public MovementViolationType?  ViolationType { get; init; }

    /// <summary>Schwere der Verletzung (0. 0 - 1.0).</summary>
    public float?  ViolationSeverity { get; init; }

    // ─── Factory Methods ───

    public static MovementValidationResult Valid()
        => new() { IsValid = true };

    public static MovementValidationResult Invalid(
        Position correctedPosition,
        MovementViolationType violationType,
        float severity = 0.5f)
        => new()
        {
            IsValid = false,
            CorrectedPosition = correctedPosition,
            ViolationType = violationType,
            ViolationSeverity = severity
        };
}
