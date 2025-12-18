using Mmo.Server.Entities;
using Mmo.Server.Networking;
using Mmo.Server.Zones;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Server.Services.Player;

/// <summary>
///     Konkrete Implementation des Player-Service.
/// </summary>
public class PlayerService(ZoneManager zoneManager, ILog log) : IPlayerService
{
    public async Task<ServerPlayerCharacter> SpawnPlayerAsync(
        Guid accountId,
        string characterName,
        ClientConnection connection)
    {
        // TODO: Charakter aus DB laden
        // var characterData = await _characterRepository.GetByNameAsync(characterName);

        await Task.Delay(1); // Simulate async DB call

        // PROTOTYPE: Erstelle neuen Spieler
        var characterId = Guid.NewGuid();
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

        // Zum ZoneManager hinzufügen
        zoneManager.AddPlayer(serverPlayer);

        log.Info(
            "Player spawned: {Name} (CharacterId: {CharacterId}, AccountId: {AccountId})",
            characterName, characterId, accountId);

        return serverPlayer;
    }

    public async Task<List<CharacterInfo>> GetCharacterListAsync(Guid accountId)
    {
        // TODO: Aus DB laden
        await Task.Delay(1);

        // PROTOTYPE: Leere Liste
        return new List<CharacterInfo>();
    }

    public async Task<CharacterData?> LoadCharacterAsync(Guid accountId, Guid characterId)
    {
        // TODO: Charakter aus DB laden
        await Task.Delay(1);

        // PROTOTYPE: Dummy-Charakter zurückgeben
        return new CharacterData(
            characterId,
            "TestCharacter",
            1,
            Race.Human,
            CharacterClass.Warrior,
            Gender.Male,
            Faction.Player,
            1, // ZoneId
            new Position(100, 100),
            100, // MaxHealth
            100, // CurrentHealth
            100, // MaxResource
            100  // CurrentResource
        );
    }

    public async Task<CharacterCreateResult> CreateCharacterAsync(
        Guid accountId,
        string name,
        Race race,
        CharacterClass characterClass,
        Gender gender)
    {
        // TODO: Validierung, DB-Insert
        await Task.Delay(1);

        // PROTOTYPE: Immer erfolgreich
        return new CharacterCreateResult(
            true,
            Guid.NewGuid()
        );
    }

    public async Task<bool> DeleteCharacterAsync(Guid accountId, Guid characterId)
    {
        // TODO: Charakter aus DB löschen
        await Task.Delay(1);

        // PROTOTYPE: Immer erfolgreich
        log.Info("Character deleted: {CharacterId} for account {AccountId}", characterId, accountId);
        return true;
    }

    public async Task RemovePlayerAsync(Guid connectionId)
    {
        ServerPlayerCharacter? player = zoneManager.RemovePlayerByConnectionId(connectionId);

        if (player != null)
        {
            // TODO: Spieler-Daten speichern
            await SavePlayerAsync(player);

            log.Info("Player removed: {Name} (ConnectionId: {ConnectionId})",
                player.Name, connectionId);
        }
    }

    public async Task SaveAndRemovePlayerAsync(Guid connectionId)
    {
        await RemovePlayerAsync(connectionId);
    }

    public async Task SavePlayerAsync(ServerPlayerCharacter playerCharacter)
    {
        // TODO: In DB speichern
        await Task.Delay(1);

        log.Debug("Player saved: {Name}", playerCharacter.Name);
    }
}
