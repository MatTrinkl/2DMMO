using MessagePack;
using Mmo.Shared.Zones.Dtos;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Tests.Zones;

/// <summary>
///     Tests for Delta DTOs (EntityPositionDelta, EntityStateDelta, ZoneContextDelta).
/// </summary>
public class DeltaDtoTests
{
    private readonly MessagePackSerializerOptions _options = MessagePackSerializerOptions.Standard;
    
    // ═══════════════════════════════════════════════════════════════
    // EntityPositionDelta Tests
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void EntityPositionDelta_Serialization_RoundTrip()
    {
        var originalId = Guid.NewGuid();
        var original = new EntityPositionDelta
        {
            EntityId = originalId,
            X = 100.5f,
            Y = 200.3f,
            VelocityX = 5.0f,
            VelocityY = -3.2f,
            Rotation = 45.0f
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<EntityPositionDelta>(bytes, _options);
        
        Assert.Equal(original.EntityId, deserialized.EntityId);
        Assert.Equal(original.X, deserialized.X);
        Assert.Equal(original.Y, deserialized.Y);
        Assert.Equal(original.VelocityX, deserialized.VelocityX);
        Assert.Equal(original.VelocityY, deserialized.VelocityY);
        Assert.Equal(original.Rotation, deserialized.Rotation);
    }
    
    [Fact]
    public void EntityPositionDelta_WithNullRotation_SerializesCorrectly()
    {
        var original = new EntityPositionDelta
        {
            EntityId = Guid.NewGuid(),
            X = 50.0f,
            Y = 75.0f,
            VelocityX = 0f,
            VelocityY = 0f,
            Rotation = null // Not changed
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<EntityPositionDelta>(bytes, _options);
        
        Assert.Null(deserialized.Rotation);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EntityStateDelta Tests
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void EntityStateDelta_Serialization_RoundTrip()
    {
        var originalId = Guid.NewGuid();
        var original = new EntityStateDelta
        {
            EntityId = originalId,
            CurrentHP = 450,
            MaxHP = 600,
            State = 2,
            ModelId = 1234,
            Level = 42
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<EntityStateDelta>(bytes, _options);
        
        Assert.Equal(original.EntityId, deserialized.EntityId);
        Assert.Equal(original.CurrentHP, deserialized.CurrentHP);
        Assert.Equal(original.MaxHP, deserialized.MaxHP);
        Assert.Equal(original.State, deserialized.State);
        Assert.Equal(original.ModelId, deserialized.ModelId);
        Assert.Equal(original.Level, deserialized.Level);
    }
    
    [Fact]
    public void EntityStateDelta_WithOnlyHPChange_SerializesCorrectly()
    {
        var original = new EntityStateDelta
        {
            EntityId = Guid.NewGuid(),
            CurrentHP = 250, // Changed
            MaxHP = null,    // Not changed
            State = null,    // Not changed
            ModelId = null,  // Not changed
            Level = null     // Not changed
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<EntityStateDelta>(bytes, _options);
        
        Assert.Equal(250, deserialized.CurrentHP);
        Assert.Null(deserialized.MaxHP);
        Assert.Null(deserialized.State);
        Assert.Null(deserialized.ModelId);
        Assert.Null(deserialized.Level);
    }
    
    [Fact]
    public void EntityStateDelta_AllFieldsNull_SerializesCorrectly()
    {
        var original = new EntityStateDelta
        {
            EntityId = Guid.NewGuid(),
            CurrentHP = null,
            MaxHP = null,
            State = null,
            ModelId = null,
            Level = null
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<EntityStateDelta>(bytes, _options);
        
        Assert.Null(deserialized.CurrentHP);
        Assert.Null(deserialized.MaxHP);
        Assert.Null(deserialized.State);
        Assert.Null(deserialized.ModelId);
        Assert.Null(deserialized.Level);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // ZoneContextDelta Tests
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void ZoneContextDelta_Serialization_RoundTrip()
    {
        var original = new ZoneContextDelta
        {
            CurrentWeather = WeatherType.Rain,
            TimeOfDay = 18.5f
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<ZoneContextDelta>(bytes, _options);
        
        Assert.Equal(WeatherType.Rain, deserialized.CurrentWeather);
        Assert.Equal(18.5f, deserialized.TimeOfDay);
    }
    
    [Fact]
    public void ZoneContextDelta_WithOnlyWeatherChange_SerializesCorrectly()
    {
        var original = new ZoneContextDelta
        {
            CurrentWeather = WeatherType.Storm,
            TimeOfDay = null // Not changed
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<ZoneContextDelta>(bytes, _options);
        
        Assert.Equal(WeatherType.Storm, deserialized.CurrentWeather);
        Assert.Null(deserialized.TimeOfDay);
    }
    
    [Fact]
    public void ZoneContextDelta_WithOnlyTimeChange_SerializesCorrectly()
    {
        var original = new ZoneContextDelta
        {
            CurrentWeather = null, // Not changed
            TimeOfDay = 6.0f // 6 AM
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<ZoneContextDelta>(bytes, _options);
        
        Assert.Null(deserialized.CurrentWeather);
        Assert.Equal(6.0f, deserialized.TimeOfDay);
    }
    
    [Fact]
    public void ZoneContextDelta_AllFieldsNull_SerializesCorrectly()
    {
        var original = new ZoneContextDelta
        {
            CurrentWeather = null,
            TimeOfDay = null
        };
        
        var bytes = MessagePackSerializer.Serialize(original, _options);
        var deserialized = MessagePackSerializer.Deserialize<ZoneContextDelta>(bytes, _options);
        
        Assert.Null(deserialized.CurrentWeather);
        Assert.Null(deserialized.TimeOfDay);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // Size Optimization Tests
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void EntityStateDelta_WithNullFields_IsSmallerThanFull()
    {
        var partial = new EntityStateDelta
        {
            EntityId = Guid.NewGuid(),
            CurrentHP = 500,
            MaxHP = null,
            State = null,
            ModelId = null,
            Level = null
        };
        
        var full = new EntityStateDelta
        {
            EntityId = partial.EntityId,
            CurrentHP = 500,
            MaxHP = 600,
            State = 1,
            ModelId = 1234,
            Level = 50
        };
        
        var partialBytes = MessagePackSerializer.Serialize(partial, _options);
        var fullBytes = MessagePackSerializer.Serialize(full, _options);
        
        // Partial delta should be smaller
        Assert.True(partialBytes.Length < fullBytes.Length,
            $"Partial size ({partialBytes.Length}) should be smaller than full size ({fullBytes.Length})");
    }
    
    [Fact]
    public void ZoneContextDelta_NullablePattern_MinimizesBandwidth()
    {
        // No changes
        var noChanges = new ZoneContextDelta
        {
            CurrentWeather = null,
            TimeOfDay = null
        };
        
        // One change
        var oneChange = new ZoneContextDelta
        {
            CurrentWeather = WeatherType.Clear,
            TimeOfDay = null
        };
        
        // Both changes
        var bothChanges = new ZoneContextDelta
        {
            CurrentWeather = WeatherType.Clear,
            TimeOfDay = 12.0f
        };
        
        var noChangesBytes = MessagePackSerializer.Serialize(noChanges, _options);
        var oneChangeBytes = MessagePackSerializer.Serialize(oneChange, _options);
        var bothChangesBytes = MessagePackSerializer.Serialize(bothChanges, _options);
        
        // Verify size increases with more data (or stays same due to MessagePack optimizations)
        Assert.True(noChangesBytes.Length <= oneChangeBytes.Length);
        Assert.True(oneChangeBytes.Length <= bothChangesBytes.Length);
        
        // At minimum, both changes should have data
        Assert.True(bothChangesBytes.Length > noChangesBytes.Length);
    }
}
