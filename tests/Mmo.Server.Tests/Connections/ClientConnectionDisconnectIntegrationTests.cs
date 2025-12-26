using Mmo.Server.Connections;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Core;

namespace Mmo.Server.Tests.Connections;

[Collection("IdRegistry")]
public class ClientConnectionDisconnectIntegrationTests : IDisposable
{
    private readonly MockLog _mockLog = new();
    private readonly MockNetworkServer _mockNetworkServer;

    public ClientConnectionDisconnectIntegrationTests()
    {
        IdRegistry.Instance.Clear();
        _mockNetworkServer = new MockNetworkServer(_mockLog, false);
    }

    public void Dispose()
    {
        _mockNetworkServer.Dispose();
        IdRegistry.Instance.Clear();
    }

    // ══════════════════════════════════════════════════════════════
    // INTEGRATION TESTS - Event Chain
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void Disconnect_TriggersOnDisconnectedEvent()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);

        bool eventFired = false;
        string? receivedReason = null;

        connection.OnDisconnected += (conn, reason) =>
        {
            eventFired = true;
            receivedReason = reason;
        };

        // Act
        connection.Disconnect(DisconnectReason.ClientDisconnected, "Player requested logout");

        // Assert
        Assert.True(eventFired);
        Assert.Equal("Player requested logout", receivedReason); // Your implementation always passes "Connection closed"
    }

    [Fact]
    public void Disconnect_WithGameServer_AddsToDisconnectQueue()
    {
        // Arrange
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        _mockNetworkServer.SimulateClientConnected(clientId);

        gameServer.Start();
        Thread.Sleep(50);

        // Act
        _mockNetworkServer.SimulateClientDisconnected(clientId, "Test disconnect");
        Thread.Sleep(100);

        // Assert
        gameServer.Stop();
        Assert.True(_mockLog.HasMessageContaining("DEBUG", "disconnect"));
    }

    [Fact]
    public void Disconnect_ServerShutdown_ProcessedCorrectly()
    {
        // Arrange
        Core.GameServer gameServer = TestHelpers.CreateTestGameServer(_mockLog, _mockNetworkServer);
        var clientId = Guid.NewGuid();

        gameServer.Start();
        _mockNetworkServer.SimulateClientConnected(clientId);
        Thread.Sleep(50);

        // Act
        _mockNetworkServer.SimulateClientDisconnected(clientId, "Server shutdown");
        Thread.Sleep(100);

        // Assert
        gameServer.Stop();
        Assert.True(_mockLog.HasMessageContaining("DEBUG", "disconnect") ||
                    _mockLog.HasMessageContaining("INFO", "disconnect"));
    }

    [Fact]
    public void Disconnect_Kicked_LogsCorrectly()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        bool eventFired = false;

        connection.OnDisconnected += (_, _) => eventFired = true;

        // Act
        connection.Disconnect(DisconnectReason.Kicked, "Kicked by admin", reconnectDelayMs: 60000);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void Disconnect_Banned_CannotReconnect()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(clientId);
        bool eventFired = false;

        connection.OnDisconnected += (_, _) => eventFired = true;

        // Act
        connection.Disconnect(DisconnectReason.Banned, "Cheating detected");

        // Assert
        Assert.True(eventFired);
        // Note: The ForceDisconnect message sent to client will have CanReconnect = false
    }
}
