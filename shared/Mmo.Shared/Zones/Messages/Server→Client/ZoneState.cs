using MessagePack;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Server_Client;

/// <summary>
///     This class updates the state of a zone. It's like a position update of all entities at ones.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneState)]
public class ZoneState : IServerMessage, ITimestampedMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ZoneState;

    /// <summary>
    ///     ID of the zone. WIP!
    /// </summary>
    [Key(2)]
    public ushort ZoneId { get; set; }

    /// <summary>
    ///     List of all Entities in this zone.
    /// </summary>
    [Key(3)]
    public List<IEntity> Entities { get; set; } = new();


    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; }
}
