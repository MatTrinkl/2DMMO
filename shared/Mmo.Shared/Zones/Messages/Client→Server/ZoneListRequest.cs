using MessagePack;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Messages.Client_Server;

/// <summary>
///     Client requests a zone transfer (Portal, Teleport, etc.).
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneListRequest)]
public record ZoneListRequest : IClientMessage
{
    /// <inheritdoc/>
    [Key(0)] public MessageType Type => MessageType.ZoneListRequest;

    /// <summary>
    /// Optional: Interacted Entity (e.g. Portal, Flight master, ...)
    /// Null = Return all discovered Zones (MapUI)
    /// Server is validating rights and type.
    /// </summary>
    [Key(1)] public Guid? InteractionEntityId { get; set; }
}
