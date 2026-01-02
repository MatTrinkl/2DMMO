# 🏪 Auction Messages (1700-1799)

**Kategorie:** 17  
**Range:** 1700-1799  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Auction-House für Player-to-Player Trading.

---

## AuctionCreate (1700)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemSlot | byte | Item-Slot | Ja |
| Quantity | int | Anzahl | Ja |
| StartPrice | long | Start-Preis (Copper) | Ja |
| BuyoutPrice | long | Buyout (optional) | Nein |
| Duration | int | Duration (Stunden) | Ja |

---

## AuctionSearch (1701)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SearchTerm | string | Search-Term | Nein |
| Category | string | Item-Category | Nein |
| MinLevel | int | Min-Level | Nein |
| MaxLevel | int | Max-Level | Nein |

---

## AuctionSearchResult (1702)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Auctions | List<AuctionListing> | Gefundene Auctions | Ja |

---

## AuctionBid (1710)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AuctionId | string | Auction-ID | Ja |
| BidAmount | long | Bid-Betrag (Copper) | Ja |

---

## AuctionBuyout (1711)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AuctionId | string | Auction-ID | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
