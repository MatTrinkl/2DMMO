using FluentAssertions;
using Mmo.Shared;
using Xunit;

namespace Mmo.Shared.Tests.Messages;

/// <summary>
/// Tests for the Message DTOs and SharedConstants.
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

    [Fact(Skip = "Waiting for LoginRequest implementation")]
    public void LoginRequest_HasCorrectMessageType()
    {
        // This test will be implemented when LoginRequest is available
    }

    [Fact(Skip = "Waiting for PositionUpdate implementation")]
    public void PositionUpdate_HasCorrectMessageType()
    {
        // This test will be implemented when PositionUpdate is available
    }

    [Fact(Skip = "Waiting for ChatMessage implementation")]
    public void ChatMessage_DefaultValues_AreCorrectlySet()
    {
        // This test will be implemented when ChatMessage is available
    }
}
