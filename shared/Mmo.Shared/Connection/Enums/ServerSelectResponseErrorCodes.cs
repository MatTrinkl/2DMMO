using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;

namespace Mmo.Shared.Connection.Enums;

/// <summary>
///     This ErrorCode is sent in <see cref="ServerSelectResponse" /> when a <see cref="ServerSelectRequest" /> is denied.
/// </summary>
public enum ServerSelectResponseErrorCodes
{
    /// <summary>
    ///     The requested realm is not found.
    /// </summary>
    RealmNotFound,

    /// <summary>
    ///     The requested realm is offline.
    /// </summary>
    RealmOffline,

    /// <summary>
    ///     The requested realm is full.
    /// </summary>
    RealmFull,

    /// <summary>
    ///     The requested realm is locked.
    /// </summary>
    RealmLocked
}
