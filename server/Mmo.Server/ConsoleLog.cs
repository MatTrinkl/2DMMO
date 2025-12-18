using Mmo.Shared.Interfaces;

namespace Mmo.Server.Logging;

/// <summary>
///     Einfache Console-Log Implementation.
///     Kann später durch Serilog, NLog, etc. ersetzt werden.
/// </summary>
public class ConsoleLog : ILog
{
    private readonly object _lock = new();

    public void Debug(string message, params object[] args) => Log(LogLevel.Debug, message, args);

    public void Info(string message, params object[] args) => Log(LogLevel.Info, message, args);

    public void Warn(string message, params object[] args) => Log(LogLevel.Warn, message, args);

    public void Error(string message, params object[] args) => Log(LogLevel.Error, message, args);

    public void Error(Exception ex, string message, params object[] args)
    {
        Log(LogLevel.Error, message, args);
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"  Exception: {ex.Message}");
            if (ex.StackTrace != null) Console.WriteLine($"  {ex.StackTrace}");

            Console.ResetColor();
        }
    }

    private void Log(LogLevel level, string message, object[] args)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss. fff");
        string formattedMessage = FormatMessage(message, args);

        lock (_lock)
        {
            Console.ForegroundColor = GetColor(level);
            Console.WriteLine($"[{timestamp}] [{level}] {formattedMessage}");
            Console.ResetColor();
        }
    }

    private static string FormatMessage(string message, object[] args)
    {
        if (args == null || args.Length == 0)
            return message;

        // Einfaches Placeholder-Replacement:  {Name} -> args[index]
        string result = message;
        for (int i = 0; i < args.Length; i++)
        {
            // Finde {xyz} und ersetze mit args[i]
            int startIndex = result.IndexOf('{');
            if (startIndex >= 0)
            {
                int endIndex = result.IndexOf('}', startIndex);
                if (endIndex > startIndex)
                    result = result.Substring(0, startIndex) +
                             args[i] +
                             result.Substring(endIndex + 1);
            }
        }

        return result;
    }

    private static ConsoleColor GetColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warn => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
    }

    private enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }
}
