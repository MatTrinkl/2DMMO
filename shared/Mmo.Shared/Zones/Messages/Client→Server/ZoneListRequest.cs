using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Client_Server;

/// <summary>
///     Client requests a zone transfer (Portal, Teleport, etc.).
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneListRequest)]
public record ZoneListRequest : IClientMessage
{
    /// <summary>
    ///     Optional: Interacted Entity (e.g. Portal, Flight master, ...)
    ///     Null = Return all discovered Zones (MapUI)
    ///     Server is validating rights and type.
    /// </summary>
    [Key(1)]
    public Guid? InteractionEntityId { get; set; }

    /// <inheritdoc />
    [Key(0)]
    public MessageType Type => MessageType.ZoneListRequest;
}
