using FluentAssertions;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Records;

namespace Mmo.Shared.Tests.Messages;

/// <summary>
///     Tests for the Message DTOs and SharedConstants.
/// </summary>
public class MessageTests
{
    [Fact]
    public void SharedConstants_DefaultPort_ShouldBe7777()
    {
        // Assert
        SharedConstants.DefaultPort.Should().Be(7777);
    }

    [Fact]
    public void SharedConstants_GameName_ShouldBe2DMMO()
    {
        // Assert
        SharedConstants.GameName.Should().Be("2DMMO");
    }

    [Fact]
    public void SharedConstants_ProtocolVersion_ShouldBePositive()
    {
        // Assert
        SharedConstants.ProtocolVersion.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LoginRequest_HasCorrectMessageType()
    {
        // Arrange
        var loginRequest = new LoginRequest("testuser", "testpass");

        // Assert
        loginRequest.Type.Should().Be(MessageType.LoginRequest);
    }

    [Fact]
    public void PositionUpdate_HasCorrectMessageType()
    {
        // Arrange
        var entity = new PlayerEntity(Guid.NewGuid(), "Player", new Position(10, 20));
        var newPosition = new Position(30, 40);
        var positionUpdate = new PositionUpdate(12345, entity, newPosition);

        // Assert
        positionUpdate.Type.Should().Be(MessageType.PositionUpdate);
    }

    [Fact]
    public void ChatMessage_DefaultValues_AreCorrectlySet()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var chatMessage = new ChatMessage(entityId, "Hello World");

        // Assert
        chatMessage.Type.Should().Be(MessageType.ChatMessage);
        chatMessage.EntityId.Should().Be(entityId);
        chatMessage.Message.Should().Be("Hello World");
    }
}
