# 📝 Reporting / Moderation Messages (3600-3699)

**Kategorie:** 36  
**Range:** 3600-3699  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [Überblick](#-überblick)
- [Datenmodell](#-datenmodell)
  - [ReportTicket](#reportticket)
  - [ReportTarget](#reporttarget)
  - [ReportEvidence](#reportevidence)
  - [FeedbackEntry](#feedbackentry)
- [Privacy & Safety](#-privacy--safety)
- [Evidence & Attachments](#-evidence--attachments)
- [Status & Notifications](#-status--notifications)
- [DTOs / Interfaces](#-dtos--interfaces)
- [Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [Regeln & Validierung](#-regeln--validierung)
- [Aktive Messages 3600-3699](#-aktive-messages-3600-3699)
  - [ReportPlayer (3600)](#reportplayer-3600)
  - [ReportPlayerResult (3601)](#reportplayerresult-3601)
  - [ReportChat (3602)](#reportchat-3602)
  - [ReportChatResult (3603)](#reportchatresult-3603)
  - [ReportBug (3604)](#reportbug-3604)
  - [ReportBugResult (3605)](#reportbugresult-3605)
  - [ReportSuggestion (3606)](#reportsuggestion-3606)
  - [ReportSuggestionResult (3607)](#reportsuggestionresult-3607)
  - [ReportExploit (3608)](#reportexploit-3608)
  - [ReportExploitResult (3609)](#reportexploitresult-3609)
  - [AppealRequest (3610)](#appealrequest-3610)
  - [AppealResult (3611)](#appealresult-3611)
  - [ReportStatusRequest (3612)](#reportstatusrequest-3612)
  - [ReportStatusResponse (3613)](#reportstatusresponse-3613)
  - [ReportEvidenceAdd (3614)](#reportevidenceadd-3614)
  - [ReportEvidenceAddResult (3615)](#reportevidenceaddresult-3615)
  - [ModerationAction (3620)](#moderationaction-3620)
  - [ModerationWarning (3621)](#moderationwarning-3621)
  - [ModerationMute (3622)](#moderationmute-3622)
  - [ModerationBan (3623)](#moderationban-3623)
  - [FeedbackPrompt (3630)](#feedbackprompt-3630)
  - [FeedbackSubmit (3631)](#feedbacksubmit-3631)
  - [FeedbackSubmitResult (3632)](#feedbacksubmitresult-3632)
  - [SurveyShow (3633)](#surveyshow-3633)
  - [SurveySubmit (3634)](#surveysubmit-3634)
  - [SurveySubmitResult (3635)](#surveysubmitresult-3635)
  - [RatingPrompt (3636)](#ratingprompt-3636)
  - [RatingSubmit (3637)](#ratingsubmit-3637)
  - [RatingSubmitResult (3638)](#ratingsubmitresult-3638)
  - [ReportReceivedEvent (3640)](#reportreceivedevent-3640)
- [Obsolete Messages](#-obsolete-messages)
- [Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [Anhang](#-anhang)

---

## 📋 Überblick

Das Reporting-System ermöglicht Spielern das Melden von Regelverstößen, Bugs und das Einreichen von Feedback. **Der Server ist authoritative** – alle Reports werden serverseitig validiert, dedupliziert und in eine Moderation-Pipeline weitergeleitet.

### Scope

| Feature | Beschreibung |
|---------|--------------|
| **Player Reports** | Meldung von Spielern (Harassment, Cheating, Griefing, unangemessene Namen) |
| **Chat Reports** | Meldung von Chat-Nachrichten mit Server-seitiger Message-ID |
| **Bug Reports** | Technische Fehlerberichte mit Reproduktionsschritten |
| **Exploit Reports** | Hochpriorisierte Sicherheitsmeldungen |
| **Feedback** | Allgemeines Spieler-Feedback |
| **Surveys** | Server-initiierte Umfragen |
| **Ratings** | NPS-artige Bewertungen |
| **Appeals** | Einsprüche gegen Moderationsmaßnahmen |

### Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│  Client                                                         │
│  ├── ReportUI (Formular, Evidence-Auswahl)                     │
│  ├── FeedbackUI (Prompts, Surveys)                             │
│  └── ModerationNotifications (Warnings, Bans)                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ Reports (Player, Chat, Bug, Exploit)
┌─────────────────────────────────────────────────────────────────┐
│  Server (Authoritative)                                         │
│  ├── ReportService (Validation, Deduplication, Rate Limiting)  │
│  ├── ModerationQueue (Ticket-System, Priority)                 │
│  ├── EvidenceService (Server-side ID References)               │
│  └── NotificationService (Warnings, Actions)                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ Moderation Actions
┌─────────────────────────────────────────────────────────────────┐
│  Moderation Pipeline                                            │
│  ├── Auto-Moderation (Spam-Detection, Profanity)               │
│  ├── Manual Review Queue                                        │
│  └── Appeal Processing                                          │
└─────────────────────────────────────────────────────────────────┘
```

### Report Flow (Übersicht)

```
Client                         Server                    Mod Queue
  │                              │                           │
  │  ReportPlayer (3600)         │                           │
  │  TargetCharacterId:  xyz      │                           │
  │  ReportType:  Harassment      │                           │
  │  Evidence: [ChatMsgIds]      │                           │
  │─────────────────────────────►│                           │
  │                              │  Validate                 │
  │                              │  Deduplicate              │
  │                              │  Create Ticket            │
  │                              │──────────────────────────►│
  │                              │                           │
  │  ReportPlayerResult (3601)   │                           │
  │  Success: true               │                           │
  │  TicketId: ABC123            │                           │
  │◄─────────────────────────────│                           │
  │                              │                           │
  │  ReportReceivedEvent (3640)  │                           │
  │  (Optional Ack)              │                           │
  │◄─────────────────────────────│                           │
```

---

## 🧠 Datenmodell

### ReportTicket

Server-seitiges Ticket für einen Report. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| TicketId | string | Eindeutige Ticket-ID (z.B. "RPT-2024-ABC123") |
| ReporterId | long | Account-ID des Meldenden |
| ReporterCharacterId | long | Character-ID des Meldenden |
| ReportType | ReportType | Art des Reports |
| Status | ReportStatus | Aktueller Status |
| Priority | ReportPriority | Priorität (Normal, High, Critical) |
| CreatedAt | long | Unix Timestamp |
| UpdatedAt | long | Unix Timestamp |
| Target | ReportTarget | Gemeldetes Ziel |
| Evidence | List\<ReportEvidence\> | Beweismaterial |
| Description | string | Beschreibung (max. 2000 Zeichen) |
| AssignedModerator | string?  | Zugewiesener Moderator |
| Resolution | ReportResolution?  | Ergebnis |
| ResolutionNote | string? | Interne Notiz |

### ReportTarget

Das gemeldete Ziel (Spieler, Chat-Nachricht, etc.).

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| TargetType | ReportTargetType | Player, ChatMessage, Bug, Exploit |
| TargetAccountId | long?  | Account-ID (bei Player-Report) |
| TargetCharacterId | long? | Character-ID (bei Player-Report) |
| TargetCharacterName | string? | Character-Name zum Zeitpunkt |
| ChatMessageIds | List\<Guid\>? | Server-seitige Chat-Message-IDs |
| ZoneId | ushort? | Zone des Vorfalls |
| Position | Position? | Position des Vorfalls |
| Timestamp | long | Zeitpunkt des Vorfalls |

### ReportEvidence

Beweismaterial für einen Report. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| EvidenceId | Guid | Eindeutige Evidence-ID |
| EvidenceType | EvidenceType | ChatLog, CombatLog, Screenshot, Custom |
| ReferenceIds | List\<Guid\> | Server-seitige IDs (ChatMessageId, CombatLogEntryId) |
| Description | string?  | Optionale Beschreibung (max. 500 Zeichen) |
| CreatedAt | long | Unix Timestamp |

### FeedbackEntry

Feedback-Eintrag. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| FeedbackId | Guid | Eindeutige Feedback-ID |
| AccountId | long | Account-ID |
| FeedbackType | FeedbackType | General, Feature, Balance, UI, Performance |
| Category | string | Kategorie |
| Content | string | Inhalt (max. 5000 Zeichen) |
| Rating | byte?  | Optional:  1-5 Sterne |
| ClientVersion | string | Client-Version |
| CreatedAt | long | Unix Timestamp |

---

## 🔒 Privacy & Safety

### PII-Schutz (Personally Identifiable Information)

| Regel | Beschreibung |
|-------|--------------|
| **Keine IP-Adressen** | Client sendet NIEMALS IP-Adressen in Reports |
| **Keine Passwörter** | Keine Credentials in Descriptions erlaubt |
| **Server-Side IDs** | Nur Server-seitige IDs für Evidence (keine Raw-Logs) |
| **Sanitization** | Server sanitized alle Text-Felder |
| **Retention** | Reports werden nach 90 Tagen anonymisiert |

### Retaliation Protection

| Schutzmaßnahme | Beschreibung |
|----------------|--------------|
| **Anonymität** | Gemeldete Spieler sehen NICHT wer sie gemeldet hat |
| **Keine Echtzeit-Updates** | Keine sofortigen Status-Updates an Reporter |
| **Batched Notifications** | Moderationsaktionen werden verzögert kommuniziert |
| **Anti-Abuse** | Rate Limiting verhindert Report-Spam gegen einzelne Spieler |

### Abuse Prevention

```csharp
// Server-seitige Validierung
public class ReportValidator
{
    // Max Reports pro Spieler pro Stunde
    private const int MaxReportsPerHour = 5;
    
    // Max Reports gegen denselben Spieler pro Tag
    private const int MaxReportsAgainstSamePlayer = 2;
    
    // Cooldown nach erfolgreichem Report
    private const int ReportCooldownSeconds = 60;
    
    // Min.  Account-Alter für Reports
    private const int MinAccountAgeDays = 3;
}
```

---

## 🧾 Evidence & Attachments

### Evidence-Typen

| Typ | Beschreibung | Max. Anzahl |
|-----|--------------|-------------|
| `ChatLog` | Chat-Message-IDs (Server-seitig) | 50 IDs |
| `CombatLog` | Combat-Log-Entry-IDs | 100 IDs |
| `Screenshot` | Screenshot-Reference (Server-validiert) | 3 |
| `Position` | Zone + Position zum Zeitpunkt | 1 |
| `Custom` | Freitext-Beschreibung | 1 |

### Evidence-Regeln

| Regel | Beschreibung |
|-------|--------------|
| **Server-Side IDs Only** | Client sendet nur IDs, Server holt die Daten |
| **Time-Bounded** | Evidence muss aus den letzten 24h stammen |
| **Ownership** | Reporter muss Witness der Evidence sein |
| **No Raw Data** | Keine Chat-Texte, keine Log-Inhalte vom Client |

### Evidence Flow

```
Client                         Server
  │                              │
  │  [Player selects chat msgs]  │
  │  [UI shows last 50 msgs]     │
  │                              │
  │  ReportChat (3602)           │
  │  ChatMessageIds: [id1, id2]  │  ← Nur Server-IDs! 
  │─────────────────────────────►│
  │                              │
  │                              │  Validate IDs exist
  │                              │  Validate reporter was in channel
  │                              │  Fetch actual content (server-side)
  │                              │  Store with ticket
  │                              │
  │  ReportChatResult (3603)     │
  │◄─────────────────────────────│
```

---

## 🔄 Status & Notifications

### Report Status Lifecycle

```
┌─────────┐    ┌──────────┐    ┌─────────────┐    ┌──────────┐
│ Pending │───►│ Assigned │───►│ In Progress │───►│ Resolved │
└─────────┘    └──────────┘    └─────────────┘    └──────────┘
     │              │                │                  │
     │              │                │                  │
     ▼              ▼                ▼                  ▼
┌──────────┐  ┌───────────┐   ┌───────────┐    ┌────────────┐
│Duplicate │  │ Escalated │   │ On Hold   │    │ Appealed   │
└──────────┘  └───────────┘   └───────────┘    └────────────┘
```

### Status-Beschreibungen

| Status | Beschreibung | Sichtbar für Reporter |
|--------|--------------|----------------------|
| `Pending` | Report eingegangen, wartet auf Review | ✅ "Received" |
| `Assigned` | Moderator zugewiesen | ❌ |
| `InProgress` | Wird bearbeitet | ❌ |
| `OnHold` | Wartet auf zusätzliche Info | ✅ "Under Review" |
| `Escalated` | An höhere Instanz eskaliert | ❌ |
| `Resolved` | Abgeschlossen | ✅ "Resolved" |
| `Duplicate` | Bereits gemeldet | ✅ "Already Reported" |
| `Appealed` | Einspruch eingereicht | ✅ (nur bei eigenen Strafen) |

### Notification Policy

| Event | Notification an Reporter |
|-------|--------------------------|
| Report eingegangen | Sofort:  `ReportReceivedEvent` |
| Status-Änderung | Nein (Privacy) |
| Report abgeschlossen | Nach 24-48h Delay:  Generische Nachricht |
| Action gegen Gemeldeten | Niemals (Privacy) |

---

## 🧱 DTOs / Interfaces

### ReportTicketDto

```csharp
[MessagePackObject]
public class ReportTicketDto
{
    [Key(0)] public string TicketId { get; set; } = "";
    [Key(1)] public ReportType ReportType { get; set; }
    [Key(2)] public ReportStatus Status { get; set; }
    [Key(3)] public long CreatedAt { get; set; }
    [Key(4)] public long UpdatedAt { get; set; }
    // Note: Target details NOT included for privacy
}
```

### ReportTargetDto

```csharp
[MessagePackObject]
public class ReportTargetDto
{
    [Key(0)] public ReportTargetType TargetType { get; set; }
    [Key(1)] public long? TargetCharacterId { get; set; }
    [Key(2)] public string?  TargetCharacterName { get; set; }
    [Key(3)] public List<Guid>? ChatMessageIds { get; set; }
    [Key(4)] public ushort? ZoneId { get; set; }
    [Key(5)] public Position? Position { get; set; }
    [Key(6)] public long Timestamp { get; set; }
}
```

### ReportEvidenceDto

```csharp
[MessagePackObject]
public class ReportEvidenceDto
{
    [Key(0)] public EvidenceType EvidenceType { get; set; }
    [Key(1)] public List<Guid> ReferenceIds { get; set; } = new();
    [Key(2)] public string? Description { get; set; }
}
```

### FeedbackDto

```csharp
[MessagePackObject]
public class FeedbackDto
{
    [Key(0)] public FeedbackType FeedbackType { get; set; }
    [Key(1)] public string Category { get; set; } = "";
    [Key(2)] public string Content { get; set; } = "";
    [Key(3)] public byte? Rating { get; set; }
}
```

### SurveyDto

```csharp
[MessagePackObject]
public class SurveyDto
{
    [Key(0)] public Guid SurveyId { get; set; }
    [Key(1)] public string Title { get; set; } = "";
    [Key(2)] public List<SurveyQuestionDto> Questions { get; set; } = new();
    [Key(3)] public long ExpiresAt { get; set; }
}

[MessagePackObject]
public class SurveyQuestionDto
{
    [Key(0)] public int QuestionId { get; set; }
    [Key(1)] public string QuestionText { get; set; } = "";
    [Key(2)] public SurveyQuestionType QuestionType { get; set; }
    [Key(3)] public List<string>? Options { get; set; }
    [Key(4)] public bool Required { get; set; }
}

[MessagePackObject]
public class SurveyAnswerDto
{
    [Key(0)] public int QuestionId { get; set; }
    [Key(1)] public string?  TextAnswer { get; set; }
    [Key(2)] public int?  SelectedOption { get; set; }
    [Key(3)] public List<int>? SelectedOptions { get; set; }
    [Key(4)] public byte? RatingAnswer { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### ReportType

```csharp
public enum ReportType : byte
{
    // Player Reports
    Harassment = 1,
    Cheating = 2,
    Griefing = 3,
    InappropriateName = 4,
    Botting = 5,
    RealMoneyTrading = 6,
    AccountSharing = 7,
    Impersonation = 8,
    
    // Chat Reports
    ChatAbuse = 20,
    Spam = 21,
    Advertising = 22,
    
    // Technical
    Bug = 40,
    Exploit = 41,
    PerformanceIssue = 42,
    
    // Feedback
    Suggestion = 60,
    BalanceFeedback = 61,
    UIFeedback = 62
}
```

### ReportTargetType

```csharp
public enum ReportTargetType :  byte
{
    Player = 1,
    ChatMessage = 2,
    Bug = 3,
    Exploit = 4,
    General = 5
}
```

### EvidenceType

```csharp
public enum EvidenceType : byte
{
    ChatLog = 1,
    CombatLog = 2,
    Screenshot = 3,
    Position = 4,
    Custom = 5
}
```

### ReportStatus

```csharp
public enum ReportStatus : byte
{
    Pending = 0,
    Assigned = 1,
    InProgress = 2,
    OnHold = 3,
    Escalated = 4,
    Resolved = 5,
    Duplicate = 6,
    Appealed = 7,
    Rejected = 8
}
```

### ReportPriority

```csharp
public enum ReportPriority : byte
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3  // Exploits, severe harassment
}
```

### ReportResolution

```csharp
public enum ReportResolution : byte
{
    NoAction = 0,
    Warning = 1,
    Mute = 2,
    TempBan = 3,
    PermBan = 4,
    NameChange = 5,
    ItemRemoval = 6,
    RollbackAction = 7,
    Duplicate = 8,
    InsufficientEvidence = 9,
    FalseReport = 10
}
```

### FeedbackType

```csharp
public enum FeedbackType : byte
{
    General = 0,
    Feature = 1,
    Balance = 2,
    UI = 3,
    Performance = 4,
    Content = 5,
    Social = 6
}
```

### ModerationActionType

```csharp
public enum ModerationActionType : byte
{
    Warning = 1,
    Mute = 2,
    Kick = 3,
    TempBan = 4,
    PermBan = 5,
    ForcedNameChange = 6,
    ChatRestriction = 7,
    TradeRestriction = 8
}
```

### ReportErrorCode

```csharp
public enum ReportErrorCode : byte
{
    None = 0,
    InvalidTarget = 1,
    TargetNotFound = 2,
    SelfReport = 3,
    DuplicateReport = 4,
    RateLimited = 5,
    AccountTooNew = 6,
    InvalidEvidence = 7,
    EvidenceExpired = 8,
    DescriptionTooLong = 9,
    DescriptionTooShort = 10,
    InvalidReportType = 11,
    TargetImmune = 12,  // GMs, etc. 
    Cooldown = 13,
    MaxReportsReached = 14,
    InvalidTicketId = 15,
    AppealNotAllowed = 16,
    AlreadyAppealed = 17,
    SurveyExpired = 18,
    SurveyAlreadyCompleted = 19
}
```

### SurveyQuestionType

```csharp
public enum SurveyQuestionType : byte
{
    Text = 1,
    SingleChoice = 2,
    MultipleChoice = 3,
    Rating = 4,  // 1-5 or 1-10
    NPS = 5      // 0-10 Net Promoter Score
}
```

---

## ⚙️ Regeln & Validierung

### Rate Limits

| Operation | Limit | Cooldown |
|-----------|-------|----------|
| `ReportPlayer` | 5/Stunde | 60s nach Report |
| `ReportChat` | 10/Stunde | 30s nach Report |
| `ReportBug` | 20/Tag | - |
| `ReportExploit` | Unlimited | - |
| `FeedbackSubmit` | 5/Tag | - |
| `AppealRequest` | 1 pro Strafe | - |

### Validierungsregeln

| Regel | Beschreibung |
|-------|--------------|
| **Account Age** | Min. 3 Tage für Player-Reports |
| **Same Target** | Max. 2 Reports gegen denselben Spieler/Tag |
| **Description Length** | Min. 20, Max. 2000 Zeichen |
| **Evidence Age** | Max. 24h alt |
| **Self-Report** | Nicht erlaubt |
| **GM Immunity** | GMs können nicht gemeldet werden |

### Deduplication

```csharp
public bool IsDuplicate(ReportRequest newReport)
{
    // Prüfe auf existierenden Report: 
    // - Selber Reporter
    // - Selbes Target
    // - Selber ReportType
    // - Innerhalb der letzten 24h
    // - Status != Resolved/Rejected
    
    return _reportRepository. Exists(r =>
        r.ReporterId == newReport.ReporterId &&
        r.Target. TargetCharacterId == newReport. TargetCharacterId &&
        r.ReportType == newReport.ReportType &&
        r.CreatedAt > DateTimeOffset.UtcNow.AddHours(-24).ToUnixTimeSeconds() &&
        r.Status != ReportStatus. Resolved &&
        r.Status != ReportStatus.Rejected
    );
}
```

### Idempotency

Reports verwenden einen `ClientRequestId` für Idempotency: 

```csharp
// Client generiert UUID für jeden Report-Versuch
var report = new ReportPlayer
{
    ClientRequestId = Guid.NewGuid(),  // Idempotency Key
    TargetCharacterId = 12345,
    ReportType = ReportType.Harassment,
    // ... 
};

// Server prüft ob ClientRequestId bereits verarbeitet wurde
// Bei Duplikat: Sende vorheriges Result zurück (kein neuer Report)
```

---

## 📩 Aktive Messages 3600-3699

---

## ReportPlayer (3600)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client meldet einen Spieler wegen Regelverstößen.  Der Server validiert, dedupliziert und erstellt ein Moderation-Ticket.

### Im Scope ✅
- Harassment, Cheating, Griefing melden
- Unangemessene Namen melden
- Botting/RMT melden
- Evidence über Server-IDs referenzieren

### Nicht im Scope ❌
- Chat-spezifische Meldungen → `ReportChat` (3602)
- Bug-Reports → `ReportBug` (3604)
- Exploits → `ReportExploit` (3608)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportPlayer` | Ja |
| ClientRequestId | Guid | Idempotency Key | Ja |
| TargetCharacterId | long | Gemeldeter Character | Ja |
| ReportType | ReportType | Art des Verstoßes | Ja |
| Description | string | Beschreibung (20-2000 Zeichen) | Ja |
| Evidence | List\<ReportEvidenceDto\>?  | Beweismaterial | Nein |
| IncidentZoneId | ushort? | Zone des Vorfalls | Nein |
| IncidentTimestamp | long? | Ungefährer Zeitpunkt | Nein |

### Erwartete Response
- `ReportPlayerResult` (3601)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportPlayer)]
public class ReportPlayer : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportPlayer;
    [Key(1)] public Guid ClientRequestId { get; set; }
    [Key(2)] public long TargetCharacterId { get; set; }
    [Key(3)] public ReportType ReportType { get; set; }
    [Key(4)] public string Description { get; set; } = "";
    [Key(5)] public List<ReportEvidenceDto>? Evidence { get; set; }
    [Key(6)] public ushort?  IncidentZoneId { get; set; }
    [Key(7)] public long? IncidentTimestamp { get; set; }
}
```

### Server-Verhalten

```csharp
public ReportPlayerResult HandleReportPlayer(ReportPlayer request, Character reporter)
{
    // 1. Idempotency Check
    var existing = _reportRepository.GetByClientRequestId(request.ClientRequestId);
    if (existing != null)
        return CreateResultFromExisting(existing);
    
    // 2. Validation
    if (request. TargetCharacterId == reporter.Id)
        return Fail(ReportErrorCode.SelfReport);
    
    var target = _characterService.GetCharacter(request.TargetCharacterId);
    if (target == null)
        return Fail(ReportErrorCode.TargetNotFound);
    
    if (target.HasFlag(CharacterFlags.Staff))
        return Fail(ReportErrorCode.TargetImmune);
    
    // 3. Rate Limit Check
    if (! _rateLimiter. CanReport(reporter.AccountId))
        return Fail(ReportErrorCode.RateLimited);
    
    // 4. Account Age Check
    if (reporter.Account.CreatedAt > DateTimeOffset.UtcNow.AddDays(-3))
        return Fail(ReportErrorCode.AccountTooNew);
    
    // 5. Duplicate Check
    if (IsDuplicateReport(reporter. AccountId, request. TargetCharacterId, request.ReportType))
        return Fail(ReportErrorCode.DuplicateReport);
    
    // 6. Description Validation
    if (request.Description.Length < 20)
        return Fail(ReportErrorCode.DescriptionTooShort);
    if (request.Description.Length > 2000)
        return Fail(ReportErrorCode.DescriptionTooLong);
    
    // 7. Evidence Validation
    if (request.Evidence != null)
    {
        foreach (var evidence in request.Evidence)
        {
            if (! ValidateEvidence(evidence, reporter))
                return Fail(ReportErrorCode.InvalidEvidence);
        }
    }
    
    // 8. Create Ticket
    var ticket = new ReportTicket
    {
        TicketId = GenerateTicketId(),
        ReporterId = reporter.AccountId,
        ReporterCharacterId = reporter.Id,
        ReportType = request.ReportType,
        Status = ReportStatus.Pending,
        Priority = DeterminePriority(request.ReportType),
        CreatedAt = DateTimeOffset.UtcNow. ToUnixTimeSeconds(),
        Target = new ReportTarget
        {
            TargetType = ReportTargetType.Player,
            TargetAccountId = target.AccountId,
            TargetCharacterId = target.Id,
            TargetCharacterName = target. Name,
            ZoneId = request.IncidentZoneId ??  reporter.CurrentZoneId,
            Position = reporter.Position,
            Timestamp = request.IncidentTimestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        },
        Description = SanitizeDescription(request.Description),
        Evidence = request.Evidence?. Select(e => MapEvidence(e)).ToList() ?? new()
    };
    
    _reportRepository.Save(ticket);
    _moderationQueue.Enqueue(ticket);
    _rateLimiter.RecordReport(reporter.AccountId);
    
    // 9. Send Ack Event
    SendToClient(reporter, new ReportReceivedEvent
    {
        TicketId = ticket.TicketId,
        ReportType = request.ReportType
    });
    
    return new ReportPlayerResult
    {
        Success = true,
        TicketId = ticket.TicketId,
        EstimatedResponseTime = GetEstimatedResponseTime(ticket. Priority)
    };
}
```

### Client-Verhalten

1. UI sammelt Report-Daten
2. Client generiert `ClientRequestId`
3.  Sende Request
4. Bei Erfolg:  Zeige Bestätigung mit TicketId
5. Bei Fehler: Zeige spezifische Fehlermeldung

### Flow-Diagramm

```
Client                         Server                    Mod Queue
  │                              │                           │
  │  ReportPlayer (3600)         │                           │
  │  Target:  "BadPlayer"         │                           │
  │  Type:  Harassment            │                           │
  │  Evidence: [ChatId1, Id2]    │                           │
  │─────────────────────────────►│                           │
  │                              │                           │
  │                              │  Validate Target ✓        │
  │                              │  Rate Limit Check ✓       │
  │                              │  Duplicate Check ✓        │
  │                              │  Validate Evidence ✓      │
  │                              │                           │
  │                              │  Create Ticket            │
  │                              │──────────────────────────►│
  │                              │                           │
  │  ReportPlayerResult (3601)   │                           │
  │  Success: true               │                           │
  │  TicketId: RPT-2024-XYZ      │                           │
  │◄─────────────────────────────│                           │
  │                              │                           │
  │  ReportReceivedEvent (3640)  │                           │
  │◄─────────────────────────────│                           │
  │                              │                           │
  │  [Show Confirmation UI]      │                           │
```

### Beispiel Payloads

```csharp
// Harassment Report
var harassmentReport = new ReportPlayer
{
    ClientRequestId = Guid.NewGuid(),
    TargetCharacterId = 12345,
    ReportType = ReportType.Harassment,
    Description = "Player has been sending abusive whispers to me and following me around zones.  Started approximately 30 minutes ago.",
    Evidence = new List<ReportEvidenceDto>
    {
        new()
        {
            EvidenceType = EvidenceType.ChatLog,
            ReferenceIds = new List<Guid>
            {
                Guid. Parse("a1b2c3d4-... "), // ChatMessageId 1
                Guid.Parse("e5f6g7h8-...")  // ChatMessageId 2
            }
        }
    },
    IncidentZoneId = 1001,
    IncidentTimestamp = DateTimeOffset.UtcNow.AddMinutes(-30).ToUnixTimeSeconds()
};

// Cheating Report
var cheatingReport = new ReportPlayer
{
    ClientRequestId = Guid.NewGuid(),
    TargetCharacterId = 67890,
    ReportType = ReportType.Cheating,
    Description = "Player appears to be using a speed hack.  Moving at impossible speeds through Elwynn Forest.",
    Evidence = new List<ReportEvidenceDto>
    {
        new()
        {
            EvidenceType = EvidenceType.Position,
            Description = "Observed moving from coords (100,200) to (500,600) in 2 seconds"
        }
    },
    IncidentZoneId = 1001
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `InvalidTarget` | Ungültige Character-ID | UI-Fehler beheben |
| `TargetNotFound` | Character existiert nicht | Neu versuchen |
| `SelfReport` | Kann sich nicht selbst melden | UI sollte verhindern |
| `DuplicateReport` | Bereits gemeldet | Zeige "Already Reported" |
| `RateLimited` | Zu viele Reports | Zeige Cooldown |
| `AccountTooNew` | Account zu jung | Zeige Mindest-Alter |
| `DescriptionTooShort` | Min. 20 Zeichen | Mehr Details anfordern |
| `TargetImmune` | GMs können nicht gemeldet | Zeige Info |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReportPlayerResult` | 3601 | Response |
| `ReportReceivedEvent` | 3640 | Acknowledgment Event |
| `ReportStatusRequest` | 3612 | Status abfragen |
| `ReportEvidenceAdd` | 3614 | Weitere Evidence hinzufügen |

---

## ReportPlayerResult (3601)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `ReportPlayer`. Enthält TicketId bei Erfolg oder Fehlercode. 

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportPlayerResult` | Ja |
| Success | bool | Erfolgreich?  | Ja |
| ErrorCode | ReportErrorCode | Fehlercode | Bei Fehler |
| ErrorMessage | string?  | Menschenlesbare Nachricht | Nein |
| TicketId | string?  | Report-Ticket-ID | Bei Erfolg |
| EstimatedResponseTime | string? | Geschätzte Bearbeitungszeit | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportPlayerResult)]
public class ReportPlayerResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportPlayerResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public ReportErrorCode ErrorCode { get; set; }
    [Key(3)] public string? ErrorMessage { get; set; }
    [Key(4)] public string? TicketId { get; set; }
    [Key(5)] public string? EstimatedResponseTime { get; set; }
}
```

### Beispiel Payloads

```csharp
// Erfolg
var successResult = new ReportPlayerResult
{
    Success = true,
    TicketId = "RPT-2024-ABC123",
    EstimatedResponseTime = "24-48 hours"
};

// Fehler
var errorResult = new ReportPlayerResult
{
    Success = false,
    ErrorCode = ReportErrorCode.DuplicateReport,
    ErrorMessage = "You have already reported this player for this issue."
};
```

---

## ReportChat (3602)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client meldet Chat-Nachrichten.  Verwendet Server-seitige Chat-Message-IDs als Referenz.

### Im Scope ✅
- Einzelne oder mehrere Chat-Nachrichten melden
- Spam, Werbung, Abuse melden
- Automatische Zuordnung zum Sender

### Nicht im Scope ❌
- Spieler-spezifische Vergehen ohne Chat → `ReportPlayer`

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportChat` | Ja |
| ClientRequestId | Guid | Idempotency Key | Ja |
| ChatMessageIds | List\<Guid\> | Server-seitige Message-IDs (max.  50) | Ja |
| ReportType | ReportType | ChatAbuse, Spam, Advertising | Ja |
| Description | string?  | Zusätzlicher Kontext | Nein |

### Erwartete Response
- `ReportChatResult` (3603)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportChat)]
public class ReportChat : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportChat;
    [Key(1)] public Guid ClientRequestId { get; set; }
    [Key(2)] public List<Guid> ChatMessageIds { get; set; } = new();
    [Key(3)] public ReportType ReportType { get; set; }
    [Key(4)] public string? Description { get; set; }
}
```

### Server-Verhalten

1. Validiere Message-IDs existieren
2. Validiere Reporter war im Channel/Zone der Messages
3. Extrahiere Sender aus Messages (automatisch)
4. Erstelle Report mit Chat-Content (server-side fetch)
5. Dedupliziere gegen existierende Chat-Reports

### Beispiel Payload

```csharp
var chatReport = new ReportChat
{
    ClientRequestId = Guid.NewGuid(),
    ChatMessageIds = new List<Guid>
    {
        Guid.Parse("msg-001-..."),
        Guid.Parse("msg-002-... "),
        Guid.Parse("msg-003-...")
    },
    ReportType = ReportType. Spam,
    Description = "Gold selling spam in Trade chat"
};
```

---

## ReportChatResult (3603)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `ReportChat`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportChatResult` | Ja |
| Success | bool | Erfolgreich?  | Ja |
| ErrorCode | ReportErrorCode | Fehlercode | Bei Fehler |
| TicketId | string? | Report-Ticket-ID | Bei Erfolg |
| ReportedMessageCount | int | Anzahl gemeldeter Messages | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportChatResult)]
public class ReportChatResult :  IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportChatResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public ReportErrorCode ErrorCode { get; set; }
    [Key(3)] public string? TicketId { get; set; }
    [Key(4)] public int ReportedMessageCount { get; set; }
}
```

---

## ReportBug (3604)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client meldet einen technischen Bug mit Reproduktionsschritten und System-Info.

### Im Scope ✅
- Technische Bugs melden
- Reproduktionsschritte dokumentieren
- Client-Version und Zone automatisch erfassen

### Nicht im Scope ❌
- Exploits → `ReportExploit` (3608)
- Balance-Feedback → `FeedbackSubmit` (3631)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportBug` | Ja |
| ClientRequestId | Guid | Idempotency Key | Ja |
| Category | BugCategory | UI, Gameplay, Performance, Network, etc. | Ja |
| Title | string | Kurzer Titel (max. 100 Zeichen) | Ja |
| Description | string | Ausführliche Beschreibung | Ja |
| ReproductionSteps | string?  | Schritte zur Reproduktion | Nein |
| ExpectedBehavior | string? | Erwartetes Verhalten | Nein |
| ActualBehavior | string? | Tatsächliches Verhalten | Nein |
| ClientVersion | string | Client-Build (auto-filled) | Ja |
| ZoneId | ushort | Aktuelle Zone (auto-filled) | Ja |
| Position | Position?  | Position (auto-filled) | Nein |

### Erwartete Response
- `ReportBugResult` (3605)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportBug)]
public class ReportBug : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportBug;
    [Key(1)] public Guid ClientRequestId { get; set; }
    [Key(2)] public BugCategory Category { get; set; }
    [Key(3)] public string Title { get; set; } = "";
    [Key(4)] public string Description { get; set; } = "";
    [Key(5)] public string? ReproductionSteps { get; set; }
    [Key(6)] public string? ExpectedBehavior { get; set; }
    [Key(7)] public string? ActualBehavior { get; set; }
    [Key(8)] public string ClientVersion { get; set; } = "";
    [Key(9)] public ushort ZoneId { get; set; }
    [Key(10)] public Position? Position { get; set; }
}
```

### Beispiel Payload

```csharp
var bugReport = new ReportBug
{
    ClientRequestId = Guid.NewGuid(),
    Category = BugCategory. Gameplay,
    Title = "Cannot interact with quest NPC after zone transfer",
    Description = "After teleporting to Elwynn Forest, the quest NPC 'Guard Thomas' shows no interaction prompt.",
    ReproductionSteps = "1. Accept quest 'Investigate Echo Ridge'\n2. Use Hearthstone to Stormwind\n3. Return to Elwynn Forest\n4. Try to interact with Guard Thomas",
    ExpectedBehavior = "Quest turn-in dialog should appear",
    ActualBehavior = "No interaction possible, NPC has no highlight",
    ClientVersion = "0.1.0-beta. 5",
    ZoneId = 1001,
    Position = new Position(100.5f, 200.3f)
};
```

---

## ReportBugResult (3605)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `ReportBug`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportBugResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | ReportErrorCode | Fehlercode | Bei Fehler |
| TicketId | string? | Bug-Ticket-ID | Bei Erfolg |
| IsKnownIssue | bool | Bereits bekannt? | Bei Erfolg |
| KnownIssueId | string? | Link zu bekanntem Issue | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportBugResult)]
public class ReportBugResult :  IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportBugResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public ReportErrorCode ErrorCode { get; set; }
    [Key(3)] public string? TicketId { get; set; }
    [Key(4)] public bool IsKnownIssue { get; set; }
    [Key(5)] public string? KnownIssueId { get; set; }
}
```

---

## ReportSuggestion (3606)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client reicht einen Verbesserungsvorschlag ein. 

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportSuggestion` | Ja |
| ClientRequestId | Guid | Idempotency Key | Ja |
| Category | FeedbackType | Feature, Balance, UI, etc. | Ja |
| Title | string | Kurzer Titel | Ja |
| Description | string | Ausführlicher Vorschlag | Ja |

### Erwartete Response
- `ReportSuggestionResult` (3607)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportSuggestion)]
public class ReportSuggestion :  IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportSuggestion;
    [Key(1)] public Guid ClientRequestId { get; set; }
    [Key(2)] public FeedbackType Category { get; set; }
    [Key(3)] public string Title { get; set; } = "";
    [Key(4)] public string Description { get; set; } = "";
}
```

---

## ReportSuggestionResult (3607)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `ReportSuggestion`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportSuggestionResult` | Ja |
| Success | bool | Erfolgreich?  | Ja |
| ErrorCode | ReportErrorCode | Fehlercode | Bei Fehler |
| TicketId | string? | Suggestion-ID | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportSuggestionResult)]
public class ReportSuggestionResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType. ReportSuggestionResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public ReportErrorCode ErrorCode { get; set; }
    [Key(3)] public string? TicketId { get; set; }
}
```

---

## ReportExploit (3608)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Hochpriorisierter Report für Sicherheitslücken und Exploits.  Kein Rate Limit.  Wird sofort eskaliert.

### Im Scope ✅
- Duplication Exploits
- Economy-breaking Bugs
- Security Vulnerabilities
- Unbeabsichtigte Mechaniken

### Nicht im Scope ❌
- Andere Spieler melden die cheaten → `ReportPlayer` mit `Cheating`
- Normale Bugs → `ReportBug`

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportExploit` | Ja |
| ClientRequestId | Guid | Idempotency Key | Ja |
| Title | string | Kurze Beschreibung | Ja |
| Description | string | Detaillierte Beschreibung | Ja |
| ReproductionSteps | string | Wie reproduzierbar | Ja |
| ImpactAssessment | string | Geschätzter Impact | Ja |
| HasBeenUsed | bool | Hat Reporter es ausgenutzt? | Ja |
| ClientVersion | string | Client-Build | Ja |

### Erwartete Response
- `ReportExploitResult` (3609)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportExploit)]
public class ReportExploit : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportExploit;
    [Key(1)] public Guid ClientRequestId { get; set; }
    [Key(2)] public string Title { get; set; } = "";
    [Key(3)] public string Description { get; set; } = "";
    [Key(4)] public string ReproductionSteps { get; set; } = "";
    [Key(5)] public string ImpactAssessment { get; set; } = "";
    [Key(6)] public bool HasBeenUsed { get; set; }
    [Key(7)] public string ClientVersion { get; set; } = "";
}
```

### Server-Verhalten

1. **KEINE Rate Limits** für Exploit-Reports
2. Automatische Eskalation an Security Team
3. Priority:  Critical
4. Reporter erhält erhöhte Anonymitäts-Garantie

---

## ReportExploitResult (3609)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `ReportExploit`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportExploitResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | ReportErrorCode | Fehlercode | Bei Fehler |
| TicketId | string? | Security-Ticket-ID | Bei Erfolg |
| Acknowledgment | string | Dankes-Nachricht | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportExploitResult)]
public class ReportExploitResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ReportExploitResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public ReportErrorCode ErrorCode { get; set; }
    [Key(3)] public string? TicketId { get; set; }
    [Key(4)] public string Acknowledgment { get; set; } = "Thank you for your responsible disclosure. ";
}
```

---

## AppealRequest (3610)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client legt Einspruch gegen eine Moderationsmaßnahme ein.

### Im Scope ✅
- Einspruch gegen Mutes
- Einspruch gegen Temp-Bans
- Einspruch gegen Namensänderungen

### Nicht im Scope ❌
- Perma-Bans (separater Support-Prozess)
- Einsprüche anderer Spieler

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AppealRequest` | Ja |
| ClientRequestId | Guid | Idempotency Key | Ja |
| ActionId | Guid | ID der Moderation-Action | Ja |
| Reason | string | Begründung des Einspruchs | Ja |
| AdditionalContext | string?  | Zusätzlicher Kontext | Nein |

### Erwartete Response
- `AppealResult` (3611)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AppealRequest)]
public class AppealRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.AppealRequest;
    [Key(1)] public Guid ClientRequestId { get; set; }
    [Key(2)] public Guid ActionId { get; set; }
    [Key(3)] public string Reason { get; set; } = "";
    [Key(4)] public string? AdditionalContext { get; set; }
}
```

### Server-Verhalten

1. Prüfe ob ActionId existiert und zum Account gehört
2. Prüfe ob Appeal für diese Action bereits eingereicht
3. Prüfe ob Action appeal-fähig ist (keine Perma-Bans)
4. Erstelle Appeal-Ticket
5. Setze Action-Status auf "Appealed"

---

## AppealResult (3611)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `AppealRequest`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AppealResult` | Ja |
| Success | bool | Erfolgreich eingereicht? | Ja |
| ErrorCode | ReportErrorCode | Fehlercode | Bei Fehler |
| AppealId | string? | Appeal-Ticket-ID | Bei Erfolg |
| EstimatedReviewTime | string?  | Geschätzte Bearbeitungszeit | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AppealResult)]
public class AppealResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType. AppealResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public ReportErrorCode ErrorCode { get; set; }
    [Key(3)] public string?  AppealId { get; set; }
    [Key(4)] public string?  EstimatedReviewTime { get; set; }
}
```

---

## ReportStatusRequest (3612)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fragt den Status eines eingereichten Reports ab.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ReportStatusRequest` | Ja |
| TicketId | string? | Spezifisches Ticket | Nein |
| ListAll | bool | Alle eigenen Tickets listen | Nein |
| MaxResults | int | Max. Anzahl (default: 10) | Nein |

### Erwartete Response
- `ReportStatusResponse` (3613)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReportStatusRequest)]
public class ReportStatusRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType. ReportStatusRequest;
    [Key(1)] public string? TicketId { get; set; }
    [Key(2)] public bool ListAll { get; set; }
    [Key(3)] public int MaxResults { get; set; } = 10;
}
```

---

## ReportStatusResponse (3613)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**

Source: docs/03-messages/36-reporting.md
