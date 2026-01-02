namespace Mmo.Shared.Character.Enums;

public enum CharacterDeleteResponseErrorCode
{
    /// <summary>
    ///     The character was not found.
    /// </summary>
    CharacterNotFound,

    /// <summary>
    ///     The character is not owned by this account.
    /// </summary>
    CharacterNotOwned,

    /// <summary>
    ///     The Confirmation needs to be "DELETE".
    /// </summary>
    InvalidConfirmation,

    /// <summary>
    ///     The character cannot be deleted while being a guild leader.
    /// </summary>
    CharacterGuildLeader
}
