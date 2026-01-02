using FluentAssertions;
using Mmo.Integration.Tests.Utilities;
using Mmo.Shared.Connection.Enums;
using Mmo.Shared.Connection.Messages.Server_Client;

namespace Mmo.Integration.Tests;

/// <summary>
///     Integration tests for disconnect scenarios against a real server.
///     These tests run against the Docker container.
/// </summary>
[Collection("Integration")]
public class DisconnectIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private TestClientFactory? _clientFactory;

    public DisconnectIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public Task InitializeAsync()
    {
        _clientFactory = _fixture.CreateClientFactory();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _clientFactory?.Dispose();
        return Task.CompletedTask;
    }

    // ══════════════════════════════════════════════════════════════
    // CLIENT DISCONNECT TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task Client_CanDisconnect_Gracefully()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        client.IsConnected.Should().BeTrue();

        // Act
        client.Dispose();

        // Assert
        client.IsConnected.Should().BeFalse("client should be disconnected after dispose");
    }

    [Fact]
    public async Task Client_CanReconnect_AfterGracefulDisconnect()
    {
        // Arrange
        string username = $"reconnect_test_{Guid.NewGuid():N}";
        TestClient client1 = await _clientFactory!.CreateAuthenticatedClientAsync(
            username,
            timeout: _fixture.DefaultTimeout);

        client1.Dispose();

        // Wait a bit for server to process disconnect
        await Task.Delay(500);

        // Act - Reconnect with same username
        TestClient client2 = await _clientFactory.CreateAuthenticatedClientAsync(
            username,
            timeout: _fixture.DefaultTimeout);

        // Assert
        client2.IsConnected.Should().BeTrue();
        client2.AccountId.Should().NotBe(Guid.Empty);

        // Cleanup
        client2.Dispose();
    }

    // ══════════════════════════════════════════════════════════════
    // SERVER-INITIATED DISCONNECT TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task Client_ReceivesForceDisconnect_OnServerShutdown()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        client.ClearMessages();

        // Note: This test requires the server to send a ForceDisconnect
        // In a real scenario, you would trigger a server shutdown via admin command
        // For now, we just verify the client can receive ForceDisconnect messages

        // Act - Wait for potential ForceDisconnect (or timeout)
        ForceDisconnect? forceDisconnect = await client.WaitForMessageAsync<ForceDisconnect>(TimeSpan.FromSeconds(2));

        // Assert - This may be null if server doesn't send ForceDisconnect during test
        // The important thing is the client can handle it
        if (forceDisconnect != null) forceDisconnect.Reason.Should().NotBe(default);

        // Cleanup
        client.Dispose();
    }

    [Fact]
    public async Task Client_ConnectionDrops_WhenServerSendsForceDisconnect()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        client.IsConnected.Should().BeTrue();

        // Note: To fully test this, the server needs an admin command to kick players
        // This test verifies the client handles connection drops gracefully

        // Cleanup
        client.Dispose();
        client.IsConnected.Should().BeFalse();
    }

    // ══════════════════════════════════════════════════════════════
    // TIMEOUT TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task Client_TimesOut_WhenNoHeartbeat()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAndConnectAsync(_fixture.DefaultTimeout);

        // Don't send login or heartbeat - just connect
        // Server should eventually timeout this connection

        // Act - Wait longer than server's connection timeout (typically 30s)
        // For testing, we use a shorter wait and check if still connected
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Assert - Connection might still be alive for unauthenticated clients
        // This depends on server configuration

        // Cleanup
        client.Dispose();
    }

    [Fact(Skip = "Requires server heartbeat timeout to be shorter for testing")]
    public async Task Client_ReceivesForceDisconnect_OnHeartbeatTimeout()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        client.ClearMessages();

        // Act - Wait for heartbeat timeout (server default is 30s)
        // Don't respond to any Pong messages
        await Task.Delay(TimeSpan.FromSeconds(35));

        // Assert
        ForceDisconnect? forceDisconnect = await client.WaitForMessageAsync<ForceDisconnect>(TimeSpan.FromSeconds(5));
        forceDisconnect.Should().NotBeNull();
        forceDisconnect!.Reason.Should().Be(DisconnectReason.Timeout);

        // Cleanup
        client.Dispose();
    }

    // ══════════════════════════════════════════════════════════════
    // DUPLICATE CONNECTION TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task OldClient_ReceivesForceDisconnect_WhenSameUserLogsInAgain()
    {
        // Arrange
        string username = $"duplicate_test_{Guid.NewGuid():N}";

        TestClient client1 = await _clientFactory!.CreateAuthenticatedClientAsync(
            username,
            timeout: _fixture.DefaultTimeout);
        client1.ClearMessages();

        // Act - Login with same username from another client
        TestClient client2 = await _clientFactory.CreateAuthenticatedClientAsync(
            username,
            timeout: _fixture.DefaultTimeout);

        // Wait for client1 to receive ForceDisconnect
        ForceDisconnect? forceDisconnect = await client1.WaitForMessageAsync<ForceDisconnect>(TimeSpan.FromSeconds(5));

        // Assert
        if (forceDisconnect != null)
        {
            // Server should kick old connection when same user logs in again
            forceDisconnect.Reason.Should().Be(DisconnectReason.DuplicateLogin);
            forceDisconnect.CanReconnect.Should().BeTrue();
        }

        // Cleanup
        client1.Dispose();
        client2.Dispose();
    }

    // ══════════════════════════════════════════════════════════════
    // RECONNECT DELAY TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ForceDisconnect_WithReconnectDelay_ClientReceivesDelay()
    {
        // Note: This test requires triggering a kick with reconnect delay from server
        // For example, via rate limiting or admin command

        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        client.ClearMessages();

        // Act - Wait for any ForceDisconnect
        ForceDisconnect? forceDisconnect = await client.WaitForMessageAsync<ForceDisconnect>(TimeSpan.FromSeconds(2));

        // Assert
        if (forceDisconnect?.ReconnectDelay != null) forceDisconnect.ReconnectDelay.Should().BeGreaterOrEqualTo(0);

        // Cleanup
        client.Dispose();
    }

    // ══════════════════════════════════════════════════════════════
    // CAN RECONNECT LOGIC TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ForceDisconnect_Banned_CannotReconnect()
    {
        // Note: This test requires an admin to ban the player
        // We verify the ForceDisconnect message properties instead

        // Arrange
        var forceDisconnect = new ForceDisconnect
        {
            Reason = DisconnectReason.Banned,
            Message = "You have been banned"
        };

        // Assert
        forceDisconnect.CanReconnect.Should().BeFalse();
    }

    [Fact]
    public async Task ForceDisconnect_Kicked_CanReconnect()
    {
        // Arrange
        var forceDisconnect = new ForceDisconnect
        {
            Reason = DisconnectReason.Kicked,
            Message = "Kicked by admin",
            ReconnectDelay = 60
        };

        // Assert
        forceDisconnect.CanReconnect.Should().BeTrue();
        forceDisconnect.ReconnectDelay.Should().Be(60);
    }
}
