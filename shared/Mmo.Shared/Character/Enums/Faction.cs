namespace Mmo.Shared.Character.Enums;

/// <summary>
///     Represents the faction affiliation of entities.
/// </summary>
public enum Faction : byte
{
    /// <summary>
    ///     Neutral to all factions.
    /// </summary>
    Neutral = 0,

    /// <summary>
    ///     Player-controlled faction.
    /// </summary>
    Player = 1,

    /// <summary>
    ///     Hostile monster faction.
    /// </summary>
    Monster = 2,

    /// <summary>
    ///     Non-hostile ambient creatures.
    /// </summary>
    Critter = 3,

    /// <summary>
    ///     Friendly to all players.
    /// </summary>
    Friendly = 4
}
