using MessagePack;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Dtos;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Server_Client;

[MessagePackObject]
[NetworkMessage(MessageType.ZoneDelta)]
public class ZoneDelta : IServerMessage, ITimestampedMessage
{
    [Key(0)]
    public MessageType Type => MessageType.ZoneDelta;
    [Key(1)]
    public long Timestamp { get; init; }
    [Key(2)]
    public long ZoneId { get; init; }
    [Key(3)]
    public List<EntityDtoUnion>? SpawnedEntities { get; init; }
    [Key(4)]
    public List<long>? DespawnedEntityIds { get; init; }
    [Key(5)]
    public Dictionary<long,Position>? PositionUpdates { get; init; }
    [Key(6)]
    public Dictionary<long,int>? StateUpdates { get; init; }
    [Key(7)]
    public ContextDelta  ContextDelta { get; init; }
}
