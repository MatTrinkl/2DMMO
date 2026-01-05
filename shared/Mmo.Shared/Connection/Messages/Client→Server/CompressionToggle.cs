using System.IO.Compression;
using MessagePack;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Client_Server;

/// <summary>
///     This class sends a request to toggle compression.
///     Client -> Server
///     Todo: Phase 3 implementation, currently a placeholder
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CompressionToggle)]
public class CompressionToggle : IClientMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CompressionToggle;

    /// <summary>
    /// Is an algorithm in place.
    /// </summary>
    [IgnoreMember] public bool Enabled => CompressionAlgorithm is not CompressionAlgorithm.None;
    /// <summary>
    /// The level of compression (0-9, 0 = fastest, 9 = best)
    /// </summary>
    [Key(2)] public CompressionLevel CompressionLevel { get; set; }
    /// <summary>
    /// Which algorithm is used.
    /// </summary>
    [Key(3)] public CompressionAlgorithm CompressionAlgorithm { get; set; }
    /// <summary>
    /// The minimum size of a message for compression.
    /// </summary>
    [Key(4)] public ushort MinMessageSize { get; set; }
}
