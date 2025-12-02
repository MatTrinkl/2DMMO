using Mmo.Shared.Enums;
using Mmo.Shared.Messages;
using Mmo.Shared.Serialization;
using Xunit;

namespace Mmo.Server.Tests;

/// <summary>
/// Tests for the MessageSerializer class.
/// </summary>
public class MessageSerializerTests
{
    [Fact]
    public void Serialize_LoginRequest_CreatesValidByteArray()
    {
        var message = new LoginRequest { Username = "TestUser" };

        var bytes = MessageSerializer.Serialize(message);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length >= 5); // At least header size
        Assert.Equal((byte)MessageType.LoginRequest, bytes[0]);
    }

    [Fact]
    public void Serialize_Deserialize_LoginRequest_RoundTrip()
    {
        var original = new LoginRequest { Username = "TestUser" };

        var bytes = MessageSerializer.Serialize(original);
        var payload = new byte[bytes.Length - 5];
        Array.Copy(bytes, 5, payload, 0, payload.Length);

        var deserialized = MessageSerializer.Deserialize<LoginRequest>(payload);

        Assert.Equal(original.Username, deserialized.Username);
        Assert.Equal(MessageType.LoginRequest, deserialized.Type);
    }

    [Fact]
    public void Serialize_Deserialize_LoginResponse_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new LoginResponse
        {
            Success = true,
            PlayerId = playerId,
            ErrorMessage = null
        };

        var bytes = MessageSerializer.Serialize(original);
        var payload = new byte[bytes.Length - 5];
        Array.Copy(bytes, 5, payload, 0, payload.Length);

        var deserialized = MessageSerializer.Deserialize<LoginResponse>(payload);

        Assert.Equal(original.Success, deserialized.Success);
        Assert.Equal(original.PlayerId, deserialized.PlayerId);
        Assert.Null(deserialized.ErrorMessage);
    }

    [Fact]
    public void Serialize_Deserialize_PlayerJoined_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var original = new PlayerJoined
        {
            PlayerId = playerId,
            Username = "TestPlayer",
            X = 100.5f,
            Y = 200.5f
        };

        var bytes = MessageSerializer.Serialize(original);
        var payload = new byte[bytes.Length - 5];
        Array.Copy(bytes, 5, payload, 0, payload.Length);

        var deserialized = MessageSerializer.Deserialize<PlayerJoined>(payload);

        Assert.Equal(original.PlayerId, deserialized.PlayerId);
        Assert.Equal(original.Username, deserialized.Username);
        Assert.Equal(original.X, deserialized.X);
        Assert.Equal(original.Y, deserialized.Y);
    }

    [Fact]
    public void Deserialize_ByType_ReturnsCorrectMessageType()
    {
        var original = new LoginRequest { Username = "TypeTest" };

        var bytes = MessageSerializer.Serialize(original);
        var payload = new byte[bytes.Length - 5];
        Array.Copy(bytes, 5, payload, 0, payload.Length);

        var deserialized = MessageSerializer.Deserialize(MessageType.LoginRequest, payload);

        Assert.IsType<LoginRequest>(deserialized);
        Assert.Equal("TypeTest", ((LoginRequest)deserialized).Username);
    }

    [Fact]
    public void Deserialize_ByType_ThrowsForUnknownType()
    {
        var payload = new byte[10];

        Assert.Throws<ArgumentException>(() =>
            MessageSerializer.Deserialize((MessageType)255, payload));
    }

    [Fact]
    public void Serialize_Deserialize_PositionUpdate_RoundTrip()
    {
        var playerId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var original = new PositionUpdate
        {
            PlayerId = playerId,
            X = 123.456f,
            Y = 789.012f,
            Timestamp = timestamp
        };

        var bytes = MessageSerializer.Serialize(original);
        var payload = new byte[bytes.Length - 5];
        Array.Copy(bytes, 5, payload, 0, payload.Length);

        var deserialized = MessageSerializer.Deserialize<PositionUpdate>(payload);

        Assert.Equal(original.PlayerId, deserialized.PlayerId);
        Assert.Equal(original.X, deserialized.X);
        Assert.Equal(original.Y, deserialized.Y);
        Assert.Equal(original.Timestamp, deserialized.Timestamp);
    }

    [Fact]
    public void Serialize_Deserialize_WorldState_RoundTrip()
    {
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var original = new WorldState
        {
            ServerTick = 12345,
            Players = new List<PlayerState>
            {
                new PlayerState { PlayerId = player1Id, Username = "Player1", X = 100f, Y = 200f },
                new PlayerState { PlayerId = player2Id, Username = "Player2", X = 300f, Y = 400f }
            }
        };

        var bytes = MessageSerializer.Serialize(original);
        var payload = new byte[bytes.Length - 5];
        Array.Copy(bytes, 5, payload, 0, payload.Length);

        var deserialized = MessageSerializer.Deserialize<WorldState>(payload);

        Assert.Equal(original.ServerTick, deserialized.ServerTick);
        Assert.Equal(2, deserialized.Players.Count);
        Assert.Equal(player1Id, deserialized.Players[0].PlayerId);
        Assert.Equal("Player1", deserialized.Players[0].Username);
    }

    [Fact]
    public void Serialize_Deserialize_ChatMessage_RoundTrip()
    {
        var senderId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var original = new ChatMessage
        {
            SenderId = senderId,
            SenderName = "ChatUser",
            Message = "Hello, World!",
            Timestamp = timestamp
        };

        var bytes = MessageSerializer.Serialize(original);
        var payload = new byte[bytes.Length - 5];
        Array.Copy(bytes, 5, payload, 0, payload.Length);

        var deserialized = MessageSerializer.Deserialize<ChatMessage>(payload);

        Assert.Equal(original.SenderId, deserialized.SenderId);
        Assert.Equal(original.SenderName, deserialized.SenderName);
        Assert.Equal(original.Message, deserialized.Message);
        Assert.Equal(original.Timestamp, deserialized.Timestamp);
    }
}
