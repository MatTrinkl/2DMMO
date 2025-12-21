using Mmo.Shared.Account.Enums;

namespace Mmo.Server.AuthenticationService.Records;

/// <summary>
///     Ergebnis einer Authentifizierung.
/// </summary>
public record AuthResult(
    bool Success,
    Guid? AccountId = null,
    string? Username = null,
    AccountFlags Flags = AccountFlags.None,
    string? Error = null
);
