namespace Mmo.Server.EntityService.Records;

/// <summary>
///     Result of entity spawn operation.
/// </summary>
public record SpawnResult(
    bool Success,
    int? LocalId = null,
    ushort? ZoneId = null,
    string? Error = null);
