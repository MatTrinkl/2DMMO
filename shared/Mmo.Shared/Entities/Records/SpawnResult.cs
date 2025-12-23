namespace Mmo.Shared.Entities.Records;

/// <summary>
///     Result of an entity spawn operation.
/// </summary>
public record SpawnResult
{
    /// <summary>Whether the spawning was successful.</summary>
    public bool Success { get; init; }

    /// <summary>The assigned LocalId in the zone (only on success).</summary>
    public ushort? LocalId { get; init; }

    /// <summary>The ZoneId where the entity was spawned (only on success).</summary>
    public ushort? ZoneId { get; init; }

    /// <summary>Error code on failure.</summary>
    public string? Error { get; init; }

    // ─── Factory Methods ───

    public static SpawnResult Succeeded(ushort localId, ushort zoneId)
        => new() { Success = true, LocalId = localId, ZoneId = zoneId };

    public static SpawnResult Failed(string error)
        => new() { Success = false, Error = error };
}
