using Mmo.Server.Networking;
using Mmo.Server.Tests.Helpers;
using Moq;

namespace Mmo.Server.Tests.GameServer;

public class GameServerTickTests
{
    private readonly MockLog _mockLog = new();
    private readonly Mock<INetworkServer> _mockNetworkServer;

    public GameServerTickTests()
    {
        _mockNetworkServer = new Mock<INetworkServer>();
    }

    [Fact]
    public async Task SlowTick_LogsWarning_WhenTickOverruns()
    {
        // Arrange:  Input phase takes longer than tick budget (40ms)
        var slowDuration = TimeSpan.FromMilliseconds(60);
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer.Object, slowDuration);
        using var cts = new CancellationTokenSource();

        // Run for enough time to get at least 2 slow ticks
        cts.CancelAfter(TimeSpan.FromMilliseconds(250));

        // Act
        await gameServer.StartServerAsync(cts.Token);

        // Assert:  Should have logged a warning about tick overrun
        Assert.True(
            _mockLog.HasMessageContaining("WARN", "overrun"),
            $"Expected overrun warning.  Messages: {string.Join(Environment.NewLine, _mockLog.Messages)}"
        );
    }

    [Fact]
    public async Task NormalTick_NoWarning_WhenWithinBudget()
    {
        // Arrange: Input phase is fast (well within 40ms budget)
        var fastDuration = TimeSpan.FromMilliseconds(5);
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer.Object, fastDuration);
        using var cts = new CancellationTokenSource();

        // Run for ~3-4 ticks then cancel
        cts.CancelAfter(TimeSpan.FromMilliseconds(150));

        // Act
        await gameServer.StartServerAsync(cts.Token);

        // Assert: Should NOT have logged any warnings about overrun
        Assert.False(
            _mockLog.HasMessageContaining("WARN", "overrun"),
            $"Unexpected overrun warning. Messages: {string.Join(Environment.NewLine, _mockLog.Messages)}"
        );
    }

    [Fact]
    public async Task SlowTick_StillIncrementsTick_EvenWhenOverrun()
    {
        // Arrange
        var slowDuration = TimeSpan.FromMilliseconds(50);
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer.Object, slowDuration);
        using var cts = new CancellationTokenSource();

        // Run for ~3 slow ticks (3 x 50ms = 150ms, plus some buffer)
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        // Act
        await gameServer.StartServerAsync(cts.Token);

        // Assert:  Ticks should still have been processed
        Assert.True(
            gameServer.CurrentTick >= 2,
            $"Expected at least 2 ticks, got {gameServer.CurrentTick}"
        );
    }

    [Fact]
    public async Task GameServer_StartsAndStops_Gracefully()
    {
        // Arrange
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer.Object, TimeSpan.FromMilliseconds(1));
        using var cts = new CancellationTokenSource();

        // Act
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Should not throw
        Exception? exception = await Record.ExceptionAsync(() => gameServer.StartServerAsync(cts.Token));

        // Assert
        Assert.Null(exception);
        Assert.False(gameServer.IsRunning);
    }

    [Fact]
    public async Task GameServer_CancelledImmediately_DoesNotThrow()
    {
        // Arrange
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer.Object, TimeSpan.Zero);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        Exception? exception = await Record.ExceptionAsync(() => gameServer.StartServerAsync(cts.Token));

        // Assert
        Assert.Null(exception);
        Assert.False(gameServer.IsRunning);
    }
}
