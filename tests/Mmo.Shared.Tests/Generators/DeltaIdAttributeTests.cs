using Mmo.Shared.Generators;

namespace Mmo.Shared.Tests.Generators;

/// <summary>
///     Tests for the DeltaIdAttribute.
/// </summary>
public class DeltaIdAttributeTests
{
    [Fact]
    public void Attribute_CanBeAppliedToProperty()
    {
        var type = typeof(TestDeltaWithCustomId);
        var property = type.GetProperty(nameof(TestDeltaWithCustomId.PlayerId));
        
        Assert.NotNull(property);
        
        var attribute = property!.GetCustomAttributes(typeof(DeltaIdAttribute), false)
            .FirstOrDefault() as DeltaIdAttribute;
        
        Assert.NotNull(attribute);
    }
    
    [Fact]
    public void Attribute_CanIdentifyIdPropertyDynamically()
    {
        // Test that we can find the ID property by scanning for the attribute
        var type = typeof(TestDeltaWithCustomId);
        
        var idProperty = type.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttributes(typeof(DeltaIdAttribute), false).Any());
        
        Assert.NotNull(idProperty);
        Assert.Equal(nameof(TestDeltaWithCustomId.PlayerId), idProperty!.Name);
    }
    
    [Fact]
    public void Attribute_WorksWithDifferentIdNames()
    {
        var playerDeltaType = typeof(TestDeltaWithCustomId);
        var questDeltaType = typeof(TestQuestDelta);
        
        var playerIdProperty = playerDeltaType.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttributes(typeof(DeltaIdAttribute), false).Any());
        var questIdProperty = questDeltaType.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttributes(typeof(DeltaIdAttribute), false).Any());
        
        Assert.NotNull(playerIdProperty);
        Assert.NotNull(questIdProperty);
        Assert.Equal("PlayerId", playerIdProperty!.Name);
        Assert.Equal("QuestId", questIdProperty!.Name);
        Assert.NotEqual(playerIdProperty.Name, questIdProperty.Name);
    }
    
    [Fact]
    public void Attribute_OnlyOneIdPropertyPerClass()
    {
        var type = typeof(TestDeltaWithCustomId);
        
        var idProperties = type.GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(DeltaIdAttribute), false).Any())
            .ToList();
        
        Assert.Single(idProperties);
    }
}

/// <summary>
///     Test delta class with custom ID property name.
/// </summary>
public class TestDeltaWithCustomId
{
    [DeltaId]
    public Guid PlayerId { get; set; }
    
    public string? Username { get; set; }
}

/// <summary>
///     Test delta class with different ID property name.
/// </summary>
public class TestQuestDelta
{
    [DeltaId]
    public int QuestId { get; set; }
    
    public int? Progress { get; set; }
}
