using MessagePack;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Server_Client;

/// <summary>
///     This class is a response to a CompressionToggle Request.
///     Sever -> Client
///     Ones per Session.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CompressionToggleResponse)]
public class CompressionToggleResponse : IResponseMessage<CompressionToggleResponseErrorCode>
{
    /// <summary>
    /// Level of compression
    /// </summary>
    [Key(5)]
    public int Level { get; init; }

    /// <summary>
    /// Active Algorithm
    /// </summary>
    [Key(6)]
    public CompressionAlgorithm? Algorithm { get; init; }

    /// <summary>
    /// Efficient minimum size.
    /// </summary>
    [Key(7)]
    public ushort? MinMessageSize { get; init; }

    /// <summary>
    /// Is compression active?
    /// </summary>
    [IgnoreMember]
    public bool? Enabled => Algorithm is not null and not CompressionAlgorithm.None;

    /// <inheritdoc />
    [Key(0)]
    public MessageType Type => MessageType.EncryptionHandshakeResponse;

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
    public CompressionToggleResponseErrorCode? ErrorCode { get; init; }

    /// <inheritdoc />
    [Key(4)]
    public string? ErrorMessage { get; init; }
}
