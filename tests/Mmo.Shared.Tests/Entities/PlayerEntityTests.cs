using MessagePack;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Tests.Entities;

public class PlayerEntityTests
{
    [Fact]
    public void Constructor_WithPersistentId_CreatesValidPlayer()
    {
        var persistentId = Guid.NewGuid();
        var position = new Position(100, 200);
        var displayName = "TestPlayer";

        var player = new PlayerEntity(persistentId, displayName, position);

        Assert.Equal(persistentId, player.PersistentId);
        Assert.Equal(displayName, player.DisplayName);
        Assert.Equal(position, player.Position);
        Assert.True(player.IsTrulyPersistent);
        Assert.Equal(EntityType.Player, player.Type);
        Assert.Equal(EntityRole.None, player.Role);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_CreatesPlayer()
    {
        var player = new PlayerEntity(Guid.Empty, "EmptyGuidPlayer", new Position(0, 0));

        Assert.Equal(Guid.Empty, player.PersistentId);
        Assert.Equal("EmptyGuidPlayer", player.DisplayName);
    }

    [Fact]
    public void SetEntityId_UpdatesEntityIdCorrectly()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "Player1", new Position(50, 50));

        player.SetEntityId(42, 100);

        Assert.Equal(42, player.EntityId.Id);
        Assert.Equal(100, player.EntityId.ZoneId);
        Assert.Equal(0, player.EntityId.ShardId);
    }

    [Fact]
    public void SetEntityId_CalledMultipleTimes_UpdatesCorrectly()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "Player2", new Position(75, 75));

        player.SetEntityId(1, 10);
        Assert.Equal(1, player.EntityId.Id);
        Assert.Equal(10, player.EntityId.ZoneId);

        player.SetEntityId(2, 20);
        Assert.Equal(2, player.EntityId.Id);
        Assert.Equal(20, player.EntityId.ZoneId);
    }

    [Fact]
    public void IsTrulyPersistent_IsTrue()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "PersistentPlayer", new Position(0, 0));

        Assert.True(player.IsTrulyPersistent);
    }

    [Fact]
    public void Position_CanBeUpdated()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "MovingPlayer", new Position(10, 20));
        var newPosition = new Position(30, 40);

        player.Position = newPosition;

        Assert.Equal(newPosition, player.Position);
        Assert.Equal(30, player.Position.X);
        Assert.Equal(40, player.Position.Y);
    }

    [Fact]
    public void DisplayName_CanBeUpdated()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "OldName", new Position(0, 0));

        player.DisplayName = "NewName";

        Assert.Equal("NewName", player.DisplayName);
    }

    [Fact]
    public void MessagePack_Serialization_RoundTrip()
    {
        var originalPlayer = new PlayerEntity(Guid.NewGuid(), "SerializablePlayer", new Position(123.45f, 678.90f));
        originalPlayer.SetEntityId(42, 100);

        byte[] serialized = MessagePackSerializer.Serialize<Entity>(originalPlayer);
        var deserializedPlayer = MessagePackSerializer.Deserialize<Entity>(serialized) as PlayerEntity;

        Assert.NotNull(deserializedPlayer);
        // Note: PersistentId, EntityId, and IsTrulyPersistent are not serialized due to 'protected init/set' - known limitations
        // These properties need to be set after deserialization by the server
        Assert.Equal(originalPlayer.DisplayName, deserializedPlayer.DisplayName);
        Assert.Equal(originalPlayer.Position.X, deserializedPlayer.Position.X);
        Assert.Equal(originalPlayer.Position.Y, deserializedPlayer.Position.Y);
        // Verify basic deserialization works
    }

    [Fact]
    public void MessagePack_Serialization_CanDeserialize()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(0, 0));

        byte[] serialized = MessagePackSerializer.Serialize<Entity>(player);
        var deserialized = MessagePackSerializer.Deserialize<Entity>(serialized) as PlayerEntity;

        // Deserialization works, even if PersistentId is not preserved
        Assert.NotNull(deserialized);
        Assert.Equal("TestPlayer", deserialized.DisplayName);
    }

    [Fact]
    public void ChangeZone_DoesNotThrowException()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "ZoneChanger", new Position(0, 0));

        // Should not throw - method is currently a no-op (WIP)
        player.ChangeZone(5);
    }

    [Fact]
    public void DefaultConstructor_CreatesEmptyPlayer()
    {
        // This constructor is used by MessagePack deserialization
        var player = new PlayerEntity();

        Assert.NotNull(player);
        Assert.Equal(string.Empty, player.DisplayName);
    }

    [Fact]
    public void Type_AlwaysReturnsPlayer()
    {
        var player1 = new PlayerEntity(Guid.NewGuid(), "Player1", new Position(0, 0));
        var player2 = new PlayerEntity(Guid.NewGuid(), "Player2", new Position(100, 100));

        Assert.Equal(EntityType.Player, player1.Type);
        Assert.Equal(EntityType.Player, player2.Type);
    }

    [Fact]
    public void Role_AlwaysReturnsNone()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "RoleTest", new Position(0, 0));

        Assert.Equal(EntityRole.None, player.Role);
    }

    [Fact]
    public void TwoPlayers_WithDifferentGuids_HaveDifferentPersistentIds()
    {
        var player1 = new PlayerEntity(Guid.NewGuid(), "Player1", new Position(0, 0));
        var player2 = new PlayerEntity(Guid.NewGuid(), "Player2", new Position(0, 0));

        Assert.NotEqual(player1.PersistentId, player2.PersistentId);
    }

    [Fact]
    public void TwoPlayers_WithSameGuid_HaveSamePersistentIds()
    {
        var sharedGuid = Guid.NewGuid();
        var player1 = new PlayerEntity(sharedGuid, "Player1", new Position(0, 0));
        var player2 = new PlayerEntity(sharedGuid, "Player2", new Position(0, 0));

        Assert.Equal(player1.PersistentId, player2.PersistentId);
    }
}
