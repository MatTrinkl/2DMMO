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
[NetworkMessage(MessageType.ReconnectResponse)]
public class ReconnectResponse : IResponseMessage<ReconnectResponseErrorCode>
{
    /// <summary>
    ///     The last zone where the player entity was active.
    /// </summary>
    [Key(5)]
    public ushort? ZoneId { get; set; }

    /// <summary>
    ///     Last sequence of the server.
    /// </summary>
    [Key(6)]
    public uint? LastServerSequence { get; set; }

    /// <summary>
    ///     Is a full resync required or only a delta.
    /// </summary>
    [Key(7)]
    public bool? ResyncRequired { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ReconnectResponse;


    /// <inheritdoc />
    [Key(1)]
    public bool Success { get; init; }

    /// <inheritdoc />
    [Key(2)]
    public GlobalErrorCode GlobalError { get; init; }

    /// <summary>
    ///     The ErrorCode if <see cref="Success" /> is false and the attempted was not successful.
    /// </summary>
    [Key(3)]
    public ReconnectResponseErrorCode? ErrorCode { get; init; }

    /// <inheritdoc />
    [Key(4)]
    public string? ErrorMessage { get; init; }
}
