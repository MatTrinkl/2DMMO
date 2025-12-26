using FluentAssertions;
using Mmo.Integration.Tests. Utilities;

namespace Mmo. Integration.Tests;

/// <summary>
/// Stress tests for disconnect scenarios.
/// These tests are skipped by default - run manually for load testing.
/// </summary>
[Collection("Integration")]
public class DisconnectStressTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private TestClientFactory? _clientFactory;

    public DisconnectStressTests()
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
        _clientFactory?. Dispose();
        return Task.CompletedTask;
    }

    // ══════════════════════════════════════════════════════════════
    // RAPID CONNECT/DISCONNECT TESTS
    // ══════════════════════════════════════════════════════════════

    [Fact(Skip = "Stress test - run manually")]
    public async Task Server_Handles_RapidConnectDisconnect()
    {
        // Arrange & Act
        for (int i = 0; i < 50; i++)
        {
            var client = await _clientFactory!.CreateAndConnectAsync(TimeSpan.FromSeconds(5));
            client.IsConnected. Should().BeTrue();
            client. Dispose();

            // Small delay between iterations
            await Task. Delay(50);
        }

        // Assert - If we get here without exceptions, the test passed
    }

    [Fact(Skip = "Stress test - run manually")]
    public async Task Server_Handles_ManySimultaneousConnections()
    {
        // Arrange
        const int clientCount = 100;
        var connectTasks = new List<Task<TestClient>>();

        // Act - Connect many clients simultaneously
        for (int i = 0; i < clientCount; i++)
        {
            connectTasks.Add(_clientFactory!.CreateAuthenticatedClientAsync(timeout: TimeSpan.FromSeconds(30)));
        }

        var clients = await Task. WhenAll(connectTasks);

        // Assert
        clients.Should().HaveCount(clientCount);
        foreach (var client in clients)
        {
            client.IsConnected.Should().BeTrue();
        }

        // Cleanup - Disconnect all
        foreach (var client in clients)
        {
            client.Dispose();
        }
    }

    [Fact(Skip = "Stress test - run manually")]
    public async Task Server_Handles_ChaosConnectDisconnect()
    {
        // Arrange
        const int iterations = 100;
        var random = new Random();
        var activeClients = new List<TestClient>();
        var lockObj = new object();

        // Act - Random connect/disconnect chaos
        var tasks = Enumerable.Range(0, iterations).Select(async i =>
        {
            await Task.Delay(random.Next(10, 100));

            if (random.Next(2) == 0 || activeClients.Count == 0)
            {
                // Connect
                try
                {
                    var client = await _clientFactory!. CreateAuthenticatedClientAsync(
                        timeout: TimeSpan. FromSeconds(10));
                    lock (lockObj)
                    {
                        activeClients. Add(client);
                    }
                }
                catch
                {
                    // Connection failed - might happen under load
                }
            }
            else
            {
                // Disconnect random client
                TestClient?  clientToDisconnect = null;
                lock (lockObj)
                {
                    if (activeClients. Count > 0)
                    {
                        int index = random.Next(activeClients.Count);
                        clientToDisconnect = activeClients[index];
                        activeClients.RemoveAt(index);
                    }
                }
                clientToDisconnect?. Dispose();
            }
        });

        await Task.WhenAll(tasks);

        // Cleanup
        lock (lockObj)
        {
            foreach (var client in activeClients)
            {
                client. Dispose();
            }
        }
    }

    [Fact(Skip = "Stress test - run manually")]
    public async Task Server_Handles_ConnectionStorm_AfterMassDisconnect()
    {
        // Arrange - Create many clients
        const int clientCount = 50;
        var clients = await _clientFactory!. CreateMultipleClientsAsync(clientCount, TimeSpan. FromSeconds(30));

        // Act - Disconnect all at once
        Parallel.ForEach(clients, client => client.Dispose());

        // Wait for server to process
        await Task.Delay(1000);

        // Try to connect new clients immediately
        var newClients = await _clientFactory. CreateMultipleClientsAsync(clientCount, TimeSpan. FromSeconds(30));

        // Assert
        newClients. Should().HaveCount(clientCount);
        foreach (var client in newClients)
        {
            client.IsConnected.Should().BeTrue();
        }

        // Cleanup
        foreach (var client in newClients)
        {
            client.Dispose();
        }
    }
}
