using Mmo.Server.PlayerService;

namespace Mmo.Server.Connections.Records;

// Result-Record für den Task
public record LoginTaskResult(
    bool Success,
    ServerPlayerCharacter? Player = null,
    string? Error = null
);
