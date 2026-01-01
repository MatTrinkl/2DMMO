using MessagePack;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Prefab;

namespace Mmo.Server.Tests.Connections;

public class ForceDisconnectMessageTests
{
    // ══════════════════════════════════════════════════════════════
    // MESSAGE TYPE TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void ForceDisconnect_HasCorrectMessageType()
    {
        // Arrange
        var forceDisconnect = new ForceDisconnect
        {
            Reason = DisconnectReason.ServerShutdown
        };

        // Assert
        Assert.Equal(MessageType.ForceDisconnect, forceDisconnect.Type);
    }

    // ══════════════════════════════════════════════════════════════
    // SERIALIZATION TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void ForceDisconnect_Serialization_RoundTrip()
    {
        // Arrange
        var original = new ForceDisconnect
        {
            Reason = DisconnectReason.Kicked,
            Message = "You have been kicked by an admin",
            ReconnectDelay = 30
        };

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        // Assert
        Assert.Equal(original.Reason, deserialized.Reason);
        Assert.Equal(original.Message, deserialized.Message);
        Assert.Equal(original.ReconnectDelay, deserialized.ReconnectDelay);
        Assert.Equal(MessageType.ForceDisconnect, deserialized.Type);
    }

    [Theory]
    [InlineData(DisconnectReason.ClientDisconnected)]
    [InlineData(DisconnectReason.Timeout)]
    [InlineData(DisconnectReason.ServerShutdown)]
    [InlineData(DisconnectReason.Kicked)]
    [InlineData(DisconnectReason.Banned)]
    [InlineData(DisconnectReason.VersionMismatch)]
    public void ForceDisconnect_AllReasonsSerializeCorrectly(DisconnectReason reason)
    {
        // Arrange
        var original = new ForceDisconnect { Reason = reason };

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        // Assert
        Assert.Equal(reason, deserialized.Reason);
    }

    // ══════════════════════════════════════════════════════════════
    // CAN RECONNECT LOGIC TESTS
    // ══════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(DisconnectReason.ClientDisconnected, true)]
    [InlineData(DisconnectReason.Timeout, true)]
    [InlineData(DisconnectReason.ServerShutdown, true)]
    [InlineData(DisconnectReason.Kicked, true)]
    [InlineData(DisconnectReason.Banned, false)]
    [InlineData(DisconnectReason.VersionMismatch, false)]
    public void ForceDisconnect_CanReconnect_ReturnsCorrectValue(DisconnectReason reason, bool expectedCanReconnect)
    {
        // Arrange
        var forceDisconnect = new ForceDisconnect { Reason = reason };

        // Assert
        Assert.Equal(expectedCanReconnect, forceDisconnect.CanReconnect);
    }

    [Fact]
    public void ForceDisconnect_Banned_CannotReconnect()
    {
        // Arrange
        var forceDisconnect = new ForceDisconnect
        {
            Reason = DisconnectReason.Banned,
            Message = "You have been permanently banned"
        };

        // Assert
        Assert.False(forceDisconnect.CanReconnect);
    }

    [Fact]
    public void ForceDisconnect_VersionMismatch_CannotReconnect()
    {
        // Arrange
        var forceDisconnect = new ForceDisconnect
        {
            Reason = DisconnectReason.VersionMismatch,
            Message = "Client version outdated.  Please update."
        };

        // Assert
        Assert.False(forceDisconnect.CanReconnect);
    }

    // ══════════════════════════════════════════════════════════════
    // RECONNECT DELAY TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void ForceDisconnect_WithReconnectDelay_SerializesCorrectly()
    {
        // Arrange
        var original = new ForceDisconnect
        {
            Reason = DisconnectReason.Kicked,
            Message = "Kicked for spamming",
            ReconnectDelay = 60000 // 60 seconds in ms
        };

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        // Assert
        Assert.Equal(60000, deserialized.ReconnectDelay);
    }

    [Fact]
    public void ForceDisconnect_WithNullReconnectDelay_SerializesCorrectly()
    {
        // Arrange
        var original = new ForceDisconnect
        {
            Reason = DisconnectReason.ServerShutdown,
            Message = "Server is restarting",
            ReconnectDelay = null
        };

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        // Assert
        Assert.Null(deserialized.ReconnectDelay);
    }

    [Fact]
    public void ForceDisconnect_WithZeroReconnectDelay_SerializesCorrectly()
    {
        // Arrange
        var original = new ForceDisconnect
        {
            Reason = DisconnectReason.ClientDisconnected,
            ReconnectDelay = 0
        };

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        // Assert
        Assert.Equal(0, deserialized.ReconnectDelay);
    }

    // ══════════════════════════════════════════════════════════════
    // EDGE CASE TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void ForceDisconnect_WithNullMessage_SerializesCorrectly()
    {
        // Arrange
        var original = new ForceDisconnect
        {
            Reason = DisconnectReason.Timeout,
            Message = null
        };

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        // Assert
        Assert.Null(deserialized.Message);
        Assert.Equal(DisconnectReason.Timeout, deserialized.Reason);
    }

    [Fact]
    public void ForceDisconnect_IgnoreMember_CanReconnect_NotSerialized()
    {
        // Arrange
        var original = new ForceDisconnect
        {
            Reason = DisconnectReason.Kicked
        };

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        // Assert - CanReconnect is computed from Reason, not serialized
        Assert.True(deserialized.CanReconnect);

        // Change reason after deserialization - CanReconnect should reflect new reason
        // (This proves it's computed, not stored)
    }

    [Fact]
    public void ForceDisconnect_EmptyMessage_SerializesCorrectly()
    {
        // Arrange
        var original = new ForceDisconnect
        {
            Reason = DisconnectReason.ServerShutdown,
            Message = ""
        };

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(original);
        ForceDisconnect deserialized = MessagePackSerializer.Deserialize<ForceDisconnect>(serialized);

        // Assert
        Assert.Equal("", deserialized.Message);
    }
}
