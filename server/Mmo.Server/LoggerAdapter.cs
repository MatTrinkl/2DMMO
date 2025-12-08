using Microsoft.Extensions.Logging;
using Mmo.Shared.Interfaces;

namespace Mmo.Server;

/// <summary>
///     An Adapter class for ILog. This will later be used which grafana.
/// </summary>
/// <param name="logger">The Interface to adapt to.</param>
public sealed class LoggerAdapter(ILogger logger) : ILog
{
    /// <summary>
    ///     Writes a debug log to the logger.
    /// </summary>
    /// <param name="messageTemplate">
    ///     The template message. Can contain multiple {} which will be replaced which objects from
    ///     <see cref="args" />.
    /// </param>
    /// <param name="args">Objects to replace in the <see cref="messageTemplate" />.</param>
    public void Debug(string messageTemplate, params object[] args)
        => logger.LogDebug(messageTemplate, args);

    /// <summary>
    ///     Writes a info log to the logger.
    /// </summary>
    /// <param name="messageTemplate">
    ///     The template message. Can contain multiple {} which will be replaced which objects from
    ///     <see cref="args" />.
    /// </param>
    /// <param name="args">Objects to replace in the <see cref="messageTemplate" />.</param>
    public void Info(string messageTemplate, params object[] args)
        => logger.LogInformation(messageTemplate, args);

    /// <summary>
    ///     Writes a warn log to the logger.
    /// </summary>
    /// <param name="messageTemplate">
    ///     The template message. Can contain multiple {} which will be replaced which objects from
    ///     <see cref="args" />.
    /// </param>
    /// <param name="args">Objects to replace in the <see cref="messageTemplate" />.</param>
    public void Warn(string messageTemplate, params object[] args)
        => logger.LogWarning(messageTemplate, args);

    /// <summary>
    ///     Writes a error log to the logger.
    /// </summary>
    /// <param name="messageTemplate">
    ///     The template message. Can contain multiple {} which will be replaced which objects from
    ///     <see cref="args" />.
    /// </param>
    /// <param name="args">Objects to replace in the <see cref="messageTemplate" />.</param>
    public void Error(string messageTemplate, params object[] args)
        => logger.LogError(messageTemplate, args);

    /// <summary>
    ///     Writes a error log to the logger.
    /// </summary>
    /// <param name="exception">The exception which triggered the error.</param>
    /// <param name="messageTemplate">
    ///     The template message. Can contain multiple {} which will be replaced which objects from
    ///     <see cref="args" />.
    /// </param>
    /// <param name="args">Objects to replace in the <see cref="messageTemplate" />.</param>
    public void Error(Exception exception, string messageTemplate, params object[] args)
        => logger.LogError(exception, messageTemplate, args);
}
