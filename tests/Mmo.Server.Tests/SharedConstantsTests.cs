using Mmo.Shared;
using Xunit;

namespace Mmo.Server.Tests;

/// <summary>
/// Tests for the SharedConstants class.
/// </summary>
public class SharedConstantsTests
{
    [Fact]
    public void DefaultPort_ShouldBe7777()
    {
        Assert.Equal(7777, SharedConstants.DefaultPort);
    }

    [Fact]
    public void GameName_ShouldBe2DMMO()
    {
        Assert.Equal("2DMMO", SharedConstants.GameName);
    }

    [Fact]
    public void ProtocolVersion_ShouldBePositive()
    {
        Assert.True(SharedConstants.ProtocolVersion > 0);
    }
}
