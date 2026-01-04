# 🏪 Auction Messages (1700-1720)

**Kategorie:** 17  
**Range:** 1700-1720 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Auction Flow](#-auction-flow)
  - [Server-Authoritative Architektur](#server-authoritative-architektur)
  - [Auction-Listing Flow](#auction-listing-flow)
  - [Bidding/Buyout Flow](#biddingbuyout-flow)
- [🧱 DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
  - [AuctionDuration enum](#auctionduration-enum)
  - [AuctionSortType enum](#auctionsorttype-enum)
  - [AuctionErrorCode enum](#auctionerrorcode-enum)
  - [AuctionListingDto](#auctionlistingdto)
  - [AuctionSearchFilterDto](#auctionsearchfilterdto)
  - [PriceHistoryEntryDto](#pricehistoryentrydto)
  - [Wichtige Konstanten](#wichtige-konstanten)
- [📩 Aktive Messages](#-aktive-messages-1700-1720)
  - [AuctionOpen (1700)](#auctionopen-1700)
  - [AuctionClose (1701)](#auctionclose-1701)
  - [AuctionSearch (1702)](#auctionsearch-1702)
  - [AuctionSearchResults (1703)](#auctionsearchresults-1703)
  - [AuctionCreate (1704)](#auctioncreate-1704)
  - [AuctionCreateResult (1705)](#auctioncreateresult-1705)
  - [AuctionBid (1706)](#auctionbid-1706)
  - [AuctionBidResult (1707)](#auctionbidresult-1707)
  - [AuctionBuyout (1708)](#auctionbuyout-1708)
  - [AuctionBuyoutResult (1709)](#auctionbuyoutresult-1709)
  - [AuctionCancel (1710)](#auctioncancel-1710)
  - [AuctionCancelResult (1711)](#auctioncancelresult-1711)
  - [AuctionExpired (1712)](#auctionexpired-1712)
  - [AuctionSold (1713)](#auctionsold-1713)
  - [AuctionOutbid (1714)](#auctionoutbid-1714)
  - [AuctionWon (1715)](#auctionwon-1715)
  - [AuctionListOwned (1716)](#auctionlistowned-1716)
  - [AuctionListBids (1717)](#auctionlistbids-1717)
  - [AuctionPriceHistory (1718)](#auctionpricehistory-1718)
  - [AuctionFavorite (1719)](#auctionfavorite-1719)
  - [AuctionFavoriteList (1720)](#auctionfavoritelist-1720)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)
  - [MessageType Enum (Auction)](#messagetype-enum-auction)
  - [Request/Response Paare](#requestresponse-paare)
  - [Datei-Struktur](#datei-struktur)

---

## 🔄 Auction Flow

### Server-Authoritative Architektur

Das Auction House System ist vollständig **Server-Authoritative**:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         AUCTION HOUSE SYSTEM                            │
├─────────────────────────────────────────────────────────────────────────┤
│  Client (UI Only)           │  Zone Server         │  Auction Database  │
│  ─────────────────          │  ────────────        │  ─────────────────  │
│  • Zeigt Listings           │  • Validierung       │  • Persistenz       │
│  • Sammelt Input            │  • Gebühren          │  • Suche/Filter     │
│  • Optimistic UI            │  • Transactions      │  • History          │
│                             │  • Notifications     │  • Favoriten        │
└─────────────────────────────────────────────────────────────────────────┘
```

**Server-Authority Regeln:**
1. Alle Transaktionen sind serverseitig atomar
2. Gold/Items werden erst nach Server-Bestätigung transferiert
3. Gebühren werden sofort bei Listing-Erstellung abgezogen
4. Bei Buyout: Instant-Transfer ohne Wartezeit
5. Bei Bid: Gold wird reserviert bis Auction endet

### Auction-Listing Flow

```
Client                      Zone Server                   Auction DB
  │                             │                             │
  │  AuctionOpen (1700)         │                             │
  │────────────────────────────►│                             │
  │                             │  Validate: NPC in Range     │
  │                             │  Load owned listings        │
  │  AuctionListOwned (1716)    │                             │
  │◄────────────────────────────│                             │
  │                             │                             │
  │  AuctionCreate (1704)       │                             │
  │────────────────────────────►│                             │
  │                             │  Validate:                  │
  │                             │  - Item exists in inventory │
  │                             │  - Gold for deposit         │
  │                             │  - Max listings not reached │
  │                             │  Deduct: Deposit fee        │
  │                             │  Remove: Item from inventory│
  │                             │                     INSERT  │
  │                             │────────────────────────────►│
  │  AuctionCreateResult (1705) │                             │
  │◄────────────────────────────│                             │
  │  Success + AuctionId        │                             │
```

### Bidding/Buyout Flow

```
Bidder                      Zone Server                   Seller (Offline)
  │                             │                             │
  │  AuctionBid (1706)          │                             │
  │────────────────────────────►│                             │
  │                             │  Validate:                  │
  │                             │  - Auction exists           │
  │                             │  - Bid >= CurrentBid + Min  │
  │                             │  - Bidder has gold          │
  │                             │                             │
  │                             │  Reserve gold from bidder   │
  │                             │  Return gold to prev bidder │
  │                             │                             │
  │  AuctionBidResult (1707)    │                             │
  │◄────────────────────────────│                             │
  │                             │                             │
  │                             │  [Previous bidder online?]  │
  │                             │  AuctionOutbid (1714)       │
  │                             │───────────────────────────► │
  │                             │                             │
  │  === Auction ends ===       │                             │
  │                             │                             │
  │  AuctionWon (1715)          │                             │
  │◄────────────────────────────│                             │
  │  Item delivered via mail    │                             │
  │                             │  AuctionSold (1713)         │
  │                             │───────────────────────────► │
  │                             │  Gold delivered via mail    │
```

---

## 🧱 DTOs / Enums / Interfaces

### AuctionDuration enum

```csharp
public enum AuctionDuration : byte
{
    Short = 0,    // 12 Stunden
    Medium = 1,   // 24 Stunden  
    Long = 2,     // 48 Stunden
}
```

### AuctionSortType enum

```csharp
public enum AuctionSortType : byte
{
    TimeLeft = 0,           // Zeit bis Ablauf (aufsteigend)
    TimeLeftDesc = 1,       // Zeit bis Ablauf (absteigend)
    BidPrice = 2,           // Aktuelles Gebot (aufsteigend)
    BidPriceDesc = 3,       // Aktuelles Gebot (absteigend)
    BuyoutPrice = 4,        // Sofortkauf-Preis (aufsteigend)
    BuyoutPriceDesc = 5,    // Sofortkauf-Preis (absteigend)
    ItemLevel = 6,          // Item-Level (aufsteigend)
    ItemLevelDesc = 7,      // Item-Level (absteigend)
    Name = 8,               // Item-Name (A-Z)
    NameDesc = 9,           // Item-Name (Z-A)
}
```

### AuctionErrorCode enum

```csharp
public enum AuctionErrorCode : byte
{
    None = 0,
    NotAtAuctioneer = 1,       // Spieler nicht bei Auktionator NPC
    ItemNotFound = 2,          // Item nicht im Inventar
    NotEnoughGold = 3,         // Nicht genug Gold für Deposit/Bid
    AuctionNotFound = 4,       // Auction existiert nicht mehr
    BidTooLow = 5,             // Gebot unter Minimum
    CannotBidOwnAuction = 6,   // Kann nicht auf eigene Auction bieten
    MaxListingsReached = 7,    // Maximale Listings erreicht
    AuctionExpired = 8,        // Auction bereits abgelaufen
    AlreadyHighestBidder = 9,  // Bereits Höchstbietender
    InvalidPrice = 10,         // Ungültiger Preis (0 oder negativ)
    InvalidDuration = 11,      // Ungültige Laufzeit
    ItemNotAuctionable = 12,   // Item kann nicht versteigert werden
    DatabaseError = 13,        // Datenbank-Fehler
}
```

### AuctionListingDto

```csharp
[MessagePackObject]
public class AuctionListingDto
{
    [Key(0)] public ulong AuctionId { get; set; }
    [Key(1)] public uint ItemId { get; set; }
    [Key(2)] public ushort StackSize { get; set; }
    [Key(3)] public byte ItemQuality { get; set; }
    [Key(4)] public uint CurrentBid { get; set; }
    [Key(5)] public uint BuyoutPrice { get; set; }     // 0 = kein Buyout
    [Key(6)] public uint TimeLeftSeconds { get; set; }
    [Key(7)] public string SellerName { get; set; }
    [Key(8)] public uint SellerId { get; set; }
    [Key(9)] public uint HighestBidderId { get; set; } // 0 = keine Gebote
    [Key(10)] public byte BidCount { get; set; }
}
```

### AuctionSearchFilterDto

```csharp
[MessagePackObject]
public class AuctionSearchFilterDto
{
    [Key(0)] public string SearchText { get; set; }        // Item-Name (partial match)
    [Key(1)] public ushort MinLevel { get; set; }          // Min Item-Level
    [Key(2)] public ushort MaxLevel { get; set; }          // Max Item-Level
    [Key(3)] public byte CategoryId { get; set; }          // Item-Kategorie (0 = alle)
    [Key(4)] public byte SubCategoryId { get; set; }       // Sub-Kategorie (0 = alle)
    [Key(5)] public byte QualityMin { get; set; }          // Min Qualität
    [Key(6)] public bool UsableOnly { get; set; }          // Nur für Klasse nutzbar
    [Key(7)] public AuctionSortType SortBy { get; set; }
    [Key(8)] public ushort PageNumber { get; set; }        // 0-basiert
    [Key(9)] public byte PageSize { get; set; }            // Max 50
}
```

### PriceHistoryEntryDto

```csharp
[MessagePackObject]
public class PriceHistoryEntryDto
{
    [Key(0)] public uint Timestamp { get; set; }      // Unix timestamp
    [Key(1)] public uint AveragePrice { get; set; }   // Durchschnittspreis
    [Key(2)] public uint MinPrice { get; set; }       // Niedrigster Preis
    [Key(3)] public uint MaxPrice { get; set; }       // Höchster Preis
    [Key(4)] public ushort Volume { get; set; }       // Verkaufte Menge
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `AUCTION_MAX_LISTINGS` | 20 | Max aktive Auctions pro Spieler |
| `AUCTION_MAX_FAVORITES` | 50 | Max favorisierte Auctions |
| `AUCTION_DEPOSIT_PERCENT` | 5 | Einstellgebühr in % des StartBid |
| `AUCTION_CUT_PERCENT` | 5 | Verkaufsgebühr in % des Verkaufspreises |
| `AUCTION_MIN_BID_INCREMENT` | 5 | Mindest-Erhöhung in % des aktuellen Gebots |
| `AUCTION_SEARCH_MAX_RESULTS` | 50 | Max Ergebnisse pro Seite |
| `AUCTION_PRICE_HISTORY_DAYS` | 30 | Tage für Preis-Historie |
| `AUCTION_NPC_RANGE` | 10.0 | Max Entfernung zum Auktionator |

---

## 📩 Aktive Messages (1700-1720)

### AuctionOpen (1700)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (bei NPC-Interaktion)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Client öffnet das Auction-House-Interface durch Interaktion mit einem Auktionator-NPC. Server validiert Entfernung und sendet initiale Daten.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionOpen` (1700) | Ja |
| NpcId | uint | Entity-ID des Auktionator-NPCs | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AuctionListOwned` (1716) + `AuctionListBids` (1717)
- **Bei Fehler:** `ErrorMessage` (910) mit `NotAtAuctioneer`

#### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `AuctionClose` | 1701 | Schließt Session |
| `AuctionListOwned` | 1716 | Eigene Listings |
| `AuctionListBids` | 1717 | Eigene Gebote |
| `AuctioneerOpen` | 1345 | Alternative NPC-Trigger |

---

### AuctionClose (1701)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client schließt das Auction-House-Interface. Server beendet die Session und gibt Ressourcen frei.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionClose` (1701) | Ja |

#### Erwartete Response
Keine dedizierte Response erforderlich.

---

### AuctionSearch (1702)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig (bei jeder Suche)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client sucht nach Auctions mit Filtern. Server führt Suche in Datenbank aus und sendet paginierte Ergebnisse.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionSearch` (1702) | Ja |
| Filter | AuctionSearchFilterDto | Such-Filter | Ja |

#### Erwartete Response
- `AuctionSearchResults` (1703)

#### Beispiel Payload
```csharp
var searchRequest = new AuctionSearchRequest
{
    Type = MessageType.AuctionSearch,
    Filter = new AuctionSearchFilterDto
    {
        SearchText = "Sword",
        MinLevel = 10,
        MaxLevel = 50,
        QualityMin = 2, // Rare+
        SortBy = AuctionSortType.BuyoutPrice,
        PageNumber = 0,
        PageSize = 20
    }
};
```

---

### AuctionSearchResults (1703)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Suchergebnisse zurück an Client. Enthält paginierte Liste von Auction-Listings.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionSearchResults` (1703) | Ja |
| Listings | AuctionListingDto[] | Array von Listings | Ja |
| TotalCount | uint | Gesamtanzahl Ergebnisse | Ja |
| PageNumber | ushort | Aktuelle Seite | Ja |
| TotalPages | ushort | Gesamtanzahl Seiten | Ja |

---

### AuctionCreate (1704)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client erstellt eine neue Auction. Server validiert Item, zieht Deposit ab und listet Auction.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionCreate` (1704) | Ja |
| InventorySlot | ushort | Slot des Items im Inventar | Ja |
| StackSize | ushort | Anzahl zu verkaufen (bei Stacks) | Ja |
| StartBid | uint | Startgebot in Gold | Ja |
| BuyoutPrice | uint | Sofortkauf-Preis (0 = kein Buyout) | Ja |
| Duration | AuctionDuration | Laufzeit | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AuctionCreateResult` (1705) mit Success
- **Bei Fehler:** `AuctionCreateResult` (1705) mit ErrorCode

#### Validierungen (Server)
1. Spieler ist bei Auktionator NPC
2. Item existiert im angegebenen Slot
3. StackSize <= Item.Count
4. StartBid > 0
5. BuyoutPrice == 0 OR BuyoutPrice >= StartBid
6. Spieler hat genug Gold für Deposit
7. Spieler hat nicht max Listings erreicht
8. Item ist handelbar (nicht Soulbound)

---

### AuctionCreateResult (1705)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt oder verweigert Auction-Erstellung.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionCreateResult` (1705) | Ja |
| Success | bool | Erfolgreich erstellt | Ja |
| ErrorCode | AuctionErrorCode | Fehlercode (wenn !Success) | Nein |
| AuctionId | ulong | ID der neuen Auction (wenn Success) | Nein |
| DepositPaid | uint | Gezahlte Einstellgebühr | Nein |

---

### AuctionBid (1706)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client gibt ein Gebot auf eine Auction ab. Gold wird reserviert bis Auction endet.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionBid` (1706) | Ja |
| AuctionId | ulong | ID der Auction | Ja |
| BidAmount | uint | Gebotsbetrag in Gold | Ja |

#### Erwartete Response
- `AuctionBidResult` (1707)

#### Validierungen (Server)
1. Auction existiert und ist aktiv
2. BidAmount >= CurrentBid + MinIncrement
3. Spieler ist nicht Verkäufer
4. Spieler ist nicht bereits Höchstbietender
5. Spieler hat genug Gold

---

### AuctionBidResult (1707)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt oder verweigert das Gebot.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionBidResult` (1707) | Ja |
| Success | bool | Gebot akzeptiert | Ja |
| ErrorCode | AuctionErrorCode | Fehlercode (wenn !Success) | Nein |
| AuctionId | ulong | ID der Auction | Ja |
| NewCurrentBid | uint | Neues aktuelles Gebot | Nein |
| GoldReserved | uint | Reserviertes Gold | Nein |

---

### AuctionBuyout (1708)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client kauft Auction sofort zum Buyout-Preis. Transaktion ist instant.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionBuyout` (1708) | Ja |
| AuctionId | ulong | ID der Auction | Ja |

#### Erwartete Response
- `AuctionBuyoutResult` (1709)

---

### AuctionBuyoutResult (1709)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt Buyout. Item wird direkt geliefert oder via Mail.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionBuyoutResult` (1709) | Ja |
| Success | bool | Buyout erfolgreich | Ja |
| ErrorCode | AuctionErrorCode | Fehlercode (wenn !Success) | Nein |
| AuctionId | ulong | ID der Auction | Ja |
| GoldPaid | uint | Gezahlter Betrag | Nein |
| DeliveredViaInventory | bool | Item direkt ins Inventar | Nein |
| DeliveredViaMail | bool | Item per Mail geliefert | Nein |

---

### AuctionCancel (1710)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client bricht eigene Auction ab. Deposit geht verloren, aber Item wird zurückgegeben.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionCancel` (1710) | Ja |
| AuctionId | ulong | ID der Auction | Ja |

#### Erwartete Response
- `AuctionCancelResult` (1711)

#### Validierungen (Server)
1. Auction existiert
2. Spieler ist Eigentümer
3. Auction hat keine Gebote (sonst nicht abbrechbar)

---

### AuctionCancelResult (1711)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt Auction-Abbruch.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionCancelResult` (1711) | Ja |
| Success | bool | Abbruch erfolgreich | Ja |
| ErrorCode | AuctionErrorCode | Fehlercode (wenn !Success) | Nein |
| AuctionId | ulong | ID der Auction | Ja |
| ItemReturned | bool | Item zurückgegeben | Nein |
| DepositLost | uint | Verlorenes Deposit | Nein |

---

### AuctionExpired (1712)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Push-Notification)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server benachrichtigt Verkäufer: Auction abgelaufen ohne Verkauf. Item wird per Mail zurückgeschickt.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionExpired` (1712) | Ja |
| AuctionId | ulong | ID der Auction | Ja |
| ItemId | uint | Item-ID | Ja |
| StackSize | ushort | Menge | Ja |

---

### AuctionSold (1713)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Push-Notification)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server benachrichtigt Verkäufer: Auction wurde verkauft. Gold (minus Gebühren) wird per Mail zugestellt.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionSold` (1713) | Ja |
| AuctionId | ulong | ID der Auction | Ja |
| ItemId | uint | Item-ID | Ja |
| StackSize | ushort | Menge | Ja |
| SalePrice | uint | Verkaufspreis | Ja |
| AuctionCut | uint | Abgezogene Gebühr | Ja |
| GoldReceived | uint | Gold für Verkäufer | Ja |
| BuyerName | string | Name des Käufers | Ja |

---

### AuctionOutbid (1714)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Push-Notification)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server benachrichtigt Bieter: Gebot wurde überboten. Reserviertes Gold wird zurückgegeben.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionOutbid` (1714) | Ja |
| AuctionId | ulong | ID der Auction | Ja |
| ItemId | uint | Item-ID (für Anzeige) | Ja |
| YourBid | uint | Dein altes Gebot | Ja |
| NewHighBid | uint | Neues Höchstgebot | Ja |
| GoldReturned | uint | Zurückgegebenes Gold | Ja |

---

### AuctionWon (1715)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Push-Notification)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server benachrichtigt Höchstbietenden: Auction gewonnen. Item wird per Mail zugestellt.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionWon` (1715) | Ja |
| AuctionId | ulong | ID der Auction | Ja |
| ItemId | uint | Item-ID | Ja |
| StackSize | ushort | Menge | Ja |
| WinningBid | uint | Gewinn-Gebot | Ja |
| SellerName | string | Name des Verkäufers | Ja |

---

### AuctionListOwned (1716)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (bei Open)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Liste aller eigenen aktiven Auctions.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionListOwned` (1716) | Ja |
| Listings | AuctionListingDto[] | Eigene Listings | Ja |
| TotalDeposits | uint | Summe aller Deposits | Ja |

---

### AuctionListBids (1717)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (bei Open)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Liste aller Auctions auf die der Spieler geboten hat.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionListBids` (1717) | Ja |
| Listings | AuctionListingDto[] | Auctions mit Geboten | Ja |
| TotalGoldReserved | uint | Summe reserviertes Gold | Ja |

---

### AuctionPriceHistory (1718)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Preis-Historie für einen Item-Typ (letzte 30 Tage).

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionPriceHistory` (1718) | Ja |
| ItemId | uint | Item-ID | Ja |
| History | PriceHistoryEntryDto[] | Preis-Daten pro Tag | Ja |

#### Annahme
Es gibt eine implizite Request-Message oder der Request wird über `AuctionSearch` mit speziellem Flag ausgelöst. Falls Request fehlt: Wird durch UI-Aktion in `AuctionSearch` mit `GetPriceHistory=true` integriert.

---

### AuctionFavorite (1719)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client markiert/entfernt Auction als Favorit für schnellen Zugriff.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionFavorite` (1719) | Ja |
| AuctionId | ulong | ID der Auction | Ja |
| IsFavorite | bool | true = hinzufügen, false = entfernen | Ja |

#### Erwartete Response
- `AuctionFavoriteList` (1720) mit aktualisierter Liste

---

### AuctionFavoriteList (1720)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Liste aller favorisierten Auctions.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AuctionFavoriteList` (1720) | Ja |
| Favorites | AuctionListingDto[] | Favorisierte Listings | Ja |
| ExpiredCount | ushort | Anzahl abgelaufener Favoriten | Ja |

---

## 🗑️ Obsolete Messages

*Keine obsoleten Messages in dieser Kategorie.*

---

## 📎 Anhang

### MessageType Enum (Auction)

Exakte Reihenfolge wie in `MessageType.cs`:

```csharp
// AUCTION HOUSE / MARKET (1700-1799)
AuctionOpen = 1700,
AuctionClose = 1701,
AuctionSearch = 1702,
AuctionSearchResults = 1703,
AuctionCreate = 1704,
AuctionCreateResult = 1705,
AuctionBid = 1706,
AuctionBidResult = 1707,
AuctionBuyout = 1708,
AuctionBuyoutResult = 1709,
AuctionCancel = 1710,
AuctionCancelResult = 1711,
AuctionExpired = 1712,
AuctionSold = 1713,
AuctionOutbid = 1714,
AuctionWon = 1715,
AuctionListOwned = 1716,
AuctionListBids = 1717,
AuctionPriceHistory = 1718,
AuctionFavorite = 1719,
AuctionFavoriteList = 1720,
```

### Request/Response Paare

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| `AuctionOpen` | 1700 | `AuctionListOwned` + `AuctionListBids` | 1716, 1717 |
| `AuctionSearch` | 1702 | `AuctionSearchResults` | 1703 |
| `AuctionCreate` | 1704 | `AuctionCreateResult` | 1705 |
| `AuctionBid` | 1706 | `AuctionBidResult` | 1707 |
| `AuctionBuyout` | 1708 | `AuctionBuyoutResult` | 1709 |
| `AuctionCancel` | 1710 | `AuctionCancelResult` | 1711 |
| `AuctionFavorite` | 1719 | `AuctionFavoriteList` | 1720 |

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   └── MessageType.cs           # Enthält AuctionOpen..AuctionFavoriteList
│   └── Messages/
│       └── Auction/
│           ├── AuctionOpenMessage.cs
│           ├── AuctionSearchMessage.cs
│           ├── AuctionSearchResultsMessage.cs
│           ├── AuctionCreateMessage.cs
│           ├── AuctionCreateResultMessage.cs
│           ├── AuctionBidMessage.cs
│           ├── AuctionBidResultMessage.cs
│           ├── AuctionBuyoutMessage.cs
│           ├── AuctionBuyoutResultMessage.cs
│           ├── AuctionCancelMessage.cs
│           ├── AuctionCancelResultMessage.cs
│           ├── AuctionExpiredMessage.cs
│           ├── AuctionSoldMessage.cs
│           ├── AuctionOutbidMessage.cs
│           ├── AuctionWonMessage.cs
│           ├── AuctionListOwnedMessage.cs
│           ├── AuctionListBidsMessage.cs
│           ├── AuctionPriceHistoryMessage.cs
│           ├── AuctionFavoriteMessage.cs
│           └── AuctionFavoriteListMessage.cs
└── DTOs/
    └── Auction/
        ├── AuctionListingDto.cs
        ├── AuctionSearchFilterDto.cs
        └── PriceHistoryEntryDto.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (21 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/17-auction.md
