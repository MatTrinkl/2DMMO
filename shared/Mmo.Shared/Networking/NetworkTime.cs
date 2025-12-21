namespace Mmo.Shared.Networking;

/// <summary>
///     This class helps by converting between DateTime and long. Also returns the current Unix Time in milliseconds as a
///     long.
/// </summary>
public static class NetworkTime
{
    /// <summary>
    ///     Returns the current Unix Time in milliseconds.
    /// </summary>
    public static long Now => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    ///     Converts a long timestamp into a DateTime object.
    /// </summary>
    /// <param name="timestamp">The timestamp in milliseconds to convert.</param>
    /// <returns>A new DateTime Object in UTC Time.</returns>
    public static DateTime ToDateTime(long timestamp)
        => DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;

    /// <summary>
    ///     Converts a DateTime object into a timestamp in milliseconds.
    /// </summary>
    /// <param name="dt">The DateTime Object to convert.</param>
    /// <returns>The timestamp as long in milliseconds.</returns>
    public static long FromDateTime(DateTime dt)
        => new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeMilliseconds();
}
