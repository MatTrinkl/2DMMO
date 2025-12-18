namespace Mmo.Server.Services.Authentication;

/// <summary>
///     Service für Authentifizierung und Account-Validierung.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    ///     Validiert Login-Credentials.
    /// </summary>
    Task<AuthResult> AuthenticateAsync(string username, string password);

    /// <summary>
    ///     Validiert einen Session-Token (für Reconnect).
    /// </summary>
    Task<AuthResult> ValidateSessionAsync(Guid sessionToken);

    /// <summary>
    ///     Invalidiert eine Session (Logout).
    /// </summary>
    Task InvalidateSessionAsync(Guid sessionToken);
}
