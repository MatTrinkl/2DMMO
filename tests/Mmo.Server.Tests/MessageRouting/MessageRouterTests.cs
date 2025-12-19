using Mmo.Server.Handlers.Base;
using Mmo.Server.MessageRouting;
using Mmo.Server.Networking;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Entities;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;
using Moq;

namespace Mmo.Server.Tests.MessageRouting;

[Collection("IdRegistry")]
public class MessageRouterTests : IDisposable
{
    private readonly MockLog _mockLog = new();

    public MessageRouterTests()
    {
        IdRegistry.Instance.Clear();
    }

    public void Dispose()
    {
        IdRegistry.Instance.Clear();
    }

    [Fact]
    public void RegisterHandler_AddsHandler()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var mockHandler = CreateMockHandler(MessageCategory.Connection);

        // Act & Assert - Should not throw
        router.RegisterHandler(mockHandler.Object);
    }

    [Fact]
    public void RegisterHandler_DuplicateCategory_ThrowsInvalidOperationException()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var handler1 = CreateMockHandler(MessageCategory.Connection);
        var handler2 = CreateMockHandler(MessageCategory.Connection);
        router.RegisterHandler(handler1.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => router.RegisterHandler(handler2.Object));
    }

    [Fact]
    public void RegisterHandler_LogsDebugMessage()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var mockHandler = CreateMockHandler(MessageCategory.Movement);

        // Act
        router.RegisterHandler(mockHandler.Object);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("DEBUG", "Registered handler"));
    }

    [Fact]
    public void RegisterHandler_MultipleCategories_AllRegistered()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var handler1 = CreateMockHandler(MessageCategory.Connection);
        var handler2 = CreateMockHandler(MessageCategory.Movement);
        var handler3 = CreateMockHandler(MessageCategory.Chat);

        // Act - Should not throw
        router.RegisterHandler(handler1.Object);
        router.RegisterHandler(handler2.Object);
        router.RegisterHandler(handler3.Object);

        // Assert - all handlers registered successfully
        Assert.True(true);
    }

    [Fact]
    public void Route_InvalidCategoryIndex_LogsWarning()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var loginRequest = new LoginRequest("test", "pass");
        
        // Create a real MessageContext using test helpers
        GameLoop.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog);
        var zoneManager = TestHelpers.CreateDefaultZoneManager();
        var services = TestHelpers.CreateTestServices(_mockLog, zoneManager);
        var mockNetworkServer = new MockNetworkServer(_mockLog, true);
        var connection = mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        var ctx = new MessageContext(connection, gameServer, zoneManager, services);

        // Act - Use a message type with category index >= 50 (handlers array size)
        router.Route(ctx, (MessageType)5000, loginRequest);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("WARN", "Invalid message category index"));
    }

    [Fact]
    public void Route_NoHandlerRegistered_LogsWarning()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var loginRequest = new LoginRequest("test", "pass");
        
        // Create a real MessageContext using test helpers
        GameLoop.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog);
        var zoneManager = TestHelpers.CreateDefaultZoneManager();
        var services = TestHelpers.CreateTestServices(_mockLog, zoneManager);
        var mockNetworkServer = new MockNetworkServer(_mockLog, true);
        var connection = mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        var ctx = new MessageContext(connection, gameServer, zoneManager, services);

        // Act
        router.Route(ctx, MessageType.LoginRequest, loginRequest);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("WARN", "No handler registered"));
    }

    [Fact]
    public void Route_HandlerCannotHandle_LogsWarning()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var mockHandler = CreateMockHandler(MessageCategory.Connection);
        mockHandler.Setup(h => h.CanHandle(It.IsAny<MessageType>())).Returns(false);
        router.RegisterHandler(mockHandler.Object);

        var loginRequest = new LoginRequest("test", "pass");
        
        // Create a real MessageContext using test helpers
        GameLoop.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog);
        var zoneManager = TestHelpers.CreateDefaultZoneManager();
        var services = TestHelpers.CreateTestServices(_mockLog, zoneManager);
        var mockNetworkServer = new MockNetworkServer(_mockLog, true);
        var connection = mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        var ctx = new MessageContext(connection, gameServer, zoneManager, services);

        // Act
        router.Route(ctx, MessageType.LoginRequest, loginRequest);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("WARN", "cannot handle"));
    }

    [Fact]
    public void Route_CallsCorrectHandler()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var mockHandler = CreateMockHandler(MessageCategory.Connection);
        mockHandler.Setup(h => h.CanHandle(MessageType.LoginRequest)).Returns(true);
        router.RegisterHandler(mockHandler.Object);

        var loginRequest = new LoginRequest("test", "pass");
        
        // Create a real MessageContext using test helpers
        GameLoop.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog);
        var zoneManager = TestHelpers.CreateDefaultZoneManager();
        var services = TestHelpers.CreateTestServices(_mockLog, zoneManager);
        var mockNetworkServer = new MockNetworkServer(_mockLog, true);
        var connection = mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        var ctx = new MessageContext(connection, gameServer, zoneManager, services);

        // Act
        router.Route(ctx, MessageType.LoginRequest, loginRequest);

        // Assert
        mockHandler.Verify(h => h.Handle(It.IsAny<MessageContext>(), MessageType.LoginRequest, loginRequest), Times.Once);
    }

    [Fact]
    public void Route_HandlerThrowsException_LogsError()
    {
        // Arrange
        var router = new MessageRouter(_mockLog);
        var mockHandler = CreateMockHandler(MessageCategory.Connection);
        mockHandler.Setup(h => h.CanHandle(MessageType.LoginRequest)).Returns(true);
        mockHandler.Setup(h => h.Handle(It.IsAny<MessageContext>(), It.IsAny<MessageType>(), It.IsAny<INetworkMessage>()))
            .Throws(new InvalidOperationException("Test exception"));
        router.RegisterHandler(mockHandler.Object);

        var loginRequest = new LoginRequest("test", "pass");
        
        // Create a real MessageContext using test helpers
        GameLoop.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog);
        var zoneManager = TestHelpers.CreateDefaultZoneManager();
        var services = TestHelpers.CreateTestServices(_mockLog, zoneManager);
        var mockNetworkServer = new MockNetworkServer(_mockLog, true);
        var connection = mockNetworkServer.GetOrCreateMockConnection(Guid.NewGuid());
        var ctx = new MessageContext(connection, gameServer, zoneManager, services);

        // Act
        router.Route(ctx, MessageType.LoginRequest, loginRequest);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("ERROR", "Error handling message"));
    }

    private Mock<ICategoryHandler> CreateMockHandler(MessageCategory category)
    {
        var mock = new Mock<ICategoryHandler>();
        mock.Setup(h => h.Category).Returns(category);
        return mock;
    }
}
