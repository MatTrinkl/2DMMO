using FluentAssertions;

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
        var loginRequest = new Mmo.Shared.Messages.Connection.LoginRequest("testuser", "testpass");

        // Assert
        loginRequest.Type.Should().Be(Mmo.Shared.Enums.MessageType.LoginRequest);
    }

    [Fact]
    public void PositionUpdate_HasCorrectMessageType()
    {
        // Arrange
        var entity = new Mmo.Shared.Entities.PlayerEntity(Guid.NewGuid(), "Player", new Mmo.Shared.Records.Position(10, 20));
        var newPosition = new Mmo.Shared.Records.Position(30, 40);
        var positionUpdate = new Mmo.Shared.Messages.Movement.PositionUpdate(12345, entity, newPosition);

        // Assert
        positionUpdate.Type.Should().Be(Mmo.Shared.Enums.MessageType.PositionUpdate);
    }

    [Fact]
    public void ChatMessage_DefaultValues_AreCorrectlySet()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var chatMessage = new Mmo.Shared.Messages.Chat.ChatMessage(entityId, "Hello World");

        // Assert
        chatMessage.Type.Should().Be(Mmo.Shared.Enums.MessageType.ChatMessage);
        chatMessage.EntityId.Should().Be(entityId);
        chatMessage.Message.Should().Be("Hello World");
    }
}
