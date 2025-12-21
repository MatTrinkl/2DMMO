using Mmo.Server.AuthenticationService.Records;

namespace Mmo.Server.AuthenticationService.Interfaces;

/// <summary>
///     Service for authentication and account validation.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    ///     Validates login credentials.
    /// </summary>
    Task<AuthResult> AuthenticateAsync(string username, string password);

    /// <summary>
    ///     Validates a session token (for reconnection).
    /// </summary>
    Task<AuthResult> ValidateSessionAsync(Guid sessionToken);

    /// <summary>
    ///     Invalidates a session (logout).
    /// </summary>
    Task InvalidateSessionAsync(Guid sessionToken);
}
