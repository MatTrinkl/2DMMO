using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
/// This class is a response to a login request.
/// </summary>
[MessagePackObject]
public class LoginResponse : INetworkMessage
{
    /// <summary>
    /// The constructor used bei <see cref="MessagePackSerializer"/>.
    /// </summary>
    [SerializationConstructor]
    public LoginResponse()
    {
    }

    /// <summary>
    /// Creates a new Login Response Message.
    /// </summary>
    /// <param name="success">True if the login was successful.</param>
    /// <param name="playerId">The Player who logs in.</param>
    /// <param name="errorMessage">The Error Message when the attempted was not successful.</param>
    public LoginResponse(bool success, Guid playerId, string? errorMessage)
    {
        Success = success;
        PlayerId = playerId;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LoginResponse;

    /// <summary>
    /// True if the login attempted was successful.
    /// </summary>
    [Key(1)]
    public bool Success { get; set; }

    /// <summary>
    /// The Player who try to connect. (and gives him his Guid for this session).
    /// </summary>
    [Key(2)]
    public Guid PlayerId { get; set; }

    /// <summary>
    /// The errormessage if <see cref="Success"/> is false and the attempted was not successful.
    /// </summary>
    [Key(3)] public string? ErrorMessage { get; set; }
}
