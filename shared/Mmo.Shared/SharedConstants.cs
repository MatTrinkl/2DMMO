namespace Mmo.Shared;

/// <summary>
/// Shared constants for the 2DMMO project.
/// </summary>
public static class SharedConstants
{
    /// <summary>
    /// The default port for the game server.
    /// </summary>
    public const int DefaultPort = 7777;

    /// <summary>
    /// The name of the game.
    /// </summary>
    public const string GameName = "2DMMO";

    /// <summary>
    /// The current version of the protocol.
    /// </summary>
    public const int ProtocolVersion = 1;

    // Network
    /// <summary>
    /// The server tick rate in Hz.
    /// </summary>
    public const int TickRate = 30; // Hz

    /// <summary>
    /// The tick interval in milliseconds (~33ms).
    /// </summary>
    public const int TickIntervalMs = 1000 / TickRate; // ~33ms

    // Protocol
    /// <summary>
    /// The message header size (1 byte type + 4 bytes length).
    /// </summary>
    public const int MessageHeaderSize = 5; // 1 byte type + 4 bytes length

    /// <summary>
    /// The maximum message size (64 KB).
    /// </summary>
    public const int MaxMessageSize = 1024 * 64; // 64 KB

    // Game
    /// <summary>
    /// The default X spawn coordinate.
    /// </summary>
    public const float DefaultSpawnX = 400f;

    /// <summary>
    /// The default Y spawn coordinate.
    /// </summary>
    public const float DefaultSpawnY = 300f;

    /// <summary>
    /// The player movement speed in pixels per second.
    /// </summary>
    public const float PlayerSpeed = 200f; // pixels per second
}
