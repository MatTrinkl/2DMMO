using Mmo.Shared.Core.Records;

namespace Mmo.Shared.Movement.Records;

/// <summary>
///     Result of a teleport operation.
/// </summary>
public record TeleportResult
{
    /// <summary>Ob der Teleport erfolgreich war.</summary>
    public bool Success { get; init; }

    /// <summary>Die finale Position nach dem Teleport.</summary>
    public Position? FinalPosition { get; init; }

    /// <summary>Die Zone nach dem Teleport (bei Cross-Zone Teleport).</summary>
    public ushort? NewZoneId { get; init; }

    /// <summary>Ob ein Zone-Wechsel stattfand.</summary>
    public bool ZoneChanged { get; init; }

    /// <summary>Error-Code bei Fehlschlag.</summary>
    public string? Error { get; init; }

    // ─── Factory Methods ───

    public static TeleportResult Succeeded(Position finalPosition)
        => new() { Success = true, FinalPosition = finalPosition, ZoneChanged = false };

    public static TeleportResult SucceededWithZoneChange(Position finalPosition, ushort newZoneId)
        => new() { Success = true, FinalPosition = finalPosition, NewZoneId = newZoneId, ZoneChanged = true };

    public static TeleportResult Failed(string error)
        => new() { Success = false, Error = error };
}
