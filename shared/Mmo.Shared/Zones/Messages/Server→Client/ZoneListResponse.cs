using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Interfaces.Generated;

namespace Mmo.Shared.Zones.Messages.Server_Client;

/// <summary>
///     Server response to a zone transfer request.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneListResponse)]
public record ZoneListResponse : IResponseMessage<ZoneListResponseErrorCode>
{
    /// <inheritdoc/>
    [Key(0)]
    public MessageType Type => MessageType.ZoneListResponse;

    /// <inheritdoc/>
    [Key(1)]
    public GlobalErrorCode GlobalError { get; init; }

    /// <inheritdoc/>
    [Key(2)]
    public string? ErrorMessage { get; init; }

    /// <inheritdoc/>
    [Key(3)]
    public ZoneListResponseErrorCode? ErrorCode { get; init; }

    /// <summary>Whether the transfer was successful.</summary>
    [Key(4)]
    public required bool Success { get; init; }

    /// <summary>The new zone ID (only on success).</summary>
    [Key(5)]
    public List<ZoneListEntry> Zones { get; init; } = [];
}
