# Integration Tests

This directory contains integration tests for the MMO server using standalone C# test clients.

## Quick Start

```bash
# Run all integration tests
dotnet test

# Run specific test class
dotnet test --filter FullyQualifiedName~SingleClientTests

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Project Structure

```
Mmo.Integration.Tests/
├── SingleClientTests.cs         # Single client scenarios
├── MultiClientTests.cs          # Multi-client scenarios  
├── StressTests.cs               # Load/stress tests (skipped by default)
└── Utilities/
    ├── IntegrationTestFixture.cs   # Shared test configuration
    ├── TestClientFactory.cs        # Factory for creating test clients
    └── TestClient.cs               # Standalone C# TCP client
```

## Test Client

The `TestClient` is a standalone C# implementation that:
- Connects to the MMO server via TCP
- Uses the `Mmo.Shared` library for messages
- Serializes messages with MessagePack
- Does NOT require Godot (easier for CI/automation)

### Usage Example

```csharp
var factory = new TestClientFactory();

// Create authenticated client
var client = await factory.CreateAuthenticatedClientAsync();

// Send message
await client.SendMessageAsync(new SomeMessage());

// Wait for response
var response = await client.WaitForMessageAsync<SomeResponse>(timeout);

// Cleanup
client.Dispose();
```

## Running with Docker

See [Docker Testing Documentation](../../docs/03-testing/DOCKER_TESTING.md) for:
- Docker-based testing
- Multi-container setup
- CI/CD integration

## Environment Variables

- `SERVER_HOST` - Server hostname (default: `localhost`)
- `SERVER_PORT` - Server port (default: `7777`)

## Notes

- Tests require the MMO server to be running
- Tests are independent and can run in parallel
- Stress tests are skipped by default (run manually)
- Use `TestClientFactory` for automatic cleanup
