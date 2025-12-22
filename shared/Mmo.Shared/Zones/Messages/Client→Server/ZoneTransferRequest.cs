using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Messages.Client_Server;

/// <summary>
///     Client requests a zone transfer (Portal, Teleport, etc.).
/// </summary>
public record ZoneTransferRequest : INetworkMessage
{
    /// <summary>Ziel-Zone ID. </summary>
    public required ushort TargetZoneId { get; init; }

    /// <summary>Transfer-Typ (für Validierung/Logging).</summary>
    public required ZoneTransferType TransferType { get; init; }

    /// <summary>Optionale Ziel-Position (z.B. bei Portal).</summary>
    public Position? TargetPosition { get; init; }

    public MessageType Type => MessageType.ZoneTransferRequest;
}
