using Mmo.Shared.Core.Interfaces;

namespace Mmo.Server.Logging;

/// <summary>
///     Simple console log implementation.
///     Can be replaced later with Serilog, NLog, etc.
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

    private static string FormatMessage(string message, object[]? args)
    {
        if (args == null || args.Length == 0)
            return message;

        // Simple Placeholder-Replacement:  {Name} -> args[index]
        string result = message;
        foreach (object arg in args)
        {
            // Find {xyz} and replace with args[i]
            int startIndex = result.IndexOf('{');
            if (startIndex < 0) continue;
            int endIndex = result.IndexOf('}', startIndex);
            if (endIndex > startIndex)
                result = result[..startIndex] +
                         arg +
                         result[(endIndex + 1)..];
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
