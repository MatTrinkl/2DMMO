using MessagePack;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Interfaces.Dtos;
using Mmo.Shared.Zones.Extensions;

namespace Mmo.Shared.Tests.Entities;

/// <summary>
///     Tests for automatically generated Delta DTOs from IEntity and ICombatEntity interfaces.
/// </summary>
public class GeneratedDeltaDtoTests
{
    [Fact]
    public void IEntityPositionDelta_ShouldBeGenerated()
    {
        // Arrange & Act
        var delta = new IEntityPositionDelta
        {
            PersistentId = Guid.NewGuid(),
            Position = new Position(100.5f, 200.5f)
        };

        // Assert
        Assert.NotEqual(Guid.Empty, delta.PersistentId);
        Assert.NotNull(delta.Position);
        Assert.Equal(100.5f, delta.Position!.X);
        Assert.Equal(200.5f, delta.Position!.Y);
    }

    [Fact]
    public void IEntityPositionDelta_ShouldImplementIDeltaDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var delta = new IEntityPositionDelta
        {
            PersistentId = entityId,
            Position = new Position(10, 20)
        };

        // Act
        var id = delta.GetId();

        // Assert
        Assert.Equal(entityId, id);
    }

    [Fact]
    public void IEntityPositionDelta_ShouldSupportNullablePosition()
    {
        // Arrange & Act
        var delta = new IEntityPositionDelta
        {
            PersistentId = Guid.NewGuid(),
            Position = null  // Position unchanged
        };

        // Assert
        Assert.Null(delta.Position);
    }

    [Fact]
    public void IEntityPositionDelta_ShouldSerializeWithMessagePack()
    {
        // Arrange
        var original = new IEntityPositionDelta
        {
            PersistentId = Guid.NewGuid(),
            Position = new Position(150.75f, 250.25f)
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(original);
        var deserialized = MessagePackSerializer.Deserialize<IEntityPositionDelta>(bytes);

        // Assert
        Assert.Equal(original.PersistentId, deserialized.PersistentId);
        Assert.Equal(original.Position, deserialized.Position);
    }

    [Fact]
    public void IEntityPositionDelta_NullPosition_ShouldSerializeCorrectly()
    {
        // Arrange
        var original = new IEntityPositionDelta
        {
            PersistentId = Guid.NewGuid(),
            Position = null
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(original);
        var deserialized = MessagePackSerializer.Deserialize<IEntityPositionDelta>(bytes);

        // Assert
        Assert.Equal(original.PersistentId, deserialized.PersistentId);
        Assert.Null(deserialized.Position);
    }

    [Fact]
    public void ICombatEntityStateDelta_ShouldBeGenerated()
    {
        // Arrange & Act
        var delta = new ICombatEntityStateDelta
        {
            PersistentId = Guid.NewGuid(),
            CurrentHealth = 75,
            MaxHealth = 100,
            CurrentResource = 50,
            MaxResource = 100,
            IsInCombat = true,
            TargetEntityId = Guid.NewGuid()
        };

        // Assert
        Assert.NotEqual(Guid.Empty, delta.PersistentId);
        Assert.Equal(75, delta.CurrentHealth);
        Assert.Equal(100, delta.MaxHealth);
        Assert.Equal(50, delta.CurrentResource);
        Assert.Equal(100, delta.MaxResource);
        Assert.True(delta.IsInCombat);
        Assert.NotNull(delta.TargetEntityId);
    }

    [Fact]
    public void ICombatEntityStateDelta_ShouldSupportNullableProperties()
    {
        // Arrange & Act
        var delta = new ICombatEntityStateDelta
        {
            PersistentId = Guid.NewGuid(),
            CurrentHealth = 50,  // Only health changed
            MaxHealth = null,    // Unchanged
            CurrentResource = null,  // Unchanged
            MaxResource = null,  // Unchanged
            IsInCombat = null,   // Unchanged
            TargetEntityId = null  // Unchanged
        };

        // Assert
        Assert.Equal(50, delta.CurrentHealth);
        Assert.Null(delta.MaxHealth);
        Assert.Null(delta.CurrentResource);
        Assert.Null(delta.MaxResource);
        Assert.Null(delta.IsInCombat);
        Assert.Null(delta.TargetEntityId);
    }

    [Fact]
    public void ICombatEntityStateDelta_ShouldSerializeWithMessagePack()
    {
        // Arrange
        var original = new ICombatEntityStateDelta
        {
            PersistentId = Guid.NewGuid(),
            CurrentHealth = 80,
            MaxHealth = 100,
            CurrentResource = 30,
            IsInCombat = true
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(original);
        var deserialized = MessagePackSerializer.Deserialize<ICombatEntityStateDelta>(bytes);

        // Assert
        Assert.Equal(original.PersistentId, deserialized.PersistentId);
        Assert.Equal(original.CurrentHealth, deserialized.CurrentHealth);
        Assert.Equal(original.MaxHealth, deserialized.MaxHealth);
        Assert.Equal(original.CurrentResource, deserialized.CurrentResource);
        Assert.Equal(original.IsInCombat, deserialized.IsInCombat);
    }

    [Fact]
    public void ICombatEntityCustomDelta_ShouldBeGenerated()
    {
        // Arrange & Act
        var delta = new ICombatEntityCustomDelta
        {
            PersistentId = Guid.NewGuid(),
            Level = 42
        };

        // Assert
        Assert.NotEqual(Guid.Empty, delta.PersistentId);
        Assert.Equal(42, delta.Level);
    }

    [Fact]
    public void ICombatEntityCustomDelta_ShouldSupportNullableLevel()
    {
        // Arrange & Act
        var delta = new ICombatEntityCustomDelta
        {
            PersistentId = Guid.NewGuid(),
            Level = null  // Level unchanged
        };

        // Assert
        Assert.Null(delta.Level);
    }

    [Fact]
    public void ICombatEntityCustomDelta_ShouldSerializeWithMessagePack()
    {
        // Arrange
        var original = new ICombatEntityCustomDelta
        {
            PersistentId = Guid.NewGuid(),
            Level = 15
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(original);
        var deserialized = MessagePackSerializer.Deserialize<ICombatEntityCustomDelta>(bytes);

        // Assert
        Assert.Equal(original.PersistentId, deserialized.PersistentId);
        Assert.Equal(original.Level, deserialized.Level);
    }

    [Fact]
    public void ICombatEntityPositionDelta_ShouldBeGenerated()
    {
        // Arrange & Act
        var delta = new ICombatEntityPositionDelta
        {
            PersistentId = Guid.NewGuid(),
            MovementSpeed = 5.5f
        };

        // Assert
        Assert.NotEqual(Guid.Empty, delta.PersistentId);
        Assert.Equal(5.5f, delta.MovementSpeed);
    }

    [Fact]
    public void AllDeltaDtos_ShouldHaveDeltaIdAttribute()
    {
        // Arrange
        var positionDeltaType = typeof(IEntityPositionDelta);
        var stateDeltaType = typeof(ICombatEntityStateDelta);
        var customDeltaType = typeof(ICombatEntityCustomDelta);
        var combatPositionDeltaType = typeof(ICombatEntityPositionDelta);

        // Act - Check PersistentId property has [DeltaId] attribute
        var positionIdProp = positionDeltaType.GetProperty("PersistentId");
        var stateIdProp = stateDeltaType.GetProperty("PersistentId");
        var customIdProp = customDeltaType.GetProperty("PersistentId");
        var combatPositionIdProp = combatPositionDeltaType.GetProperty("PersistentId");

        // Assert
        Assert.NotNull(positionIdProp);
        Assert.NotNull(stateIdProp);
        Assert.NotNull(customIdProp);
        Assert.NotNull(combatPositionIdProp);

        // Verify [DeltaId] attribute exists on ID properties
        var deltaIdAttr = typeof(Mmo.Shared.Generators.DeltaIdAttribute);
        Assert.True(positionIdProp!.GetCustomAttributes(deltaIdAttr, false).Length > 0);
        Assert.True(stateIdProp!.GetCustomAttributes(deltaIdAttr, false).Length > 0);
        Assert.True(customIdProp!.GetCustomAttributes(deltaIdAttr, false).Length > 0);
        Assert.True(combatPositionIdProp!.GetCustomAttributes(deltaIdAttr, false).Length > 0);
    }

    [Fact]
    public void DeltaDtos_ShouldSupportO1Lookup()
    {
        // Arrange
        var entityId1 = Guid.NewGuid();
        var entityId2 = Guid.NewGuid();
        var entityId3 = Guid.NewGuid();

        var deltas = new List<IEntityPositionDelta>
        {
            new() { PersistentId = entityId1, Position = new Position(10, 10) },
            new() { PersistentId = entityId2, Position = new Position(20, 20) },
            new() { PersistentId = entityId3, Position = new Position(30, 30) }
        };

        // Act
        var deltaDict = deltas.ToDeltaDictionary<Guid, IEntityPositionDelta>();

        // Assert - O(1) lookup
        Assert.True(deltaDict.TryGetValue(entityId1, out var delta1));
        Assert.Equal(10f, delta1!.Position!.X);

        Assert.True(deltaDict.TryGetValue(entityId2, out var delta2));
        Assert.Equal(20f, delta2!.Position!.X);

        Assert.True(deltaDict.TryGetValue(entityId3, out var delta3));
        Assert.Equal(30f, delta3!.Position!.X);

        Assert.False(deltaDict.TryGetValue(Guid.NewGuid(), out _));
    }

    [Fact]
    public void MixedDeltaDtos_ShouldWorkInCollection()
    {
        // Arrange
        var positionDeltas = new List<IEntityPositionDelta>
        {
            new() { PersistentId = Guid.NewGuid(), Position = new Position(100, 200) }
        };

        var stateDeltas = new List<ICombatEntityStateDelta>
        {
            new() { PersistentId = Guid.NewGuid(), CurrentHealth = 75, MaxHealth = 100 }
        };

        var levelDeltas = new List<ICombatEntityCustomDelta>
        {
            new() { PersistentId = Guid.NewGuid(), Level = 10 }
        };

        // Act & Assert - All should serialize independently
        var posBytes = MessagePackSerializer.Serialize(positionDeltas);
        var stateBytes = MessagePackSerializer.Serialize(stateDeltas);
        var levelBytes = MessagePackSerializer.Serialize(levelDeltas);

        Assert.NotEmpty(posBytes);
        Assert.NotEmpty(stateBytes);
        Assert.NotEmpty(levelBytes);

        var deserializedPos = MessagePackSerializer.Deserialize<List<IEntityPositionDelta>>(posBytes);
        var deserializedState = MessagePackSerializer.Deserialize<List<ICombatEntityStateDelta>>(stateBytes);
        var deserializedLevel = MessagePackSerializer.Deserialize<List<ICombatEntityCustomDelta>>(levelBytes);

        Assert.Single(deserializedPos);
        Assert.Single(deserializedState);
        Assert.Single(deserializedLevel);
    }
}
