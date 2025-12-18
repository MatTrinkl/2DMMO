using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Response to a logout request.
/// </summary>
[MessagePackObject]
public class LogoutResponse : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public LogoutResponse()
    {
    }

    /// <summary>
    ///     Creates a new LogoutResponse message.
    /// </summary>
    /// <param name="success">Whether the logout was successful.</param>
    public LogoutResponse(bool success)
    {
        Success = success;
    }

    /// <summary>
    ///     Whether the logout was successful.
    /// </summary>
    [Key(1)]
    public bool Success { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LogoutRequest;
}
