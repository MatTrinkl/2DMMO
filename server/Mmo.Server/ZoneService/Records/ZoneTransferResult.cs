using Mmo.Shared.Core.Records;

namespace Mmo.Server.ZoneService.Records;

/// <summary>
///     Result of zone transfer operation.
/// </summary>
public record ZoneTransferResult(
    bool Success,
    ushort? NewZoneId = null,
    Position? SpawnPosition = null,
    string? Error = null);
