using MessagePack;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Entities.Dtos;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Interfaces;

namespace Mmo.Shared.Zones.Messages.Server_Client;

/// <summary>
///     This class updates the state of a zone. It's like a position update of all entities at ones.
///     Server → Client
///     Not complete until implemented in server and client.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneState)]
public class ZoneState : IServerMessage, ITimestampedMessage
{
    /// <summary>
    /// Static Zone configuration
    /// </summary>
    [Key(2)]
    public required ZoneConfigDto ZoneConfig { get; init; }

    /// <summary>
    /// Dynamic Zone state (without entities).
    /// </summary>
    [Key(3)]
    public required ZoneContextDto ZoneContext { get; init; }

    /// <summary>
    /// All entities in the zone.
    /// </summary>
    [Key(4)]
    public required List<EntityDtoUnion> Entities { get; init; }

    /// <summary>
    /// Which state is this message?
    /// </summary>
    [Key(5)]
    public ZoneStateType State { get; init; } = ZoneStateType.FullSync;
    /// <summary>
    /// Player character entity.
    /// </summary>
    [Key(6)] public CharacterEntityDto? MyCharacter { get; init; }
    /// <summary>
    /// If this is true an Entity Batch will follow.
    /// </summary>
    [Key(7)] public bool HasMoreEntities { get; init; }
    /// <summary>
    /// Sum of all entities in the zone.
    /// </summary>
    [Key(8)] public int TotalEntitiesCount { get; init; }

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
