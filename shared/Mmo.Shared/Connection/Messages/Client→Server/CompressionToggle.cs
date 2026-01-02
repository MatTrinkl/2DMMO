using MessagePack;
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
}
