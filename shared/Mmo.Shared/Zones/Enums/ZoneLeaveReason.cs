using Mmo.Shared.Zones.Messages.Client_Server;

namespace Mmo.Shared.Zones.Enums;

/// <summary>
///     The reason why <see cref="LeaveZone" /> is called by the client.
/// </summary>
public enum ZoneLeaveReason
{
    /// <summary>
    ///     The client is logging out of the game (no new Zone needs to be loaded).
    /// </summary>
    Logout,

    /// <summary>
    ///     The client is closed the game.
    /// </summary>
    ExitGame,

    /// <summary>
    ///     The client wants to switch character.
    /// </summary>
    CharacterSwitch
}
