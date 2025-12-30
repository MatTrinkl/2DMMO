using MessagePack;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Interfaces.Dtos;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Zones.Messages.Server_Client;

namespace Mmo.Shared.Tests.Zones;

/// <summary>
///     Tests for ZoneDelta message with generated Delta DTOs.
/// </summary>
public class ZoneDeltaIntegrationTests
{
    [Fact]
    public void ZoneDelta_ShouldUseGeneratedPositionDeltas()
    {
        // Arrange
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            PositionUpdates = new List<IEntityPositionDelta>
            {
                new() { PersistentId = Guid.NewGuid(), Position = new Position(100, 200) },
                new() { PersistentId = Guid.NewGuid(), Position = new Position(300, 400) }
            }
        };

        // Assert
        Assert.Equal(MessageType.ZoneDelta, zoneDelta.Type);
        Assert.NotNull(zoneDelta.PositionUpdates);
        Assert.Equal(2, zoneDelta.PositionUpdates.Count);
        Assert.Equal(100f, zoneDelta.PositionUpdates[0].Position!.X);
        Assert.Equal(400f, zoneDelta.PositionUpdates[1].Position!.Y);
    }

    [Fact]
    public void ZoneDelta_ShouldUseGeneratedStateDeltas()
    {
        // Arrange
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            StateUpdates = new List<ICombatEntityStateDelta>
            {
                new()
                {
                    PersistentId = Guid.NewGuid(),
                    CurrentHealth = 75,
                    MaxHealth = 100,
                    IsInCombat = true
                }
            }
        };

        // Assert
        Assert.NotNull(zoneDelta.StateUpdates);
        Assert.Single(zoneDelta.StateUpdates);
        Assert.Equal(75, zoneDelta.StateUpdates[0].CurrentHealth);
        Assert.True(zoneDelta.StateUpdates[0].IsInCombat);
    }

    [Fact]
    public void ZoneDelta_ShouldUseGeneratedLevelDeltas()
    {
        // Arrange
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            LevelUpdates = new List<ICombatEntityCustomDelta>
            {
                new() { PersistentId = Guid.NewGuid(), Level = 42 }
            }
        };

        // Assert
        Assert.NotNull(zoneDelta.LevelUpdates);
        Assert.Single(zoneDelta.LevelUpdates);
        Assert.Equal(42, zoneDelta.LevelUpdates[0].Level);
    }

    [Fact]
    public void ZoneDelta_AllNullCollections_ShouldSerialize()
    {
        // Arrange - No changes this tick
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            SpawnedEntities = null,
            DespawnedEntityIds = null,
            PositionUpdates = null,
            StateUpdates = null,
            LevelUpdates = null
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert - Null collections minimize bandwidth
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.SpawnedEntities);
        Assert.Null(deserialized.DespawnedEntityIds);
        Assert.Null(deserialized.PositionUpdates);
        Assert.Null(deserialized.StateUpdates);
        Assert.Null(deserialized.LevelUpdates);
    }

    [Fact]
    public void ZoneDelta_WithAllDeltaTypes_ShouldSerialize()
    {
        // Arrange
        var entityId1 = Guid.NewGuid();
        var entityId2 = Guid.NewGuid();

        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            PositionUpdates = new List<IEntityPositionDelta>
            {
                new() { PersistentId = entityId1, Position = new Position(100, 200) }
            },
            StateUpdates = new List<ICombatEntityStateDelta>
            {
                new()
                {
                    PersistentId = entityId1,
                    CurrentHealth = 80,
                    MaxHealth = 100
                }
            },
            LevelUpdates = new List<ICombatEntityCustomDelta>
            {
                new() { PersistentId = entityId2, Level = 5 }
            },
            DespawnedEntityIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.Equal(zoneDelta.ZoneId, deserialized.ZoneId);
        Assert.Equal(zoneDelta.Timestamp, deserialized.Timestamp);
        
        Assert.NotNull(deserialized.PositionUpdates);
        Assert.Single(deserialized.PositionUpdates);
        Assert.Equal(entityId1, deserialized.PositionUpdates[0].PersistentId);
        
        Assert.NotNull(deserialized.StateUpdates);
        Assert.Single(deserialized.StateUpdates);
        Assert.Equal(80, deserialized.StateUpdates[0].CurrentHealth);
        
        Assert.NotNull(deserialized.LevelUpdates);
        Assert.Single(deserialized.LevelUpdates);
        Assert.Equal(5, deserialized.LevelUpdates[0].Level);
        
        Assert.NotNull(deserialized.DespawnedEntityIds);
        Assert.Single(deserialized.DespawnedEntityIds);
    }

    [Fact]
    public void ZoneDelta_OnlyPositionChanges_OthersNull()
    {
        // Arrange - Only position changed this tick
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            PositionUpdates = new List<IEntityPositionDelta>
            {
                new() { PersistentId = Guid.NewGuid(), Position = new Position(500, 600) }
            },
            StateUpdates = null,  // No state changes
            LevelUpdates = null,  // No level changes
            SpawnedEntities = null,
            DespawnedEntityIds = null
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);

        // Assert - Serialized size should be minimal (only position data)
        Assert.NotEmpty(bytes);
        
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);
        Assert.NotNull(deserialized.PositionUpdates);
        Assert.Single(deserialized.PositionUpdates);
        Assert.Null(deserialized.StateUpdates);
        Assert.Null(deserialized.LevelUpdates);
    }

    [Fact]
    public void ZoneDelta_PartialStateUpdates_ShouldUseNullablePattern()
    {
        // Arrange - Only some state properties changed
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            StateUpdates = new List<ICombatEntityStateDelta>
            {
                new()
                {
                    PersistentId = Guid.NewGuid(),
                    CurrentHealth = 50,      // Changed
                    MaxHealth = null,        // Unchanged
                    CurrentResource = null,  // Unchanged
                    MaxResource = null,      // Unchanged
                    IsInCombat = true,       // Changed
                    TargetEntityId = null    // Unchanged
                }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.NotNull(deserialized.StateUpdates);
        var stateUpdate = deserialized.StateUpdates[0];
        Assert.Equal(50, stateUpdate.CurrentHealth);  // Set value
        Assert.Null(stateUpdate.MaxHealth);           // Null = unchanged
        Assert.Null(stateUpdate.CurrentResource);     // Null = unchanged
        Assert.True(stateUpdate.IsInCombat);          // Set value
    }

    [Fact]
    public void ZoneDelta_BandwidthComparison_DeltaVsFull()
    {
        // This test demonstrates the bandwidth savings of delta updates

        // Arrange - Scenario: 10 entities, only 1 changed position
        var changedEntityId = Guid.NewGuid();
        
        // Delta approach: Only send the one changed position
        var zoneDeltaSmall = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            PositionUpdates = new List<IEntityPositionDelta>
            {
                new() { PersistentId = changedEntityId, Position = new Position(100, 200) }
            },
            StateUpdates = null,
            LevelUpdates = null,
            SpawnedEntities = null,
            DespawnedEntityIds = null
        };

        // Act
        byte[] deltaBytes = MessagePackSerializer.Serialize(zoneDeltaSmall);

        // Assert - Delta approach is much smaller
        // Full entity DTO would be ~200 bytes, delta is ~50 bytes
        Assert.True(deltaBytes.Length < 100, $"Delta size {deltaBytes.Length} should be < 100 bytes");
        
        // Verify it deserializes correctly
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(deltaBytes);
        Assert.NotNull(deserialized.PositionUpdates);
        Assert.Single(deserialized.PositionUpdates);
    }

    [Fact]
    public void ZoneDelta_MultipleEntityUpdates_SameType()
    {
        // Arrange - Multiple entities with position changes
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            PositionUpdates = new List<IEntityPositionDelta>
            {
                new() { PersistentId = Guid.NewGuid(), Position = new Position(10, 10) },
                new() { PersistentId = Guid.NewGuid(), Position = new Position(20, 20) },
                new() { PersistentId = Guid.NewGuid(), Position = new Position(30, 30) },
                new() { PersistentId = Guid.NewGuid(), Position = new Position(40, 40) },
                new() { PersistentId = Guid.NewGuid(), Position = new Position(50, 50) }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.NotNull(deserialized.PositionUpdates);
        Assert.Equal(5, deserialized.PositionUpdates.Count);
        
        // Verify all positions preserved
        for (int i = 0; i < 5; i++)
        {
            float expected = (i + 1) * 10f;
            Assert.Equal(expected, deserialized.PositionUpdates[i].Position!.X);
            Assert.Equal(expected, deserialized.PositionUpdates[i].Position!.Y);
        }
    }
}
