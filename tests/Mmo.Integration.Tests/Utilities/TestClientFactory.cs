namespace Mmo.Integration.Tests.Utilities;

/// <summary>
/// Factory for creating test clients with common configuration.
/// </summary>
public class TestClientFactory : IDisposable
{
    private readonly string _serverHost;
    private readonly int _serverPort;
    private readonly List<TestClient> _clients = new();
    private int _clientCounter = 0;

    public TestClientFactory(string? serverHost = null, int? serverPort = null)
    {
        _serverHost = serverHost ?? Environment.GetEnvironmentVariable("SERVER_HOST") ?? "localhost";
        _serverPort = serverPort ?? int.Parse(Environment.GetEnvironmentVariable("SERVER_PORT") ?? "7777");
    }

    /// <summary>
    /// Creates a new test client.
    /// </summary>
    public TestClient CreateClient()
    {
        var client = new TestClient(_serverHost, _serverPort);
        _clients.Add(client);
        return client;
    }

    /// <summary>
    /// Creates a client and connects it to the server.
    /// </summary>
    public async Task<TestClient> CreateAndConnectAsync(TimeSpan? timeout = null)
    {
        var client = CreateClient();
        var connected = await client.ConnectAsync(timeout ?? TimeSpan.FromSeconds(5));
        
        if (!connected)
            throw new Exception("Failed to connect to server");
        
        return client;
    }

    /// <summary>
    /// Creates a client, connects it, and logs in.
    /// </summary>
    public async Task<TestClient> CreateAuthenticatedClientAsync(
        string? username = null, 
        string? password = null,
        TimeSpan? timeout = null)
    {
        var client = await CreateAndConnectAsync(timeout);
        
        username ??= $"test_user_{Interlocked.Increment(ref _clientCounter)}";
        password ??= "test_password";
        
        var loginResponse = await client.LoginAsync(username, password, timeout ?? TimeSpan.FromSeconds(5));
        
        if (loginResponse == null || !loginResponse.Success)
            throw new Exception($"Login failed: {loginResponse?.ErrorMessage ?? "No response"}");
        
        return client;
    }

    /// <summary>
    /// Creates multiple authenticated clients.
    /// </summary>
    public async Task<List<TestClient>> CreateMultipleClientsAsync(int count, TimeSpan? timeout = null)
    {
        var clients = new List<TestClient>();
        var tasks = new List<Task<TestClient>>();
        
        for (int i = 0; i < count; i++)
        {
            tasks.Add(CreateAuthenticatedClientAsync(timeout: timeout));
        }
        
        await Task.WhenAll(tasks);
        
        return tasks.Select(t => t.Result).ToList();
    }

    /// <summary>
    /// Disposes all created clients.
    /// </summary>
    public void Dispose()
    {
        foreach (var client in _clients)
        {
            client.Dispose();
        }
        
        _clients.Clear();
    }
}
