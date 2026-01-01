using MessagePack;
using Mmo.Shared.Character.Messages.Server_Client;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Messages.Client_Server;

/// <summary>
///     This is sent after the Zone is finished loading in the client.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneLoadedAck)]
public record ZoneLoadedAck : IClientMessage
{
    /// <inheritdoc/>
    [Key(0)]
    public MessageType Type => MessageType.ZoneLoadedAck;

    /// <summary>
    /// ID of the Zone which the character is spawning in. (For validation)
    /// </summary>
    [Key(1)]
    public ushort ZoneId { get; set; }

    /// <summary>
    /// The time the loading needed to process. (For metrics)
    /// </summary>
    [Key(2)]
    public int? LoadTimeMs { get; set; }
}
