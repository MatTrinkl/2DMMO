# ⚔️ Equipment / Gear Messages (3900-3999)

**Kategorie:** 39  
**Range:** 3900-3999  



[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [EquipItem (3900)](#equipitem-3900)
- [UnequipItem (3902)](#unequipitem-3902)
- [EquipmentSync (3904)](#equipmentsync-3904)
- [DurabilityUpdate (3910)](#durabilityupdate-3910)
- [DurabilityWarning (3911)](#durabilitywarning-3911)
- [GemSocket (3920)](#gemsocket-3920)
- [EnchantApply (3930)](#enchantapply-3930)
- [ReforgeConfirm (3942)](#reforgeconfirm-3942)
- [SetBonusUpdate (3950)](#setbonusupdate-3950)
- [WeaponSwapRequest (3960)](#weaponswaprequest-3960)
- [OutfitSave (3970)](#outfitsave-3970)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Equipment- und Gear-Management** im 2DMMO.

Das Equipment-System implementiert:
- Equipment-Slots (Head, Chest, Legs, Weapon, etc.)
- Durability-System (Wear & Repair)
- Gem-Socketing
- Enchantments
- Reforging (Stat-Rerolling)
- Set-Bonuses
- Weapon-Swapping
- Outfit-System (Saved Equipment-Sets)

**Server Authority**: Alle Equipment-Changes sind server-authoritative.

---

## EquipItem (3900)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Equipt Item aus Inventory.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InventorySlot | byte | Source-Slot | Ja |
| EquipSlot | byte | Target Equipment-Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** `EquipItemResult` (3901)
- **Bei Fehler:** `EquipItemResult` (3901) mit ErrorCode

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `WRONG_SLOT` | Item passt nicht in Slot | Richtigen Slot wählen |
| `LEVEL_TOO_LOW` | Level-Requirement nicht erfüllt | Leveln |
| `CLASS_RESTRICTION` | Falsche Klasse | Anderes Item |

### Notizen
- **Auto-Equip**: Double-Click equipt automatisch
- **Swap**: Wenn Slot belegt, wird altes Item in Inventory getauscht

---

## UnequipItem (3902)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Unequipt Item (zurück ins Inventory).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EquipSlot | byte | Equipment-Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** Item in Inventory
- **Bei Fehler:** `UnequipItemResult` (3903)

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INVENTORY_FULL` | Kein Platz | Platz schaffen |

---

## EquipmentSync (3904)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Sync komplettes Equipment nach Login.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EquipSlots | List<EquipSlotInfo> | Alle Slots | Ja |

**EquipSlotInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SlotIndex | byte | Slot (0=Head, 1=Neck, etc.) |
| ItemId | uint | Equipped Item (0=empty) |
| Durability | int | Current Durability |
| MaxDurability | int | Max Durability |

---

## DurabilityUpdate (3910)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Durability eines Items hat sich geändert.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EquipSlot | byte | Equipment-Slot | Ja |
| NewDurability | int | Neue Durability | Ja |
| MaxDurability | int | Max Durability | Ja |

### Notizen
- **Loss**: Durability sinkt bei Tod, Skills, Blocks
- **Repair**: Bei NPC oder mit Repair-Kit
- **Yellow**: Bei <50% Durability Icon wird gelb

---

## DurabilityWarning (3911)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Warning dass Durability niedrig ist.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EquipSlot | byte | Slot mit low Durability | Ja |
| DurabilityPercent | float | Prozent (0.0-1.0) | Ja |

### Notizen
- **Threshold**: Warning bei <10%
- **UI**: Rotes Blink-Icon

---

## GemSocket (3920)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Sockt Gem in Item.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EquipSlot | byte | Equipment-Slot | Ja |
| SocketIndex | byte | Socket-Index (0-2) | Ja |
| GemItemId | uint | Gem-Item-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** `GemSocketResult` (3921)
- **Bei Fehler:** `GemSocketResult` (3921)

### Notizen
- **Sockets**: Items haben 0-3 Sockets
- **Permanent**: Gems sind permanent (außer Extract)
- **Bonus**: Matching Socket-Colors geben Bonus

---

## EnchantApply (3930)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Applied Enchant auf Item.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EquipSlot | byte | Equipment-Slot | Ja |
| EnchantId | uint | Enchant-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** `EnchantApplyResult` (3931)
- **Bei Fehler:** `EnchantApplyResult` (3931)

### Notizen
- **Overwrite**: Neuer Enchant ersetzt alten
- **Slots**: Weapon, Gloves, Boots, Chest können enchanted werden
- **Cost**: Enchant-Materials + Gold

---

## ReforgeConfirm (3942)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigt Reforge (Stat-Rerolling).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EquipSlot | byte | Equipment-Slot | Ja |
| FromStatType | string | Zu reduzierender Stat | Ja |
| ToStatType | string | Zu erhöhender Stat | Ja |

### Notizen
- **Use-Case**: Unwanted Stat → Wanted Stat
- **Percentage**: 40% des Stats wird konvertiert
- **Cost**: Gold-Cost

---

## SetBonusUpdate (3950)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Set-Bonus Status-Update.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SetId | uint | Item-Set-ID | Ja |
| PiecesEquipped | int | Equipped Pieces | Ja |
| ActiveBonuses | List<uint> | Aktive Bonus-IDs | Ja |

### Notizen
- **Tiers**: 2-Set, 4-Set, 6-Set Bonuses
- **Examples**: "+5% Crit", "Procs extra damage"

---

## WeaponSwapRequest (3960)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Swappt Waffen (Main-Hand ↔ Off-Hand).

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Bei Erfolg:** `WeaponSwapResult` (3961)

### Notizen
- **Hotkey**: Standard ~-Key
- **GCD**: Löst kurzen GCD aus (1s)
- **Use-Case**: Ranged ↔ Melee Swap

---

## OutfitSave (3970)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Speichert aktuelles Equipment als Outfit.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| OutfitName | string | Outfit-Name | Ja |
| OutfitSlot | byte | Slot (0-9, max 10 Outfits) | Ja |

### Notizen
- **Quick-Change**: Schnelles Equipment-Swapping
- **Use-Case**: Tank ↔ DPS Sets

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
