using Mmo.Server.MessageRouting.Handler;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handlers;

/// <summary>
///     Handler for Movement messages (Category 2, range 200-299).
///     Handles position updates, teleports, movement corrections.
/// </summary>
public class MovementHandler : BaseCategoryHandler
{
    public MovementHandler(ILog log) : base(log)
    {
    }

    /// <inheritdoc />
    public override MessageCategory Category => MessageCategory.Movement;

    /// <inheritdoc />
    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for specific Movement message types
        // Example:
        // Register(MessageType.PositionUpdate, HandlePositionUpdate);
        // Register(MessageType.TeleportRequest, HandleTeleportRequest);
        // Register(MessageType.JumpRequest, HandleJumpRequest);
    }

    // TODO: Add handler methods
    // Example:
    // private void HandlePositionUpdate(MessageContext ctx, PositionUpdateMessage msg) { }
}
