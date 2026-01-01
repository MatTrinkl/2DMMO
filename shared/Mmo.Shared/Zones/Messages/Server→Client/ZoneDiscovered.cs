using MessagePack;
using Mmo.Shared.Entities.Dtos;
using Mmo.Shared.Entities.Interfaces.Dtos;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Interfaces.Dtos;

namespace Mmo.Shared.Zones.Messages.Server_Client;

/// <summary>
/// This Message is send to the client when he enters a zone for the first time, he will receive some XP for that.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneDiscovered)]
public class ZoneDiscovered : IServerMessage
{
    /// <summary>
    ///     Gets the message type (ZoneDelta).
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ZoneDiscovered;

    /// <summary>
    ///     Gets or sets the zone ID.
    /// </summary>
    [Key(1)]
    public ushort ZoneId { get; init; }

    /// <summary>
    ///     Gets or sets the list of newly spawned entities (nullable).
    ///     Null if no entities spawned this tick.
    /// </summary>
    [Key(2)]
    public int XpBonus { get; init; }
}
