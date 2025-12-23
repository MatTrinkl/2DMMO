using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Server_Client;

/// <summary>
///     Server response to a zone transfer request.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ZoneTransferResponse)]
public record ZoneTransferResponse : INetworkMessage
{
    /// <summary>Ob der Transfer erfolgreich war.</summary>
    [Key(1)]
    public required bool Success { get; init; }

    /// <summary>Die neue Zone-ID (nur bei Erfolg).</summary>
    [Key(2)]
    public ushort? NewZoneId { get; init; }

    /// <summary>Error-Code bei Fehlschlag.</summary>
    [Key(3)]
    public string? Error { get; init; }

    /// <summary>Benutzerfreundliche Fehlermeldung. </summary>
    [Key(4)]
    public string? ErrorMessage { get; init; }

    [Key(0)]
    public MessageType Type => MessageType.ZoneTransferResponse;

    // ─── Factory Methods ───

    public static ZoneTransferResponse Succeeded(ushort newZoneId)
        => new() { Success = true, NewZoneId = newZoneId };

    public static ZoneTransferResponse Failed(string error, string? message = null)
        => new() { Success = false, Error = error, ErrorMessage = message };
}
