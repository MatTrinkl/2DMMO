using Mmo.Server.MessageRouting.Handler;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handlers;

/// <summary>
///     Handler for Ping/System messages (Category 9, range 900-999).
///     Handles ping, pong, latency, error messages.
///     Note: This handler does NOT use a service (direct handling).
/// </summary>
public class PingHandler : BaseCategoryHandler
{
    public PingHandler(ILog log) : base(log)
    {
    }

    /// <inheritdoc />
    public override MessageCategory Category => MessageCategory.Ping;

    /// <inheritdoc />
    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for specific Ping/System message types
        // Example:
        // Register(MessageType.Ping, HandlePing);
        // Register(MessageType.LatencyReport, HandleLatencyReport);
    }

    // TODO: Add handler methods
    // Example:
    // private void HandlePing(MessageContext ctx, PingMessage msg) { }
}
