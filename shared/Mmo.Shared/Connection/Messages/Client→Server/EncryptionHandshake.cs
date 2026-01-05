using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Networking;

namespace Mmo.Shared.Connection.Messages.Client_Server;

/// <summary>
///     This class opens an EncryptionHandshake with the server. Used later in TCP with TLS (Phase 3)
///     Client->Server
///     Todo: Implement this system.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.EncryptionHandshake)]
public class EncryptionHandshake : IClientMessage, ITimestampedMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.EncryptionHandshake;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; } = NetworkTime.Now;
    /// <summary>
    /// Client ECDH Public Key.
    /// </summary>
    [Key(2)] public byte[] ClientPublicKey { get; init; } = Array.Empty<byte>();
    /// <summary>
    /// Supported cipher suites.
    /// </summary>
    [Key(3)] public ushort[] SupportedCipherSuites { get; init; } = Array.Empty<ushort>();
    /// <summary>
    /// 32-byte Random for key derivation.
    /// </summary>
    [Key(4)] public byte[] ClientRandom { get; init; } = new byte[32];
}
