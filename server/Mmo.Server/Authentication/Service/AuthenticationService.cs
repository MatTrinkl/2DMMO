using Mmo.Shared.Authentification.Interfaces;
using Mmo.Shared.Authentification.Records;
using Mmo.Shared.Connection.Messages;
using Mmo.Shared.Core.Interfaces;

namespace Mmo.Server.AuthenticationService;

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
            username
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

    private static string? ValidateLoginInput(string username)
    {
        //Todo:implement
        if (string.IsNullOrWhiteSpace(username))
            return "Username cannot be empty";

        if (username.Length < 3)
            return "Username must be at least 3 characters";

        if (username.Length > 20)
            return "Username must be at most 20 characters";

        // TODO: Regex für erlaubte Zeichen
        // if (!Regex.IsMatch(request.Username, "^[a-zA-Z0-9_]+$"))
        //     return "Username can only contain letters, numbers, and underscores";

        // TODO: Password-Validierung
        // if (string.IsNullOrWhiteSpace(request.Password))
        //     return "Password cannot be empty";

        return null;
    }
}
