using MessagePack;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Networking;

namespace Mmo.Shared.Connection.Messages.Server_Client;

/// <summary>
///     This class is a response to a login request.
///     Sever -> Client
///     Ones per Session.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.LoginResponse)]
public class LoginResponse : ITimestampedMessage, IResponseMessage<LoginResponseErrorCode>
{
    /// <summary>
    ///     The Token is used when the player needs to reconnect.
    ///     This is only set when <see cref="Success" /> is true.
    /// </summary>
    [Key(5)]
    public string? SessionToken { get; init; }

    /// <summary>
    ///     The ID of the account of the player from the Server DB.
    ///     This is only set when <see cref="Success" /> is true.
    /// </summary>
    [Key(6)]
    public Guid? AccountId { get; init; }

    /// <summary>
    ///     The Display Name of the account of the player from the Server DB. Can be different from the
    ///     <see cref="LoginRequest.Username" />.
    ///     This is only set when <see cref="Success" /> is true.
    /// </summary>
    [Key(7)]
    public string? AccountName { get; init; }

    /// <summary>
    ///     Has this account premium status. Todo: Upgrade this to a status enum.
    ///     This is only set when <see cref="Success" /> is true.
    /// </summary>
    [Key(8)]
    public bool? IsPremium { get; init; }

    /// <summary>
    ///     True if the login attempted was successful.
    /// </summary>
    [Key(1)]
    public bool Success { get; init; }

    /// <summary>
    ///     The global error when <see cref="Success" /> is false.
    /// </summary>
    [Key(2)]
    public GlobalErrorCode GlobalError { get; init; }

    /// <summary>
    ///     The specific error when <see cref="Success" /> is false and <see cref="GlobalError" /> =
    ///     <see cref="GlobalErrorCode.None" />.
    /// </summary>
    [Key(3)]
    public LoginResponseErrorCode? ErrorCode { get; init; }

    /// <inheritdoc />
    [Key(4)]
    public string? ErrorMessage { get; init; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LoginResponse;

    /// <summary>
    ///     The time when this Response is created on the server. This uses <see cref="NetworkTime" />.
    ///     This value
    /// </summary>
    [Key(9)]
    public long Timestamp { get; init; } = NetworkTime.Now;
}
