using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Prefab;

namespace Mmo.Server.Tests.GameServer;

public class GameServerTickTests
{
    private readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer;

    public GameServerTickTests()
    {
        _mockNetworkServer = new MockNetworkServer(_mockLog, true);
    }

    [Fact]
    public void SlowTick_LogsWarning_WhenTickOverruns()
    {
        // Arrange: Tick takes longer than tick budget (50ms at 20Hz)
        var slowDuration = TimeSpan.FromMilliseconds(80);
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer, slowDuration);

        // Act: Run for enough time to get at least 2 slow ticks
        gameServer.Start();
        Thread.Sleep(250);
        gameServer.Stop();

        // Assert: Should have logged a warning about tick overrun
        Assert.True(
            _mockLog.HasMessageContaining("WARN", "took"),
            $"Expected overrun warning. Messages: {string.Join(Environment.NewLine, _mockLog.Messages)}"
        );
    }

    [Fact]
    public void NormalTick_NoWarning_WhenWithinBudget()
    {
        // Arrange: Tick is fast (well within 50ms budget at 20Hz)
        var fastDuration = TimeSpan.FromMilliseconds(5);
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer, fastDuration);

        // Act: Run for ~3-4 ticks then stop
        gameServer.Start();
        Thread.Sleep(300);
        gameServer.Stop();

        // Assert: Should NOT have logged any warnings about overrun
        Assert.False(
            _mockLog.HasMessageContaining("WARN", "took"),
            $"Unexpected overrun warning. Messages: {string.Join(Environment.NewLine, _mockLog.Messages)}"
        );
    }

    [Fact]
    public void SlowTick_StillIncrementsTick_EvenWhenOverrun()
    {
        // Arrange
        var slowDuration = TimeSpan.FromMilliseconds(70);
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer, slowDuration);

        // Act: Run for ~3 slow ticks (3 x 70ms = 210ms, plus some buffer)
        gameServer.Start();
        Thread.Sleep(250);
        gameServer.Stop();

        // Assert: Ticks should still have been processed
        Assert.True(
            gameServer.TickCount >= 2,
            $"Expected at least 2 ticks, got {gameServer.TickCount}"
        );
    }

    [Fact]
    public void GameServer_StartsAndStops_Gracefully()
    {
        // Arrange
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer, TimeSpan.FromMilliseconds(1));

        // Act: Start and stop
        gameServer.Start();
        Thread.Sleep(100);
        gameServer.Stop();

        // Assert: Should have processed some ticks
        Assert.True(gameServer.TickCount > 0);
    }

    [Fact]
    public void GameServer_StopWithoutStart_DoesNotThrow()
    {
        // Arrange
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer, TimeSpan.Zero);

        // Act & Assert: Should not throw
        Exception? exception = Record.Exception(() => gameServer.Stop());
        Assert.Null(exception);
    }
}
