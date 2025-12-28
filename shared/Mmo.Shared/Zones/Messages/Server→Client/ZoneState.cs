using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Interfaces;
using EntityDtoUnion = Mmo.Shared.Entities.Dtos.EntityDtoUnion;

namespace Mmo.Shared.Zones.Messages.Server_Client;

/// <summary>
///     This class updates the state of a zone. It's like a position update of all entities at ones.
///     Server → Client
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneState)]
public class ZoneState : IServerMessage, ITimestampedMessage
{
    [Key(2)] public required ZoneConfigDto ZoneConfig { get; init; }

    [Key(3)] public required ZoneContextDto ZoneContext { get; init; }

    [Key(4)] public required List<EntityDtoUnion> Entities { get; init; }

    /// <summary>
    ///     ID of the zone.
    /// </summary>
    [IgnoreMember]
    public ushort ZoneId => ZoneConfig.ZoneId;

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ZoneState;

    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; }
}
