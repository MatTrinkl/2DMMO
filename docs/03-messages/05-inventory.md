# 🎒 Inventory Messages (0500-0599)

**Kategorie:** 5  
**Range:** 0500-0599  
**Phase:** Phase 2  
**Status:** ✅ Vollständig dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Inventory-System** im 2DMMO.

Das Inventory-System implementiert:
- Item-Management (Pickup, Drop, Use, Destroy)
- Inventory-Operationen (Move, Swap, Split, Merge)
- Item-States (Lock, Cooldown, Durability)
- Item-Enhancement (Enchant, Socket, Upgrade, Transmog, Salvage)
- Bag-Management (Sort, Expand)
- Item-Tooltips und Links
- Item-Identification

**Server Authority**: Server validiert alle Inventory-Operationen. Client zeigt optimistic Updates, wartet aber auf Server-Bestätigung.

---

## InventoryUpdate (500)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Vollständige Inventory-Synchronisation. Wird gesendet beim Login, Zone-Transfer oder nach größeren Inventory-Änderungen. Enthält alle Items in allen Bags mit vollständigen Daten.

Diese Message ist der "Full State" des Inventars. Sie wird verwendet um Client und Server zu synchronisieren, besonders nach Reconnects.

### Im Scope ✅
- Komplette Inventory-Daten
- Alle Bags mit allen Slots
- Item-IDs, Stacks, Durability
- Bank-Tabs (falls geöffnet)

### Nicht im Scope ❌
- Single Slot Updates → siehe `InventorySlotUpdate` (501)
- Item-Tooltips → siehe `ItemTooltipRequest` (533)
- Equipment Slots → siehe `Equipment` Messages (3900-3999)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Bags | BagData[] | Array aller Bags | Ja |
| Currency | CurrencyData | Geld/Tokens | Ja |

**BagData:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| BagSlot | int | Bag-Index (0-4) |
| Size | int | Anzahl Slots |
| Items | ItemData[] | Items in Bag |

**ItemData:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Slot | int | Slot-Index |
| ItemId | uint | Item-Template-ID |
| StackSize | int | Stack-Größe |
| Durability | int | Haltbarkeit |
| Enchants | uint[] | Enchantment-IDs |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `InventorySlotUpdate` | 501 | Incremental Updates |
| `BagExpand` | 532 | Bag-Size erhöhen |

### Beispiel Payload
```csharp
var invUpdate = new InventoryUpdate
{
    Type = MessageType.InventoryUpdate,
    Bags = new[]
    {
        new BagData
        {
            BagSlot = 0, // Main Backpack
            Size = 16,
            Items = new[]
            {
                new ItemData { Slot = 0, ItemId = 1234, StackSize = 20, Durability = 100 },
                new ItemData { Slot = 1, ItemId = 5678, StackSize = 1, Durability = 85 }
            }
        }
    },
    Currency = new CurrencyData { Gold = 1000, Silver = 50 }
};
```

### Notizen
- **Full Sync**: Wird bei Login und Reconnect gesendet
- **Size**: Kann groß werden (~5KB für volles Inventory)
- **Compression**: MessagePack komprimiert gut
- **Update Frequency**: Selten (nur bei Major Changes)

---

## InventorySlotUpdate (501)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Incrementeller Update eines einzelnen Inventory-Slots. Wird verwendet für optimistic Updates nach Operationen.

### Im Scope ✅
- Single Slot Change
- Stack-Size Änderungen
- Durability Updates
- Item Added/Removed

### Nicht im Scope ❌
- Full Inventory Sync → siehe `InventoryUpdate` (500)
- Equipment Changes → siehe `Equipment` Messages (3900-3999)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Bag-Index | Ja |
| Slot | int | Slot-Index in Bag | Ja |
| Item | ItemData | Item-Data (null = empty) | Nein |

### Beispiel Payload
```csharp
var slotUpdate = new InventorySlotUpdate
{
    Type = MessageType.InventorySlotUpdate,
    BagSlot = 0,
    Slot = 5,
    Item = new ItemData { ItemId = 1234, StackSize = 15 }
};
```

### Notizen
- **Performance**: Viel kleiner als Full Update (~100 bytes vs 5KB)
- **Frequency**: Nach jeder Inventory-Operation

---

## ItemPickup (502)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** �� Ja

### Beschreibung

Client möchte Item von Ground/Loot-Window aufheben.

### Im Scope ✅
- Item vom Boden aufheben
- Item aus Loot-Window nehmen
- Auto-Looting
- Stack-Merging

### Nicht im Scope ❌
- Trading → siehe `Trading` Messages (1100-1199)
- Quest-Item-Rewards → siehe `Quest` Messages (1000-1099)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Ground-Item oder Loot-Container | Ja |
| ItemId | uint | Item-ID zum Aufheben | Ja |
| Amount | int | Anzahl (für Stacks) | Ja |

### Erwartete Response
- **Bei Erfolg:** `InventorySlotUpdate` (501)
- **Bei Fehler:** `ErrorMessage` (910) mit Code `INVENTORY_FULL`

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INVENTORY_FULL` | Kein Platz im Inventory | Platz schaffen |
| `TOO_FAR_AWAY` | Item außer Reichweite | Näher herangehen |
| `ITEM_ALREADY_LOOTED` | Anderer Spieler war schneller | N/A |
| `NOT_YOUR_LOOT` | Item gehört anderem Spieler | Warten |

### Notizen
- **Range Check**: 5m Radius
- **Loot Rights**: Personal Loot vs Group Loot
- **Auto-Loot**: Client kann Auto-Loot aktivieren

---

## ItemPickupFailed (503)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item-Pickup ist fehlgeschlagen (mit Reason).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item das nicht aufgehoben wurde | Ja |
| Reason | enum | Failure-Reason | Ja |

### Notizen
- **Reasons**: INVENTORY_FULL, TOO_FAR, NOT_YOUR_LOOT, ALREADY_LOOTED

---

## ItemDrop (504)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Spieler droppt Item auf den Boden.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Bag-Index | Ja |
| Slot | int | Slot-Index | Ja |
| Amount | int | Anzahl (für Stacks) | Ja |

### Erwartete Response
- **Bei Erfolg:** `InventorySlotUpdate` (501) + Ground-Item-Spawn

### Notizen
- **Soulbound Check**: Soulbound Items können nicht gedroppt werden
- **Combat Restriction**: Im Combat nicht möglich
- **Quest Items**: Quest-Items können nicht gedroppt werden

---

## ItemUse (505)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Spieler verwendet Item (Potion, Food, Quest-Item, Consumable).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Bag-Index | Ja |
| Slot | int | Slot-Index | Ja |
| TargetId | int | Target (für targetable Items) | Nein |

### Erwartete Response
- **Bei Erfolg:** `ItemUseResult` (506)
- **Bei Fehler:** `ErrorMessage` (910)

### Notizen
- **Consumables**: Stack-Size wird reduziert
- **Cooldowns**: Server prüft Item-Cooldown
- **Combat**: Manche Items nur außerhalb Combat verwendbar

---

## ItemUseResult (506)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item wurde erfolgreich verwendet. Enthält Effekt-Information.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Verwendetes Item | Ja |
| Success | bool | Ob erfolgreich | Ja |
| Effect | string | Effekt-Beschreibung | Nein |
| CooldownMs | uint | Item-Cooldown | Nein |

### Notizen
- **Effects**: HP/Mana-Restore, Buff-Application, Quest-Progress
- **Stack Update**: `InventorySlotUpdate` folgt separat

---

## ItemDestroy (507)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Spieler zerstört Item permanent.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Bag-Index | Ja |
| Slot | int | Slot-Index | Ja |
| Amount | int | Anzahl (für Stacks) | Ja |

### Erwartete Response
- **Bei Erfolg:** `InventorySlotUpdate` (501)

### Notizen
- **Confirmation**: Client sollte Confirmation-Dialog zeigen
- **Irreversible**: Keine Restore-Möglichkeit
- **Value Items**: Warnung bei wertvollen Items

---

## ItemSplit (508)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Stack wird aufgeteilt (z.B. 20 → 10 + 10).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceBag | int | Source Bag | Ja |
| SourceSlot | int | Source Slot | Ja |
| DestBag | int | Destination Bag | Ja |
| DestSlot | int | Destination Slot | Ja |
| Amount | int | Anzahl zu verschieben | Ja |

### Erwartete Response
- **Bei Erfolg:** 2x `InventorySlotUpdate` (501)

### Notizen
- **Empty Slot**: Destination muss leer sein
- **Stack Limit**: Amount <= SourceStack

---

## ItemMerge (509)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Zwei Stacks des gleichen Items werden zusammengefügt.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceBag | int | Source Bag | Ja |
| SourceSlot | int | Source Slot | Ja |
| DestBag | int | Destination Bag | Ja |
| DestSlot | int | Destination Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** 2x `InventorySlotUpdate` (501)

### Notizen
- **Same ItemId**: Nur gleiche Items können merged werden
- **Stack Limit**: Max Stack-Size beachten (z.B. 200)

---

## ItemMove (510)

**Richtung:** 📤 Client → Server  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item wird von einem Slot zu einem anderen verschoben.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceBag | int | Source Bag | Ja |
| SourceSlot | int | Source Slot | Ja |
| DestBag | int | Destination Bag | Ja |
| DestSlot | int | Destination Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** 2x `InventorySlotUpdate` (501)

### Notizen
- **Empty Dest**: Destination muss leer sein (sonst siehe `ItemSwap`)
- **Validation**: Server prüft Bag-Existence und Slot-Validity

---

## ItemSwap (511)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Zwei Items werden vertauscht (Swap-Operation).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Bag1 | int | Bag 1 | Ja |
| Slot1 | int | Slot 1 | Ja |
| Bag2 | int | Bag 2 | Ja |
| Slot2 | int | Slot 2 | Ja |

### Erwartete Response
- **Bei Erfolg:** 2x `InventorySlotUpdate` (501)

### Notizen
- **Atomic Operation**: Beide Items werden gleichzeitig vertauscht

---

## ItemLock (512)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item wird gelockt (kann nicht versehentlich verkauft/zerstört werden).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Bag-Index | Ja |
| Slot | int | Slot-Index | Ja |
| Locked | bool | true = lock, false = unlock | Ja |

### Erwartete Response
- **Bei Erfolg:** `InventorySlotUpdate` (501)

### Notizen
- **Protection**: Verhindert versehentliches Löschen/Verkaufen
- **Visual**: Client zeigt Lock-Icon

---

## ItemUnlock (513)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item wird entsperrt. Siehe `ItemLock` (512).

---

## ItemCooldownStart (514)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item-Cooldown wurde gestartet (nach Item-Use).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item mit Cooldown | Ja |
| CooldownMs | uint | Cooldown in ms | Ja |
| StartTime | long | Server-Timestamp | Ja |

### Notizen
- **Shared Cooldowns**: Manche Items teilen Cooldowns (z.B. Potions)
- **Global Cooldown**: 1.5s GCD für Consumables

---

## ItemCooldownEnd (515)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item-Cooldown ist abgelaufen.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item | Ja |

---

## ItemDurabilityChange (516)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Durability eines Items hat sich geändert (durch Combat, Death).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Bag-Index | Ja |
| Slot | int | Slot-Index | Ja |
| NewDurability | int | Neue Haltbarkeit (0-100) | Ja |

### Notizen
- **Death Penalty**: -10% Durability bei Tod
- **Combat**: -0.1% pro Hit
- **Broken**: Bei 0% Durability → Item unbrauchbar

---

## ItemRepair (517)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Einzelnes Item reparieren (bei NPC).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Bag-Index | Ja |
| Slot | int | Slot-Index | Ja |

### Erwartete Response
- **Bei Erfolg:** `InventorySlotUpdate` (501) + Currency-Update
- **Bei Fehler:** `INSUFFICIENT_FUNDS`

### Notizen
- **Cost**: Abhängig von Item-Level und Durability-Loss
- **NPC**: Nur bei Repair-NPCs möglich

---

## ItemRepairAll (518)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Alle Items reparieren (bei NPC).

### Request Payload
Keine Parameter

### Erwartete Response
- **Bei Erfolg:** Multiple `InventorySlotUpdate` (501) + Currency-Update
- **Bei Fehler:** `INSUFFICIENT_FUNDS`

### Notizen
- **Batch Operation**: Repariert alle Equipment + Inventory Items
- **Cost**: Summe aller Einzelkosten

---

## ItemEnchant (519)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item mit Enchantment versehen.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetBag | int | Item Bag | Ja |
| TargetSlot | int | Item Slot | Ja |
| EnchantId | uint | Enchantment-ID | Ja |
| MaterialBag | int | Material Bag | Nein |
| MaterialSlot | int | Material Slot | Nein |

### Erwartete Response
- **Bei Erfolg:** `ItemEnchantResult` (520)
- **Bei Fehler:** `ENCHANT_FAILED`

---

## ItemEnchantResult (520)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Enchantment-Result (Success/Failure).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Ob erfolgreich | Ja |
| ItemId | uint | Enchantetes Item | Ja |
| EnchantId | uint | Applied Enchantment | Nein |

### Notizen
- **Success Rate**: Abhängig von Enchantment-Level
- **Failure**: Item wird nicht zerstört (nur Material verloren)

---

## ItemSocket (521)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Gem in Item-Socket einfügen.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemBag | int | Item Bag | Ja |
| ItemSlot | int | Item Slot | Ja |
| GemBag | int | Gem Bag | Ja |
| GemSlot | int | Gem Slot | Ja |
| SocketIndex | int | Socket-Index (0-2) | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemSocketResult` (522)

---

## ItemSocketResult (522)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Socketing-Result.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Ob erfolgreich | Ja |
| ItemId | uint | Item | Ja |
| GemId | uint | Eingefügte Gem | Ja |
| SocketIndex | int | Socket-Index | Ja |

---

## ItemUpgrade (523)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item upgraden (Item-Level erhöhen).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemBag | int | Item Bag | Ja |
| ItemSlot | int | Item Slot | Ja |
| MaterialBag | int | Material Bag | Ja |
| MaterialSlot | int | Material Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemUpgradeResult` (524)

---

## ItemUpgradeResult (524)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Upgrade-Result.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Ob erfolgreich | Ja |
| ItemId | uint | Item | Ja |
| NewItemLevel | int | Neues Item-Level | Ja |

---

## ItemTransmog (525)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item-Appearance ändern (Transmogrification).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetBag | int | Item Bag | Ja |
| TargetSlot | int | Item Slot | Ja |
| TransmogItemId | uint | Appearance-Item-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemTransmogResult` (526)

---

## ItemTransmogResult (526)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Transmog-Result.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Ob erfolgreich | Ja |
| ItemId | uint | Item | Ja |
| TransmogAppearance | uint | Neue Appearance-ID | Ja |

---

## ItemSalvage (527)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Item zerlegen → Materials erhalten.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemBag | int | Item Bag | Ja |
| ItemSlot | int | Item Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemSalvageResult` (528)

---

## ItemSalvageResult (528)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Salvage-Result mit erhaltenen Materials.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Ob erfolgreich | Ja |
| Materials | MaterialReward[] | Erhaltene Materials | Ja |

**MaterialReward:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ItemId | uint | Material-ID |
| Amount | int | Anzahl |

---

## ItemIdentify (529)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Unidentified Item identifizieren.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemBag | int | Item Bag | Ja |
| ItemSlot | int | Item Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemIdentifyResult` (530)

---

## ItemIdentifyResult (530)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Identify-Result mit enthüllten Stats.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Ob erfolgreich | Ja |
| ItemId | uint | Identifiziertes Item | Ja |
| RevealedStats | ItemStat[] | Enthüllte Stats | Ja |

---

## BagSort (531)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Bag automatisch sortieren.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Zu sortierender Bag | Ja |
| SortMode | enum | Quality/Type/Name | Ja |

### Erwartete Response
- **Bei Erfolg:** Multiple `InventorySlotUpdate` (501)

---

## BagExpand (532)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Bag-Size erweitern (durch Purchase).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagSlot | int | Zu erweiternder Bag | Ja |

### Erwartete Response
- **Bei Erfolg:** `InventoryUpdate` (500)
- **Bei Fehler:** `INSUFFICIENT_FUNDS`

### Notizen
- **Cost**: Steigt mit jeder Erweiterung (z.B. 10g, 100g, 1000g)
- **Max Size**: 32 Slots pro Bag

---

## ItemTooltipRequest (533)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Client fragt vollständige Item-Tooltip-Data ab.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** `ItemTooltipResponse` (534)

---

## ItemTooltipResponse (534)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Vollständige Item-Tooltip-Information.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item-ID | Ja |
| Name | string | Item-Name | Ja |
| Quality | enum | Common/Uncommon/Rare/Epic/Legendary | Ja |
| ItemLevel | int | Item-Level | Ja |
| Stats | ItemStat[] | Stats | Ja |
| Description | string | Flavor-Text | Nein |

---

## ItemLink (535)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Spieler hat Item-Link in Chat gepostet.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Player der Link postete | Ja |
| ItemId | uint | Verlinktes Item | Ja |
| Channel | enum | Chat-Channel | Ja |

---

## 🔗 Verwandte Kategorien

- **Equipment (39)**: Equipment-Slots und Gear → `EquipItem` (3900)
- **Loot (31)**: Loot-Generation → `LootGenerated` (3100)
- **Trading (11)**: Item-Trade zwischen Spielern → `TradeRequest` (1100)
- **Auction (17)**: Auction-House → `AuctionCreate` (1700)
- **Crafting (16)**: Item-Herstellung → `CraftItem` (1600)

---

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 2.0.0  
**Status**: ✅ Vollständig dokumentiert (36/36 Messages)

[← Zurück zur Übersicht](README.md)
