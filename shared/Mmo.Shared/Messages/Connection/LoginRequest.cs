using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
/// This class sends a login request to the server.
/// </summary>
[MessagePackObject]
public class LoginRequest : INetworkMessage
{
    /// <summary>
    /// The constructor used bei <see cref="MessagePackSerializer"/>.
    /// </summary>
    [SerializationConstructor]
    public LoginRequest()
    {
    }
    /// <summary>
    /// Creates a new LoginRequest Message.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <param name="password">the password of the player.</param>
    public LoginRequest(string username, string password)
    {
        Username = username;
        Password = password;
    }
    /// <summary>
    /// The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LoginRequest;
    /// <summary>
    /// Username of the Player
    /// </summary>
    [Key(1)]
    public string Username { get; set; }
    /// <summary>
    /// Password of the player (later hashed).
    /// </summary>
    [Key(2)]
    public string Password { get; set; }
}
