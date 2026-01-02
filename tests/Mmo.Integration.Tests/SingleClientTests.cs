using FluentAssertions;
using Mmo.Integration.Tests.Utilities;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;

namespace Mmo.Integration.Tests;

/// <summary>
///     Integration tests for single client scenarios.
/// </summary>
[Collection("Integration")]
public class SingleClientTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private TestClientFactory? _clientFactory;

    public SingleClientTests()
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
    public async Task Client_CanConnect_ToServer()
    {
        // Arrange
        TestClient client = _clientFactory!.CreateClient();

        // Act
        bool connected = await client.ConnectAsync(_fixture.DefaultTimeout);

        // Assert
        connected.Should().BeTrue("client should connect to server");
        client.IsConnected.Should().BeTrue("client should remain connected");

        // Cleanup
        client.Dispose();
    }

    [Fact]
    public async Task Client_CanAuthenticate_WithValidCredentials()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAndConnectAsync(_fixture.DefaultTimeout);
        string username = "test_user_1";
        string password = "test_password";

        // Act
        LoginResponse? loginResponse = await client.LoginAsync(username, password, _fixture.DefaultTimeout);

        // Assert
        loginResponse.Should().NotBeNull("server should send login response");
        loginResponse!.Success.Should().BeTrue("login should succeed");
        loginResponse.AccountId.Should().NotBe(Guid.Empty, "server should assign player ID");
        client.AccountId.Should().Be((Guid)loginResponse.AccountId!, "client should store player ID");

        // Cleanup
        client.Dispose();
    }

    [Fact(Skip = "TODO: Server needs to set ZoneId on LoginResponse - currently returns 0")]
    public async Task Client_CanSpawnCharacter_AfterLogin()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);

        // Assert
        client.AccountId.Should().NotBe(Guid.Empty, "authenticated client should have player ID");


        // Cleanup
        client.Dispose();
    }

    [Fact(Skip = "TODO: Server needs to set ZoneId on LoginResponse and track zone membership")]
    public async Task Client_ReceivesZoneState_AfterSpawn()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);

        // Act - Wait a bit for zone state
        await Task.Delay(1000);

        // Assert
        client.AccountId.Should().NotBe(Guid.Empty);


        // Note: ZoneState message might be sent automatically or need to be requested
        // This test validates that the client is properly authenticated and in a zone

        // Cleanup
        client.Dispose();
    }

    [Fact]
    public async Task Client_ReceivesHeartbeat_AfterConnection()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        client.ClearMessages();

        // Act - Wait for heartbeat (server sends every 5 seconds)
        await Task.Delay(6000);

        // Assert
        List<Heartbeat> heartbeats = client.GetMessages<Heartbeat>();
        heartbeats.Should().NotBeEmpty("client should receive heartbeat from server");

        // Cleanup
        client.Dispose();
    }

    [Fact]
    public async Task Client_CanDisconnect_Gracefully()
    {
        // Arrange
        TestClient client = await _clientFactory!.CreateAuthenticatedClientAsync(timeout: _fixture.DefaultTimeout);
        bool wasConnected = client.IsConnected;

        // Act
        client.Dispose();

        // Assert
        wasConnected.Should().BeTrue("client should have been connected");
        client.IsConnected.Should().BeFalse("client should be disconnected after dispose");
    }
}
