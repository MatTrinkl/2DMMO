# 🤝 Trading Messages (1100-1199)

**Kategorie:** 11  
**Range:** 1100-1199  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [TradeRequest (1100)](#traderequest-1100)
- [TradeRequestResponse (1101)](#traderequestresponse-1101)
- [TradeUpdate (1102)](#tradeupdate-1102)
- [TradeSetItem (1103)](#tradesetitem-1103)
- [TradeRemoveItem (1104)](#traderemoveitem-1104)
- [TradeSetGold (1105)](#tradesetgold-1105)
- [TradeConfirm (1106)](#tradeconfirm-1106)
- [TradeUnconfirm (1107)](#tradeunconfirm-1107)
- [TradeLock (1108)](#tradelock-1108)
- [TradeCancel (1109)](#tradecancel-1109)
- [TradeComplete (1110)](#tradecomplete-1110)
- [TradeError (1111)](#tradeerror-1111)
- [TradeBusy (1112)](#tradebusy-1112)
- [TradeTargetBusy (1113)](#tradetargetbusy-1113)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Player-to-Player Trading** im 2DMMO.

Das Trading-System implementiert:
- Sichere Item- und Gold-Übertragung zwischen Spielern
- Two-Step Confirmation (Confirm → Lock)
- Anti-Scam Protection (beide Spieler müssen Lock vor Complete)
- Trade-Window UI-Sync
- Trade-Cancel Mechanismen
- Distance/Range Checks
- Soulbound/BoP Item Restrictions

**Server Authority**: Alle Trades werden server-seitig validiert. Beide Spieler müssen Confirm und Lock bestätigen bevor Trade executed wird.

**Trade Flow**: Request → Accept → Set Items/Gold → Confirm → Lock → Complete

---

## TradeRequest (1100)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client initiiert Trade-Request an anderen Spieler. Server prüft Range, Target-Validity und sendet Request-Prompt an Target.

### Im Scope ✅
- Trade-Request an Spieler in Range
- Range-Check (max. 10m)
- Target-Busy-Check
- Ignore-List Check

### Nicht im Scope ❌
- Auction House → verwende `AuctionCreate` (1704)
- Vendor-Trading → verwende `VendorBuy/Sell` (1314/1316)
- Mail-Items → verwende `MailSend` (1802)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetPlayerId | int | Entity-ID des Trade-Partners | Ja |

### Erwartete Response
- **Bei Erfolg:** `TradeRequestResponse` (1101) an beide Spieler
- **Bei Fehler:** `TradeError` (1111) mit Code

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TradeRequestResponse` | 1101 | Prompt an Target-Spieler |
| `TradeUpdate` | 1102 | Trade-Window wird geöffnet |
| `TradeBusy` | 1112 | Wenn Requester bereits im Trade |
| `TradeTargetBusy` | 1113 | Wenn Target bereits im Trade |

### Flow-Diagramm
```
Player A              Server              Player B
  │                      │                   │
  │  TradeRequest(B)     │                   │
  │─────────────────────►│                   │
  │                      │  Range Check      │
  │                      │  Busy Check       │
  │                      │                   │
  │                      │  Request Prompt   │
  │                      │──────────────────►│
  │                      │                   │
  │                      │  Accept/Decline   │
  │                      │◄──────────────────│
  │                      │                   │
  │  TradeUpdate         │  TradeUpdate      │
  │◄─────────────────────┼──────────────────►│
```

### Beispiel Payload
```csharp
var tradeRequest = new TradeRequest
{
    Type = MessageType.TradeRequest,
    TargetPlayerId = 50002
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `TARGET_TOO_FAR` | Target außerhalb Range (>10m) | Näher kommen |
| `TARGET_NOT_FOUND` | Target-Entity existiert nicht | Erneut targetieren |
| `ALREADY_TRADING` | Requester ist bereits im Trade | Trade abschließen/canceln |
| `TARGET_TRADING` | Target ist bereits im Trade | Warten |
| `TARGET_IGNORED_YOU` | Target hat Requester auf Ignore | Ignorieren |
| `TARGET_BUSY` | Target ist beschäftigt (Combat, AFK, etc.) | Später versuchen |

### Notizen
- **Range**: Max. 10 Meter zwischen Spielern
- **Movement**: Trade cancelled wenn Spieler sich >15m entfernen
- **Combat**: Trade nicht möglich im Kampf
- **Timeout**: Request-Prompt expires nach 30 Sekunden
- **Cooldown**: 1 Sekunde zwischen Trade-Requests (Spam-Prevention)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)

## TradeRequestResponse (1101)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Prompt an Target-Spieler dass Trade-Request empfangen wurde. Target kann Accept/Decline.

### Im Scope ✅
- Trade-Request Prompt
- Requester-Info (Name, Level)
- Accept/Decline Buttons

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequesterPlayerId | int | Entity-ID des Requesters | Ja |
| RequesterName | string | Name des Requesters | Ja |
| RequesterLevel | int | Level des Requesters | Ja |
| ExpiryTime | long | Unix Timestamp (Ablauf) | Ja |

### Beispiel Payload
```csharp
var requestResponse = new TradeRequestResponse
{
    Type = MessageType.TradeRequestResponse,
    RequesterPlayerId = 50001,
    RequesterName = "Aragorn",
    RequesterLevel = 10,
    ExpiryTime = DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds()
};
```

### Notizen
- **Timeout**: 30 Sekunden zum Accept
- **Auto-Decline**: Bei Timeout oder Movement >15m
- **UI**: Client zeigt Popup mit Accept/Decline

---

## TradeUpdate (1102)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Sync-Update des Trade-States. Wird gesendet bei Trade-Start, Item-Changes, Gold-Changes, Confirm-Changes, etc.

### Im Scope ✅
- Kompletter Trade-State
- Items beider Spieler
- Gold beider Spieler
- Confirm/Lock Status

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TradeId | string | Eindeutige Trade-Session-ID (UUID) | Ja |
| OwnSlots | List<TradeSlot> | Eigene Trade-Slots | Ja |
| OtherSlots | List<TradeSlot> | Trade-Slots des Partners | Ja |
| OwnGold | int | Eigenes Gold (Copper) | Ja |
| OtherGold | int | Gold des Partners (Copper) | Ja |
| OwnConfirmed | bool | Eigener Confirm-Status | Ja |
| OtherConfirmed | bool | Confirm-Status des Partners | Ja |
| OwnLocked | bool | Eigener Lock-Status | Ja |
| OtherLocked | bool | Lock-Status des Partners | Ja |

**TradeSlot**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SlotIndex | byte | Slot-Index (0-7, max. 8 Items) |
| ItemId | uint | Item-ID |
| Quantity | int | Anzahl |

### Beispiel Payload
```csharp
var tradeUpdate = new TradeUpdate
{
    Type = MessageType.TradeUpdate,
    TradeId = "a3f7c2b1-4d5e-6f7a-8b9c-0d1e2f3a4b5c",
    OwnSlots = new List<TradeSlot>
    {
        new TradeSlot { SlotIndex = 0, ItemId = 5001, Quantity = 1 }
    },
    OtherSlots = new List<TradeSlot>
    {
        new TradeSlot { SlotIndex = 0, ItemId = 5002, Quantity = 5 }
    },
    OwnGold = 10000,
    OtherGold = 5000,
    OwnConfirmed = true,
    OtherConfirmed = false,
    OwnLocked = false,
    OtherLocked = false
};
```

### Notizen
- **Max Slots**: 8 Item-Slots pro Spieler
- **Gold-Cap**: Max. 999,999 Gold pro Trade
- **Sync**: Beide Spieler erhalten identisches Update

---

## TradeSetItem (1103)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fügt Item zu Trade-Window hinzu. Server validiert Item-Ownership, Soulbound-Status, und Quantity.

### Im Scope ✅
- Item zu Trade hinzufügen
- Stack-Items (Quantity)
- Slot-Assignment

### Nicht im Scope ❌
- Item entfernen → verwende `TradeRemoveItem` (1104)
- Gold setzen → verwende `TradeSetGold` (1105)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Ziel-Slot im Trade-Window (0-7) | Ja |
| InventorySlot | byte | Source-Slot im Inventory | Ja |
| Quantity | int | Anzahl (bei Stackable Items) | Ja |

### Erwartete Response
- **Bei Erfolg:** `TradeUpdate` (1102) an beide Spieler
- **Bei Fehler:** `TradeError` (1111)

### Beispiel Payload
```csharp
var setItem = new TradeSetItem
{
    Type = MessageType.TradeSetItem,
    SlotIndex = 0,
    InventorySlot = 15,
    Quantity = 1
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ITEM_SOULBOUND` | Item ist Soulbound/BoP | Anderes Item wählen |
| `ITEM_NOT_FOUND` | Item nicht im Inventory | Inventory-Sync |
| `INVALID_QUANTITY` | Quantity > Stack-Size | Quantity reduzieren |
| `SLOT_OCCUPIED` | Trade-Slot bereits belegt | Anderen Slot wählen |
| `TRADE_LOCKED` | Trade ist bereits gelocked | Unlock erforderlich |

### Notizen
- **Soulbound**: Soulbound/BoP Items können nicht traded werden
- **Quest-Items**: Quest-Items können nicht traded werden
- **Auto-Unconfirm**: Beide Spieler werden auto-unconfirmed nach Item-Change

---

## TradeRemoveItem (1104)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entfernt Item aus Trade-Window.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Trade-Slot zum leeren | Ja |

### Erwartete Response
- **Immer:** `TradeUpdate` (1102) an beide Spieler

### Notizen
- **Auto-Unconfirm**: Beide Spieler werden auto-unconfirmed
- **Lock**: Nicht möglich wenn Trade gelocked

---

## TradeSetGold (1105)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Setzt Gold-Amount im Trade.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GoldAmount | int | Gold in Copper | Ja |

### Erwartete Response
- **Bei Erfolg:** `TradeUpdate` (1102)
- **Bei Fehler:** `TradeError` (1111)

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold | Amount reduzieren |
| `AMOUNT_TOO_HIGH` | Amount > 999,999 Gold | Cap einhalten |
| `TRADE_LOCKED` | Trade ist gelocked | Unlock erforderlich |

### Notizen
- **Max**: 999,999 Gold (9999g 99s 99c)
- **Validation**: Server prüft ob Spieler genug Gold hat
- **Auto-Unconfirm**: Beide Spieler werden auto-unconfirmed

---

## TradeConfirm (1106)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigt Trade-Content (erster Confirmation-Step). Beide Spieler müssen Confirm bevor Lock möglich ist.

### Im Scope ✅
- Trade-Content bestätigen
- Vorbereitung für Lock
- Anti-Scam Protection

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `TradeUpdate` (1102) mit OwnConfirmed=true

### Notizen
- **Two-Step**: Confirm → Lock → Complete
- **Visual**: Client zeigt grünes Checkmark
- **Unconfirm**: Automatisch bei Item/Gold-Change

---

## TradeUnconfirm (1107)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Nimmt Confirmation zurück.

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `TradeUpdate` (1102) mit OwnConfirmed=false

### Notizen
- **Auto**: Passiert automatisch bei Item/Gold-Change
- **Manual**: Spieler kann auch manuell unconfirm

---

## TradeLock (1108)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** �� Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Locked den Trade (zweiter Confirmation-Step). Wenn beide Spieler Locked haben, wird Trade executed.

### Im Scope ✅
- Trade finalisieren
- Auto-Complete wenn beide gelocked

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Bei Erfolg:** `TradeUpdate` (1102) mit OwnLocked=true
- **Bei beiden Locked:** `TradeComplete` (1110)

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_CONFIRMED` | Spieler hat noch nicht confirmed | Zuerst Confirm |
| `OTHER_NOT_CONFIRMED` | Partner hat noch nicht confirmed | Warten |

### Notizen
- **Final Step**: Letzte Chance vor Trade-Execution
- **Auto-Complete**: Wenn beide Locked, executed Server automatisch
- **No Undo**: Nach Lock kann nur noch Cancel

---

## TradeCancel (1109)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Cancelled Trade. Items/Gold werden zurückgegeben.

### Im Scope ✅
- Trade abbrechen
- Items zurückgeben
- Cleanup

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** Trade-Window wird geschlossen, Items werden returned

### Notizen
- **Any Time**: Kann jederzeit gecancelt werden (auch nach Lock)
- **Notification**: Beide Spieler sehen Cancel-Message
- **Auto-Cancel**: Bei Movement >15m oder Combat

---

## TradeComplete (1110)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Server hat Trade erfolgreich executed. Items/Gold wurden übertragen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TradeId | string | Trade-Session-ID | Ja |
| Success | bool | Trade erfolgreich? | Ja |

### Beispiel Payload
```csharp
var complete = new TradeComplete
{
    Type = MessageType.TradeComplete,
    TradeId = "a3f7c2b1-4d5e-6f7a-8b9c-0d1e2f3a4b5c",
    Success = true
};
```

### Notizen
- **Atomic**: Trade ist atomic - entweder alles oder nichts
- **Logging**: Alle Trades werden geloggt (Audit-Trail)
- **UI**: Client zeigt Success-Message und schließt Trade-Window

---

## TradeError (1111)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Fehler während Trade-Operation.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ErrorCode | string | Fehlercode | Ja |
| ErrorMessage | string | Menschenlesbare Message | Ja |

### Beispiel Payload
```csharp
var error = new TradeError
{
    Type = MessageType.TradeError,
    ErrorCode = "INVENTORY_FULL",
    ErrorMessage = "Your inventory is full. Cannot complete trade."
};
```

### Error Codes
Siehe einzelne Messages oben für spezifische Error Codes.

---

## TradeBusy (1112)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Requester ist bereits im Trade.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Info-Text | Ja |

### Notizen
- **Prevention**: Client sollte Trade-Button disablen wenn bereits trading

---

## TradeTargetBusy (1113)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Target-Spieler ist bereits im Trade.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetName | string | Name des Targets | Ja |
| Message | string | Info-Text | Ja |

### Notizen
- **Retry**: Spieler kann später erneut versuchen

---
