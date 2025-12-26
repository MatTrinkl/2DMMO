using Mmo.Shared.Character.Messages.Client_Server;

namespace Mmo.Shared.Character.Enums;

/// <summary>
///     The Error Codes when a <see cref="CharacterSelectRequest" /> request is not successful.
/// </summary>
public enum CharacterSelectResponseErrorCode
{
    /// <summary>
    ///     The selected character is not found.
    /// </summary>
    CharacterNotFound,

    /// <summary>
    ///     The selected character belongs to another account.
    /// </summary>
    CharacterNotOwned,

    /// <summary>
    ///     The selected character is currently logged in.
    /// </summary>
    CharacterInUse,

    /// <summary>
    ///     The selected character is already deleted.
    /// </summary>
    CharacterDeleted
}
