using MessagePack;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Messages.ZoneEvents;
using Mmo.Shared.Records;

namespace Mmo.Shared.Tests.Serialization;

/// <summary>
///     Tests for MessagePack serialization of all message types.
/// </summary>
public class MessageSerializerTests
{
    // ══════════════════════════════════════════════════════════
    // CONNECTION MESSAGES
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void Serialize_LoginRequest_RoundTrip()
    {
        var original = new LoginRequest("testuser", "password123");

        byte[] serialized = MessagePackSerializer.Serialize(original);
        LoginRequest deserialized = MessagePackSerializer.Deserialize<LoginRequest>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.LoginRequest, deserialized.Type);
        Assert.Equal("testuser", deserialized.Username);
        Assert.Equal("password123", deserialized.Password);
    }

    [Fact]
    public void Serialize_LoginResponse_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new LoginResponse(true, playerId, 0, null);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        LoginResponse deserialized = MessagePackSerializer.Deserialize<LoginResponse>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.LoginResponse, deserialized.Type);
        Assert.True(deserialized.Success);
        Assert.Equal(playerId, deserialized.PlayerId);
        Assert.Equal(0, deserialized.ZoneId);
        Assert.Null(deserialized.ErrorMessage);
    }

    [Fact]
    public void Serialize_Heartbeat_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new Heartbeat(12345L, playerId);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        Heartbeat deserialized = MessagePackSerializer.Deserialize<Heartbeat>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.Heartbeat, deserialized.Type);
        Assert.Equal(12345L, deserialized.Timestamp);
        Assert.Equal(playerId, deserialized.PlayerId);
    }

    [Fact]
    public void Serialize_Disconnect_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new Disconnect(playerId);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        Disconnect deserialized = MessagePackSerializer.Deserialize<Disconnect>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.Disconnect, deserialized.Type);
        Assert.Equal(playerId, deserialized.PlayerId);
    }

    // ══════════════════════════════════════════════════════════
    // MOVEMENT MESSAGES
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void Serialize_PositionUpdate_RoundTrip()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "TestPlayer", new Position(100, 200));
        var newPos = new Position(110, 210);
        var original = new PositionUpdate(98765L, player, newPos);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        PositionUpdate deserialized = MessagePackSerializer.Deserialize<PositionUpdate>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.PositionUpdate, deserialized.Type);
        Assert.Equal(98765L, deserialized.Timestamp);
        Assert.NotNull(deserialized.NewPosition);
        Assert.Equal(110, deserialized.NewPosition.X);
        Assert.Equal(210, deserialized.NewPosition.Y);
    }

    [Fact]
    public void Serialize_PositionBroadcast_RoundTrip()
    {
        var entityId = Guid.NewGuid();
        var newPos = new Position(50, 75);
        var original = new PositionBroadcast(11111L, entityId, newPos);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        PositionBroadcast deserialized = MessagePackSerializer.Deserialize<PositionBroadcast>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.PositionBroadcast, deserialized.Type);
        Assert.Equal(11111L, deserialized.Timestamp);
        Assert.Equal(entityId, deserialized.EntityId);
        Assert.NotNull(deserialized.NewPosition);
        Assert.Equal(50, deserialized.NewPosition.X);
        Assert.Equal(75, deserialized.NewPosition.Y);
    }

    // ══════════════════════════════════════════════════════════
    // CHAT MESSAGES
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void Serialize_ChatMessage_RoundTrip()
    {
        var entityId = Guid.NewGuid();
        var original = new ChatMessage(entityId, "Hello, world!");

        byte[] serialized = MessagePackSerializer.Serialize(original);
        ChatMessage deserialized = MessagePackSerializer.Deserialize<ChatMessage>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.ChatMessage, deserialized.Type);
        Assert.Equal("Hello, world!", deserialized.Message);
        Assert.Equal(entityId, deserialized.EntityId);
    }

    [Fact]
    public void Serialize_ChatBroadcast_RoundTrip()
    {
        var entityId = Guid.NewGuid();
        var original = new ChatBroadcast(entityId, "Broadcast message");

        byte[] serialized = MessagePackSerializer.Serialize(original);
        ChatBroadcast deserialized = MessagePackSerializer.Deserialize<ChatBroadcast>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.ChatBroadcast, deserialized.Type);
        Assert.Equal("Broadcast message", deserialized.Message);
        Assert.Equal(entityId, deserialized.EntityId);
    }

    // ══════════════════════════════════════════════════════════
    // ZONE EVENT MESSAGES
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void Serialize_JoinZone_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new JoinZone(playerId);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        JoinZone deserialized = MessagePackSerializer.Deserialize<JoinZone>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.JoinZone, deserialized.Type);
        Assert.Equal(playerId, deserialized.PlayerId);
    }

    [Fact]
    public void Serialize_LeaveZone_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new LeaveZone(playerId);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        LeaveZone deserialized = MessagePackSerializer.Deserialize<LeaveZone>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.LeaveZone, deserialized.Type);
        Assert.Equal(playerId, deserialized.PlayerId);
    }

    [Fact]
    public void Serialize_PlayerJoinedZone_RoundTrip()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "NewPlayer", new Position(10, 20));
        var original = new PlayerJoinedZone(player);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        PlayerJoinedZone deserialized = MessagePackSerializer.Deserialize<PlayerJoinedZone>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.PlayerJoinedZone, deserialized.Type);
        Assert.NotNull(deserialized.Player);
        Assert.Equal("NewPlayer", deserialized.Player.DisplayName);
    }

    [Fact]
    public void Serialize_PlayerLeftZone_RoundTrip()
    {
        var player = new PlayerEntity(Guid.NewGuid(), "LeavingPlayer", new Position(10, 20));
        var original = new PlayerLeftZone(player);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        PlayerLeftZone deserialized = MessagePackSerializer.Deserialize<PlayerLeftZone>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.PlayerLeftZone, deserialized.Type);
        Assert.NotNull(deserialized.Player);
        Assert.Equal("LeavingPlayer", deserialized.Player.DisplayName);
    }

    // ══════════════════════════════════════════════════════════
    // EDGE CASES
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void Serialize_EmptyStrings_RoundTrip()
    {
        var original = new LoginRequest("", "");

        byte[] serialized = MessagePackSerializer.Serialize(original);
        LoginRequest deserialized = MessagePackSerializer.Deserialize<LoginRequest>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal("", deserialized.Username);
        Assert.Equal("", deserialized.Password);
    }

    [Fact]
    public void Serialize_EmptyGuid_RoundTrip()
    {
        var original = new JoinZone(Guid.Empty);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        JoinZone deserialized = MessagePackSerializer.Deserialize<JoinZone>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(Guid.Empty, deserialized.PlayerId);
    }

    [Fact]
    public void Serialize_LongTimestamp_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new Heartbeat(long.MaxValue, playerId);

        byte[] serialized = MessagePackSerializer.Serialize(original);
        Heartbeat deserialized = MessagePackSerializer.Deserialize<Heartbeat>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(long.MaxValue, deserialized.Timestamp);
        Assert.Equal(playerId, deserialized.PlayerId);
    }

    [Fact]
    public void Serialize_SpecialCharacters_RoundTrip()
    {
        var entityId = Guid.NewGuid();
        var original = new ChatMessage(entityId, "Hello 你好 🎮 \n\t\r");

        byte[] serialized = MessagePackSerializer.Serialize(original);
        ChatMessage deserialized = MessagePackSerializer.Deserialize<ChatMessage>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal("Hello 你好 🎮 \n\t\r", deserialized.Message);
        Assert.Equal(entityId, deserialized.EntityId);
    }
}
