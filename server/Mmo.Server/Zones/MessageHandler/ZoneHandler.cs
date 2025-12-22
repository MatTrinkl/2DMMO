using Mmo.Server.AsyncTask.Interface;
using Mmo.Server.MessageRouting.Handler;
using Mmo.Server.Messages;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.Zones.Interfaces;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Zones.Messages.Client_Server;
using Mmo.Shared.Zones.Messages.Server_Client;

namespace Mmo.Server.Zones.MessageHandler;

public class ZoneHandler(IZoneService zoneService, ILog log) : BaseCategoryHandler(log)
{
    public override MessageCategory Category => MessageCategory.Zone;

    protected override void RegisterHandlers() =>
        Register<ZoneTransferRequest>(MessageType.ZoneTransferRequest, HandleZoneTransfer);

    private void HandleZoneTransfer(MessageContext ctx, ZoneTransferRequest request)
    {
        ctx.GetService<IAsyncTaskService>().Run(ctx.ConnectionId, async () => zoneService.RequestZoneTransferAsync(
            ctx.ServerPlayer!.Entity.PersistentId,
            request.TargetZoneId
        ), (ctx, result) =>
        {
            ctx.GetService<IBroadcastService>().SendToPlayer(ctx.Connection, result.Success
                ? ZoneTransferResponse.Succeeded(request.TargetZoneId)
                : ZoneTransferResponse.Failed(result.Error!));
        });
    }
}
