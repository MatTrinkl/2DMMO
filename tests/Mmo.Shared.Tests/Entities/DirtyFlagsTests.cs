using Mmo.Shared.Entities.Enums;

namespace Mmo.Shared.Tests.Entities;

/// <summary>
///     Tests for the DirtyFlags enum.
/// </summary>
public class DirtyFlagsTests
{
    [Fact]
    public void None_HasZeroValue()
    {
        Assert.Equal(0u, (uint)DirtyFlags.None);
    }
    
    [Fact]
    public void Position_HasCorrectBitFlag()
    {
        Assert.Equal(1u << 0, (uint)DirtyFlags.Position);
        Assert.Equal(1u, (uint)DirtyFlags.Position);
    }
    
    [Fact]
    public void Velocity_HasCorrectBitFlag()
    {
        Assert.Equal(1u << 1, (uint)DirtyFlags.Velocity);
        Assert.Equal(2u, (uint)DirtyFlags.Velocity);
    }
    
    [Fact]
    public void Health_HasCorrectBitFlag()
    {
        Assert.Equal(1u << 3, (uint)DirtyFlags.Health);
        Assert.Equal(8u, (uint)DirtyFlags.Health);
    }
    
    [Fact]
    public void Spawned_HasCorrectBitFlag()
    {
        Assert.Equal(1u << 30, (uint)DirtyFlags.Spawned);
    }
    
    [Fact]
    public void Despawned_HasCorrectBitFlag()
    {
        Assert.Equal(1u << 31, (uint)DirtyFlags.Despawned);
    }
    
    [Fact]
    public void Movement_CombinesCorrectFlags()
    {
        var expected = DirtyFlags.Position | DirtyFlags.Velocity | DirtyFlags.Rotation;
        Assert.Equal(expected, DirtyFlags.Movement);
        
        // Verify individual flags are included
        Assert.True(DirtyFlags.Movement.HasFlag(DirtyFlags.Position));
        Assert.True(DirtyFlags.Movement.HasFlag(DirtyFlags.Velocity));
        Assert.True(DirtyFlags.Movement.HasFlag(DirtyFlags.Rotation));
    }
    
    [Fact]
    public void Combat_CombinesCorrectFlags()
    {
        var expected = DirtyFlags.Health | DirtyFlags.MaxHealth | DirtyFlags.State;
        Assert.Equal(expected, DirtyFlags.Combat);
        
        // Verify individual flags are included
        Assert.True(DirtyFlags.Combat.HasFlag(DirtyFlags.Health));
        Assert.True(DirtyFlags.Combat.HasFlag(DirtyFlags.MaxHealth));
        Assert.True(DirtyFlags.Combat.HasFlag(DirtyFlags.State));
    }
    
    [Fact]
    public void AllStats_CombinesCorrectFlags()
    {
        var expected = DirtyFlags.Health | DirtyFlags.MaxHealth | DirtyFlags.Resource | DirtyFlags.MaxResource | DirtyFlags.Level;
        Assert.Equal(expected, DirtyFlags.AllStats);
        
        // Verify individual flags are included
        Assert.True(DirtyFlags.AllStats.HasFlag(DirtyFlags.Health));
        Assert.True(DirtyFlags.AllStats.HasFlag(DirtyFlags.MaxHealth));
        Assert.True(DirtyFlags.AllStats.HasFlag(DirtyFlags.Resource));
        Assert.True(DirtyFlags.AllStats.HasFlag(DirtyFlags.MaxResource));
        Assert.True(DirtyFlags.AllStats.HasFlag(DirtyFlags.Level));
    }
    
    [Fact]
    public void BitwiseOr_CombinesFlags()
    {
        var combined = DirtyFlags.Position | DirtyFlags.Health;
        
        Assert.True(combined.HasFlag(DirtyFlags.Position));
        Assert.True(combined.HasFlag(DirtyFlags.Health));
        Assert.False(combined.HasFlag(DirtyFlags.Velocity));
    }
    
    [Fact]
    public void BitwiseAnd_ChecksFlags()
    {
        var flags = DirtyFlags.Position | DirtyFlags.Health;
        
        Assert.NotEqual(DirtyFlags.None, flags & DirtyFlags.Position);
        Assert.NotEqual(DirtyFlags.None, flags & DirtyFlags.Health);
        Assert.Equal(DirtyFlags.None, flags & DirtyFlags.Velocity);
    }
    
    [Fact]
    public void HasFlag_WorksCorrectly()
    {
        var flags = DirtyFlags.Position | DirtyFlags.Health | DirtyFlags.Velocity;
        
        Assert.True(flags.HasFlag(DirtyFlags.Position));
        Assert.True(flags.HasFlag(DirtyFlags.Health));
        Assert.True(flags.HasFlag(DirtyFlags.Velocity));
        Assert.False(flags.HasFlag(DirtyFlags.Rotation));
        Assert.False(flags.HasFlag(DirtyFlags.MaxHealth));
    }
    
    [Fact]
    public void AllFlags_AreUnique()
    {
        // Get all enum values except the combination flags
        var individualFlags = new[]
        {
            DirtyFlags.Position,
            DirtyFlags.Velocity,
            DirtyFlags.Rotation,
            DirtyFlags.Health,
            DirtyFlags.MaxHealth,
            DirtyFlags.Resource,
            DirtyFlags.MaxResource,
            DirtyFlags.State,
            DirtyFlags.Model,
            DirtyFlags.Level,
            DirtyFlags.Spawned,
            DirtyFlags.Despawned
        };
        
        // Convert to uint and check uniqueness
        var values = individualFlags.Select(f => (uint)f).ToList();
        var uniqueValues = values.Distinct().ToList();
        
        Assert.Equal(values.Count, uniqueValues.Count);
    }
    
    [Fact]
    public void AllFlags_ArePowersOfTwo()
    {
        // Get all enum values except None and combination flags
        var individualFlags = new[]
        {
            DirtyFlags.Position,
            DirtyFlags.Velocity,
            DirtyFlags.Rotation,
            DirtyFlags.Health,
            DirtyFlags.MaxHealth,
            DirtyFlags.Resource,
            DirtyFlags.MaxResource,
            DirtyFlags.State,
            DirtyFlags.Model,
            DirtyFlags.Level,
            DirtyFlags.Spawned,
            DirtyFlags.Despawned
        };
        
        foreach (var flag in individualFlags)
        {
            var value = (uint)flag;
            // A power of 2 has only one bit set, so (value & (value - 1)) == 0
            Assert.True(value > 0 && (value & (value - 1)) == 0,
                $"{flag} ({value}) is not a power of 2");
        }
    }
    
    [Theory]
    [InlineData(DirtyFlags.Position, DirtyFlags.Position, DirtyFlags.Position)]
    [InlineData(DirtyFlags.Position, DirtyFlags.Health, DirtyFlags.Position | DirtyFlags.Health)]
    [InlineData(DirtyFlags.Movement, DirtyFlags.Health, DirtyFlags.Position | DirtyFlags.Velocity | DirtyFlags.Rotation | DirtyFlags.Health)]
    public void BitwiseOr_ProducesExpectedResults(DirtyFlags a, DirtyFlags b, DirtyFlags expected)
    {
        var result = a | b;
        Assert.Equal(expected, result);
    }
}
