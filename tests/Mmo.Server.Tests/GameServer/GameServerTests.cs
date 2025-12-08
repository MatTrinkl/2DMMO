using Mmo.Server.GameLoop;
using Mmo.Server.Tests.GameServer;
using Mmo.Shared;
using Mmo.Shared.Interfaces;
using Moq;

namespace Mmo.Server.Tests;

public class GameServerTests
{

    [Fact]
    public void HappyGameServerTest()
    {
        var loggerMock = new Mock<ILog>();
        var gameServer = new Server.GameLoop.GameServer(loggerMock.Object);
        var clt = new CancellationTokenSource();
        gameServer.StartServerAsync(clt.Token);
        Task.Delay(100).Wait();
        clt.Cancel();
        long currenTick = gameServer.CurrentTick;
        Task.Delay(100).Wait();
        Assert.False(gameServer.IsRunning);
        Assert.True(gameServer.CurrentTick > 0);
        Assert.Equal(currenTick, gameServer.CurrentTick);
    }

    [Fact]
    public async Task HappyGameServerReadLogTest()
    {
        var loggerMock = new Mock<ILog>();

        var gameServer = new Server.GameLoop.GameServer(loggerMock.Object);
        var clt = new CancellationTokenSource();
        var task=gameServer.StartServerAsync(clt.Token);
        Task.Delay(110).Wait();
        clt.Cancel();

        Task.Delay(100).Wait();
        await task;

        loggerMock.Verify(l => l.Info(
                "GameServer starting with {TickRate} Hz...",
                30),
            Times.Once);

        loggerMock.Verify(l => l.Info(
                "GameServer stopped after {Ticks} ticks.",
                4L),
            Times.Once);
    }
    [Fact]
    public async Task GameServerWarningWhenATickIsToLong()
    {
        var logMock = new Mock<ILog>();

        // Create a Tick which is longer then the 33.3ms
        var slowWork = SharedConstants.TickDuration + TimeSpan.FromMilliseconds(10);

        var server = new SlowGameServer(logMock.Object, slowWork);

        using var cts = new CancellationTokenSource();
        var task=server.StartServerAsync(cts.Token);
        Task.Delay(100).Wait();
        cts.Cancel();
        await task;

        // Assert: Warning is logged min. 1 time.
        logMock.Verify(l => l.Warn(
                "Tick {Tick} overrun: {ElapsedMs:F2} ms (budget: {BudgetMs:F2} ms)",
                It.Is<object[]>(args =>
                    args.Length == 3
                    // Currentick
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
