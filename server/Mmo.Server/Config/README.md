# Server Configuration

This directory contains configuration files for the server.

## Files

### prefabs.json

Defines all entity prefabs (templates) used in the game. Each prefab has:

- `id`: Unique numeric identifier (ushort, 0-65535)
- `name`: Human-readable name for the prefab
- `category`: The category (player, npc, mob, object)
- `description`: Optional description

**ID Ranges:**

- Players: 1-99
- NPCs: 100-999
    - Questgivers: 100-199
    - Vendors: 200-299
- Mobs: 1000-1999
- Objects: 2000-3999
    - Interactable: 2000-2999
    - Static: 3000-3999

**Usage:**

```csharp
// Load prefabs at server startup
PrefabRegistry.Instance.LoadFromFile("Config/prefabs.json");

// Access via PrefabIds (fallback to constants if not loaded)
var playerPrefab = PrefabIds.PlayerDefault; // Returns 1
var goblinPrefab = PrefabIds.MobGoblin;    // Returns 1000

// Or access directly via registry
var id = PrefabRegistry.Instance.GetId("mob", "goblin");
var config = PrefabRegistry.Instance.GetConfig(1000);
```

**Adding new prefabs:**

1. Add entry to `prefabs.json`
2. Optionally add property to `PrefabIds.cs` for convenient access
3. Restart server to reload

### Zones/

Contains zone configuration files. See individual zone JSON files for structure.
