using Microsoft.Extensions.Logging;
using Mmo.Shared.Interfaces;

namespace Mmo.Server;

public sealed class LoggerAdapter(ILogger logger) : ILog
{
    public void Debug(string messageTemplate, params object[] args)
        => logger.LogDebug(messageTemplate, args);

    public void Info(string messageTemplate, params object[] args)
        => logger.LogInformation(messageTemplate, args);

    public void Warn(string messageTemplate, params object[] args)
        => logger.LogWarning(messageTemplate, args);

    public void Error(string messageTemplate, params object[] args)
        => logger.LogError(messageTemplate, args);

    public void Error(Exception exception, string messageTemplate, params object[] args)
        => logger.LogError(exception, messageTemplate, args);
}

