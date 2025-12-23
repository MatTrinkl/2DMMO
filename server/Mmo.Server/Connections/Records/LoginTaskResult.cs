using Mmo.Server.PlayerService;

namespace Mmo.Server.Connections.Records;

/// <summary>
///     Result record for an async login task operation.
/// </summary>
/// <param name="Success">Whether the login was successful.</param>
/// <param name="Player">The spawned player character (only on success).</param>
/// <param name="Error">Error message (only on failure).</param>
public record LoginTaskResult(
    bool Success,
    ServerPlayerCharacter? Player = null,
    string? Error = null
);
