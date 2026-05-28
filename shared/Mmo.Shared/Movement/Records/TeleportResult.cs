namespace Mmo.Shared.Movement.Records;

/// <summary>
///     Result of a teleport operation.
/// </summary>
public record TeleportResult
{
    /// <summary>Whether the teleport was successful.</summary>
    public bool Success { get; init; }

    /// <summary>The final position after the teleport.</summary>
    public Position? FinalPosition { get; init; }

    /// <summary>The zone after the teleport (in case of cross-zone teleport).</summary>
    public ushort? NewZoneId { get; init; }

    /// <summary>Whether a zone change occurred.</summary>
    public bool ZoneChanged { get; init; }

    /// <summary>Error code on failure.</summary>
    public string? Error { get; init; }

    // ─── Factory Methods ───

    public static TeleportResult Succeeded(Position finalPosition)
        => new() { Success = true, FinalPosition = finalPosition, ZoneChanged = false };

    public static TeleportResult SucceededWithZoneChange(Position finalPosition, ushort newZoneId)
        => new() { Success = true, FinalPosition = finalPosition, NewZoneId = newZoneId, ZoneChanged = true };

    public static TeleportResult Failed(string error)
        => new() { Success = false, Error = error };
}
