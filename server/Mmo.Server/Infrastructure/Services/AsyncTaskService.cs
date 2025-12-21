using Mmo.Server.Core;
using Mmo.Server.Infrastructure.Interfaces;
using Mmo.Server.Messages;

namespace Mmo.Server.Infrastructure.Services;

/// <summary>
///     Implementation of IAsyncTaskService.
///     Delegates to GameServer's completion queue.
/// </summary>
public class AsyncTaskService : IAsyncTaskService
{
    private readonly GameServer _gameServer;

    public AsyncTaskService(GameServer gameServer)
    {
        _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
    }

    /// <inheritdoc />
    public void Run<T>(
        Guid connectionId,
        Task<T> task,
        Action<MessageContext, T> onCompleted,
        Action<MessageContext, Exception>? onError = null)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                if (onError != null)
                    _gameServer.QueueCompletion(connectionId, ctx => onError(ctx, t.Exception!.InnerException!));
                else
                    _gameServer.QueueCompletion(connectionId, ctx =>
                        ctx.SendError("INTERNAL_ERROR", "An error occurred"));
            }
            else if (t.IsCompletedSuccessfully)
            {
                _gameServer.QueueCompletion(connectionId, ctx => onCompleted(ctx, t.Result));
            }
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    /// <inheritdoc />
    public void Run(
        Guid connectionId,
        Task task,
        Action<MessageContext> onCompleted,
        Action<MessageContext, Exception>? onError = null)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                if (onError != null)
                    _gameServer.QueueCompletion(connectionId, ctx => onError(ctx, t.Exception!.InnerException!));
                else
                    _gameServer.QueueCompletion(connectionId, ctx =>
                        ctx.SendError("INTERNAL_ERROR", "An error occurred"));
            }
            else if (t.IsCompletedSuccessfully)
            {
                _gameServer.QueueCompletion(connectionId, ctx => onCompleted(ctx));
            }
        }, TaskContinuationOptions.ExecuteSynchronously);
    }
}
