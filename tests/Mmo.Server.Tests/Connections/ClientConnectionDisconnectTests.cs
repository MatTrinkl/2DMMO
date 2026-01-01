using Mmo.Server.Connections;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Entities.Structs;
using Mmo.Shared.Prefab;

namespace Mmo.Server.Tests.Connections;

public class ClientConnectionDisconnectTests
{
    private readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer;

    public ClientConnectionDisconnectTests()
    {
        _mockNetworkServer = new MockNetworkServer(_mockLog, false);
    }

    // ══════════════════════════════════════════════════════════════
    // UNIT TESTS - Disconnect Method
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void Disconnect_InvokesOnDisconnectedEvent()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        bool eventFired = false;

        connection.OnDisconnected += (conn, reason) => { eventFired = true; };

        // Act
        connection.Disconnect(DisconnectReason.ClientDisconnected, "Test disconnect");

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void Disconnect_WithNullMessage_InvokesEventSuccessfully()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        bool eventFired = false;

        connection.OnDisconnected += (_, _) => eventFired = true;

        // Act
        connection.Disconnect(DisconnectReason.ServerShutdown);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void Disconnect_CalledTwice_OnlyFiresEventOnce()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        int eventCount = 0;

        connection.OnDisconnected += (_, _) => eventCount++;

        // Act
        connection.Disconnect(DisconnectReason.Kicked, "First disconnect");
        connection.Disconnect(DisconnectReason.Kicked, "Second disconnect");

        // Assert
        Assert.Equal(1, eventCount);
    }

    [Theory]
    [InlineData(DisconnectReason.ClientDisconnected)]
    [InlineData(DisconnectReason.Timeout)]
    [InlineData(DisconnectReason.ServerShutdown)]
    [InlineData(DisconnectReason.Kicked)]
    [InlineData(DisconnectReason.Banned)]
    [InlineData(DisconnectReason.VersionMismatch)]
    public void Disconnect_AllReasons_FireEvent(DisconnectReason reason)
    {
        // Arrange
        var clientId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        bool eventFired = false;

        connection.OnDisconnected += (_, _) => eventFired = true;

        // Act
        connection.Disconnect(reason);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void Disconnect_WithReconnectDelay_DoesNotThrow()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        bool eventFired = false;

        connection.OnDisconnected += (_, _) => eventFired = true;

        // Act
        connection.Disconnect(DisconnectReason.Kicked, "Spam detected", 30000);

        // Assert
        Assert.True(eventFired);
    }
}
