using Mmo.Server.PlayerService;

namespace Mmo.Server.Connections.Records;

// Result record for the task
public record LoginTaskResult(
    bool Success,
    ServerPlayerCharacter? Player = null,
    string? Error = null
);
