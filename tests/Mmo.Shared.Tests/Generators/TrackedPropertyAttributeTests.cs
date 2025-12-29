using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Generators;

namespace Mmo.Shared.Tests.Generators;

/// <summary>
///     Tests for the TrackedPropertyAttribute.
/// </summary>
public class TrackedPropertyAttributeTests
{
    [Fact]
    public void Constructor_SetsFlag()
    {
        var attribute = new TrackedPropertyAttribute(DirtyFlags.Position);
        
        Assert.Equal(DirtyFlags.Position, attribute.Flag);
    }
    
    [Fact]
    public void Constructor_AcceptsCombinedFlags()
    {
        var combinedFlag = DirtyFlags.Position | DirtyFlags.Velocity;
        var attribute = new TrackedPropertyAttribute(combinedFlag);
        
        Assert.Equal(combinedFlag, attribute.Flag);
    }
    
    [Fact]
    public void Constructor_AcceptsPredefinedCombinations()
    {
        var attribute = new TrackedPropertyAttribute(DirtyFlags.Movement);
        
        Assert.Equal(DirtyFlags.Movement, attribute.Flag);
    }
    
    [Theory]
    [InlineData(DirtyFlags.None)]
    [InlineData(DirtyFlags.Position)]
    [InlineData(DirtyFlags.Health)]
    [InlineData(DirtyFlags.Movement)]
    [InlineData(DirtyFlags.Combat)]
    [InlineData(DirtyFlags.AllStats)]
    public void Constructor_AcceptsAllDirtyFlagValues(DirtyFlags flag)
    {
        var attribute = new TrackedPropertyAttribute(flag);
        
        Assert.Equal(flag, attribute.Flag);
    }
    
    [Fact]
    public void Attribute_CanBeAppliedToProperty()
    {
        // This is a compile-time test - if it compiles, the attribute can be applied
        var type = typeof(TestEntityWithTrackedProperty);
        var property = type.GetProperty(nameof(TestEntityWithTrackedProperty.X));
        
        Assert.NotNull(property);
        
        var attribute = property!.GetCustomAttributes(typeof(TrackedPropertyAttribute), false)
            .FirstOrDefault() as TrackedPropertyAttribute;
        
        Assert.NotNull(attribute);
        Assert.Equal(DirtyFlags.Position, attribute!.Flag);
    }
    
    [Fact]
    public void Attribute_MultiplePropertiesCanHaveDifferentFlags()
    {
        var type = typeof(TestEntityWithTrackedProperty);
        
        var xProperty = type.GetProperty(nameof(TestEntityWithTrackedProperty.X));
        var healthProperty = type.GetProperty(nameof(TestEntityWithTrackedProperty.Health));
        
        var xAttribute = xProperty!.GetCustomAttributes(typeof(TrackedPropertyAttribute), false)
            .FirstOrDefault() as TrackedPropertyAttribute;
        var healthAttribute = healthProperty!.GetCustomAttributes(typeof(TrackedPropertyAttribute), false)
            .FirstOrDefault() as TrackedPropertyAttribute;
        
        Assert.NotNull(xAttribute);
        Assert.NotNull(healthAttribute);
        Assert.Equal(DirtyFlags.Position, xAttribute!.Flag);
        Assert.Equal(DirtyFlags.Health, healthAttribute!.Flag);
    }
}

/// <summary>
///     Test entity class with tracked properties for attribute testing.
/// </summary>
public class TestEntityWithTrackedProperty
{
    [TrackedProperty(DirtyFlags.Position)]
    public float X { get; set; }
    
    [TrackedProperty(DirtyFlags.Position)]
    public float Y { get; set; }
    
    [TrackedProperty(DirtyFlags.Health)]
    public int Health { get; set; }
}
