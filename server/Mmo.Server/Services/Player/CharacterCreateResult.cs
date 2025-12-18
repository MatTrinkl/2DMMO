namespace Mmo.Server.Services.Player;

public record CharacterCreateResult(
    bool Success,
    Guid? CharacterId = null,
    string? Error = null
);
