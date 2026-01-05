using MessagePack;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Server_Client;

/// <summary>
///     This class is a response to an Encryption handshake.
///     Sever -> Client
///     Ones per Session.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.EncryptionHandshakeResponse)]
public class EncryptionHandshakeResponse : IResponseMessage<EncryptionHandshakeResponseErrorCode>
{
    /// <summary>
    /// Servers ECDH Public Key
    /// </summary>
    [Key(5)]
    public byte[]? ServerPublicKey { get; init; }
    /// <summary>
    /// Selected Cipher suite
    /// </summary>
    [Key(6)] public ushort? SelectedCipherSuite { get; init; }
    /// <summary>
    /// 32-Byte Random for key Derivation
    /// </summary>
    [Key(7)] public byte[]? ServerRandom { get; init; } = new byte[32];
    /// <summary>
    /// Used protocol version.
    /// </summary>
    [Key(8)] public ushort? ProtocolVersion { get; init; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
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
    public EncryptionHandshakeResponseErrorCode? ErrorCode { get; init; }

    /// <inheritdoc />
    [Key(4)]
    public string? ErrorMessage { get; init; }
}
