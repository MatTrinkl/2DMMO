using Mmo.Shared.Generators.Attributes;

namespace Mmo.Shared.Tests.Generators.TestData;

/// <summary>
/// Test class for ListEntry generator.
/// </summary>
[GenerateListEntry("ZoneListEntry")]
[GenerateListEntry("ZoneMapEntry")]
[GenerateListEntry("ZoneDungeonEntry")]
public partial class ZoneTestData
{
    [BaseData] 
    public ushort Id { get; init; }
    
    [BaseData] 
    public string Name { get; init; } = "";
    
    [OptionalData("ZoneListEntry", "ZoneDungeonEntry")]
    public byte RecommendedLevel { get; init; }
    
    [OptionalData("ZoneMapEntry")]
    public bool IsDiscovered { get; init; }
    
    [OptionalData("ZoneDungeonEntry")]
    public ushort MaxPlayers { get; init; }
    
    [OptionalData("ZoneListEntry")]
    public int? Cost { get; init; }
    
    // No attribute = ignored
    public ZoneBoundsTestData Bounds { get; init; }
    
    [IgnoreData]
    public HashSet<Guid> EntityIds { get; init; } = new();
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
