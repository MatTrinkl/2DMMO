using MessagePack;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Server_Client;

/// <summary>
///     This class is a response to a reconnect request.
///     Sever -> Client
///     Ones per Session.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ServerSelectResponse)]
public class ServerSelectResponse : IResponseMessage<ServerSelectResponseErrorCodes>
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ServerSelectResponse;

    /// <inheritdoc/>
    [Key(1)]
    public bool Success { get; init; }

    /// <inheritdoc/>
    [Key(2)]
    public GlobalErrorCode GlobalError { get; init; }

    /// <summary>
    ///     The ErrorCode if <see cref="Success" /> is false and the attempted was not successful.
    /// </summary>
    [Key(3)]
    public ServerSelectResponseErrorCodes? ErrorCode { get; init; }

    /// <inheritdoc/>
    [Key(4)]
    public string? ErrorMessage { get; init; }

    /// <summary>
    ///    The realm the client is switching to.
    /// </summary>
    [Key(5)]
    public int? RealmId { get; set; }

    /// <summary>
    ///     Name of the new realm.
    /// </summary>
    [Key(6)]
    public string? RealmName { get; set; }

    /// <summary>
    ///     Token for the transfer. (Validation on the new realm)
    /// </summary>
    [Key(7)]
    public string? TransferToken { get; set; }
}
