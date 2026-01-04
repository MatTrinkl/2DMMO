# Dirty-Tracking System

## Overview

The Dirty-Tracking System is an automatic change detection mechanism that optimizes network bandwidth by sending only changed entity properties. It's designed to be **extensible and generalizable** to any data structure, not just predefined entity types.

**🚀 NEW: Automatic Code Generation** - The `DirtyTrackingGenerator` source generator automatically creates all boilerplate code. Just mark your entity with attributes!

## Quick Start

```csharp
// 1. Mark your entity (must be partial!)
[GenerateDirtyTracking(IdPropertyName = "EntityId")]
public partial class PlayerEntity
{
    public Guid EntityId { get; set; }
    
    [TrackedProperty(DirtyFlags.Position)]
    public float X { get; set; }
    
    [TrackedProperty(DirtyFlags.Health)]
    public int HP { get; set; }
}

// 2. Generator automatically creates:
// - PlayerEntity.DirtyTracking.g.cs (IDirtyTrackable implementation)
// - PlayerEntityPositionDelta.g.cs (Delta DTO)
// - PlayerEntityStateDelta.g.cs (Delta DTO)
// - PlayerEntity.DeltaExtensions.g.cs (ToPositionDelta(), ToStateDelta())

// 3. Use it!
var entity = new PlayerEntity();
entity.X = 10;  // Change detected automatically (future enhancement)
var delta = entity.ToPositionDelta();  // Generated extension method
```

## Problem Statement

Without dirty-tracking, the server must send full entity updates every tick (40ms), even if only one property changed. For a zone with 100 entities:

```
❌ WITHOUT Dirty-Tracking:
- Full entity update: ~200 bytes per entity
- 100 entities × 200 bytes = 20 KB per tick
- 25 ticks/second × 20 KB = 500 KB/s per player
- 100 players = 50 MB/s bandwidth

✅ WITH Dirty-Tracking (Delta Updates):
- Position delta: ~30 bytes
- Only 20 entities moving per tick
- 20 entities × 30 bytes = 600 bytes per tick
- 25 ticks/second × 600 bytes = 15 KB/s per player
- 100 players = 1.5 MB/s bandwidth
- 💰 Bandwidth Reduction: 97%
```

## Architecture

### Core Components

```
┌─────────────────────────────────────────────────────────────┐
│  Dirty-Tracking System                                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────┐        ┌─────────────────────────┐  │
│  │  DirtyFlags      │◄───────│  IDirtyTrackable        │  │
│  │  (enum)          │        │  (interface)            │  │
│  │                  │        │                         │  │
│  │  - Position      │        │  + DirtyFlags           │  │
│  │  - Health        │        │  + IsDirty              │  │
│  │  - Movement      │        │  + ClearDirtyFlags()    │  │
│  └──────────────────┘        │  + MarkDirty(flags)     │  │
│           ▲                   └─────────────────────────┘  │
│           │                              ▲                  │
│           │                              │                  │
│  ┌────────┴────────────┐    ┌───────────┴─────────┐       │
│  │ TrackedProperty     │    │ Generated Code      │       │
│  │ Attribute           │    │ (via DtoGenerator)  │       │
│  │                     │    │                     │       │
│  │ [TrackedProperty(  │    │ - Property Wrappers│       │
│  │   DirtyFlags.Pos)] │    │ - Delta Extensions │       │
│  └─────────────────────┘    └─────────────────────┘       │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Delta DTOs                                           │ │
│  │  ┌──────────────────┐  ┌───────────────────┐         │ │
│  │  │ EntityPosition   │  │ EntityStateDelta  │         │ │
│  │  │ Delta            │  │                   │         │ │
│  │  │                  │  │ - CurrentHP?      │         │ │
│  │  │ - EntityId       │  │ - MaxHP?          │         │ │
│  │  │ - X, Y           │  │ - State?          │         │ │
│  │  │ - VelocityX/Y    │  │                   │         │ │
│  │  └──────────────────┘  └───────────────────┘         │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Design Principles

1. **Extensibility First**: System works with any entity type or data structure
2. **Nullable Pattern**: Delta DTOs use nullable fields (null = unchanged)
3. **Bitwise Flags**: DirtyFlags enum allows efficient flag combinations
4. **Generator-Driven**: Code generation reduces boilerplate and errors
5. **Zero Overhead**: No performance cost when properties don't change

## Usage

### 1. Basic Entity with Dirty-Tracking

```csharp
[GenerateDirtyTracking(IdPropertyName = "PersistentId")]
public partial class PlayerEntity
{
    // ID property - automatically transferred to Delta DTOs
    public Guid PersistentId { get; set; }
    
    [TrackedProperty(DirtyFlags.Position)]
    public float X { get; set; }
    
    [TrackedProperty(DirtyFlags.Position)]
    public float Y { get; set; }
    
    [TrackedProperty(DirtyFlags.Health)]
    public int CurrentHP { get; set; }
    
    [TrackedProperty(DirtyFlags.Health | DirtyFlags.MaxHealth)]
    public int MaxHP { get; set; }
    
    // Non-tracked properties work normally
    public string Name { get; set; }
}
```

**Generated Code** (by DirtyTrackingGenerator):

```csharp
// File: PlayerEntity.DirtyTracking.g.cs
// Auto-generated by DirtyTrackingGenerator
partial class PlayerEntity : IDirtyTrackable
{
    private DirtyFlags _dirtyFlags = DirtyFlags.None;
    
    public DirtyFlags DirtyFlags => _dirtyFlags;
    public bool IsDirty => _dirtyFlags != DirtyFlags.None;
    
    public void ClearDirtyFlags() => _dirtyFlags = DirtyFlags.None;
    public void MarkDirty(DirtyFlags flags) => _dirtyFlags |= flags;
}

// File: PlayerEntityPositionDelta.g.cs
// Auto-generated Delta DTO for Position-flagged properties
[MessagePackObject]
public class PlayerEntityPositionDelta : IDeltaDto<Guid>
{
    [Key(0)]
    [DeltaId]  // Automatically transferred from IdPropertyName
    public Guid PersistentId { get; set; }
    
    [Key(1)]
    public float? X { get; set; }  // Nullable for delta pattern
    
    [Key(2)]
    public float? Y { get; set; }
    
    public Guid GetId() => PersistentId;  // O(1) lookup helper
}

// File: PlayerEntityStateDelta.g.cs  
// Auto-generated Delta DTO for Health-flagged properties
[MessagePackObject]
public class PlayerEntityStateDelta : IDeltaDto<Guid>
{
    [Key(0)]
    [DeltaId]
    public Guid PersistentId { get; set; }
    
    [Key(1)]
    public int? CurrentHP { get; set; }
    
    [Key(2)]
    public int? MaxHP { get; set; }
    
    public Guid GetId() => PersistentId;
}

// File: PlayerEntity.DeltaExtensions.g.cs
// Auto-generated extension methods
public static class PlayerEntityDeltaExtensions
{
    public static PlayerEntityPositionDelta ToPositionDelta(this PlayerEntity entity)
    {
        return new PlayerEntityPositionDelta
        {
            PersistentId = entity.PersistentId,
            X = entity.X,
            Y = entity.Y
        };
    }
    
    public static PlayerEntityStateDelta ToStateDelta(this PlayerEntity entity)
    {
        return new PlayerEntityStateDelta
        {
            PersistentId = entity.PersistentId,
            CurrentHP = entity.CurrentHP,
            MaxHP = entity.MaxHP
        };
    }
}
```

**Note:** Property wrappers for automatic dirty flag setting on property changes are a future enhancement. Currently, use `MarkDirty()` manually when properties change.

### 2. Creating Delta DTOs

Delta DTOs follow the **Nullable Pattern** and use the **`[DeltaId]` Attribute** to identify the ID field:

```csharp
[MessagePackObject]
public class EntityStateDelta
{
    [Key(0)]
    [DeltaId]  // Marks this as the identifier field
    public Guid EntityId { get; set; }
    
    // Nullable fields: null = unchanged, value = changed
    [Key(1)] public int? CurrentHP { get; set; }
    [Key(2)] public int? MaxHP { get; set; }
    [Key(3)] public byte? State { get; set; }
}
```

**The `[DeltaId]` Attribute:**

The `DeltaIdAttribute` allows the system to recognize which property contains the entity identifier, regardless of its name. This is crucial for extensibility since different data structures use different ID field names:

```csharp
// Entity delta - uses EntityId
[MessagePackObject]
public class EntityPositionDelta
{
    [Key(0)]
    [DeltaId]
    public Guid EntityId { get; set; }
    // ...
}

// Player-specific delta - uses PlayerId
[MessagePackObject]
public class PlayerInventoryDelta
{
    [Key(0)]
    [DeltaId]
    public Guid PlayerId { get; set; }
    // ...
}

// Quest delta - uses QuestId (different type too!)
[MessagePackObject]
public class QuestProgressDelta
{
    [Key(0)]
    [DeltaId]
    public int QuestId { get; set; }
    // ...
}
```

This attribute enables:
- **Automatic ID field detection** in code generators
- **Flexible naming** (EntityId, PlayerId, QuestId, etc.)
- **Different ID types** (Guid, int, long, etc.)
- **Reflection-based helpers** to find the ID property dynamically

**Usage:**

```csharp
// Server-side: Create delta based on dirty flags
if (entity.DirtyFlags.HasFlag(DirtyFlags.Health))
{
    var delta = new EntityStateDelta
    {
        EntityId = entity.PersistentId,
        CurrentHP = entity.CurrentHP,  // Changed
        MaxHP = null,                   // Not changed
        State = null                    // Not changed
    };
    
    SendToClients(delta);
    entity.ClearDirtyFlags();
}

// Client-side: Apply delta with O(1) lookup
public void ApplyDeltas(List<EntityStateDelta> stateDeltas)
{
    // Convert to dictionary for O(1) lookup
    var deltaDict = stateDeltas.ToDeltaDictionary<Guid, EntityStateDelta>();
    
    // Fast lookup for each entity
    foreach (var entity in visibleEntities)
    {
        if (deltaDict.TryGetValue(entity.PersistentId, out var delta))
        {
            if (delta.CurrentHP.HasValue)
                entity.CurrentHP = delta.CurrentHP.Value;
            
            if (delta.MaxHP.HasValue)
                entity.MaxHP = delta.MaxHP.Value;
            
            if (delta.State.HasValue)
                entity.State = (EntityState)delta.State.Value;
        }
    }
}

// Alternative: Single entity lookup
public void ApplyDelta(EntityStateDelta delta)
{
    var entity = GetEntity(delta.GetId());  // O(1) using GetId() helper
    
    if (delta.CurrentHP.HasValue)
        entity.CurrentHP = delta.CurrentHP.Value;
    
    if (delta.MaxHP.HasValue)
        entity.MaxHP = delta.MaxHP.Value;
    
    if (delta.State.HasValue)
        entity.State = (EntityState)delta.State.Value;
}
```

### 3. O(1) Lookup with IDeltaDto Interface

All Delta DTOs implement `IDeltaDto<TId>` for efficient ID-based lookup:

```csharp
// Delta DTOs implement IDeltaDto<TId>
public class EntityPositionDelta : IDeltaDto<Guid>
{
    [Key(0)]
    [DeltaId]
    public Guid EntityId { get; set; }
    
    // ... other properties
    
    // O(1) lookup helper
    public Guid GetId() => EntityId;
}

// Server: Send list of deltas
var positionDeltas = new List<EntityPositionDelta> { /* ... */ };
SendToClients(new ZoneDelta { PositionUpdates = positionDeltas });

// Client: Convert to dictionary for O(1) lookup
var deltaDict = zoneDelta.PositionUpdates.ToDeltaDictionary<Guid, EntityPositionDelta>();

// Fast lookup by entity ID
if (deltaDict.TryGetValue(myEntityId, out var delta))
{
    // Apply delta - O(1) lookup
    entity.X = delta.X;
    entity.Y = delta.Y;
}

// Performance comparison:
// - Linear search: O(n) - slow for large lists
// - Dictionary lookup: O(1) - fast regardless of list size
```

**Extension Methods:**

```csharp
// Convert to dictionary for O(1) lookup
var dict = deltas.ToDeltaDictionary<Guid, EntityPositionDelta>();

// Or use TryGetDelta for single lookup (O(n))
if (deltas.TryGetDelta(entityId, out var delta))
{
    // Process delta
}
```

### 4. Delta DTOs with and without ID

#### With ID (for Lists with Lookup)

For objects that occur in lists and need to be looked up by ID:

```csharp
[GenerateDirtyTracking(
    IdPropertyName = "PersistentId",  // ← Specify ID
    FlagsEnumType = "Mmo.Shared.Entities.Enums.EntityDirtyFlags"
)]
public interface IEntity
{
    Guid PersistentId { get; }
    
    [TrackDirty("Position")]
    Position Position { get; set; }
}

// Generated: IEntityDelta : IDeltaDto<Guid>
// With GetId() for O(1) Dictionary-Lookup
```

**Generated Code:**

```csharp
[MessagePackObject]
public class IEntityDelta : IDeltaDto<System.Guid>
{
    [Key(0)]
    [DeltaId]
    public System.Guid PersistentId { get; set; }
    
    [Key(1)]
    public Position? Position { get; set; }
    
    public System.Guid GetId() => PersistentId;  // For O(1) lookup
}
```

#### Without ID (for Singletons)

For objects where there is only one instance (ZoneContext, PlayerStats, etc.):

```csharp
[GenerateDirtyTracking(
    // IdPropertyName NOT specified!
    FlagsEnumType = "Mmo.Shared.Zones.Enums.ZoneDirtyFlags"
)]
public interface IZoneContext
{
    [TrackDirty("Weather")]
    WeatherType CurrentWeather { get; set; }
    
    [TrackDirty("TimeOfDay")]
    float TimeOfDay { get; set; }
}

// Generated: IZoneContextDelta (plain DTO)
// No IDeltaDto, no GetId() - not needed!
```

**Generated Code:**

```csharp
[MessagePackObject]
public class IZoneContextDelta  // NO IDeltaDto<T>!
{
    [Key(0)]
    public WeatherType? CurrentWeather { get; set; }
    
    [Key(1)]
    public float? TimeOfDay { get; set; }
    
    // NO GetId() - not needed for singletons!
}
```

**When to use which:**

| Use Case | IdPropertyName | Generated Delta DTO | Use For |
|----------|----------------|---------------------|---------|
| **Entities in lists** | `"PersistentId"` | `IEntityDelta : IDeltaDto<Guid>` with `GetId()` | Entities, Players, NPCs, Items in lists |
| **Singleton objects** | `null` (not specified) | `IZoneContextDelta` (plain DTO, no interface) | Zone context, Guild info, Server config |

### 5. Custom DirtyFlags for Different Entity Types

The system is extensible to other domains:

```csharp
// Example: Quest system with custom dirty flags
[Flags]
public enum QuestDirtyFlags : uint
{
    None = 0,
    Objectives = 1 << 0,
    Progress = 1 << 1,
    Rewards = 1 << 2,
    State = 1 << 3,
    
    // Combinations
    All = Objectives | Progress | Rewards | State
}

[GenerateDirtyTracking]
public partial class Quest
{
    [TrackedProperty(QuestDirtyFlags.Objectives)]
    public List<QuestObjective> Objectives { get; set; }
    
    [TrackedProperty(QuestDirtyFlags.Progress)]
    public int CurrentProgress { get; set; }
    
    [TrackedProperty(QuestDirtyFlags.State)]
    public QuestState State { get; set; }
}

// Delta DTO for quest updates
[MessagePackObject]
public class QuestDelta
{
    [Key(0)] public int QuestId { get; set; }
    [Key(1)] public List<QuestObjective>? Objectives { get; set; }
    [Key(2)] public int? CurrentProgress { get; set; }
    [Key(3)] public QuestState? State { get; set; }
}
```

## DirtyFlags Enum

### Predefined Flags (Entity-Specific)

```csharp
[Flags]
public enum DirtyFlags : uint
{
    None = 0,
    
    // Movement (Bits 0-2)
    Position = 1 << 0,   // 0x00000001
    Velocity = 1 << 1,   // 0x00000002
    Rotation = 1 << 2,   // 0x00000004
    
    // Combat/State (Bits 3-7)
    Health = 1 << 3,     // 0x00000008
    MaxHealth = 1 << 4,  // 0x00000010
    Resource = 1 << 5,   // 0x00000020
    MaxResource = 1 << 6,// 0x00000040
    State = 1 << 7,      // 0x00000080
    
    // Visual (Bits 8-9)
    Model = 1 << 8,      // 0x00000100
    Level = 1 << 9,      // 0x00000200
    
    // Lifecycle (Bits 30-31)
    Spawned = 1u << 30,  // 0x40000000
    Despawned = 1u << 31,// 0x80000000
    
    // Combinations
    Movement = Position | Velocity | Rotation,
    Combat = Health | MaxHealth | State,
    AllStats = Health | MaxHealth | Resource | MaxResource | Level
}
```

### Bitwise Operations

```csharp
// Setting flags
entity.MarkDirty(DirtyFlags.Position);
entity.MarkDirty(DirtyFlags.Health | DirtyFlags.MaxHealth);

// Checking flags
if (entity.DirtyFlags.HasFlag(DirtyFlags.Position))
{
    // Send position update
}

// Checking multiple flags
if ((entity.DirtyFlags & DirtyFlags.Movement) != DirtyFlags.None)
{
    // Any movement property changed
}

// Clearing flags
entity.ClearDirtyFlags();
```

## Delta DTOs

### EntityPositionDelta

**Purpose:** Transmit position and movement changes.

```csharp
[MessagePackObject]
public class EntityPositionDelta
{
    [Key(0)] public Guid EntityId { get; set; }
    [Key(1)] public float X { get; set; }
    [Key(2)] public float Y { get; set; }
    [Key(3)] public float VelocityX { get; set; }
    [Key(4)] public float VelocityY { get; set; }
    [Key(5)] public float? Rotation { get; set; }  // Optional
}
```

**Use Case:** High-frequency movement updates (every tick for moving entities).

### EntityStateDelta

**Purpose:** Transmit state and stat changes.

```csharp
[MessagePackObject]
public class EntityStateDelta
{
    [Key(0)] public Guid EntityId { get; set; }
    [Key(1)] public int? CurrentHP { get; set; }
    [Key(2)] public int? MaxHP { get; set; }
    [Key(3)] public byte? State { get; set; }
    [Key(4)] public uint? ModelId { get; set; }
    [Key(5)] public int? Level { get; set; }
}
```

**Use Case:** Combat updates, level-ups, polymorphs.

### ZoneContextDelta

**Purpose:** Transmit zone-wide environmental changes.

```csharp
[MessagePackObject]
public class ZoneContextDelta
{
    [Key(0)] public WeatherType? CurrentWeather { get; set; }
    [Key(1)] public float? TimeOfDay { get; set; }
}
```

**Use Case:** Weather changes, day/night cycle.

## ZoneDelta Message

The `ZoneDelta` message batches all delta updates for a tick:

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneDelta)]
public class ZoneDelta : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneDelta;
    [Key(1)] public long Timestamp { get; init; }
    [Key(2)] public ushort ZoneId { get; init; }
    
    // All collections are nullable (null = no changes)
    [Key(3)] public List<EntityDtoUnion>? SpawnedEntities { get; init; }
    [Key(4)] public List<Guid>? DespawnedEntityIds { get; init; }
    [Key(5)] public List<EntityPositionDelta>? PositionUpdates { get; init; }
    [Key(6)] public List<EntityStateDelta>? StateUpdates { get; init; }
    [Key(7)] public ZoneContextDelta? ContextDelta { get; init; }
}
```

**Server-Side (Game Loop - Output Phase):**

```csharp
public ZoneDelta BuildDeltaForClient(Guid clientId, long currentTick)
{
    var player = _playerService.GetPlayer(clientId);
    var visibleEntities = _chunkService.GetVisibleEntities(player);
    
    var delta = new ZoneDelta
    {
        Timestamp = currentTick,
        ZoneId = player.CurrentZoneId
    };
    
    // Collect position changes
    var positionDeltas = new List<EntityPositionDelta>();
    foreach (var entity in visibleEntities)
    {
        if (entity.DirtyFlags.HasFlag(DirtyFlags.Position))
        {
            positionDeltas.Add(new EntityPositionDelta
            {
                EntityId = entity.PersistentId,
                X = entity.X,
                Y = entity.Y,
                VelocityX = entity.VelocityX,
                VelocityY = entity.VelocityY,
                Rotation = entity.DirtyFlags.HasFlag(DirtyFlags.Rotation) 
                    ? entity.Rotation 
                    : null
            });
        }
    }
    delta.PositionUpdates = positionDeltas.Any() ? positionDeltas : null;
    
    // Similar for StateUpdates, ContextDelta, etc.
    
    // Clear dirty flags after collecting
    foreach (var entity in visibleEntities)
    {
        entity.ClearDirtyFlags();
    }
    
    return delta;
}
```

## Extensibility Patterns

### 1. Inventory System Example

```csharp
// Custom DirtyFlags
[Flags]
public enum InventoryDirtyFlags : uint
{
    None = 0,
    Slot1 = 1 << 0,
    Slot2 = 1 << 1,
    // ... up to Slot32
    Gold = 1u << 31,
    
    AllSlots = 0x7FFFFFFF,  // All slots except Gold
    Everything = 0xFFFFFFFF
}

// Tracked inventory
[GenerateDirtyTracking]
public partial class Inventory
{
    [TrackedProperty(InventoryDirtyFlags.Slot1)]
    public Item? Slot1 { get; set; }
    
    // ... more slots
    
    [TrackedProperty(InventoryDirtyFlags.Gold)]
    public int Gold { get; set; }
}

// Delta DTO
[MessagePackObject]
public class InventoryDelta
{
    [Key(0)]
    [DeltaId]  // Marks PlayerId as the identifier
    public Guid PlayerId { get; set; }
    
    [Key(1)] public Item? Slot1 { get; set; }
    // ... only changed slots
    [Key(32)] public int? Gold { get; set; }
}
```

### 2. Guild System Example

```csharp
[Flags]
public enum GuildDirtyFlags : uint
{
    None = 0,
    Name = 1 << 0,
    Members = 1 << 1,
    Level = 1 << 2,
    Experience = 1 << 3,
    Perks = 1 << 4,
    BankContents = 1 << 5
}

[GenerateDirtyTracking]
public partial class Guild
{
    [TrackedProperty(GuildDirtyFlags.Name)]
    public string Name { get; set; }
    
    [TrackedProperty(GuildDirtyFlags.Members)]
    public List<GuildMember> Members { get; set; }
    
    [TrackedProperty(GuildDirtyFlags.Experience)]
    public long Experience { get; set; }
}

// Delta DTO for guild updates
[MessagePackObject]
public class GuildDelta
{
    [Key(0)]
    [DeltaId]  // Marks GuildId as the identifier
    public Guid GuildId { get; set; }
    
    [Key(1)] public string? Name { get; set; }
    [Key(2)] public List<GuildMember>? Members { get; set; }
    [Key(3)] public long? Experience { get; set; }
}
```

### 3. Helper Method to Find ID Property

The `[DeltaId]` attribute enables reflection-based helpers:

```csharp
public static class DeltaHelper
{
    /// <summary>
    ///     Finds the ID property in a delta DTO using the [DeltaId] attribute.
    /// </summary>
    public static PropertyInfo? GetIdProperty(Type deltaType)
    {
        return deltaType.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<DeltaIdAttribute>() != null);
    }
    
    /// <summary>
    ///     Gets the ID value from a delta DTO instance.
    /// </summary>
    public static object? GetIdValue(object deltaInstance)
    {
        var idProperty = GetIdProperty(deltaInstance.GetType());
        return idProperty?.GetValue(deltaInstance);
    }
}

// Usage:
var delta = new PlayerInventoryDelta { PlayerId = playerId, ... };
var id = DeltaHelper.GetIdValue(delta);  // Returns the PlayerId value
```
```

## Performance Considerations

### Memory Usage

- `DirtyFlags`: 4 bytes (uint) per trackable entity
- Property wrappers: Minimal overhead (one extra field per tracked property)
- Delta DTOs: Only allocate when changes exist

### CPU Usage

- Bitwise operations are extremely fast (~1 CPU cycle)
- Property change detection: Simple value comparison
- No reflection or dynamic code at runtime

### Network Bandwidth

**Example: 100-entity zone, 25 Hz update rate**

| Scenario | Entities Changed | Data/Tick | Data/Second | Bandwidth (100 players) |
|----------|------------------|-----------|-------------|-------------------------|
| Full Sync | 100 | 20 KB | 500 KB/s | 50 MB/s |
| Delta (10% moving) | 10 | 600 bytes | 15 KB/s | 1.5 MB/s |
| Delta (1 HP change) | 1 | 20 bytes | 500 bytes/s | 50 KB/s |

**Savings: 90-99% bandwidth reduction**

## Best Practices

### DO ✅

1. **Clear dirty flags after synchronization**
   ```csharp
   SendDeltaToClients(delta);
   entity.ClearDirtyFlags();
   ```

2. **Use predefined flag combinations**
   ```csharp
   entity.MarkDirty(DirtyFlags.Movement);  // Position + Velocity + Rotation
   ```

3. **Batch updates in game loop**
   ```csharp
   // Collect all deltas, send one ZoneDelta message
   ```

4. **Make Delta DTOs serializable**
   ```csharp
   [MessagePackObject]
   public class CustomDelta { ... }
   ```

5. **Use nullable pattern for optional fields**
   ```csharp
   [Key(5)] public float? Rotation { get; set; }  // null = unchanged
   ```

### DON'T ❌

1. **Don't forget to clear flags**
   ```csharp
   // ❌ Will keep resending the same update
   SendDelta(delta);
   // ✅ Clear after sending
   entity.ClearDirtyFlags();
   ```

2. **Don't use dirty tracking for infrequent events**
   ```csharp
   // ❌ Overkill for rare events
   [TrackedProperty(DirtyFlags.Achievement)]
   public Achievement LastAchievement { get; set; }
   
   // ✅ Use dedicated message instead
   SendMessage(new AchievementUnlocked { ... });
   ```

3. **Don't mix null and default values**
   ```csharp
   // ❌ Ambiguous: is 0 a change or no change?
   [Key(1)] public int CurrentHP { get; set; }
   
   // ✅ Clear semantics
   [Key(1)] public int? CurrentHP { get; set; }  // null = no change
   ```

4. **Don't track immutable properties**
   ```csharp
   // ❌ ID never changes
   [TrackedProperty(DirtyFlags.Id)]
   public Guid PersistentId { get; set; }
   
   // ✅ Just a regular property
   public Guid PersistentId { get; set; }
   ```

## Testing

See `tests/Mmo.Shared.Tests/` for comprehensive test suites:

- `Entities/DirtyFlagsTests.cs` - Enum functionality
- `Generators/TrackedPropertyAttributeTests.cs` - Attribute validation
- `Zones/DeltaDtoTests.cs` - Delta DTO serialization and nullable pattern

## Future Enhancements

1. **Generic Delta Generator**: Auto-generate delta DTOs from entities
2. **Compression**: Bit-packing for ultra-compact deltas
3. **Priority System**: Send high-priority changes first
4. **Interpolation Hints**: Include velocity for client-side prediction
5. **Delta History**: Track change history for debugging

## See Also

- [Zone Messages Documentation](../API/01-zone.md)
- [Chunk-Based Sync](CHUNK_BASED_SYNC.md)
- [Game Loop Architecture](GAME_LOOP.md)
- [Network Protocol](NETWORK_PROTOCOL.md)

Source: docs/02-architecture/DIRTY_TRACKING.md
