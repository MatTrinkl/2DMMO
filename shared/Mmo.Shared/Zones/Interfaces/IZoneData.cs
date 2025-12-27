using Mmo.Shared.Character.Enums;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Shared.Zones.Interfaces;

/// <summary>
///     Interface for synchronized zone data between Zone and ZoneDto.
///     Contains all properties that are sent to the client.
///     Excludes server-only properties like Experience and Gold.
/// </summary>
public interface IZoneData
{
    /// <summary>
    ///     Unique ID of the zone.
    /// </summary>
    ushort ZoneId { get; }

    /// <summary>
    ///     Display name of the zone.
    /// </summary>
    string ZoneName { get; }

    /// <summary>
    ///     Properties of the zone.
    /// </summary>
    ZoneFlags ZoneFlags { get; }

    /// <summary>
    ///     This level is the recommended minimum.
    /// </summary>
    int RecommendedMinLevel { get; }

    /// <summary>
    ///     This level is the recommended maximum.
    /// </summary>
    int RecommendedMaxLevel { get; }

    /// <summary>
    ///     Is Player vs Player enabled in this zone.
    /// </summary>
    bool IsPvpEnabled { get; }

    /// <summary>
    ///     Is this zone contested between two or more Factions.
    /// </summary>
    bool IsContested { get; }

    /// <summary>
    ///     The faction controlling this zone. WIP: No final decision if this is even in the game. Todo: make a decision!
    /// </summary>
    Faction? ControllingFaction { get; }

    /// <summary>
    ///     Is this zone a savespace (no combat, no pvp)?
    /// </summary>
    bool IsSanctuary { get; }

    /// <summary>
    ///     The default spawnpoint when entering the zone. Todo: Have a Mapping for zonetransfer (neihbors) and default
    ///     fallback
    /// </summary>
    Position DefaultSpawnPoint { get; }

    /// <summary>
    ///     Position of the Graveyard for respawn after death.
    /// </summary>
    Position? GraveyardPosition { get; }

    /// <summary>
    ///     PLACEHOLDER! The ID of the music for the audio system.
    /// </summary>
    string MusicId { get; }

    /// <summary>
    ///     PLACEHOLDER! The ID of the ambience sounds for the audio system.
    /// </summary>
    string AmbienceId { get; }

    /// <summary>
    ///     The boarder in the west.
    /// </summary>
    ZoneBounds Boarder { get; }
}
