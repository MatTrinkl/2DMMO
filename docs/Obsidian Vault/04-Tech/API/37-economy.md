# 💰 Economy Messages (3700-3799)

**Kategorie:** 37  
**Range:** 3700-3799  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [💳 Balances & Sync](#-balances--sync)
- [🧾 Transaction Logging & Audit](#-transaction-logging--audit)
- [🧮 Taxes, Fees & Sinks](#-taxes-fees--sinks)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#️-regeln--sicherheit)
- [📩 Aktive Messages 3700–3799](#-aktive-messages-37003799)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang](#-anhang)

---

## 📋 Überblick

Diese Kategorie umfasst alle Messages für das **Economy-System** im 2DMMO. Das Wirtschaftssystem ist **Server-autoritativ** – der Client zeigt nur an, was der Server bestätigt hat.

### Scope

| Im Scope ✅ | Nicht im Scope ❌ |
|-------------|-------------------|
| Währungsverwaltung (Gold, Premium, Tokens) | Echtgeld-Transaktionen (extern) |
| Balance-Synchronisation | Payment-Gateway Integration |
| Transaktionsprotokollierung | Rückbuchungen/Chargebacks |
| Steuern und Gebühren | Komplexe Marktanalysen |
| Währungstausch | Cross-Server Transfers |
| Kopfgeld-System | |

### Architektur-Übersicht

```
┌─────────────────────────────────────────────────────────────────┐
│  Client                                                          │
│  ├── WalletUI (Anzeige)                                         │
│  ├── TransactionHistory (readonly)                              │
│  └── CurrencyExchangeUI                                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Server (AUTORITATIV)                                            │
│  ├── WalletService (Balance Management)                         │
│  ├── TransactionService (Atomic Commits)                        │
│  ├── TaxService (Fee Calculation)                               │
│  └── AuditService (Logging)                                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Persistence                                                     │
│  ├── Wallets (Balance + Revision)                               │
│  ├── Transactions (Idempotent Log)                              │
│  └── TaxRules (Configuration)                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🧠 Datenmodell

### Currency

```csharp
[MessagePackObject]
public class CurrencyDefinition
{
    [Key(0)] public ushort CurrencyId { get; set; }
    [Key(1)] public string Name { get; set; } = "";
    [Key(2)] public string Symbol { get; set; } = "";
    [Key(3)] public byte Precision { get; set; } = 0; // Dezimalstellen
    [Key(4)] public bool IsTradeable { get; set; } = true;
    [Key(5)] public bool IsPremium { get; set; } = false;
    [Key(6)] public long MaxAmount { get; set; } = long.MaxValue;
    [Key(7)] public long WeeklyCap { get; set; } = 0; // 0 = kein Cap
}
```

**Standard-Währungen:**
| CurrencyId | Name | Symbol | Precision | Tradeable | Premium |
|------------|------|--------|-----------|-----------|---------|
| 1 | Gold | 🪙 | 0 | ✅ | ❌ |
| 2 | Premium Coins | 💎 | 0 | ❌ | ✅ |
| 10 | Honor Points | ⚔️ | 0 | ❌ | ❌ |
| 11 | Arena Points | 🏆 | 0 | ❌ | ❌ |
| 20 | Crafting Tokens | 🔧 | 0 | ✅ | ❌ |

### Wallet

```csharp
[MessagePackObject]
public class WalletEntry
{
    [Key(0)] public ushort CurrencyId { get; set; }
    [Key(1)] public long Balance { get; set; }
    [Key(2)] public long WeeklyEarned { get; set; } // Für Weekly Caps
    [Key(3)] public uint Revision { get; set; } // Optimistic Locking
}

[MessagePackObject]
public class CharacterWallet
{
    [Key(0)] public long CharacterId { get; set; }
    [Key(1)] public List<WalletEntry> Entries { get; set; } = new();
    [Key(2)] public long LastSyncTimeMs { get; set; }
}
```

### Transaction

```csharp
[MessagePackObject]
public class TransactionRecord
{
    [Key(0)] public Guid TransactionId { get; set; } // Idempotency Key
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public ushort CurrencyId { get; set; }
    [Key(3)] public long Amount { get; set; } // + = Einnahme, - = Ausgabe
    [Key(4)] public long BalanceAfter { get; set; }
    [Key(5)] public TransactionSourceType SourceType { get; set; }
    [Key(6)] public string? SourceReference { get; set; } // z.B. QuestId, TradeId
    [Key(7)] public long TimestampMs { get; set; }
    [Key(8)] public long? TaxAmount { get; set; } // Abgezogene Steuer
}
```

### TaxRule

```csharp
[MessagePackObject]
public class TaxRule
{
    [Key(0)] public TransactionSourceType SourceType { get; set; }
    [Key(1)] public ushort CurrencyId { get; set; }
    [Key(2)] public decimal TaxRate { get; set; } // 0.0 - 1.0 (z.B. 0.05 = 5%)
    [Key(3)] public long MinFee { get; set; }
    [Key(4)] public long MaxFee { get; set; }
    [Key(5)] public TaxDestination Destination { get; set; }
}
```

---

## 💳 Balances & Sync

### Synchronisation-Strategie

| Strategie | Wann | Inhalt |
|-----------|------|--------|
| **Full Sync** | Login, Reconnect | Komplettes Wallet mit allen Währungen |
| **Delta Update** | Bei Änderung | Nur geänderte Währung + neue Revision |

### Balance Sync Flow

```
Client                         Server
  │                              │
  │  [Login/Reconnect]           │
  │                              │
  │  CurrencyListRequest (3701)  │
  │─────────────────────────────►│
  │                              │
  │  CurrencyListResponse (3702) │
  │  [Full Wallet Snapshot]      │
  │◄─────────────────────────────│
  │                              │
  │  [Spieler verdient Gold]     │
  │                              │
  │  GoldUpdate (3703)           │
  │  [Delta: +100 Gold]          │
  │◄─────────────────────────────│
  │                              │
  │  [Spieler kauft Item]        │
  │                              │
  │  GoldTransaction (3704)      │
  │  [Delta: -50 Gold, TxId]     │
  │◄─────────────────────────────│
```

### Revision-basiertes Optimistic Locking

```csharp
// Server-seitig bei Transaktion
public bool TryUpdateBalance(long characterId, ushort currencyId, long delta, uint expectedRevision)
{
    var wallet = GetWallet(characterId, currencyId);
    
    // Optimistic Lock Check
    if (wallet.Revision != expectedRevision)
    {
        // Conflict - Client muss resyncen
        return false;
    }
    
    wallet.Balance += delta;
    wallet.Revision++;
    
    return true;
}
```

---

## 🧾 Transaction Logging & Audit

### Idempotenz

Jede Transaktion hat eine **TransactionId (GUID)** als Idempotency Key:

```csharp
// Anti-Dupe: Gleiche TransactionId = gleiche Antwort
public TransactionResult ProcessTransaction(TransactionRequest request)
{
    // Prüfe ob TransactionId bereits existiert
    var existing = _transactionLog.Find(request.TransactionId);
    if (existing != null)
    {
        // Idempotent: Gleiche Antwort zurückgeben
        return existing.Result;
    }
    
    // Neue Transaktion verarbeiten
    var result = ExecuteTransaction(request);
    _transactionLog.Save(request.TransactionId, result);
    
    return result;
}
```

### Audit Trail

Alle Economy-Aktionen werden protokolliert:

| Feld | Beschreibung |
|------|--------------|
| TransactionId | Eindeutige ID |
| CharacterId | Betroffener Character |
| CurrencyId | Währung |
| Amount | Betrag (+ oder -) |
| BalanceAfter | Stand nach Transaktion |
| SourceType | Ursprung (Quest, Trade, etc.) |
| SourceReference | Referenz-ID |
| Timestamp | Server-Zeit |
| TaxAmount | Abgezogene Steuer |

---

## 🧮 Taxes, Fees & Sinks

### Steuerberechnung

```csharp
public long CalculateTax(long amount, TaxRule rule)
{
    // Prozentuale Steuer
    var tax = (long)Math.Round(amount * rule.TaxRate, MidpointRounding.ToEven); // Banker's Rounding
    
    // Min/Max Grenzen
    tax = Math.Max(tax, rule.MinFee);
    tax = Math.Min(tax, rule.MaxFee);
    
    return tax;
}
```

### Standard-Steuersätze

| SourceType | Steuer | Min | Max | Destination |
|------------|--------|-----|-----|-------------|
| Trade | 5% | 1 | 10000 | Sink |
| AuctionSale | 5% | 1 | 50000 | Sink |
| AuctionDeposit | 1% | 1 | 1000 | Sink |
| MailCOD | 3% | 1 | 5000 | Sink |
| GuildBankWithdraw | 0% | 0 | 0 | - |

### Rounding Rules

- **Banker's Rounding** (MidpointRounding.ToEven) für alle Berechnungen
- Vermeidet systematische Auf-/Abrundung
- Beispiel: 2.5 → 2, 3.5 → 4

### Gold Sinks (Geld-Vernichtung)

| Sink | Beschreibung |
|------|--------------|
| Steuern | Aus Wirtschaft entfernt |
| Reparaturen | Equipment-Abnutzung |
| Teleport-Kosten | Convenience Fee |
| Auction Deposits | Nicht erstattbar bei Ablauf |
| Respec-Kosten | Talent-Reset |

---

## 🧱 DTOs / Interfaces

### INetworkMessage Interface

```csharp
public interface INetworkMessage
{
    MessageType Type { get; }
}

public interface IClientMessage : INetworkMessage { }
public interface IServerMessage : INetworkMessage { }
```

### CurrencyUpdateDto

```csharp
[MessagePackObject]
public class CurrencyUpdateDto
{
    [Key(0)] public ushort CurrencyId { get; set; }
    [Key(1)] public long NewBalance { get; set; }
    [Key(2)] public long Delta { get; set; }
    [Key(3)] public uint Revision { get; set; }
    [Key(4)] public TransactionSourceType? Source { get; set; }
}
```

### GoldTransactionDto

```csharp
[MessagePackObject]
public class GoldTransactionDto
{
    [Key(0)] public Guid TransactionId { get; set; }
    [Key(1)] public long Amount { get; set; }
    [Key(2)] public long NewBalance { get; set; }
    [Key(3)] public TransactionSourceType SourceType { get; set; }
    [Key(4)] public string? SourceReference { get; set; }
    [Key(5)] public long? TaxAmount { get; set; }
    [Key(6)] public long TimestampMs { get; set; }
}
```

### BountyDto

```csharp
[MessagePackObject]
public class BountyDto
{
    [Key(0)] public Guid BountyId { get; set; }
    [Key(1)] public long TargetCharacterId { get; set; }
    [Key(2)] public string TargetCharacterName { get; set; } = "";
    [Key(3)] public long RewardAmount { get; set; }
    [Key(4)] public ushort CurrencyId { get; set; }
    [Key(5)] public long PlacedByCharacterId { get; set; }
    [Key(6)] public long ExpiresAtMs { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### TransactionSourceType

```csharp
public enum TransactionSourceType : byte
{
    // Einnahmen (1-49)
    Loot = 1,
    QuestReward = 2,
    Trade = 3,
    AuctionSale = 4,
    MailReceive = 5,
    VendorSell = 6,
    Achievement = 7,
    GuildBankWithdraw = 8,
    BountyClaim = 9,
    AdminGrant = 10,
    
    // Ausgaben (50-99)
    VendorBuy = 50,
    TradeGive = 51,
    AuctionBid = 52,
    AuctionDeposit = 53,
    MailSend = 54,
    MailCOD = 55,
    Repair = 56,
    Respec = 57,
    Teleport = 58,
    GuildBankDeposit = 59,
    BountyPlace = 60,
    CurrencyExchange = 61,
    
    // System (100+)
    TaxDeduction = 100,
    Expiry = 101,
    AdminRemove = 102
}
```

### TaxDestination

```csharp
public enum TaxDestination : byte
{
    Sink = 0,       // Geld wird vernichtet
    Treasury = 1,   // Server-Treasury (für Events)
    Guild = 2       // Guild-Bank
}
```

### EconomyErrorCode

```csharp
public enum EconomyErrorCode : ushort
{
    // Allgemein (0-99)
    Success = 0,
    UnknownError = 1,
    InvalidRequest = 2,
    RateLimited = 3,
    
    // Balance (100-199)
    InsufficientFunds = 100,
    CurrencyNotFound = 101,
    CurrencyCapReached = 102,
    WeeklyCapReached = 103,
    BalanceOverflow = 104,
    
    // Transaktion (200-299)
    DuplicateTransaction = 200,
    TransactionFailed = 201,
    InvalidAmount = 202,
    NegativeAmount = 203,
    
    // Exchange (300-399)
    ExchangeNotAvailable = 300,
    InvalidExchangeRate = 301,
    ExchangeLimitReached = 302,
    
    // Token (400-499)
    InvalidToken = 400,
    TokenExpired = 401,
    TokenAlreadyRedeemed = 402,
    
    // Bounty (500-599)
    BountyNotFound = 500,
    BountyExpired = 501,
    CannotBountyYourself = 502,
    BountyAlreadyExists = 503,
    InvalidBountyAmount = 504
}
```

---

## ⚙️ Regeln & Sicherheit

### Server-Autorität

| Regel | Beschreibung |
|-------|--------------|
| **Keine Client-Beträge** | Client sendet NIE Beträge, nur Aktions-Requests |
| **Server berechnet** | Alle Beträge werden Server-seitig berechnet |
| **Atomic Commits** | Balance + Log in einer Transaktion |
| **Idempotenz** | Gleiche TransactionId = gleiche Antwort |

### Anti-Cheat / Anti-Dupe

```csharp
// KRITISCH: Jede Economy-Aktion braucht autorisiertes Event
public class EconomyValidator
{
    public bool ValidateTransaction(TransactionRequest request)
    {
        // 1. Source-Event muss existieren
        var sourceEvent = GetSourceEvent(request.SourceType, request.SourceReference);
        if (sourceEvent == null) return false;
        
        // 2. Source-Event darf nicht bereits verarbeitet sein
        if (sourceEvent.IsProcessed) return false;
        
        // 3. Betrag muss mit Source übereinstimmen
        if (request.Amount != sourceEvent.ExpectedAmount) return false;
        
        // 4. Character muss berechtigt sein
        if (sourceEvent.CharacterId != request.CharacterId) return false;
        
        return true;
    }
}
```

### Rate Limits

| Message | Limit | Cooldown |
|---------|-------|----------|
| CurrencyListRequest | 10/min | - |
| CurrencyExchange | 5/min | 10s |
| BountyPlace | 3/min | 30s |
| BountyClaim | 10/min | - |

### Plausibilitätsprüfungen

```csharp
public class EconomyPlausibility
{
    // Max Gold pro Transaktion
    public const long MAX_SINGLE_TRANSACTION = 10_000_000;
    
    // Max Gold pro Stunde (Anti-Bot)
    public const long MAX_GOLD_PER_HOUR = 100_000_000;
    
    // Verdächtige Aktivität loggen
    public void CheckSuspiciousActivity(long characterId, long amount)
    {
        var hourlyTotal = GetHourlyTotal(characterId);
        if (hourlyTotal + amount > MAX_GOLD_PER_HOUR)
        {
            LogSuspiciousActivity(characterId, "Excessive gold gain");
            // GM-Alert auslösen
        }
    }
}
```

---

## 📩 Aktive Messages 3700–3799

Die Messages sind in der Reihenfolge wie in `MessageType.cs` definiert.

---

## CurrencyUpdate (3700)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (bei jeder Balance-Änderung)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert Client über Änderung einer Währung. Wird nach jeder Balance-Änderung gesendet.

### Im Scope ✅

- Balance-Update für eine Währung
- Delta (Änderungsbetrag)
- Neue Revision für Sync

### Nicht im Scope ❌

- Mehrere Währungen gleichzeitig → mehrere Messages
- Transaction-Details → verwende `GoldTransaction` (3704)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CurrencyUpdate` | Ja |
| CurrencyId | ushort | Währungs-ID | Ja |
| NewBalance | long | Neuer Kontostand | Ja |
| Delta | long | Änderungsbetrag (+/-) | Ja |
| Revision | uint | Neue Wallet-Revision | Ja |
| Source | TransactionSourceType? | Ursprung der Änderung | Nein |

### Erwartete Response

- Keine (Server-Push)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CurrencyUpdate)]
public class CurrencyUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CurrencyUpdate;
    [Key(1)] public ushort CurrencyId { get; set; }
    [Key(2)] public long NewBalance { get; set; }
    [Key(3)] public long Delta { get; set; }
    [Key(4)] public uint Revision { get; set; }
    [Key(5)] public TransactionSourceType? Source { get; set; }
}
```

### Client-Verhalten

```csharp
public void OnCurrencyUpdate(CurrencyUpdate update)
{
    // 1. Lokales Wallet aktualisieren
    _wallet.SetBalance(update.CurrencyId, update.NewBalance, update.Revision);
    
    // 2. UI aktualisieren
    _currencyUI.UpdateDisplay(update.CurrencyId, update.NewBalance, update.Delta);
    
    // 3. Optional: Floating Text anzeigen
    if (update.Delta != 0)
    {
        var color = update.Delta > 0 ? Color.Green : Color.Red;
        _floatingText.Show($"{update.Delta:+#;-#} {GetCurrencySymbol(update.CurrencyId)}", color);
    }
}
```

### Beispiel Payload

```csharp
// Gold erhalten
var goldGain = new CurrencyUpdate
{
    CurrencyId = 1, // Gold
    NewBalance = 15000,
    Delta = +500,
    Revision = 42,
    Source = TransactionSourceType.QuestReward
};

// Gold ausgegeben
var goldSpent = new CurrencyUpdate
{
    CurrencyId = 1,
    NewBalance = 14500,
    Delta = -500,
    Revision = 43,
    Source = TransactionSourceType.VendorBuy
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `GoldUpdate` | 3703 | Spezialisiert für Gold |
| `GoldTransaction` | 3704 | Mit Transaction-Details |
| `CurrencyListResponse` | 3702 | Full Sync |

---

## CurrencyListRequest (3701)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (Login, Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert komplette Währungsliste mit Balances an.

### Im Scope ✅

- Full Wallet Sync
- Alle Währungen des Characters

### Nicht im Scope ❌

- Einzelne Währung abfragen

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CurrencyListRequest` | Ja |

### Erwartete Response

- `CurrencyListResponse` (3702)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CurrencyListRequest)]
public class CurrencyListRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.CurrencyListRequest;
}
```

### Server-Verhalten

```csharp
public void HandleCurrencyListRequest(ClientConnection conn)
{
    var character = GetCharacter(conn);
    var wallet = _walletService.GetWallet(character.Id);
    
    var response = new CurrencyListResponse
    {
        Currencies = _currencyService.GetAllDefinitions(),
        Balances = wallet.Entries,
        ServerTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };
    
    Send(conn, response);
}
```

---

## CurrencyListResponse (3702)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort mit komplettem Wallet-Snapshot.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CurrencyListResponse` | Ja |
| Currencies | List\<CurrencyDefinition\> | Verfügbare Währungen | Ja |
| Balances | List\<WalletEntry\> | Character-Balances | Ja |
| ServerTimeMs | long | Server-Zeit | Ja |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CurrencyListResponse)]
public class CurrencyListResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CurrencyListResponse;
    [Key(1)] public List<CurrencyDefinition> Currencies { get; set; } = new();
    [Key(2)] public List<WalletEntry> Balances { get; set; } = new();
    [Key(3)] public long ServerTimeMs { get; set; }
}
```

### Beispiel Payload

```csharp
var response = new CurrencyListResponse
{
    Currencies = new List<CurrencyDefinition>
    {
        new() { CurrencyId = 1, Name = "Gold", Symbol = "🪙", Precision = 0, IsTradeable = true },
        new() { CurrencyId = 2, Name = "Premium Coins", Symbol = "💎", Precision = 0, IsPremium = true },
        new() { CurrencyId = 10, Name = "Honor Points", Symbol = "⚔️", WeeklyCap = 5000 }
    },
    Balances = new List<WalletEntry>
    {
        new() { CurrencyId = 1, Balance = 15000, Revision = 42 },
        new() { CurrencyId = 2, Balance = 100, Revision = 5 },
        new() { CurrencyId = 10, Balance = 2500, WeeklyEarned = 2500, Revision = 15 }
    },
    ServerTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

---

## GoldUpdate (3703)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Spezialisierte Message für Gold-Updates. Enthält zusätzlich Breakdown (Copper, Silver, Gold).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.GoldUpdate` | Ja |
| TotalCopper | long | Gesamtbetrag in Copper | Ja |
| Delta | long | Änderung in Copper | Ja |
| Revision | uint | Wallet-Revision | Ja |

### Erwartete Response

- Keine (Server-Push)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GoldUpdate)]
public class GoldUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GoldUpdate;
    [Key(1)] public long TotalCopper { get; set; }
    [Key(2)] public long Delta { get; set; }
    [Key(3)] public uint Revision { get; set; }
    
    // Computed Properties
    [IgnoreMember] public int Gold => (int)(TotalCopper / 10000);
    [IgnoreMember] public int Silver => (int)(TotalCopper % 10000 / 100);
    [IgnoreMember] public int Copper => (int)(TotalCopper % 100);
}
```

---

## GoldTransaction (3704)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Detaillierte Gold-Transaktion mit Audit-Informationen.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.GoldTransaction` | Ja |
| TransactionId | Guid | Eindeutige Transaktions-ID | Ja |
| Amount | long | Betrag (+/-) | Ja |
| NewBalance | long | Neuer Kontostand | Ja |
| SourceType | TransactionSourceType | Ursprung | Ja |
| SourceReference | string? | Referenz-ID | Nein |
| TaxAmount | long? | Abgezogene Steuer | Nein |
| TimestampMs | long | Server-Zeit | Ja |

### Erwartete Response

- Keine (Server-Push)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GoldTransaction)]
public class GoldTransaction : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GoldTransaction;
    [Key(1)] public Guid TransactionId { get; set; }
    [Key(2)] public long Amount { get; set; }
    [Key(3)] public long NewBalance { get; set; }
    [Key(4)] public TransactionSourceType SourceType { get; set; }
    [Key(5)] public string? SourceReference { get; set; }
    [Key(6)] public long? TaxAmount { get; set; }
    [Key(7)] public long TimestampMs { get; set; }
}
```

### Beispiel Payload

```csharp
// Quest-Belohnung
var questReward = new GoldTransaction
{
    TransactionId = Guid.NewGuid(),
    Amount = +500,
    NewBalance = 15500,
    SourceType = TransactionSourceType.QuestReward,
    SourceReference = "quest_123",
    TimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

// Trade mit Steuer
var tradeSale = new GoldTransaction
{
    TransactionId = Guid.NewGuid(),
    Amount = +950, // 1000 - 5% Steuer
    NewBalance = 16450,
    SourceType = TransactionSourceType.Trade,
    SourceReference = "trade_456",
    TaxAmount = 50,
    TimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

---

## CurrencyExchange (3710)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte Währung tauschen (z.B. Gold → Premium Coins).

### Im Scope ✅

- Währungstausch zwischen unterstützten Paaren
- Rate-Berechnung Server-seitig

### Nicht im Scope ❌

- Echtgeld-Kauf → externe API

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CurrencyExchange` | Ja |
| FromCurrencyId | ushort | Quell-Währung | Ja |
| ToCurrencyId | ushort | Ziel-Währung | Ja |
| Amount | long | Menge der Quell-Währung | Ja |

### Erwartete Response

- `CurrencyExchangeResult` (3711)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CurrencyExchange)]
public class CurrencyExchange : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.CurrencyExchange;
    [Key(1)] public ushort FromCurrencyId { get; set; }
    [Key(2)] public ushort ToCurrencyId { get; set; }
    [Key(3)] public long Amount { get; set; }
}
```

### Server-Verhalten

```csharp
public void HandleCurrencyExchange(ClientConnection conn, CurrencyExchange request)
{
    var character = GetCharacter(conn);
    
    // Validierung
    if (!_exchangeService.CanExchange(request.FromCurrencyId, request.ToCurrencyId))
    {
        SendError(conn, EconomyErrorCode.ExchangeNotAvailable);
        return;
    }
    
    // Rate abrufen
    var rate = _exchangeService.GetRate(request.FromCurrencyId, request.ToCurrencyId);
    var receiveAmount = (long)(request.Amount * rate);
    
    // Transaktion durchführen
    var result = _walletService.Exchange(
        character.Id,
        request.FromCurrencyId, request.Amount,
        request.ToCurrencyId, receiveAmount
    );
    
    Send(conn, new CurrencyExchangeResult
    {
        Success = result.Success,
        ErrorCode = result.ErrorCode,
        FromAmount = request.Amount,
        ToAmount = receiveAmount,
        NewFromBalance = result.NewFromBalance,
        NewToBalance = result.NewToBalance
    });
}
```

---

## CurrencyExchangeResult (3711)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Ergebnis des Währungstauschs.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CurrencyExchangeResult` | Ja |
| Success | bool | Tausch erfolgreich? | Ja |
| ErrorCode | EconomyErrorCode | Fehlercode | Nein |
| FromAmount | long | Getauschte Menge | Bei Erfolg |
| ToAmount | long | Erhaltene Menge | Bei Erfolg |
| NewFromBalance | long | Neuer Quell-Kontostand | Bei Erfolg |
| NewToBalance | long | Neuer Ziel-Kontostand | Bei Erfolg |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CurrencyExchangeResult)]
public class CurrencyExchangeResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CurrencyExchangeResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public EconomyErrorCode ErrorCode { get; set; }
    [Key(3)] public long FromAmount { get; set; }
    [Key(4)] public long ToAmount { get; set; }
    [Key(5)] public long NewFromBalance { get; set; }
    [Key(6)] public long NewToBalance { get; set; }
}
```

---

## CurrencyCap (3712)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Informiert Client, dass ein Währungs-Cap erreicht wurde.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CurrencyCap` | Ja |
| CurrencyId | ushort | Betroffene Währung | Ja |
| CapType | CurrencyCapType | Art des Caps | Ja |
| CurrentAmount | long | Aktueller Stand | Ja |
| MaxAmount | long | Maximum | Ja |
| ResetTimeMs | long? | Wann wird Cap zurückgesetzt | Nein |

### Erwartete Response

- Keine (Server-Push)

### Code-Beispiel

```csharp
public enum CurrencyCapType : byte
{
    TotalCap = 0,
    WeeklyCap = 1,
    DailyCap = 2
}

[MessagePackObject]
[NetworkMessage(MessageType.CurrencyCap)]
public class CurrencyCap : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CurrencyCap;
    [Key(1)] public ushort CurrencyId { get; set; }
    [Key(2)] public CurrencyCapType CapType { get; set; }
    [Key(3)] public long CurrentAmount { get; set; }
    [Key(4)] public long MaxAmount { get; set; }
    [Key(5)] public long? ResetTimeMs { get; set; }
}
```

---

## TokenPurchase (3720)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client kauft einen Token (z.B. Game-Time Token).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TokenPurchase` | Ja |
| TokenType | ushort | Art des Tokens | Ja |
| Quantity | int | Anzahl | Ja |

### Erwartete Response

- `TokenPurchaseResult` (3721)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TokenPurchase)]
public class TokenPurchase : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TokenPurchase;
    [Key(1)] public ushort TokenType { get; set; }
    [Key(2)] public int Quantity { get; set; }
}
```

---

## TokenPurchaseResult (3721)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Ergebnis des Token-Kaufs.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TokenPurchaseResult` | Ja |
| Success | bool | Kauf erfolgreich? | Ja |
| ErrorCode | EconomyErrorCode | Fehlercode | Nein |
| TokenType | ushort | Gekaufter Token-Typ | Bei Erfolg |
| Quantity | int | Gekaufte Anzahl | Bei Erfolg |
| TotalCost | long | Bezahlter Betrag | Bei Erfolg |
| NewBalance | long | Neuer Kontostand | Bei Erfolg |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TokenPurchaseResult)]
public class TokenPurchaseResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TokenPurchaseResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public EconomyErrorCode ErrorCode { get; set; }
    [Key(3)] public ushort TokenType { get; set; }
    [Key(4)] public int Quantity { get; set; }
    [Key(5)] public long TotalCost { get; set; }
    [Key(6)] public long NewBalance { get; set; }
}
```

---

## TokenRedeem (3722)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client löst einen Token ein.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TokenRedeem` | Ja |
| TokenCode | string | Token-Code | Ja |

### Erwartete Response

- `TokenRedeemResult` (3723)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TokenRedeem)]
public class TokenRedeem : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TokenRedeem;
    [Key(1)] public string TokenCode { get; set; } = "";
}
```

---

## TokenRedeemResult (3723)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Ergebnis der Token-Einlösung.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TokenRedeemResult` | Ja |
| Success | bool | Einlösung erfolgreich? | Ja |
| ErrorCode | EconomyErrorCode | Fehlercode | Nein |
| RewardType | string | Art der Belohnung | Bei Erfolg |
| RewardAmount | long | Menge | Bei Erfolg |
| RewardDescription | string | Beschreibung | Bei Erfolg |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TokenRedeemResult)]
public class TokenRedeemResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TokenRedeemResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public EconomyErrorCode ErrorCode { get; set; }
    [Key(3)] public string? RewardType { get; set; }
    [Key(4)] public long RewardAmount { get; set; }
    [Key(5)] public string? RewardDescription { get; set; }
}
```

---

## PremiumCurrencyUpdate (3724)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Update für Premium-Währung (💎 Premium Coins).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PremiumCurrencyUpdate` | Ja |
| NewBalance | long | Neuer Kontostand | Ja |
| Delta | long | Änderung | Ja |
| Revision | uint | Wallet-Revision | Ja |
| Source | string? | Ursprung | Nein |

### Erwartete Response

- Keine (Server-Push)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PremiumCurrencyUpdate)]
public class PremiumCurrencyUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PremiumCurrencyUpdate;
    [Key(1)] public long NewBalance { get; set; }
    [Key(2)] public long Delta { get; set; }
    [Key(3)] public uint Revision { get; set; }
    [Key(4)] public string? Source { get; set; }
}
```

---

## BountyPlace (3730)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Spieler setzt Kopfgeld auf einen anderen Spieler aus.

### Im Scope ✅

- Kopfgeld auf PvP-Gegner setzen
- Gold als Belohnung

### Nicht im Scope ❌

- Kopfgeld auf NPCs
- Kopfgeld auf sich selbst

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BountyPlace` | Ja |
| TargetCharacterId | long | Ziel-Character | Ja |
| Amount | long | Kopfgeld-Betrag | Ja |
| CurrencyId | ushort | Währung (Standard: Gold) | Ja |

### Erwartete Response

- `BountyClaimResult` (3733) mit Success-Status

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BountyPlace)]
public class BountyPlace : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.BountyPlace;
    [Key(1)] public long TargetCharacterId { get; set; }
    [Key(2)] public long Amount { get; set; }
    [Key(3)] public ushort CurrencyId { get; set; } = 1; // Gold
}
```

### Server-Verhalten

```csharp
public void HandleBountyPlace(ClientConnection conn, BountyPlace request)
{
    var character = GetCharacter(conn);
    
    // Validierungen
    if (request.TargetCharacterId == character.Id)
    {
        SendError(conn, EconomyErrorCode.CannotBountyYourself);
        return;
    }
    
    if (request.Amount < MIN_BOUNTY_AMOUNT)
    {
        SendError(conn, EconomyErrorCode.InvalidBountyAmount);
        return;
    }
    
    // Gold abziehen
    var deductResult = _walletService.Deduct(character.Id, request.CurrencyId, request.Amount);
    if (!deductResult.Success)
    {
        SendError(conn, deductResult.ErrorCode);
        return;
    }
    
    // Bounty erstellen
    var bounty = _bountyService.Create(
        character.Id,
        request.TargetCharacterId,
        request.Amount,
        request.CurrencyId
    );
    
    // Bestätigung senden
    Send(conn, new BountyClaimResult
    {
        Success = true,
        BountyId = bounty.Id
    });
}
```

### Flow-Diagramm

```
Client (Placer)              Server                    Client (Target)
       │                        │                            │
       │  BountyPlace (3730)    │                            │
       │  Target: PlayerB       │                            │
       │  Amount: 1000 Gold     │                            │
       │───────────────────────►│                            │
       │                        │                            │
       │                        │  [Validate & Deduct Gold]  │
       │                        │                            │
       │  BountyClaimResult     │                            │
       │  Success: true         │                            │
       │◄───────────────────────│                            │
       │                        │                            │
       │                        │  [Broadcast: New Bounty]   │
       │                        │───────────────────────────►│
       │                        │                            │
       │  [Someone kills PlayerB]                            │
       │                        │                            │
       │                        │  BountyClaimResult         │
       │                        │  Reward: 1000 Gold         │
       │                        │──────────────────────────►│
```

---

## BountyList (3731)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Liste aller aktiven Kopfgelder.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BountyList` | Ja |
| Bounties | List\<BountyDto\> | Aktive Kopfgelder | Ja |
| TotalCount | int | Gesamtzahl | Ja |

### Erwartete Response

- Keine (Server-Push oder Response auf impliziten Request)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BountyList)]
public class BountyList : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BountyList;
    [Key(1)] public List<BountyDto> Bounties { get; set; } = new();
    [Key(2)] public int TotalCount { get; set; }
}
```

---

## BountyClaim (3732)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client beansprucht ein Kopfgeld nach Kill.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BountyClaim` | Ja |
| BountyId | Guid | Kopfgeld-ID | Ja |
| KillEventId | Guid | Referenz zum Kill-Event | Ja |

### Erwartete Response

- `BountyClaimResult` (3733)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BountyClaim)]
public class BountyClaim : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.BountyClaim;
    [Key(1)] public Guid BountyId { get; set; }
    [Key(2)] public Guid KillEventId { get; set; }
}
```

---

## BountyClaimResult (3733)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Ergebnis des Kopfgeld-Claims.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BountyClaimResult` | Ja |
| Success | bool | Claim erfolgreich? | Ja |
| ErrorCode | EconomyErrorCode | Fehlercode | Nein |
| BountyId | Guid | Kopfgeld-ID | Bei Erfolg |
| RewardAmount | long | Erhaltene Belohnung | Bei Erfolg |
| CurrencyId | ushort | Währung | Bei Erfolg |
| NewBalance | long | Neuer Kontostand | Bei Erfolg |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BountyClaimResult)]
public class BountyClaimResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BountyClaimResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public EconomyErrorCode ErrorCode { get; set; }
    [Key(3)] public Guid BountyId { get; set; }
    [Key(4)] public long RewardAmount { get; set; }
    [Key(5)] public ushort CurrencyId { get; set; }
    [Key(6)] public long NewBalance { get; set; }
}
```

---

## 🗑️ Obsolete Messages

Keine obsoleten Messages in dieser Kategorie.

---

## 🧨 Edge Cases & Fehlerfälle

### Concurrent Modifications

```csharp
// Problem: Zwei Transaktionen gleichzeitig
// Lösung: Optimistic Locking mit Revision

public class WalletService
{
    public TransactionResult TryTransaction(long characterId, long amount, uint expectedRevision)
    {
        using var transaction = _db.BeginTransaction(IsolationLevel.Serializable);
        
        var wallet = GetWallet(characterId);
        
        // Revision Check
        if (wallet.Revision != expectedRevision)
        {
            transaction.Rollback();
            return TransactionResult.Conflict;
        }
        
        // Ausführen
        wallet.Balance += amount;
        wallet.Revision++;
        
        transaction.Commit();
        return TransactionResult.Success;
    }
}
```

### Overflow Protection

```csharp
public bool CanAddAmount(long currentBalance, long amount)
{
    // Overflow Check
    if (amount > 0 && currentBalance > long.MaxValue - amount)
        return false;
    
    // Underflow Check
    if (amount < 0 && currentBalance < long.MinValue - amount)
        return false;
    
    // Negativ-Balance Check
    if (currentBalance + amount < 0)
        return false;
    
    return true;
}
```

### Duplicate Transaction Detection

```csharp
// Idempotenz: Gleiche TransactionId = gleiche Antwort
public TransactionResult ProcessWithIdempotency(Guid transactionId, Func<TransactionResult> action)
{
    // Check Cache
    if (_transactionCache.TryGet(transactionId, out var cached))
    {
        _metrics.IncrementDuplicateRequests();
        return cached;
    }
    
    // Execute
    var result = action();
    
    // Cache für 24h
    _transactionCache.Set(transactionId, result, TimeSpan.FromHours(24));
    
    return result;
}
```

### Weekly Cap Reset

```csharp
// Wöchentlicher Reset (Montag 00:00 UTC)
public class WeeklyCapResetJob
{
    public void Execute()
    {
        var currencies = GetCurrenciesWithWeeklyCap();
        
        foreach (var currency in currencies)
        {
            _db.Execute(@"
                UPDATE wallet_entries 
                SET weekly_earned = 0 
                WHERE currency_id = @CurrencyId",
                new { currency.CurrencyId });
        }
        
        // Broadcast Reset an alle Online-Spieler
        BroadcastToAll(new CurrencyUpdate { /* Reset Info */ });
    }
}
```

---

## 📎 Anhang

### Integration mit anderen Systemen

| System | Interaction |
|--------|-------------|
| Trading (11) | Gold-Transfer zwischen Spielern |
| Auction (17) | Gebote, Verkäufe, Deposits |
| Mail (18) | COD-Zahlungen, Gold-Attachments |
| Loot (31) | Gold-Drops von Mobs |
| Vendor/NPC (13) | Kauf/Verkauf |
| Quest (10) | Gold-Belohnungen |
| Guild (08) | Guild Bank Deposits/Withdrawals |

### MessageType Enum Updates

Die folgenden Messages sind bereits in `MessageType.cs` definiert:

```csharp
// ECONOMY / CURRENCY (3700-3799)
CurrencyUpdate = 3700,
CurrencyListRequest = 3701,
CurrencyListResponse = 3702,
GoldUpdate = 3703,
GoldTransaction = 3704,
CurrencyExchange = 3710,
CurrencyExchangeResult = 3711,
CurrencyCap = 3712,
TokenPurchase = 3720,
TokenPurchaseResult = 3721,
TokenRedeem = 3722,
TokenRedeemResult = 3723,
PremiumCurrencyUpdate = 3724,
BountyPlace = 3730,
BountyList = 3731,
BountyClaim = 3732,
BountyClaimResult = 3733,
```

### Datei-Struktur

```
Mmo.Shared/
├── Economy/
│   ├── Enums/
│   │   ├── TransactionSourceType.cs
│   │   ├── TaxDestination.cs
│   │   ├── CurrencyCapType.cs
│   │   └── EconomyErrorCode.cs
│   ├── Dtos/
│   │   ├── CurrencyDefinition.cs
│   │   ├── WalletEntry.cs
│   │   ├── CharacterWallet.cs
│   │   ├── TransactionRecord.cs
│   │   ├── TaxRule.cs
│   │   ├── CurrencyUpdateDto.cs
│   │   ├── GoldTransactionDto.cs
│   │   └── BountyDto.cs
│   └── Messages/
│       ├── CurrencyUpdate.cs
│       ├── CurrencyListRequest.cs
│       ├── CurrencyListResponse.cs
│       ├── GoldUpdate.cs
│       ├── GoldTransaction.cs
│       ├── CurrencyExchange.cs
│       ├── CurrencyExchangeResult.cs
│       ├── CurrencyCap.cs
│       ├── TokenPurchase.cs
│       ├── TokenPurchaseResult.cs
│       ├── TokenRedeem.cs
│       ├── TokenRedeemResult.cs
│       ├── PremiumCurrencyUpdate.cs
│       ├── BountyPlace.cs
│       ├── BountyList.cs
│       ├── BountyClaim.cs
│       └── BountyClaimResult.cs
```

---

**Letzte Aktualisierung**: 2026-01-03  
**Version**: 3.0.0  
**Status**: ✅ Vollständig dokumentiert (17 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/37-economy.md
