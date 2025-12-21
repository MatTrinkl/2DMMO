using Mmo.Server.Messages;

namespace Mmo.Server.AsyncTask.Interface;

public interface IAsyncTaskService
{
        /// <summary>
        ///     Führt async Task aus, dann Callback im Game Loop mit frischem Context.
        /// </summary>
        /// <typeparam name="TResult">Result-Typ des async Tasks</typeparam>
        void Run<TResult>(
            Guid connectionId,
            Func<Task<TResult>> asyncTask,
            Action<MessageContext, TResult> onComplete);

        /// <summary>
        ///     Führt async Task aus (ohne Result), dann Callback im Game Loop.
        /// </summary>
        void Run(
            Guid connectionId,
            Func<Task> asyncTask,
            Action<MessageContext> onComplete);
}
