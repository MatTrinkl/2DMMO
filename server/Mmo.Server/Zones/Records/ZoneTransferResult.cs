using Mmo.Shared.Core.Records;

namespace Mmo.Server.Zones.Records;

/// <summary>
///     Result of a zone transfer operation.
/// </summary>
public record ZoneTransferResult
{
    /// <summary>Ob der Transfer erfolgreich war. </summary>
    public bool Success { get; init; }

    /// <summary>Die neue Zone-ID (nur bei Erfolg).</summary>
    public ushort? NewZoneId { get; init; }

    /// <summary>Die Spawn-Position in der neuen Zone. </summary>
    public Position? SpawnPosition { get; init; }

    /// <summary>Error-Code bei Fehlschlag.</summary>
    public string? Error { get; init; }

    // ─── Factory Methods ───

    public static ZoneTransferResult Succeeded(ushort newZoneId, Position spawnPosition)
        => new() { Success = true, NewZoneId = newZoneId, SpawnPosition = spawnPosition };

    public static ZoneTransferResult Failed(string error)
        => new() { Success = false, Error = error };
}
