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
                return; // No callback on error
            }

            // Queue callback with result → will be executed in Game Loop
            gameServer.QueueCompletion(connectionId, ctx =>
            {
                // ctx is FRESH from GameServer created!
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
