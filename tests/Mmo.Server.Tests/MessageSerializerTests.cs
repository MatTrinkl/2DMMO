using System.Diagnostics;
using MessagePack;
using Mmo.Shared;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Messages.ZoneEvents;
using Mmo.Shared.Records;
using Mmo.Shared.Serialization;

namespace Mmo.Server.Tests;

/// <summary>
///     Tests for the MessageSerializer class.
/// </summary>
public class MessageSerializerTests
{
    [Fact]
    public void Serialize_LoginRequest_CreatesValidByteArray()
    {
        var message = new LoginRequest("TestUser", "TestPassword");

        byte[] bytes = MessageSerializer.Serialize(message);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length >= SharedConstants.MessageHeaderSize);
        Assert.Equal((byte)MessageType.LoginRequest, bytes[1]);
    }

    [Fact]
    public void Serialize_Deserialize_LoginRequest_RoundTrip()
    {
        var original = new LoginRequest("TestUser", "TestPassword");

        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (LoginRequest)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Username, deserialized.Username);
        Assert.Equal(MessageType.LoginRequest, deserialized.Type);
    }

    [Fact]
    public void Serialize_Deserialize_LoginResponse_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new LoginResponse(true, playerId, null);

        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (LoginResponse)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Success, deserialized.Success);
        Assert.Equal(original.PlayerId, deserialized.PlayerId);
        Assert.Null(deserialized.ErrorMessage);
    }

    [Fact]
    public void Serialize_Deserialize_PlayerJoined_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new PlayerJoinedZone
            (new PlayerState(playerId, username:"Player1", 100f, 200f));


        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (PlayerJoinedZone)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Player.PlayerId, deserialized.Player.PlayerId);
        Assert.Equal(original.Player.Username, deserialized.Player.Username);
        Assert.Equal(original.Player.Position.X, deserialized.Player.Position.X);
        Assert.Equal(original.Player.Position.Y, deserialized.Player.Position.Y);
    }

    [Fact]
    public void Deserialize_ByType_ReturnsCorrectMessageType()
    {
        var original = new LoginRequest("TestUser", "TestPassword");

        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (LoginRequest)MessageSerializer.Deserialize(bytes);

        Assert.IsType<LoginRequest>(deserialized);
        Assert.Equal("TestUser", deserialized.Username);
    }

    [Fact]
    public void Deserialize_ByType_ThrowsForUnknownType()
    {
        byte[] payload = new byte[10];

        Assert.Throws<MessagePackSerializationException>(() =>
            MessageSerializer.Deserialize(payload));
    }

    [Fact]
    public void Serialize_Deserialize_PositionUpdate_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var original = new PositionUpdate(timestamp, new PlayerState(playerId, "Player1", new Position(100f, 200f)),
            new Position(123.456f, 789.012f));
        ;

        byte[] bytes = MessageSerializer.Serialize(original);

        var deserialized = (PositionUpdate)MessageSerializer.Deserialize(bytes);

        Debug.Assert(original.EntityOldPosition != null, "original.EntityOldPosition != null");
        Debug.Assert(deserialized.EntityOldPosition != null, "deserialized.EntityOldPosition != null");
        Assert.Equal(original.EntityOldPosition.EntityId, deserialized.EntityOldPosition.EntityId);
        Assert.Equal(original.EntityOldPosition.Position.X, deserialized.EntityOldPosition.Position.X);
        Debug.Assert(original.NewPosition != null, "original.NewPosition != null");
        Debug.Assert(deserialized.NewPosition != null, "deserialized.NewPosition != null");
        Assert.Equal(original.NewPosition.Y, deserialized.NewPosition.Y);
        Assert.Equal(original.Timestamp, deserialized.Timestamp);
    }

    [Fact]
    public void Serialize_Deserialize_WorldState_RoundTrip()
    {
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var original = new ZoneState(12345, zoneId,
            [new PlayerState(player1Id, "Player1", 0, 0), new PlayerState(player2Id, "Player2", 0, 0)]);

        byte[] bytes = MessageSerializer.Serialize(original);

        var deserialized = (ZoneState)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Timestamp, deserialized.Timestamp);
        Assert.Equal(2, deserialized.Entities.Count);
        Assert.Equal(player1Id, deserialized.Entities[0].EntityId);
        Assert.Equal("Player1",
            ((PlayerState)deserialized.Entities[0]).Username);
    }

    [Fact]
    public void Serialize_Deserialize_ChatMessage_RoundTrip()
    {
        var senderId = Guid.NewGuid();
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var original = new ChatMessage(senderId, "Hello, World!");


        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (ChatMessage)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.EntityId, deserialized.EntityId);
        Assert.Equal(original.Message, deserialized.Message);
    }
}
