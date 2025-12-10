using Mmo. Shared.Interfaces;

namespace Mmo.Server.Tests. Helpers;

public class MockLog :  ILog
{
    public List<string> Messages { get; } = new();

    public void Debug(string message, params object[] args)
        => Messages.Add($"[DEBUG] {string.Format(message, args)}");

    public void Info(string message, params object[] args)
        => Messages.Add($"[INFO] {string. Format(message, args)}");

    public void Warn(string message, params object[] args)
        => Messages. Add($"[WARN] {string.Format(message, args)}");

    public void Error(string message, params object[] args)
        => Messages.Add($"[ERROR] {string.Format(message, args)}");

    public void Error(Exception ex, string message, params object[] args)
        => Messages. Add($"[ERROR] {string.Format(message, args)}: {ex.Message}");
}
