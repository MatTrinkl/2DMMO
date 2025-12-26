using MessagePack;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Chat.Messages;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Connection.Messages;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Messaging.Serialization;
using Mmo.Shared.Movement;
using Mmo.Shared.Zones.Messages.Client_Server;
using Mmo.Shared.Zones.Messages.Server_Brodcast;

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
        var original = new LoginRequest() { Username = "testuser", Password = "password123" };

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
        var accountId = Guid.NewGuid();
        var original = new LoginResponse(){Success = true,AccountId = accountId, AccountName = "Test"};

        byte[] serialized = MessagePackSerializer.Serialize(original);
        LoginResponse deserialized = MessagePackSerializer.Deserialize<LoginResponse>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.LoginResponse, deserialized.Type);
        Assert.True(deserialized.Success);
        Assert.Equal(accountId, deserialized.AccountId);
        Assert.Equal("Test", deserialized.AccountName);
        Assert.Null(deserialized.ErrorMessage);
    }

    [Fact]
    public void Serialize_Heartbeat_RoundTrip()
    {
        uint sessionSequenceNumber = 77;
        var original = new Heartbeat(){SequenceNumber = sessionSequenceNumber};

        byte[] serialized = MessagePackSerializer.Serialize(original);
        Heartbeat deserialized = MessagePackSerializer.Deserialize<Heartbeat>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Timestamp, deserialized.Timestamp);
        Assert.Equal(sessionSequenceNumber, deserialized.SequenceNumber);
    }

    [Fact]
    public void Serialize_Disconnect_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new ForceDisconnect() { Reason = DisconnectReason.ClientDisconnected };

        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        Assert.NotNull(deserialized);
        Assert.Equal(MessageType.ForceDisconnect, deserialized.Type);
    }

    // ══════════════════════════════════════════════════════════
    // MOVEMENT MESSAGES
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void Serialize_PositionUpdate_RoundTrip()
    {
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "TestPlayer", new Position(100, 200));
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
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "NewPlayer", new Position(10, 20));
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
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "LeavingPlayer", new Position(10, 20));
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
        var original = new LoginRequest() { Username = "", Password = "" };

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

    // ══════════════════════════════════════════════════════════
    // MESSAGE SERIALIZER DESERIALIZE TESTS (MessageSerializer.cs)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public void MessageSerializer_Deserialize_LoginRequest_ReturnsCorrectType()
    {
        var original = new LoginRequest() { Username = "user", Password = "pass" };
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<LoginRequest>(deserialized);
        var login = (LoginRequest)deserialized;
        Assert.Equal("user", login.Username);
    }

    [Fact]
    public void MessageSerializer_Deserialize_LoginResponse_ReturnsCorrectType()
    {
        var original = new LoginResponse(){Success = true, AccountId = Guid.NewGuid()};
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<LoginResponse>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_Heartbeat_ReturnsCorrectType()
    {
        var original = new Heartbeat();
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<Heartbeat>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_Disconnect_ReturnsCorrectType()
    {
        var original = new ForceDisconnect(){Reason = DisconnectReason.Timeout};
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<ForceDisconnect>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_LogoutRequest_ReturnsCorrectType()
    {
        var original = new LogoutRequest();
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<LogoutRequest>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_JoinZone_ReturnsCorrectType()
    {
        var original = new JoinZone(Guid.NewGuid());
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<JoinZone>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_LeaveZone_ReturnsCorrectType()
    {
        var original = new LeaveZone(Guid.NewGuid());
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<LeaveZone>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_PositionUpdate_ReturnsCorrectType()
    {
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(0, 0));
        var original = new PositionUpdate(12345L, player, new Position(10, 20));
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<PositionUpdate>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_PositionBroadcast_ReturnsCorrectType()
    {
        var original = new PositionBroadcast(12345L, Guid.NewGuid(), new Position(10, 20));
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<PositionBroadcast>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_ChatMessage_ReturnsCorrectType()
    {
        var original = new ChatMessage(Guid.NewGuid(), "Hello");
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<ChatMessage>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_ChatBroadcast_ReturnsCorrectType()
    {
        var original = new ChatBroadcast(Guid.NewGuid(), "Broadcast");
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<ChatBroadcast>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_PlayerJoinedZone_ReturnsCorrectType()
    {
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(0, 0));
        var original = new PlayerJoinedZone(player);
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<PlayerJoinedZone>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_PlayerLeftZone_ReturnsCorrectType()
    {
        var player = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player", new Position(0, 0));
        var original = new PlayerLeftZone(player);
        byte[] serialized = MessageSerializer.Serialize(original);

        INetworkMessage deserialized = MessageSerializer.Deserialize(serialized);

        Assert.IsType<PlayerLeftZone>(deserialized);
    }

    [Fact]
    public void MessageSerializer_Deserialize_CorruptedData_ThrowsException()
    {
        // Create a message with corrupted data
        var original = new LoginRequest() { Username = "user", Password = "pass" };
        byte[] serialized = MessageSerializer.Serialize(original);

        // Corrupt the data by setting the MessageType byte to an invalid value (byte.MaxValue = 255)
        // This causes MessagePack to fail when trying to deserialize the enum
        serialized[1] = byte.MaxValue;

        // MessagePack should throw when trying to deserialize corrupted data
        Assert.ThrowsAny<Exception>(() =>
            MessageSerializer.Deserialize(serialized));
    }
}
