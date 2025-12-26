namespace Mmo.Integration.Tests.Utilities;

/// <summary>
///     Shared fixture for integration tests.
///     Provides common configuration and utilities.
/// </summary>
public class IntegrationTestFixture : IDisposable
{
    public IntegrationTestFixture()
    {
        ServerHost = Environment.GetEnvironmentVariable("SERVER_HOST") ?? "localhost";
        ServerPort = int.Parse(Environment.GetEnvironmentVariable("SERVER_PORT") ?? "7777");
        DefaultTimeout = TimeSpan.FromSeconds(10);
    }

    public string ServerHost { get; }
    public int ServerPort { get; }
    public TimeSpan DefaultTimeout { get; }

    public void Dispose()
    {
        // Cleanup if needed
    }

    public TestClientFactory CreateClientFactory() => new(ServerHost, ServerPort);
}
