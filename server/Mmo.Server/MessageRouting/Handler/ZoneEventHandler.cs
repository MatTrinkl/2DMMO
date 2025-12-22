using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handler;

/// <summary>
///     Handler for Zone Events (MessageCategory.Zone, range 100-199).
///     Handles zone-related events like player joining/leaving zones.
/// </summary>
public class ZoneEventHandler : BaseCategoryHandler
{
    public ZoneEventHandler(ILog log) : base(log)
    {
    }

    public override MessageCategory Category => MessageCategory.Zone;

    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for zone event messages
        // Example:
        // Register(MessageType.ZoneEventPlayerJoined, HandlePlayerJoined);
        // Register(MessageType.ZoneEventPlayerLeft, HandlePlayerLeft);
    }

    // Example handler method structure:
    // private void HandlePlayerJoined(MessageContext ctx, ZoneEventPlayerJoinedMessage msg)
    // {
    //     if (!RequireInGame(ctx)) return;
    //     
    //     var zoneService = ctx.GetService<IZoneService>();
    //     // Handler logic here - delegate to service
    // }
}
