using Mmo.Shared.Character.Messages.Client_Server;

namespace Mmo.Shared.Character.Enums;

/// <summary>
/// The Error Codes when a <see cref="CharacterCreateRequest"/> request is not successful.
/// </summary>
public enum CharacterCreateResponseErrorCode
{
    /// <summary>
    /// The submitted name is already taken.
    /// </summary>
    NameTaken,
    /// <summary>
    /// The submitted name is invalid.
    /// </summary>
    NameInvalid,
    /// <summary>
    /// The submitted name is too long.
    /// </summary>
    MaxCharactersReached,
    /// <summary>
    /// The race id is invalid.
    /// </summary>
    InvalidRace,
    /// <summary>
    /// The class id is invalid
    /// </summary>
    InvalidClass,
    /// <summary>
    /// The combo race+class is invalid.
    /// </summary>
    InvalidRaceClassCombo
}
