using Mmo.Shared.Entities;

namespace Mmo.Shared.Tests;

/// <summary>
///     xUnit test collection to prevent parallel execution of tests that use the IdRegistry singleton.
///     All tests in this collection will run sequentially, avoiding race conditions on shared state.
/// </summary>
[CollectionDefinition("IdRegistry")]
public class IdRegistryTestCollection : ICollectionFixture<IdRegistryFixture>
{
}

/// <summary>
///     Fixture that ensures IdRegistry is cleared before and after the entire test collection.
/// </summary>
public class IdRegistryFixture : IDisposable
{
    public IdRegistryFixture()
    {
        IdRegistry.Instance.Clear();
    }

    public void Dispose()
    {
        IdRegistry.Instance.Clear();
    }
}
