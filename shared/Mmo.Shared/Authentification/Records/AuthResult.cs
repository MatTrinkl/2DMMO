using Mmo.Shared.Account.Enums;
using Mmo.Shared.Connection.Enums;

namespace Mmo.Shared.Authentification.Records;

/// <summary>
///     Result of an authentification request.
/// </summary>
public record AuthResult(
    bool Success,
    Guid? AccountId = null,
    string? Username = null,
    AccountFlags Flags = AccountFlags.None,
    LoginResponseErrorCode? ErrorCode = null,
    string? ErrorMessage = null
);
