using Mmo.Server.Connections;
using Mmo.Server.Player;
using Mmo.Server.PlayerService.Records;
using Mmo.Server.Tests.Helpers;
using Mmo.Server.Zones;
using Mmo.Server.Zones.Configurations;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Records;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones.Structs;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Prefab;

namespace Mmo.Server.Tests.Services;

[Collection("IdRegistry")]
public class PlayerServiceTests : IDisposable
{
    private readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer;
    private readonly Player.Service.PlayerService _playerService;
    private readonly ZoneManager _zoneManager;

    public PlayerServiceTests()
    {
        IdRegistry.Instance.Clear();
        _mockNetworkServer = new MockNetworkServer(_mockLog, true);
        var defaultZone = TestHelpers.CreateTestZone(0, "default");
        _zoneManager = new ZoneManager(0);
        _zoneManager.RegisterZone(defaultZone);
        _playerService = new Player.Service.PlayerService(_zoneManager, _mockLog);
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
        await _playerService.SpawnPlayerAsync(accountId, "Player", connection);

        // Assert
        Assert.Single(_zoneManager.GetAllServerPlayers());
        Assert.True(_zoneManager.TryGetPlayerByConnectionId(connection.Id, out _));
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
        Assert.Empty(_zoneManager.GetAllServerPlayers());
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
