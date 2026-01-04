# 🎒 Inventory / Items Messages (0500-0543)

**Kategorie:** 5  
**Range:** 0500-0543 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

### 🔄 Flow-Diagramme
- [Inventory-Flow (Übersicht)](#-inventory-flow-übersicht)
- [Item-Movement-Flow](#item-movement-flow)
- [Item-Use-Flow](#item-use-flow)
- [Enchant/Socket-Flow](#enchantsocket-flow)

### 🧱 DTOs & Enums
- [InventoryItemDto](#inventoryitemdto)
- [BagInfoDto](#baginfodto)
- [ItemSlotType Enum](#itemslottype-enum)
- [ItemQuality Enum](#itemquality-enum)

### 📩 Messages (0500-0543) — Reihenfolge laut MessageType.cs
- [InventoryUpdate (500)](#inventoryupdate-500)
- [InventorySlotUpdate (501)](#inventoryslotupdate-501)
- [ItemPickup (502)](#itempickup-502)
- [ItemPickupFailed (503)](#itempickupfailed-503)
- [ItemDrop (504)](#itemdrop-504)
- [ItemUse (505)](#itemuse-505)
- [ItemUseResult (506)](#itemuseresult-506)
- [ItemDestroy (507)](#itemdestroy-507)
- [ItemSplit (508)](#itemsplit-508)
- [ItemMerge (509)](#itemmerge-509)
- [ItemMove (510)](#itemmove-510)
- [ItemSwap (511)](#itemswap-511)
- [ItemLock (512)](#itemlock-512)
- [ItemUnlock (513)](#itemunlock-513)
- [ItemCooldownStart (514)](#itemcooldownstart-514)
- [ItemCooldownEnd (515)](#itemcooldownend-515)
- [ItemDurabilityChange (516)](#itemdurabilitychange-516)
- [ItemRepair (517)](#itemrepair-517)
- [ItemRepairAll (518)](#itemrepairall-518)
- [ItemEnchant (519)](#itemenchant-519)
- [ItemEnchantResult (520)](#itemenchantresult-520)
- [ItemSocket (521)](#itemsocket-521)
- [ItemSocketResult (522)](#itemsocketresult-522)
- [ItemUpgrade (523)](#itemupgrade-523)
- [ItemUpgradeResult (524)](#itemupgraderesult-524)
- [ItemTransmog (525)](#itemtransmog-525)
- [ItemTransmogResult (526)](#itemtransmogresult-526)
- [ItemSalvage (527)](#itemsalvage-527)
- [ItemSalvageResult (528)](#itemsalvageresult-528)
- [ItemIdentify (529)](#itemidentify-529)
- [ItemIdentifyResult (530)](#itemidentifyresult-530)
- [BagSort (531)](#bagsort-531)
- [BagExpand (532)](#bagexpand-532)
- [ItemTooltipRequest (533)](#itemtooltiprequest-533)
- [ItemTooltipResponse (534)](#itemtooltipresponse-534)
- [ItemLink (535)](#itemlink-535)
- [ItemMoveResponse (536)](#itemmoveresponse-536)
- [ItemSplitResponse (537)](#itemsplitresponse-537)
- [ItemUseResponse (538)](#itemuseresponse-538)
- [ItemDeleteResponse (539)](#itemdeleteresponse-539)
- [ItemStackResponse (540)](#itemstackresponse-540)
- [ItemSortResponse (541)](#itemsortresponse-541)
- [ItemLockResponse (542)](#itemlockresponse-542)
- [BagExpandResponse (543)](#bagexpandresponse-543)

### 🗑️ Obsolete Messages
- Keine

### 📎 Anhang
- [MessageType Enum (Inventory-Bereich)](#messagetype-enum-inventory-bereich)
- [Request/Response Paare](#requestresponse-paare)
- [Datei-Struktur](#datei-struktur)

---

## 🔄 Inventory-Flow (Übersicht)

### Grundprinzip: Server-Authoritative Inventory

\`\`\`
┌─────────────────────────────────────────────────────────────────┐
│                    INVENTORY SYSTEM ARCHITEKTUR                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Client (UI)              Gateway              Zone Server        │
│     │                        │                      │             │
│     │  User Action           │                      │             │
│     │  (Drag & Drop)         │                      │             │
│     │─────────────────────────────────────────────►│             │
│     │                        │                      │             │
│     │                        │              ┌───────┴───────┐    │
│     │                        │              │ VALIDATION     │    │
│     │                        │              │ - Slot exists? │    │
│     │                        │              │ - Item locked? │    │
│     │                        │              │ - Permission?  │    │
│     │                        │              └───────┬───────┘    │
│     │                        │                      │             │
│     │  Response              │                      │             │
│     │◄─────────────────────────────────────────────│             │
│     │                        │                      │             │
│     │  (UI Update)           │                      │             │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

WICHTIG: Client kann NIEMALS Inventory-State direkt ändern!
         Alle Änderungen müssen vom Server bestätigt werden.
\`\`\`

### Item-Movement-Flow

\`\`\`
Client                    Server                    
  │                          │                      
  │  (User drags item        │                      
  │   from Slot A to B)      │                      
  │                          │                      
  │  ItemMove (510)          │                      
  │  FromSlot=A, ToSlot=B    │                      
  │─────────────────────────►│                      
  │                          │  ┌─ Validate slots   
  │                          │  ├─ Check locked     
  │                          │  ├─ Check stackable  
  │                          │  └─ Execute move     
  │                          │                      
  │  ItemMoveResponse (536)  │                      
  │  Success=true            │                      
  │◄─────────────────────────│                      
  │                          │                      
  │  (Client updates UI)     │                      
\`\`\`

### Item-Use-Flow

\`\`\`
Client                    Server                    
  │                          │                      
  │  ItemUse (505)           │                      
  │  SlotIndex, TargetId     │                      
  │─────────────────────────►│                      
  │                          │  ┌─ Validate usable  
  │                          │  ├─ Check cooldown   
  │                          │  ├─ Check target     
  │                          │  ├─ Execute effect   
  │                          │  └─ Consume item     
  │                          │                      
  │  ItemUseResult (506)     │                      
  │◄─────────────────────────│                      
  │                          │                      
  │  [Effect Messages]       │                      
  │  (HealEvent, BuffApply)  │                      
  │◄─────────────────────────│                      
\`\`\`

### Enchant/Socket-Flow

\`\`\`
Client                    Server                    
  │                          │                      
  │  ItemEnchant (519)       │                      
  │  ItemSlot, EnchantId     │                      
  │─────────────────────────►│                      
  │                          │  ┌─ Validate item    
  │                          │  ├─ Check materials  
  │                          │  ├─ Check gold       
  │                          │  ├─ Roll success     
  │                          │  └─ Apply enchant    
  │                          │                      
  │  ItemEnchantResult (520) │                      
  │  Success/Fail, NewStats  │                      
  │◄─────────────────────────│                      
\`\`\`

---

## 🧱 DTOs / Enums / Interfaces

### InventoryItemDto

\`\`\`csharp
[MessagePackObject]
public class InventoryItemDto
{
    [Key(0)] public MessageType Type => MessageType.InventorySlotUpdate;
    [Key(1)] public byte SlotIndex { get; set; }
    [Key(2)] public sbyte BagId { get; set; }
    [Key(3)] public uint ItemId { get; set; }
    [Key(4)] public int Quantity { get; set; }
    [Key(5)] public int Durability { get; set; }
    [Key(6)] public bool IsLocked { get; set; }
    [Key(7)] public bool IsSoulbound { get; set; }
    [Key(8)] public List<uint> Enchantments { get; set; }
    [Key(9)] public List<uint> Sockets { get; set; }
    [Key(10)] public uint TransmogId { get; set; }
}
\`\`\`

### BagInfoDto

\`\`\`csharp
[MessagePackObject]
public class BagInfoDto
{
    [Key(0)] public byte BagSlot { get; set; }
    [Key(1)] public uint BagItemId { get; set; }
    [Key(2)] public byte Slots { get; set; }
}
\`\`\`

### ItemSlotType Enum

\`\`\`csharp
public enum ItemSlotType : byte
{
    Backpack = 0,
    Bag1 = 1,
    Bag2 = 2,
    Bag3 = 3,
    Bag4 = 4,
    Bank = 10,
    GuildBank = 20
}
\`\`\`

### ItemQuality Enum

\`\`\`csharp
public enum ItemQuality : byte
{
    Poor = 0,
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Epic = 4,
    Legendary = 5,
    Artifact = 6
}
\`\`\`

---

## 📩 Aktive Messages (0500-0543)

---

## InventoryUpdate (500)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nur bei Login/Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert das komplette Inventory des Spielers nach Login oder Reconnect.

### Im Scope ✅
- Vollständige Inventory-Daten (alle Bags)
- Item-Properties (Durability, Enchantments, Soulbound-Status)
- Bag-Configuration

### Nicht im Scope ❌
- Bank-Items → verwende \`BankSync\` (4000)
- Equipment-Items → verwende \`EquipmentSync\` (3900)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BackpackSlots | byte | Anzahl Backpack-Slots | Ja |
| EquippedBags | List<BagInfoDto> | Equipped Bags | Ja |
| Items | List<InventoryItemDto> | Alle Items | Ja |
| Gold | long | Gold in Copper | Ja |

### Erwartete Response
- Keine (ist initiale Sync)

---

## InventorySlotUpdate (501)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ Sehr häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Aktualisiert einen einzelnen Inventory-Slot.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot-Index | Ja |
| BagId | sbyte | Bag-ID | Ja |
| Item | InventoryItemDto | Item-Daten (null=leer) | Nein |

---

## ItemPickup (502)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client versucht Item aus Welt aufzuheben.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootId | uint | Loot-Container ID | Ja |
| ItemIndex | byte | Index im Container | Ja |

### Erwartete Response
- \`InventorySlotUpdate\` (501) oder \`ItemPickupFailed\` (503)

---

## ItemPickupFailed (503)

**Richtung:** 📥 Server → Client  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server informiert dass ItemPickup fehlgeschlagen ist.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootId | uint | Loot-Container ID | Ja |
| ErrorCode | string | Fehlercode | Ja |

---

## ItemDrop (504)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client droppt Item auf Boden.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |
| Quantity | int | Anzahl (0=alle) | Ja |

### Erwartete Response
- \`InventorySlotUpdate\` (501)

---

## ItemUse (505)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client verwendet Item (Potion, Scroll, Quest-Item).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Item-Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |
| TargetId | int | Target (0=self) | Nein |

### Erwartete Response
- \`ItemUseResult\` (506)

---

## ItemUseResult (506)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf ItemUse.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| ItemId | uint | Verwendetes Item | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemDestroy (507)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client löscht Item permanent.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |
| Confirmation | bool | Bestätigung | Ja |

### Erwartete Response
- \`ItemDeleteResponse\` (539)

---

## ItemSplit (508)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client teilt Stack.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FromSlot | byte | Source-Slot | Ja |
| FromBagId | sbyte | Source-Bag | Ja |
| ToSlot | byte | Target-Slot | Ja |
| ToBagId | sbyte | Target-Bag | Ja |
| Quantity | int | Menge | Ja |

### Erwartete Response
- \`ItemSplitResponse\` (537)

---

## ItemMerge (509)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client merged zwei Stacks.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FromSlot | byte | Source-Slot | Ja |
| FromBagId | sbyte | Source-Bag | Ja |
| ToSlot | byte | Target-Slot | Ja |
| ToBagId | sbyte | Target-Bag | Ja |

### Erwartete Response
- \`ItemStackResponse\` (540)

---

## ItemMove (510)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client verschiebt Item zwischen Slots.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FromSlot | byte | Source-Slot | Ja |
| FromBagId | sbyte | Source-Bag | Ja |
| ToSlot | byte | Target-Slot | Ja |
| ToBagId | sbyte | Target-Bag | Ja |

### Erwartete Response
- \`ItemMoveResponse\` (536)

---

## ItemSwap (511)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client tauscht zwei Items.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotA | byte | Erster Slot | Ja |
| BagIdA | sbyte | Erste Bag | Ja |
| SlotB | byte | Zweiter Slot | Ja |
| BagIdB | sbyte | Zweite Bag | Ja |

### Erwartete Response
- \`ItemMoveResponse\` (536)

---

## ItemLock (512)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client lockt Item.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |

### Erwartete Response
- \`ItemLockResponse\` (542)

---

## ItemUnlock (513)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client entsperrt Item.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |

### Erwartete Response
- \`ItemLockResponse\` (542)

---

## ItemCooldownStart (514)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server informiert über Cooldown-Start.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item-ID | Ja |
| CooldownCategory | uint | Cooldown-Kategorie | Ja |
| Duration | float | Dauer in Sekunden | Ja |
| StartTime | long | Server-Timestamp | Ja |

---

## ItemCooldownEnd (515)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server informiert über Cooldown-Ende.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item-ID | Ja |
| CooldownCategory | uint | Cooldown-Kategorie | Ja |

---

## ItemDurabilityChange (516)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server informiert über Durability-Änderung.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |
| NewDurability | int | Neue Durability | Ja |

---

## ItemRepair (517)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client repariert einzelnes Item.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |
| VendorId | uint | Vendor-NPC ID | Ja |

### Erwartete Response
- \`ItemDurabilityChange\` (516)

---

## ItemRepairAll (518)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client repariert alle Items.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| VendorId | uint | Vendor-NPC ID | Ja |

### Erwartete Response
- Multiple \`ItemDurabilityChange\` (516)

---

## ItemEnchant (519)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client enchanted Item.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Item-Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |
| EnchantId | uint | Enchant-Definition | Ja |

### Erwartete Response
- \`ItemEnchantResult\` (520)

---

## ItemEnchantResult (520)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf Enchant.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| NewEnchantments | List<uint> | Aktuelle Enchants | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemSocket (521)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client sockelt Gem.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemSlot | byte | Item-Slot | Ja |
| ItemBagId | sbyte | Item Bag-ID | Ja |
| SocketIndex | byte | Socket-Position | Ja |
| GemSlot | byte | Gem-Slot | Ja |
| GemBagId | sbyte | Gem Bag-ID | Ja |

### Erwartete Response
- \`ItemSocketResult\` (522)

---

## ItemSocketResult (522)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf Socket.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| NewSockets | List<uint> | Aktuelle Gems | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemUpgrade (523)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client upgraded Item-Level.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |

### Erwartete Response
- \`ItemUpgradeResult\` (524)

---

## ItemUpgradeResult (524)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf Upgrade.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| NewItemLevel | int | Neues Level | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemTransmog (525)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client ändert Item-Appearance.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |
| TransmogId | uint | Appearance-ID | Ja |

### Erwartete Response
- \`ItemTransmogResult\` (526)

---

## ItemTransmogResult (526)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf Transmog.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| NewTransmogId | uint | Neue Appearance | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemSalvage (527)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client salvaged Item für Materials.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |

### Erwartete Response
- \`ItemSalvageResult\` (528)

---

## ItemSalvageResult (528)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf Salvage.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| Materials | List<(uint,int)> | Erhaltene Materials | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemIdentify (529)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client identifiziert unbekanntes Item.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |

### Erwartete Response
- \`ItemIdentifyResult\` (530)

---

## ItemIdentifyResult (530)

**Richtung:** 📥 Server → Client  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server enthüllt Item-Properties.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| RevealedItem | InventoryItemDto | Item-Details | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## BagSort (531)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client sortiert Bag-Inhalt.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagId | sbyte | Bag-ID (-1=alle) | Ja |
| SortMode | string | "type", "quality", "name" | Ja |

### Erwartete Response
- \`ItemSortResponse\` (541)

---

## BagExpand (532)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client erweitert Bag-Größe.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagId | sbyte | Bag-ID | Ja |
| NewSize | byte | Neue Slot-Anzahl | Ja |

### Erwartete Response
- \`BagExpandResponse\` (543)

---

## ItemTooltipRequest (533)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client fordert Tooltip-Daten an.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item-Definition | Ja |
| InstanceId | uint | Item-Instance | Nein |

### Erwartete Response
- \`ItemTooltipResponse\` (534)

---

## ItemTooltipResponse (534)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server sendet Tooltip-Daten.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item-ID | Ja |
| Name | string | Item-Name | Ja |
| Quality | ItemQuality | Qualität | Ja |
| ItemLevel | int | Item-Level | Ja |
| Description | string | Beschreibung | Ja |
| Stats | Dictionary<string,int> | Stat-Boni | Ja |

---

## ItemLink (535)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Client sendet Item-Link im Chat.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| BagId | sbyte | Bag-ID | Ja |
| Channel | ChatChannelType | Chat-Kanal | Ja |

### Erwartete Response
- \`ChatBroadcast\` (401)

---

## ItemMoveResponse (536)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf ItemMove/ItemSwap.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemSplitResponse (537)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf ItemSplit.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemUseResponse (538)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf ItemUse.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| ItemId | uint | Item-ID | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemDeleteResponse (539)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf ItemDestroy.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemStackResponse (540)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf ItemMerge.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemSortResponse (541)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf BagSort.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## ItemLockResponse (542)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf ItemLock/ItemUnlock.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| IsLocked | bool | Neuer Lock-Status | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## BagExpandResponse (543)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  

### Beschreibung
Server antwortet auf BagExpand.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| BagId | sbyte | Erweiterte Bag | Bei Erfolg |
| NewSlotCount | byte | Neue Slot-Anzahl | Bei Erfolg |
| GoldCost | int | Kosten in Gold | Bei Erfolg |
| ErrorCode | string | Fehlercode | Bei Fehler |

---

## 🗑️ Obsolete Messages

Keine obsoleten Messages in dieser Kategorie.

---

## 📎 Anhang

### MessageType Enum (Inventory-Bereich)

\`\`\`csharp
// INVENTORY / ITEMS (0500-0599)
InventoryUpdate = 500,
InventorySlotUpdate = 501,
ItemPickup = 502,
ItemPickupFailed = 503,
ItemDrop = 504,
ItemUse = 505,
ItemUseResult = 506,
ItemDestroy = 507,
ItemSplit = 508,
ItemMerge = 509,
ItemMove = 510,
ItemSwap = 511,
ItemLock = 512,
ItemUnlock = 513,
ItemCooldownStart = 514,
ItemCooldownEnd = 515,
ItemDurabilityChange = 516,
ItemRepair = 517,
ItemRepairAll = 518,
ItemEnchant = 519,
ItemEnchantResult = 520,
ItemSocket = 521,
ItemSocketResult = 522,
ItemUpgrade = 523,
ItemUpgradeResult = 524,
ItemTransmog = 525,
ItemTransmogResult = 526,
ItemSalvage = 527,
ItemSalvageResult = 528,
ItemIdentify = 529,
ItemIdentifyResult = 530,
BagSort = 531,
BagExpand = 532,
ItemTooltipRequest = 533,
ItemTooltipResponse = 534,
ItemLink = 535,
ItemMoveResponse = 536,
ItemSplitResponse = 537,
ItemUseResponse = 538,
ItemDeleteResponse = 539,
ItemStackResponse = 540,
ItemSortResponse = 541,
ItemLockResponse = 542,
BagExpandResponse = 543,
\`\`\`

### Request/Response Paare

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| ItemPickup | 502 | InventorySlotUpdate/ItemPickupFailed | 501/503 |
| ItemDrop | 504 | InventorySlotUpdate | 501 |
| ItemUse | 505 | ItemUseResult | 506 |
| ItemDestroy | 507 | ItemDeleteResponse | 539 |
| ItemSplit | 508 | ItemSplitResponse | 537 |
| ItemMerge | 509 | ItemStackResponse | 540 |
| ItemMove | 510 | ItemMoveResponse | 536 |
| ItemSwap | 511 | ItemMoveResponse | 536 |
| ItemLock | 512 | ItemLockResponse | 542 |
| ItemUnlock | 513 | ItemLockResponse | 542 |
| ItemRepair | 517 | ItemDurabilityChange | 516 |
| ItemRepairAll | 518 | ItemDurabilityChange | 516 |
| ItemEnchant | 519 | ItemEnchantResult | 520 |
| ItemSocket | 521 | ItemSocketResult | 522 |
| ItemUpgrade | 523 | ItemUpgradeResult | 524 |
| ItemTransmog | 525 | ItemTransmogResult | 526 |
| ItemSalvage | 527 | ItemSalvageResult | 528 |
| ItemIdentify | 529 | ItemIdentifyResult | 530 |
| BagSort | 531 | ItemSortResponse | 541 |
| BagExpand | 532 | BagExpandResponse | 543 |
| ItemTooltipRequest | 533 | ItemTooltipResponse | 534 |
| ItemLink | 535 | ChatBroadcast | 401 |

### Datei-Struktur

\`\`\`
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs          # 500-543 Inventory
├── DTOs/
│   └── Inventory/
│       ├── InventoryUpdateDto.cs
│       ├── InventoryItemDto.cs
│       ├── BagInfoDto.cs
│       └── ...
└── Contracts/
    └── IInventoryMessage.cs
\`\`\`

---

## 🔗 Verwandte Kategorien

- **Equipment (39)**: Equipped Items
- **Bank (40)**: Bank-Storage
- **Loot (31)**: Item-Loot
- **Trading (11)**: Item-Trading
- **Crafting (16)**: Crafting-Materials

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Vollständig dokumentiert (44 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/05-inventory.md
