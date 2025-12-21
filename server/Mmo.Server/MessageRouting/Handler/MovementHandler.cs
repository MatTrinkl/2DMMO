using Mmo.Server.Messages;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handler;

/// <summary>
///     Handler for Movement (MessageCategory.Movement, range 200-299).
///     Handles movement and position updates for entities.
/// </summary>
public class MovementHandler : BaseCategoryHandler
{
    public MovementHandler(ILog log) : base(log)
    {
    }

    public override MessageCategory Category => MessageCategory.Movement;

    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for movement messages
        // Example:
        // Register(MessageType.MovementPositionUpdate, HandlePositionUpdate);
        // Register(MessageType.MovementTeleportRequest, HandleTeleportRequest);
    }

    // Example handler method structure:
    // private void HandlePositionUpdate(MessageContext ctx, PositionUpdateMessage msg)
    // {
    //     if (!RequireInGame(ctx)) return;
    //     
    //     var movementService = ctx.GetService<IMovementService>();
    //     // Handler logic here - delegate to service
    // }
}
