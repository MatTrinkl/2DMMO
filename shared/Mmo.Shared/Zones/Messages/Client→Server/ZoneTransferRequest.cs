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
[NetworkMessage(MessageType.ZoneTransferRequest)]
public record ZoneTransferRequest : INetworkMessage
{
    /// <summary>Ziel-Zone ID. </summary>
    [Key(1)]
    public required ushort TargetZoneId { get; init; }

    /// <summary>Transfer-Typ (für Validierung/Logging).</summary>
    [Key(2)]
    public required ZoneTransferType TransferType { get; init; }

    /// <summary>Optionale Ziel-Position (z.B. bei Portal).</summary>
    [Key(3)]
    public Position? TargetPosition { get; init; }

    [Key(0)]
    public MessageType Type => MessageType.ZoneTransferRequest;
}
