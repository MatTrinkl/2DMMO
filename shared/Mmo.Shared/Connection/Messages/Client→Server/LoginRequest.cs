using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Client_Server;

/// <summary>
///     This class sends a login request to the server.
///     Client -> Server
///     Ones per Session.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.LoginRequest)]
public class LoginRequest : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LoginRequest;

    /// <summary>
    ///     Username of the Player
    /// </summary>
    [Key(1)]
    public string Username { get; set; } = "";

    /// <summary>
    ///     Password of the player (later hashed).
    /// </summary>
    [Key(2)]
    public string Password { get; set; } = "";

    /// <summary>
    /// Currently Placeholder. Todo: Upgrade to real version checking.
    /// </summary>
    [Key(3)]
    public string ClientVersion { get; set; } = "a0.0.1";

    /// <summary>
    /// Not implemented yet. Todo: Use HardwareId as a Anti Cheat mechanic.
    /// </summary>
    [Key(4)]
    public string HardwareId { get; set; } = "";
}
