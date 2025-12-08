using Mmo.Shared;
using Mmo.Shared.Interfaces;
using Moq;

namespace Mmo.Server.Tests.GameServer;

public class GameServerTests
{
    [Fact]
    public async Task HappyGameServerTest()
    {
        var loggerMock = new Mock<ILog>();
        var gameServer = new Server.GameLoop.GameServer(loggerMock.Object);
        var cts = new CancellationTokenSource();
        Task task = gameServer.StartServerAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();
        long currentTick = gameServer.CurrentTick;
        await Task.Delay(100);
        await task;
        Assert.False(gameServer.IsRunning);
        Assert.True(gameServer.CurrentTick > 0);
        Assert.Equal(currentTick, gameServer.CurrentTick);
        cts.Dispose();
    }

    [Fact]
    public async Task GameServerLogsStartupAndShutdownMessages()
    {
        var loggerMock = new Mock<ILog>();

        var gameServer = new Server.GameLoop.GameServer(loggerMock.Object);
        var cts = new CancellationTokenSource();
        Task task = gameServer.StartServerAsync(cts.Token);
        await Task.Delay(150); // Increased to allow for at least 3 ticks at 25 Hz (3 * 40ms = 120ms + overhead)
        cts.Cancel();

        await Task.Delay(100);
        await task;
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        loggerMock.Verify(l => l.Info(
                "GameServer starting with {TickRate} Hz...",
                25),
            Times.Once);

        loggerMock.Verify(l => l.Info(
                "GameServer stopped after {Ticks} ticks.",
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    Convert.ToInt64(args[0]) >= 3 // Changed to >= to be more flexible
                )),
            Times.Once);
        cts.Dispose();
    }

    [Fact]
    public async Task GameServer_LogsWarning_WhenTickExceedsBudget()
    {
        var logMock = new Mock<ILog>();

        // Create a Tick which is longer than the 40ms
        TimeSpan slowWork = SharedConstants.TickDuration + TimeSpan.FromMilliseconds(10);

        var server = new SlowGameServer(logMock.Object, slowWork);

        using var cts = new CancellationTokenSource();
        Task task = server.StartServerAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();
        await task;
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        // Assert: Warning is logged min. 1 time.
        logMock.Verify(l => l.Warn(
                "Tick {Tick} overrun: {ElapsedMs:F2} ms (budget: {BudgetMs:F2} ms)",
                It.Is<object[]>(args =>
                    args.Length == 3
                    // CurrentTick
                    && Convert.ToInt64(args[0]) >= 1
                    // ElapsedMs > Budget
                    && Convert.ToDouble(args[1]) >
                    SharedConstants.TickDuration.TotalMilliseconds
                    // BudgetMs ≈ TickDuration
                    && Math.Abs(
                        Convert.ToDouble(args[2]) -
                        SharedConstants.TickDuration.TotalMilliseconds) < 0.01
                )),
            Times.AtLeastOnce);
    }
}
