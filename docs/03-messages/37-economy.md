# 💰 Economy / Currency Messages (3700-3799)

**Kategorie:** 37  
**Range:** 3700-3799  



[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [CurrencyUpdate (3700)](#currencyupdate-3700)
- [CurrencyListRequest (3701)](#currencylistrequest-3701)
- [CurrencyListResponse (3702)](#currencylistresponse-3702)
- [GoldUpdate (3703)](#goldupdate-3703)
- [GoldTransaction (3704)](#goldtransaction-3704)
- [CurrencyExchange (3710)](#currencyexchange-3710)
- [CurrencyExchangeResult (3711)](#currencyexchangeresult-3711)
- [TokenPurchase (3720)](#tokenpurchase-3720)
- [TokenPurchaseResult (3721)](#tokenpurchaseresult-3721)
- [BountyPlace (3730)](#bountyplace-3730)
- [BountyClaim (3732)](#bountyclaim-3732)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Economy- und Währungssysteme** im 2DMMO.

Das Economy-System implementiert:
- Multi-Currency System (Gold, Tokens, Premium-Currency)
- Currency-Exchange zwischen verschiedenen Währungen
- Gold-Transactions und Auditing
- Token-System (gekauft mit Real-Money)
- Bounty-System (Spieler-Kopfgelder)
- Currency-Caps und Limits

**Server Authority**: Alle Currency-Transactions sind server-authoritative.

---

## CurrencyUpdate (3700)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Update einer Währung.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CurrencyId | uint | Währungs-ID | Ja |
| NewAmount | int | Neuer Betrag | Ja |
| Change | int | Änderung | Ja |

---

## GoldUpdate (3703)

**Richtung:** 📡 Broadcast  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Gold-Betrag hat sich geändert.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewGold | long | Neuer Gold-Betrag (Copper) | Ja |
| Change | long | Änderung | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
