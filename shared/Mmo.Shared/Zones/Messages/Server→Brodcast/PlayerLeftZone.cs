using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Server_Brodcast;

/// <summary>
///     This class is send to all clients to inform them that a player has left a zone. Todo: ZoneId
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.PlayerLeftZone)]
public class PlayerLeftZone : INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.PlayerLeftZone;
}
