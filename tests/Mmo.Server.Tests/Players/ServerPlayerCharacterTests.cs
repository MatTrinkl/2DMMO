using Mmo.Server.Connections;
using Mmo.Server.Entities;
using Mmo.Server.Player;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Core.Records;

namespace Mmo.Server.Tests.Players;

public class ServerPlayerCharacterTests
{
    private static readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer;

    public ServerPlayerCharacterTests()
    {
        _mockNetworkServer = new MockNetworkServer(_mockLog, true);
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var persistentId = Guid.NewGuid();
        var connectionId = Guid.NewGuid();
        var entity = new CharacterEntity(persistentId, Guid.NewGuid(), "TestPlayer", new Position(100, 200));
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(connectionId);

        var serverPlayer = new ServerPlayerCharacter(entity, connection);

        Assert.Equal(entity, serverPlayer.Entity);
        Assert.Equal(connectionId, serverPlayer.Connection.Id);
        Assert.Equal("TestPlayer", serverPlayer.Entity.DisplayName);
        Assert.Equal(persistentId, serverPlayer.Entity.PersistentId);
    }

    [Fact]
    public void ConnectedAt_IsSetToCurrentTime()
    {
        var entity = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Test", new Position(0, 0));
        DateTimeOffset before = DateTimeOffset.UtcNow;

        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        var serverPlayer = new ServerPlayerCharacter(entity, connection);

        DateTimeOffset after = DateTimeOffset.UtcNow;
        Assert.True(serverPlayer.ConnectedAt >= before);
        Assert.True(serverPlayer.ConnectedAt <= after);
    }

    [Fact]
    public void LastActivity_CanBeUpdated()
    {
        var entity = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Test", new Position(0, 0));
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        var serverPlayer = new ServerPlayerCharacter(entity, connection);
        DateTimeOffset originalActivity = serverPlayer.LastActivity;

        Thread.Sleep(10); // Kleine Verzögerung
        serverPlayer.LastActivity = DateTimeOffset.UtcNow;

        Assert.True(serverPlayer.LastActivity > originalActivity);
    }
}
