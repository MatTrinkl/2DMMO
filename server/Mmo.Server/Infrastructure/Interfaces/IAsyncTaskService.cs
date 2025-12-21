using Mmo.Server.Messages;

namespace Mmo.Server.Infrastructure.Interfaces;

/// <summary>
///     Service for handling async task execution in the game loop.
///     Provides a clean interface for handlers to execute async operations
///     without blocking the game loop.
/// </summary>
public interface IAsyncTaskService
{
    /// <summary>
    ///     Runs an async task and queues the result for processing in the game loop.
    /// </summary>
    /// <typeparam name="T">Result type of the task</typeparam>
    /// <param name="connectionId">The connection ID this task belongs to</param>
    /// <param name="task">The async task to execute</param>
    /// <param name="onCompleted">Callback when task completes (executed in game loop)</param>
    /// <param name="onError">Optional callback on error</param>
    void Run<T>(
        Guid connectionId,
        Task<T> task,
        Action<MessageContext, T> onCompleted,
        Action<MessageContext, Exception>? onError = null);

    /// <summary>
    ///     Runs an async task without a result.
    /// </summary>
    /// <param name="connectionId">The connection ID this task belongs to</param>
    /// <param name="task">The async task to execute</param>
    /// <param name="onCompleted">Callback when task completes</param>
    /// <param name="onError">Optional callback on error</param>
    void Run(
        Guid connectionId,
        Task task,
        Action<MessageContext> onCompleted,
        Action<MessageContext, Exception>? onError = null);
}
