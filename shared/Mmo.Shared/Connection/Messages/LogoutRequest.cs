using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages;

/// <summary>
///     This class is sent when a logout is requested by a player.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.LogoutRequest)]
public class LogoutRequest : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public LogoutRequest()
    {
    }

    /// <summary>
    ///     Creates a new LogoutRequest Message.
    /// </summary>
    /// <param name="playerId">The player which request the logout.</param>
    public LogoutRequest(Guid playerId)
    {
        PlayerId = playerId;
    }

    /// <summary>
    ///     The player who requests the logout.
    /// </summary>
    [Key(1)]
    public Guid PlayerId { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.LogoutRequest;
}
