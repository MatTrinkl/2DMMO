namespace Mmo.Shared.Connection.Enums;

/// <summary>
/// Describes what compression algorithm is used for server client communication.
/// </summary>
public enum CompressionAlgorithm
{
    /// <summary>
    /// No compression
    /// </summary>
    None=0,
    /// <summary>
    /// Fast, moderate compression
    /// </summary>
    Lz4=1,
    /// <summary>
    /// Slower, better compression
    /// </summary>
    Zstd=2
}
