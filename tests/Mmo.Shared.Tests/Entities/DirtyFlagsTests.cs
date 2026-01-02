using Mmo.Shared.Entities.Enums;

namespace Mmo.Shared.Tests.Entities;

/// <summary>
///     Tests for the EntityDirtyFlags enum.
/// </summary>
public class DirtyFlagsTests
{
    [Fact]
    public void None_HasZeroValue() => Assert.Equal(0u, (uint)EntityDirtyFlags.None);

    [Fact]
    public void Position_HasCorrectBitFlag()
    {
        Assert.Equal(1u << 0, (uint)EntityDirtyFlags.Position);
        Assert.Equal(1u, (uint)EntityDirtyFlags.Position);
    }

    [Fact]
    public void Velocity_HasCorrectBitFlag()
    {
        Assert.Equal(1u << 1, (uint)EntityDirtyFlags.Velocity);
        Assert.Equal(2u, (uint)EntityDirtyFlags.Velocity);
    }

    [Fact]
    public void Health_HasCorrectBitFlag()
    {
        Assert.Equal(1u << 3, (uint)EntityDirtyFlags.Health);
        Assert.Equal(8u, (uint)EntityDirtyFlags.Health);
    }

    [Fact]
    public void Spawned_HasCorrectBitFlag() => Assert.Equal(1u << 30, (uint)EntityDirtyFlags.Spawned);

    [Fact]
    public void Despawned_HasCorrectBitFlag() => Assert.Equal(1u << 31, (uint)EntityDirtyFlags.Despawned);

    [Fact]
    public void Movement_CombinesCorrectFlags()
    {
        EntityDirtyFlags expected = EntityDirtyFlags.Position | EntityDirtyFlags.Velocity | EntityDirtyFlags.Rotation;
        Assert.Equal(EntityDirtyFlags.Movement, expected);

        // Verify individual flags are included
        Assert.True(EntityDirtyFlags.Movement.HasFlag(EntityDirtyFlags.Position));
        Assert.True(EntityDirtyFlags.Movement.HasFlag(EntityDirtyFlags.Velocity));
        Assert.True(EntityDirtyFlags.Movement.HasFlag(EntityDirtyFlags.Rotation));
    }

    [Fact]
    public void Combat_CombinesCorrectFlags()
    {
        EntityDirtyFlags expected = EntityDirtyFlags.Health | EntityDirtyFlags.MaxHealth | EntityDirtyFlags.State;
        Assert.Equal(EntityDirtyFlags.Combat, expected);

        // Verify individual flags are included
        Assert.True(EntityDirtyFlags.Combat.HasFlag(EntityDirtyFlags.Health));
        Assert.True(EntityDirtyFlags.Combat.HasFlag(EntityDirtyFlags.MaxHealth));
        Assert.True(EntityDirtyFlags.Combat.HasFlag(EntityDirtyFlags.State));
    }

    [Fact]
    public void AllStats_CombinesCorrectFlags()
    {
        EntityDirtyFlags expected = EntityDirtyFlags.Health | EntityDirtyFlags.MaxHealth | EntityDirtyFlags.Resource |
                                    EntityDirtyFlags.MaxResource | EntityDirtyFlags.Level;
        Assert.Equal(EntityDirtyFlags.AllStats, expected);

        // Verify individual flags are included
        Assert.True(EntityDirtyFlags.AllStats.HasFlag(EntityDirtyFlags.Health));
        Assert.True(EntityDirtyFlags.AllStats.HasFlag(EntityDirtyFlags.MaxHealth));
        Assert.True(EntityDirtyFlags.AllStats.HasFlag(EntityDirtyFlags.Resource));
        Assert.True(EntityDirtyFlags.AllStats.HasFlag(EntityDirtyFlags.MaxResource));
        Assert.True(EntityDirtyFlags.AllStats.HasFlag(EntityDirtyFlags.Level));
    }

    [Fact]
    public void BitwiseOr_CombinesFlags()
    {
        EntityDirtyFlags combined = EntityDirtyFlags.Position | EntityDirtyFlags.Health;

        Assert.True(combined.HasFlag(EntityDirtyFlags.Position));
        Assert.True(combined.HasFlag(EntityDirtyFlags.Health));
        Assert.False(combined.HasFlag(EntityDirtyFlags.Velocity));
    }

    [Fact]
    public void BitwiseAnd_ChecksFlags()
    {
        EntityDirtyFlags flags = EntityDirtyFlags.Position | EntityDirtyFlags.Health;

        Assert.NotEqual(EntityDirtyFlags.None, flags & EntityDirtyFlags.Position);
        Assert.NotEqual(EntityDirtyFlags.None, flags & EntityDirtyFlags.Health);
        Assert.Equal(EntityDirtyFlags.None, flags & EntityDirtyFlags.Velocity);
    }

    [Fact]
    public void HasFlag_WorksCorrectly()
    {
        EntityDirtyFlags flags = EntityDirtyFlags.Position | EntityDirtyFlags.Health | EntityDirtyFlags.Velocity;

        Assert.True(flags.HasFlag(EntityDirtyFlags.Position));
        Assert.True(flags.HasFlag(EntityDirtyFlags.Health));
        Assert.True(flags.HasFlag(EntityDirtyFlags.Velocity));
        Assert.False(flags.HasFlag(EntityDirtyFlags.Rotation));
        Assert.False(flags.HasFlag(EntityDirtyFlags.MaxHealth));
    }

    [Fact]
    public void AllFlags_AreUnique()
    {
        // Get all enum values except the combination flags
        EntityDirtyFlags[] individualFlags = new[]
        {
            EntityDirtyFlags.Position,
            EntityDirtyFlags.Velocity,
            EntityDirtyFlags.Rotation,
            EntityDirtyFlags.Health,
            EntityDirtyFlags.MaxHealth,
            EntityDirtyFlags.Resource,
            EntityDirtyFlags.MaxResource,
            EntityDirtyFlags.State,
            EntityDirtyFlags.Model,
            EntityDirtyFlags.Level,
            EntityDirtyFlags.Spawned,
            EntityDirtyFlags.Despawned
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
        EntityDirtyFlags[] individualFlags = new[]
        {
            EntityDirtyFlags.Position,
            EntityDirtyFlags.Velocity,
            EntityDirtyFlags.Rotation,
            EntityDirtyFlags.Health,
            EntityDirtyFlags.MaxHealth,
            EntityDirtyFlags.Resource,
            EntityDirtyFlags.MaxResource,
            EntityDirtyFlags.State,
            EntityDirtyFlags.Model,
            EntityDirtyFlags.Level,
            EntityDirtyFlags.Spawned,
            EntityDirtyFlags.Despawned
        };

        foreach (EntityDirtyFlags flag in individualFlags)
        {
            uint value = (uint)flag;
            // A power of 2 has only one bit set, so (value & (value - 1)) == 0
            Assert.True(value > 0 && (value & value - 1) == 0,
                $"{flag} ({value}) is not a power of 2");
        }
    }

    [Theory]
    [InlineData(EntityDirtyFlags.Position, EntityDirtyFlags.Position, EntityDirtyFlags.Position)]
    [InlineData(EntityDirtyFlags.Position, EntityDirtyFlags.Health,
        EntityDirtyFlags.Position | EntityDirtyFlags.Health)]
    [InlineData(EntityDirtyFlags.Movement, EntityDirtyFlags.Health,
        EntityDirtyFlags.Position | EntityDirtyFlags.Velocity | EntityDirtyFlags.Rotation | EntityDirtyFlags.Health)]
    public void BitwiseOr_ProducesExpectedResults(EntityDirtyFlags a, EntityDirtyFlags b, EntityDirtyFlags expected)
    {
        EntityDirtyFlags result = a | b;
        Assert.Equal(expected, result);
    }
}
