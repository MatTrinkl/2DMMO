# 🎒 Inventory Messages (500-599)

**Kategorie:** 05  
**Range:** 500-599  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [InventorySync (500)](#inventorysync-500)
- [ItemAdd (501)](#itemadd-501)
- [ItemRemove (502)](#itemremove-502)
- [ItemMove (503)](#itemmove-503)
- [ItemSplit (504)](#itemsplit-504)
- [ItemUse (505)](#itemuse-505)
- [ItemDelete (506)](#itemdelete-506)
- [ItemStack (507)](#itemstack-507)
- [ItemSort (508)](#itemsort-508)
- [ItemLock (509)](#itemlock-509)
- [BagExpand (510)](#bagexpand-510)
- [InventoryFullNotification (511)](#inventoryfullnotification-511)
- [ItemMoveResponse (520)](#itemmoveresponse-520)
- [ItemSplitResponse (521)](#itemsplitresponse-521)
- [ItemUseResponse (522)](#itemuseresponse-522)
- [ItemDeleteResponse (523)](#itemdeleteresponse-523)
- [ItemStackResponse (524)](#itemstackresponse-524)
- [ItemSortResponse (525)](#itemsortresponse-525)
- [ItemLockResponse (526)](#itemlockresponse-526)
- [BagExpandResponse (527)](#bagexpandresponse-527)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Inventory-System** im 2DMMO.

Das Inventory-System implementiert:
- **Multi-Bag System**: Backpack + 4 zusätzliche Bags (erweiterbar)
- **Item Stacking**: Items können bis zum MaxStack gestackt werden
- **Drag & Drop**: Client-side Drag&Drop mit Server-Validation
- **Auto-Stacking**: Neue Items werden automatisch zu bestehenden Stacks hinzugefügt
- **Sorting**: Auto-Sort nach Typ, Qualität, oder Name
- **Item Locking**: Items können gelockt werden (verhindert versehentliches Löschen/Verkaufen)
- **Inventory Capacity**: Backpack: 16 Slots, zusätzliche Bags: 8-20 Slots (je nach Qualität)

**Server Authority**: Alle Inventory-Changes sind server-authoritative. Client sendet Requests, Server validiert und broadcastet Changes.

**Performance**: Inventory-Updates werden gebatched (max 20 Updates/Sekunde).

---

## InventorySync (500)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nur bei Login/Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert das komplette Inventory des Spielers nach Login oder Reconnect. Enthält alle Items in allen Bags mit vollständigen Daten (ItemID, Quantity, Slot, Properties).

Diese Message wird nur einmal pro Session gesendet. Nachfolgende Änderungen werden via `ItemAdd` (501), `ItemRemove` (502), etc. kommuniziert.

### Im Scope ✅
- Vollständige Inventory-Daten (alle Bags)
- Item-Properties (Durability, Enchantments, Soulbound-Status)
- Bag-Configuration (welche Bags sind equipped)
- Locked-Items Status

### Nicht im Scope ❌
- Bank-Items → verwende `BankSync` (4000)
- Equipment-Items → verwende `EquipmentSync` (3900)
- Laufende Updates → verwende `ItemAdd/Remove` (501/502)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BackpackSlots | byte | Anzahl Backpack-Slots (16-24) | Ja |
| EquippedBags | List<BagInfo> | Info über equipped Bags | Ja |
| Items | List<InventoryItem> | Alle Items | Ja |
| Gold | long | Gold-Amount (in Copper) | Ja |

**BagInfo:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| BagSlot | byte | Bag-Slot (0-3, Backpack = -1) |
| BagItemId | uint | Item-ID des Bag-Items |
| Slots | byte | Anzahl Slots in diesem Bag |

**InventoryItem:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SlotIndex | byte | Slot-Index (0-99) |
| BagId | byte | Bag-ID (-1=Backpack, 0-3=Bags) |
| ItemId | uint | Item-ID |
| Quantity | int | Stack-Größe |
| Durability | int | Haltbarkeit (0-100) |
| IsLocked | bool | Item gelockt? |
| IsSoulbound | bool | Soulbound? |
| Enchantments | List<uint> | Enchantment-IDs |

### Erwartete Response
- Keine Response erforderlich (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ItemAdd` | 501 | Für neue Items nach Login |
| `ItemRemove` | 502 | Für entfernte Items |
| `BankSync` | 4000 | Für Bank-Items |
| `EquipmentSync` | 3900 | Für equipped Items |

### Flow-Diagramm
```
Client                    Server                    Database
  │                          │                          │
  │  LoginSuccess            │                          │
  │◄─────────────────────────│                          │
  │                          │  Load Inventory          │
  │                          │─────────────────────────►│
  │                          │                          │
  │                          │  Inventory Data          │
  │                          │◄─────────────────────────│
  │  InventorySync (500)     │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  (Client populates UI)   │                          │
```

### Beispiel Payload
```csharp
var inventorySync = new InventorySync
{
    Type = MessageType.InventorySync,
    BackpackSlots = 16,
    EquippedBags = new List<BagInfo>
    {
        new BagInfo { BagSlot = 0, BagItemId = 5001, Slots = 8 }, // Small Bag
        new BagInfo { BagSlot = 1, BagItemId = 5002, Slots = 12 } // Medium Bag
    },
    Items = new List<InventoryItem>
    {
        new InventoryItem 
        { 
            SlotIndex = 0, 
            BagId = -1, // Backpack
            ItemId = 2001, // Health Potion
            Quantity = 5,
            Durability = 100,
            IsLocked = false,
            IsSoulbound = false
        },
        new InventoryItem
        {
            SlotIndex = 1,
            BagId = -1,
            ItemId = 1001, // Iron Sword
            Quantity = 1,
            Durability = 85,
            IsLocked = true, // Locked!
            IsSoulbound = true,
            Enchantments = new List<uint> { 3001 } // +5 Fire Damage
        }
    },
    Gold = 12345 // 1 Gold, 23 Silver, 45 Copper
};
```

### Error Codes
Keine - InventorySync kann nicht fehlschlagen (ist initiale Sync)

### Notizen
- **Loading-Time**: Kann 100-500ms dauern bei vielen Items
- **Compression**: Message wird komprimiert bei >50 Items (Phase 3)
- **Caching**: Client cached Inventory-State lokal
- **Max Items**: Max 100 Items im Inventory (16 Backpack + 4x21 Bags)
- **Gold-Format**: Stored als Copper (1 Gold = 10000 Copper)

---

## ItemAdd (501)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ Sehr häufig (Loot, Crafting, Trading)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Item wurde dem Inventory hinzugefügt. Dies kann durch Loot, Crafting, Trading, Quest-Rewards, oder Item-Kauf geschehen. Server findet automatisch den besten Slot (existierender Stack oder erster freier Slot).

### Im Scope ✅
- Neue Items (via Loot, Crafting, Trading, etc.)
- Auto-Stacking zu existierenden Stacks
- Slot-Allocation durch Server
- Item-Properties (Durability, Soulbound, etc.)

### Nicht im Scope ❌
- Item-Movement zwischen Slots → verwende `ItemMove` (503)
- Item-Splitting → verwende `ItemSplit` (504)
- Equipment → verwende `EquipItem` (3901)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot wo Item hinzugefügt wurde | Ja |
| BagId | byte | Bag-ID (-1=Backpack, 0-3=Bags) | Ja |
| ItemId | uint | Item-ID | Ja |
| Quantity | int | Hinzugefügte Anzahl | Ja |
| NewStackSize | int | Neue Stack-Größe im Slot | Ja |
| Durability | int | Haltbarkeit (0-100) | Ja |
| IsSoulbound | bool | Soulbound? | Ja |
| Source | string | "loot", "craft", "trade", "quest", "vendor" | Ja |

### Erwartete Response
- Keine Response erforderlich (ist Notification)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ItemRemove` | 502 | Gegenteil (Item entfernen) |
| `LootItem` | 3101 | Loot-Quelle |
| `CraftItem` | 1601 | Crafting-Quelle |
| `InventoryFullNotification` | 511 | Falls kein Platz |

### Beispiel Payload
```csharp
// Neues Item (Loot)
var itemAdd = new ItemAdd
{
    Type = MessageType.ItemAdd,
    SlotIndex = 5,
    BagId = -1, // Backpack
    ItemId = 2001, // Health Potion
    Quantity = 3, // 3 Potions gelooted
    NewStackSize = 8, // War 5, jetzt 8
    Durability = 100,
    IsSoulbound = false,
    Source = "loot"
};

// Soulbound Quest-Reward
var questReward = new ItemAdd
{
    Type = MessageType.ItemAdd,
    SlotIndex = 10,
    BagId = 0, // Bag 1
    ItemId = 1234, // Epic Sword
    Quantity = 1,
    NewStackSize = 1,
    Durability = 100,
    IsSoulbound = true, // Quest-Reward = Soulbound
    Source = "quest"
};
```

### Error Codes
Keine - Falls kein Platz, wird `InventoryFullNotification` (511) gesendet

### Notizen
- **Auto-Stacking**: Server versucht immer erst zu stacken, dann neuen Slot
- **Slot-Allocation**: Server wählt ersten freien Slot (Backpack → Bags)
- **Animation**: Client zeigt Item-Add Animation (Items fliegen ins Inventory)
- **Sound**: Client spielt Loot-Sound
- **UI-Notification**: "Received: [Item] x3"
- **Max-Stack**: Item-abhängig (z.B. Potions: 20, Reagents: 200)

---

## ItemRemove (502)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ Sehr häufig (Item-Use, Selling, Crafting)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Item wurde aus dem Inventory entfernt. Dies kann durch Item-Use (Potions), Verkauf, Crafting (Materials), Trading, oder Deletion geschehen.

### Im Scope ✅
- Item-Consumption (Potion-Use, Crafting)
- Item-Verkauf an Vendor
- Item-Trading
- Item-Deletion
- Partial Stack-Removal

### Nicht im Scope ❌
- Equipment → verwende `UnequipItem` (3902)
- Item-Drop (auf Boden) → verwende `ItemDrop` (3105)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | byte | Bag-ID | Ja |
| Quantity | int | Entfernte Anzahl | Ja |
| RemainingStack | int | Verbleibende Stack-Größe (0=Slot leer) | Ja |
| Reason | string | "used", "sold", "crafted", "traded", "deleted" | Ja |

### Erwartete Response
- Keine Response erforderlich

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ItemAdd` | 501 | Gegenteil |
| `ItemUse` | 505 | Use-Request |
| `VendorSell` | 1312 | Verkauf |

### Beispiel Payload
```csharp
// Potion verwendet
var itemRemove = new ItemRemove
{
    Type = MessageType.ItemRemove,
    SlotIndex = 5,
    BagId = -1,
    Quantity = 1, // 1 Potion verwendet
    RemainingStack = 7, // 7 Potions übrig
    Reason = "used"
};

// Stack komplett verkauft
var soldStack = new ItemRemove
{
    Type = MessageType.ItemRemove,
    SlotIndex = 10,
    BagId = 0,
    Quantity = 15,
    RemainingStack = 0, // Slot jetzt leer
    Reason = "sold"
};
```

### Error Codes
Keine - Server sendet nur bei erfolgreicher Removal

### Notizen
- **Slot-Cleanup**: Client leert Slot wenn RemainingStack = 0
- **UI-Update**: Client aktualisiert Slot sofort
- **No Undo**: Item-Removal ist permanent (außer Vendor-Buyback)

---

## ItemMove (503)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig (Drag&Drop)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client möchte Item zwischen Slots verschieben (Drag&Drop). Server validiert ob Move erlaubt ist (Item nicht locked, Slots existieren) und führt Move durch.

### Im Scope ✅
- Drag&Drop zwischen Slots
- Slot-Swap (zwei Items tauschen)
- Auto-Stacking wenn Target-Slot gleichen Item-Type hat
- Cross-Bag Movement

### Nicht im Scope ❌
- Stack-Splitting → verwende `ItemSplit` (504)
- Equipment → verwende `EquipItem` (3901)
- Item-Deletion → verwende `ItemDelete` (506)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FromSlot | byte | Source-Slot | Ja |
| FromBagId | byte | Source-Bag-ID | Ja |
| ToSlot | byte | Target-Slot | Ja |
| ToBagId | byte | Target-Bag-ID | Ja |
| AutoStack | bool | Auto-Merge wenn Target gleicher Type | Ja |

### Erwartete Response
- `ItemMoveResponse` (520)

### Folge-Messages bei Erfolg
- `ItemRemove` (502) + `ItemAdd` (501) für Position-Update

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ItemSplit` | 504 | Für Stack-Splitting |
| `ItemAdd` | 501 | Server sendet bei erfolgreicher Move |
| `ItemRemove` | 502 | Server sendet bei erfolgreicher Move |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │  (Drag Item from Slot 5  │
  │   to Slot 10)            │
  │                          │
  │  ItemMove (503)          │
  │  FromSlot=5, ToSlot=10   │
  │─────────────────────────►│
  │                          │  ┌─ Validate: Slot exists?
  │                          │  ├─ Validate: Item locked?
  │                          │  ├─ Validate: Can merge?
  │                          │  └─ Execute Move
  │                          │
  │  ItemRemove (502)        │
  │  Slot=5                  │
  │◄─────────────────────────│
  │                          │
  │  ItemAdd (501)           │
  │  Slot=10                 │
  │◄─────────────────────────│
```

### Beispiel Payload
```csharp
// Einfacher Move
var itemMove = new ItemMove
{
    Type = MessageType.ItemMove,
    FromSlot = 5,
    FromBagId = -1, // Backpack
    ToSlot = 10,
    ToBagId = 0, // Bag 1
    AutoStack = true
};

// Swap (zwei Items tauschen)
var itemSwap = new ItemMove
{
    Type = MessageType.ItemMove,
    FromSlot = 3,
    FromBagId = -1,
    ToSlot = 8,
    ToBagId = -1,
    AutoStack = false // Kein Stack, nur Swap
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ITEM_LOCKED` | Item ist gelockt | Unlock Item erst |
| `INVALID_SLOT` | Slot existiert nicht | Validen Slot wählen |
| `CANNOT_STACK` | Items nicht stackbar | Anderen Slot wählen |
| `BAG_NOT_EQUIPPED` | Bag nicht equipped | Bag equippen |

### Notizen
- **Client-Side Prediction**: Client kann Move sofort darstellen, muss aber auf Server-Confirm warten
- **Rollback**: Bei Error rollback Client-Side Prediction
- **Auto-Stack**: Falls ToSlot gleichen Item-Type hat und AutoStack=true → Stacks werden gemerged
- **Performance**: Rate-Limited auf 10 Moves/Sekunde (Anti-Spam)

---

## ItemSplit (504)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig (Stack-Management)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client möchte einen Stack in zwei Stacks aufteilen. Wird verwendet um z.B. 20 Potions in 2x10 zu splitten.

### Im Scope ✅
- Stack-Splitting
- Partial Stack-Movement
- Target-Slot kann leer oder gleicher Item-Type sein

### Nicht im Scope ❌
- Komplettes Item-Movement → verwende `ItemMove` (503)
- Stack-Merging → automatisch via `ItemMove` mit AutoStack=true

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FromSlot | byte | Source-Slot (mit Stack) | Ja |
| FromBagId | byte | Source-Bag-ID | Ja |
| ToSlot | byte | Target-Slot (leer oder gleicher Type) | Ja |
| ToBagId | byte | Target-Bag-ID | Ja |
| Quantity | int | Zu bewegende Anzahl | Ja |

### Erwartete Response
- `ItemSplitResponse` (521)

### Folge-Messages bei Erfolg
- `ItemRemove` (502) für Source-Stack
- `ItemAdd` (501) für neuen Split-Stack

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ItemMove` | 503 | Für komplettes Item-Movement |
| `ItemStack` | 507 | Für Auto-Stacking |

### Beispiel Payload
```csharp
var itemSplit = new ItemSplit
{
    Type = MessageType.ItemSplit,
    FromSlot = 5,
    FromBagId = -1, // Backpack
    ToSlot = 15,
    ToBagId = 0, // Bag 1
    Quantity = 10 // 10 von 20 Potions verschieben
};
// Result: FromSlot hat 10, ToSlot hat 10
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_QUANTITY` | Nicht genug Items im Stack | Kleinere Quantity |
| `SLOT_OCCUPIED` | Target-Slot besetzt (anderer Type) | Anderen Slot wählen |
| `ITEM_NOT_STACKABLE` | Item ist nicht stackbar | - |
| `INVALID_QUANTITY` | Quantity < 1 oder > Stack-Größe | Valide Quantity |

### Notizen
- **UI**: Meist via Shift+Drag oder Rechtsklick-Menu
- **Min Quantity**: 1
- **Max Quantity**: Current Stack-Größe - 1

---

## ItemUse (505)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig (Combat, Healing)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client möchte Item verwenden (Potion, Food, Scroll, Quest-Item). Server validiert ob Item usable ist, führt Effect aus und removed Item (falls Consumable).

### Im Scope ✅
- Consumable-Items (Potions, Food)
- Usable-Items (Scrolls, Quest-Items)
- Cooldown-Management
- Target-Selection (für z.B. Buff-Potions auf andere Spieler)

### Nicht im Scope ❌
- Equipment → verwende `EquipItem` (3901)
- Item-Deletion → verwende `ItemDelete` (506)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Item-Slot | Ja |
| BagId | byte | Bag-ID | Ja |
| TargetId | int | Target-Entity (0=self) | Nein |

### Erwartete Response
- `ItemUseResponse` (522)

### Folge-Messages bei Erfolg
- `ItemRemove` (502) falls Item consumed
- Effect-Message (z.B. `HealEvent` 304)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ItemRemove` | 502 | Item wird entfernt nach Use |
| `HealEvent` | 304 | Bei Healing-Potion |
| `BuffApplied` | 1500 | Bei Buff-Potion |
| `CooldownStart` | 3200 | Cooldown startet |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │  (Player clicks Potion)  │
  │                          │
  │  ItemUse (505)           │
  │  SlotIndex=5             │
  │─────────────────────────►│
  │                          │  ┌─ Validate: Item exists?
  │                          │  ├─ Validate: On Cooldown?
  │                          │  ├─ Validate: In Combat?
  │                          │  ├─ Execute Effect
  │                          │  └─ Remove Item
  │                          │
  │  HealEvent (304)         │
  │◄─────────────────────────│
  │                          │
  │  ItemRemove (502)        │
  │◄─────────────────────────│
  │                          │
  │  CooldownStart (3200)    │
  │◄─────────────────────────│
```

### Beispiel Payload
```csharp
// Health Potion auf Self
var usePotion = new ItemUse
{
    Type = MessageType.ItemUse,
    SlotIndex = 5,
    BagId = -1,
    TargetId = 0 // Self
};

// Buff-Potion auf Party-Member
var buffPotion = new ItemUse
{
    Type = MessageType.ItemUse,
    SlotIndex = 8,
    BagId = 0,
    TargetId = 9876 // Party-Member
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ITEM_ON_COOLDOWN` | Item ist auf Cooldown | Warten |
| `CANNOT_USE_IN_COMBAT` | Item kann nicht in Combat benutzt werden | Combat verlassen |
| `INVALID_TARGET` | Target ungültig oder out of range | Anderen Target wählen |
| `ITEM_NOT_USABLE` | Item ist nicht usable | - |
| `INSUFFICIENT_LEVEL` | Level zu niedrig | Leveln |

### Notizen
- **Cooldowns**: Potions: 1min, Food: 30s (Out-of-Combat only)
- **Combat-Restrictions**: Food kann nicht in Combat benutzt werden
- **Shared Cooldowns**: Potions teilen Cooldown-Category
- **Animation**: Client zeigt Use-Animation

---

## ItemDelete (506)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client löscht Item permanent. Verwendet für Greyitems oder ungewollte Items. Server validiert ob Item deletable ist (nicht locked, nicht soulbound-wichtig).

### Im Scope ✅
- Permanentes Löschen von Items
- Bestätigung bei wertvollen Items (Server-seitig)
- Lock-Check

### Nicht im Scope ❌
- Vendor-Verkauf → verwende `VendorSell` (1312)
- Item-Drop → verwende `ItemDrop` (3105)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | byte | Bag-ID | Ja |
| Quantity | int | Zu löschende Anzahl (0=alle) | Ja |
| Confirmation | bool | Bestätigung (für wichtige Items) | Ja |

### Erwartete Response
- `ItemDeleteResponse` (523)

### Folge-Messages bei Erfolg
- `ItemRemove` (502) für gelöschtes Item

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ItemRemove` | 502 | Item wird entfernt |
| `ItemLock` | 509 | Item locken verhindert Deletion |

### Beispiel Payload
```csharp
var itemDelete = new ItemDelete
{
    Type = MessageType.ItemDelete,
    SlotIndex = 10,
    BagId = 0,
    Quantity = 0, // Alle
    Confirmation = true
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ITEM_LOCKED` | Item ist gelockt | Unlock erst |
| `CONFIRMATION_REQUIRED` | Bestätigung fehlt für wichtiges Item | Confirmation=true setzen |
| `ITEM_NOT_FOUND` | Item existiert nicht | - |

### Notizen
- **Confirmation**: Für Items > Uncommon Quality oder Soulbound
- **No Undo**: Deletion ist permanent
- **Alternative**: Besser Items an Vendor verkaufen

---

## ItemStack (507)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Client fordert Auto-Stacking aller Items an. Server merget alle stackbaren Items.

### Im Scope ✅
- Auto-Merge aller Stacks vom gleichen Type
- Inventory-Optimization

### Nicht im Scope ❌
- Item-Sorting → verwende `ItemSort` (508)

### Request Payload
Keine zusätzlichen Felder (nur MessageType)

### Erwartete Response
- `ItemStackResponse` (524)

### Folge-Messages bei Erfolg
- Mehrere `ItemRemove` (502) + `ItemAdd` (501) für Stack-Merge

### Beispiel Payload
```csharp
var itemStack = new ItemStack
{
    Type = MessageType.ItemStack
};
```

### Notizen
- **Phase 2**: Nicht im Prototyp
- **Performance**: Kann bis zu 2 Sekunden dauern bei vollem Inventory

---

## ItemSort (508)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Client fordert Auto-Sort des Inventory an. Server sortiert nach Typ, Qualität, oder Name.

### Im Scope ✅
- Sort nach Type, Quality, Name
- Bag-spezifisch oder gesamtes Inventory

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SortMode | string | "type", "quality", "name" | Ja |
| BagId | byte | Bag-ID (-1=alle) | Ja |

### Erwartete Response
- `ItemSortResponse` (525)

### Folge-Messages bei Erfolg
- Mehrere `ItemMove` Notifications für sortierte Items

### Beispiel Payload
```csharp
var itemSort = new ItemSort
{
    Type = MessageType.ItemSort,
    SortMode = "quality",
    BagId = -1 // Alle Bags
};
```

### Notizen
- **Phase 2**: Nicht im Prototyp

---

## ItemLock (509)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client locked/unlocked Item. Gelocked Items können nicht versehentlich gelöscht, verkauft oder getraded werden.

### Im Scope ✅
- Lock/Unlock von Items
- Protection gegen versehentliche Deletion

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | byte | Bag-ID | Ja |
| Locked | bool | true=lock, false=unlock | Ja |

### Erwartete Response
- `ItemLockResponse` (526)

### Folge-Messages bei Erfolg
- `ItemLockChanged` Event

### Beispiel Payload
```csharp
var itemLock = new ItemLock
{
    Type = MessageType.ItemLock,
    SlotIndex = 5,
    BagId = -1,
    Locked = true // Lock Item
};
```

### Notizen
- **UI**: Locked Items zeigen Lock-Icon
- **Protection**: Verhindert Delete, Sell, Trade

---

## BagExpand (510)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (nur via Purchase)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client erweitert Backpack-Größe oder equipped größeren Bag. Backpack kann von 16 auf 24 Slots erweitert werden (via Gold-Purchase).

### Im Scope ✅
- Backpack-Expansion (16 → 20 → 24)
- Bag-Equipment (8, 12, 16, 20 Slot Bags)
- Cost-Validation

### Nicht im Scope ❌
- Bag-Crafting → verwende `CraftItem` (1601)
- Bag-Purchase → verwende `VendorBuy` (1311)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetType | string | "backpack" oder "bag" | Ja |
| BagSlot | byte | Bag-Slot (0-3, nur bei type=bag) | Nein |
| NewSize | byte | Neue Größe (nur bei type=backpack) | Nein |

### Erwartete Response
- `BagExpandResponse` (527)

### Folge-Messages bei Erfolg
- `GoldUpdate` (3703) mit neuem Gold-Betrag

### Beispiel Payload
```csharp
// Backpack erweitern
var expandBackpack = new BagExpand
{
    Type = MessageType.BagExpand,
    TargetType = "backpack",
    NewSize = 20 // 16 → 20
};

// Bag equippen
var equipBag = new BagExpand
{
    Type = MessageType.BagExpand,
    TargetType = "bag",
    BagSlot = 0,
    NewSize = 12 // 12-Slot Bag
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold | Gold farmen |
| `MAX_SIZE_REACHED` | Backpack bereits max Size | - |
| `BAG_NOT_IN_INVENTORY` | Bag nicht im Inventory | Bag erst erwerben |

### Notizen
- **Costs**: 16→20: 10 Gold, 20→24: 100 Gold
- **Max Size**: Backpack: 24, Bags: 20
- **Phase 2**: Bag-System vollständig implementiert

---

## InventoryFullNotification (511)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (bei Loot wenn voll)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert dass Inventory voll ist und Item nicht hinzugefügt werden konnte.

### Im Scope ✅
- Notification dass Inventory voll
- Item-Info das nicht geadded wurde
- Empfehlung: Platz machen

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item das nicht geadded wurde | Ja |
| Quantity | int | Nicht geaddete Quantity | Ja |
| Reason | string | "inventory_full", "bag_full" | Ja |

### Beispiel Payload
```csharp
var inventoryFull = new InventoryFullNotification
{
    Type = MessageType.InventoryFullNotification,
    ItemId = 2001, // Health Potion
    Quantity = 5,
    Reason = "inventory_full"
};
```

### Notizen
- **UI**: Client zeigt Warning "Inventory Full!"
- **Loot**: Item bleibt im Loot-Window oder auf Boden
- **Mail**: Phase 2 - Items werden per Mail gesendet

---

## ItemMoveResponse (520)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ItemMove Request. Bestätigt erfolgreiche Item-Bewegung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Move erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SLOT_OCCUPIED` | Ziel-Slot bereits belegt |
| `ITEM_LOCKED` | Item ist gelockt |
| `INVALID_SLOT` | Ungültiger Slot |

---

## ItemSplitResponse (521)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ItemSplit Request. Bestätigt erfolgreichen Stack-Split oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Split erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `INVALID_AMOUNT` | Amount > Stack-Size oder < 1 |
| `NOT_STACKABLE` | Item ist nicht stackable |
| `SLOT_OCCUPIED` | Ziel-Slot belegt |

---

## ItemUseResponse (522)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ItemUse Request. Bestätigt erfolgreiche Item-Nutzung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Use erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| ItemId | uint | Verwendetes Item | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `ON_COOLDOWN` | Item auf Cooldown |
| `INSUFFICIENT_LEVEL` | Level zu niedrig |
| `IN_COMBAT` | Nicht im Kampf nutzbar |
| `WRONG_CLASS` | Falsche Klasse |

---

## ItemDeleteResponse (523)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ItemDelete Request. Bestätigt erfolgreiche Item-Löschung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Delete erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `ITEM_LOCKED` | Item ist gelockt |
| `ITEM_NOT_FOUND` | Item nicht gefunden |

---

## ItemStackResponse (524)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ItemStack Request. Bestätigt erfolgreiches Stack-Merge oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Stack erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_STACKABLE` | Items nicht stackable |
| `DIFFERENT_ITEMS` | Unterschiedliche Items |

---

## ItemSortResponse (525)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ItemSort Request. Bestätigt erfolgreiche Sortierung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Sort erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

---

## ItemLockResponse (526)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ItemLock Request. Bestätigt erfolgreiche Lock-Änderung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Lock-Änderung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| IsLocked | bool | Neuer Lock-Status | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `ITEM_NOT_FOUND` | Item nicht gefunden |

---

## BagExpandResponse (527)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf BagExpand Request. Bestätigt erfolgreiche Bag-Erweiterung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Expansion erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| BagSlot | int | Erweiterte Bag | Bei Erfolg |
| NewSlotCount | int | Neue Slot-Anzahl | Bei Erfolg |
| GoldCost | int | Kosten in Gold | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold |
| `MAX_SLOTS_REACHED` | Maximum bereits erreicht |
| `INVALID_BAG` | Ungültiger Bag-Slot |

---

## 🔗 Verwandte Kategorien

- **Equipment (39)**: Equipped Items → `EquipItem` (3901), `UnequipItem` (3902)
- **Bank (40)**: Bank-Storage → `BankSync` (4000), `BankDeposit` (4001)
- **Loot (31)**: Item-Loot → `LootRequest` (3100), `LootItem` (3101)
- **Trading (11)**: Item-Trading → `TradeOffer` (1100), `TradeAccept` (1102)
- **Crafting (16)**: Crafting-Materials → `CraftItem` (1601)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.1.0  
**Status**: ✅ Vollständig dokumentiert (20/20 Messages)

[← Zurück zur Übersicht](README.md)
