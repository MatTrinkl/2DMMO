using Mmo.Server.AsyncTask.Interface;
using Mmo.Server.Core;
using Mmo.Server.Messages;
using Mmo.Shared.Core.Interfaces;

namespace Mmo.Server.AsyncTask.Services;

public class AsyncTaskService(GameServer gameServer, ILog log) : IAsyncTaskService
{
    public void Run<TResult>(
        Guid connectionId,
        Func<Task<TResult>> asyncTask,
        Action<MessageContext, TResult> onComplete)
    {
        Task.Run(async () =>
        {
            TResult result;

            try
            {
                result = await asyncTask();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Async task failed for {ConnectionId}", connectionId);
                return; // Kein Callback bei Fehler
            }

            // Queue Callback mit Result → wird im Game Loop ausgeführt
            gameServer.QueueCompletion(connectionId, ctx =>
            {
                // ctx ist FRISCH vom GameServer erstellt!
                onComplete(ctx, result);
            });
        });
    }

    public void Run(
        Guid connectionId,
        Func<Task> asyncTask,
        Action<MessageContext> onComplete)
    {
        Task.Run(async () =>
        {
            try
            {
                await asyncTask();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Async task failed for {ConnectionId}", connectionId);
                return;
            }

            gameServer.QueueCompletion(connectionId, onComplete);
        });
    }
}
