using Mmo.Server.Network;
using Mmo.Server.Players;
using Mmo.Server.ServicePlayer;
using Mmo.Server.ServicePlayer.Records;
using Mmo.Server.Tests.Helpers;
using Mmo.Server.Zones;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.Tests.Services;

[Collection("IdRegistry")]
public class PlayerServiceTests : IDisposable
{
    private readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer;
    private readonly PlayerService _playerService;
    private readonly ZoneManager _zoneManager;

    public PlayerServiceTests()
    {
        IdRegistry.Instance.Clear();
        _mockNetworkServer = new MockNetworkServer(_mockLog, true);
        var defaultZone = new Zone(0, "default", new ZoneBounds(0, 0, 1000, 1000));
        _zoneManager = new ZoneManager(0, defaultZone);
        _playerService = new PlayerService(_zoneManager, _mockLog);
    }

    public void Dispose() => IdRegistry.Instance.Clear();

    [Fact]
    public async Task SpawnPlayerAsync_CreatesPlayerWithCorrectProperties()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        string characterName = "TestCharacter";
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());

        // Act
        ServerPlayerCharacter result = await _playerService.SpawnPlayerAsync(accountId, characterName, connection);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(characterName, result.Entity.DisplayName);
        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(new Position(100, 100), result.Entity.Position);
    }

    [Fact]
    public async Task SpawnPlayerAsync_SetsDefaultCombatProperties()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());

        // Act
        ServerPlayerCharacter result = await _playerService.SpawnPlayerAsync(accountId, "Player", connection);

        // Assert
        Assert.Equal(1, result.Entity.Level);
        Assert.Equal(100, result.Entity.MaxHealth);
        Assert.Equal(100, result.Entity.CurrentHealth);
        Assert.Equal(100, result.Entity.MaxResource);
        Assert.Equal(100, result.Entity.CurrentResource);
    }

    [Fact]
    public async Task SpawnPlayerAsync_SetsDefaultCharacterAttributes()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());

        // Act
        ServerPlayerCharacter result = await _playerService.SpawnPlayerAsync(accountId, "Player", connection);

        // Assert
        Assert.Equal(Race.Human, result.Entity.Race);
        Assert.Equal(CharacterClass.Warrior, result.Entity.Class);
        Assert.Equal(Faction.Player, result.Entity.Faction);
    }

    [Fact]
    public async Task SpawnPlayerAsync_AddsPlayerToZoneManager()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());

        // Act
        ServerPlayerCharacter result = await _playerService.SpawnPlayerAsync(accountId, "Player", connection);

        // Assert
        Assert.Equal(1, _zoneManager.PlayerCount);
        Assert.True(_zoneManager.HasPlayerWithConnectionId(connection.Id));
    }

    [Fact]
    public async Task SpawnPlayerAsync_LogsPlayerSpawn()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());

        // Act
        await _playerService.SpawnPlayerAsync(accountId, "SpawnedPlayer", connection);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("INFO", "Player spawned"));
    }

    [Fact]
    public async Task GetCharacterListAsync_ReturnsEmptyList()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        List<CharacterInfo> result = await _playerService.GetCharacterListAsync(accountId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateCharacterAsync_ReturnsSuccessfulResult()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        CharacterCreateResult result = await _playerService.CreateCharacterAsync(
            accountId, "NewCharacter", Race.Elf, CharacterClass.Mage, Gender.Female);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.CharacterId);
        Assert.NotEqual(Guid.Empty, result.CharacterId.Value);
    }

    [Fact]
    public async Task RemovePlayerAsync_RemovesExistingPlayer()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        await _playerService.SpawnPlayerAsync(accountId, "ToRemove", connection);

        // Act
        await _playerService.RemovePlayerAsync(connection.Id);

        // Assert
        Assert.Equal(0, _zoneManager.PlayerCount);
    }

    [Fact]
    public async Task RemovePlayerAsync_NonExistentPlayer_DoesNotThrow()
    {
        // Arrange
        var nonExistentConnectionId = Guid.NewGuid();

        // Act & Assert - Should not throw
        await _playerService.RemovePlayerAsync(nonExistentConnectionId);
    }

    [Fact]
    public async Task RemovePlayerAsync_LogsPlayerRemoval()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        await _playerService.SpawnPlayerAsync(accountId, "LoggedPlayer", connection);

        // Act
        await _playerService.RemovePlayerAsync(connection.Id);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("INFO", "Player removed"));
    }

    [Fact]
    public async Task SavePlayerAsync_LogsPlayerSave()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        ServerPlayerCharacter player = await _playerService.SpawnPlayerAsync(accountId, "SavedPlayer", connection);

        // Act
        await _playerService.SavePlayerAsync(player);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("DEBUG", "Player saved"));
    }

    [Fact]
    public async Task SpawnPlayerAsync_MultipleSpawns_EachHasUniqueCharacterId()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        ClientConnection connection1 = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        ClientConnection connection2 = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());

        // Act
        ServerPlayerCharacter player1 = await _playerService.SpawnPlayerAsync(accountId, "Player1", connection1);
        ServerPlayerCharacter player2 = await _playerService.SpawnPlayerAsync(accountId, "Player2", connection2);

        // Assert
        Assert.NotEqual(player1.Entity.PersistentId, player2.Entity.PersistentId);
    }
}
