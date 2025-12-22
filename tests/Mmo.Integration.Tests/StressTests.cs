using FluentAssertions;
using Mmo.Integration.Tests.Utilities;

namespace Mmo.Integration.Tests;

/// <summary>
/// Stress tests for server load testing.
/// These tests are skipped by default and should be run manually.
/// </summary>
[Collection("Integration")]
public class StressTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private TestClientFactory? _clientFactory;

    public StressTests()
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

    [Fact(Skip = "Stress test - run manually")]
    public async Task Server_Handles_50ConcurrentClients()
    {
        // Arrange
        const int clientCount = 50;
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        var clients = await _clientFactory!.CreateMultipleClientsAsync(clientCount, timeout);

        // Assert
        clients.Should().HaveCount(clientCount, "all clients should connect");
        
        foreach (var client in clients)
        {
            client.IsConnected.Should().BeTrue("each client should remain connected");
        }

        // Keep connections alive for a bit
        await Task.Delay(5000);

        // All should still be connected
        var connectedCount = clients.Count(c => c.IsConnected);
        connectedCount.Should().Be(clientCount, "all clients should remain connected after 5 seconds");

        // Cleanup
        foreach (var client in clients)
        {
            client.Dispose();
        }
    }

    [Fact(Skip = "Stress test - run manually")]
    public async Task Server_Handles_RapidConnectDisconnect()
    {
        // Arrange
        const int iterations = 100;
        var successCount = 0;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            try
            {
                var client = await _clientFactory!.CreateAuthenticatedClientAsync(
                    timeout: TimeSpan.FromSeconds(5));
                
                successCount++;
                client.Dispose();
            }
            catch
            {
                // Log but continue
            }
        }

        // Assert
        var successRate = (double)successCount / iterations;
        successRate.Should().BeGreaterThan(0.95, "at least 95% of rapid connections should succeed");
    }

    [Fact(Skip = "Stress test - run manually")]
    public async Task Server_Handles_100ConcurrentClients()
    {
        // Arrange
        const int clientCount = 100;
        var timeout = TimeSpan.FromSeconds(60);
        var clients = new List<TestClient>();

        try
        {
            // Act - Connect in batches to avoid overwhelming the network
            const int batchSize = 10;
            for (int i = 0; i < clientCount; i += batchSize)
            {
                var batch = await _clientFactory!.CreateMultipleClientsAsync(
                    Math.Min(batchSize, clientCount - i), 
                    timeout);
                
                clients.AddRange(batch);
                await Task.Delay(500); // Small delay between batches
            }

            // Assert
            clients.Should().HaveCount(clientCount, "all clients should connect");
            
            var connectedCount = clients.Count(c => c.IsConnected);
            connectedCount.Should().BeGreaterThan((int)(clientCount * 0.95), 
                "at least 95% of clients should be connected");

            // Keep alive for a bit
            await Task.Delay(10000);

            // Check stability
            var stillConnected = clients.Count(c => c.IsConnected);
            stillConnected.Should().BeGreaterThan((int)(clientCount * 0.90), 
                "at least 90% of clients should remain connected after 10 seconds");
        }
        finally
        {
            // Cleanup
            foreach (var client in clients)
            {
                client.Dispose();
            }
        }
    }

    [Fact(Skip = "Stress test - run manually")]
    public async Task Server_MaintainsStability_UnderContinuousLoad()
    {
        // Arrange
        const int duration = 60; // seconds
        const int simultaneousClients = 20;
        var startTime = DateTime.UtcNow;
        var activeClients = new List<TestClient>();
        var totalConnections = 0;
        var failedConnections = 0;

        // Act - Maintain continuous load
        while ((DateTime.UtcNow - startTime).TotalSeconds < duration)
        {
            // Add new clients if below threshold
            while (activeClients.Count < simultaneousClients)
            {
                try
                {
                    var client = await _clientFactory!.CreateAuthenticatedClientAsync(
                        timeout: TimeSpan.FromSeconds(10));
                    activeClients.Add(client);
                    totalConnections++;
                }
                catch
                {
                    failedConnections++;
                }
            }

            // Random disconnect
            if (activeClients.Count > 0 && Random.Shared.Next(0, 5) == 0)
            {
                var clientToRemove = activeClients[Random.Shared.Next(activeClients.Count)];
                clientToRemove.Dispose();
                activeClients.Remove(clientToRemove);
            }

            await Task.Delay(1000);
        }

        // Cleanup
        foreach (var client in activeClients)
        {
            client.Dispose();
        }

        // Assert
        totalConnections.Should().BeGreaterThan(0, "should have made connections");
        var successRate = (double)(totalConnections - failedConnections) / totalConnections;
        successRate.Should().BeGreaterThan(0.90, "success rate should be above 90%");
    }
}
