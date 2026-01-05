# 📬 Mail Messages (1800-1899)

**Kategorie:** 18  
**Range:** 1800-1816  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Mail Flow](#-mail-flow)
  - [Server-Authoritative Architecture](#server-authoritative-architecture)
  - [Mail Senden Flow](#mail-senden-flow)
  - [Mail Empfangen + Attachments Flow](#mail-empfangen--attachments-flow)
- [🧱 DTOs und Enums](#-dtos-und-enums)
  - [MailType Enum](#mailtype-enum)
  - [MailErrorCode Enum](#mailerrorcode-enum)
  - [MailHeaderDto](#mailheaderdto)
  - [MailContentDto](#mailcontentdto)
  - [MailAttachmentDto](#mailattachmentdto)
  - [Wichtige Konstanten](#wichtige-konstanten)
- [📩 Messages](#-messages)
  - [MailInboxRequest (1800)](#mailinboxrequest-1800)
  - [MailInboxResponse (1801)](#mailinboxresponse-1801)
  - [MailSend (1802)](#mailsend-1802)
  - [MailSendResult (1803)](#mailsendresult-1803)
  - [MailRead (1804)](#mailread-1804)
  - [MailMarkRead (1805)](#mailmarkread-1805)
  - [MailTakeAttachment (1806)](#mailtakeattachment-1806)
  - [MailTakeAttachmentResult (1807)](#mailtakeattachmentresult-1807)
  - [MailTakeGold (1808)](#mailtakegold-1808)
  - [MailTakeGoldResult (1809)](#mailtakegoldresult-1809)
  - [MailTakeAll (1810)](#mailtakeall-1810)
  - [MailDelete (1811)](#maildelete-1811)
  - [MailReturn (1812)](#mailreturn-1812)
  - [MailNotification (1813)](#mailnotification-1813)
  - [MailCashOnDelivery (1814)](#mailcashondelivery-1814)
  - [MailCashOnDeliveryPay (1815)](#mailcashondeliverypay-1815)
  - [MailExpired (1816)](#mailexpired-1816)
- [📎 Anhang](#-anhang)
  - [MessageType Enum (Code)](#messagetype-enum-code)
  - [Request/Response Paare](#requestresponse-paare)
  - [Datei-Struktur](#datei-struktur)

---

## 🔄 Mail Flow

### Server-Authoritative Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     MAIL SYSTEM - Server Authority                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│   Client A              Server                  Client B             │
│      │                    │                        │                 │
│      │  MailSend (1802)   │                        │                 │
│      │───────────────────►│                        │                 │
│      │                    │  Validate:             │                 │
│      │                    │  - Recipient exists    │                 │
│      │                    │  - Attachments valid   │                 │
│      │                    │  - Gold available      │                 │
│      │                    │  - COD amount ok       │                 │
│      │                    │                        │                 │
│      │  MailSendResult    │                        │                 │
│      │◄───────────────────│                        │                 │
│      │       (1803)       │                        │                 │
│      │                    │  MailNotification      │                 │
│      │                    │───────────────────────►│                 │
│      │                    │        (1813)          │                 │
│                                                                      │
│   SERVER ist AUTHORITATIVE für:                                      │
│   • Mail-Versand Validierung                                         │
│   • Attachment/Gold-Transfers                                        │
│   • COD-Zahlungen                                                    │
│   • Mail-Expiration (30 Tage)                                        │
│   • Inbox-Limits (50 Mails max)                                      │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Mail Senden Flow

```
Client                           Server                         Database
  │                                │                                │
  │  MailSend (1802)               │                                │
  │  [Recipient, Subject,          │                                │
  │   Body, Attachments?, Gold?,   │                                │
  │   COD?]                        │                                │
  │───────────────────────────────►│                                │
  │                                │  Validate Recipient            │
  │                                │───────────────────────────────►│
  │                                │                                │
  │                                │◄───────────────────────────────│
  │                                │                                │
  │                                │  IF Attachments:               │
  │                                │    Remove from Sender Inventory│
  │                                │  IF Gold:                      │
  │                                │    Deduct from Sender          │
  │                                │    (+ Posting Fee: 30c/mail)   │
  │                                │                                │
  │                                │  Store Mail in DB              │
  │                                │───────────────────────────────►│
  │                                │                                │
  │  MailSendResult (1803)         │                                │
  │  [Success, MailId, ErrorCode?] │                                │
  │◄───────────────────────────────│                                │
  │                                │                                │
  │                                │  IF Recipient Online:          │
  │                                │  MailNotification (1813)       │
  │                                │───────────────────────────────►│
  │                                │                        Recipient│
```

### Mail Empfangen + Attachments Flow

```
Client                           Server                         Database
  │                                │                                │
  │  MailInboxRequest (1800)       │                                │
  │───────────────────────────────►│                                │
  │                                │  Load Mail Headers             │
  │                                │───────────────────────────────►│
  │                                │                                │
  │  MailInboxResponse (1801)      │                                │
  │  [MailHeader[]]                │                                │
  │◄───────────────────────────────│                                │
  │                                │                                │
  │  MailRead (1804)               │                                │
  │  [MailId]                      │                                │
  │───────────────────────────────►│                                │
  │                                │  Load Full Mail Content        │
  │                                │───────────────────────────────►│
  │                                │                                │
  │  MailRead (1804)               │                                │
  │  [MailContent]                 │                                │
  │◄───────────────────────────────│                                │
  │                                │                                │
  │  MailTakeAll (1810)            │                                │
  │  [MailId]                      │                                │
  │───────────────────────────────►│                                │
  │                                │  IF COD: Check Gold            │
  │                                │  Transfer Items to Inventory   │
  │                                │  Transfer Gold to Wallet       │
  │                                │  IF COD: Send Gold to Sender   │
  │                                │                                │
  │  MailTakeAttachmentResult(1807)│                                │
  │  [Success, Items, Gold]        │                                │
  │◄───────────────────────────────│                                │
```

---

## 🧱 DTOs und Enums

### MailType Enum

```csharp
public enum MailType : byte
{
    Player = 0,        // Von Spieler zu Spieler
    System = 1,        // Systemnachricht (z.B. Auction outbid)
    Auction = 2,       // Auction House (Items/Gold)
    Guild = 3,         // Guild-bezogene Mails
    Gm = 4,            // GM/Admin Nachricht
    Calendar = 5       // Event-Einladungen
}
```

### MailErrorCode Enum

```csharp
public enum MailErrorCode : byte
{
    None = 0,
    RecipientNotFound = 1,
    RecipientIgnored = 2,
    InboxFull = 3,
    NotEnoughGold = 4,
    InvalidAttachment = 5,
    TooManyAttachments = 6,
    CannotMailToSelf = 7,
    SubjectTooLong = 8,
    BodyTooLong = 9,
    MailNotFound = 10,
    AttachmentNotFound = 11,
    InventoryFull = 12,
    CodNotAffordable = 13,
    MailExpired = 14,
    RateLimited = 15
}
```

### MailHeaderDto

```csharp
[MessagePackObject]
public class MailHeaderDto
{
    [Key(0)] public long MailId { get; set; }
    [Key(1)] public MailType Type { get; set; }
    [Key(2)] public string SenderName { get; set; }      // max 16 chars
    [Key(3)] public string Subject { get; set; }         // max 64 chars
    [Key(4)] public bool IsRead { get; set; }
    [Key(5)] public bool HasAttachments { get; set; }
    [Key(6)] public bool HasGold { get; set; }
    [Key(7)] public bool IsCod { get; set; }
    [Key(8)] public long CodAmount { get; set; }         // in Copper
    [Key(9)] public long ExpiresAt { get; set; }         // Unix timestamp
    [Key(10)] public long CreatedAt { get; set; }        // Unix timestamp
}
```

### MailContentDto

```csharp
[MessagePackObject]
public class MailContentDto
{
    [Key(0)] public long MailId { get; set; }
    [Key(1)] public MailType Type { get; set; }
    [Key(2)] public long SenderId { get; set; }
    [Key(3)] public string SenderName { get; set; }
    [Key(4)] public string Subject { get; set; }
    [Key(5)] public string Body { get; set; }            // max 500 chars
    [Key(6)] public List<MailAttachmentDto> Attachments { get; set; }
    [Key(7)] public long Gold { get; set; }              // in Copper
    [Key(8)] public bool IsCod { get; set; }
    [Key(9)] public long CodAmount { get; set; }
    [Key(10)] public long ExpiresAt { get; set; }
    [Key(11)] public long CreatedAt { get; set; }
}
```

### MailAttachmentDto

```csharp
[MessagePackObject]
public class MailAttachmentDto
{
    [Key(0)] public byte SlotIndex { get; set; }         // 0-11 (max 12 slots)
    [Key(1)] public int ItemId { get; set; }
    [Key(2)] public int Quantity { get; set; }
    [Key(3)] public byte[] ItemData { get; set; }        // Serialized item (enchants, sockets, etc.)
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `MAX_INBOX_SIZE` | 50 | Maximale Mails in Inbox |
| `MAX_ATTACHMENTS` | 12 | Max Attachments pro Mail |
| `MAX_GOLD_PER_MAIL` | 10000g | Max Gold pro Mail (1M Copper) |
| `MAIL_EXPIRY_DAYS` | 30 | Tage bis Mail abläuft |
| `MAIL_RETURN_DAYS` | 3 | Tage für Rücksendung abgelaufener Mail |
| `POSTING_FEE` | 30c | Gebühr pro gesendeter Mail |
| `COD_FEE_PERCENT` | 5 | Prozent COD-Gebühr |
| `MAX_SUBJECT_LENGTH` | 64 | Max Zeichen für Betreff |
| `MAX_BODY_LENGTH` | 500 | Max Zeichen für Nachricht |

---

## 📩 Messages

### MailInboxRequest (1800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Mailbox-Öffnung  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client fordert Liste aller Mails in der Inbox an. Gibt nur Header zurück (ohne Body/Attachments) für Performance.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `MailInboxResponse` (1801)
- **Bei Fehler:** `MailInboxResponse` (1801) mit leerer Liste

#### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `MailInboxResponse` | 1801 | Response |
| `MailRead` | 1804 | Details einer Mail laden |

---

### MailInboxResponse (1801)

**Richtung:** 📥 Server → Client  
**Frequenz:** Nach MailInboxRequest  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Liste aller Mail-Header in der Inbox des Spielers.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| Mails | MailHeaderDto[] | Liste der Mail-Header | Ja |
| TotalCount | int | Gesamtanzahl Mails | Ja |
| UnreadCount | int | Anzahl ungelesener Mails | Ja |

#### Beispiel Payload
```csharp
var response = new MailInboxResponse
{
    RequestId = 12345,
    Mails = new[]
    {
        new MailHeaderDto
        {
            MailId = 98765,
            Type = MailType.Player,
            SenderName = "FriendlyPlayer",
            Subject = "Check out this loot!",
            IsRead = false,
            HasAttachments = true,
            HasGold = false,
            IsCod = false,
            ExpiresAt = 1735776000
        }
    },
    TotalCount = 15,
    UnreadCount = 3
};
```

---

### MailSend (1802)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Mail-Versand  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client sendet eine neue Mail an einen anderen Spieler. Kann Text, Items und/oder Gold enthalten. Unterstützt Cash-on-Delivery (COD).

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| RecipientName | string | Empfänger-Name (max 16) | Ja |
| Subject | string | Betreff (max 64) | Ja |
| Body | string | Nachricht (max 500) | Nein |
| Attachments | AttachmentSlot[] | Item-Slots aus Inventory | Nein |
| Gold | long | Gold in Copper | Nein |
| IsCod | bool | Cash-on-Delivery aktiviert? | Nein |
| CodAmount | long | COD-Betrag in Copper | Wenn IsCod |

#### Validierung (Server)
- Empfänger existiert und hat Client nicht blockiert
- Empfänger-Inbox nicht voll (< 50 Mails)
- Absender hat genug Gold (Gold + PostingFee)
- Attachments sind valide und im Inventory
- COD-Betrag <= MAX_GOLD_PER_MAIL
- Kein Self-Mail

#### Erwartete Response
- `MailSendResult` (1803)

---

### MailSendResult (1803)

**Richtung:** 📥 Server → Client  
**Frequenz:** Nach MailSend  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt oder verweigert Mail-Versand.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| Success | bool | Versand erfolgreich? | Ja |
| MailId | long | ID der gesendeten Mail | Bei Erfolg |
| ErrorCode | MailErrorCode | Fehlercode | Bei Fehler |
| GoldDeducted | long | Abgezogenes Gold (inkl. Gebühr) | Bei Erfolg |

---

### MailRead (1804)

**Richtung:** 📤 Client → Server / 📥 Server → Client  
**Frequenz:** Bei Mail-Öffnung  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Bidirektionale Message: Client sendet Request mit MailId, Server antwortet mit vollem Mail-Content.

#### Request Payload (C→S)
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| MailId | long | ID der zu lesenden Mail | Ja |

#### Response Payload (S→C)
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| Success | bool | Mail gefunden? | Ja |
| Content | MailContentDto | Vollständiger Inhalt | Bei Erfolg |
| ErrorCode | MailErrorCode | Fehlercode | Bei Fehler |

#### Seiteneffekt
- Mail wird automatisch als "gelesen" markiert

---

### MailMarkRead (1805)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (Batch-Markierung)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client markiert eine oder mehrere Mails als gelesen ohne den Inhalt zu laden.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MailIds | long[] | IDs der zu markierenden Mails | Ja |

#### Erwartete Response
- Keine explizite Response (Fire-and-Forget)
- Bei Fehler: Keine Änderung

---

### MailTakeAttachment (1806)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Attachment-Entnahme  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client nimmt ein einzelnes Attachment aus einer Mail. Prüft Inventory-Platz.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| MailId | long | Mail-ID | Ja |
| SlotIndex | byte | Attachment-Slot (0-11) | Ja |

#### Erwartete Response
- `MailTakeAttachmentResult` (1807)

---

### MailTakeAttachmentResult (1807)

**Richtung:** 📥 Server → Client  
**Frequenz:** Nach MailTakeAttachment  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt Attachment-Entnahme oder meldet Fehler.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| Success | bool | Entnahme erfolgreich? | Ja |
| MailId | long | Mail-ID | Ja |
| SlotIndex | byte | Entnommener Slot | Ja |
| ItemId | int | Item-ID (für Client-Sync) | Bei Erfolg |
| Quantity | int | Entnommene Menge | Bei Erfolg |
| InventorySlot | int | Ziel-Slot im Inventory | Bei Erfolg |
| ErrorCode | MailErrorCode | Fehlercode | Bei Fehler |

---

### MailTakeGold (1808)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Gold-Entnahme  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client nimmt Gold aus einer Mail.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| MailId | long | Mail-ID | Ja |

#### Erwartete Response
- `MailTakeGoldResult` (1809)

---

### MailTakeGoldResult (1809)

**Richtung:** 📥 Server → Client  
**Frequenz:** Nach MailTakeGold  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt Gold-Entnahme.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| Success | bool | Entnahme erfolgreich? | Ja |
| MailId | long | Mail-ID | Ja |
| GoldReceived | long | Erhaltenes Gold (Copper) | Bei Erfolg |
| NewBalance | long | Neuer Kontostand | Bei Erfolg |
| ErrorCode | MailErrorCode | Fehlercode | Bei Fehler |

---

### MailTakeAll (1810)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei "Alles nehmen"  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client nimmt alle Attachments und Gold aus einer Mail in einem Request. Bei COD-Mails wird automatisch bezahlt.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| MailId | long | Mail-ID | Ja |

#### Validierung (Server)
- Ausreichend Inventory-Platz für alle Items
- Bei COD: Ausreichend Gold für Zahlung
- Mail existiert und gehört dem Spieler

#### Erwartete Response
- `MailTakeAttachmentResult` (1807) mit aggregierten Daten

---

### MailDelete (1811)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Mail-Löschung  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client löscht eine Mail. Attachments/Gold müssen vorher entnommen sein.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MailIds | long[] | IDs der zu löschenden Mails | Ja |

#### Validierung
- Mails haben keine Attachments oder Gold mehr
- Alternative: Bei COD-Mails ohne Zahlung → automatisch Return

#### Erwartete Response
- Keine explizite Response (Fire-and-Forget)
- Bei Fehler: Mail bleibt erhalten

---

### MailReturn (1812)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client sendet Mail mit Attachments/Gold zurück an Absender.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| MailId | long | Mail-ID | Ja |

#### Validierung
- Mail hat Attachments oder Gold
- Mail ist von Typ Player (nicht System/Auction)
- Absender-Inbox nicht voll

#### Erwartete Response
- `MailSendResult` (1803) mit neuer MailId für Return-Mail

---

### MailNotification (1813)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei neuer Mail (wenn online)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server informiert Client über neue Mail. Enthält Header für UI-Benachrichtigung.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MailHeader | MailHeaderDto | Header der neuen Mail | Ja |
| NewUnreadCount | int | Neue Anzahl ungelesener Mails | Ja |

#### Client-Verhalten
- UI-Notification anzeigen
- Mail-Icon aktualisieren
- Optional: Sound abspielen

---

### MailCashOnDelivery (1814)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei COD-Mail Details  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Details zu einer COD-Mail wenn Client sie öffnet. Zeigt Zahlungsanforderung.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MailId | long | Mail-ID | Ja |
| CodAmount | long | Zu zahlender Betrag (Copper) | Ja |
| SenderName | string | Name des Verkäufers | Ja |
| ItemPreviews | ItemPreview[] | Vorschau der Items | Ja |

---

### MailCashOnDeliveryPay (1815)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei COD-Zahlung  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client zahlt COD-Betrag und erhält Items. Gold wird an Absender weitergeleitet (minus 5% Gebühr).

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Correlation ID | Ja |
| MailId | long | Mail-ID | Ja |

#### Validierung
- Client hat genug Gold
- Inventory hat Platz für Items

#### Erwartete Response
- `MailTakeAttachmentResult` (1807) mit Erfolg/Fehler

#### Seiteneffekte
- Gold wird an Absender gesendet (als neue Mail)
- COD-Gebühr (5%) wird abgezogen
- Items werden in Inventory transferiert

---

### MailExpired (1816)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Mail-Ablauf  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server informiert Client dass eine Mail abgelaufen ist. Bei Attachments/Gold wird Mail automatisch an Absender zurückgesendet.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MailId | long | Abgelaufene Mail-ID | Ja |
| WasReturned | bool | Wurde zurückgesendet? | Ja |
| Reason | string | "Expired" / "InboxFull" | Ja |

---

## 📎 Anhang

### MessageType Enum (Code)

```csharp
// ═══════════════════════════════════════════════════════════════
// MAIL SYSTEM (1800-1899)
// ═══════════════════════════════════════════════════════════════
MailInboxRequest = 1800,
MailInboxResponse = 1801,
MailSend = 1802,
MailSendResult = 1803,
MailRead = 1804,
MailMarkRead = 1805,
MailTakeAttachment = 1806,
MailTakeAttachmentResult = 1807,
MailTakeGold = 1808,
MailTakeGoldResult = 1809,
MailTakeAll = 1810,
MailDelete = 1811,
MailReturn = 1812,
MailNotification = 1813,
MailCashOnDelivery = 1814,
MailCashOnDeliveryPay = 1815,
MailExpired = 1816,
```

### Request/Response Paare

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| MailInboxRequest | 1800 | MailInboxResponse | 1801 |
| MailSend | 1802 | MailSendResult | 1803 |
| MailRead | 1804 | MailRead | 1804 |
| MailTakeAttachment | 1806 | MailTakeAttachmentResult | 1807 |
| MailTakeGold | 1808 | MailTakeGoldResult | 1809 |
| MailTakeAll | 1810 | MailTakeAttachmentResult | 1807 |
| MailReturn | 1812 | MailSendResult | 1803 |
| MailCashOnDeliveryPay | 1815 | MailTakeAttachmentResult | 1807 |

**Fire-and-Forget Messages (keine Response):**
- MailMarkRead (1805)
- MailDelete (1811)

**Server-initiated (Push):**
- MailNotification (1813)
- MailCashOnDelivery (1814)
- MailExpired (1816)

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   └── MessageType.cs          # Mail = 1800-1816
│   └── Messages/
│       └── Mail/
│           ├── MailInboxRequest.cs
│           ├── MailInboxResponse.cs
│           ├── MailSend.cs
│           ├── MailSendResult.cs
│           ├── MailRead.cs
│           ├── MailMarkRead.cs
│           ├── MailTakeAttachment.cs
│           ├── MailTakeAttachmentResult.cs
│           ├── MailTakeGold.cs
│           ├── MailTakeGoldResult.cs
│           ├── MailTakeAll.cs
│           ├── MailDelete.cs
│           ├── MailReturn.cs
│           ├── MailNotification.cs
│           ├── MailCashOnDelivery.cs
│           ├── MailCashOnDeliveryPay.cs
│           └── MailExpired.cs
└── Dtos/
    └── Mail/
        ├── MailHeaderDto.cs
        ├── MailContentDto.cs
        └── MailAttachmentDto.cs
```

---

**Letzte Aktualisierung:** 2026-01-02  
**Version:** 3.0.0  
**Status:** ✅ Aligned mit MessageType Enum (17 Messages: 1800-1816)

[← Zurück zur Übersicht](README.md)
