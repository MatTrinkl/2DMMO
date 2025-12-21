namespace Mmo.Shared.Entities.Records;

/// <summary>
///     Result of an entity spawn operation.
/// </summary>
public record SpawnResult
{
    /// <summary>Ob das Spawning erfolgreich war. </summary>
    public bool Success { get; init; }

    /// <summary>Die zugewiesene LocalId in der Zone (nur bei Erfolg).</summary>
    public ushort? LocalId { get; init; }

    /// <summary>Die ZoneId wo die Entity gespawnt wurde (nur bei Erfolg).</summary>
    public ushort?  ZoneId { get; init; }

    /// <summary>Error-Code bei Fehlschlag. </summary>
    public string? Error { get; init; }

    // ─── Factory Methods ───

    public static SpawnResult Succeeded(ushort localId, ushort zoneId)
        => new() { Success = true, LocalId = localId, ZoneId = zoneId };

    public static SpawnResult Failed(string error)
        => new() { Success = false, Error = error };
}
