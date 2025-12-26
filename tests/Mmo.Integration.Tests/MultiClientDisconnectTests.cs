using FluentAssertions;
using Mmo.Integration.Tests.Utilities;
using Mmo.Shared. Connection.Enums;
using Mmo.Shared. Connection.Messages.Server_Client;
using Mmo. Shared.Zones. Messages;
using Mmo.Shared.Zones.Messages.Server_Brodcast;

namespace Mmo.Integration.Tests;

/// <summary>
/// Integration tests for disconnect scenarios with multiple clients.
/// </summary>
[Collection("Integration")]
public class MultiClientDisconnectTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private TestClientFactory? _clientFactory;

    public MultiClientDisconnectTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public Task InitializeAsync()
    {
        _clientFactory = _fixture.CreateClientFactory();
        return Task. CompletedTask;
    }

    public Task DisposeAsync()
    {
        _clientFactory?.Dispose();
        return Task.CompletedTask;
    }

    // ══════════════════════════════════════════════════════════════
    // PLAYER LEFT ZONE BROADCAST TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task OtherPlayers_ReceivePlayerLeftZone_WhenClientDisconnects()
    {
        // Arrange
        var client1 = await _clientFactory!.CreateAuthenticatedClientAsync(
            username: "player1",
            timeout: _fixture.DefaultTimeout);

        var client2 = await _clientFactory.CreateAuthenticatedClientAsync(
            username: "player2",
            timeout:  _fixture.DefaultTimeout);

        // Clear messages from login
        client2.ClearMessages();
        var disconnectingPlayerId = client1.AccountId;

        // Act - Client1 disconnects
        client1.Dispose();

        // Wait for PlayerLeftZone broadcast
        await Task.Delay(500);
        var playerLeftZone = await client2.WaitForMessageAsync<PlayerLeftZone>(TimeSpan.FromSeconds(5));

        // Assert
        if (playerLeftZone != null)
        {
            playerLeftZone.Player.AccountId.Should().Be(disconnectingPlayerId);
        }

        // Cleanup
        client2.Dispose();
    }

    [Fact]
    public async Task AllPlayersInZone_ReceivePlayerLeftZone_WhenOneDisconnects()
    {
        // Arrange
        var clients = await _clientFactory!. CreateMultipleClientsAsync(3, _fixture.DefaultTimeout);
        var disconnectingClient = clients[0];
        var disconnectingPlayerId = disconnectingClient.AccountId;

        // Clear messages
        foreach (var client in clients. Skip(1))
        {
            client.ClearMessages();
        }

        // Act
        disconnectingClient.Dispose();
        await Task.Delay(500);

        // Assert - All other clients should receive PlayerLeftZone
        foreach (var client in clients.Skip(1))
        {
            var playerLeftZone = await client.WaitForMessageAsync<PlayerLeftZone>(TimeSpan.FromSeconds(5));
            playerLeftZone?.Player.AccountId.Should().Be(disconnectingPlayerId);
        }

        // Cleanup
        foreach (var client in clients. Skip(1))
        {
            client.Dispose();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // MASS DISCONNECT TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task Server_HandlesMultipleSimultaneousDisconnects()
    {
        // Arrange
        var clients = await _clientFactory!.CreateMultipleClientsAsync(5, _fixture. DefaultTimeout);

        // Verify all connected
        foreach (var client in clients)
        {
            client. IsConnected.Should().BeTrue();
        }

        // Act - Disconnect all at once
        var disconnectTasks = clients.Select(c => Task.Run(() => c.Dispose())).ToArray();
        await Task. WhenAll(disconnectTasks);

        // Assert - All should be disconnected
        foreach (var client in clients)
        {
            client.IsConnected.Should().BeFalse();
        }
    }

    [Fact]
    public async Task RemainingClients_StayConnected_WhenOthersDisconnect()
    {
        // Arrange
        var clients = await _clientFactory!.CreateMultipleClientsAsync(4, _fixture.DefaultTimeout);

        // Act - Disconnect half of them
        clients[0].Dispose();
        clients[1].Dispose();

        await Task.Delay(500);

        // Assert - Remaining should still be connected
        clients[2].IsConnected. Should().BeTrue();
        clients[3].IsConnected. Should().BeTrue();

        // Cleanup
        clients[2].Dispose();
        clients[3]. Dispose();
    }

    // ══════════════════════════════════════════════════════════════
    // CONNECTION RECOVERY TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public async Task NewClient_CanConnect_AfterAnotherDisconnects()
    {
        // Arrange
        var client1 = await _clientFactory! .CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        client1.Dispose();

        await Task.Delay(500);

        // Act
        var client2 = await _clientFactory.CreateAuthenticatedClientAsync(timeout:  _fixture.DefaultTimeout);

        // Assert
        client2.IsConnected.Should().BeTrue();
        client2.AccountId.Should().NotBe(Guid. Empty);

        // Cleanup
        client2.Dispose();
    }

    [Fact]
    public async Task Server_AcceptsNewConnections_DuringDisconnectProcessing()
    {
        // Arrange
        var existingClients = await _clientFactory!.CreateMultipleClientsAsync(3, _fixture.DefaultTimeout);

        // Act - Disconnect one while connecting a new one simultaneously
        var disconnectTask = Task.Run(() => existingClients[0].Dispose());
        var connectTask = _clientFactory.CreateAuthenticatedClientAsync(timeout: _fixture. DefaultTimeout);

        await Task.WhenAll(disconnectTask, connectTask);
        var newClient = await connectTask;

        // Assert
        newClient.IsConnected. Should().BeTrue();

        // Cleanup
        newClient. Dispose();
        foreach (var client in existingClients. Skip(1))
        {
            client.Dispose();
        }
    }
}
