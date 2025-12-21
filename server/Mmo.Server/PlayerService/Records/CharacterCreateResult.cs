namespace Mmo.Server.PlayerService.Records;

/// <summary>
///     Represents the result of a character creation attempt.
/// </summary>
/// <param name="Success">Whether the character creation was successful.</param>
/// <param name="CharacterId">The ID of the created character (if successful).</param>
/// <param name="Error">Error message if creation failed.</param>
public record CharacterCreateResult(
    bool Success,
    Guid? CharacterId = null,
    string? Error = null
);
