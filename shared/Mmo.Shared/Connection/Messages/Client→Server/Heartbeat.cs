using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Networking;

namespace Mmo.Shared.Connection.Messages.Client_Server;

/// <summary>
///     This class indicates a heartbeat of a client. Should be sent every 5 second.
///     Client->Server
///     Response is a Pong (901)
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.Heartbeat)]
public class Heartbeat : IClientMessage, ITimestampedMessage
{
    /// <summary>
    ///     The concurrent number of Heartbeats send to the server. With this the server can monitor how long is between to
    ///     beats and if packages are lost.
    /// </summary>
    [Key(2)]
    public uint SequenceNumber { get; init; } = 0;

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.Heartbeat;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; } = NetworkTime.Now;
}
