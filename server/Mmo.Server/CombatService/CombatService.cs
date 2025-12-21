using Mmo.Server.CombatService.Interfaces;
using Mmo.Shared.Core.Interfaces;

namespace Mmo.Server.CombatService;

/// <summary>
///     Stub implementation of ICombatService.
///     TODO: Implement combat logic.
/// </summary>
public class CombatService : ICombatService
{
    private readonly ILog _log;

    public CombatService(ILog log)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    // TODO: Implement combat methods
}
