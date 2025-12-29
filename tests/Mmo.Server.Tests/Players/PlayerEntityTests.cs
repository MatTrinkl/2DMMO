using MessagePack;
using Mmo.Server.Entities;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Entities.Structs;

namespace Mmo.Shared.Tests.Entities;

public class PlayerEntityTests
{
    [Fact]
    public void Constructor_WithPersistentId_CreatesValidPlayer()
    {
        var persistentId = Guid.NewGuid();
        var position = new Position(100, 200);
        string displayName = "TestPlayer";

        var player = new CharacterEntity(persistentId, Guid.NewGuid(), displayName, position,
            EntityIdentity.Unassigned(0));

        Assert.Equal(persistentId, player.PersistentId);
        Assert.Equal(displayName, player.DisplayName);
        Assert.Equal(position, player.Position);
        Assert.True(player.IsTrulyPersistent);
        Assert.Equal(EntityType.Player, player.Type);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_CreatesPlayer()
    {
        var player = new CharacterEntity(Guid.Empty, Guid.NewGuid(), "EmptyGuidPlayer", new Position(0, 0),
            EntityIdentity.Unassigned(0));

        Assert.Equal(Guid.Empty, player.PersistentId);
        Assert.Equal("EmptyGuidPlayer", player.DisplayName);
    }

    [Fact]
    public void SetEntityId_UpdatesEntityIdCorrectly()
    {
        var player = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(50, 50),
            EntityIdentity.Unassigned(0));

        player.SetEntityId(42, 100);

        Assert.Equal(42, player.RuntimeId.LocalId);
        Assert.Equal(100, player.RuntimeId.ZoneId);
        Assert.Equal(0, player.RuntimeId.ShardId);
    }

    [Fact]
    public void SetEntityId_CalledMultipleTimes_UpdatesCorrectly()
    {
        var player = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(75, 75),
            EntityIdentity.Unassigned(0));

        player.SetEntityId(1, 10);
        Assert.Equal(1, player.RuntimeId.LocalId);
        Assert.Equal(10, player.RuntimeId.ZoneId);

        player.SetEntityId(2, 20);
        Assert.Equal(2, player.RuntimeId.LocalId);
        Assert.Equal(20, player.RuntimeId.ZoneId);
    }

    [Fact]
    public void IsTrulyPersistent_IsTrue()
    {
        var player = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "PersistentPlayer", new Position(0, 0),
            EntityIdentity.Unassigned(0));

        Assert.True(player.IsTrulyPersistent);
    }

    [Fact]
    public void Position_CanBeUpdated()
    {
        var player = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "MovingPlayer", new Position(10, 20),
            EntityIdentity.Unassigned(0));
        var newPosition = new Position(30, 40);

        player.Position = newPosition;

        Assert.Equal(newPosition, player.Position);
        Assert.Equal(30, player.Position.X);
        Assert.Equal(40, player.Position.Y);
    }

    [Fact]
    public void ChangeZone_DoesNotThrowException()
    {
        var player = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "ZoneChanger", new Position(0, 0),
            EntityIdentity.Unassigned(0));

        // Should not throw - method is currently a no-op (WIP)
        player.ChangeZone(5);
    }

    [Fact]
    public void Type_AlwaysReturnsPlayer()
    {
        var player1 = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(0, 0),
            EntityIdentity.Unassigned(0));
        var player2 = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(100, 100),
            EntityIdentity.Unassigned(0));

        Assert.Equal(EntityType.Player, player1.Type);
        Assert.Equal(EntityType.Player, player2.Type);
    }


    [Fact]
    public void TwoPlayers_WithDifferentGuids_HaveDifferentPersistentIds()
    {
        var player1 = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(0, 0),
            EntityIdentity.Unassigned(0));
        var player2 = new CharacterEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(0, 0),
            EntityIdentity.Unassigned(0));

        Assert.NotEqual(player1.PersistentId, player2.PersistentId);
    }

    [Fact]
    public void TwoPlayers_WithSameGuid_HaveSamePersistentIds()
    {
        var sharedGuid = Guid.NewGuid();
        var player1 = new CharacterEntity(sharedGuid, Guid.NewGuid(), "Player1", new Position(0, 0),
            EntityIdentity.Unassigned(0));
        var player2 = new CharacterEntity(sharedGuid, Guid.NewGuid(), "Player2", new Position(0, 0),
            EntityIdentity.Unassigned(0));

        Assert.Equal(player1.PersistentId, player2.PersistentId);
    }
}
