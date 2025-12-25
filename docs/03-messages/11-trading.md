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
