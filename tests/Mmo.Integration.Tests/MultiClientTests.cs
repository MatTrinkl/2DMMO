using FluentAssertions;
using Mmo.Integration.Tests.Utilities;
using Mmo.Shared.Zones.Messages.Server_Brodcast;

namespace Mmo.Integration.Tests;

/// <summary>
/// Integration tests for multi-client scenarios.
/// </summary>
[Collection("Integration")]
public class MultiClientTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private TestClientFactory? _clientFactory;

    public MultiClientTests()
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

    [Fact]
    public async Task TwoClients_CanConnectSimultaneously()
    {
        // Arrange & Act
        var client1 = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        var client2 = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);

        // Assert
        client1.IsConnected.Should().BeTrue("first client should be connected");
        client2.IsConnected.Should().BeTrue("second client should be connected");
        client1.AccountId.Should().NotBe(client2.AccountId, "clients should have different player IDs");

        // Cleanup
        client1.Dispose();
        client2.Dispose();
    }

    [Fact(Skip = "TODO: Server needs to broadcast PlayerJoinedZone when a player enters a zone")]
    public async Task Client_ReceivesPlayerJoined_WhenOtherConnects()
    {
        // Arrange
        var client1 = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        client1.ClearMessages();

        // Act - Second client connects to same zone
        var client2 = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);

        // Wait for broadcast
        await Task.Delay(1000);

        // Assert
        var playerJoinedMessages = client1.GetMessages<PlayerJoinedZone>();
        playerJoinedMessages.Should().NotBeEmpty("first client should receive PlayerJoined event");

        // The joined player should be client2
        var joinedEvent = playerJoinedMessages.FirstOrDefault();
        if (joinedEvent != null)
        {
            joinedEvent.Player.Should().NotBeNull("joined event should contain player data");
        }

        // Cleanup
        client1.Dispose();
        client2.Dispose();
    }

    [Fact(Skip = "TODO: Server needs to broadcast PlayerLeftZone when a player disconnects")]
    public async Task Client_ReceivesPlayerLeft_WhenOtherDisconnects()
    {
        // Arrange
        var client1 = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        var client2 = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);

        await Task.Delay(1000);
        client1.ClearMessages();

        // Act - Client 2 disconnects
        client2.Dispose();

        // Wait for broadcast
        await Task.Delay(1000);

        // Assert
        var playerLeftMessages = client1.GetMessages<PlayerLeftZone>();
        playerLeftMessages.Should().NotBeEmpty("first client should receive PlayerLeft event");

        // Cleanup
        client1.Dispose();
    }

    [Fact(Skip = "TODO: Server needs to broadcast PlayerJoinedZone for zone awareness tests")]
    public async Task MultipleClients_CanConnectSimultaneously()
    {
        // Arrange
        const int clientCount = 5;

        // Act
        var clients = await _clientFactory!.CreateMultipleClientsAsync(clientCount, _fixture.DefaultTimeout);

        // Assert
        clients.Should().HaveCount(clientCount, "all clients should connect successfully");

        foreach (var client in clients)
        {
            client.IsConnected.Should().BeTrue("each client should be connected");
            client.AccountId.Should().NotBe(Guid.Empty, "each client should have a player ID");
        }

        // All player IDs should be unique
        var playerIds = clients.Select(c => c.AccountId).ToList();
        playerIds.Should().OnlyHaveUniqueItems("each client should have a unique player ID");

        // Cleanup
        foreach (var client in clients)
        {
            client.Dispose();
        }
    }

    [Fact(Skip = "TODO: Server needs to set ZoneId in LoginResponse and broadcast PlayerJoinedZone events")]
    public async Task TwoClients_InSameZone_CanSeeEachOther()
    {
        // Arrange
        var client1 = await _clientFactory!.CreateAuthenticatedClientAsync("player1", timeout: _fixture.DefaultTimeout);
        await Task.Delay(500);

        client1.ClearMessages();

        // Act
        var client2 = await _clientFactory!.CreateAuthenticatedClientAsync("player2", timeout: _fixture.DefaultTimeout);

        // Wait for messages to propagate
        await Task.Delay(1000);


        var joinedMessages = client1.GetMessages<PlayerJoinedZone>();
        joinedMessages.Should().NotBeEmpty("client1 should see client2 joining");

        // Cleanup
        client1.Dispose();
        client2.Dispose();
    }

    [Fact]
    public async Task Clients_CanConnect_AndDisconnect_Rapidly()
    {
        // Arrange & Act
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
                await Task.Delay(100);
                client.Dispose();
            }));
        }

        // Assert - wait for all tasks to complete
        await Task.WhenAll(tasks);

        // If we get here without exceptions, the test passes
        tasks.Should().HaveCount(10, "all tasks should complete");
    }
}
