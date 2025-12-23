using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages;

/// <summary>
///     This class sends a login request to the server.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.LoginRequest)]
public class LoginRequest : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public LoginRequest()
    {
    }

    /// <summary>
    ///     Creates a new LoginRequest Message.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <param name="password">the password of the player.</param>
    public LoginRequest(string username, string password)
    {
        Username = username;
        Password = password;
    }

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
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LoginRequest;
}
