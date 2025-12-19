namespace Mmo.Shared;

/// <summary>
///     Shared constants for the 2DMMO project.
/// </summary>
public static class SharedConstants
{
    /// <summary>
    ///     The default port for the game server.
    /// </summary>
    public const int DefaultPort = 7777;

    /// <summary>
    ///     The name of the game.
    /// </summary>
    public const string GameName = "2DMMO";

    /// <summary>
    ///     The current version of the protocol.
    /// </summary>
    public const int ProtocolVersion = 1;

    // Network
    /// <summary>
    ///     The server tick rate in Hz.
    /// </summary>
    public const int TickRate = 25; // Hz

    /// <summary>
    ///     The maximum message size (64 KB).
    /// </summary>
    public const int MaxMessageSize = 1024 * 64; // 64 KB

    // Game
    /// <summary>
    ///     The default X spawn coordinate.
    /// </summary>
    public const float DefaultSpawnX = 400f;

    /// <summary>
    ///     The default Y spawn coordinate.
    /// </summary>
    public const float DefaultSpawnY = 300f;

    /// <summary>
    ///     The player movement speed in pixels per second.
    /// </summary>
    public const float PlayerSpeed = 200f; // pixels per second

    /// <summary>
    ///     The amount of time until a player is flagged as Away from Keyboard.
    /// </summary>
    public const int TimeToAfkInSeconds = 300; // 5 minutes

    /// <summary>
    ///     The Client times out when he doesn't send a heartbeat in this time.
    /// </summary>
    public const int TimeToConnectionDeadInSeconds = 30;

    /// <summary>
    ///     The tick interval in milliseconds (40ms).
    /// </summary>
    public static readonly TimeSpan TickDuration =
        TimeSpan.FromSeconds(1.0 / TickRate);

    /// <summary>
    ///     The maximum numbers of players on one server/shard later.
    /// </summary>
    public static readonly int MaxConnection = 1000;
}
