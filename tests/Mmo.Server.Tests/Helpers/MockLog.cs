using Mmo.Shared.Interfaces;

namespace Mmo.Server.Tests.Helpers;

public class MockLog : ILog
{
    public List<string> Messages { get; } = new();

    public void Debug(string message, params object[] args)
        => Messages.Add($"[DEBUG] {FormatMessage(message, args)}");

    public void Info(string message, params object[] args)
        => Messages.Add($"[INFO] {FormatMessage(message, args)}");

    public void Warn(string message, params object[] args)
        => Messages.Add($"[WARN] {FormatMessage(message, args)}");

    public void Error(string message, params object[] args)
        => Messages.Add($"[ERROR] {FormatMessage(message, args)}");

    public void Error(Exception ex, string message, params object[] args)
        => Messages.Add($"[ERROR] {FormatMessage(message, args)}: {ex.Message}");

    /// <summary>
    /// Formats a structured logging message (with {Name} or {Name:Format} placeholders) 
    /// by directly substituting the argument values.
    /// </summary>
    private static string FormatMessage(string message, object[] args)
    {
        if (args == null || args.Length == 0)
            return message;

        // Replace {PropertyName} or {PropertyName:Format} with actual values
        var formatted = message;
        for (int i = 0; i < args.Length && i < args.Length; i++)
        {
            var startIndex = formatted.IndexOf('{');
            if (startIndex < 0)
                break;
                
            var endIndex = formatted.IndexOf('}', startIndex);
            if (endIndex <= startIndex)
                break;

            // Extract the placeholder content
            var placeholder = formatted.Substring(startIndex + 1, endIndex - startIndex - 1);
            
            // Check for format specifier (e.g., "PropertyName:F2")
            var colonIndex = placeholder.IndexOf(':');
            string formattedValue;
            
            if (colonIndex >= 0)
            {
                // Has format specifier - apply it
                var formatSpec = placeholder.Substring(colonIndex + 1).Trim();
                var arg = args[i];
                
                // Try to format with the specifier
                if (arg is IFormattable formattable)
                {
                    formattedValue = formattable.ToString(formatSpec, null);
                }
                else
                {
                    formattedValue = arg?.ToString() ?? "null";
                }
            }
            else
            {
                // No format specifier - just convert to string
                formattedValue = args[i]?.ToString() ?? "null";
            }
            
            // Replace the placeholder with the formatted value
            formatted = formatted.Substring(0, startIndex) + 
                       formattedValue + 
                       formatted.Substring(endIndex + 1);
        }

        return formatted;
    }
}
