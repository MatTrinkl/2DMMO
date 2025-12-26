using Mmo.Shared.Core.Constants;

namespace Mmo.Shared.Messaging.Enums;

/// <summary>
/// Global error codes for all responses.
/// Are checked first, when set then ignore specific ErrorCode.
/// </summary>
public enum GlobalErrorCode : byte
{
    /// <summary>No Error set.</summary>
    None = 0,

    // ═══ Session/Auth (1-19) ═══

    /// <summary>Session expired - back to log in. </summary>
    SessionExpired = 1,

    /// <summary>Not authenticated on the server. </summary>
    NotAuthenticated = 2,

    /// <summary>No Permission to do this action.</summary>
    InsufficientPermissions = 3,

    // ═══ Server-Status (20-39) ═══

    /// <summary>Server is shutting down. </summary>
    ServerShuttingDown = 20,

    /// <summary>Server is overloaded.</summary>
    ServerOverloaded = 21,

    /// <summary>Server is in maintenance mode.</summary>
    MaintenanceMode = 22,

    // ═══ Rate Limiting (40-59) ═══

    /// <summary>To many requests. More Information under: <see cref="RateLimits"/>.</summary>
    RateLimited = 40,

    // ═══ Allgemeine Fehler (60-79) ═══

    /// <summary>Internal Server Error.</summary>
    InternalServerError = 60,

    /// <summary>Database Error.</summary>
    DatabaseError = 61,

    /// <summary>Validation failed.</summary>
    ValidationError = 62
}
