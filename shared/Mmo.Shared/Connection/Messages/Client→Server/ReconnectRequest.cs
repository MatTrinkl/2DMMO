using MessagePack;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Client_Server;

/// <summary>
///     This class sends a reconnect request to the server.
///     Client -> Server
///     Ones per Session.
///     Response: <see cref="ReconnectResponse"/> with Success=true or ErrorCode.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ReconnectRequest)]
public class ReconnectRequest : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ReconnectRequest;

    /// <summary>
    ///     SessionToken of the Player
    ///     This one is active for 30 days.
    /// </summary>
    [Key(1)]
    public string SessionToken { get; set; } = "";

    /// <summary>
    ///     Last recived <see cref="Heartbeat.SequenceNumber"/>. Its safed with the session token locally.
    /// </summary>
    [Key(2)]
    public uint LastSequenceNumber { get; set; }

    /// <summary>
    /// Currently Placeholder. Todo: Upgrade to real version checking.
    /// </summary>
    [Key(3)]
    public string ClientVersion { get; set; } = "a0.0.1";
}
