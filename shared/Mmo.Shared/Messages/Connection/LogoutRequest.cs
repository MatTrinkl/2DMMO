using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
/// This class is sent when a logout is requested by a player.
/// </summary>
[MessagePackObject]
public class LogoutRequest : INetworkMessage
{
    /// <summary>
    /// The constructor used bei <see cref="MessagePackSerializer"/>.
    /// </summary>
    [SerializationConstructor]
    public LogoutRequest()
    {
    }
    /// <summary>
    /// Creates a new LogoutRequest Message.
    /// </summary>
    /// <param name="playerId">The player which request the logout.</param>
    public LogoutRequest(Guid playerId)
    {
        PlayerId = playerId;
    }
    /// <summary>
    /// The Message Type of this Message.
    /// </summary>
    [Key(0)] public MessageType Type => MessageType.LogoutRequest;
    /// <summary>
    /// The player who requests the logout.
    /// </summary>
    [Key(1)] public Guid PlayerId { get; set; }
}
