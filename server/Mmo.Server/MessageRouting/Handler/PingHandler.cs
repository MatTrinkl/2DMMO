using Mmo.Server.Messages;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handler;

/// <summary>
///     Handler for Ping/System (MessageCategory.Ping, range 900-999).
///     Handles ping/latency/heartbeat messages. Does not require a service.
/// </summary>
public class PingHandler : BaseCategoryHandler
{
    public PingHandler(ILog log) : base(log)
    {
    }

    public override MessageCategory Category => MessageCategory.Ping;

    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for ping/system messages
        // Example:
        // Register(MessageType.PingRequest, HandlePingRequest);
        // Register(MessageType.Heartbeat, HandleHeartbeat);
    }

    // Example handler method structure:
    // private void HandlePingRequest(MessageContext ctx, PingRequestMessage msg)
    // {
    //     // Simple response - no service needed
    //     ctx.Send(new PingResponseMessage { Timestamp = msg.Timestamp });
    // }
}
