// tests/Mmo.Server.Tests/Helpers/MockLog.cs

using Mmo.Shared.Interfaces;

namespace Mmo.Server.Tests.Helpers;

/// <summary>
///     A mock implementation of ILog for testing.
///     Captures all log messages for assertions.
/// </summary>
public class MockLog : ILog
{
    public List<string> Messages { get; } = new();
    public List<(string Level, string Message, object[] Args)> RawMessages { get; } = new();

    public void Debug(string message, params object[] args)
    {
        RawMessages.Add(("DEBUG", message, args));
        Messages.Add($"[DEBUG] {FormatMessage(message, args)}");
    }

    public void Info(string message, params object[] args)
    {
        RawMessages.Add(("INFO", message, args));
        Messages.Add($"[INFO] {FormatMessage(message, args)}");
    }

    public void Warn(string message, params object[] args)
    {
        RawMessages.Add(("WARN", message, args));
        Messages.Add($"[WARN] {FormatMessage(message, args)}");
    }

    public void Error(string message, params object[] args)
    {
        RawMessages.Add(("ERROR", message, args));
        Messages.Add($"[ERROR] {FormatMessage(message, args)}");
    }

    public void Error(Exception ex, string message, params object[] args)
    {
        RawMessages.Add(("ERROR", message, args));
        Messages.Add($"[ERROR] {FormatMessage(message, args)}: {ex.Message}");
    }

    /// <summary>
    ///     Formats a message with named placeholders like {TickRate} or indexed {0}.
    /// </summary>
    private static string FormatMessage(string message, object[] args)
    {
        if (args == null || args.Length == 0)
            return message;

        try
        {
            // Versuche zuerst benannte Platzhalter zu ersetzen
            // z.B. "{TickRate}" wird durch args[0] ersetzt, "{Tick}" durch args[1], etc.
            string result = message;
            int argIndex = 0;

            // Finde alle {Name} Platzhalter und ersetze sie der Reihe nach
            int startIndex = 0;
            while (startIndex < result.Length && argIndex < args.Length)
            {
                int openBrace = result.IndexOf('{', startIndex);
                if (openBrace == -1) break;

                int closeBrace = result.IndexOf('}', openBrace);
                if (closeBrace == -1) break;

                // Überprüfe ob es ein escaped brace ist {{ oder }}
                if (openBrace + 1 < result.Length && result[openBrace + 1] == '{')
                {
                    startIndex = openBrace + 2;
                    continue;
                }

                // Extrahiere den Platzhalter (z.B. "TickRate" oder "0")
                string placeholder = result.Substring(openBrace, closeBrace - openBrace + 1);

                // Ersetze mit dem entsprechenden Argument
                string replacement = args[argIndex]?.ToString() ?? "null";
                result = result.Remove(openBrace, closeBrace - openBrace + 1)
                    .Insert(openBrace, replacement);

                startIndex = openBrace + replacement.Length;
                argIndex++;
            }

            return result;
        }
        catch
        {
            // Fallback:  Gib einfach die Message mit den Args als String zurück
            return $"{message} [{string.Join(", ", args)}]";
        }
    }

    /// <summary>
    ///     Clears all captured messages.
    /// </summary>
    public void Clear()
    {
        Messages.Clear();
        RawMessages.Clear();
    }

    /// <summary>
    ///     Returns true if any message contains the specified text.
    /// </summary>
    public bool HasMessageContaining(string text) =>
        Messages.Any(m => m.Contains(text, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    ///     Returns true if any message at the specified level contains the text.
    /// </summary>
    public bool HasMessageContaining(string level, string text)
    {
        return Messages.Any(m =>
            m.StartsWith($"[{level}]", StringComparison.OrdinalIgnoreCase) &&
            m.Contains(text, StringComparison.OrdinalIgnoreCase));
    }
}
