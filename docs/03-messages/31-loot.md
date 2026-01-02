# 💰 Loot / Rewards Messages (3100-3199)

**Kategorie:** 31  
**Range:** 3100-3199  



[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [LootWindowOpen (3100)](#lootwindowopen-3100)
- [LootWindowClose (3101)](#lootwindowclose-3101)
- [LootItem (3102)](#lootitem-3102)
- [LootItemResult (3103)](#lootitemresult-3103)
- [LootGold (3104)](#lootgold-3104)
- [LootAll (3105)](#lootall-3105)
- [LootRollStart (3110)](#lootrollstart-3110)
- [LootRollNeed (3111)](#lootrollneed-3111)
- [LootRollGreed (3112)](#lootrollgreed-3112)
- [LootRollPass (3113)](#lootrollpass-3113)
- [LootRollResult (3114)](#lootrollresult-3114)
- [LootRollWinner (3115)](#lootrollwinner-3115)
- [LootMasterAssign (3120)](#lootmasterassign-3120)
- [LootRulesChange (3121)](#lootruleschange-3121)
- [PersonalLoot (3130)](#personalloot-3130)
- [BonusRollPrompt (3131)](#bonusrollprompt-3131)
- [BonusRollUse (3132)](#bonusrolluse-3132)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Loot-System** im 2DMMO.

Das Loot-System implementiert:
- Loot-Window für tote NPCs/Chests
- Group-Loot mit Need/Greed/Pass Rolling
- Master-Looter System
- Personal-Loot (jeder eigener Loot)
- Bonus-Roll System
- Loot-Threshold (Auto-Pass für Low-Quality)
- Gold-Distribution in Gruppen

**Server Authority**: Alle Loot-Distribution ist server-seitig. Client sendet Loot-Requests, Server validiert und verteilt.

---

## LootWindowOpen (3100)

**Richtung:** �� Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet Loot-Window für lootbare Entity (toter NPC, Chest, Resource-Node).

### Im Scope ✅
- Loot-Window öffnen
- Liste aller lootbaren Items
- Gold-Amount
- Looting-Rules (Solo/Group)

### Nicht im Scope ❌
- Quest-Rewards → verwende `QuestRewardReceive` (1007)
- Mail-Attachments → verwende `MailTakeAttachment` (1806)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Entity-ID der Loot-Source | Ja |
| LootItems | List<LootItemInfo> | Lootbare Items | Ja |
| Gold | int | Gold (Copper) | Ja |
| LootMode | string | "solo", "group_roll", "group_master", "personal" | Ja |

**LootItemInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SlotIndex | byte | Loot-Slot (0-N) |
| ItemId | uint | Item-ID |
| Quantity | int | Anzahl |
| Quality | string | "poor", "common", "uncommon", "rare", "epic", "legendary" |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootWindowClose` | 3101 | Window schließen |
| `LootItem` | 3102 | Item looten |
| `LootGold` | 3104 | Gold looten |
| `LootAll` | 3105 | Alles looten |

### Beispiel Payload
```csharp
var lootOpen = new LootWindowOpen
{
    Type = MessageType.LootWindowOpen,
    LootSourceId = 60001,
    LootItems = new List<LootItemInfo>
    {
        new LootItemInfo
        {
            SlotIndex = 0,
            ItemId = 5001,
            Quantity = 1,
            Quality = "rare"
        },
        new LootItemInfo
        {
            SlotIndex = 1,
            ItemId = 5002,
            Quantity = 3,
            Quality = "common"
        }
    },
    Gold = 1500,
    LootMode = "solo"
};
```

### Notizen
- **Range**: Max 5m Range zur Loot-Source
- **Despawn**: Loot despawned nach 2 Minuten
- **Protected**: Loot ist 30s für Killer/Party protected

---

## LootWindowClose (3101)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Schließt Loot-Window.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |

### Notizen
- **Auto-Close**: Bei Movement >10m
- **Loot bleibt**: Unlootete Items bleiben für andere

---

## LootItem (3102)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lootet einzelnes Item aus Loot-Window.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |
| SlotIndex | byte | Item-Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** `LootItemResult` (3103) + Item in Inventory
- **Bei Fehler:** `LootItemResult` (3103) mit ErrorCode

### Beispiel Payload
```csharp
var lootItem = new LootItem
{
    Type = MessageType.LootItem,
    LootSourceId = 60001,
    SlotIndex = 0
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INVENTORY_FULL` | Kein Platz im Inventory | Platz schaffen |
| `ITEM_GONE` | Item bereits gelooted (Group) | Ignorieren |
| `TOO_FAR` | Zu weit von Source entfernt | Näher kommen |

### Notizen
- **Quick-Loot**: SHIFT+Click für schnelles Looten
- **Group**: Bei Group-Loot startet Roll

---

## LootItemResult (3103)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Result des Loot-Versuchs.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Item gelooted? | Ja |
| ItemId | uint | Item-ID | Ja |
| Quantity | int | Quantity | Bei Success |
| ErrorCode | string | Error | Bei Fehler |

---

## LootGold (3104)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lootet Gold aus Loot-Window.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |

### Erwartete Response
- **Immer:** Gold wird zum Inventory hinzugefügt

### Notizen
- **Group**: Gold wird automatisch auf Party verteilt
- **Share**: Jeder Party-Member erhält gleichen Anteil

---

## LootAll (3105)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lootet alle Items und Gold auf einmal.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |

### Erwartete Response
- **Bei Erfolg:** Alle lootbaren Items werden gelooted
- **Bei Inventory voll:** So viele wie möglich

### Notizen
- **Hotkey**: Standard Keybind: SHIFT+Click auf Source
- **Group**: Funktioniert nur bei Personal-Loot
- **Threshold**: Respektiert Loot-Threshold

---

## LootRollStart (3110)

**Richtung:** 📡 Broadcast (Server → Party)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet Loot-Roll für Item in Group-Loot-Mode.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |
| SlotIndex | byte | Item-Slot | Ja |
| ItemId | uint | Item-ID | Ja |
| Quality | string | Item-Quality | Ja |
| Timeout | int | Sekunden zum Rollen | Ja |

### Beispiel Payload
```csharp
var rollStart = new LootRollStart
{
    Type = MessageType.LootRollStart,
    LootSourceId = 60001,
    SlotIndex = 0,
    ItemId = 5001,
    Quality = "rare",
    Timeout = 30 // 30 Sekunden
};
```

### Notizen
- **Timeout**: 30 Sekunden Standard
- **Auto-Pass**: Bei Timeout auto-pass
- **UI**: Client zeigt Roll-Window

---

## LootRollNeed (3111)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler würfelt "Need" (höchste Priorität).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |
| SlotIndex | byte | Item-Slot | Ja |

### Erwartete Response
- **Bei Erfolg:** Broadcast an Party mit Roll-Result

### Notizen
- **Priority**: Need > Greed > Pass
- **Restriction**: Nur wenn Item für Class verwendbar
- **Roll**: 1-100 Random

---

## LootRollGreed (3112)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler würfelt "Greed" (mittlere Priorität).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |
| SlotIndex | byte | Item-Slot | Ja |

### Notizen
- **Use-Case**: Item für andere Klasse aber zum Verkaufen
- **Roll**: 1-100 Random

---

## LootRollPass (3113)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler passt (kein Interesse am Item).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |
| SlotIndex | byte | Item-Slot | Ja |

---

## LootRollResult (3114)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Zwischenergebnis: Spieler hat gewürfelt.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Spieler-ID | Ja |
| PlayerName | string | Spieler-Name | Ja |
| RollType | string | "need", "greed", "pass" | Ja |
| RollValue | int | Würfel-Ergebnis (1-100) | Bei need/greed |

---

## LootRollWinner (3115)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Finales Roll-Ergebnis: Gewinner erhält Item.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |
| SlotIndex | byte | Item-Slot | Ja |
| WinnerId | int | Gewinner-ID | Ja |
| WinnerName | string | Gewinner-Name | Ja |
| RollType | string | "need" oder "greed" | Ja |
| RollValue | int | Gewinn-Würfel | Ja |

### Beispiel Payload
```csharp
var rollWinner = new LootRollWinner
{
    Type = MessageType.LootRollWinner,
    LootSourceId = 60001,
    SlotIndex = 0,
    WinnerId = 50001,
    WinnerName = "Aragorn",
    RollType = "need",
    RollValue = 95
};
```

### Notizen
- **UI**: Client zeigt Winner-Notification
- **Sound**: Winner hört special Sound
- **Chat**: "Aragorn won [Item] (Need 95)"

---

## LootMasterAssign (3120)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Master-Looter assigned Item direkt an Party-Member.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |
| SlotIndex | byte | Item-Slot | Ja |
| RecipientId | int | Empfänger-ID | Ja |

### Notizen
- **Permission**: Nur Master-Looter
- **No Roll**: Kein Rolling bei Master-Loot
- **Use-Case**: Raid-Loot-Council

---

## LootRulesChange (3121)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Ändert Loot-Rules für Party.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootMode | string | "group_roll", "group_master", "personal", "free_for_all" | Ja |
| MasterLooterId | int | Master-Looter-ID (wenn master) | Nein |

### Notizen
- **Modes**:
  - **group_roll**: Need/Greed/Pass
  - **group_master**: Leader assigned
  - **personal**: Jeder eigener Loot
  - **free_for_all**: Erster gewinnt

---

## PersonalLoot (3130)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Personal-Loot Mode: Jeder Spieler erhält eigenen Loot.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSourceId | int | Loot-Source | Ja |
| PersonalItems | List<LootItemInfo> | Nur für dich | Ja |
| PersonalGold | int | Dein Gold-Anteil | Ja |

### Notizen
- **Modern**: Standard in modernen MMOs
- **Fair**: Alle haben Chance auf Loot
- **No Drama**: Kein Ninja-Looting

---

## BonusRollPrompt (3131)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Prompt für Bonus-Roll (Extra-Chance auf Boss-Loot).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | uint | Boss-ID | Ja |
| CostTokens | int | Token-Cost | Ja |
| Timeout | int | Sekunden zum Entscheiden | Ja |

### Notizen
- **Use-Case**: Raid-Boss Kills
- **Cost**: Bonus-Roll-Token (Weekly-Cap)
- **Extra-Loot**: Zweite Chance auf Loot

---

## BonusRollUse (3132)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Verwendet Bonus-Roll-Token.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | uint | Boss-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** Zusätzlicher Loot oder Gold
- **Bei kein Glück:** "Better luck next time" Message

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
