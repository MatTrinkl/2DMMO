using Mmo.Shared.Generators.Attributes;

namespace Mmo.Shared.Tests.Generators.TestData;

/// <summary>
/// Test class for ListEntry generator.
/// </summary>
[GenerateListEntry("ZoneListEntry")]
[GenerateListEntry("ZoneMapEntry")]
[GenerateListEntry("ZoneDungeonEntry")]
public readonly partial struct ZoneTestData
{
    [BaseData] 
    public ushort Id { get; }
    
    [BaseData] 
    public string Name { get; }
    
    [OptionalData("ZoneListEntry", "ZoneDungeonEntry")]
    public byte RecommendedLevel { get; }
    
    [OptionalData("ZoneMapEntry")]
    public bool IsDiscovered { get; }
    
    [OptionalData("ZoneDungeonEntry")]
    public ushort MaxPlayers { get; }
    
    [OptionalData("ZoneListEntry")]
    public int? Cost { get; }
    
    // No attribute = ignored
    public ZoneBoundsTestData Bounds { get; }
    
    [IgnoreData]
    public HashSet<Guid> EntityIds { get; }
}

/// <summary>
/// Test data for zone bounds.
/// </summary>
public readonly struct ZoneBoundsTestData
{
    public float MinX { get; }
    public float MinY { get; }
    public float MaxX { get; }
    public float MaxY { get; }
}
