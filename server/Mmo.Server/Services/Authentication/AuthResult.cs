using Mmo.Shared.Enums;

namespace Mmo.Server.Services.Authentication;

/// <summary>
///     Ergebnis einer Authentifizierung.
/// </summary>
public record AuthResult(
    bool Success,
    Guid?  AccountId = null,
    string? Username = null,
    AccountFlags Flags = AccountFlags.None,
    string? Error = null
);
