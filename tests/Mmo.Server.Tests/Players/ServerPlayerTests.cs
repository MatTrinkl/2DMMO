using Mmo.Server.Entities;
using Mmo.Shared.Entities;
using Mmo.Shared.Records;

namespace Mmo.Server.Tests.Players;

public class ServerPlayerTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var persistentId = Guid.NewGuid();
        var connectionId = Guid.NewGuid();
        var entity = new PlayerEntity(persistentId, "TestPlayer", new Position(100, 200));

        var serverPlayer = new ServerPlayer(entity, connectionId);

        Assert.Equal(entity, serverPlayer.Entity);
        Assert.Equal(connectionId, serverPlayer.ConnectionId);
        Assert.Equal("TestPlayer", serverPlayer.Entity.DisplayName);
        Assert.Equal(persistentId, serverPlayer.Entity.PersistentId);
    }

    [Fact]
    public void ConnectedAt_IsSetToCurrentTime()
    {
        var entity = new PlayerEntity(Guid.NewGuid(), "Test", new Position(0, 0));
        DateTimeOffset before = DateTimeOffset.UtcNow;

        var serverPlayer = new ServerPlayer(entity, Guid.NewGuid());

        DateTimeOffset after = DateTimeOffset.UtcNow;
        Assert.True(serverPlayer.ConnectedAt >= before);
        Assert.True(serverPlayer.ConnectedAt <= after);
    }

    [Fact]
    public void LastActivity_CanBeUpdated()
    {
        var entity = new PlayerEntity(Guid.NewGuid(), "Test", new Position(0, 0));
        var serverPlayer = new ServerPlayer(entity, Guid.NewGuid());
        DateTimeOffset originalActivity = serverPlayer.LastActivity;

        Thread.Sleep(10); // Kleine Verzögerung
        serverPlayer.LastActivity = DateTimeOffset.UtcNow;

        Assert.True(serverPlayer.LastActivity > originalActivity);
    }
}
