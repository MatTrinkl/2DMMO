using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Core.Constants;

namespace Mmo.Shared.Connection.Enums;

/// <summary>
///     This ErrorCode is sent in <see cref="LoginResponse" /> when a <see cref="LoginRequest" /> is denied.
/// </summary>
public enum LoginResponseErrorCode : byte
{
    /// <summary>
    ///     Is sent when the <see cref="LoginRequest.Username" /> or <see cref="LoginRequest.Password" /> is incorrect.
    /// </summary>
    InvalidCredentials = 0,

    /// <summary>
    ///     Is sent when the requested account is flagged as banned.
    /// </summary>
    AccountBanned = 1,

    /// <summary>
    ///     Is sent when <see cref="LoginRequest.ClientVersion" /> doesn't match the accepted server version of the game.
    /// </summary>
    VersionMismatch = 2,

    /// <summary>
    ///     Is sent when the server is full.
    /// </summary>
    ServerFull = 3,

    /// <summary>
    ///     Is sent when the account is already logged in. Todo: Anti cheat system needs to handle this. (Maybe kick the old or
    ///     denie the new)
    /// </summary>
    AlreadyLoggedIn = 4,

    /// <summary>
    ///     This is sent when the <see cref="RateLimits.LoginRequestPerMinute" /> of client requests is reached.
    /// </summary>
    RateLimitExceeded = 5
}
