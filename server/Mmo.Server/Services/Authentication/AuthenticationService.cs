using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Services.Authentication;

/// <summary>
///     Concrete implementation of the Authentication service.
///     TODO: Replace with real DB access later.
/// </summary>
public class AuthenticationService(ILog log) : IAuthenticationService
{
    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        // TODO: Implement real DB validation
        // - Compare password hash
        // - Check account status (banned, suspended, etc.)
        // - Update last login

        await Task.Delay(1); // Simulate async DB call

        // PROTOTYPE: Akzeptiere jeden Login
        log.Info("Authentication successful for user: {Username}", username);

        return new AuthResult(
            true,
            Guid.NewGuid(), // TODO:  Echte Account-ID aus DB
            username,
            AccountFlags.None
        );
    }

    public async Task<AuthResult> ValidateSessionAsync(Guid sessionToken)
    {
        // TODO: Session aus Redis/DB validieren
        await Task.Delay(1);

        return new AuthResult(false, Error: "Session validation not implemented");
    }

    public async Task InvalidateSessionAsync(Guid sessionToken)
    {
        // TODO: Session in Redis/DB invalidieren
        await Task.Delay(1);

        log.Info("Session invalidated:  {SessionToken}", sessionToken);
    }
}
