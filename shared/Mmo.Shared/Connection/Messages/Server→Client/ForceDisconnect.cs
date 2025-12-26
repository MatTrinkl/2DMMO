using MessagePack;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Server_Client;

/// <summary>
///     This class forces a client to disconnect.
///     This is only to inform the client why he is disconnecting. The initial TCP disconnect is coming from the server.
///     Server -> Client
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ForceDisconnect)]
public class ForceDisconnect : IServerMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ForceDisconnect;

    /// <summary>
    /// The reason the client needs to disconnect.
    /// </summary>
    [Key(1)]
    public DisconnectReason Reason { get; init; }

    /// <summary>
    /// A Message explaining the reason.
    /// </summary>
    [Key(2)]
    public string? Message { get; set; }

    /// <summary>
    /// Is the client allowed to reconnect.
    /// </summary>
    [IgnoreMember]
    public bool CanReconnect => Reason is not (DisconnectReason.Banned or DisconnectReason.VersionMismatch);

    /// <summary>
    /// The amount of seconds until he can reconnect. (Timeout timer)
    /// </summary>
    [Key(3)]
    public int? ReconnectDelay { get; set; }
}
