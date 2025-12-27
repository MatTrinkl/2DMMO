using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Messages.Server_Client;

/// <summary>
///     This class updates the state of a zone. It's like a position update of all entities at ones.
///     Server → Client
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneState)]
public class ZoneState : IServerMessage, ITimestampedMessage
{
    /// <summary>
    ///     ID of the zone.
    /// </summary>
    [Key(2)]
    public ushort ZoneId { get; set; }

    [Key(3)] public string ZoneName { get; set; } = "";
    [Key(4)] public ZoneFlags ZoneFlags { get; set; }
    [Key(5)] public WeatherType WeatherType { get; set; }
    [Key(6)] public float TimeOfDay { get; set; }

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
