using Mmo.Shared.Character.Interfaces.Dtos;
using Mmo.Shared.Entities.Interfaces.Dtos;
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
        var deltaType = typeof(IZoneContextDelta);

        // Should not implement IDeltaDto
        Assert.DoesNotContain(deltaType.GetInterfaces()
, i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDeltaDto<>));
    }

    [Fact]
    public void GenerateDirtyTracking_WithoutIdPropertyName_DoesNotHaveGetIdMethod()
    {
        // IZoneContextDelta should NOT have GetId method
        var deltaType = typeof(IZoneContextDelta);

        // Should not have GetId method
        Assert.Null(deltaType.GetMethod("GetId"));
    }

    [Fact]
    public void GenerateDirtyTracking_WithoutIdPropertyName_HasNullableProperties()
    {
        // IZoneContextDelta should have nullable properties
        var deltaType = typeof(IZoneContextDelta);

        // Check CurrentWeather property is nullable
        var weatherProp = deltaType.GetProperty("CurrentWeather");
        Assert.NotNull(weatherProp);

        // Should be nullable (either nullable value type or reference type)
        var isNullable = Nullable.GetUnderlyingType(weatherProp!.PropertyType) != null
                        || !weatherProp.PropertyType.IsValueType;
        Assert.True(isNullable);
    }

    [Fact]
    public void GenerateDirtyTracking_WithIdPropertyName_GeneratesIDeltaDto()
    {
        // IEntityDelta SHOULD implement IDeltaDto<Guid>
        var deltaType = typeof(IEntityDelta);

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
        var deltaType = typeof(IEntityDelta);

        // Should have GetId method
        Assert.NotNull(deltaType.GetMethod("GetId"));
    }

    [Fact]
    public void GenerateDirtyTracking_WithIdPropertyName_HasIdPropertyWithDeltaIdAttribute()
    {
        // IEntityDelta SHOULD have PersistentId with [DeltaId]
        var deltaType = typeof(IEntityDelta);
        var idProp = deltaType.GetProperty("PersistentId");

        Assert.NotNull(idProp);

        var hasAttribute = idProp!.GetCustomAttributes(typeof(Mmo.Shared.Generators.DeltaIdAttribute), false).Any();
        Assert.True(hasAttribute);
    }

    [Fact]
    public void GenerateDirtyTracking_ICharacterEntityDelta_InheritsIdFromIEntity()
    {
        // ICharacterEntityDelta inherits from ICombatEntity which inherits from IEntity
        // It should still implement IDeltaDto<Guid> because IEntity has IdPropertyName set
        var deltaType = typeof(ICharacterEntityDelta);

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
        var deltaType = typeof(IZoneContextDelta);

        // CurrentWeather (tracked)
        var weatherProp = deltaType.GetProperty("CurrentWeather");
        Assert.NotNull(weatherProp);

        // TimeOfDay (tracked)
        var timeProp = deltaType.GetProperty("TimeOfDay");
        Assert.NotNull(timeProp);
        Assert.Equal(typeof(float?), timeProp!.PropertyType);

        // ControllingFaction (tracked)
        var factionProp = deltaType.GetProperty("ControllingFaction");
        Assert.NotNull(factionProp);
    }

    [Fact]
    public void IZoneContextDelta_DoesNotHaveNonTrackedProperties()
    {
        // Verify non-tracked properties are NOT in the delta
        var deltaType = typeof(IZoneContextDelta);

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
        var deltaType = typeof(IEntityDelta);

        // Should have Position (tracked)
        var positionProp = deltaType.GetProperty("Position");
        Assert.NotNull(positionProp);

        // Position should be nullable
        var isNullable = Nullable.GetUnderlyingType(positionProp!.PropertyType) != null
                        || !positionProp.PropertyType.IsValueType;
        Assert.True(isNullable);
    }
}
