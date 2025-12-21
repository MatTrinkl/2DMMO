using Mmo.Server.MessageRouting.Handler;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handlers;

/// <summary>
///     Handler for ZoneEvent messages (Category 1, range 100-199).
///     Handles zone-related events like joins, leaves, transfers.
/// </summary>
public class ZoneEventHandler : BaseCategoryHandler
{
    public ZoneEventHandler(ILog log) : base(log)
    {
    }

    /// <inheritdoc />
    public override MessageCategory Category => MessageCategory.ZoneEvent;

    /// <inheritdoc />
    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for specific ZoneEvent message types
        // Example:
        // Register(MessageType.JoinZone, HandleJoinZone);
        // Register(MessageType.LeaveZone, HandleLeaveZone);
        // Register(MessageType.ZoneTransferRequest, HandleZoneTransferRequest);
    }

    // TODO: Add handler methods
    // Example:
    // private void HandleJoinZone(MessageContext ctx, JoinZoneMessage msg) { }
}
