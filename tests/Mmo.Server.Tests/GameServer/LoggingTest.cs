using Microsoft.Extensions.Logging;
using Moq;
using static Moq.It;

namespace Mmo.Server.Tests.GameServer;

public class LoggingTest
{
    [Fact]
    public void Info_Forwards_To_ILogger_LogInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var adapter = new LoggerAdapter(loggerMock.Object);

        // Act
        adapter.Info("Hello World!");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                IsAny<EventId>(),
                Is<IsAnyType>((v, t) =>
                    v.ToString() == "Hello World!"),
                IsAny<Exception>(),
                IsAny<Func<IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public void Debug_Forwards_To_ILogger_LogInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var adapter = new LoggerAdapter(loggerMock.Object);

        // Act
        adapter.Debug("Hello World!");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                IsAny<EventId>(),
                Is<IsAnyType>((v, t) =>
                    v.ToString() == "Hello World!"),
                IsAny<Exception>(),
                IsAny<Func<IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public void Warn_Forwards_To_ILogger_LogInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var adapter = new LoggerAdapter(loggerMock.Object);

        // Act
        adapter.Warn("Hello World!");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                IsAny<EventId>(),
                Is<IsAnyType>((v, t) =>
                    v.ToString() == "Hello World!"),
                IsAny<Exception>(),
                IsAny<Func<IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public void Error_Forwards_To_ILogger_LogInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var adapter = new LoggerAdapter(loggerMock.Object);

        // Act
        adapter.Error("Hello World!");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                IsAny<EventId>(),
                Is<IsAnyType>((v, t) =>
                    v.ToString() == "Hello World!"),
                IsAny<Exception>(),
                IsAny<Func<IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public void Exception_Forwards_To_ILogger_LogInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var adapter = new LoggerAdapter(loggerMock.Object);

        // Act
        adapter.Error(new Exception("Hello World!"), "Hello World!");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                IsAny<EventId>(),
                Is<IsAnyType>((v, t) =>
                    v.ToString() == "Hello World!"),
                IsAny<Exception>(),
                IsAny<Func<IsAnyType, Exception, string>>()!),
            Times.Once);
    }
}
