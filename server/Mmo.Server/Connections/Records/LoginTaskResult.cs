using Mmo.Server.Player;
using Mmo.Shared.Connection.Enums;

namespace Mmo.Server.Connections.Records;

/// <summary>
///     Result record for an async login task operation.
/// </summary>
/// <param name="Success">Whether the login was successful.</param>
/// <param name="Player">The spawned player character (only on success).</param>
/// <param name="ErrorCode">Error Code (only on failure).</param>
/// <param name="ErrorMessage">Error message (only on failure).</param>
public record LoginTaskResult(
    bool Success,
    ServerPlayerCharacter? Player = null,
    LoginResponseErrorCode? ErrorCode = null,
    string? ErrorMessage = null
);
