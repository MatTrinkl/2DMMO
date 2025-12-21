namespace Mmo.Shared.Core.Interfaces;

/// <summary>
///     This interface is used for logging metrics. Can be implemented with normal console logging or later used with
///     Grafana.
/// </summary>
public interface ILog
{
    void Debug(string messageTemplate, params object[] args);
    void Info(string messageTemplate, params object[] args);
    void Warn(string messageTemplate, params object[] args);
    void Error(string messageTemplate, params object[] args);
    void Error(Exception exception, string messageTemplate, params object[] args);
}
