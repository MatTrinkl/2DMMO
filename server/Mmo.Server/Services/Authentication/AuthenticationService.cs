using Mmo. Shared.Enums;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Services.Authentication;

/// <summary>
///     Konkrete Implementation des Authentication-Service.
///     TODO: Später mit echtem DB-Zugriff ersetzen.
/// </summary>
public class AuthenticationService(ILog log) : IAuthenticationService
{
    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        // TODO: Echte DB-Validierung implementieren
        // - Password-Hash vergleichen
        // - Account-Status prüfen (banned, suspended, etc.)
        // - Last-Login aktualisieren

        await Task. Delay(1); // Simulate async DB call

        // PROTOTYPE: Akzeptiere jeden Login
        log.Info("Authentication successful for user: {Username}", username);

        return new AuthResult(
            Success: true,
            AccountId:  Guid.NewGuid(),  // TODO:  Echte Account-ID aus DB
            Username: username,
            Flags: AccountFlags.None
        );
    }

    public async Task<AuthResult> ValidateSessionAsync(Guid sessionToken)
    {
        // TODO: Session aus Redis/DB validieren
        await Task.Delay(1);

        return new AuthResult(Success: false, Error: "Session validation not implemented");
    }

    public async Task InvalidateSessionAsync(Guid sessionToken)
    {
        // TODO: Session in Redis/DB invalidieren
        await Task.Delay(1);

        log.Info("Session invalidated:  {SessionToken}", sessionToken);
    }
}
