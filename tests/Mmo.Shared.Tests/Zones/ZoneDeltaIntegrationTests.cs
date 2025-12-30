using MessagePack;
using Mmo.Shared.Character.Interfaces.Dtos;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Entities.Dtos;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Npc.Interfaces.Dtos;
using Mmo.Shared.Zones.Messages.Server_Client;

namespace Mmo.Shared.Tests.Zones;

/// <summary>
///     Tests for ZoneDelta message with generated unified Delta DTOs and Union pattern.
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
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta { PersistentId = entityId1, Position = new Position(100, 200) },
                new ICharacterEntityDelta { PersistentId = entityId2, Position = new Position(300, 400) }
            }
        };

        // Assert
        Assert.Equal(MessageType.ZoneDelta, zoneDelta.Type);
        Assert.NotNull(zoneDelta.EntityUpdates);
        Assert.Equal(2, zoneDelta.EntityUpdates.Count);
        
        var delta1 = (ICharacterEntityDelta)zoneDelta.EntityUpdates[0];
        var delta2 = (ICharacterEntityDelta)zoneDelta.EntityUpdates[1];
        
        Assert.NotNull(delta1.Position);
        Assert.Equal(100f, delta1.Position!.X);
        Assert.NotNull(delta2.Position);
        Assert.Equal(400f, delta2.Position!.Y);
        // Other properties should be null (unchanged)
        Assert.Null(delta1.CurrentHealth);
        Assert.Null(delta1.Level);
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
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta
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
        
        var delta = (ICharacterEntityDelta)zoneDelta.EntityUpdates[0];
        Assert.Equal(75, delta.CurrentHealth);
        Assert.Equal(100, delta.MaxHealth);
        Assert.True(delta.IsInCombat);
        // Position should be null (unchanged)
        Assert.Null(delta.Position);
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
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta { PersistentId = entityId, Level = 42 }
            }
        };

        // Assert
        Assert.NotNull(zoneDelta.EntityUpdates);
        Assert.Single(zoneDelta.EntityUpdates);
        
        var delta = (ICharacterEntityDelta)zoneDelta.EntityUpdates[0];
        Assert.Equal(42, delta.Level);
        // Other properties should be null (unchanged)
        Assert.Null(delta.Position);
        Assert.Null(delta.CurrentHealth);
    }

    [Fact]
    public void ZoneDelta_NullEntityUpdates_Serialization()
    {
        // Arrange - No entity updates this tick
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = null
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.Null(deserialized.EntityUpdates);
    }

    [Fact]
    public void ZoneDelta_MultipleEntitiesWithDifferentChanges()
    {
        // Arrange - Different changes per entity
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var guid3 = Guid.NewGuid();
        
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta
                {
                    PersistentId = guid1,
                    Position = new Position(10, 20)  // Only position
                },
                new ICharacterEntityDelta
                {
                    PersistentId = guid2,
                    CurrentHealth = 100  // Only health
                },
                new ICharacterEntityDelta
                {
                    PersistentId = guid3,
                    Level = 10,
                    IsInCombat = true  // Level and combat state
                }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.NotNull(deserialized.EntityUpdates);
        Assert.Equal(3, deserialized.EntityUpdates.Count);
        
        var delta1 = (ICharacterEntityDelta)deserialized.EntityUpdates[0];
        var delta2 = (ICharacterEntityDelta)deserialized.EntityUpdates[1];
        var delta3 = (ICharacterEntityDelta)deserialized.EntityUpdates[2];
        
        // Entity 1: Only position changed
        Assert.NotNull(delta1.Position);
        Assert.Equal(10f, delta1.Position!.X);
        Assert.Null(delta1.CurrentHealth);
        Assert.Null(delta1.Level);
        
        // Entity 2: Only health changed
        Assert.Equal(100, delta2.CurrentHealth);
        Assert.Null(delta2.Position);
        Assert.Null(delta2.Level);
        
        // Entity 3: Level and combat state changed
        Assert.Equal(10, delta3.Level);
        Assert.True(delta3.IsInCombat);
        Assert.Null(delta3.Position);
    }

    [Fact]
    public void ZoneDelta_MixedEntityTypes_CharacterAndNpc()
    {
        // Arrange - Mix of Character and NPC deltas
        var characterId = Guid.NewGuid();
        var npcId = Guid.NewGuid();
        
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta
                {
                    PersistentId = characterId,
                    CurrentHealth = 80,
                    Position = new Position(50, 60)
                },
                new INpcEntityDelta
                {
                    PersistentId = npcId,
                    CurrentHealth = 120,
                    Level = 5
                }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.NotNull(deserialized.EntityUpdates);
        Assert.Equal(2, deserialized.EntityUpdates.Count);
        
        // Check types
        Assert.IsAssignableFrom<ICharacterEntityDelta>(deserialized.EntityUpdates[0]);
        Assert.IsAssignableFrom<INpcEntityDelta>(deserialized.EntityUpdates[1]);
        
        // Check character delta
        var characterDelta = (ICharacterEntityDelta)deserialized.EntityUpdates[0];
        Assert.Equal(80, characterDelta.CurrentHealth);
        Assert.NotNull(characterDelta.Position);
        
        // Check NPC delta
        var npcDelta = (INpcEntityDelta)deserialized.EntityUpdates[1];
        Assert.Equal(120, npcDelta.CurrentHealth);
        Assert.Equal(5, npcDelta.Level);
    }

    [Fact]
    public void ZoneDelta_UnifiedDelta_AllFields()
    {
        // Arrange - Delta with all fields set
        var entityId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta
                {
                    PersistentId = entityId,
                    CurrentHealth = 50,
                    MaxHealth = 100,
                    CurrentResource = 30,
                    MaxResource = 50,
                    IsInCombat = true,
                    TargetEntityId = targetId,
                    Position = new Position(100, 200),
                    Level = 25,
                    MovementSpeed = 1.5f
                }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.NotNull(deserialized.EntityUpdates);
        var delta = (ICharacterEntityDelta)deserialized.EntityUpdates[0];
        
        Assert.Equal(entityId, delta.PersistentId);
        Assert.Equal(50, delta.CurrentHealth);
        Assert.Equal(100, delta.MaxHealth);
        Assert.Equal(30, delta.CurrentResource);
        Assert.Equal(50, delta.MaxResource);
        Assert.True(delta.IsInCombat);
        Assert.Equal(targetId, delta.TargetEntityId);
        Assert.NotNull(delta.Position);
        Assert.Equal(100f, delta.Position!.X);
        Assert.Equal(200f, delta.Position!.Y);
        Assert.Equal(25, delta.Level);
        Assert.Equal(1.5f, delta.MovementSpeed);
    }

    [Fact]
    public void ZoneDelta_Bandwidth_NullFieldsOmitted()
    {
        // Arrange - Partial update with few fields
        var zoneDeltaPartial = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta
                {
                    PersistentId = Guid.NewGuid(),
                    CurrentHealth = 75  // Only one field changed
                }
            }
        };

        // Arrange - Full update with all fields
        var zoneDeltaFull = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta
                {
                    PersistentId = Guid.NewGuid(),
                    CurrentHealth = 75,
                    MaxHealth = 100,
                    CurrentResource = 50,
                    MaxResource = 100,
                    IsInCombat = true,
                    TargetEntityId = Guid.NewGuid(),
                    Position = new Position(100, 200),
                    Level = 10,
                    MovementSpeed = 1.0f
                }
            }
        };

        // Act
        byte[] partialBytes = MessagePackSerializer.Serialize(zoneDeltaPartial);
        byte[] fullBytes = MessagePackSerializer.Serialize(zoneDeltaFull);

        // Assert - Partial should be significantly smaller
        Assert.True(partialBytes.Length < fullBytes.Length);
        // MessagePack should omit null fields, making partial much smaller
        var savings = 1.0 - ((double)partialBytes.Length / fullBytes.Length);
        Assert.True(savings > 0.3, $"Expected >30% savings, got {savings:P}");
    }

    [Fact]
    public void ZoneDelta_Serialization_RoundTrip()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        
        var original = new ZoneDelta
        {
            Timestamp = 1234567890L,
            ZoneId = 5,
            SpawnedEntities = null,
            DespawnedEntityIds = null,
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta
                {
                    PersistentId = guid1,
                    Position = new Position(100, 200),
                    CurrentHealth = 75
                },
                new INpcEntityDelta
                {
                    PersistentId = guid2,
                    Level = 10,
                    IsInCombat = true
                }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(original);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        Assert.Equal(MessageType.ZoneDelta, deserialized.Type);
        Assert.Equal(1234567890L, deserialized.Timestamp);
        Assert.Equal((ushort)5, deserialized.ZoneId);
        Assert.Null(deserialized.SpawnedEntities);
        Assert.Null(deserialized.DespawnedEntityIds);
        Assert.NotNull(deserialized.EntityUpdates);
        Assert.Equal(2, deserialized.EntityUpdates.Count);
    }

    [Fact]
    public void ZoneDelta_PositionOnlyUpdate()
    {
        // Arrange - Typical scenario: entity moved
        var zoneDelta = new ZoneDelta
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ZoneId = 1,
            EntityUpdates = new List<EntityDeltaUnion>
            {
                new ICharacterEntityDelta
                {
                    PersistentId = Guid.NewGuid(),
                    Position = new Position(150, 250)
                    // All other fields are null = unchanged
                }
            }
        };

        // Act
        byte[] bytes = MessagePackSerializer.Serialize(zoneDelta);
        var deserialized = MessagePackSerializer.Deserialize<ZoneDelta>(bytes);

        // Assert
        var delta = (ICharacterEntityDelta)deserialized.EntityUpdates![0];
        Assert.NotNull(delta.Position);
        Assert.Equal(150f, delta.Position!.X);
        Assert.Equal(250f, delta.Position!.Y);
        
        // Verify other fields are null (not transmitted)
        Assert.Null(delta.CurrentHealth);
        Assert.Null(delta.MaxHealth);
        Assert.Null(delta.Level);
        Assert.Null(delta.IsInCombat);
    }
}
