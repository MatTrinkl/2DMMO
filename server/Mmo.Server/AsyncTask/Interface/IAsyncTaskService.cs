using Mmo.Server.Messages;

namespace Mmo.Server.AsyncTask.Interface;

public interface IAsyncTaskService
{
    /// <summary>
    ///     Executes async task, then callback in Game Loop with fresh context.
    /// </summary>
    /// <typeparam name="TResult">Result type of the async task</typeparam>
    void Run<TResult>(
        Guid connectionId,
        Func<Task<TResult>> asyncTask,
        Action<MessageContext, TResult> onComplete);

    /// <summary>
    ///     Executes async task (without result), then callback in Game Loop.
    /// </summary>
    void Run(
        Guid connectionId,
        Func<Task> asyncTask,
        Action<MessageContext> onComplete);
}
