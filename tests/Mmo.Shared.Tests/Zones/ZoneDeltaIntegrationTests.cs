using MessagePack;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Interfaces.Dtos;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Zones.Messages.Server_Client;

namespace Mmo.Shared.Tests.Zones;

/// <summary>
///     Tests for ZoneDelta message with generated unified Delta DTOs.
/// </summary>
public class ZoneDeltaIntegrationTests
{
    [Fact]
    public void ZoneDelta_ShouldUseUnifiedEntityDeltas_PositionOnly()
    {
        // Arrange - Only position changed
        var entityId1 = Guid.NewGuid();
        var entityId2 = Guid.NewGuid();
        
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<ICombatEntityDelta>
            {
                new() { PersistentId = entityId1, Position = new Position(100, 200) },
                new() { PersistentId = entityId2, Position = new Position(300, 400) }
            }
        };

        // Assert
        Assert.Equal(MessageType.ZoneDelta, zoneDelta.Type);
        Assert.NotNull(zoneDelta.EntityUpdates);
        Assert.Equal(2, zoneDelta.EntityUpdates.Count);
        Assert.NotNull(zoneDelta.EntityUpdates[0].Position);
        Assert.Equal(100f, zoneDelta.EntityUpdates[0].Position!.X);
        Assert.NotNull(zoneDelta.EntityUpdates[1].Position);
        Assert.Equal(400f, zoneDelta.EntityUpdates[1].Position!.Y);
        // Other properties should be null (unchanged)
        Assert.Null(zoneDelta.EntityUpdates[0].CurrentHealth);
        Assert.Null(zoneDelta.EntityUpdates[0].Level);
    }

    [Fact]
    public void ZoneDelta_ShouldUseUnifiedEntityDeltas_StateOnly()
    {
        // Arrange - Only combat state changed
        var entityId = Guid.NewGuid();
        
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<ICombatEntityDelta>
            {
                new()
                {
                    PersistentId = entityId,
                    CurrentHealth = 75,
                    MaxHealth = 100,
                    IsInCombat = true
                }
            }
        };

        // Assert
        Assert.NotNull(zoneDelta.EntityUpdates);
        Assert.Single(zoneDelta.EntityUpdates);
        Assert.Equal(75, zoneDelta.EntityUpdates[0].CurrentHealth);
        Assert.Equal(100, zoneDelta.EntityUpdates[0].MaxHealth);
        Assert.True(zoneDelta.EntityUpdates[0].IsInCombat);
        // Position should be null (unchanged)
        Assert.Null(zoneDelta.EntityUpdates[0].Position);
    }

    [Fact]
    public void ZoneDelta_ShouldUseUnifiedEntityDeltas_LevelOnly()
    {
        // Arrange - Only level changed
        var entityId = Guid.NewGuid();
        
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<ICombatEntityDelta>
            {
                new() { PersistentId = entityId, Level = 42 }
            }
        };

        // Assert
        Assert.NotNull(zoneDelta.EntityUpdates);
        Assert.Single(zoneDelta.EntityUpdates);
        Assert.Equal(42, zoneDelta.EntityUpdates[0].Level);
        // Other properties should be null (unchanged)
        Assert.Null(zoneDelta.EntityUpdates[0].Position);
        Assert.Null(zoneDelta.EntityUpdates[0].CurrentHealth);
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
            EntityUpdates = null
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert - Null collections minimize bandwidth
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.SpawnedEntities);
        Assert.Null(deserialized.DespawnedEntityIds);
        Assert.Null(deserialized.EntityUpdates);
    }

    [Fact]
    public void ZoneDelta_WithMixedDeltaTypes_ShouldSerialize()
    {
        // Arrange - Different entities with different changes
        var entityId1 = Guid.NewGuid();
        var entityId2 = Guid.NewGuid();
        var entityId3 = Guid.NewGuid();

        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<ICombatEntityDelta>
            {
                // Entity 1: Only position changed
                new() { PersistentId = entityId1, Position = new Position(100, 200) },
                // Entity 2: Only state changed
                new() { PersistentId = entityId2, CurrentHealth = 80, MaxHealth = 100, IsInCombat = true },
                // Entity 3: Only level changed
                new() { PersistentId = entityId3, Level = 5 }
            },
            DespawnedEntityIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.Equal(zoneDelta.ZoneId, deserialized.ZoneId);
        Assert.Equal(zoneDelta.Timestamp, deserialized.Timestamp);
        
        Assert.NotNull(deserialized.EntityUpdates);
        Assert.Equal(3, deserialized.EntityUpdates.Count);
        
        // Verify entity 1 (position only)
        var entity1 = deserialized.EntityUpdates[0];
        Assert.Equal(entityId1, entity1.PersistentId);
        Assert.NotNull(entity1.Position);
        Assert.Equal(100f, entity1.Position!.X);
        Assert.Null(entity1.CurrentHealth);
        
        // Verify entity 2 (state only)
        var entity2 = deserialized.EntityUpdates[1];
        Assert.Equal(entityId2, entity2.PersistentId);
        Assert.Equal(80, entity2.CurrentHealth);
        Assert.True(entity2.IsInCombat);
        Assert.Null(entity2.Position);
        
        // Verify entity 3 (level only)
        var entity3 = deserialized.EntityUpdates[2];
        Assert.Equal(entityId3, entity3.PersistentId);
        Assert.Equal(5, entity3.Level);
        Assert.Null(entity3.Position);
        Assert.Null(entity3.CurrentHealth);
        
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
            EntityUpdates = new List<ICombatEntityDelta>
            {
                new() { PersistentId = Guid.NewGuid(), Position = new Position(500, 600) }
            },
            SpawnedEntities = null,
            DespawnedEntityIds = null
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);

        // Assert - Serialized size should be minimal (only position data)
        Assert.NotEmpty(bytes);
        
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);
        Assert.NotNull(deserialized.EntityUpdates);
        Assert.Single(deserialized.EntityUpdates);
        Assert.NotNull(deserialized.EntityUpdates[0].Position);
        Assert.Null(deserialized.EntityUpdates[0].CurrentHealth);
        Assert.Null(deserialized.EntityUpdates[0].Level);
    }

    [Fact]
    public void ZoneDelta_PartialStateUpdates_ShouldUseNullablePattern()
    {
        // Arrange - Only some state properties changed
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<ICombatEntityDelta>
            {
                new()
                {
                    PersistentId = Guid.NewGuid(),
                    CurrentHealth = 50,      // Changed
                    MaxHealth = null,        // Unchanged
                    CurrentResource = null,  // Unchanged
                    MaxResource = null,      // Unchanged
                    IsInCombat = true,       // Changed
                    TargetEntityId = null,   // Unchanged
                    Position = null,         // Unchanged
                    Level = null             // Unchanged
                }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.NotNull(deserialized.EntityUpdates);
        var entityUpdate = deserialized.EntityUpdates[0];
        Assert.Equal(50, entityUpdate.CurrentHealth);  // Set value
        Assert.Null(entityUpdate.MaxHealth);           // Null = unchanged
        Assert.Null(entityUpdate.CurrentResource);     // Null = unchanged
        Assert.True(entityUpdate.IsInCombat);          // Set value
        Assert.Null(entityUpdate.Position);            // Null = unchanged
    }

    [Fact]
    public void ZoneDelta_BandwidthComparison_UnifiedDeltaVsFull()
    {
        // This test demonstrates the bandwidth savings of unified delta updates

        // Arrange - Scenario: 10 entities, only 1 changed position
        var changedEntityId = Guid.NewGuid();
        
        // Delta approach: Only send the one changed entity with only position set
        var zoneDeltaSmall = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<ICombatEntityDelta>
            {
                new() { PersistentId = changedEntityId, Position = new Position(100, 200) }
                // All other properties null = unchanged
            },
            SpawnedEntities = null,
            DespawnedEntityIds = null
        };

        // Act
        byte[] deltaBytes = MessagePackSerializer.Serialize(zoneDeltaSmall);

        // Assert - Delta approach is much smaller
        // Full entity DTO would be ~200 bytes, unified delta with one property is ~50 bytes
        Assert.True(deltaBytes.Length < 100, $"Delta size {deltaBytes.Length} should be < 100 bytes");
        
        // Verify it deserializes correctly
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(deltaBytes);
        Assert.NotNull(deserialized.EntityUpdates);
        Assert.Single(deserialized.EntityUpdates);
        Assert.NotNull(deserialized.EntityUpdates[0].Position);
        // Other properties should be null
        Assert.Null(deserialized.EntityUpdates[0].CurrentHealth);
        Assert.Null(deserialized.EntityUpdates[0].Level);
    }

    [Fact]
    public void ZoneDelta_MultipleEntityUpdates_DifferentChanges()
    {
        // Arrange - Multiple entities with different types of changes
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<ICombatEntityDelta>
            {
                new() { PersistentId = Guid.NewGuid(), Position = new Position(10, 10) },
                new() { PersistentId = Guid.NewGuid(), Position = new Position(20, 20), CurrentHealth = 90 },
                new() { PersistentId = Guid.NewGuid(), Level = 5, IsInCombat = true },
                new() { PersistentId = Guid.NewGuid(), CurrentHealth = 50, MaxHealth = 100 },
                new() { PersistentId = Guid.NewGuid(), Position = new Position(50, 50), Level = 10 }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.NotNull(deserialized.EntityUpdates);
        Assert.Equal(5, deserialized.EntityUpdates.Count);
        
        // Verify entity 0 - only position
        Assert.NotNull(deserialized.EntityUpdates[0].Position);
        Assert.Null(deserialized.EntityUpdates[0].CurrentHealth);
        
        // Verify entity 1 - position and health
        Assert.NotNull(deserialized.EntityUpdates[1].Position);
        Assert.Equal(90, deserialized.EntityUpdates[1].CurrentHealth);
        
        // Verify entity 2 - level and combat state
        Assert.Equal(5, deserialized.EntityUpdates[2].Level);
        Assert.True(deserialized.EntityUpdates[2].IsInCombat);
        Assert.Null(deserialized.EntityUpdates[2].Position);
        
        // Verify entity 3 - health only
        Assert.Equal(50, deserialized.EntityUpdates[3].CurrentHealth);
        Assert.Equal(100, deserialized.EntityUpdates[3].MaxHealth);
        Assert.Null(deserialized.EntityUpdates[3].Position);
        
        // Verify entity 4 - position and level
        Assert.NotNull(deserialized.EntityUpdates[4].Position);
        Assert.Equal(10, deserialized.EntityUpdates[4].Level);
    }
}
