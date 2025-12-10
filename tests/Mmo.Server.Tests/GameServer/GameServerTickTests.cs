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

        // Run for ~2 ticks then cancel
        cts.CancelAfter(TimeSpan.FromMilliseconds(150));

        // Act
        await gameServer.StartServerAsync(cts.Token);

        // Assert:  Should have logged a warning about tick overrun
        Assert.Contains(_mockLog.Messages, m => m.Contains("[WARN]") && m.Contains("overrun"));
    }

    [Fact]
    public async Task NormalTick_NoWarning_WhenWithinBudget()
    {
        // Arrange: Input phase is fast
        var fastDuration = TimeSpan.Zero;
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer.Object, fastDuration);
        using var cts = new CancellationTokenSource();

        // Run for ~3 ticks then cancel
        cts.CancelAfter(TimeSpan.FromMilliseconds(130));

        // Act
        await gameServer.StartServerAsync(cts.Token);

        // Assert: Should NOT have logged any warnings
        Assert.DoesNotContain(_mockLog. Messages, m => m.Contains("[WARN]") && m.Contains("overrun"));
    }

    [Fact]
    public async Task SlowTick_StillIncrementsTick_EvenWhenOverrun()
    {
        // Arrange
        var slowDuration = TimeSpan.FromMilliseconds(50);
        var gameServer = new SlowGameServer(_mockLog, _mockNetworkServer.Object, slowDuration);
        using var cts = new CancellationTokenSource();

        // Run for ~3 slow ticks
        cts.CancelAfter(TimeSpan.FromMilliseconds(180));

        // Act
        await gameServer.StartServerAsync(cts.Token);

        // Assert:  Ticks should still have been processed
        Assert.True(gameServer.CurrentTick >= 2);
    }
}
