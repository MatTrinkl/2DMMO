using Mmo.Server.Connections;
using Mmo.Server.PlayerService.Records;
using Mmo.Shared.Character.Enums;

namespace Mmo.Server.PlayerService.Interfaces;

/// <summary>
///     Service for player operations (Spawn, Load, Save).
/// </summary>
public interface IPlayerService
{
    /// <summary>
    ///     Spawns a new player or loads an existing one.
    /// </summary>
    Task<ServerPlayerCharacter> SpawnPlayerAsync(
        Guid accountId,
        string characterName,
        ClientConnection connection);

    /// <summary>
    ///     Loads character list for account.
    /// </summary>
    Task<List<CharacterInfo>> GetCharacterListAsync(Guid accountId);

    /// <summary>
    ///     Creates a new character.
    /// </summary>
    Task<CharacterCreateResult> CreateCharacterAsync(
        Guid accountId,
        string name,
        Race race,
        CharacterClass characterClass,
        Gender gender);

    /// <summary>
    ///     Removes player (logout/disconnect).
    /// </summary>
    Task RemovePlayerAsync(Guid connectionId);

    /// <summary>
    ///     Saves player data.
    /// </summary>
    Task SavePlayerAsync(ServerPlayerCharacter playerCharacter);
}
