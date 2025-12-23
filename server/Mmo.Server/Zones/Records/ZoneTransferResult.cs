using Mmo.Shared.Core.Records;

namespace Mmo.Server.Zones.Records;

/// <summary>
///     Result of a zone transfer operation.
/// </summary>
public record ZoneTransferResult
{
    /// <summary>Whether the transfer was successful.</summary>
    public bool Success { get; init; }

    /// <summary>The new zone ID (only on success).</summary>
    public ushort? NewZoneId { get; init; }

    /// <summary>The spawn position in the new zone.</summary>
    public Position? SpawnPosition { get; init; }

    /// <summary>Error code on failure.</summary>
    public string? Error { get; init; }

    // ─── Factory Methods ───

    public static ZoneTransferResult Succeeded(ushort newZoneId, Position spawnPosition)
        => new() { Success = true, NewZoneId = newZoneId, SpawnPosition = spawnPosition };

    public static ZoneTransferResult Failed(string error)
        => new() { Success = false, Error = error };
}
