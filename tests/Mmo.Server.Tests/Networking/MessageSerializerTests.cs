using System.Diagnostics;
using MessagePack;
using Mmo.Server.Entities;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Chat.Messages;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Serialization;
using Mmo.Shared.Movement;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Prefab;

namespace Mmo.Server.Tests.Networking;

/// <summary>
///     Tests for the MessageSerializer class.
/// </summary>
public class MessageSerializerTests
{
    [Fact]
    public void Serialize_LoginRequest_CreatesValidByteArray()
    {
        var message = new LoginRequest { Username = "TestUser", Password = "TestPassword" };

        byte[] bytes = MessageSerializer.Serialize(message);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length >= 1);
        Assert.Equal((byte)MessageType.LoginRequest, bytes[1]);
    }

    [Fact]
    public void Serialize_Deserialize_LoginRequest_RoundTrip()
    {
        var original = new LoginRequest { Username = "TestUser", Password = "TestPassword" };

        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (LoginRequest)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Username, deserialized.Username);
        Assert.Equal(MessageType.LoginRequest, deserialized.Type);
    }

    [Fact]
    public void Serialize_Deserialize_LoginResponse_RoundTrip()
    {
        var accountId = Guid.NewGuid();
        var original = new LoginResponse { AccountId = accountId, Success = true, AccountName = "Test" };

        byte[] bytes = MessageSerializer.Serialize(original);
        var deserialized = (LoginResponse)MessageSerializer.Deserialize(bytes);

        Assert.Equal(original.Success, deserialized.Success);
        Assert.Equal(original.AccountId, deserialized.AccountId);
        Assert.Null(deserialized.ErrorMessage);
    }


    [Fact]
    public void Deserialize_ByType_ReturnsCorrectMessageType()
    {
        var original = new LoginRequest { Username = "TestUser", Password = "TestPassword" };

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
        var characterEntity = new CharacterEntity(Guid.Empty, Guid.NewGuid(), "Player1", new Position(100f, 200f), EntityIdentity.Unassigned(PrefabIds.PlayerDefault));
        var original = new PositionUpdate(timestamp,
            characterEntity.ToDto(),
            new Position(123.456f, 789.012f));

        byte[] bytes = MessageSerializer.Serialize(original);

        var deserialized = (PositionUpdate)MessageSerializer.Deserialize(bytes);

        Debug.Assert(original.Entity != null);
        Debug.Assert(deserialized.Entity != null);
        var originalDto = (CharacterEntityDto)original.Entity;
        var deserializedDto = (CharacterEntityDto)deserialized.Entity;
        Assert.Equal(originalDto.PersistentId, deserializedDto.PersistentId);
        Assert.Equal(originalDto.Position.X, deserializedDto.Position.X);
        Debug.Assert(original.NewPosition != null);
        Debug.Assert(deserialized.NewPosition != null);
        Assert.Equal(original.NewPosition.Y, deserialized.NewPosition.Y);
        Assert.Equal(original.Timestamp, deserialized.Timestamp);
    }

    //Todo: ZoneStateTests

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
