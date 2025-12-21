namespace Mmo.Shared.Npc.Enums;

/// <summary>
///     Represents the AI state of an NPC.
/// </summary>
public enum NpcAiState : byte
{
    /// <summary>
    ///     NPC is idle at spawn location.
    /// </summary>
    Idle = 0,

    /// <summary>
    ///     NPC is patrolling along a path.
    /// </summary>
    Patrol = 1,

    /// <summary>
    ///     NPC is actively in combat.
    /// </summary>
    Combat = 2,

    /// <summary>
    ///     NPC is evading and returning to spawn location.
    /// </summary>
    Evade = 3,

    /// <summary>
    ///     NPC is fleeing from combat.
    /// </summary>
    Flee = 4,

    /// <summary>
    ///     NPC is dead.
    /// </summary>
    Dead = 5,

    /// <summary>
    ///     NPC is controlled by a script or event.
    /// </summary>
    Scripted = 6
}
