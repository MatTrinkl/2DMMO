namespace Mmo.Shared.Connection.Enums;

/// <summary>
/// Describes the population amount of a server.
/// </summary>
public enum RealmPopulationStatus
{
    /// <summary>
    /// The population is low.
    /// </summary>
    Low,
    /// <summary>
    /// The population is medium.
    /// </summary>
    Medium,
    /// <summary>
    /// The population is high.
    /// </summary>
    High,
    /// <summary>
    /// The server is full, no new accounts can join.
    /// </summary>
    Full
}
