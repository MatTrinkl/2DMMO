using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     This class is a response to a login request.
/// </summary>
[MessagePackObject]
public class LoginResponse : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public LoginResponse()
    {
    }

    /// <summary>
    ///     Creates a new Login Response Message.
    /// </summary>
    /// <param name="success">True if the login was successful.</param>
    /// <param name="playerId">The Player who logs in.</param>
    /// <param name="zoneId">The id of the zone where the player logs in.</param>
    /// <param name="errorMessage">The Error Message when the attempted was not successful.</param>
    public LoginResponse(bool success, Guid playerId, ushort zoneId, string? errorMessage)
    {
        Success = success;
        PlayerId = playerId;
        ZoneId = zoneId;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    ///     True if the login attempted was successful.
    /// </summary>
    [Key(1)]
    public bool Success { get; set; }

    /// <summary>
    ///     The Player who try to connect. (and gives him his Guid for this session).
    /// </summary>
    [Key(2)]
    public Guid PlayerId { get; set; }

    /// <summary>
    ///     The id of the zone where the player logs in.
    /// </summary>
    [Key(3)]
    public ushort ZoneId { get; set; }

    /// <summary>
    ///     The errormessage if <see cref="Success" /> is false and the attempted was not successful.
    /// </summary>
    [Key(4)]
    public string? ErrorMessage { get; set; }


    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LoginResponse;
}
