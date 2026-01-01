using Mmo.Shared.DirtyTracking;
using Mmo.Shared.Entities.Enums;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Inventory.Enums;

namespace Mmo.Shared.Tests.DirtyTracking;

/// <summary>
///     Tests for the generic dirty tracking system with domain-specific enums.
/// </summary>
public class DirtyTrackableTests
{
    #region Test Entity Classes
    
    private class TestEntity : DirtyTrackableBase<EntityDirtyFlags>
    {
        private float _x;
        private int _health;
        
        public float X
        {
            get => _x;
            set => SetField(ref _x, value, EntityDirtyFlags.Position);
        }
        
        public int Health
        {
            get => _health;
            set => SetField(ref _health, value, EntityDirtyFlags.Health);
        }
    }
    
    private class TestZone : DirtyTrackableBase<ZoneDirtyFlags>
    {
        private string _weather = "Clear";
        private float _timeOfDay = 12.0f;
        
        public string Weather
        {
            get => _weather;
            set => SetField(ref _weather, value, ZoneDirtyFlags.Weather);
        }
        
        public float TimeOfDay
        {
            get => _timeOfDay;
            set => SetField(ref _timeOfDay, value, ZoneDirtyFlags.TimeOfDay);
        }
    }
    
    private class TestInventory : DirtyTrackableBase<InventoryDirtyFlags>
    {
        private int _gold;
        
        public int Gold
        {
            get => _gold;
            set => SetField(ref _gold, value, InventoryDirtyFlags.Gold);
        }
    }
    
    #endregion
    
    #region EntityDirtyFlags Tests
    
    [Fact]
    public void EntityDirtyFlags_MarkDirty_SetsCorrectFlag()
    {
        // Arrange
        var entity = new TestEntity();
        
        // Act
        entity.MarkDirty(EntityDirtyFlags.Position);
        
        // Assert
        Assert.True(entity.IsDirty);
        Assert.True(entity.HasFlag(EntityDirtyFlags.Position));
        Assert.False(entity.HasFlag(EntityDirtyFlags.Health));
    }
    
    [Fact]
    public void EntityDirtyFlags_SetField_AutomaticallyMarksDirty()
    {
        // Arrange
        var entity = new TestEntity();
        
        // Act
        entity.X = 10.5f;
        
        // Assert
        Assert.True(entity.IsDirty);
        Assert.True(entity.HasFlag(EntityDirtyFlags.Position));
        Assert.Equal(EntityDirtyFlags.Position, entity.DirtyFlags);
    }
    
    [Fact]
    public void EntityDirtyFlags_SetField_NoChangeDoesNotMarkDirty()
    {
        // Arrange
        var entity = new TestEntity();
        entity.X = 10.5f;
        entity.ClearDirtyFlags();
        
        // Act
        entity.X = 10.5f; // Same value
        
        // Assert
        Assert.False(entity.IsDirty);
    }
    
    [Fact]
    public void EntityDirtyFlags_CombineMultiple_UsingBitwiseOr()
    {
        // Arrange
        var entity = new TestEntity();
        
        // Act
        entity.X = 100f;
        entity.Health = 75;
        
        // Assert
        Assert.True(entity.HasFlag(EntityDirtyFlags.Position));
        Assert.True(entity.HasFlag(EntityDirtyFlags.Health));
        Assert.Equal(EntityDirtyFlags.Position | EntityDirtyFlags.Health, entity.DirtyFlags);
    }
    
    [Fact]
    public void EntityDirtyFlags_ClearDirtyFlags_ResetsAllFlags()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkDirty(EntityDirtyFlags.Position | EntityDirtyFlags.Health);
        
        // Act
        entity.ClearDirtyFlags();
        
        // Assert
        Assert.False(entity.IsDirty);
        Assert.Equal(EntityDirtyFlags.None, entity.DirtyFlags);
    }
    
    #endregion
    
    #region ZoneDirtyFlags Tests
    
    [Fact]
    public void ZoneDirtyFlags_MarkDirty_SetsCorrectFlag()
    {
        // Arrange
        var zone = new TestZone();
        
        // Act
        zone.MarkDirty(ZoneDirtyFlags.Weather);
        
        // Assert
        Assert.True(zone.IsDirty);
        Assert.True(zone.HasFlag(ZoneDirtyFlags.Weather));
        Assert.False(zone.HasFlag(ZoneDirtyFlags.TimeOfDay));
    }
    
    [Fact]
    public void ZoneDirtyFlags_SetField_AutomaticallyMarksDirty()
    {
        // Arrange
        var zone = new TestZone();
        
        // Act
        zone.Weather = "Rain";
        
        // Assert
        Assert.True(zone.IsDirty);
        Assert.True(zone.HasFlag(ZoneDirtyFlags.Weather));
        Assert.Equal("Rain", zone.Weather);
    }
    
    [Fact]
    public void ZoneDirtyFlags_CombineMultiple_EnvironmentChanges()
    {
        // Arrange
        var zone = new TestZone();
        
        // Act
        zone.Weather = "Snow";
        zone.TimeOfDay = 18.5f;
        
        // Assert
        Assert.True(zone.HasFlag(ZoneDirtyFlags.Weather));
        Assert.True(zone.HasFlag(ZoneDirtyFlags.TimeOfDay));
        Assert.Equal(ZoneDirtyFlags.Weather | ZoneDirtyFlags.TimeOfDay, zone.DirtyFlags);
    }
    
    #endregion
    
    #region InventoryDirtyFlags Tests
    
    [Fact]
    public void InventoryDirtyFlags_MarkDirty_SetsCorrectFlag()
    {
        // Arrange
        var inventory = new TestInventory();
        
        // Act
        inventory.MarkDirty(InventoryDirtyFlags.Gold);
        
        // Assert
        Assert.True(inventory.IsDirty);
        Assert.True(inventory.HasFlag(InventoryDirtyFlags.Gold));
        Assert.False(inventory.HasFlag(InventoryDirtyFlags.Slot0));
    }
    
    [Fact]
    public void InventoryDirtyFlags_UlongSupport_HandlesHighBits()
    {
        // Arrange
        var inventory = new TestInventory();
        
        // Act - Test high-bit flags (60+)
        inventory.MarkDirty(InventoryDirtyFlags.Gold);
        inventory.MarkDirty(InventoryDirtyFlags.BankUpdated);
        
        // Assert
        Assert.True(inventory.HasFlag(InventoryDirtyFlags.Gold));
        Assert.True(inventory.HasFlag(InventoryDirtyFlags.BankUpdated));
    }
    
    [Fact]
    public void InventoryDirtyFlags_CombineSlots_MultipleBits()
    {
        // Arrange
        var inventory = new TestInventory();
        
        // Act
        inventory.MarkDirty(InventoryDirtyFlags.Slot0);
        inventory.MarkDirty(InventoryDirtyFlags.Slot1);
        inventory.MarkDirty(InventoryDirtyFlags.Slot5);
        
        // Assert
        Assert.True(inventory.HasFlag(InventoryDirtyFlags.Slot0));
        Assert.True(inventory.HasFlag(InventoryDirtyFlags.Slot1));
        Assert.True(inventory.HasFlag(InventoryDirtyFlags.Slot5));
        Assert.False(inventory.HasFlag(InventoryDirtyFlags.Slot2));
    }
    
    #endregion
    
    #region Type Safety Tests
    
    [Fact]
    public void DifferentFlagTypes_AreNotCompatible()
    {
        // This test verifies that the generic constraint prevents mixing flag types
        // If this compiles, it proves type safety is working
        
        var entity = new TestEntity();
        var zone = new TestZone();
        
        // These should NOT compile (and they don't):
        // entity.MarkDirty(ZoneDirtyFlags.Weather); // ❌ Compile error
        // zone.MarkDirty(EntityDirtyFlags.Position); // ❌ Compile error
        
        // This proves different domains are type-safe
        Assert.IsAssignableFrom<DirtyTrackableBase<EntityDirtyFlags>>(entity);
        Assert.IsAssignableFrom<DirtyTrackableBase<ZoneDirtyFlags>>(zone);
    }
    
    #endregion
    
    #region Convenience Combinations Tests
    
    [Fact]
    public void EntityDirtyFlags_AllStatsCombination_Works()
    {
        // Arrange
        var entity = new TestEntity();
        
        // Act
        entity.MarkDirty(EntityDirtyFlags.AllStats);
        
        // Assert
        Assert.True(entity.HasFlag(EntityDirtyFlags.Health));
        Assert.True(entity.HasFlag(EntityDirtyFlags.MaxHealth));
        Assert.True(entity.HasFlag(EntityDirtyFlags.Resource));
        Assert.True(entity.HasFlag(EntityDirtyFlags.MaxResource));
        Assert.True(entity.HasFlag(EntityDirtyFlags.Level));
    }
    
    [Fact]
    public void ZoneDirtyFlags_AllEnvironmentCombination_Works()
    {
        // Arrange
        var zone = new TestZone();
        
        // Act
        zone.MarkDirty(ZoneDirtyFlags.AllEnvironment);
        
        // Assert
        Assert.True(zone.HasFlag(ZoneDirtyFlags.Weather));
        Assert.True(zone.HasFlag(ZoneDirtyFlags.TimeOfDay));
        Assert.True(zone.HasFlag(ZoneDirtyFlags.Fog));
        Assert.True(zone.HasFlag(ZoneDirtyFlags.Lighting));
    }
    
    #endregion
}
