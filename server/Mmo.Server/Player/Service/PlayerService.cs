using Mmo.Server.Connections;
using Mmo.Server.Player.Interfaces;
using Mmo.Server.PlayerService;
using Mmo.Server.PlayerService.Records;
using Mmo.Server.Zones;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;

namespace Mmo.Server.Player.Service;

/// <summary>
///     Concrete implementation of the Player service.
/// </summary>
public class PlayerService(ZoneManager zoneManager, ILog log) : IPlayerService
{
    public async Task<ServerPlayerCharacter> SpawnPlayerAsync(
        Guid accountId,
        string characterName,
        ClientConnection connection)
    {
        // TODO: Load character from DB
        // var characterData = await _characterRepository.GetByNameAsync(characterName);

        await Task.Delay(1); // Simulate async DB call

        // PROTOTYPE: Create new player
        Guid characterId = IdRegistry.Instance.GeneratePersistentId();
        var startPosition = new Position(100, 100);

        var playerEntity = new PlayerEntity(characterId, accountId, characterName, startPosition)
        {
            Level = 1,
            MaxHealth = 100,
            CurrentHealth = 100,
            MaxResource = 100,
            CurrentResource = 100,
            Race = Race.Human,
            Class = CharacterClass.Warrior,
            Faction = Faction.Player
        };

        var serverPlayer = new ServerPlayerCharacter(playerEntity, connection)
        {
            AccountId = accountId
        };

        // Add to ZoneManager
        zoneManager.AddPlayer(serverPlayer);

        log.Info(
            "Player spawned: {Name} (CharacterId: {CharacterId}, AccountId: {AccountId})",
            characterName, characterId, accountId);

        return serverPlayer;
    }

    public async Task<List<CharacterInfo>> GetCharacterListAsync(Guid accountId)
    {
        // TODO: Load from DB
        await Task.Delay(1);

        // PROTOTYPE: Empty list
        return new List<CharacterInfo>();
    }

    public async Task<CharacterCreateResult> CreateCharacterAsync(
        Guid accountId,
        string name,
        Race race,
        CharacterClass characterClass,
        Gender gender)
    {
        // TODO: Validation, DB insert
        await Task.Delay(1);

        // PROTOTYPE: Always successful
        return new CharacterCreateResult(
            true,
            Guid.NewGuid()
        );
    }

    public async Task RemovePlayerAsync(Guid connectionId)
    {
        ServerPlayerCharacter? player = zoneManager.RemovePlayerByConnectionId(connectionId);

        if (player != null)
        {
            // TODO: Save player data
            await SavePlayerAsync(player);

            log.Info("Player removed: {Name} (ConnectionId: {ConnectionId})",
                player.Name, connectionId);
        }
    }

    public async Task SavePlayerAsync(ServerPlayerCharacter playerCharacter)
    {
        // TODO: Save to DB
        await Task.Delay(1);

        log.Debug("Player saved: {Name}", playerCharacter.Name);
    }
}
