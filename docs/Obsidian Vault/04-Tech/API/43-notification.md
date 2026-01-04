# 🔔 Notification Messages (4300-4399)

**Kategorie:** 43  
**Range:** 4300-4399  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung  

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🗂️ Types & Channels](#️-types--channels)
- [✅ Delivery, Ack & Dedup Regeln](#-delivery-ack--dedup-regeln)
- [⏳ TTL, Expiry & Persistence](#-ttl-expiry--persistence)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#️-regeln--sicherheit)
- [📩 Aktive Messages 4300–4399](#-aktive-messages-43004399)
  - [NotificationShow (4300)](#notificationshow-4300)
  - [NotificationDismiss (4301)](#notificationdismiss-4301)
  - [NotificationQueue (4302)](#notificationqueue-4302)
  - [AlertPopup (4310)](#alertpopup-4310)
  - [AlertConfirm (4311)](#alertconfirm-4311)
  - [AlertDismiss (4312)](#alertdismiss-4312)
  - [ToastMessage (4320)](#toastmessage-4320)
  - [ToastAchievement (4321)](#toastachievement-4321)
  - [ToastLevelUp (4322)](#toastlevelup-4322)
  - [ToastLoot (4323)](#toastloot-4323)
  - [BossWarning (4330)](#bosswarning-4330)
  - [BossAbility (4331)](#bossability-4331)
  - [BossPhase (4332)](#bossphase-4332)
  - [CountdownStart (4340)](#countdownstart-4340)
  - [CountdownUpdate (4341)](#countdownupdate-4341)
  - [CountdownCancel (4342)](#countdowncancel-4342)
  - [ScreenEffect (4350)](#screeneffect-4350)
  - [ScreenShake (4351)](#screenshake-4351)
  - [ScreenFlash (4352)](#screenflash-4352)
  - [ScreenFade (4353)](#screenfade-4353)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang (MessageType Enum Updates)](#-anhang-messagetype-enum-updates)

---

## 📋 Überblick

Diese Spezifikation beschreibt das **Notification-, Alert-, Toast-, Boss-Warning-, Countdown- und Screen-Effect-System** des 2D Pixelart-MMORPG. Der Stil, Detailgrad und die Struktur folgen exakt den Vorlagen `00-connection.md` und `01-zone.md`.

**Ziele der Kategorie 43:**
- Benutzeroberfläche mit zuverlässigen, priorisierten Notifications versorgen (toast, banner, modal, inbox, badge).
- Server-autoritative IDs und Dedupe-Keys garantieren Idempotenz und sichere Retries.
- Synchronisation zwischen **ephemeren** und **persistenten** Notifications sicherstellen (Reconnect-safe, Delta + Snapshot).
- UX-Regeln wie Stacking, Prioritäten, Auto-Dismiss und Kanal-Policies definieren.
- Sicherheits- und Anti-Spam-Vorgaben um Leakage und Abuse zu verhindern.

**Integrationen (Pflicht erwähnt):**
- **System (09)**: Maintenance, ServerMotd, RateLimitWarnings.
- **Chat (04)**: Mention-Hinweise, Moderation-Warnings.
- **Party (07)** & **Guild (08)**: Einladungen, Rollen-Änderungen, Roster-Änderungen.
- **Auction (17)** & **Mail (18)**: Auktionsausgänge, neue Post (Inbox/Badge).
- **Quest (10)** & **Economy (37)**: Quest-Fortschritt, Currency Caps, Bounties.
- **Admin (23)**: GM-Hinweise, Kicks/Bans via NotificationChannel=Modal.

---

## 🧠 Datenmodell

| Entität | Felder | Beschreibung |
| ------- | ------ | ------------ |
| Notification | `NotificationId (Guid)`, `DedupeKey (string?)`, `Channel`, `Priority`, `Title`, `Body`, `Icon`, `Actions[]`, `Tags[]`, `CreatedAt`, `ExpiresAt`, `Persistent`, `RequiresAck`, `BadgeDelta`, `LocalizationKey`, `LocalizationArgs[]`, `SourceSystem` | Kernobjekt; server-autoritative ID + optionaler DedupeKey |
| DeliveryTarget | `CharacterId`, `AccountId`, `Scope (Self/Party/Guild/Zone/World)`, `ShardId?` | Routing-Information |
| AckState | `NotificationId`, `AckedAt`, `AckSource (Click/Timeout/Programmatic)`, `ClientSequence`, `ServerSequence` | Zustandsobjekt für Confirm/Dismiss |
| Revision | `RevisionId`, `SnapshotVersion`, `DeltaVersion`, `Checksum` | Synchronisationsanker für Deltas |
| BadgeState | `Mailbox`, `Auction`, `Quest`, `Social`, `System`, `Custom` | Aggregierte Counts, werden über NotificationQueue aktualisiert |

**Designprinzipien:**
- **Server-authoritative IDs**: `NotificationId` ist GUID, wird nur serverseitig generiert.
- **Idempotenz**: Kombination aus `NotificationId` + optional `DedupeKey` verhindert Doppelanzeigen.
- **Correlation**: Client-seitige Requests tragen `ClientSequence`; Server antwortet mit `ServerSequence` + `RequestId`.
- **Channel Safety**: Channel entscheidet UI-Routing, Animations-Typ und Dismiss-Policy.
- **Priority First**: Höhere Prioritäten verdrängen niedrigere, wenn UI-Grenzen erreicht werden.

---

## 🗂️ Types & Channels

| Channel | Zweck | Lifetime | Interaktion | Stacking |
| ------- | ----- | -------- | ----------- | -------- |
| **Toast** | Kurze, nicht-blockierende Hinweise (Loot, XP, System-Short) | 2-6s | Optional (Click=Dismiss+Action) | FIFO, max 3 visible |
| **Banner** | Breitformat, wichtig aber nicht modal (World-Events, Boss-Warnings) | 6-12s | Optional Buttons (z.B. „Teleport“) | Priority preempts lower |
| **Modal** | Blockierend, erfordert Entscheidung (GM-Bestätigung, Risky Actions) | Bis Ack | Buttons required | Kein Stacking, sequentiell |
| **Inbox** | Persistent, abrufbar im Notification-Center | Bis TTL | Dismiss + MarkRead | Unbegrenzt, paginiert |
| **Badge** | Zähler pro Feature (Mail, Auction, Party Invites) | Bis nächsten Update | Nicht klickbar | Aggregiert |

**Priorität (Priority):** `Low=0`, `Normal=1`, `High=2`, `Critical=3`.  
- Critical erzwingt Modal oder Banner + ScreenEffect.  
- High kann laufende Toasts verdrängen.  
- Low wird gedrosselt, wenn Bandbreite/Rate-Limits greifen.

**Delivery Targets:**  
- **Self**: Nur eigener Client.  
- **Party/Guild**: Broadcast an Mitglieder (Server → Clients, 📡).  
- **Zone/World**: Broadcast mit Scope-Filter (z.B. BossWarning in aktueller Zone).  
- **System**: Global, aber kanalisiert (z.B. Maintenance Banner).

---

## ✅ Delivery, Ack & Dedup Regeln

1) **Server-authoritative NotificationId**  
- GUID wird beim Erzeugen vergeben; niemals vom Client generiert.  
- Optionaler `DedupeKey` (string) für idempotente Replays (z.B. beim Reconnect).

2) **Ack-Pflicht**  
- `RequiresAck=true`: Client sendet `NotificationDismiss (4301)` oder `AlertConfirm (4311)` mit `ClientSequence`.  
- Server antwortet mit gleicher Message-ID (Ack-Flag) und `ServerSequence`.  
- Retries alle 5s bis Ack oder `ExpiresAt`.

3) **Deduplication**  
- Client dedupliziert per `NotificationId`.  
- Server dedupliziert per `(CharacterId, DedupeKey)` innerhalb TTL.  
- Bei Dedupe-Hit: Server sendet `NotificationQueue (4302)` Delta (Badge + State).

4) **Retries**  
- Server speichert nicht-acknowledged Notifications persistent (Inbox) und sendet beim Reconnect als Snapshot (`NotificationQueue`).  
- Ephemere Toasts werden max. 3x resend, dann in Inbox umgewandelt, wenn `PersistentFallback=true`.

5) **Rate Limits**  
- Pro Spieler: 30 Notifications/Minute total, 10/Minute pro Channel.  
- Global throttle: Critical nur 5/Minute/Zone.  
- Verstöße triggern `RateLimitWarning (917)` aus Kategorie 09.

---

## ⏳ TTL, Expiry & Persistence

| Channel | Standard TTL | Persistenz | Fallback |
| -------- | ------------ | ---------- | -------- |
| Toast | 6s (auto) | Nein | Bei Nicht-Zustellung → Inbox |
| Banner | 12s | Optional | Bei Expiry ohne Ack → Inbox |
| Modal | Bis Ack | Ja | Keine Auto-Dismiss |
| Inbox | 30 Tage | Ja | Hard-delete nach TTL |
| Badge | Bis nächstes Update | Ja (aggregiert) | Reset via Snapshot |

- `ExpiresAt` wird serverseitig gesetzt.  
- Client verwirft abgelaufene Notifications und sendet `NotificationDismiss` mit Reason=Expired (kein UI).  
- Persistente Notifications werden im Character-Storage abgelegt; Modal-Acks blockieren bis Antwort.

---

## 🔄 Sync, Deltas & Revisioning

**Snapshot vs Delta**  
- **Snapshot**: `NotificationQueue (4302)` liefert kompletten Zustand (Inbox, BadgeCounts, offene Modals).  
- **Delta**: `NotificationShow (4300)` / `Toast*` / `Boss*` liefern einzelne Events; Badge-Änderungen werden als Delta in `NotificationQueue` gesendet (BadgeDelta).  

**Reconnect-Sicherheit**  
- Beim Reconnect sendet Server **immer** `NotificationQueue` mit `RevisionId`.  
- Client schickt letzte `RevisionId` in `NotificationDismiss` → Server entscheidet Snapshot/Deltas.  

**Revision Fields**  
- `SnapshotVersion` (ulong) monoton steigend.  
- `DeltaVersion` (ulong) pro Session.  
- `Checksum` (uint) CRC32 über persistente Notifications.

**Konsistenz-Strategie**  
- **At-Least-Once** Delivery für Toast/Banner (idempotent via ID).  
- **Exactly-Once** Render für Modals via `RequiresAck` + dedup.  
- **Best-Effort** für ScreenEffects (nicht persistiert).  

---

## 🧱 DTOs / Interfaces

**NotificationDto (Basis)**  
```csharp
[MessagePackObject]
public class NotificationDto
{
    [Key(0)] public MessageType Type => MessageType.NotificationShow;
    [Key(1)] public Guid NotificationId { get; set; }
    [Key(2)] public string? DedupeKey { get; set; }
    [Key(3)] public NotificationChannel Channel { get; set; }
    [Key(4)] public NotificationPriority Priority { get; set; }
    [Key(5)] public string Title { get; set; } = string.Empty;
    [Key(6)] public string Body { get; set; } = string.Empty;
    [Key(7)] public string? Icon { get; set; }
    [Key(8)] public List<NotificationActionDto>? Actions { get; set; }
    [Key(9)] public List<string>? Tags { get; set; }
    [Key(10)] public long CreatedAt { get; set; }
    [Key(11)] public long? ExpiresAt { get; set; }
    [Key(12)] public bool Persistent { get; set; }
    [Key(13)] public bool RequiresAck { get; set; }
    [Key(14)] public int? BadgeDelta { get; set; }
    [Key(15)] public string? LocalizationKey { get; set; }
    [Key(16)] public List<string>? LocalizationArgs { get; set; }
    [Key(17)] public string SourceSystem { get; set; } = "system";
}
```

**NotificationActionDto**  
```csharp
[MessagePackObject]
public class NotificationActionDto
{
    [Key(0)] public string ActionId { get; set; } = string.Empty; // e.g. "open_mail"
    [Key(1)] public string Label { get; set; } = string.Empty;   // Localization key friendly
    [Key(2)] public NotificationActionType ActionType { get; set; }
    [Key(3)] public string? Payload { get; set; } // JSON-safe string or MessagePack-serialized args
}
```

**NotificationAckRequest (für 4301 / 4311)**  
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.NotificationDismiss)]
public class NotificationAckRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.NotificationDismiss;
    [Key(1)] public Guid NotificationId { get; set; }
    [Key(2)] public string Reason { get; set; } = "user";
    [Key(3)] public uint ClientSequence { get; set; }
    [Key(4)] public ulong LastKnownRevision { get; set; }
}

[MessagePackObject]
[NetworkMessage(MessageType.NotificationDismiss)]
public class NotificationAckResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.NotificationDismiss;
    [Key(1)] public Guid NotificationId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public string? ErrorCode { get; set; }
    [Key(4)] public uint ClientSequence { get; set; }
    [Key(5)] public uint ServerSequence { get; set; }
    [Key(6)] public ulong SnapshotVersion { get; set; }
}
```

**NotificationQueueDto (Snapshot/Delta)**  
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.NotificationQueue)]
public class NotificationQueueDto : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.NotificationQueue;
    [Key(1)] public ulong SnapshotVersion { get; set; }
    [Key(2)] public ulong DeltaVersion { get; set; }
    [Key(3)] public List<NotificationDto> Inbox { get; set; } = new();
    [Key(4)] public List<NotificationDto>? EphemeralPending { get; set; }
    [Key(5)] public BadgeStateDto Badges { get; set; } = new();
    [Key(6)] public bool IsDelta { get; set; }
}
```

**BadgeStateDto**  
```csharp
[MessagePackObject]
public class BadgeStateDto
{
    [Key(0)] public int Mail { get; set; }
    [Key(1)] public int Auction { get; set; }
    [Key(2)] public int Party { get; set; }
    [Key(3)] public int Guild { get; set; }
    [Key(4)] public int Quest { get; set; }
    [Key(5)] public int System { get; set; }
    [Key(6)] public int Custom { get; set; }
}
```

**CountdownDto**  
```csharp
[MessagePackObject]
public class CountdownDto
{
    [Key(0)] public Guid CountdownId { get; set; }
    [Key(1)] public int DurationSeconds { get; set; }
    [Key(2)] public string Message { get; set; } = string.Empty;
    [Key(3)] public string Category { get; set; } = "default"; // e.g., "raid", "pvp", "system"
    [Key(4)] public bool IsSkippable { get; set; }
}
```

**ScreenEffectDto**  
```csharp
[MessagePackObject]
public class ScreenEffectDto
{
    [Key(0)] public string EffectType { get; set; } = "shake";
    [Key(1)] public float Intensity { get; set; }
    [Key(2)] public int DurationMs { get; set; }
    [Key(3)] public string? Color { get; set; }
    [Key(4)] public string? Easing { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

```csharp
public enum NotificationChannel : byte
{
    Toast = 1,
    Banner = 2,
    Modal = 3,
    Inbox = 4,
    Badge = 5
}

public enum NotificationPriority : byte
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public enum NotificationActionType : byte
{
    OpenUrl = 1,
    OpenPanel = 2,
    FocusEntity = 3,
    AcceptInvite = 4,
    DeclineInvite = 5,
    Acknowledge = 6
}

public enum NotificationErrorCode
{
    NONE,
    NOT_FOUND,
    ALREADY_ACKED,
    EXPIRED,
    RATE_LIMITED,
    INVALID_CHANNEL,
    UNAUTHORIZED
}
```

**Standard-ErrorCodes (tabellarisch)**  
| Code | Beschreibung | Server-Aktion |
| ---- | ------------ | ------------- |
| `NOT_FOUND` | NotificationId unbekannt | Ignorieren + Snapshot erzwingen |
| `ALREADY_ACKED` | Ack doppelt | Idempotent → Success=true |
| `EXPIRED` | TTL abgelaufen | Keine Retry, Inbox optional |
| `RATE_LIMITED` | Per-Player Limit überschritten | Drop + RateLimitWarning |
| `INVALID_CHANNEL` | Channel nicht erlaubt für Benutzer | Drop + Audit |
| `UNAUTHORIZED` | Keine Berechtigung (GM/Server-only) | Disconnect optional |

---

## ⚙️ Regeln & Sicherheit

- **Anti-Spam:** Throttling (siehe Delivery-Regeln), serverseitige Allowlist der Channels pro SourceSystem (z.B. Economy darf Badge+Banner, nicht Modal).  
- **Privacy:** Keine sensitiven Daten (GM-Kommandos, interne IDs) im Notification-Body. Player-bezogene Daten nur wenn Empfänger=Owner.  
- **Localization Safety:** Verwendung von `LocalizationKey` + `LocalizationArgs` anstelle von untrusted Plaintext.  
- **Input Validation:** Client sendet nie freie Texte; alle Felder sind IDs oder Enums.  
- **Integrity:** Modals mit `RequiresAck` blocken Spiel-Eingaben bis Response; serverseitig Timeout=30s (dann ForceDismiss).  
- **UI Injection Prevention:** `Icon` und `Payload` werden gegen Allowlist geprüft; keine HTML/JS/Markdown-Interpretation.  
- **Rate Limits:** Global + pro Account; bei Verstoß sendet Kategorie 09 `RateLimitWarning (917)`.  
- **GM Visibility:** GM-spezifische Notifications (Channel Modal) setzen Flag `RequiresAck=true` und Audit-Logging mit SessionId.  
- **Cross-System Safety:** Quellen `Admin (23)`, `Economy (37)`, `Mail (18)` müssen `SourceSystem` setzen, um Routing + Audit zu unterstützen.  

---

## 📩 Aktive Messages 4300–4399

Die nachfolgenden Messages sind **in der Reihenfolge des MessageType-Enums** dokumentiert. Jede Client-Request-Message hat eine definierte Response (gleiche oder andere ID) zur Korrelation (`ClientSequence`, `ServerSequence`, `NotificationId`).

---

## NotificationShow (4300)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (Event-driven)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server pusht eine neue Notification oder Toast/Banner/Modal. Diese Message wird sowohl für ephemere (Toast/Banner) als auch persistente (Inbox, Modal) Inhalte genutzt. Deduplizierung erfolgt über `NotificationId` und optional `DedupeKey`.

### Im Scope ✅
- Zustellung neuer Notifications an den Client.
- UI-Routing via `Channel`.
- Prioritätsbasierte Anzeige (stack/replace).
- Optionales Badge-Update (`BadgeDelta`).

### Nicht im Scope ❌
- Acknowledgement (wird mit `NotificationDismiss` geliefert).
- Snapshot-Übermittlung (nutze `NotificationQueue`).
- Modale Antwort (nutze `AlertConfirm`).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | Server-autoritative ID | Ja |
| DedupeKey | string? | Idempotenz-Key | Nein |
| Channel | NotificationChannel | Toast/Banner/Modal/Inbox/Badge | Ja |
| Priority | NotificationPriority | Low/Normal/High/Critical | Ja |
| Title | string | UI Titel (lokalisiert) | Ja |
| Body | string | UI Text (lokalisiert oder Placeholder) | Ja |
| Icon | string? | Icon-Key | Nein |
| Actions | List<NotificationActionDto>? | Buttons/Links | Nein |
| Tags | List<string>? | Filter/Grouping | Nein |
| CreatedAt | long | Unix ms | Ja |
| ExpiresAt | long? | Unix ms | Nein |
| Persistent | bool | Inbox speichern? | Ja |
| RequiresAck | bool | Ack-Pflicht? | Ja |
| BadgeDelta | int? | Delta für Channel=Badge | Nein |
| LocalizationKey | string? | String-Key | Nein |
| LocalizationArgs | List<string>? | Parameter | Nein |
| SourceSystem | string | z.B. "mail", "quest", "system" | Ja |

### Erwartete Response
- `NotificationDismiss (4301)` mit Reason=auto/click/timeout bei `RequiresAck=true`.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.NotificationShow)]
public class NotificationShow : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.NotificationShow;
    [Key(1)] public Guid NotificationId { get; init; }
    [Key(2)] public string? DedupeKey { get; init; }
    [Key(3)] public NotificationChannel Channel { get; init; }
    [Key(4)] public NotificationPriority Priority { get; init; }
    [Key(5)] public string Title { get; init; } = string.Empty;
    [Key(6)] public string Body { get; init; } = string.Empty;
    [Key(7)] public string? Icon { get; init; }
    [Key(8)] public List<NotificationActionDto>? Actions { get; init; }
    [Key(9)] public List<string>? Tags { get; init; }
    [Key(10)] public long CreatedAt { get; init; }
    [Key(11)] public long? ExpiresAt { get; init; }
    [Key(12)] public bool Persistent { get; init; }
    [Key(13)] public bool RequiresAck { get; init; }
    [Key(14)] public int? BadgeDelta { get; init; }
    [Key(15)] public string? LocalizationKey { get; init; }
    [Key(16)] public List<string>? LocalizationArgs { get; init; }
    [Key(17)] public string SourceSystem { get; init; } = "system";
}
```

### Server-Verhalten
- Erzeugt `NotificationId`, prüft Rate-Limits und Channel-Allowlist.
- Persistiert, wenn `Persistent=true` oder `RequiresAck=true`.
- Schickt Retry alle 5s bis Ack oder `ExpiresAt`.
- Bei Dedupe-Hit wird kein neues UI-Element erzeugt; stattdessen `NotificationQueue (Delta)` gesendet.
- Bei Critical + Modal: pausiert Input bis Ack.

### Client-Verhalten
- Dedupe per `NotificationId`.
- Route per `Channel` in UI-Layer (ToastStack, BannerBar, ModalManager, InboxStore, BadgeStore).
- Wenn `RequiresAck=true`: sendet `NotificationDismiss` (Reason=user/auto) sobald angezeigt/geschlossen.
- Persistiert Inbox-Einträge lokal (Cache) bis SnapshotVersion bestätigt.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │                              │  Build NotificationDto
  │                              │  Persist if needed
  │                              │
  │  NotificationShow (4300)     │
  │◄─────────────────────────────│
  │  Render UI / Route           │
  │  send NotificationDismiss    │ (bei Ack-Pflicht)
  │─────────────────────────────►│
  │                              │  NotificationDismiss Ack
  │  NotificationDismiss (Ack)   │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
// Mail Badge + Inbox persistent
var mailNotif = new NotificationShow
{
    NotificationId = Guid.NewGuid(),
    DedupeKey = "mail:new",
    Channel = NotificationChannel.Inbox,
    Priority = NotificationPriority.Normal,
    Title = "Neue Post",
    Body = "Du hast 2 neue Nachrichten.",
    Icon = "icon_mail",
    Persistent = true,
    RequiresAck = true,
    BadgeDelta = 2,
    SourceSystem = "mail",
    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

// Critical boss banner
var bossBanner = new NotificationShow
{
    NotificationId = Guid.NewGuid(),
    Channel = NotificationChannel.Banner,
    Priority = NotificationPriority.Critical,
    Title = "Boss-Fähigkeit in 5s",
    Body = "Weiche aus!",
    Icon = "icon_skull",
    Persistent = false,
    RequiresAck = false,
    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeMilliseconds(),
    SourceSystem = "raid"
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `RATE_LIMITED` | Zu viele Notifications |
| `INVALID_CHANNEL` | Channel nicht erlaubt |
| `UNAUTHORIZED` | Quelle nicht berechtigt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `NotificationDismiss` | 4301 | Ack für RequiresAck |
| `NotificationQueue` | 4302 | Snapshot/Deltas |
| `AlertPopup` | 4310 | Modal Spezialisierung |
| `ToastMessage` | 4320 | Toast Spezialisierung |

---

## NotificationDismiss (4301)

**Richtung:** 📤 Client → Server (Request) / 📥 Server → Client (Ack)  
**Frequenz:** Häufig (pro Anzeige)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigt Empfang/Anzeige oder Dismiss einer Notification. Wird auch automatisch gesendet, wenn ein Toast via Timeout verschwindet oder bei Expiry. Server antwortet mit derselben Message-ID als Ack.

### Im Scope ✅
- Ack für `NotificationShow` (RequiresAck).
- User-initiierte Dismiss-Aktion.
- Auto-Dismiss (Timeout).
- Expiry-Dismiss (Client verpasst Anzeige).

### Nicht im Scope ❌
- Modal-Buttons (nutze `AlertConfirm`).
- Snapshot-Synchronisation (nutze `NotificationQueue`).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | Ziel | Ja |
| Reason | string | `user`/`auto`/`expired` | Ja |
| ClientSequence | uint | Sequenznummer | Ja |
| LastKnownRevision | ulong | Revision für Delta-Entscheidung | Ja |

### Erwartete Response
- `NotificationDismiss (4301)` (Server → Client) mit `Success` und `ServerSequence`.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.NotificationDismiss)]
public class NotificationDismiss : IClientMessage, IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.NotificationDismiss;
    [Key(1)] public Guid NotificationId { get; set; }
    [Key(2)] public string Reason { get; set; } = "user";
    [Key(3)] public uint ClientSequence { get; set; }
    [Key(4)] public uint? ServerSequence { get; set; } // Nur in Response gesetzt
    [Key(5)] public bool? Success { get; set; }        // Nur in Response
    [Key(6)] public string? ErrorCode { get; set; }    // Nur in Response
    [Key(7)] public ulong LastKnownRevision { get; set; }
    [Key(8)] public ulong? SnapshotVersion { get; set; } // Response
}
```

### Server-Verhalten
- Validiert `NotificationId` + Ownership.
- Markiert Notification als `Acked` und entfernt aus Pending-Queue.
- Aktualisiert Badge-State wenn `BadgeDelta` vorhanden.
- Sendet Response mit `Success=true` und ggf. SnapshotVersion.
- Bei `NOT_FOUND` → sendet Snapshot, damit Client neu synchronisiert.

### Client-Verhalten
- Sendet Request beim Schließen/Auto-Dismiss.
- Bei fehlender Response: Retries mit Exponential Backoff (1/2/4/8s).
- Bei Response mit `SnapshotVersion`: aktualisiert lokalen Store.

### Flow-Diagramm
```
Client                              Server
  │                                   │
  │ NotificationShow (4300)           │
  │◄──────────────────────────────────│
  │ Render Toast/Banner/Modal        │
  │ NotificationDismiss (4301)       │
  │──────────────────────────────────►│
  │                                   │ validate / persist
  │ NotificationDismiss (Ack)         │
  │◄──────────────────────────────────│
```

### Beispiel Payloads
```csharp
// Client dismisses manually
var req = new NotificationDismiss
{
    NotificationId = notifId,
    Reason = "user",
    ClientSequence = 42,
    LastKnownRevision = currentRevision
};

// Server ack
var resp = new NotificationDismiss
{
    NotificationId = notifId,
    Reason = "user",
    ClientSequence = 42,
    ServerSequence = 1001,
    Success = true,
    SnapshotVersion = 12
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `NOT_FOUND` | NotificationId unbekannt |
| `ALREADY_ACKED` | Bereits bestätigt |
| `EXPIRED` | Abgelaufen |
| `RATE_LIMITED` | Zu viele Acks (Missbrauch) |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `NotificationShow` | 4300 | Ursprung |
| `NotificationQueue` | 4302 | Snapshot bei Desync |
| `AlertConfirm` | 4311 | Modal-Bestätigung |

---

## NotificationQueue (4302)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login, Reconnect, Delta bei Badge)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Liefert Snapshot oder Delta der Notification-Lage (Inbox, offene Modals, Badge-Zähler). Wird beim Login/Reconnect gesendet oder wenn Badge-Zähler sich ändern.

### Im Scope ✅
- Vollständige Synchronisation nach Reconnect.
- Badge-Counts aktualisieren.
- Inbox-Seiten liefern (Pagination über PagingToken optional).
- Delta-Send bei Badge-Änderungen oder neuem SnapshotVersion.

### Nicht im Scope ❌
- Einzelne ephemere Toasts (dafür `NotificationShow`).
- Boss-/Countdown-/ScreenEffect-Events.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SnapshotVersion | ulong | Version des Snapshot | Ja |
| DeltaVersion | ulong | Version des Deltas | Ja |
| Inbox | List<NotificationDto> | Persistente Notifications | Ja |
| EphemeralPending | List<NotificationDto>? | Noch nicht angezeigte ephemere | Nein |
| Badges | BadgeStateDto | Aggregierte Badge-Zähler | Ja |
| IsDelta | bool | true=Delta, false=Full | Ja |

### Erwartete Response
- Keine (Server → Client Snapshot). Client sendet `NotificationDismiss` für Acks einzelner Items.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.NotificationQueue)]
public class NotificationQueue : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.NotificationQueue;
    [Key(1)] public ulong SnapshotVersion { get; init; }
    [Key(2)] public ulong DeltaVersion { get; init; }
    [Key(3)] public List<NotificationDto> Inbox { get; init; } = new();
    [Key(4)] public List<NotificationDto>? EphemeralPending { get; init; }
    [Key(5)] public BadgeStateDto Badges { get; init; } = new();
    [Key(6)] public bool IsDelta { get; init; }
}
```

### Server-Verhalten
- Baut Snapshot beim Login/Recover mit allen persistenten Notifications.
- Sendet Delta (IsDelta=true) nur mit Änderungen (BadgeDelta, neue Inbox-Items).
- Wenn Client-Revision unbekannt oder kleiner: sendet Full Snapshot.
- Synchronisiert `BadgeState` aus Mail/Auction/Quest Systemen.

### Client-Verhalten
- Wenn `IsDelta=false`: ersetzt lokale Inbox/Badges.
- Wenn `IsDelta=true`: wendet Deltas an (add/replace by NotificationId).
- Verwendet `SnapshotVersion` als Persistenz-Anchor.
- Aktualisiert UI-Badges sofort; triggert `BadgeCountUpdatedEvent` intern.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  Login/Reconnect             │
  │─────────────────────────────►│
  │                              │  Build Snapshot
  │  NotificationQueue (Full)    │
  │◄─────────────────────────────│
  │  Merge Inbox/Badges          │
  │  Send pending Acks (4301)    │
  │─────────────────────────────►│
  │                              │  Optional Delta when badge change
  │  NotificationQueue (Delta)   │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var full = new NotificationQueue
{
    SnapshotVersion = 42,
    DeltaVersion = 0,
    IsDelta = false,
    Inbox = new List<NotificationDto> { /* ... */ },
    Badges = new BadgeStateDto { Mail = 2, Auction = 1 }
};

var delta = new NotificationQueue
{
    SnapshotVersion = 42,
    DeltaVersion = 3,
    IsDelta = true,
    EphemeralPending = new()
    {
        new NotificationDto { NotificationId = Guid.NewGuid(), Channel = NotificationChannel.Toast, Priority = NotificationPriority.Normal, Title = "System", Body = "Server-Restart in 10m", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
    },
    Badges = new BadgeStateDto { Mail = 3, Auction = 1 }
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `NONE` | Snapshot erfolgreich |
| `RATE_LIMITED` | Zu viele Snapshots (Missbrauch) |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `NotificationShow` | 4300 | Einzel-Delta |
| `NotificationDismiss` | 4301 | Ack einzelner Items |

---

## AlertPopup (4310)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server (kann Input blocken)

### Beschreibung
Modal-Dialog mit Buttons. Wird verwendet für GM-Warnungen, kritische Entscheidungen (z.B. Dungeon verlassen), AGB-Hinweise, Anti-Cheat-Warnings.

### Im Scope ✅
- Blockierende Modals mit Pflichtantwort.
- Mehrere Buttons (OK/Cancel/Retry).
- Optional Timer/Countdown (z.B. Auto-Abbruch).
- Optionale Checkbox ("Nicht erneut zeigen").

### Nicht im Scope ❌
- Nicht-blockierende Banner/Toasts.
- Persistente Inbox (dafür `NotificationShow` mit Channel=Inbox).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AlertId | Guid | Korrelations-ID | Ja |
| Title | string | Überschrift | Ja |
| Message | string | Inhalt | Ja |
| Buttons | List<string> | Button-Labels | Ja |
| DefaultButton | int | Index Default | Ja |
| TimeoutSeconds | int? | Auto-Aktion | Nein |
| Priority | NotificationPriority | Priorität | Ja |
| RequiresAck | bool | Immer true | Ja |
| LocalizationKey | string? | Key | Nein |
| LocalizationArgs | List<string>? | Args | Nein |

### Erwartete Response
- `AlertConfirm (4311)` bei Button-Klick.
- `AlertDismiss (4312)` bei Schließen/Timeout.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AlertPopup)]
public class AlertPopup : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.AlertPopup;
    [Key(1)] public Guid AlertId { get; init; }
    [Key(2)] public string Title { get; init; } = string.Empty;
    [Key(3)] public string Message { get; init; } = string.Empty;
    [Key(4)] public List<string> Buttons { get; init; } = new();
    [Key(5)] public int DefaultButton { get; init; }
    [Key(6)] public int? TimeoutSeconds { get; init; }
    [Key(7)] public NotificationPriority Priority { get; init; } = NotificationPriority.High;
    [Key(8)] public bool RequiresAck { get; init; } = true;
    [Key(9)] public string? LocalizationKey { get; init; }
    [Key(10)] public List<string>? LocalizationArgs { get; init; }
}
```

### Server-Verhalten
- Pausiert Input, wenn Priority ≥ High.
- Startet Timeout-Timer; bei Ablauf sendet `AlertDismiss` mit Reason=timeout.
- Audit-Log bei GM/Anti-Cheat Alerts.
- Sendet optional ScreenEffect (4350) für visuelles Feedback.

### Client-Verhalten
- Rendert Modal, deaktiviert restliche UI.
- Startet Countdown visual, falls TimeoutSeconds gesetzt.
- Sendet `AlertConfirm` oder `AlertDismiss` je nach Nutzeraktion/Timeout.
- Merkt `AlertId`, um doppelte Popups zu vermeiden (dedupe).

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  AlertPopup (4310)           │
  │◄─────────────────────────────│
  │  Render Modal                │
  │  AlertConfirm (4311)         │
  │─────────────────────────────►│
  │                              │  Apply action / persist ack
  │  AlertDismiss (4312)         │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var gmAlert = new AlertPopup
{
    AlertId = Guid.NewGuid(),
    Title = "GM Anfrage",
    Message = "Ein GM möchte mit dir sprechen.",
    Buttons = new() { "OK" },
    DefaultButton = 0,
    Priority = NotificationPriority.Critical
};

var dungeonLeave = new AlertPopup
{
    AlertId = Guid.NewGuid(),
    Title = "Dungeon verlassen?",
    Message = "Du verlässt die Instanz und verlierst Fortschritt.",
    Buttons = new() { "Verlassen", "Abbrechen" },
    DefaultButton = 1,
    TimeoutSeconds = 15,
    Priority = NotificationPriority.High
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `UNAUTHORIZED` | Spieler darf Alert nicht sehen |
| `RATE_LIMITED` | Zu viele Modals |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `AlertConfirm` | 4311 | Button-Klick |
| `AlertDismiss` | 4312 | Schließen/Timeout |
| `ScreenEffect` | 4350 | Optionaler Effekt |

---

## AlertConfirm (4311)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client bestätigt einen Alert-Dialog mit einem Button. Server verarbeitet die Aktion (z.B. Dungeon verlassen) und antwortet mit `AlertDismiss` als finalem Abschluss.

### Im Scope ✅
- Button-Klick inklusive ButtonIndex.
- Übertragung von ClientSequence zur Korrelation.
- Triggern serverseitiger Aktionen (Teleport, Accept, Kick, AcceptInvite).

### Nicht im Scope ❌
- UI-Schließen ohne Auswahl (nutze `AlertDismiss`).
- Mehrfache Klicks (werden dedupliziert).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AlertId | Guid | Korrelations-ID | Ja |
| ButtonIndex | int | 0-basiert | Ja |
| ClientSequence | uint | Sequenz | Ja |

### Erwartete Response
- `AlertDismiss (4312)` (Server → Client) mit Result=accepted/declined/timeouted.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AlertConfirm)]
public class AlertConfirm : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.AlertConfirm;
    [Key(1)] public Guid AlertId { get; set; }
    [Key(2)] public int ButtonIndex { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Prüft gültiges AlertId, dedupliziert per `(AlertId, ClientSequence)`.
- Führt Aktion aus (Teleport, Accept, Kick, etc.).
- Sendet `AlertDismiss` mit Result=accepted.
- Bei Fehler sendet `AlertDismiss` mit Result=error + ErrorCode.

### Client-Verhalten
- Sendet nur einmal pro AlertId; bei Timeout/Retry Button disabled.
- Erwartet `AlertDismiss` als Abschluss; bei Ausbleiben → Retry mit Backoff.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  AlertConfirm (4311)         │
  │─────────────────────────────►│
  │                              │  Validate + apply action
  │  AlertDismiss (4312)         │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var confirm = new AlertConfirm
{
    AlertId = alertId,
    ButtonIndex = 0,
    ClientSequence = 7
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `NOT_FOUND` | AlertId unbekannt/abgelaufen |
| `ALREADY_ACKED` | Bereits bestätigt |
| `UNAUTHORIZED` | Keine Berechtigung |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `AlertPopup` | 4310 | Ursprung |
| `AlertDismiss` | 4312 | Response |

---

## AlertDismiss (4312)

**Richtung:** 📥 Server → Client (final) / 📤 Client → Server (Timeout/Close)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Schließt ein Alert-Modal und bestätigt Abschluss. Wird vom Server als Response auf `AlertConfirm` gesendet oder vom Client, wenn Modal geschlossen/timeout passiert.

### Im Scope ✅
- Abschluss eines Modals.
- Ergebnis transportieren (`Result` = accepted/declined/timeout/error).
- Retry-Signal für hängende Modals.

### Nicht im Scope ❌
- Neue Notifications (4300).
- Snapshot (4302).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AlertId | Guid | Korrelations-ID | Ja |
| Result | string | accepted/declined/timeout/error | Ja |
| ErrorCode | string? | Fehlercode bei error | Nein |
| ClientSequence | uint? | Echo aus Confirm | Nein |
| ServerSequence | uint? | Antwort-Seq | Nein |

### Erwartete Response
- Keine (finale Nachricht).

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AlertDismiss)]
public class AlertDismiss : IServerMessage, IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.AlertDismiss;
    [Key(1)] public Guid AlertId { get; set; }
    [Key(2)] public string Result { get; set; } = "accepted";
    [Key(3)] public string? ErrorCode { get; set; }
    [Key(4)] public uint? ClientSequence { get; set; }
    [Key(5)] public uint? ServerSequence { get; set; }
}
```

### Server-Verhalten
- Bei Server-Send: schließt Modal, setzt `Result`.
- Bei Client-Send: prüft Timeout oder Close, loggt Grund.
- Auf Fehler: setzt `ErrorCode` und `Result=error`.

### Client-Verhalten
- Bei Empfang: entfernt Modal aus UI, entblockt Input.
- Bei Send (Timeout/Close): informiert Server über Abbruch.
- Bei fehlender Server-Antwort: Timeout nach 5s, erneut senden.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  AlertConfirm / Timeout      │
  │─────────────────────────────►│
  │                              │  Aktion / Timeout / Error
  │  AlertDismiss (Result)       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new AlertDismiss
{
    AlertId = alertId,
    Result = "accepted",
    ClientSequence = 7,
    ServerSequence = 1002
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `NOT_FOUND` | Alert unbekannt |
| `EXPIRED` | Timeout überschritten |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `AlertPopup` | 4310 | Ursprung |
| `AlertConfirm` | 4311 | Request |

---

## ToastMessage (4320)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Kurze, ephemere UI-Toast für allgemeine Hinweise (XP-Gain, System-Info, Chat-Filter-Warnung). Idempotent via NotificationId, aber standardmäßig nicht persistent.

### Im Scope ✅
- Kurze Auto-Dismiss Hinweise.
- Klickbare Action (ein optionaler Button).
- Leichte Animation (fade/slide).

### Nicht im Scope ❌
- Modals oder Banners.
- Persistente Speicherungen (Inbox).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | ID | Ja |
| Message | string | Inhalt | Ja |
| Type | string | info/success/warning/error | Ja |
| DurationMs | int | Default 4000ms | Ja |
| Icon | string? | Icon-Key | Nein |
| Action | NotificationActionDto? | Optionaler Button | Nein |
| CreatedAt | long | Unix ms | Ja |
| LocalizationKey | string? | Key | Nein |
| LocalizationArgs | List<string>? | Args | Nein |
| RequiresAck | bool | Default false | Ja |

### Erwartete Response
- `NotificationDismiss (4301)` (Reason=auto/user) falls RequiresAck=true (optional Flag).

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ToastMessage)]
public class ToastMessage : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ToastMessage;
    [Key(1)] public Guid NotificationId { get; init; }
    [Key(2)] public string Message { get; init; } = string.Empty;
    [Key(3)] public string ToastType { get; init; } = "info";
    [Key(4)] public int DurationMs { get; init; } = 4000;
    [Key(5)] public string? Icon { get; init; }
    [Key(6)] public NotificationActionDto? Action { get; init; }
    [Key(7)] public long CreatedAt { get; init; }
    [Key(8)] public string? LocalizationKey { get; init; }
    [Key(9)] public List<string>? LocalizationArgs { get; init; }
    [Key(10)] public bool RequiresAck { get; init; }
}
```

### Server-Verhalten
- Setzt `RequiresAck` nur für sicherheitsrelevante Toasts (z.B. AntiCheatWarning).
- Drosselt auf max. 10 Toasts pro Minute.
- Konvertiert in `NotificationShow` wenn Client offline (Inbox Fallback).

### Client-Verhalten
- Rendert Toast, auto-dismiss nach `DurationMs`.
- Bei `Action` klickbar → sendet NotificationDismiss (Reason=user) + führt Action aus.
- Bei `RequiresAck`: sendet Ack nach Render.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  ToastMessage (4320)         │
  │◄─────────────────────────────│
  │  Render + AutoDismiss        │
  │  NotificationDismiss (4301)  │ (wenn RequiresAck)
  │─────────────────────────────►│
  │  NotificationDismiss (Ack)   │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var xpToast = new ToastMessage
{
    NotificationId = Guid.NewGuid(),
    Message = "+350 XP",
    ToastType = "success",
    Icon = "xp_star",
    DurationMs = 3500,
    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `RATE_LIMITED` | Toast-Limit erreicht |
| `INVALID_CHANNEL` | Channel nicht Toast |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `NotificationShow` | 4300 | Alternativer Pfad |
| `ToastAchievement` | 4321 | Spezialisierung |
| `NotificationDismiss` | 4301 | Ack |

---

## ToastAchievement (4321)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spezieller Toast für Achievement-Unlocks mit zusätzlicher Telemetry (Points, Icon). Wird zusätzlich zu `AchievementUnlocked (1900)` gesendet.

### Im Scope ✅
- Achievement-UI animieren.
- Punkte + Icon anzeigen.
- Optional BadgeDelta auf AchievementBadge.

### Nicht im Scope ❌
- Vollständige Achievement-Daten (siehe Kategorie 19).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | ID | Ja |
| AchievementId | uint | Unlock-ID | Ja |
| Title | string | Anzeige | Ja |
| Points | int | Punkte | Ja |
| Icon | string | Icon-Key | Ja |
| CreatedAt | long | Zeit | Ja |
| RequiresAck | bool | Standard false | Ja |

### Erwartete Response
- Optional `NotificationDismiss` wenn `RequiresAck=true`.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ToastAchievement)]
public class ToastAchievement : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ToastAchievement;
    [Key(1)] public Guid NotificationId { get; init; }
    [Key(2)] public uint AchievementId { get; init; }
    [Key(3)] public string Title { get; init; } = string.Empty;
    [Key(4)] public int Points { get; init; }
    [Key(5)] public string Icon { get; init; } = string.Empty;
    [Key(6)] public long CreatedAt { get; init; }
    [Key(7)] public bool RequiresAck { get; init; }
}
```

### Server-Verhalten
- Konsistenz mit `AchievementUnlocked (1900)` sicherstellen (gleiche AchievementId).
- Setzt `RequiresAck` nur bei Tutorial/Onboarding.
- BadgeDelta auf `AchievementBadge` intern, aber nicht gesondert übertragen (Client berechnet aus Count).

### Client-Verhalten
- Spielt Achievement-Animation und Sound.
- Aktualisiert Achievement-Punkte lokal.
- Optional Ack senden.

### Beispiel Payloads
```csharp
var achieveToast = new ToastAchievement
{
    NotificationId = Guid.NewGuid(),
    AchievementId = 501,
    Title = "Kartograph",
    Points = 10,
    Icon = "ach_map",
    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `NOT_FOUND` | Achievement existiert nicht |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `AchievementUnlocked` | 1900 | Fach-Event |
| `ToastMessage` | 4320 | Generisch |

---

## ToastLevelUp (4322)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Toast für Levelaufstieg. Ergänzt `LevelUp (600)` (Kategorie 06) mit UI-spezifischen Daten.

### Im Scope ✅
- Anzeige neues Level.
- Belohnungen (freigeschaltete Abilities) optional.
- ScreenEffect (4352) optional.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | ID | Ja |
| NewLevel | int | Neues Level | Ja |
| Rewards | List<string>? | Freigeschaltete Features | Nein |
| Icon | string? | Icon | Nein |
| CreatedAt | long | Zeit | Ja |
| RequiresAck | bool | Default false | Ja |

### Erwartete Response
- Optional `NotificationDismiss` bei RequiresAck.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ToastLevelUp)]
public class ToastLevelUp : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ToastLevelUp;
    [Key(1)] public Guid NotificationId { get; init; }
    [Key(2)] public int NewLevel { get; init; }
    [Key(3)] public List<string>? Rewards { get; init; }
    [Key(4)] public string? Icon { get; init; }
    [Key(5)] public long CreatedAt { get; init; }
    [Key(6)] public bool RequiresAck { get; init; }
}
```

### Server-Verhalten
- Senden direkt nach `LevelUp (600)`.
- Optionale Kombination mit ScreenFlash (4352) für visuelle Wirkung.

### Client-Verhalten
- Spielt LevelUp-Animation und Sound.
- Aktualisiert UI (Talentpunkte etc.).
- Optional Ack.

### Beispiel Payloads
```csharp
var lvlToast = new ToastLevelUp
{
    NotificationId = Guid.NewGuid(),
    NewLevel = 25,
    Rewards = new() { "Talentpunkt +1" },
    Icon = "level_star",
    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `INVALID_LEVEL` | Level ungültig |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `LevelUp` | 600 | Primäre Stat-Message |
| `ScreenFlash` | 4352 | Optionaler Effekt |

---

## ToastLoot (4323)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (Loot)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Loot-Toast für Items/Gold. Ergänzt `LootItemResult (3103)` aus Kategorie 31 mit UI-Feedback.

### Im Scope ✅
- ItemName, Rarity, Quantity anzeigen.
- Stapelungen kombinieren (DedupeKey = ItemId + Quality).
- Optional Sound abhängig von Rarity.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | ID | Ja |
| ItemId | uint | Item | Ja |
| ItemName | string | Anzeige | Ja |
| Quantity | int | Anzahl | Ja |
| Quality | byte | 0-6 | Ja |
| Icon | string? | Icon-Key | Nein |
| CreatedAt | long | Zeit | Ja |

### Erwartete Response
- Keine (ephemer), optional `NotificationDismiss` wenn RequiresAck gesetzt.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ToastLoot)]
public class ToastLoot : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ToastLoot;
    [Key(1)] public Guid NotificationId { get; init; }
    [Key(2)] public uint ItemId { get; init; }
    [Key(3)] public string ItemName { get; init; } = string.Empty;
    [Key(4)] public int Quantity { get; init; }
    [Key(5)] public byte Quality { get; init; }
    [Key(6)] public string? Icon { get; init; }
    [Key(7)] public long CreatedAt { get; init; }
}
```

### Server-Verhalten
- DedupeKey = ItemId+Quality für schnelle Stacking-Toasts.
- Bei epischen Items: setzt Priority=High und sendet optional `ScreenFlash`.

### Client-Verhalten
- Stacked Anzeige pro DedupeKey.
- Farb-Coding nach Quality (Common=grau, Epic=violett).
- Spielt Loot-Sound.

### Beispiel Payloads
```csharp
var loot = new ToastLoot
{
    NotificationId = Guid.NewGuid(),
    ItemId = 10023,
    ItemName = "Glänzendes Schwert",
    Quantity = 1,
    Quality = 4,
    Icon = "sword_rare",
    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `NOT_FOUND` | Item unbekannt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `LootItemResult` | 3103 | Fach-Event |
| `ToastMessage` | 4320 | Generische Toasts |

---

## BossWarning (4330)

**Richtung:** 📥 Server → Client / 📡 Broadcast (Zone)  
**Frequenz:** Mittel (pro Mechanik)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Warnung vor Boss-Mechaniken. Wird zonenweit an alle relevanten Spieler gesendet. Kann Banner- oder ScreenEffect auslösen.

### Im Scope ✅
- Pre-Warnungs-Countdown (z.B. 5s bis Flächenangriff).
- Text + Marker (z.B. BossName).
- Lokalisation via Key.

### Nicht im Scope ❌
- Schadensberechnung (Combat-Kategorie).
- Persistente Speicherung (nicht erforderlich).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | ID | Ja |
| BossId | uint | Boss | Ja |
| WarningType | string | ability/phase/enrage | Ja |
| Message | string | Anzeige | Ja |
| CountdownSeconds | int? | Vorlaufzeit | Nein |
| Icon | string? | Icon | Nein |
| Priority | NotificationPriority | Standard High | Ja |

### Erwartete Response
- Keine (Event). Optional `NotificationDismiss` für Logging.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BossWarning)]
public class BossWarning : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BossWarning;
    [Key(1)] public Guid NotificationId { get; init; }
    [Key(2)] public uint BossId { get; init; }
    [Key(3)] public string WarningType { get; init; } = "ability";
    [Key(4)] public string Message { get; init; } = string.Empty;
    [Key(5)] public int? CountdownSeconds { get; init; }
    [Key(6)] public string? Icon { get; init; }
    [Key(7)] public NotificationPriority Priority { get; init; } = NotificationPriority.High;
}
```

### Server-Verhalten
- Broadcast nur an Spieler in Reichweite/Zone.
- Optional: triggert `ScreenFlash` oder `ScreenShake`.
- Anti-Spam: max 1/second/Boss per Zone.

### Client-Verhalten
- Zeigt Banner/Toast je nach UI-Settings.
- Spielt Warn-Sound, färbt UI.
- Markiert Boss-Attack Zones (falls vorhanden).

### Beispiel Payloads
```csharp
var warn = new BossWarning
{
    NotificationId = Guid.NewGuid(),
    BossId = 9001,
    WarningType = "ability",
    Message = "Feuersturm in 5s",
    CountdownSeconds = 5,
    Icon = "icon_fire"
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| `ZONE_NOT_FOUND` | Zone nicht gesetzt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `BossAbility` | 4331 | Folgt oft danach |
| `ScreenShake` | 4351 | Effekt |

---

## BossAbility (4331)

**Richtung:** 📥 Server → Client / 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Teilt mit, dass eine Boss-Fähigkeit jetzt ausgeführt wird. Wird mit Target-Information geliefert.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | ID | Ja |
| BossId | uint | Boss | Ja |
| AbilityId | uint | Fähigkeit | Ja |
| AbilityName | string | Anzeige | Ja |
| TargetId | Guid? | Ziel | Nein |
| Icon | string? | Icon | Nein |
| Priority | NotificationPriority | Normal/High | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BossAbility)]
public class BossAbility : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BossAbility;
    [Key(1)] public Guid NotificationId { get; init; }
    [Key(2)] public uint BossId { get; init; }
    [Key(3)] public uint AbilityId { get; init; }
    [Key(4)] public string AbilityName { get; init; } = string.Empty;
    [Key(5)] public Guid? TargetId { get; init; }
    [Key(6)] public string? Icon { get; init; }
    [Key(7)] public NotificationPriority Priority { get; init; } = NotificationPriority.High;
}
```

### Server-Verhalten
- Broadcast in betroffene Zone/Gruppe.
- Optionale Marker für TargetId (UI Hook).

### Client-Verhalten
- Zeigt UI-Timer/Marker an.
- Ruft BossMechanic UI.
- Kein Ack erforderlich.

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `BossWarning` | 4330 | Pre-Warnung |
| `BossPhase` | 4332 | Statusänderung |

---

## BossPhase (4332)

**Richtung:** 📥 Server → Client / 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Boss wechselt Phase. Client aktualisiert Boss-UI und Mechanik-Set.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | Guid | ID | Ja |
| BossId | uint | Boss | Ja |
| Phase | int | Neue Phase | Ja |
| PhaseName | string | Anzeigename | Ja |
| Priority | NotificationPriority | Normal | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BossPhase)]
public class BossPhase : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BossPhase;
    [Key(1)] public Guid NotificationId { get; init; }
    [Key(2)] public uint BossId { get; init; }
    [Key(3)] public int Phase { get; init; }
    [Key(4)] public string PhaseName { get; init; } = string.Empty;
    [Key(5)] public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
}
```

### Server-Verhalten
- Sendet nur bei tatsächlicher Phase-Änderung (dedupe).
- Optional `ScreenEffect` bei Phasenwechsel (z.B. Flash).

### Client-Verhalten
- Aktualisiert Boss-UI, Timer, Mechanik-Anzeigen.
- Verwaltet Stage-Musik/Visuals.

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `BossWarning` | 4330 | Übergang ankündigen |
| `ScreenFlash` | 4352 | Optionaler Effekt |

---

## CountdownStart (4340)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet einen Countdown (z.B. Raid-Pull, Event-Start, PvP-Start). Client zeigt UI-Timer und optional Sound.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CountdownId | Guid | ID | Ja |
| DurationSeconds | int | Gesamtdauer | Ja |
| Message | string | Anzeige | Ja |
| Category | string | z.B. raid/pvp/system | Ja |
| StartTimestamp | long | Unix ms | Ja |

### Erwartete Response
- Keine; Client sendet optional `NotificationDismiss` wenn lokal abgebrochen.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CountdownStart)]
public class CountdownStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CountdownStart;
    [Key(1)] public Guid CountdownId { get; init; }
    [Key(2)] public int DurationSeconds { get; init; }
    [Key(3)] public string Message { get; init; } = string.Empty;
    [Key(4)] public string Category { get; init; } = "system";
    [Key(5)] public long StartTimestamp { get; init; }
}
```

### Server-Verhalten
- Broadcast an alle relevanten Spieler (z.B. Raid-Gruppe).
- Persistiert nicht; bei Reconnect wird Countdown über verbleibende Zeit rekonstruiert (DeltaVersion).

### Client-Verhalten
- Startet Timer; bei Reconnect justiert mit `StartTimestamp + Duration`.
- Spielt Pull-Sound (Raid).
- Kann lokal abbrechen (UI), sendet `NotificationDismiss` optional.

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `CountdownUpdate` | 4341 | Tick-Updates |
| `CountdownCancel` | 4342 | Abbruch |

---

## CountdownUpdate (4341)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (1/s)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert verbleibende Sekunden eines laufenden Countdowns. Dient zur Korrektur bei Latenz/Jitter.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CountdownId | Guid | ID | Ja |
| SecondsRemaining | int | Restzeit | Ja |
| ServerTime | long | Unix ms | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CountdownUpdate)]
public class CountdownUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CountdownUpdate;
    [Key(1)] public Guid CountdownId { get; init; }
    [Key(2)] public int SecondsRemaining { get; init; }
    [Key(3)] public long ServerTime { get; init; }
}
```

### Server-Verhalten
- Sendet alle 1s an betroffene Clients.
- Stoppt, wenn `SecondsRemaining <=0` oder `CountdownCancel` gesendet.

### Client-Verhalten
- Korrigiert Timer-Drift.
- Bei Sprung rückwärts >1s: Blendet UI Soft-Correct ein.

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `CountdownStart` | 4340 | Start |
| `CountdownCancel` | 4342 | Ende |

---

## CountdownCancel (4342)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Beendet einen laufenden Countdown (z.B. Pull abgebrochen).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CountdownId | Guid | ID | Ja |
| Reason | string | aborted/completed | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CountdownCancel)]
public class CountdownCancel : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CountdownCancel;
    [Key(1)] public Guid CountdownId { get; init; }
    [Key(2)] public string Reason { get; init; } = "aborted";
}
```

### Server-Verhalten
- Stoppt Updates, setzt Reason.
- Optional sendet `NotificationShow` (banner) mit Abbruch-Grund.

### Client-Verhalten
- Entfernt Countdown-UI.
- Bei Reason=completed → spielt Erfolgssound.

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `CountdownStart` | 4340 | Start |
| `CountdownUpdate` | 4341 | Zwischenstände |

---

## ScreenEffect (4350)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Generischer Screen-Effekt (Shake/Flash/Fade/Blur). Wird genutzt für Boss-Warnungen, Crit-Hits, World-Events.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EffectType | string | shake/flash/fade/blur | Ja |
| Intensity | float | 0.0-1.0 | Ja |
| DurationMs | int | ms | Ja |
| Color | string? | Hex | Nein |
| Easing | string? | easeInOut | Nein |
| NotificationId | Guid? | Optional ID für Dedupe | Nein |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ScreenEffect)]
public class ScreenEffect : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ScreenEffect;
    [Key(1)] public string EffectType { get; init; } = "shake";
    [Key(2)] public float Intensity { get; init; }
    [Key(3)] public int DurationMs { get; init; }
    [Key(4)] public string? Color { get; init; }
    [Key(5)] public string? Easing { get; init; }
    [Key(6)] public Guid? NotificationId { get; init; }
}
```

### Server-Verhalten
- Kann zusammen mit anderen Messages gesendet werden (z.B. BossWarning).
- Anti-Spam: max 1 Effekt pro 300ms.

### Client-Verhalten
- Führt Effekt aus; dedupliziert via NotificationId falls vorhanden.
- Berücksichtigt Accessibility (ScreenShake off).

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `ScreenShake` | 4351 | Spezialisierung |
| `ScreenFlash` | 4352 | Spezialisierung |
| `ScreenFade` | 4353 | Spezialisierung |

---

## ScreenShake (4351)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spezial-Effekt für Screen-Shake (Explosionen, Boss Slams).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Intensity | float | 0-1 | Ja |
| DurationMs | int | ms | Ja |
| Frequency | float? | Hz | Nein |
| Axis | string? | x/y/both | Nein |
| NotificationId | Guid? | Dedupe | Nein |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ScreenShake)]
public class ScreenShake : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ScreenShake;
    [Key(1)] public float Intensity { get; init; }
    [Key(2)] public int DurationMs { get; init; }
    [Key(3)] public float? Frequency { get; init; }
    [Key(4)] public string? Axis { get; init; }
    [Key(5)] public Guid? NotificationId { get; init; }
}
```

### Server-Verhalten
- Häufig gekoppelt an `BossAbility`.
- Reduziert Intensität je nach Distanz (optional).

### Client-Verhalten
- Skaliert Intensität mit Einstellungen (Accessibility).
- Unterdrückt wenn UI-Safety aktiv.

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `ScreenEffect` | 4350 | Generisch |

---

## ScreenFlash (4352)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Vollbild-Flash (z.B. Boss-Phase-Wechsel, Achievement). Kann Farbe steuern.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Color | string | Hex-Farbe | Ja |
| DurationMs | int | ms | Ja |
| Opacity | float? | 0-1 | Nein |
| NotificationId | Guid? | Dedupe | Nein |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ScreenFlash)]
public class ScreenFlash : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ScreenFlash;
    [Key(1)] public string Color { get; init; } = "#FFFFFF";
    [Key(2)] public int DurationMs { get; init; }
    [Key(3)] public float? Opacity { get; init; }
    [Key(4)] public Guid? NotificationId { get; init; }
}
```

### Server-Verhalten
- Optional bei LevelUp/PhaseChange.
- Accessibility: deaktiviert, wenn Spieler Photosensitivity-Schutz aktiv hat.

### Client-Verhalten
- Blendet Farbe über Bildschirm.
- Berücksichtigt Opacity.

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `ScreenEffect` | 4350 | Generisch |
| `ScreenFade` | 4353 | Langsamer Effekt |

---

## ScreenFade (4353)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fade-In/Fade-Out Effekt für Szenenwechsel, Cutscenes oder Teleports.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FadeIn | bool | true=FadeIn, false=FadeOut | Ja |
| DurationMs | int | ms | Ja |
| Color | string | Hex | Ja |
| NotificationId | Guid? | Dedupe | Nein |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ScreenFade)]
public class ScreenFade : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ScreenFade;
    [Key(1)] public bool FadeIn { get; init; }
    [Key(2)] public int DurationMs { get; init; }
    [Key(3)] public string Color { get; init; } = "#000000";
    [Key(4)] public Guid? NotificationId { get; init; }
}
```

### Server-Verhalten
- Wird oft vor/after Teleport oder Cutscene gesendet.
- Kombiniert mit `ScreenEffect` für komplexere Übergänge.

### Client-Verhalten
- Startet Fade; blockiert Eingabe optional während FadeOut.
- Dedupliziert via NotificationId.

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| `ScreenFlash` | 4352 | Alternative |
| `CutsceneStart` | 4400 | Nachfolgend |

---

## 🗑️ Obsolete Messages

| Message | ID | Grund |
| ------- | -- | ----- |
| _Keine_ | — | Alle Messages in 4300–4399 sind aktiv. |

---

## 🧨 Edge Cases & Fehlerfälle

- **Lost Acks**: Wenn Client nach `NotificationShow` disconnected, Server speichert Notification als Inbox (Persistent=true) und sendet Snapshot bei Reconnect.  
- **Duplicate NotificationId**: Client ignoriert, server loggt Warnung, kein UI-Doppler.  
- **Badge Drift**: Wenn Badge-Zähler negativ wird, Server setzt auf 0 und sendet Full Snapshot.  
- **Modal Timeout**: Server sendet `AlertDismiss` mit Result=timeout, optional Kick wenn sicherheitskritisch.  
- **Rate Limit Breach**: Server verwirft zusätzliche Toasts, sendet `RateLimitWarning (917)` und optional Inbox-Eintrag.  
- **Localization Missing**: Client zeigt Fallback-Body; sendet Telemetry.  
- **Accessibility Off**: ScreenShake/Flash unterdrückt; NotificationDismiss wird trotzdem geschickt wenn RequiresAck.  
- **Multi-Session Conflict**: Bei parallelen Logins gewinnt neueste Session; alte Session erhält `ForceDisconnect (5)` bevor NotificationQueue zugestellt wird.  
- **Boss Warnings ohne BossContext**: Wird gedroppt und server loggt.  
- **Countdown Drifts**: Wenn Clock Drift >1s erkannt, Client sendet Diagnostik-Event (intern) und resynchronisiert mit ServerTime.  
- **Inbox Overflow**: Bei >500 offenen Einträgen älteste Items archivieren (Server), Client erhält Snapshot mit gekürzter Liste + Hinweis.  
- **Action Payload Validation**: Server validiert Payload gegen erlaubte Keys; invalid → `UNAUTHORIZED`.  

---

## Flows (ASCII)

### Push: NotificationShow → Ack
```
Client                         Server
  │                              │
  │  NotificationShow            │
  │◄─────────────────────────────│
  │  UI anzeigen                 │
  │  NotificationDismiss         │ (bei Ack-Pflicht)
  │─────────────────────────────►│
  │  NotificationDismiss (Ack)   │
  │◄─────────────────────────────│
```

### Inbox Sync: NotificationInboxSyncRequest (implizit bei Login) → NotificationQueue
```
Client                         Server
  │                              │
  │  Login/Rejoin                │
  │─────────────────────────────►│
  │                              │  Lade persistente Notifications
  │  NotificationQueue (Full)    │
  │◄─────────────────────────────│
  │  Apply + Acks falls nötig    │
```

### Delete/Dismiss: NotificationDismissRequest → Ack → NotificationDismissedEvent (optional)
```
Client                         Server
  │                              │
  │ NotificationDismiss          │
  │─────────────────────────────►│
  │                              │  Persist Ack
  │ NotificationDismiss (Ack)    │
  │◄─────────────────────────────│
  │                              │  (optional) Broadcast removal
```

### Badge Count: NotificationQueue Delta → UI Updates
```
Client                         Server
  │                              │
  │  NotificationQueue (Delta)   │ (Badges updated)
  │◄─────────────────────────────│
  │  Update Badge UI             │
```

### Boss Warning + ScreenEffect
```
Client                         Server
  │                              │
  │  BossWarning (4330)          │
  │◄─────────────────────────────│
  │  ScreenShake (4351)          │
  │◄─────────────────────────────│
  │  UI reagiert                 │
```

---

## 📎 Anhang (MessageType Enum Updates)

Es wurden **keine neuen MessageType-Einträge** hinzugefügt. Zur Vollständigkeit hier die relevanten Zeilen aus `MessageType.cs` (4300-4353):

```csharp
// NOTIFICATIONS / ALERTS (4300-4399)
NotificationShow = 4300,
NotificationDismiss = 4301,
NotificationQueue = 4302,
AlertPopup = 4310,
AlertConfirm = 4311,
AlertDismiss = 4312,
ToastMessage = 4320,
ToastAchievement = 4321,
ToastLevelUp = 4322,
ToastLoot = 4323,
BossWarning = 4330,
BossAbility = 4331,
BossPhase = 4332,
CountdownStart = 4340,
CountdownUpdate = 4341,
CountdownCancel = 4342,
ScreenEffect = 4350,
ScreenShake = 4351,
ScreenFlash = 4352,
ScreenFade = 4353,
```

---

**Letzte Aktualisierung:** 2026-01-03  
**Version:** 3.0.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/43-notification.md
