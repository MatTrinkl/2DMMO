using Mmo.Server.ZoneService.Interfaces;
using Mmo.Server.ZoneService.Records;
using Mmo.Shared.Core.Interfaces;

namespace Mmo.Server.ZoneService;

/// <summary>
///     Stub implementation of IZoneService.
///     TODO: Implement zone transfer logic.
/// </summary>
public class ZoneService : IZoneService
{
    private readonly ILog _log;

    public ZoneService(ILog log)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <inheritdoc />
    public Task<ZoneTransferResult> TransferPlayerAsync(Guid persistentId, ushort targetZoneId)
    {
        _log.Debug("ZoneService.TransferPlayerAsync called for {PersistentId} to zone {ZoneId}",
            persistentId, targetZoneId);

        // TODO: Implement zone transfer logic
        return Task.FromResult(new ZoneTransferResult(
            false,
            Error: "Zone transfer not yet implemented"));
    }
}
