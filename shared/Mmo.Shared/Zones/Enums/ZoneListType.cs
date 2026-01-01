namespace Mmo.Shared.Zones.Enums;

/// <summary>
/// Which type of ZoneList should be returned to the client.
/// </summary>
public enum ZoneListType
{
    /// <summary>
    /// All Zones, discovered and undiscovered
    /// </summary>
    All = 0,
    /// <summary>
    /// Only the discovered Zones
    /// </summary>
    Discovered = 1,
    /// <summary>
    /// Only Zones where the client has a Flight path.
    /// </summary>
    FlightPaths = 2,
    /// <summary>
    /// All Zones reachable by a teleportation (e.g. Portal)
    /// </summary>
    TeleportDestinations = 3,
    /// <summary>
    /// All hearthstone locations
    /// </summary>
    HearthstoneLocations = 4,
    /// <summary>
    /// All available dungeons
    /// </summary>
    DungeonList = 5
}
