using Mmo.Shared.DirtyTracking;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Entities.Interfaces;

namespace Mmo.Shared.Tests.Generators;

/// <summary>
///     Tests for the DirtyTrackingGenerator.
/// </summary>
public class DirtyTrackingGeneratorTests
{

    [Fact]
    public void TestEntity_IsDirty_InitiallyFalse()
    {
        var entity = new TestEntity();

        Assert.False(entity.IsDirty);
        Assert.Equal(EntityDirtyFlags.None, entity.DirtyFlags);
    }

    [Fact]
    public void TestEntity_MarkDirty_SetsDirtyFlag()
    {
        var entity = new TestEntity();

        entity.MarkDirty(EntityDirtyFlags.Position);

        Assert.True(entity.IsDirty);
        Assert.True(entity.DirtyFlags.HasFlag(EntityDirtyFlags.Position));
    }

    [Fact]
    public void TestEntity_ClearDirtyFlags_ClearsFlags()
    {
        var entity = new TestEntity();
        entity.MarkDirty(EntityDirtyFlags.Position);

        entity.ClearDirtyFlags();

        Assert.False(entity.IsDirty);
        Assert.Equal(EntityDirtyFlags.None, entity.DirtyFlags);
    }
}
