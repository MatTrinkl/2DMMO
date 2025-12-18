using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Request to reconnect using a session token.
/// </summary>
[MessagePackObject]
public class ReconnectRequest : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ReconnectRequest()
    {
    }

    /// <summary>
    ///     Creates a new ReconnectRequest message.
    /// </summary>
    /// <param name="sessionToken">The session token to validate.</param>
    public ReconnectRequest(Guid sessionToken)
    {
        SessionToken = sessionToken;
    }

    /// <summary>
    ///     The session token to validate.
    /// </summary>
    [Key(1)]
    public Guid SessionToken { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ReconnectRequest;
}
