using System.Reflection;
using Mmo.Shared.Character.Interfaces.Dtos;
using Mmo.Shared.Entities.Interfaces.Dtos;
using Mmo.Shared.Generators;
using Mmo.Shared.Zones.Interfaces;
using Mmo.Shared.Zones.Interfaces.Dtos;

namespace Mmo.Shared.Tests.Generators;

/// <summary>
///     Tests for the DeltaDtoGenerator to verify optional IdPropertyName handling.
/// </summary>
public class DeltaDtoGeneratorTests
{
    [Fact]
    public void GenerateDirtyTracking_WithoutIdPropertyName_GeneratesPlainDeltaDto()
    {
        // IZoneContextDelta should NOT implement IDeltaDto
        Type deltaType = typeof(IZoneContextDelta);

        // Should not implement IDeltaDto
        Assert.DoesNotContain(deltaType.GetInterfaces()
            , i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDeltaDto<>));
    }

    [Fact]
    public void GenerateDirtyTracking_WithoutIdPropertyName_DoesNotHaveGetIdMethod()
    {
        // IZoneContextDelta should NOT have GetId method
        Type deltaType = typeof(IZoneContextDelta);

        // Should not have GetId method
        Assert.Null(deltaType.GetMethod("GetId"));
    }

    [Fact]
    public void GenerateDirtyTracking_WithoutIdPropertyName_HasNullableProperties()
    {
        // IZoneContextDelta should have nullable properties
        Type deltaType = typeof(IZoneContextDelta);

        // Check CurrentWeather property is nullable
        PropertyInfo? weatherProp = deltaType.GetProperty("CurrentWeather");
        Assert.NotNull(weatherProp);

        // Should be nullable (either nullable value type or reference type)
        bool isNullable = Nullable.GetUnderlyingType(weatherProp!.PropertyType) != null
                          || !weatherProp.PropertyType.IsValueType;
        Assert.True(isNullable);
    }

    [Fact]
    public void GenerateDirtyTracking_WithIdPropertyName_GeneratesIDeltaDto()
    {
        // IEntityDelta SHOULD implement IDeltaDto<Guid>
        Type deltaType = typeof(IEntityDelta);

        // Should implement IDeltaDto<Guid>
        Assert.Contains(deltaType.GetInterfaces()
            , i => i.IsGenericType &&
                   i.GetGenericTypeDefinition() == typeof(IDeltaDto<>) &&
                   i.GetGenericArguments()[0] == typeof(Guid));
    }

    [Fact]
    public void GenerateDirtyTracking_WithIdPropertyName_HasGetIdMethod()
    {
        // IEntityDelta SHOULD have GetId method
        Type deltaType = typeof(IEntityDelta);

        // Should have GetId method
        Assert.NotNull(deltaType.GetMethod("GetId"));
    }

    [Fact]
    public void GenerateDirtyTracking_WithIdPropertyName_HasIdPropertyWithDeltaIdAttribute()
    {
        // IEntityDelta SHOULD have PersistentId with [DeltaId]
        Type deltaType = typeof(IEntityDelta);
        PropertyInfo? idProp = deltaType.GetProperty("PersistentId");

        Assert.NotNull(idProp);

        bool hasAttribute = idProp!.GetCustomAttributes(typeof(DeltaIdAttribute), false).Any();
        Assert.True(hasAttribute);
    }

    [Fact]
    public void GenerateDirtyTracking_ICharacterEntityDelta_InheritsIdFromIEntity()
    {
        // ICharacterEntityDelta inherits from ICombatEntity which inherits from IEntity
        // It should still implement IDeltaDto<Guid> because IEntity has IdPropertyName set
        Type deltaType = typeof(ICharacterEntityDelta);

        // Should implement IDeltaDto<Guid>
        Assert.Contains(deltaType.GetInterfaces()
            , i => i.IsGenericType &&
                   i.GetGenericTypeDefinition() == typeof(IDeltaDto<>) &&
                   i.GetGenericArguments()[0] == typeof(Guid));

        // Should have GetId method
        Assert.NotNull(deltaType.GetMethod("GetId"));
    }

    [Fact]
    public void IZoneContextDelta_HasCorrectTrackedProperties()
    {
        // Verify all tracked properties are present and nullable
        Type deltaType = typeof(IZoneContextDelta);

        // CurrentWeather (tracked)
        PropertyInfo? weatherProp = deltaType.GetProperty("CurrentWeather");
        Assert.NotNull(weatherProp);

        // TimeOfDay (tracked)
        PropertyInfo? timeProp = deltaType.GetProperty("TimeOfDay");
        Assert.NotNull(timeProp);
        Assert.Equal(typeof(float?), timeProp!.PropertyType);

        // ControllingFaction (tracked)
        PropertyInfo? factionProp = deltaType.GetProperty("ControllingFaction");
        Assert.NotNull(factionProp);
    }

    [Fact]
    public void IZoneContextDelta_DoesNotHaveNonTrackedProperties()
    {
        // Verify non-tracked properties are NOT in the delta
        Type deltaType = typeof(IZoneContextDelta);

        // ShardId (not tracked)
        Assert.Null(deltaType.GetProperty("ShardId"));

        // IsNight (computed property, not tracked)
        Assert.Null(deltaType.GetProperty("IsNight"));

        // LockReason (not tracked)
        Assert.Null(deltaType.GetProperty("LockReason"));

        // IsInstance (not tracked)
        Assert.Null(deltaType.GetProperty("IsInstance"));
    }

    [Fact]
    public void IEntityDelta_HasCorrectTrackedProperties()
    {
        // Verify IEntity's tracked properties
        Type deltaType = typeof(IEntityDelta);

        // Should have Position (tracked)
        PropertyInfo? positionProp = deltaType.GetProperty("Position");
        Assert.NotNull(positionProp);

        // Position should be nullable
        bool isNullable = Nullable.GetUnderlyingType(positionProp!.PropertyType) != null
                          || !positionProp.PropertyType.IsValueType;
        Assert.True(isNullable);
    }
}
