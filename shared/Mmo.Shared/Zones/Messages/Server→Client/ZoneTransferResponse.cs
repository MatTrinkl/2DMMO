using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Zones.Messages.Server_Client;


/// <summary>
///     Server response to a zone transfer request.
/// </summary>
public record ZoneTransferResponse :  INetworkMessage
{
    public MessageType Type => MessageType.ZoneTransferResponse;

    /// <summary>Ob der Transfer erfolgreich war.</summary>
    public required bool Success { get; init; }

    /// <summary>Die neue Zone-ID (nur bei Erfolg).</summary>
    public ushort? NewZoneId { get; init; }

    /// <summary>Error-Code bei Fehlschlag.</summary>
    public string? Error { get; init; }

    /// <summary>Benutzerfreundliche Fehlermeldung. </summary>
    public string? ErrorMessage { get; init; }

    // ─── Factory Methods ───

    public static ZoneTransferResponse Succeeded(ushort newZoneId)
        => new() { Success = true, NewZoneId = newZoneId };

    public static ZoneTransferResponse Failed(string error, string?  message = null)
        => new() { Success = false, Error = error, ErrorMessage = message };
}
