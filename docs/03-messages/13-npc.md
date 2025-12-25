# 🤖 NPC Messages (1300-1399)

**Kategorie:** 13  
**Range:** 1300-1399  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [NPCInteract (1300)](#npcinteract-1300)
- [NPCDialogOpen (1301)](#npcdiagopen-1301)
- [NPCDialogSelect (1302)](#npcdiagselect-1302)
- [NPCDialogClose (1303)](#npcdiagclose-1303)
- [VendorOpen (1310)](#vendoropen-1310)
- [VendorBuy (1311)](#vendorbuy-1311)
- [VendorSell (1312)](#vendorsell-1312)
- [VendorBuyback (1313)](#vendorbuyback-1313)
- [VendorClose (1314)](#vendorclose-1314)
- [TrainerOpen (1320)](#traineropen-1320)
- [TrainerLearn (1321)](#trainerlearn-1321)
- [RepairAll (1330)](#repairall-1330)
- [FlightMasterOpen (1340)](#flightmasteropen-1340)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **NPC-Interaction und Service-NPCs** im 2DMMO.

Das NPC-System implementiert:
- **Dialog-System**: Tree-based Dialogs mit Branching
- **Vendor-System**: Buy/Sell Items, Buyback-Feature
- **Trainer-System**: Learn Skills/Spells (Phase 2)
- **Repair-System**: Repair Equipment
- **Flight-Master**: Fast-Travel zwischen Points (Phase 2)
- **Quest-Givers**: Quest-Interaction → siehe Quest-Category (1000-1099)

**NPC-Types**:
- **Vendor**: Verkauft Items
- **Trainer**: Lehrt Skills/Spells
- **Quest-Giver**: Gibt Quests
- **Repair**: Repariert Equipment
- **Flight-Master**: Fast-Travel
- **Banker**: Bank-Access → siehe Bank-Category (4000-4099)

**Interaction-Range**: 5m

---

## NPCInteract (1300)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client interagiert mit NPC (Right-Click oder 'F'-Key). Server validiert Range und öffnet entsprechenden NPC-Service.

### Im Scope ✅
- Interact mit allen NPC-Types
- Range-Check (5m)
- Auto-Open entsprechendes UI (Dialog, Vendor, Trainer, etc.)

### Nicht im Scope ❌
- Entity-Interact allgemein → verwende `EntityInteract` (1410)
- Combat-NPCs → keine Interaction während Combat

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | NPC Entity-ID | Ja |

### Erwartete Response
- **Bei Dialog-NPC:** `NPCDialogOpen` (1301)
- **Bei Vendor:** `VendorOpen` (1310)
- **Bei Trainer:** `TrainerOpen` (1320)
- **Bei Repair:** Repair-UI-Open
- **Bei Flight-Master:** `FlightMasterOpen` (1340)
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `NPCDialogOpen` | 1301 | Für Dialog-NPCs |
| `VendorOpen` | 1310 | Für Vendors |
| `TrainerOpen` | 1320 | Für Trainer |

### Beispiel Payload
```csharp
var npcInteract = new NPCInteract
{
    Type = MessageType.NPCInteract,
    NPCId = 12345 // Vendor-NPC
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `OUT_OF_RANGE` | NPC zu weit weg (>5m) | Näher herangehen |
| `NPC_HOSTILE` | NPC ist feindlich | Combat |
| `IN_COMBAT` | Kann nicht interagieren während Combat | Combat beenden |
| `NPC_BUSY` | NPC wird bereits von anderem Spieler genutzt | Warten |

### Notizen
- **Range**: Max 5m
- **Facing**: Spieler dreht sich automatisch zum NPC
- **UI**: Server öffnet entsprechendes UI
- **Busy-State**: NPCs können nur von 1 Spieler gleichzeitig genutzt werden (Phase 2: Queue-System)

---

## NPCDialogOpen (1301)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server öffnet Dialog-UI mit NPC-Text und Optionen. Client zeigt Dialog-Window.

### Im Scope ✅
- Dialog-Text (mit Variablen wie `{PlayerName}`)
- Dialog-Optionen (Branching)
- Quest-Marker-Integration
- Goodbye-Option (schließt Dialog)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | NPC-ID | Ja |
| DialogText | string | Dialog-Text (max 500 Zeichen) | Ja |
| Options | List<DialogOption> | Dialog-Optionen | Ja |

**DialogOption:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| OptionId | uint | Option-ID (für Select) |
| OptionText | string | Angezeigter Text |
| Icon | string | Optional Icon ("vendor", "trainer", "quest", "gossip") |
| RequiresQuest | uint | Quest-ID falls benötigt |

### Erwartete Response
- Client sendet `NPCDialogSelect` (1302) oder `NPCDialogClose` (1303)

### Beispiel Payload
```csharp
var dialogOpen = new NPCDialogOpen
{
    Type = MessageType.NPCDialogOpen,
    NPCId = 12345,
    DialogText = "Greetings, {PlayerName}! Welcome to my shop. How can I help you today?",
    Options = new List<DialogOption>
    {
        new DialogOption 
        { 
            OptionId = 1, 
            OptionText = "I'd like to browse your goods.", 
            Icon = "vendor" 
        },
        new DialogOption 
        { 
            OptionId = 2, 
            OptionText = "Can you repair my equipment?", 
            Icon = "repair" 
        },
        new DialogOption 
        { 
            OptionId = 3, 
            OptionText = "Tell me about this area.", 
            Icon = "gossip" 
        },
        new DialogOption 
        { 
            OptionId = 99, 
            OptionText = "Goodbye.", 
            Icon = "" 
        }
    }
};
```

### Notizen
- **Variables**: `{PlayerName}`, `{ClassName}`, `{Level}` werden ersetzt
- **Quest-Integration**: Quest-Marker (!) wird neben Option angezeigt
- **Branching**: OptionId bestimmt nächsten Dialog-Node
- **Goodbye**: OptionId 99 schließt immer Dialog

---

## NPCDialogSelect (1302)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client wählt Dialog-Option. Server führt entsprechende Action aus (öffnet Vendor, gibt Quest, etc.).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | NPC-ID | Ja |
| OptionId | uint | Gewählte Option | Ja |

### Erwartete Response
- **Bei Vendor-Option:** `VendorOpen` (1310)
- **Bei Trainer-Option:** `TrainerOpen` (1320)
- **Bei Gossip-Option:** `NPCDialogOpen` (1301) mit nächstem Dialog-Node
- **Bei Goodbye:** Dialog schließt
- **Bei Fehler:** `ErrorMessage` (910)

### Beispiel Payload
```csharp
var dialogSelect = new NPCDialogSelect
{
    Type = MessageType.NPCDialogSelect,
    NPCId = 12345,
    OptionId = 1 // "Browse goods"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INVALID_OPTION` | Option-ID existiert nicht | - |
| `QUEST_REQUIREMENT_NOT_MET` | Quest nicht completed | Quest erst machen |
| `NPC_OUT_OF_RANGE` | NPC zu weit weg | Näher herangehen |

### Notizen
- **State-Tracking**: Server trackt Dialog-State pro Spieler
- **Timeout**: Dialog schließt nach 5min Inaktivität

---

## NPCDialogClose (1303)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client schließt Dialog (ESC-Key oder Close-Button).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | NPC-ID | Ja |

### Erwartete Response
- Server bestätigt Close

### Beispiel Payload
```csharp
var dialogClose = new NPCDialogClose
{
    Type = MessageType.NPCDialogClose,
    NPCId = 12345
};
```

### Notizen
- **Auto-Close**: Wird automatisch bei Wegbewegung (>10m) gesendet

---

## VendorOpen (1310)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server öffnet Vendor-UI mit verfügbaren Items. Client zeigt Vendor-Window.

### Im Scope ✅
- Item-Liste (mit Preisen)
- Limited-Stock Items
- Buyback-Tab
- Repair-Tab (bei Repair-Vendors)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Vendor-NPC-ID | Ja |
| Items | List<VendorItem> | Verkaufte Items | Ja |
| BuybackItems | List<BuybackItem> | Kürzlich verkaufte Items | Nein |
| CanRepair | bool | Vendor kann reparieren? | Ja |

**VendorItem:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ItemId | uint | Item-ID |
| Price | int | Preis in Copper |
| Stock | int | Verfügbarer Stock (-1=unlimited) |
| MaxStack | int | Max kaufbare Anzahl |

**BuybackItem:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ItemId | uint | Item-ID |
| Quantity | int | Anzahl |
| Price | int | Buyback-Preis (80% vom Verkaufspreis) |
| ExpiresAt | long | Unix Timestamp wann abläuft |

### Erwartete Response
- Client sendet `VendorBuy` (1311), `VendorSell` (1312), oder `VendorClose` (1314)

### Beispiel Payload
```csharp
var vendorOpen = new VendorOpen
{
    Type = MessageType.VendorOpen,
    NPCId = 12345,
    Items = new List<VendorItem>
    {
        new VendorItem 
        { 
            ItemId = 2001, // Health Potion
            Price = 500, // 5 Silver
            Stock = -1, // Unlimited
            MaxStack = 20
        },
        new VendorItem 
        { 
            ItemId = 1001, // Iron Sword
            Price = 10000, // 1 Gold
            Stock = 3, // Limited
            MaxStack = 1
        }
    },
    BuybackItems = new List<BuybackItem>
    {
        new BuybackItem 
        { 
            ItemId = 1234, 
            Quantity = 1, 
            Price = 8000, // 80c
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() 
        }
    },
    CanRepair = true
};
```

### Notizen
- **Stock**: Limited-Stock Items refreshen nach Server-Restart
- **Buyback**: Letzte 12 verkaufte Items, 30min Expiry
- **Reputation**: Phase 2 - Discounts basierend auf Reputation

---

## VendorBuy (1311)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client kauft Item vom Vendor. Server validiert Gold, Stock, Inventory-Space.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Vendor-ID | Ja |
| ItemId | uint | Item-ID | Ja |
| Quantity | int | Anzahl | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemAdd` (501) + `GoldUpdate` (3703)
- **Bei Fehler:** `ErrorMessage` (910)

### Beispiel Payload
```csharp
var vendorBuy = new VendorBuy
{
    Type = MessageType.VendorBuy,
    NPCId = 12345,
    ItemId = 2001, // Health Potion
    Quantity = 10
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold | Gold farmen |
| `ITEM_NOT_AVAILABLE` | Item nicht im Vendor-Inventory | - |
| `OUT_OF_STOCK` | Stock aufgebraucht | Später wiederkommen |
| `INVENTORY_FULL` | Inventory voll | Platz machen |
| `INVALID_QUANTITY` | Quantity ungültig | Korrekte Anzahl |

### Notizen
- **Transaction**: Atomic (Gold abgezogen + Item added)
- **Stack-Limit**: Kann max MaxStack auf einmal kaufen
- **Auto-Stack**: Neue Items werden automatisch gestackt

---

## VendorSell (1312)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client verkauft Item an Vendor. Item wandert in Buyback-Tab.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InventorySlot | byte | Zu verkaufendes Item-Slot | Ja |
| BagId | byte | Bag-ID | Ja |
| Quantity | int | Anzahl | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemRemove` (502) + `GoldUpdate` (3703)
- **Bei Fehler:** `ErrorMessage` (910)

### Beispiel Payload
```csharp
var vendorSell = new VendorSell
{
    Type = MessageType.VendorSell,
    InventorySlot = 5,
    BagId = -1, // Backpack
    Quantity = 10
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ITEM_NOT_FOUND` | Item nicht im Inventory | - |
| `ITEM_BOUND` | Soulbound-Item kann nicht verkauft werden | - |
| `ITEM_NOT_SELLABLE` | Quest-Item o.ä. | - |

### Notizen
- **Sell-Price**: 25% vom Vendor-Kaufpreis (standard)
- **Buyback**: Item erscheint in Buyback-Tab
- **Buyback-Price**: 80% vom Verkaufspreis (= 20% vom Vendor-Kaufpreis)
- **Buyback-Duration**: 30 Minuten

---

## VendorBuyback (1313)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client kauft versehentlich verkauftes Item zurück aus Buyback-Tab.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Vendor-ID | Ja |
| BuybackIndex | byte | Index in Buyback-Tab (0-11) | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemAdd` (501) + `GoldUpdate` (3703)
- **Bei Fehler:** `ErrorMessage` (910)

### Beispiel Payload
```csharp
var buyback = new VendorBuyback
{
    Type = MessageType.VendorBuyback,
    NPCId = 12345,
    BuybackIndex = 0 // Letztes verkauftes Item
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `BUYBACK_EXPIRED` | Item abgelaufen (>30min) | - |
| `BUYBACK_NOT_FOUND` | Index ungültig | - |
| `INSUFFICIENT_GOLD` | Nicht genug Gold | - |
| `INVENTORY_FULL` | Inventory voll | Platz machen |

### Notizen
- **Buyback-Price**: 80% vom Original-Verkaufspreis
- **Expiry**: 30 Minuten
- **Max-Entries**: 12 Items in Buyback-Tab

---

## VendorClose (1314)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client schließt Vendor-UI.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Vendor-ID | Ja |

### Beispiel Payload
```csharp
var vendorClose = new VendorClose
{
    Type = MessageType.VendorClose,
    NPCId = 12345
};
```

### Notizen
- **Auto-Close**: Bei >10m Entfernung

---

## TrainerOpen (1320)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Server öffnet Trainer-UI mit verfügbaren Skills/Spells.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Trainer-NPC-ID | Ja |
| Skills | List<TrainerSkill> | Lernbare Skills | Ja |

**TrainerSkill:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SkillId | uint | Skill-ID |
| SkillName | string | Skill-Name |
| Cost | int | Lernkosten in Copper |
| RequiredLevel | int | Benötigtes Level |
| IsLearned | bool | Bereits gelernt? |

### Beispiel Payload
```csharp
var trainerOpen = new TrainerOpen
{
    Type = MessageType.TrainerOpen,
    NPCId = 12345,
    Skills = new List<TrainerSkill>
    {
        new TrainerSkill 
        { 
            SkillId = 3001, 
            SkillName = "Fireball", 
            Cost = 10000, 
            RequiredLevel = 10, 
            IsLearned = false 
        },
        new TrainerSkill 
        { 
            SkillId = 3002, 
            SkillName = "Frostbolt", 
            Cost = 15000, 
            RequiredLevel = 12, 
            IsLearned = true 
        }
    }
};
```

### Notizen
- **Phase 2**: Nicht im Prototyp
- **Class-Specific**: Trainer nur für bestimmte Klassen

---

## TrainerLearn (1321)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Client lernt Skill vom Trainer.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Trainer-ID | Ja |
| SkillId | uint | Skill-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** `SkillLearned` Event + `GoldUpdate` (3703)
- **Bei Fehler:** `ErrorMessage` (910)

### Beispiel Payload
```csharp
var trainerLearn = new TrainerLearn
{
    Type = MessageType.TrainerLearn,
    NPCId = 12345,
    SkillId = 3001
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold | - |
| `INSUFFICIENT_LEVEL` | Level zu niedrig | Leveln |
| `SKILL_ALREADY_LEARNED` | Bereits gelernt | - |
| `WRONG_CLASS` | Skill für andere Klasse | - |

### Notizen
- **Phase 2**: Nicht im Prototyp

---

## RepairAll (1330)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client repariert alle equipped Items beim Repair-Vendor.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Repair-Vendor-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** `GoldUpdate` (3703) + Durability-Updates
- **Bei Fehler:** `ErrorMessage` (910)

### Beispiel Payload
```csharp
var repairAll = new RepairAll
{
    Type = MessageType.RepairAll,
    NPCId = 12345
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold | - |
| `NO_ITEMS_TO_REPAIR` | Alle Items bei 100% Durability | - |

### Notizen
- **Cost**: Basiert auf Item-Quality und Durability-Loss
- **Formula**: `RepairCost = ItemValue * (1 - Durability/100) * 0.1`

---

## FlightMasterOpen (1340)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Server öffnet Flight-Master-UI mit verfügbaren Flugzielen.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Flight-Master-ID | Ja |
| Destinations | List<FlightDestination> | Verfügbare Ziele | Ja |

**FlightDestination:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| DestinationId | uint | Ziel-ID |
| Name | string | Ziel-Name |
| Cost | int | Flugkosten |
| FlightTime | int | Flugzeit in Sekunden |
| IsDiscovered | bool | Bereits entdeckt? |

### Beispiel Payload
```csharp
var flightMaster = new FlightMasterOpen
{
    Type = MessageType.FlightMasterOpen,
    NPCId = 12345,
    Destinations = new List<FlightDestination>
    {
        new FlightDestination 
        { 
            DestinationId = 1, 
            Name = "Stormwind City", 
            Cost = 5000, 
            FlightTime = 120, 
            IsDiscovered = true 
        },
        new FlightDestination 
        { 
            DestinationId = 2, 
            Name = "Ironforge", 
            Cost = 10000, 
            FlightTime = 240, 
            IsDiscovered = false 
        }
    }
};
```

### Notizen
- **Phase 2**: Nicht im Prototyp
- **Discovery**: Muss Flight-Point erst entdecken (nahe kommen)

---

## 🔗 Verwandte Kategorien

- **Entity (14)**: Entity-Interaction → `EntityInteract` (1410)
- **Inventory (05)**: Item-Buy/Sell → `ItemAdd` (501), `ItemRemove` (502)
- **Economy (37)**: Gold-Transactions → `GoldUpdate` (3703)
- **Quest (10)**: Quest-NPCs → `QuestAccept` (1000), `QuestComplete` (1002)
- **Skills (17)**: Skill-Learning → `SkillLearned` Event (Phase 2)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0  
**Status**: ✅ Vollständig dokumentiert (13/13 Messages)

[← Zurück zur Übersicht](README.md)
