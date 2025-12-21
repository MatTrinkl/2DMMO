using Mmo.Shared.Account.Enums;
using Mmo.Shared.Enums;

namespace Mmo.Server.ServiceAuthentication.Records;

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
