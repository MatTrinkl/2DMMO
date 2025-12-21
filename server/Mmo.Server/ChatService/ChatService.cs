using Mmo.Server.ChatService.Interfaces;
using Mmo.Shared.Core.Interfaces;

namespace Mmo.Server.ChatService;

/// <summary>
///     Stub implementation of IChatService.
///     TODO: Implement chat logic.
/// </summary>
public class ChatService : IChatService
{
    private readonly ILog _log;

    public ChatService(ILog log)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    // TODO: Implement chat methods
}
