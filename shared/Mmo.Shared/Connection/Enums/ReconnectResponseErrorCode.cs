using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;

namespace Mmo.Shared.Connection.Enums;

/// <summary>
///     This ErrorCode is sent in <see cref="ReconnectResponse" /> when a <see cref="ReconnectRequest" /> is denied.
/// </summary>
public enum ReconnectResponseErrorCode : byte
{
    /// <summary>
    ///     Default fallback.
    /// </summary>
    UnknownError = 0,

    /// <summary>
    ///     Is sent when the token is no longer active.
    /// </summary>
    SessionExpired = 1,

    /// <summary>
    ///     Is sent when the token is not valid
    /// </summary>
    SessionNotFound = 2,

    /// <summary>
    ///     Is sent when the time since the disconnect is longer than 30 seconds.
    /// </summary>
    ReconnectWindowClosed = 3,

    /// <summary>
    ///     Is sent when the client version is different to the original login attempted.
    /// </summary>
    DifferentClient = 4
}
