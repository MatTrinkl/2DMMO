using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Entities.Interfaces;

namespace Mmo.Shared.Tests.Generators;

/// <summary>
///     Tests for the DirtyTrackingGenerator.
/// </summary>
public class DirtyTrackingGeneratorTests
{
    [Fact]
    public void TestEntity_ImplementsIDirtyTrackable()
    {
        var entity = new TestEntity();
        
        Assert.IsAssignableFrom<IDirtyTrackable>(entity);
    }
    
    [Fact]
    public void TestEntity_IsDirty_InitiallyFalse()
    {
        var entity = new TestEntity();
        
        Assert.False(entity.IsDirty);
        Assert.Equal(DirtyFlags.None, entity.DirtyFlags);
    }
    
    [Fact]
    public void TestEntity_MarkDirty_SetsDirtyFlag()
    {
        var entity = new TestEntity();
        
        entity.MarkDirty(DirtyFlags.Position);
        
        Assert.True(entity.IsDirty);
        Assert.True(entity.DirtyFlags.HasFlag(DirtyFlags.Position));
    }
    
    [Fact]
    public void TestEntity_ClearDirtyFlags_ClearsFlags()
    {
        var entity = new TestEntity();
        entity.MarkDirty(DirtyFlags.Position);
        
        entity.ClearDirtyFlags();
        
        Assert.False(entity.IsDirty);
        Assert.Equal(DirtyFlags.None, entity.DirtyFlags);
    }
}
