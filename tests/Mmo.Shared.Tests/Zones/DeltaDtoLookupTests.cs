using Mmo.Shared.Zones.Dtos;
using Mmo.Shared.Zones.Extensions;
using Mmo.Shared.Zones.Interfaces;

namespace Mmo.Shared.Tests.Zones;

/// <summary>
///     Tests for IDeltaDto interface and O(1) lookup functionality.
/// </summary>
public class DeltaDtoLookupTests
{
    [Fact]
    public void EntityPositionDelta_ImplementsIDeltaDto()
    {
        var delta = new EntityPositionDelta { EntityId = Guid.NewGuid() };
        
        Assert.IsAssignableFrom<IDeltaDto<Guid>>(delta);
    }
    
    [Fact]
    public void EntityStateDelta_ImplementsIDeltaDto()
    {
        var delta = new EntityStateDelta { EntityId = Guid.NewGuid() };
        
        Assert.IsAssignableFrom<IDeltaDto<Guid>>(delta);
    }
    
    [Fact]
    public void GetId_ReturnsCorrectEntityId()
    {
        var entityId = Guid.NewGuid();
        var delta = new EntityPositionDelta { EntityId = entityId };
        
        Assert.Equal(entityId, delta.GetId());
    }
    
    [Fact]
    public void ToDeltaDictionary_CreatesValidDictionary()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        
        var deltas = new List<EntityPositionDelta>
        {
            new() { EntityId = id1, X = 10, Y = 20 },
            new() { EntityId = id2, X = 30, Y = 40 },
            new() { EntityId = id3, X = 50, Y = 60 }
        };
        
        var dict = deltas.ToDeltaDictionary<Guid, EntityPositionDelta>();
        
        Assert.Equal(3, dict.Count);
        Assert.True(dict.ContainsKey(id1));
        Assert.True(dict.ContainsKey(id2));
        Assert.True(dict.ContainsKey(id3));
    }
    
    [Fact]
    public void ToDeltaDictionary_EnablesO1Lookup()
    {
        var targetId = Guid.NewGuid();
        
        // Create a list with 1000 deltas
        var deltas = Enumerable.Range(0, 1000)
            .Select(i => new EntityPositionDelta { EntityId = Guid.NewGuid(), X = i, Y = i })
            .ToList();
        
        // Add target delta
        deltas.Add(new EntityPositionDelta { EntityId = targetId, X = 999, Y = 888 });
        
        // Convert to dictionary - O(n) operation
        var dict = deltas.ToDeltaDictionary<Guid, EntityPositionDelta>();
        
        // Lookup is O(1)
        var found = dict.TryGetValue(targetId, out var delta);
        
        Assert.True(found);
        Assert.NotNull(delta);
        Assert.Equal(targetId, delta.EntityId);
        Assert.Equal(999, delta.X);
        Assert.Equal(888, delta.Y);
    }
    
    [Fact]
    public void ToDeltaDictionary_WorksWithEntityStateDelta()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        
        var deltas = new List<EntityStateDelta>
        {
            new() { EntityId = id1, CurrentHP = 100 },
            new() { EntityId = id2, CurrentHP = 50 }
        };
        
        var dict = deltas.ToDeltaDictionary<Guid, EntityStateDelta>();
        
        Assert.Equal(2, dict.Count);
        Assert.Equal(100, dict[id1].CurrentHP);
        Assert.Equal(50, dict[id2].CurrentHP);
    }
    
    [Fact]
    public void TryGetDelta_FindsDeltaById()
    {
        var targetId = Guid.NewGuid();
        
        var deltas = new List<EntityPositionDelta>
        {
            new() { EntityId = Guid.NewGuid(), X = 10 },
            new() { EntityId = targetId, X = 20 },
            new() { EntityId = Guid.NewGuid(), X = 30 }
        };
        
        var found = deltas.TryGetDelta(targetId, out var delta);
        
        Assert.True(found);
        Assert.NotNull(delta);
        Assert.Equal(targetId, delta!.EntityId);
        Assert.Equal(20, delta.X);
    }
    
    [Fact]
    public void TryGetDelta_ReturnsFalseWhenNotFound()
    {
        var deltas = new List<EntityPositionDelta>
        {
            new() { EntityId = Guid.NewGuid(), X = 10 },
            new() { EntityId = Guid.NewGuid(), X = 20 }
        };
        
        var found = deltas.TryGetDelta(Guid.NewGuid(), out var delta);
        
        Assert.False(found);
        Assert.Null(delta);
    }
    
    [Fact]
    public void ToDeltaDictionary_WithEmptyList_ReturnsEmptyDictionary()
    {
        var deltas = new List<EntityPositionDelta>();
        
        var dict = deltas.ToDeltaDictionary<Guid, EntityPositionDelta>();
        
        Assert.NotNull(dict);
        Assert.Empty(dict);
    }
    
    [Fact]
    public void PerformanceComparison_DictionaryVsLinearSearch()
    {
        // Create 10000 deltas
        var deltas = Enumerable.Range(0, 10000)
            .Select(i => new EntityPositionDelta { EntityId = Guid.NewGuid(), X = i })
            .ToList();
        
        var targetId = deltas[5000].EntityId; // Middle of the list
        
        // Method 1: Linear search (O(n))
        var linearResult = deltas.FirstOrDefault(d => d.EntityId == targetId);
        
        // Method 2: Dictionary lookup (O(1) after O(n) conversion)
        var dict = deltas.ToDeltaDictionary<Guid, EntityPositionDelta>();
        var dictResult = dict.TryGetValue(targetId, out var dictDelta);
        
        // Both methods should find the same delta
        Assert.NotNull(linearResult);
        Assert.True(dictResult);
        Assert.Equal(linearResult.EntityId, dictDelta.EntityId);
        Assert.Equal(linearResult.X, dictDelta.X);
    }
    
    [Fact]
    public void RealWorldScenario_ApplyingMultipleDeltas()
    {
        // Simulate receiving delta updates from server
        var entityIds = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToList();
        
        var positionDeltas = entityIds.Select(id => new EntityPositionDelta
        {
            EntityId = id,
            X = Random.Shared.Next(0, 1000),
            Y = Random.Shared.Next(0, 1000)
        }).ToList();
        
        // Convert to dictionary for fast lookup
        var deltaDict = positionDeltas.ToDeltaDictionary<Guid, EntityPositionDelta>();
        
        // Client applies deltas to local entities
        foreach (var entityId in entityIds)
        {
            if (deltaDict.TryGetValue(entityId, out var delta))
            {
                // Apply delta (simulated)
                Assert.NotNull(delta);
                Assert.Equal(entityId, delta.EntityId);
                Assert.InRange(delta.X, 0, 1000);
                Assert.InRange(delta.Y, 0, 1000);
            }
        }
        
        // All entities should have been found
        Assert.Equal(100, deltaDict.Count);
    }
}
