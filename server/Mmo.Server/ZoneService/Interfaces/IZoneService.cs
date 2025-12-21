using Mmo.Server.ZoneService.Records;

namespace Mmo.Server.ZoneService.Interfaces;

/// <summary>
///     Service for zone transfer operations.
/// </summary>
public interface IZoneService
{
    /// <summary>
    ///     Transfers a player to a different zone.
    /// </summary>
    /// <param name="persistentId">The player's persistent ID.</param>
    /// <param name="targetZoneId">The target zone ID.</param>
    /// <returns>Zone transfer result with new zone info on success.</returns>
    Task<ZoneTransferResult> TransferPlayerAsync(Guid persistentId, ushort targetZoneId);
}
