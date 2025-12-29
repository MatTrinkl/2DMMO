using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Server_Broadcast;

/// <summary>
///     This class is send to all clients to inform them that a player has left a zone. Todo: ZoneId
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.CharacterLeftZone)]
public class CharacterLeftZone : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.CharacterLeftZone;
    [Key(1)] public Guid PlayerId { get; init; }
}
