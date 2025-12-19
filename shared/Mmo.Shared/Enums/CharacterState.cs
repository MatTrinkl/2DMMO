namespace Mmo.Shared.Enums;

/// <summary>
///     Represents the life state of a character.
/// </summary>
public enum CharacterState : byte
{
    /// <summary>
    ///     Character is alive and can act normally.
    /// </summary>
    Alive = 0,

    /// <summary>
    ///     Character is dead (corpse state).
    /// </summary>
    Dead = 1,

    /// <summary>
    ///     Character is in ghost form after death.
    /// </summary>
    Ghost = 2
}
