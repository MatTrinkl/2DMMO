using System.Diagnostics;
using MessagePack;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Chat.Messages;
using Mmo.Shared.Connection.Messages;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Serialization;
using Mmo.Shared.Movement;
using Mmo.Shared.Zones.Messages.Server_Brodcast;
using Mmo.Shared.Zones.Messages.Server_Client;

namespace Mmo.Server.Tests.Networking;

/// <summary>
///     Tests for the MessageSerializer class.
/// </summary>
public class MessageSerializerTests
{
    [Fact]
    public void Serialize_LoginRequest_CreatesValidByteArray()
    {
        var message = new LoginRequest() { Username = "TestUser", Password = "TestPassword" };

        byte[] bytes = MessageSerializer.Serialize(message);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length >= 1);
        Assert.Equal((byte)MessageType.LoginRequest, bytes[1]);
    }

    [Fact]
    public void Serialize_Deserialize_LoginRequest_RoundTrip()
    {
        var original = new LoginRequest() { Username = "TestUser", Password = "TestPassword" };

        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (LoginRequest)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Username, deserialized.Username);
        Assert.Equal(MessageType.LoginRequest, deserialized.Type);
    }

    [Fact]
    public void Serialize_Deserialize_LoginResponse_RoundTrip()
    {
        var accountId = Guid.NewGuid();
        var original = new LoginResponse(){AccountId = accountId,Success = true,AccountName = "Test"};

        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (LoginResponse)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Success, deserialized.Success);
        Assert.Equal(original.AccountId, deserialized.AccountId);
        Assert.Null(deserialized.ErrorMessage);
    }

    [Fact]
    public void Serialize_Deserialize_PlayerJoined_RoundTrip()
    {
        var original = new PlayerJoinedZone
            (new PlayerEntity(Guid.Empty, Guid.NewGuid(), "Player1", new Position(100f, 200f)));


        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (PlayerJoinedZone)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Player.RuntimeId, deserialized.Player.RuntimeId);
        Assert.Equal(original.Player.DisplayName, deserialized.Player.DisplayName);
        Assert.Equal(original.Player.Position.X, deserialized.Player.Position.X);
        Assert.Equal(original.Player.Position.Y, deserialized.Player.Position.Y);
    }

    [Fact]
    public void Deserialize_ByType_ReturnsCorrectMessageType()
    {
        var original = new LoginRequest() { Username = "TestUser", Password = "TestPassword" };

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
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var original = new PositionUpdate(timestamp,
            new PlayerEntity(Guid.Empty, Guid.NewGuid(), "Player1", new Position(100f, 200f)),
            new Position(123.456f, 789.012f));

        byte[] bytes = MessageSerializer.Serialize(original);

        var deserialized = (PositionUpdate)MessageSerializer.Deserialize(bytes);

        Debug.Assert(original.EntityOldPosition != null);
        Debug.Assert(deserialized.EntityOldPosition != null);
        Assert.Equal(original.EntityOldPosition.RuntimeId, deserialized.EntityOldPosition.RuntimeId);
        Assert.Equal(original.EntityOldPosition.Position.X, deserialized.EntityOldPosition.Position.X);
        Debug.Assert(original.NewPosition != null);
        Debug.Assert(deserialized.NewPosition != null);
        Assert.Equal(original.NewPosition.Y, deserialized.NewPosition.Y);
        Assert.Equal(original.Timestamp, deserialized.Timestamp);
    }

    [Fact]
    public void Serialize_Deserialize_WorldState_RoundTrip()
    {
        ushort zoneId = 0;
        var original = new ZoneState(12345, zoneId,
        [
            new PlayerEntity(Guid.Empty, Guid.NewGuid(), "Player1", new Position(0, 0)),
            new PlayerEntity(Guid.Empty, Guid.NewGuid(), "Player2", new Position(0, 0))
        ]);

        byte[] bytes = MessageSerializer.Serialize(original);

        var deserialized = (ZoneState)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Timestamp, deserialized.Timestamp);
        Assert.Equal(2, deserialized.Entities.Count);
        Assert.Equal("Player1",
            ((PlayerEntity)deserialized.Entities[0]).DisplayName);
        Assert.Equal("Player2",
            ((PlayerEntity)deserialized.Entities[1]).DisplayName);
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
