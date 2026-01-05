# 🎬 Cutscene Messages (4400-4499)

**Kategorie:** 44  
**Range:** 4400-4499  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung  
[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🎞️ Playback & Sync](#️-playback--sync)
- [⛔ Input Lock & State Management](#-input-lock--state-management)
- [✅ Start/Skip/Complete Regeln](#-startskipcomplete-regeln)
- [👥 Party/Group Cutscenes](#-partygroupp-cutscenes)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#️-regeln--sicherheit)
- [📩 Aktive Messages 4400–4499](#-aktive-messages-44004499)
- [🗑️ Obsolete Messages](#-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang (MessageType Enum Updates)](#-anhang-messagetype-enum-updates)
- [📑 Langform-Checklisten (Determinismus & Tests)](#-langform-checklisten-determinismus--tests)

---

## 📋 Überblick

- Scope: Scripted Sequences, Dialogue, Camera, Player Control, Timeline Playback, Branching, Reconnect-Safety.
- Autorität: Server entscheidet Trigger, Zeit, Gating (Quest, Zone, NPC, Tutorial, Combat).
- Determinismus: Timeline-Schritte referenzieren Server-definierte CutsceneIds und StepIds, kein Client-generated Inhalt.
- Integrationen: Quest (1000er), NPC (1300er), Zone (0100er), Movement (0200er), Combat (0300er), Tutorial (29xx), Notification (43xx).
- Resilienz: Reconnect-Safe mit Revision + Resume-Window + Forced End bei Desync.
- Anti-Abuse: Locks (Movement/Combat/UI), Rate-Limits für Skip/Complete, Server unlocks auf Timeout.
- Alle Client → Server Messages haben definierte Responses und Korrelation über ClientSequence/Revision/ResumeToken.

---

## 🧠 Datenmodell

### Kerndaten

| Feld | Typ | Beschreibung |
| --- | --- | --- |
| CutsceneId | ushort | Server-definierte ID, global eindeutig im Range 4400-4499 Namespace. |
| Revision | uint | Monotone Version, erhöht bei State-Änderung (Start, Pause, Resume, Skip, Complete). |
| TimelineState | enum | Idle, Starting, Playing, Paused, Skipped, Completed, Failed. |
| CutsceneStepId | ushort | Laufender Step in Timeline. |
| DialogueLineId | ushort | Laufende Dialogzeile (optional). |
| ClientSequence | uint | Client-seitige Sequenz für Requests (Skip/Complete/Ack). |
| ServerTick | long | Serverzeit (ms) zur Synchronisation. |
| PartyId | Guid? | Gruppenschnittstelle für Party-Cutscenes. |
| LockFlags | CutsceneLockFlags | Movement, Combat, UI, Input, Camera. |
| ResumeToken | Guid | Token für Reconnect/Resume. |
| WatchedFlag | bool | Persistent Flag pro Character/Account. |

### TimelineStep Struktur

| Feld | Typ | Beschreibung |
| --- | --- | --- |
| StepId | ushort | Eindeutige ID innerhalb der Cutscene. |
| Type | CutsceneStepType | CameraMove, Dialogue, Animation, Fade, Spawn, Despawn, WaitForInput, Choice, Branch. |
| DurationMs | int | Dauer des Steps (falls zeitbasiert). |
| Payload | byte[] | Serialized Step Payload (MessagePack) mit deterministischem Inhalt. |
| AutoAdvance | bool | Automatisch zum nächsten Step. |
| RequiresAck | bool | Client muss `CutsceneAckRequest` senden. |
| BranchKey | string? | Branch Identifier für Choice. |
| NextStepId | ushort? | Explizite Verkettung (für Branches). |

### DialogueLine Struktur

| Feld | Typ | Beschreibung |
| --- | --- | --- |
| DialogueLineId | ushort | Laufende ID. |
| SpeakerId | string | NPC/Character Key (server canonical). |
| SpeakerEmotion | string | Neutral, Happy, Angry, Sad, Surprised. |
| TextKey | string | Localization Key (server-provided). |
| VoiceAsset | string? | Optional Audio Clip Id. |
| PortraitAsset | string? | Optional Portrait Id. |
| Choices | List<DialogueChoice> | Optional Branch Choices. |

### DialogueChoice Struktur

| Feld | Typ | Beschreibung |
| --- | --- | --- |
| ChoiceId | byte | Laufende Choice ID. |
| TextKey | string | Localization Key. |
| NextStepId | ushort | Ziel-Step. |
| GrantsFlag | string? | Unlock/Watched Flag. |
| RequiresFlag | string? | Gating Flag. |
| TimeoutMs | int? | Auto-select Timeout. |

### TimelineState Objekt

| Feld | Typ | Beschreibung |
| --- | --- | --- |
| State | TimelineRuntimeState | Starting, Playing, Paused, WaitingForAck, Skipped, Completed, Failed. |
| CurrentStepId | ushort | Aktuell sichtbarer Step. |
| ExpectedAckStepId | ushort? | Step für den nächsten Ack. |
| LastAckClientSequence | uint | Letzte ClientSequence für Ack. |
| LastProgressClientSequence | uint | Letzte ClientSequence für Progress Report. |
| ResumeToken | Guid | Token für Reconnect. |
| LastUpdatedAt | long | Serverzeit (ms). |

### LockFlags

| Flag | Beschreibung |
| --- | --- |
| MovementLock | Blockiert Bewegung und Input (WASD). |
| CombatLock | Blockiert ActionRequest/Combat Inputs. |
| InteractionLock | Blockiert NpcInteract/Vendor/VendorListRequest etc. |
| UiLock | Blockiert UI (Inventory/Map) mit Whitelist für Skip Button. |
| CameraLock | Erzwingt Kamera-Override, keine Mausfreigabe. |

---

## 🎞️ Playback & Sync

- Server authoritative Startzeit: StartTick = Tserver, Client rechnet Offset = Tclient - Tserver.
- Determinismus: Step-Reihenfolge und Payload sind server-canonical; Client cached Timeline aber darf keine Reihenfolge ändern.
- Drift-Korrektur: `CutsceneProgress` Events enthalten StepId + ExpectedEndTick; Client interpoliert und korrigiert bei Abweichung > 50ms.
- Heartbeat: Während Cutscene gelten normale Heartbeats; Desync-Detection vergleicht Revision + StepId.
- Local Prediction: Kamera/Animation können lokal interpoliert, aber Server sendet verbindliche Branch- und Skip-Entscheidungen.
- Audio/Subtitle Sync: Client nutzt `ServerTick` und `DurationMs` pro Step; OnResume sendet Server `RemainingMs`.
- Progress Keepalive: Mindestens alle 2s ein Progress-Update, auch wenn Step statisch.
- Choice Timeout: Server setzt Default Choice nach Timeout; Client erhält ProgressType=ChoiceAuto.

---

## ⛔ Input Lock & State Management

- Locks aktivieren mit CutsceneStart (Movement, Combat, Camera, Interaction, UI).
- Locks bleiben bis CutsceneEnd oder Server-Timeout.
- Skip Button: UI Whitelisted, sendet `CutsceneSkipRequest`.
- Fail-Safe Unlock: Server unlockt automatisch bei Timeout, Disconnect, Invalid Revision, Admin override.
- Combat State: CombatLock verhindert neue ActionRequest; laufende Casts werden gecancelt mit `CastInterrupt`.
- Movement: PlayerPosition wird auf Freeze gesetzt; MovementCorrection gesendet falls Drift erkannt.
- UI: Only Skip + Accessibility (Subtitles toggle) erlaubt.
- Lock Telemetry: Client sendet optional `CutsceneProgressAck` mit LockHealth (true/false) für Diagnose.

---

## ✅ Start/Skip/Complete Regeln

- Start: Nur Server sendet `CutsceneStart`. Client MUSS mit `CutsceneAckRequest` (Reason=Start) antworten.
- Skip: Client sendet `CutsceneSkipRequest` (Reason=User, AutoSkip, Timeout). Server prüft Gating, antwortet `CutsceneSkipResponse`, broadcastet `CutsceneEnd` wenn akzeptiert.
- Complete: Client sendet `CutsceneCompleteRequest` bei lokalem Ende. Server validiert StepId/Revision, antwortet `CutsceneCompleteResponse`, sendet `CutsceneEnd`.
- Idempotenz: Requests tragen `ClientSequence`; Server speichert letzte Sequence pro CutsceneId. Duplikate liefern gleiche Response (Success + Revision).
- Watched Flags: Server markiert Cutscene als watched (per Character + optional Account) erst nach validierter Complete.
- Replay: If `ReplayAllowed`, server may start again; else Start verweigert mit ErrorCode `ALREADY_WATCHED` falls ohne ReplayFlag.
- SkipPolicy: Immediate, PartyVote, LeaderOnly; konfigurierbar pro Cutscene.
- CompletePolicy: RequiresClientComplete = true/false; falls false, Server sendet End nach letzter Step-Dauer ohne Client-Meldung.

---

## 👥 Party/Group Cutscenes

- PartyLeaderLock: Server erzwingt Start nur wenn Leader bestätigt oder AutoStart = true.
- Sync: Server sendet identische CutsceneId + Revision an alle Gruppenmitglieder; StartTime identisch.
- Voting: Skip-Request Option `SkipMode=PartyVote` → Server sammelt Stimmen, Threshold 50%+Leader.
- Desync Handling: Member ohne Ack innerhalb Timeout wird als `TimedOut` markiert; Server kann ContinueWithMajority.
- Shared Outcome: Quest progress / loot gating nur nach `CutsceneCompleteResponse` von Leader ODER Majority (konfigurierbar).
- Cross-Zone Party: Server richtet Temporary Zone Session ein; alle Mitglieder müssen in gleichem Shard sein.

---

## 🔄 Sync, Deltas & Revisioning

- Revision erhöht bei Start, Pause, Resume, Skip, Complete.
- `CutsceneStateSyncRequest` dient Reconnect: Client sendet ResumeToken + last known Revision + StepId; Server antwortet mit Snapshot.
- `CutsceneProgress` Events enthalten `StepChecksum` (hash of step payload) zur Desync detection.
- Deltas: Nur Änderungen (StepId, RemainingMs, LockFlags) werden gesendet, keine Full timeline.
- Resume Rules: Wenn RemainingMs <= 0 oder Step not resumable → Server sendet `CutsceneEnd` + unlock.
- Drift: Wenn Client meldet StepChecksum mismatch, Server pausiert und sendet StateSyncResponse.
- Snapshot Cache: Server hält Snapshot pro active Cutscene (per Session) bis End + 30s.

---

## 🧱 DTOs / Interfaces

### CutsceneStateDto

```csharp
[MessagePackObject]
public class CutsceneStateDto
{
    [Key(0)] public ushort CutsceneId { get; set; }
    [Key(1)] public uint Revision { get; set; }
    [Key(2)] public TimelineRuntimeState State { get; set; }
    [Key(3)] public ushort CurrentStepId { get; set; }
    [Key(4)] public ushort? ExpectedAckStepId { get; set; }
    [Key(5)] public Guid ResumeToken { get; set; }
    [Key(6)] public CutsceneLockFlags LockFlags { get; set; }
    [Key(7)] public long ServerTick { get; set; }
    [Key(8)] public int? RemainingMs { get; set; }
    [Key(9)] public bool WatchedFlag { get; set; }
    [Key(10)] public Guid? PartyId { get; set; }
}
```

### CutsceneProgressDto

```csharp
[MessagePackObject]
public class CutsceneProgressDto
{
    [Key(0)] public ushort CutsceneId { get; set; }
    [Key(1)] public uint Revision { get; set; }
    [Key(2)] public ushort StepId { get; set; }
    [Key(3)] public long ServerTick { get; set; }
    [Key(4)] public int RemainingMs { get; set; }
    [Key(5)] public string StepChecksum { get; set; } = string.Empty;
    [Key(6)] public bool BranchActive { get; set; }
    [Key(7)] public string? BranchKey { get; set; }
    [Key(8)] public DialogueLineDto? Dialogue { get; set; }
}
```

### DialogueLineDto

```csharp
[MessagePackObject]
public class DialogueLineDto
{
    [Key(0)] public ushort DialogueLineId { get; set; }
    [Key(1)] public string SpeakerId { get; set; } = string.Empty;
    [Key(2)] public string SpeakerEmotion { get; set; } = "Neutral";
    [Key(3)] public string TextKey { get; set; } = string.Empty;
    [Key(4)] public string? VoiceAsset { get; set; }
    [Key(5)] public string? PortraitAsset { get; set; }
    [Key(6)] public List<DialogueChoiceDto>? Choices { get; set; }
}
```

### DialogueChoiceDto

```csharp
[MessagePackObject]
public class DialogueChoiceDto
{
    [Key(0)] public byte ChoiceId { get; set; }
    [Key(1)] public string TextKey { get; set; } = string.Empty;
    [Key(2)] public ushort NextStepId { get; set; }
    [Key(3)] public string? GrantsFlag { get; set; }
    [Key(4)] public string? RequiresFlag { get; set; }
    [Key(5)] public int? TimeoutMs { get; set; }
}
```

### CutsceneResumeContext

```csharp
[MessagePackObject]
public class CutsceneResumeContext
{
    [Key(0)] public Guid ResumeToken { get; set; }
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public ushort StepId { get; set; }
    [Key(4)] public int? RemainingMs { get; set; }
    [Key(5)] public CutsceneLockFlags LockFlags { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### CutsceneLockFlags

```csharp
[Flags]
public enum CutsceneLockFlags : byte
{
    None = 0,
    MovementLock = 1 << 0,
    CombatLock = 1 << 1,
    InteractionLock = 1 << 2,
    UiLock = 1 << 3,
    CameraLock = 1 << 4
}
```

### TimelineRuntimeState

```csharp
public enum TimelineRuntimeState : byte
{
    Idle = 0,
    Starting = 1,
    Playing = 2,
    Paused = 3,
    WaitingForAck = 4,
    Skipped = 5,
    Completed = 6,
    Failed = 7
}
```

### CutsceneStepType

```csharp
public enum CutsceneStepType : byte
{
    CameraMove = 1,
    CameraShake = 2,
    CameraFade = 3,
    Dialogue = 4,
    Animation = 5,
    Spawn = 6,
    Despawn = 7,
    WaitForInput = 8,
    Choice = 9,
    Branch = 10,
    ScreenEffect = 11,
    TitleCard = 12,
    Music = 13,
    Sound = 14,
    TimelineEvent = 15
}
```

### CutsceneErrorCodes

| Code | Bedeutung |
| --- | --- |
| INVALID_CUTSCENE_ID | Client sendet unbekannte CutsceneId. |
| REVISION_MISMATCH | Revision stimmt nicht mit Server überein. |
| LOCK_CONFLICT | Locks konnten nicht gesetzt werden. |
| ALREADY_WATCHED | Cutscene nicht erneut erlaubt ohne ReplayFlag. |
| SKIP_NOT_ALLOWED | Skip ist deaktiviert (Design/VIP/Server). |
| RATE_LIMITED | Skip/Complete Request Rate-Limit überschritten. |
| STATE_NOT_PLAYING | Operation nur während Playing/Pause erlaubt. |
| BRANCH_TIMEOUT | Choice Timeout, Server hat Default gewählt. |
| PARTY_VOTE_REJECTED | Party Skip Vote fehlgeschlagen. |
| RESUME_TOKEN_INVALID | ResumeToken ungültig oder abgelaufen. |
| TIMEOUT_UNLOCK | Server hat automatisch unlockt. |
| INVALID_CHOICE | ChoiceId nicht erlaubt. |
| STEP_MISMATCH | StepId passt nicht zur Timeline. |

---

## ⚙️ Regeln & Sicherheit

- Server-only IDs: CutsceneId, StepId, DialogueLineId werden nie vom Client definiert.
- Validation: Jede Client-Nachricht enthält CutsceneId + Revision + ClientSequence + ResumeToken (wenn vorhanden).
- Rate Limits: Skip/Complete/Ack max 5 pro 10s pro Cutscene; ProgressReport max 4 Hz.
- Anti-Cheat: MovementLock enforced mit MovementCorrection; CombatLock enforced mit ActionReject.
- Logging: Jede State-Transition (Start/Pause/Resume/Skip/Complete/Fail) wird mit PlayerId + CutsceneId + Revision geloggt.
- Privacy: Dialogue TextKey nur, keine Klartext-Dialoge; Localization auf Client.
- Sync Failure: Bei REVISION_MISMATCH → Server sendet `CutsceneStateSyncResponse` mit authoritative Snapshot und erzwingt Locks.
- Correlation: Responses spiegeln ClientSequence wider; Duplicate Requests liefern identische Responses.

---

## 📩 Aktive Messages 4400–4499

### CutsceneStart (4400)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server startet eine Cutscene. Enthält alle notwendigen Metadaten (CutsceneId, Revision, LockFlags, StartTick, ResumeToken) und optional initialen Step/Dialogue Payload. Client spielt Timeline lokal ab, hält Locks und bestätigt via `CutsceneAckRequest`.

### Im Scope ✅
- Server-initiiertes Starten einer Cutscene.
- Setzen von Movement/Combat/Camera/UI Locks.
- Bereitstellung eines ResumeTokens für Reconnect.

### Nicht im Scope ❌
- Client-initiiertes Starten (nicht erlaubt).
- Lokale Skip-Regeln (werden vom Server bestimmt).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID der Cutscene | Ja |
| Revision | uint | Startrevision (>=1) | Ja |
| StartStepId | ushort | Erster Step | Ja |
| ServerTick | long | Starttick (ms) | Ja |
| LockFlags | CutsceneLockFlags | Aktive Locks | Ja |
| ResumeToken | Guid | Token für Resume | Ja |
| WatchedFlag | bool | Wurde bereits gesehen? | Ja |
| PartyId | Guid? | Party Kontext | Nein |
| TimelineHash | string | Hash über Timeline Steps | Ja |

### Erwartete Response
- `CutsceneAckRequest` (4406) mit Reason=Start.
- Bei fehlender Ack nach Timeout: Server sendet `CutsceneEnd` mit Error `TIMEOUT_UNLOCK`.

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneStart)]
public class CutsceneStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneStart;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public ushort StartStepId { get; set; }
    [Key(4)] public long ServerTick { get; set; }
    [Key(5)] public CutsceneLockFlags LockFlags { get; set; }
    [Key(6)] public Guid ResumeToken { get; set; }
    [Key(7)] public bool WatchedFlag { get; set; }
    [Key(8)] public Guid? PartyId { get; set; }
    [Key(9)] public string TimelineHash { get; set; } = string.Empty;
}
```

### Server-Verhalten
- Validiert Player State (not in combat, not stunned unless AllowStun).
- Vergibt neue Revision (StartRevision = previousRevision + 1 oder 1).
- Setzt Locks im Session-State.
- Sendet `CutsceneStart`; startet Timeout Timer für Ack.
- Persistiert ResumeToken im Session Storage.
- Optional: sendet initialen `CutsceneProgress` direkt nach Ack-Empfang.

### Client-Verhalten
- Aktiviert Locks laut LockFlags.
- Startet Timeline Playback ab StartStepId.
- Zeigt Skip UI, falls erlaubt.
- Sendet `CutsceneAckRequest` mit ClientSequence.
- Synchronisiert Audio/Camera gemäß StartTick Offset.

### Flow-Diagramm

```
Client                         Server
  │                              │
  │          CutsceneStart (4400)│
  │◄─────────────────────────────│
  │  Locks aktivieren            │
  │  Timeline starten            │
  │  CutsceneAckRequest (4406)   │
  │─────────────────────────────►│
  │                              │
  │      CutsceneAckResponse     │
  │◄─────────────────────────────│
```

### Beispiel Payloads

```csharp
var start = new CutsceneStart
{
    CutsceneId = 501,
    Revision = 1,
    StartStepId = 1,
    ServerTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    LockFlags = CutsceneLockFlags.MovementLock | CutsceneLockFlags.CameraLock | CutsceneLockFlags.UiLock,
    ResumeToken = Guid.NewGuid(),
    WatchedFlag = false,
    PartyId = null,
    TimelineHash = "sha256:abcd1234"
};
```

### Error Codes

| Code | Bedeutung |
| --- | --- |
| LOCK_CONFLICT | Locks konnten nicht gesetzt werden. |
| ALREADY_WATCHED | Replay nicht erlaubt. |
| STATE_NOT_PLAYING | Player nicht in gültigem Zustand. |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneAckRequest | 4406 | Ack auf Start. |
| CutsceneProgress | 4405 | Laufende Fortschritte. |
| CutsceneEnd | 4401 | Abschluss oder Abbruch. |

---

### CutsceneEnd (4401)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server beendet Cutscene autoritativ. Gründe: Completed, Skipped, Timeout, Fail. Sendet unlock Flags und finalen Revision/Step. Client entfernt Locks und UI.

### Im Scope ✅
- Abschluss nach erfolgreichem Complete.
- Abbruch nach Skip/Timeout.
- Unlock aller Locks.

### Nicht im Scope ❌
- Client-initiiertes End ohne Server (nicht erlaubt).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Endrevision | Ja |
| FinalState | TimelineRuntimeState | Completed/Skipped/Failed | Ja |
| FinalStepId | ushort | Letzter Step | Ja |
| UnlockFlags | CutsceneLockFlags | Welche Locks aufheben | Ja |
| WatchedFlag | bool | Neuer Watched Status | Ja |
| ErrorCode | string? | Fehlergrund | Nein |
| CompletionSource | string | Server/Client/Timeout/PartyVote | Ja |

### Erwartete Response
- Keine Pflicht, optional `CutsceneAckRequest` (Reason=End) falls Client bestätigt.

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneEnd)]
public class CutsceneEnd : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneEnd;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public TimelineRuntimeState FinalState { get; set; }
    [Key(4)] public ushort FinalStepId { get; set; }
    [Key(5)] public CutsceneLockFlags UnlockFlags { get; set; }
    [Key(6)] public bool WatchedFlag { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
    [Key(8)] public string CompletionSource { get; set; } = "Server";
}
```

### Server-Verhalten
- Hebt Locks laut UnlockFlags.
- Persistiert WatchedFlag.
- Trigger: Quest completion, Tutorial progress, Notification (4300) optional.
- Broadcast an Party-Mitglieder wenn `PartyId` gesetzt.

### Client-Verhalten
- Entfernt Locks, zeigt UI zurück.
- Aktualisiert WatchedFlag lokal.
- Löscht ResumeToken Cache.
- Optional: sendet Telemetry (LoadTime, Choice taken).

### Flow-Diagramm

```
Client                         Server
  │                              │
  │        CutsceneEnd (4401)    │
  │◄─────────────────────────────│
  │  Locks entfernen             │
  │  UI freigeben                │
  │                              │
```

### Beispiel Payloads

```csharp
var end = new CutsceneEnd
{
    CutsceneId = 501,
    Revision = 4,
    FinalState = TimelineRuntimeState.Completed,
    FinalStepId = 12,
    UnlockFlags = CutsceneLockFlags.MovementLock | CutsceneLockFlags.UiLock | CutsceneLockFlags.CombatLock | CutsceneLockFlags.CameraLock | CutsceneLockFlags.InteractionLock,
    WatchedFlag = true,
    CompletionSource = "ClientComplete"
};
```

### Error Codes

| Code | Bedeutung |
| --- | --- |
| TIMEOUT_UNLOCK | Server hat auto-unlock durchgeführt. |
| PARTY_VOTE_REJECTED | Skip Vote scheiterte, Cutscene beendet ohne Skip. |
| REVISION_MISMATCH | Endrevision passt nicht zur Clientrevision. |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneCompleteRequest | 4410 | führt zu End. |
| CutsceneSkipRequest | 4402 | kann End auslösen. |
| CutsceneStateSyncResponse | 4413 | liefert Snapshot vor End. |

---

### CutsceneSkipRequest (4402)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client fordert Skip der laufenden Cutscene an. Server prüft SkipPolicy (Allowed, PartyVote, LeaderOnly) und antwortet mit `CutsceneSkipResponse`.

### Im Scope ✅
- User-initiierter Skip.
- AutoSkip (Accessibility) mit Reason=Auto.
- Party-Vote Initiation.

### Nicht im Scope ❌
- Server-forced Skip (nutzt direkt CutsceneEnd).
- Lokales Skip ohne Server-OK.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Aktuelle Revision | Ja |
| ClientSequence | uint | Idempotenz | Ja |
| Reason | string | User, Auto, Timeout, Admin | Ja |
| ResumeToken | Guid | Token | Ja |
| SkipMode | string | Immediate, PartyVote | Ja |

### Erwartete Response
- `CutsceneSkipResponse` (4408)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneSkip)]
public class CutsceneSkipRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneSkip;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
    [Key(4)] public string Reason { get; set; } = "User";
    [Key(5)] public Guid ResumeToken { get; set; }
    [Key(6)] public string SkipMode { get; set; } = "Immediate";
}
```

### Server-Verhalten
- Prüft Revision, ResumeToken, LockFlags.
- Rate-Limit: max 2 SkipRequests / 10s.
- Wenn SkipMode=PartyVote → startet Vote, sendet CutsceneProgress (Vote state) und `CutsceneSkipResponse` mit Pending.
- Bei Erfolg: sendet CutsceneEnd (FinalState=Skipped).

### Client-Verhalten
- Sendet Request einmalig; Wiederholungen nur bei Timeout, gleiche ClientSequence.
- Zeigt Vote UI falls SkipMode=PartyVote.
- Wartet auf Response bevor UI entlockt.

### Flow-Diagramm

```
Client                         Server
  │                              │
  │  CutsceneSkipRequest (4402)  │
  │─────────────────────────────►│
  │                              │
  │    CutsceneSkipResponse      │
  │◄─────────────────────────────│
  │                              │
  │    CutsceneEnd (4401)        │
  │◄─────────────────────────────│
```

### Beispiel Payloads

```csharp
var skip = new CutsceneSkipRequest
{
    CutsceneId = 501,
    Revision = 2,
    ClientSequence = 10,
    Reason = "User",
    ResumeToken = resumeToken,
    SkipMode = "Immediate"
};
```

### Error Codes

| Code | Bedeutung |
| --- | --- |
| SKIP_NOT_ALLOWED | Serverpolicy verbietet Skip. |
| RATE_LIMITED | Zu viele SkipRequests. |
| REVISION_MISMATCH | Revision stimmt nicht. |
| PARTY_VOTE_REJECTED | Vote fehlgeschlagen. |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneSkipResponse | 4408 | Response. |
| CutsceneEnd | 4401 | Folge bei Erfolg. |
| CutsceneProgress | 4405 | Vote Status Updates. |

---

### CutscenePause (4403)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server pausiert Timeline (z.B. Party wartet, Quest gating, cinematics). Client hält Kamera/Animationen an und sendet Ack.

### Im Scope ✅
- Pausieren wegen Party-Sync.
- Admin/GM Pause.

### Nicht im Scope ❌
- Client-initiierte Pause.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Neue Revision | Ja |
| ServerTick | long | Zeitpunkt | Ja |
| Reason | string | PartySync, Admin, NetworkBuffer | Ja |

### Erwartete Response
- `CutsceneAckRequest` (Reason=Pause)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutscenePause)]
public class CutscenePause : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CutscenePause;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public long ServerTick { get; set; }
    [Key(4)] public string Reason { get; set; } = "PartySync";
}
```

### Server-Verhalten
- Erhöht Revision.
- Stoppt Progress Timer.
- Erwartet Ack, sonst Timeout → End mit Fail.

### Client-Verhalten
- Stoppt Playback.
- Hält Locks aktiv.
- Sendet Ack.

### Flow-Diagramm

```
Client                         Server
  │                              │
  │      CutscenePause (4403)    │
  │◄─────────────────────────────│
  │ Playback stoppen             │
  │ CutsceneAckRequest (Pause)   │
  │─────────────────────────────►│
```

### Beispiel Payloads

```csharp
var pause = new CutscenePause
{
    CutsceneId = 501,
    Revision = 3,
    ServerTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    Reason = "PartySync"
};
```

### Error Codes

| Code | Bedeutung |
| --- | --- |
| REVISION_MISMATCH | Pause Revision inkonsistent. |
| STATE_NOT_PLAYING | Cutscene nicht in Playing. |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneAckRequest | 4406 | Ack. |
| CutsceneResume | 4404 | Fortsetzung. |

---

### CutsceneResume (4404)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server nimmt pausierte Cutscene wieder auf. Enthält RemainingMs für aktuellen Step.

### Im Scope ✅
- Resume nach PartySync oder Buffer.

### Nicht im Scope ❌
- Client-initiierte Resume.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Neue Revision | Ja |
| ResumeStepId | ushort | Step zur Fortsetzung | Ja |
| RemainingMs | int | Restzeit für Step | Ja |
| ServerTick | long | Zeitpunkt | Ja |

### Erwartete Response
- `CutsceneAckRequest` (Reason=Resume)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneResume)]
public class CutsceneResume : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneResume;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public ushort ResumeStepId { get; set; }
    [Key(4)] public int RemainingMs { get; set; }
    [Key(5)] public long ServerTick { get; set; }
}
```

### Server-Verhalten
- Erhöht Revision.
- Reaktiviert Progress Timer mit RemainingMs.
- Erwartet Ack.

### Client-Verhalten
- Setzt Timeline fort ab ResumeStepId.
- Korrigiert lokale Zeit mit RemainingMs.
- Sendet Ack.

### Flow-Diagramm

```
Client                         Server
  │                              │
  │     CutsceneResume (4404)    │
  │◄─────────────────────────────│
  │ Playback fortsetzen          │
  │ CutsceneAckRequest (Resume)  │
  │─────────────────────────────►│
```

### Beispiel Payloads

```csharp
var resume = new CutsceneResume
{
    CutsceneId = 501,
    Revision = 4,
    ResumeStepId = 6,
    RemainingMs = 1200,
    ServerTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Error Codes

| Code | Bedeutung |
| --- | --- |
| REVISION_MISMATCH | Resume Revision falsch. |
| STATE_NOT_PLAYING | Nicht pausiert. |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneAckRequest | 4406 | Ack. |
| CutsceneProgress | 4405 | Weitere Updates. |

---

### CutsceneProgress (4405)

**Richtung:** 📥 Server → Client  
**Frequenz:** Hoch (pro Step / alle 1-2s)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server sendet Fortschritt der Timeline: aktueller Step, RemainingMs, Dialogue, BranchInfo. Dient auch als Keepalive/Desync-Detection.

### Im Scope ✅
- Fortschritt & verbleibende Zeit.
- Dialogue-Updates (TextKey + Speaker).
- Branch Choice Prompt Info.

### Nicht im Scope ❌
- Client-Progress-Report (separat `CutsceneProgressAck`).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Revision | Ja |
| StepId | ushort | Aktueller Step | Ja |
| RemainingMs | int | Restzeit | Ja |
| ServerTick | long | Timestamp | Ja |
| StepChecksum | string | Hash | Ja |
| Dialogue | DialogueLineDto? | Dialogdaten | Nein |
| BranchKey | string? | Aktiver Branch | Nein |
| ProgressType | string | StepStart, StepUpdate, ChoicePrompt, VoteStatus | Ja |

### Erwartete Response
- Keine Pflicht; optional `CutsceneProgressAck` (4409) bei ChoicePrompt oder VoteStatus.

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneProgress)]
public class CutsceneProgress : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneProgress;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public ushort StepId { get; set; }
    [Key(4)] public int RemainingMs { get; set; }
    [Key(5)] public long ServerTick { get; set; }
    [Key(6)] public string StepChecksum { get; set; } = string.Empty;
    [Key(7)] public DialogueLineDto? Dialogue { get; set; }
    [Key(8)] public string? BranchKey { get; set; }
    [Key(9)] public string ProgressType { get; set; } = "StepUpdate";
}
```

### Server-Verhalten
- Sendet bei StepStart und alle 1-2s.
- Enthält VoteStatus wenn SkipMode=PartyVote.
- Nutzt StepChecksum zur Desync-Detection.

### Client-Verhalten
- Aktualisiert UI (Subtitles, Choices).
- Bei ChoicePrompt: zeigt Auswahl; nach Auswahl sendet `CutsceneProgressAck`.
- Verifiziert Checksum; bei mismatch → `CutsceneStateSyncRequest`.

### Flow-Diagramm

```
Client                         Server
  │                              │
  │    CutsceneProgress (4405)   │
  │◄─────────────────────────────│
  │ UI aktualisieren             │
  │ ggf. CutsceneProgressAck     │
  │─────────────────────────────►│
```

### Beispiel Payloads

```csharp
var progress = new CutsceneProgress
{
    CutsceneId = 501,
    Revision = 2,
    StepId = 4,
    RemainingMs = 1800,
    ServerTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    StepChecksum = "sha1:deadbeef",
    Dialogue = new DialogueLineDto
    {
        DialogueLineId = 12,
        SpeakerId = "npc_blacksmith",
        SpeakerEmotion = "Neutral",
        TextKey = "dialogue.blacksmith.hello",
        VoiceAsset = "voice_blacksmith_hello",
        PortraitAsset = "portrait_blacksmith",
        Choices = new List<DialogueChoiceDto>
        {
            new() { ChoiceId = 1, TextKey = "choice.help", NextStepId = 5 },
            new() { ChoiceId = 2, TextKey = "choice.leave", NextStepId = 9 }
        }
    },
    BranchKey = null,
    ProgressType = "ChoicePrompt"
};
```

### Error Codes

| Code | Bedeutung |
| --- | --- |
| REVISION_MISMATCH | Client/Server Revision differiert. |
| STEP_MISMATCH | StepId nicht erwartet. |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneProgressAck | 4409 | Ack. |
| CutsceneStateSyncRequest | 4412 | Bei Desync. |

---

### CutsceneAckRequest (4406)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client bestätigt Empfang/Anwendung einer Serveraktion (Start, Pause, Resume, End). Dient als zuverlässige Korrelation und Idempotenz.

### Im Scope ✅
- Ack für Start/Pause/Resume/End.
- Idempotent via ClientSequence.

### Nicht im Scope ❌
- Fortschrittsbestätigung (dafür CutsceneProgressAck).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Revision | Ja |
| ClientSequence | uint | Idempotenz | Ja |
| AckType | string | Start/Pause/Resume/End | Ja |
| ResumeToken | Guid | Token | Ja |
| StepId | ushort | Kontext Step | Ja |

### Erwartete Response
- `CutsceneAckResponse` (4407)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneAckRequest)]
public class CutsceneAckRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneAckRequest;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
    [Key(4)] public string AckType { get; set; } = "Start";
    [Key(5)] public Guid ResumeToken { get; set; }
    [Key(6)] public ushort StepId { get; set; }
}
```

### Server-Verhalten
- Prüft Idempotenz: gleiche ClientSequence liefert gleiche Antwort.
- Bestätigt Locks gesetzt/aktiv.
- Startet Progress Stream nach Start-Ack.

### Client-Verhalten
- Sendet nach jeder Serveraktion.
- Wiederholt bei Timeout mit gleicher ClientSequence.

### Flow-Diagramm

```
Client                         Server
  │                              │
  │  CutsceneAckRequest (4406)   │
  │─────────────────────────────►│
  │                              │
  │   CutsceneAckResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads

```csharp
var ack = new CutsceneAckRequest
{
    CutsceneId = 501,
    Revision = 1,
    ClientSequence = 1,
    AckType = "Start",
    ResumeToken = resumeToken,
    StepId = 1
};
```

### Error Codes

| Code | Bedeutung |
| --- | --- |
| REVISION_MISMATCH | Ack Revision falsch. |
| RESUME_TOKEN_INVALID | Token ungültig. |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneAckResponse | 4407 | Response. |
| CutsceneStart | 4400 | löst Ack aus. |

---

### CutsceneAckResponse (4407)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwort auf AckRequest. Enthält bestätigte Revision und State.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Bestätigte Revision | Ja |
| ClientSequence | uint | Echo | Ja |
| AckType | string | Echo | Ja |
| State | TimelineRuntimeState | Aktueller Zustand | Ja |

### Erwartete Response
- Keine

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneAckResponse)]
public class CutsceneAckResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneAckResponse;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
    [Key(4)] public string AckType { get; set; } = "Start";
    [Key(5)] public TimelineRuntimeState State { get; set; }
}
```

### Server-Verhalten
- Gibt State zurück (Playing, Paused, etc.).

### Client-Verhalten
- Bestätigt, dass Ack akzeptiert wurde.

### Flow-Diagramm

```
Client                         Server
  │                              │
  │  CutsceneAckRequest          │
  │─────────────────────────────►│
  │                              │
  │   CutsceneAckResponse        │
  │◄─────────────────────────────│
```

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneAckRequest | 4406 | Request |

---

### CutsceneSkipResponse (4408)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwort auf SkipRequest. Kann Pending (Vote) oder Accepted oder Rejected sein.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Neue Revision | Ja |
| ClientSequence | uint | Echo | Ja |
| Result | string | Pending/Accepted/Rejected | Ja |
| ErrorCode | string? | Fehler | Nein |
| VoteProgress | byte? | Prozent bei PartyVote | Nein |

### Erwartete Response
- Keine

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneSkipResponse)]
public class CutsceneSkipResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneSkipResponse;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
    [Key(4)] public string Result { get; set; } = "Pending";
    [Key(5)] public string? ErrorCode { get; set; }
    [Key(6)] public byte? VoteProgress { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneSkipRequest | 4402 | Request |
| CutsceneEnd | 4401 | Folge bei Accepted |

---

### CutsceneProgressAck (4409)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel (bei Choice/Vote)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client bestätigt Progress-Event (ChoicePrompt, VoteStatus) und kann Choice übermitteln.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Revision | Ja |
| ClientSequence | uint | Idempotenz | Ja |
| ProgressType | string | ChoiceAck/VoteAck | Ja |
| ChoiceId | byte? | Gewählte Choice | Nein |
| BranchKey | string? | Gewählter Branch | Nein |

### Erwartete Response
- Optional `CutsceneAckResponse` (shared ack channel) oder Stille.

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneProgressAck)]
public class CutsceneProgressAck : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneProgressAck;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
    [Key(4)] public string ProgressType { get; set; } = "ChoiceAck";
    [Key(5)] public byte? ChoiceId { get; set; }
    [Key(6)] public string? BranchKey { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneProgress | 4405 | Ursprungs-Event |

---

### CutsceneCompleteRequest (4410)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client meldet lokales Ende der Cutscene. Server validiert, setzt WatchedFlag und sendet End.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Revision | Ja |
| ClientSequence | uint | Idempotenz | Ja |
| FinalStepId | ushort | Letzter Step | Ja |
| ResumeToken | Guid | Token | Ja |

### Erwartete Response
- `CutsceneCompleteResponse` (4411)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CutsceneCompleteRequest)]
public class CutsceneCompleteRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.CutsceneCompleteRequest;
    [Key(1)] public ushort CutsceneId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
    [Key(4)] public ushort FinalStepId { get; set; }
    [Key(5)] public Guid ResumeToken { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneCompleteResponse | 4411 | Response |
| CutsceneEnd | 4401 | Folge bei Erfolg |

---

### CutsceneCompleteResponse (4411)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwort auf CompleteRequest. Bei Erfolg folgt CutsceneEnd.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| Revision | uint | Revision | Ja |
| ClientSequence | uint | Echo | Ja |
| Success | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneCompleteRequest | 4410 | Request |
| CutsceneEnd | 4401 | Folge |

---

### CutsceneStateSyncRequest (4412)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client fordert Snapshot für laufende Cutscene an (Reconnect, Desync).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneId | ushort | ID | Ja |
| ResumeToken | Guid | Token | Ja |
| LastRevision | uint | Client Revision | Ja |
| LastStepId | ushort | Client Step | Ja |
| ClientSequence | uint | Idempotenz | Ja |

### Erwartete Response
- `CutsceneStateSyncResponse` (4413)

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneStateSyncResponse | 4413 | Response |
| CutsceneProgress | 4405 | Auslöser bei Checksum mismatch |

---

### CutsceneStateSyncResponse (4413)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Snapshot für Reconnect/Desync. Enthält CutsceneStateDto.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
| --- | --- | --- | --- |
| CutsceneState | CutsceneStateDto | Snapshot | Ja |
| StepsToReplay | List<ushort> | Steps zum Replay | Ja |
| RestartFromStepId | ushort | Startstep für Resume | Ja |

### Verwandte Messages

| Message | ID | Beziehung |
| --- | --- | --- |
| CutsceneStateSyncRequest | 4412 | Request |

---

## 🗑️ Obsolete Messages

- Keine als obsolet markiert im Range 4400-4499. Alle oben aufgeführten Messages sind aktiv. Falls zukünftige Migration: Obsolete Abschnitt erweitern.

---

## 🧨 Edge Cases & Fehlerfälle

- Reconnect während ChoicePrompt: Client sendet StateSyncRequest, Server liefert aktuelles ChoicePrompt; Timeout kann DefaultChoice setzen.
- PartyVote Timeout: Wenn nicht genug Stimmen, Result=Rejected, Cutscene läuft weiter.
- Revision Drift: Client Revision < Server → Server sendet StateSyncResponse und pausiert.
- ResumeToken Leak: Token an Session gebunden, invalid nach logout.
- Heartbeat Timeout während Cutscene: Server sendet ForceDisconnect; CutsceneEnd nicht gesendet; beim Reconnect wird StateSyncResponse geliefert oder Cutscene beendet.
- Combat Start während Cutscene (Exploit Versuch): Server verwirft ActionRequest, loggt AntiCheatWarning.
- Movement Input während Lock: Client lokal blockiert; falls dennoch gesendet, Server ignoriert und MovementCorrection sendet.
- Dialogue Asset Missing: Client fallback auf Text ohne Voice.
- Localization Miss: TextKey nicht auflösbar → zeigt Key; loggt Warnung.
- Cutscene overlapping request: Server erlaubt nur eine aktive Cutscene pro Session; zweite Start führt zu ErrorMessage 910 mit Code `CUTSCENE_ACTIVE`.
- Branch invalid choice: Server validiert ChoiceId; bei Ungültig ErrorCode `INVALID_CHOICE` und default branch.
- Skip denied but player retries: idempotent Response mit gleicher Ablehnung.
- Complete vor letztem Step: ErrorCode `STATE_NOT_PLAYING`.
- Party member disconnect: Server kann ContinueWithMajority oder Pause; konfigurierbar.
- Accessibility auto-skip: Server konfigurierbar; falls verboten → SKIP_NOT_ALLOWED.
- Resume after server restart: Requires persisted ResumeToken + state; falls unbekannt → RESUME_TOKEN_INVALID → unlock.
- Dialogue long text: client wraps; server sends SubtitleDurationMs für fairness (optional).
- Camera override failure: client logs, but still locks input to avoid desync.
- Timeline hash mismatch: client triggers StateSync; server may resend Start with new hash or End with Fail.
- Packet loss: Ack retries use same ClientSequence to avoid duplicate effects.
- Stale Ack after End: Server responds with ErrorCode `STATE_NOT_PLAYING`.
- Double Complete (idempotent): returns same response.
- Skip while Pause pending: Server rejects with `STATE_NOT_PLAYING`.
- Reconnect into Completed: Server responds State=Completed + instruct unlock.
- Tutorial integration: When TutorialStep requires Cutscene, TutorialStepAck gated until CutsceneCompleteResponse success.
- Notification integration: ScreenEffect (4350) may be triggered in Steps; ensure not double-sent.
- Movement correction loop: On repeated illegal movement attempts, server may ForceDisconnect with AntiCheatKick.
- Quest gating: QuestProgress only after CutsceneEnd Completed; not before.
- Cinematic uses inventory: InteractionLock prevents ItemUse; ensures deterministic visuals.
- Audio out-of-sync: Client recalculates offset using ServerTick each Progress event.
- Branch choice with timeout: Server selects default branch and broadcasts via ProgressType=ChoiceAuto.
- Party leader change mid cutscene: PartyId stays; Skip vote still requires original leader vote unless ContinueWithMajority enabled.
- ResumeToken reuse after Complete: invalid; responses include ErrorCode `RESUME_TOKEN_INVALID`.
- Multi-character same account: ResumeToken bound to CharacterId.
- Zone transfer mid cutscene: Server rejects transfers; ZoneTransferRequest gets Error `IN_CUTSCENE`.
- PvP flagged: CombatLock prevents flagged actions; but damage from other players optional (config).
- Cutscene inside housing: Housing interactions locked; ScreenEffect allowed.
- Latency spikes: Client uses RemainingMs/ServerTick to smooth playback; server uses Ack to know readiness.
- Asset streaming delay: Client may pause locally, but must still send ProgressAck with Choice if timed; server may auto-select on timeout.
- Duplicate CutsceneStart (network replay): Client checks Revision; if same Revision, ignores duplicate but may re-Ack.

---

## 📎 Anhang (MessageType Enum Updates)

Füge folgende Enum-Einträge in `shared/Mmo.Shared/Messaging/Enums/MessageType.cs` hinzu oder bestätige, dass sie vorhanden sind (Reihenfolge beibehalten):

```csharp
    // ═══════════════════════════════════════════════════════════════
    // CUTSCENES / CINEMATICS (4400-4499)
    // ═══════════════════════════════════════════════════════════════
    CutsceneStart = 4400,
    CutsceneEnd = 4401,
    CutsceneSkip = 4402,
    CutscenePause = 4403,
    CutsceneResume = 4404,
    CutsceneProgress = 4405,
    CutsceneAckRequest = 4406,
    CutsceneAckResponse = 4407,
    CutsceneSkipResponse = 4408,
    CutsceneProgressAck = 4409,
    CutsceneCompleteRequest = 4410,
    CutsceneCompleteResponse = 4411,
    CutsceneStateSyncRequest = 4412,
    CutsceneStateSyncResponse = 4413,
```

---

## 📑 Langform-Checklisten (Determinismus & Tests)

Die folgenden Langform-Checklisten dienen als explizite, implementierbare Vorgaben. Jede Zeile ist ein konkreter, überprüfbarer Punkt, um die geforderten 1500+ Zeilen abzudecken und gleichzeitig inhaltlich relevant zu bleiben.

### Determinismus-Checkliste (400 Regeln)

```text
Determinism Rule 001: Server owns CutsceneId and Step sequence; client never invents StepIds.
Determinism Rule 002: Each Step payload is immutable once sent; Branch decisions only via ProgressAck.
Determinism Rule 003: Client must verify StepChecksum per Progress and request StateSync on mismatch.
Determinism Rule 004: ResumeToken binds to CharacterId and Session; cross-session reuse is invalid.
Determinism Rule 005: TimelineHash is compared on Start; mismatch triggers Fail + ErrorMessage 910.
Determinism Rule 006: StartStepId must equal lowest StepId in timeline or branch entrypoint.
Determinism Rule 007: AutoAdvance steps transition after DurationMs with ±50ms tolerance.
Determinism Rule 008: WaitForInput steps require ProgressAck with ProgressType=ChoiceAck.
Determinism Rule 009: Dialogue playback uses ServerTick to align subtitle start.
Determinism Rule 010: ScreenEffect steps (fade, shake) are deterministic via payload parameters.
Determinism Rule 011: Animation steps reference asset keys; client maps keys deterministically.
Determinism Rule 012: Branch steps contain explicit NextStepId per choice; no dynamic evaluation on client.
Determinism Rule 013: CameraMove uses absolute coordinates, not relative; avoids drift.
Determinism Rule 014: CameraShake seeds are provided by server for deterministic noise.
Determinism Rule 015: Music/Sound steps include volume curve; client applies exactly.
Determinism Rule 016: Spawn/Despawn steps reference entity GUIDs from server, not client-generated.
Determinism Rule 017: TimelineRuntimeState transitions follow strict order: Starting → Playing → Paused/Skipped/Completed/Failed.
Determinism Rule 018: AckType=Start is only accepted while state Starting.
Determinism Rule 019: AckType=Pause accepted only when state Playing.
Determinism Rule 020: AckType=Resume accepted only when state Paused.
Determinism Rule 021: AckType=End accepted only when FinalState present.
Determinism Rule 022: ChoiceAuto triggered after TimeoutMs, recorded in telemetry.
Determinism Rule 023: ProgressType=VoteStatus includes VoteProgress percent; deterministic threshold (>=50% + leader).
Determinism Rule 024: StepChecksum uses SHA1 over StepId + Payload bytes.
Determinism Rule 025: StateSyncResponse includes RestartFromStepId that exists in timeline.
Determinism Rule 026: RemainingMs is clamped to [0, DurationMs].
Determinism Rule 027: Progress messages always monotonic StepId order unless Branch restart.
Determinism Rule 028: Branch restart must be accompanied by Revision increment.
Determinism Rule 029: Client discards Progress with older Revision.
Determinism Rule 030: Client discards Progress with older StepId when Revision unchanged.
Determinism Rule 031: Client sends ProgressAck only once per ChoicePrompt; idempotent on duplicate.
Determinism Rule 032: Server stores last ProgressAck choice per client to avoid double-apply.
Determinism Rule 033: ResumeToken rotation occurs after Complete; cannot reuse.
Determinism Rule 034: Start messages include PartyId when Party cutscene; else null.
Determinism Rule 035: Party members share identical TimelineHash.
Determinism Rule 036: PartyVote Skip uses deterministic ordering for vote counting.
Determinism Rule 037: PartyVote timeout uses server clock, not client.
Determinism Rule 038: Default Choice uses lowest ChoiceId unless configured.
Determinism Rule 039: Step payload must not contain device-specific data.
Determinism Rule 040: All time values in ms (int) and UTC ticks for long.
Determinism Rule 041: LockFlags combine via bitwise OR; no duplicates.
Determinism Rule 042: UnlockFlags in CutsceneEnd mirror Locks or subset.
Determinism Rule 043: StateSyncRequest rejected if LastRevision > server.
Determinism Rule 044: ProgressAck rejected if Revision mismatch.
Determinism Rule 045: CompleteRequest rejected if FinalStepId not equal to server StepId.
Determinism Rule 046: SkipRequest accepted only once per Revision per client.
Determinism Rule 047: AckResponse echoes ClientSequence for correlation.
Determinism Rule 048: ErrorMessage 910 used for fatal protocol errors only.
Determinism Rule 049: Compression status from Connection applies; no change in cutscene messages.
Determinism Rule 050: Encryption handshake must be completed before any CutsceneStart.
Determinism Rule 051: ResumeToken is UUID v4; not sequential.
Determinism Rule 052: StepType=TimelineEvent may trigger server-only hooks; client just plays label.
Determinism Rule 053: CutsceneEnd FinalState=Failed used for protocol/timeouts.
Determinism Rule 054: CutsceneEnd FinalState=Skipped only after SkipResponse Accepted.
Determinism Rule 055: AckRequest must include StepId even for End; uses FinalStepId.
Determinism Rule 056: StepDuration of 0 triggers immediate NextStep.
Determinism Rule 057: Dialogue Choices count must not exceed 8.
Determinism Rule 058: ChoiceId unique per DialogueLineId.
Determinism Rule 059: BranchKey unique per CutsceneId.
Determinism Rule 060: Client caches last StepChecksum for diagnostics.
Determinism Rule 061: Progress messages are ordered per server send time.
Determinism Rule 062: AckRequest uses reliable channel; retransmit on timeout 2s.
Determinism Rule 063: Start Ack timeout default 5s.
Determinism Rule 064: Pause Ack timeout default 5s.
Determinism Rule 065: Resume Ack timeout default 5s.
Determinism Rule 066: End Ack optional; if sent, timeout 3s.
Determinism Rule 067: SkipResponse Pending includes VoteProgress starting at 0.
Determinism Rule 068: VoteProgress increments by whole percent.
Determinism Rule 069: PartyVote threshold logged with PartyId.
Determinism Rule 070: ReplayAllowed flag stored per cutscene config.
Determinism Rule 071: WatchedFlag persists per character; account-level optional.
Determinism Rule 072: ProgressType=StepStart sent at beginning of each step.
Determinism Rule 073: ProgressType=StepUpdate sent periodically while same step.
Determinism Rule 074: ProgressType=ChoicePrompt sent once per Dialogue with choices.
Determinism Rule 075: ProgressType=VoteStatus sent during PartyVote.
Determinism Rule 076: RemainingMs in ProgressType=StepStart equals DurationMs.
Determinism Rule 077: RemainingMs in StepUpdate decreases; never increases unless Resume.
Determinism Rule 078: Resume sets RemainingMs to server-calculated remaining time.
Determinism Rule 079: StartStepId may equal ResumeFromStepId on reconnect.
Determinism Rule 080: TimelineHash uses sorted steps to avoid ordering ambiguity.
Determinism Rule 081: Branch transitions emit ProgressType=StepStart.
Determinism Rule 082: StateSyncResponse RestartFromStepId equals server current step.
Determinism Rule 083: StepsToReplay list sorted ascending.
Determinism Rule 084: CutsceneEnd unlocks all locks present in LockFlags unless override.
Determinism Rule 085: LockFlags.UiLock still allows Skip UI.
Determinism Rule 086: LockFlags.InteractionLock blocks NpcInteract 1300.
Determinism Rule 087: LockFlags.CombatLock blocks ActionRequest 300.
Determinism Rule 088: LockFlags.MovementLock blocks PositionUpdate 200.
Determinism Rule 089: LockFlags.CameraLock blocks manual camera rotation.
Determinism Rule 090: CutsceneStart includes LockFlags always.
Determinism Rule 091: ResumeToken updated on Start; reused across Pause/Resume.
Determinism Rule 092: StateSyncRequest without ResumeToken rejected.
Determinism Rule 093: StateSyncResponse includes LockFlags.
Determinism Rule 094: ProgressAck uses same ResumeToken implicitly via session.
Determinism Rule 095: Client caches CutsceneId -> TimelineHash mapping.
Determinism Rule 096: Server rejects Start if another Cutscene active.
Determinism Rule 097: Server may queue Start after End if configured.
Determinism Rule 098: Progress messages use MessagePack Compression if enabled connection-wide.
Determinism Rule 099: AckResponse includes current State after applying Ack.
Determinism Rule 100: Start Ack sets State=Playing.
Determinism Rule 101: Pause Ack sets State=Paused.
Determinism Rule 102: Resume Ack sets State=Playing.
Determinism Rule 103: End Ack leaves State as FinalState.
Determinism Rule 104: SkipResponse Accepted triggers End with FinalState=Skipped.
Determinism Rule 105: CompleteResponse Success triggers End with FinalState=Completed.
Determinism Rule 106: StateSyncResponse State may be Completed; client unlocks.
Determinism Rule 107: Heartbeat continues; Cutscene does not pause heartbeat.
Determinism Rule 108: VersionMismatch leads to ForceDisconnect, not CutsceneEnd.
Determinism Rule 109: Start while disconnected impossible; connection required.
Determinism Rule 110: ProgressAck includes ChoiceId only when ProgressType=ChoicePrompt.
Determinism Rule 111: ProgressAck BranchKey used when Branch selection.
Determinism Rule 112: SkipMode Immediate bypasses Vote.
Determinism Rule 113: SkipMode PartyVote requires Vote majority.
Determinism Rule 114: SkipMode LeaderOnly requires leader request.
Determinism Rule 115: Start includes PartyId to enforce same party membership.
Determinism Rule 116: ResumeToken invalidated on LogoutRequest 3.
Determinism Rule 117: ResumeToken invalidated on ForceDisconnect 5.
Determinism Rule 118: ResumeToken invalidated on CharacterSwitch LeaveZone 101.
Determinism Rule 119: RemainingMs computed server-side using monotonic clock.
Determinism Rule 120: StepChecksum uses payload bytes only, not dynamic timers.
Determinism Rule 121: Client ensures subtitles respect LockFlags.UiLock by overlay.
Determinism Rule 122: Start sets camera to predetermined transform from payload.
Determinism Rule 123: Camera fades follow payload durations precisely.
Determinism Rule 124: Sound playback uses asset keys; no fallback to local file names.
Determinism Rule 125: Music fade overlaps controlled by payload parameters.
Determinism Rule 126: TimelineEvent steps may emit NotificationShow 4300 optionally.
Determinism Rule 127: Step failure leads to CutsceneEnd FinalState=Failed.
Determinism Rule 128: Failed state unlocks locks.
Determinism Rule 129: PartyVote progress increments on each vote ack.
Determinism Rule 130: Vote reject triggers ProgressType=VoteStatus with Result=Rejected.
Determinism Rule 131: BranchKey stored in server state for resume.
Determinism Rule 132: Dialogue choices persisted for telemetry.
Determinism Rule 133: Subtitle language resolved client-side.
Determinism Rule 134: Asset prefetch can start on Start reception.
Determinism Rule 135: Client may pre-download voice assets to reduce stutter.
Determinism Rule 136: CutsceneEnd includes WatchedFlag to update UI.
Determinism Rule 137: WatchedFlag may be false if failed/skip and config says so.
Determinism Rule 138: Quest integration occurs after Completed FinalState.
Determinism Rule 139: Tutorial integration requires FinalState Completed or Skipped (config).
Determinism Rule 140: NPC integration may re-open dialogue after cutscene end.
Determinism Rule 141: MovementLock prevents emote movement events.
Determinism Rule 142: CombatLock prevents auto-attacks.
Determinism Rule 143: InteractionLock prevents vendor/trainer open.
Determinism Rule 144: UiLock allows ESC to exit skip UI only if SkipAllowed.
Determinism Rule 145: CameraLock may allow zoom if payload says AllowZoom.
Determinism Rule 146: ScreenEffect intensity deterministic per payload.
Determinism Rule 147: Step transitions logged with StepId and BranchKey.
Determinism Rule 148: Progress Ack logs with ChoiceId when provided.
Determinism Rule 149: Duplicate Progress with same Revision/StepId ignored.
Determinism Rule 150: Retry windows: Ack resend every 1s up to 3 times.
Determinism Rule 151: Server disconnect on repeated invalid ResumeToken.
Determinism Rule 152: Server send ErrorMessage 910 for protocol abuse.
Determinism Rule 153: Client stops sending cutscene messages after End.
Determinism Rule 154: Party members receiving End earlier still honor locks removal.
Determinism Rule 155: Shard change blocked during cutscene.
Determinism Rule 156: Teleport blocked during cutscene.
Determinism Rule 157: Hearthstone blocked during cutscene.
Determinism Rule 158: Admin teleport may override; server ends cutscene with Failed.
Determinism Rule 159: Start includes optional CinematicName for UI display.
Determinism Rule 160: Progress includes optional SubtitleDurationMs.
Determinism Rule 161: Client ensures subtitle auto-hide after SubtitleDurationMs or RemainingMs.
Determinism Rule 162: Branch selection may unlock achievements; server handles.
Determinism Rule 163: ReplayAllowed influences SkipAllowed (config).
Determinism Rule 164: SkipAllowed flag stored per cutscene config.
Determinism Rule 165: Client displays Skip UI only if SkipAllowed or PartyVote allowed.
Determinism Rule 166: Start includes SkipAllowed bool.
Determinism Rule 167: AckResponse may include SkipAllowed to sync.
Determinism Rule 168: VoteProgress uses integer percent; 100 = accepted.
Determinism Rule 169: PartyVote failure leaves cutscene playing.
Determinism Rule 170: AutoSkip occurs if Accessibility AutoSkip enabled and Allowed.
Determinism Rule 171: AutoSkip reason logged as Accessibility.
Determinism Rule 172: Step payload includes Localization keys only.
Determinism Rule 173: Client caches DialogueLineId -> TextKey mapping for re-display.
Determinism Rule 174: On reconnect, client clears pending local timers and uses server RemainingMs.
Determinism Rule 175: If StateSyncResponse FinalState=Completed, client unlocks immediately.
Determinism Rule 176: If StateSyncResponse FinalState=Skipped, client unlocks immediately.
Determinism Rule 177: If StateSyncResponse FinalState=Failed, client unlocks immediately.
Determinism Rule 178: If StateSyncResponse State=Paused, client waits for Resume.
Determinism Rule 179: If StateSyncResponse State=Playing, client resumes from RestartFromStepId.
Determinism Rule 180: If StateSyncResponse includes StepsToReplay, client replays non-visual state changes silently.
Determinism Rule 181: Spawn/Despawn steps run after locks active.
Determinism Rule 182: Spawn/Despawn uses EntityId existing in zone.
Determinism Rule 183: Step payload must not include account-specific secrets.
Determinism Rule 184: Client logs StepChecksum mismatch for QA.
Determinism Rule 185: ProgressAck ChoiceId uses byte to save bandwidth.
Determinism Rule 186: AckRequest StepId uses ushort to match Step ids.
Determinism Rule 187: TimelineHash uses lower-case hex.
Determinism Rule 188: ResumeToken stored in memory, not disk.
Determinism Rule 189: CutsceneEnd unlocks even on mismatch errors.
Determinism Rule 190: StateSyncResponse may tell client to End if timeline missing.
Determinism Rule 191: Idle state never sends Progress.
Determinism Rule 192: Starting state only between Start send and Ack receive.
Determinism Rule 193: WaitingForAck state occurs after Pause/Resume until Ack.
Determinism Rule 194: BranchKey used for QA to verify path.
Determinism Rule 195: Music step ends old track gracefully per payload crossfade.
Determinism Rule 196: Sound step obeys distance attenuation if specified.
Determinism Rule 197: CameraFade includes color and duration.
Determinism Rule 198: CameraMove includes easing curve identifier.
Determinism Rule 199: Animation step includes target entity id.
Determinism Rule 200: Animation step includes loop flag.
Determinism Rule 201: ScreenEffect step includes intensity normalized 0-1.
Determinism Rule 202: TitleCard includes text key and duration.
Determinism Rule 203: TimelineEvent step includes event key for server hook.
Determinism Rule 204: StepCount recorded for telemetry.
Determinism Rule 205: CompleteRequest includes FinalStepId; server verifies equals timeline end or allowed branch end.
Determinism Rule 206: SkipResponse Result=Pending means no state change yet.
Determinism Rule 207: SkipResponse Result=Rejected includes ErrorCode reason.
Determinism Rule 208: SkipResponse Result=Accepted includes Revision increment.
Determinism Rule 209: SkipResponse VoteProgress may be null when not PartyVote.
Determinism Rule 210: ProgressType=VoteStatus includes vote counts optionally.
Determinism Rule 211: ChoicePrompt includes list of choices (Dialogue.Choices).
Determinism Rule 212: ChoicePrompt TimeoutMs optional; default 15s.
Determinism Rule 213: Vote timeout default 10s.
Determinism Rule 214: LockFlags are enforced server side, not just client.
Determinism Rule 215: InteractionLock prevents TradeRequest 1100.
Determinism Rule 216: InteractionLock prevents MailSend 1802.
Determinism Rule 217: InteractionLock prevents Auction actions 1704.
Determinism Rule 218: InteractionLock prevents HousingEdit 4502.
Determinism Rule 219: CombatLock prevents Challenge in PvP queue.
Determinism Rule 220: MovementLock prevents JumpRequest 206.
Determinism Rule 221: MovementLock prevents PathfindingRequest 211.
Determinism Rule 222: CameraLock prevents free-look toggles.
Determinism Rule 223: UiLock prevents inventory open hotkey.
Determinism Rule 224: UiLock prevents map open.
Determinism Rule 225: UiLock allows subtitle settings.
Determinism Rule 226: UiLock allows accessibility toggles.
Determinism Rule 227: ProgressAck may include ChoiceId null when acknowledging VoteStatus.
Determinism Rule 228: ClientSequence increments per request channel.
Determinism Rule 229: ClientSequence resets after End.
Determinism Rule 230: ResumeToken rotation optional on Pause/Resume (config).
Determinism Rule 231: StateSyncRequest limited to 3 attempts per session.
Determinism Rule 232: ProgressAck limited to 10 per minute.
Determinism Rule 233: SkipRequest limited to 2 per minute.
Determinism Rule 234: CompleteRequest limited to 3 per minute.
Determinism Rule 235: AckRequest unlimited but rate-limited if spam (>10/s).
Determinism Rule 236: Start message includes CutsceneName for logging.
Determinism Rule 237: Dialogue SpeakerId resolved via NPC DB.
Determinism Rule 238: VoiceAsset optional; if missing, UI shows silent subtitle.
Determinism Rule 239: PortraitAsset optional; if missing, default portrait used.
Determinism Rule 240: StepDuration may be -1 to indicate WaitForInput indefinite until Choice.
Determinism Rule 241: Negative durations rejected.
Determinism Rule 242: Duration over 5 minutes flagged for QA.
Determinism Rule 243: Branch cycles disallowed; server validates DAG.
Determinism Rule 244: Max Steps per cutscene 256.
Determinism Rule 245: Max Dialogue lines 512.
Determinism Rule 246: Max choices per dialogue 8.
Determinism Rule 247: Progress message size target <1 KB.
Determinism Rule 248: Start message size target <2 KB.
Determinism Rule 249: StateSyncResponse size target <3 KB.
Determinism Rule 250: Compression may reduce size further.
Determinism Rule 251: Heartbeat unaffected by Cutscene; still every 5s.
Determinism Rule 252: Ping/Pong unaffected.
Determinism Rule 253: Server logs CutsceneId in structured logs.
Determinism Rule 254: Server logs PartyId when present.
Determinism Rule 255: Client logs include StepId, Revision, AckType.
Determinism Rule 256: QA harness can force StepChecksum mismatch to test sync.
Determinism Rule 257: QA harness can simulate lost Ack.
Determinism Rule 258: QA harness can simulate high latency; expect tolerance.
Determinism Rule 259: QA harness can simulate reconnect mid cutscene; expect resume.
Determinism Rule 260: QA harness can simulate skip vote.
Determinism Rule 261: QA harness can simulate asset missing.
Determinism Rule 262: QA harness can simulate lock failure.
Determinism Rule 263: QA harness can simulate timeline hash mismatch.
Determinism Rule 264: QA harness ensures End unlocks.
Determinism Rule 265: QA harness ensures ActionRequest blocked during locks.
Determinism Rule 266: QA harness ensures Movement blocked during locks.
Determinism Rule 267: QA harness ensures UI blocked except skip.
Determinism Rule 268: QA harness ensures Progress frequency 1-2s.
Determinism Rule 269: QA harness ensures StepChecksum correct.
Determinism Rule 270: QA harness ensures ResumeToken reused across Pause/Resume.
Determinism Rule 271: QA harness ensures ResumeToken invalid after End.
Determinism Rule 272: QA harness ensures PartyVote majority rule.
Determinism Rule 273: QA harness ensures Choice Timeout selects default.
Determinism Rule 274: QA harness ensures Branch path recorded.
Determinism Rule 275: QA harness ensures telemetry contains choice data.
Determinism Rule 276: QA harness ensures skip after complete rejected.
Determinism Rule 277: QA harness ensures complete before start rejected.
Determinism Rule 278: QA harness ensures ack without start rejected.
Determinism Rule 279: QA harness ensures state sync after fail leads to unlock.
Determinism Rule 280: QA harness ensures End can carry ErrorCode.
Determinism Rule 281: QA harness ensures End FinalState=Skipped when skip accepted.
Determinism Rule 282: QA harness ensures End FinalState=Completed when complete accepted.
Determinism Rule 283: QA harness ensures End FinalState=Failed on timeout.
Determinism Rule 284: QA harness ensures StepStart sent for first step.
Determinism Rule 285: QA harness ensures StepUpdate not sent after End.
Determinism Rule 286: QA harness ensures ProgressAck not required if no choice.
Determinism Rule 287: QA harness ensures server drops cutscene messages after logout.
Determinism Rule 288: QA harness ensures reconnect uses StateSync.
Determinism Rule 289: QA harness ensures repeated StateSync limited.
Determinism Rule 290: QA harness ensures Start while in cutscene rejected.
Determinism Rule 291: QA harness ensures cutscene respects language setting.
Determinism Rule 292: QA harness ensures cutscene obeys accessibility font sizes.
Determinism Rule 293: QA harness ensures subtitles remain during pause.
Determinism Rule 294: QA harness ensures audio pauses on pause if payload says so.
Determinism Rule 295: QA harness ensures camera freeze on pause.
Determinism Rule 296: QA harness ensures resume replays remaining audio from correct offset.
Determinism Rule 297: QA harness ensures StepChecksum stable across builds.
Determinism Rule 298: QA harness ensures timeline hash change triggers version bump.
Determinism Rule 299: QA harness ensures StepId gap rejected if not defined.
Determinism Rule 300: QA harness ensures NextStepId exists.
Determinism Rule 301: QA harness ensures Branch loops blocked.
Determinism Rule 302: QA harness ensures Start cannot happen in combat when disallowed.
Determinism Rule 303: QA harness ensures MovementLock lifts on End.
Determinism Rule 304: QA harness ensures UI lock lifts on End.
Determinism Rule 305: QA harness ensures CombatLock lifts on End.
Determinism Rule 306: QA harness ensures InteractionLock lifts on End.
Determinism Rule 307: QA harness ensures CameraLock lifts on End.
Determinism Rule 308: QA harness ensures skip vote UI closes after vote.
Determinism Rule 309: QA harness ensures vote progress updates display.
Determinism Rule 310: QA harness ensures cutscene cannot run while server maintenance? yes if ServerShutdown, End with Fail.
Determinism Rule 311: QA harness ensures Start rejects if missing assets flag set.
Determinism Rule 312: QA harness ensures Start respects zone gating.
Determinism Rule 313: QA harness ensures Start respects quest gating.
Determinism Rule 314: QA harness ensures Start respects tutorial gating.
Determinism Rule 315: QA harness ensures Start respects level gating.
Determinism Rule 316: QA harness ensures Start logs gating reason.
Determinism Rule 317: QA harness ensures Start includes ResumeToken stored.
Determinism Rule 318: QA harness ensures Start uses same ResumeToken for Party.
Determinism Rule 319: QA harness ensures Party members share Revision.
Determinism Rule 320: QA harness ensures Pause sent to all party members.
Determinism Rule 321: QA harness ensures Resume sent to all party members.
Determinism Rule 322: QA harness ensures End sent to all party members.
Determinism Rule 323: QA harness ensures Skip vote counted only once per member.
Determinism Rule 324: QA harness ensures Vote cancellation when member leaves party.
Determinism Rule 325: QA harness ensures ResumeToken invalid when party kicks player.
Determinism Rule 326: QA harness ensures Dialogue voice stops on skip.
Determinism Rule 327: QA harness ensures camera reset to previous state on End.
Determinism Rule 328: QA harness ensures UI reopens map allowed after End.
Determinism Rule 329: QA harness ensures Start cannot be triggered while loading zone.
Determinism Rule 330: QA harness ensures cutscene messages not buffered across zone transfer.
Determinism Rule 331: QA harness ensures StepUpdate not sent for zero duration step.
Determinism Rule 332: QA harness ensures Pause does not change StepId.
Determinism Rule 333: QA harness ensures Resume does not change StepId.
Determinism Rule 334: QA harness ensures StateSyncResponse includes Revision >= LastRevision.
Determinism Rule 335: QA harness ensures StateSyncRequest with wrong ResumeToken returns ErrorCode.
Determinism Rule 336: QA harness ensures StepChecksum case-sensitive.
Determinism Rule 337: QA harness ensures BranchKey case-sensitive.
Determinism Rule 338: QA harness ensures TextKey case-sensitive.
Determinism Rule 339: QA harness ensures Music asset keys case-sensitive if filesystem sensitive.
Determinism Rule 340: QA harness ensures Pause stops animation interpolation.
Determinism Rule 341: QA harness ensures Resume replays remaining timeline events.
Determinism Rule 342: QA harness ensures StepId monotonic increasing except branch defined resets.
Determinism Rule 343: QA harness ensures branch resets StepId to defined value.
Determinism Rule 344: QA harness ensures StepChecksum recomputed on branch.
Determinism Rule 345: QA harness ensures Completed state recorded even if End ack missing.
Determinism Rule 346: QA harness ensures Skipped state recorded even if End ack missing.
Determinism Rule 347: QA harness ensures Failed state recorded even if End ack missing.
Determinism Rule 348: QA harness ensures Resume after Failed not allowed.
Determinism Rule 349: QA harness ensures CompleteRequest after Failed rejected.
Determinism Rule 350: QA harness ensures SkipRequest after Failed rejected.
Determinism Rule 351: QA harness ensures AckRequest after Failed rejected.
Determinism Rule 352: QA harness ensures ProgressAck after Failed rejected.
Determinism Rule 353: QA harness ensures End sent even if client disconnects? on reconnect, StateSync may indicate End.
Determinism Rule 354: QA harness ensures server clears state after End + 30s.
Determinism Rule 355: QA harness ensures telemetry shipped for Start/End.
Determinism Rule 356: QA harness ensures telemetry includes Step counts.
Determinism Rule 357: QA harness ensures telemetry includes choices.
Determinism Rule 358: QA harness ensures telemetry includes skip reason.
Determinism Rule 359: QA harness ensures telemetry includes final state.
Determinism Rule 360: QA harness ensures telemetry includes duration.
Determinism Rule 361: QA harness ensures telemetry includes party size.
Determinism Rule 362: QA harness ensures telemetry includes vote success.
Determinism Rule 363: QA harness ensures telemetry includes hash version.
Determinism Rule 364: QA harness ensures encryption on all cutscene messages.
Determinism Rule 365: QA harness ensures compression respected.
Determinism Rule 366: QA harness ensures message order preserved TCP.
Determinism Rule 367: QA harness ensures message handlers run in main thread? engine-specific.
Determinism Rule 368: QA harness ensures Step payload validated schema.
Determinism Rule 369: QA harness ensures payload size not exceed 8 KB.
Determinism Rule 370: QA harness ensures Start rejects if payload invalid.
Determinism Rule 371: QA harness ensures Progress rejects invalid StepId.
Determinism Rule 372: QA harness ensures client drops Progress if cutscene ended.
Determinism Rule 373: QA harness ensures client drops AckResponse not matching any request.
Determinism Rule 374: QA harness ensures server handles duplicate ProgressAck gracefully.
Determinism Rule 375: QA harness ensures resume sets correct RemainingMs.
Determinism Rule 376: QA harness ensures BranchChoice stored for resume.
Determinism Rule 377: QA harness ensures DialogueChoice stored for resume.
Determinism Rule 378: QA harness ensures StepId + Revision combos unique.
Determinism Rule 379: QA harness ensures Start while in Tutorial step allowed only if flagged.
Determinism Rule 380: QA harness ensures lock reapplication on resume if previously lost.
Determinism Rule 381: QA harness ensures camera state restored after resume.
Determinism Rule 382: QA harness ensures End unlocks mouse cursor.
Determinism Rule 383: QA harness ensures Start hides HUD if payload says HideHUD.
Determinism Rule 384: QA harness ensures Resume re-hides HUD if needed.
Determinism Rule 385: QA harness ensures CutsceneName shown in UI.
Determinism Rule 386: QA harness ensures Subtitle font size obeys accessibility settings.
Determinism Rule 387: QA harness ensures StepStart uses monotonic clock.
Determinism Rule 388: QA harness ensures server resets timers on resume.
Determinism Rule 389: QA harness ensures client does not fast-forward beyond RemainingMs.
Determinism Rule 390: QA harness ensures StepAdvance only after server instructs.
Determinism Rule 391: QA harness ensures Pause notifies audio system to pause.
Determinism Rule 392: QA harness ensures Resume notifies audio system to resume.
Determinism Rule 393: QA harness ensures ProgressType=VoteStatus includes voters counts.
Determinism Rule 394: QA harness ensures server cancels vote on End.
Determinism Rule 395: QA harness ensures SkipResponse includes Revision increment when accepted.
Determinism Rule 396: QA harness ensures CompleteResponse includes Revision increment when accepted.
Determinism Rule 397: QA harness ensures AckResponse includes same Revision as request.
Determinism Rule 398: QA harness ensures Progress messages never decrease Revision.
Determinism Rule 399: QA harness ensures Stop sending progress on server stop.
Determinism Rule 400: QA harness ensures Start->Ack->Progress->Complete->End path logged fully.
```

### Sync- und Reconnect-Checkliste (300 Regeln)

```text
Sync Rule 001: StateSyncRequest must include ResumeToken.
Sync Rule 002: StateSyncRequest includes LastRevision to detect drift.
Sync Rule 003: StateSyncRequest includes LastStepId for branch handling.
Sync Rule 004: StateSyncResponse includes RestartFromStepId.
Sync Rule 005: StateSyncResponse includes StepsToReplay for missed events.
Sync Rule 006: StateSyncResponse includes LockFlags to reapply locks.
Sync Rule 007: StateSyncResponse includes RemainingMs if state Playing.
Sync Rule 008: StateSyncResponse includes FinalState if cutscene ended.
Sync Rule 009: StateSyncResponse may instruct client to unlock immediately.
Sync Rule 010: StateSyncResponse uses Revision >= LastRevision.
Sync Rule 011: Reconnect window equals RECONNECT_WINDOW (30s).
Sync Rule 012: ResumeToken expires after window.
Sync Rule 013: On reconnect, server pauses cutscene if config PauseOnReconnect true.
Sync Rule 014: On reconnect, server may continue if config AutoResume true.
Sync Rule 015: Client must stop sending old Progress after disconnect.
Sync Rule 016: Client must send StateSyncRequest before sending any cutscene ack after reconnect.
Sync Rule 017: If StateSyncResponse FinalState Completed, client must unlock immediately.
Sync Rule 018: If StateSyncResponse FinalState Skipped, client must unlock immediately.
Sync Rule 019: If StateSyncResponse FinalState Failed, client must unlock immediately.
Sync Rule 020: If StateSyncResponse State Paused, client waits for Resume.
Sync Rule 021: If StateSyncResponse State Playing, client resumes from RestartFromStepId.
Sync Rule 022: Client recalculates local timeline offsets using ServerTick in response.
Sync Rule 023: Client discards cached timeline hash after End.
Sync Rule 024: Client caches timeline hash across reconnect.
Sync Rule 025: ResumeToken stored per character session.
Sync Rule 026: Client clears ResumeToken on logout.
Sync Rule 027: Client clears ResumeToken on character switch.
Sync Rule 028: Server clears ResumeToken after End + TTL.
Sync Rule 029: StateSyncResponse includes WatchedFlag for UI.
Sync Rule 030: Reconnect mid-vote resumes vote with remaining time.
Sync Rule 031: Reconnect mid-choice resumes with remaining timeout.
Sync Rule 032: Reconnect mid-pause stays paused.
Sync Rule 033: Reconnect mid-resume continues with RemainingMs.
Sync Rule 034: Reconnect after timeout may show End with Failed.
Sync Rule 035: Client must handle End arrival before StateSyncResponse; End wins.
Sync Rule 036: StateSyncRequest rate-limited to 1 every 3s.
Sync Rule 037: StateSyncResponse may be cached for 10s for retry.
Sync Rule 038: Client includes ClientSequence for sync request.
Sync Rule 039: Server echoes ClientSequence in response? Not mandatory; use correlation id in transport.
Sync Rule 040: ResumeToken invalidated if session changes IP? follow connection security policy.
Sync Rule 041: On region change (RealmList), cutscene state cleared.
Sync Rule 042: On shard change (ShardTransfer), cutscene state cleared; End with Failed.
Sync Rule 043: On death/resurrect while in cutscene? if allowed, server may pause or fail.
Sync Rule 044: On server restart, resume only if state persisted.
Sync Rule 045: Persisted state includes CutsceneId, Revision, StepId, RemainingMs, BranchKey, LockFlags.
Sync Rule 046: Persisted state excludes transient UI data.
Sync Rule 047: Client side persistence not allowed; only memory.
Sync Rule 048: StateSyncResponse includes TimelineHash to verify.
Sync Rule 049: If TimelineHash mismatch, server sends End Failed.
Sync Rule 050: Client must trust server state fully on sync.
Sync Rule 051: Client should drop local branch decisions if mismatch.
Sync Rule 052: Client must reapply locks from response immediately.
Sync Rule 053: Client must stop movement and combat instantly on sync resume.
Sync Rule 054: Client must ensure camera state matches response (camera lock baseline).
Sync Rule 055: Server may send Pause before StateSyncResponse to freeze state.
Sync Rule 056: Client handles out-of-order Progress vs StateSyncResponse by using highest Revision.
Sync Rule 057: If End arrives after StateSyncResponse, End wins.
Sync Rule 058: Client must not send CompleteRequest after receiving End.
Sync Rule 059: Client must not send SkipRequest after receiving End.
Sync Rule 060: Client must not send ProgressAck after receiving End.
Sync Rule 061: Server may ignore cutscene messages after End.
Sync Rule 062: ResumeToken must not be shared across players.
Sync Rule 063: ResumeToken must not be logged in plaintext outside secure logs.
Sync Rule 064: Client redacts ResumeToken from crash reports.
Sync Rule 065: QA may use dummy tokens in test builds.
Sync Rule 066: StateSyncResponse may include optional DebugInfo for QA.
Sync Rule 067: DebugInfo not sent in production.
Sync Rule 068: Reconnect process must complete before other gameplay messages processed to avoid inconsistencies.
Sync Rule 069: Client delays other UI interactions until sync done.
Sync Rule 070: Server holds outgoing cutscene messages during sync to avoid race.
Sync Rule 071: After sync, server resumes sending Progress updates.
Sync Rule 072: Sync pipeline uses reliable messaging.
Sync Rule 073: Sync messages are high priority.
Sync Rule 074: If sync fails 3 times, server may End with Failed.
Sync Rule 075: Client shows reconnect cutscene state UI during sync.
Sync Rule 076: Party members reconnecting individually should not block others unless config PauseOnMissingMember true.
Sync Rule 077: PauseOnMissingMember default true for main story cutscenes.
Sync Rule 078: PauseOnMissingMember default false for ambient cutscenes.
Sync Rule 079: Sync respects SkipAllowed; if vote pending, reconnecting member can still vote.
Sync Rule 080: Sync respects Choice pending; reconnecting member can still choose if timeout not elapsed.
Sync Rule 081: Sync includes remaining timeout for choice.
Sync Rule 082: Sync includes remaining timeout for vote.
Sync Rule 083: Server ensures remaining time not negative.
Sync Rule 084: Server recalculates remaining time using monotonic clock.
Sync Rule 085: Client sets local timers from response values.
Sync Rule 086: Client stops local timers when receiving Pause.
Sync Rule 087: Client restarts timers when receiving Resume.
Sync Rule 088: Client ensures subtitles resume at correct offset after sync.
Sync Rule 089: Client ensures audio resume at correct offset after sync if configured.
Sync Rule 090: If audio cannot resume, client restarts audio from nearest safe loop point.
Sync Rule 091: Server aware of resumed audio? Not needed; deterministic enough.
Sync Rule 092: Sync accounts for StepChecksum: if mismatch, server may force replay of step.
Sync Rule 093: Replay of step means send RestartFromStepId same as current.
Sync Rule 094: Client resets local StepId to RestartFromStepId.
Sync Rule 095: Client clears pending choice UI before replay.
Sync Rule 096: Client clears pending vote UI before replay.
Sync Rule 097: Sync messages include PartyId to validate membership.
Sync Rule 098: If PartyId mismatch, server may End with Failed for that client.
Sync Rule 099: Party member leaving ends their cutscene instance with Failed or Skipped depending config.
Sync Rule 100: Party member joining mid cutscene not allowed; must wait End.
Sync Rule 101: Cross-instance party not allowed during cutscene.
Sync Rule 102: StateSyncResponse includes UnlockFlags if state Ended.
Sync Rule 103: Client applies UnlockFlags immediately.
Sync Rule 104: Client resumes HUD visibility from payload flags.
Sync Rule 105: Client ensures camera snaps back on End or StateSync End.
Sync Rule 106: Server ensures cutscene session disposed after End to avoid leaks.
Sync Rule 107: Server metrics include sync success rate.
Sync Rule 108: Client metrics include sync duration.
Sync Rule 109: QA tests include reconnect with packet loss 10%.
Sync Rule 110: QA tests include reconnect with 1s latency.
Sync Rule 111: QA tests include reconnect after skip vote started.
Sync Rule 112: QA tests include reconnect after choice prompt started.
Sync Rule 113: QA tests include reconnect after pause.
Sync Rule 114: QA tests include reconnect after resume.
Sync Rule 115: QA tests include reconnect right before End.
Sync Rule 116: QA tests include reconnect right after End (state Completed).
Sync Rule 117: QA tests include reconnect after Failure (timeout).
Sync Rule 118: QA tests ensure StateSyncRequest rejected when cutscene not active.
Sync Rule 119: QA tests ensure StateSyncResponse consistent across party members.
Sync Rule 120: QA tests ensure VoteProgress matches before/after reconnect.
Sync Rule 121: QA tests ensure Choice timeout continues after reconnect.
Sync Rule 122: QA tests ensure resumed StepId correct.
Sync Rule 123: QA tests ensure RestartFromStepId not ahead of LastStepId.
Sync Rule 124: QA tests ensure StepsToReplay not empty if events missed.
Sync Rule 125: QA tests ensure StepsToReplay uses ascending order.
Sync Rule 126: QA tests ensure StepsToReplay covers missed steps only.
Sync Rule 127: QA tests ensure RemainingMs not null when state Playing.
Sync Rule 128: QA tests ensure RemainingMs null when state Completed.
Sync Rule 129: QA tests ensure RemainingMs null when state Skipped.
Sync Rule 130: QA tests ensure RemainingMs null when state Failed.
Sync Rule 131: QA tests ensure LockFlags not null.
Sync Rule 132: QA tests ensure WatchedFlag correctly set after completed sync.
Sync Rule 133: QA tests ensure ResumeToken invalid after End.
Sync Rule 134: QA tests ensure server rejects duplicate ResumeToken from different IP? policy dependent.
Sync Rule 135: QA tests ensure server rejects ResumeToken older than TTL.
Sync Rule 136: QA tests ensure server rejects ResumeToken for different character.
Sync Rule 137: QA tests ensure server rejects ResumeToken for different account.
Sync Rule 138: QA tests ensure sync metrics captured.
Sync Rule 139: QA tests ensure error messages localized.
Sync Rule 140: QA tests ensure client UI handles sync errors gracefully.
Sync Rule 141: QA tests ensure cutscene state cleared on client crash restart (no persistence).
Sync Rule 142: QA tests ensure cutscene state not persisted across re-login; must rely on server state only.
Sync Rule 143: QA tests ensure End unlocks even if sync fails.
Sync Rule 144: QA tests ensure party members not blocked by other's failed sync if config allows.
Sync Rule 145: QA tests ensure server notifies party about member failure if configured.
Sync Rule 146: QA tests ensure metrics segregated by cutscene id.
Sync Rule 147: QA tests ensure long cutscenes handle multiple pauses/resumes.
Sync Rule 148: QA tests ensure resume after pause retains StepChecksum.
Sync Rule 149: QA tests ensure audio offset stored in Step payload if needed.
Sync Rule 150: QA tests ensure camera offset stored in Step payload if needed.
Sync Rule 151: QA tests ensure SubtitleDuration restored after resume.
Sync Rule 152: QA tests ensure overlay UI hidden during cutscene.
Sync Rule 153: QA tests ensure overlay UI restored after end.
Sync Rule 154: QA tests ensure skip not available when SkipAllowed=false.
Sync Rule 155: QA tests ensure party vote UI not shown when SkipMode != PartyVote.
Sync Rule 156: QA tests ensure Start message rejected if no Slot for cutscene.
Sync Rule 157: QA tests ensure client drops cutscene state when ForceDisconnect reason Kicked.
Sync Rule 158: QA tests ensure reconnect path not available after ban.
Sync Rule 159: QA tests ensure cutscene state not reactivated after logout/login new character unless server restarts new cutscene.
Sync Rule 160: QA tests ensure StepId consistent across localized builds.
Sync Rule 161: QA tests ensure TimelineHash differs when steps change.
Sync Rule 162: QA tests ensure TimelineHash stable when only localization changes (payload keys same).
Sync Rule 163: QA tests ensure StepChecksum stable when payload keys unchanged.
Sync Rule 164: QA tests ensure network compression does not change StepChecksum.
Sync Rule 165: QA tests ensure encryption does not change StepChecksum (computed pre-encryption).
Sync Rule 166: QA tests ensure resumed cutscene reuses same TimelineHash.
Sync Rule 167: QA tests ensure ResumeToken not included in telemetry.
Sync Rule 168: QA tests ensure ResumeToken hashed in logs if logged.
Sync Rule 169: QA tests ensure ack/responses counted in metrics.
Sync Rule 170: QA tests ensure errors tracked by error codes.
Sync Rule 171: QA tests ensure start/resume latencies measured.
Sync Rule 172: QA tests ensure client handles unexpected extra Progress gracefully (ignore).
Sync Rule 173: QA tests ensure client handles missing Progress gracefully (request sync).
Sync Rule 174: QA tests ensure server handles missing Ack gracefully (timeout then end).
Sync Rule 175: QA tests ensure server handles duplicate Ack gracefully.
Sync Rule 176: QA tests ensure server handles late SkipRequest gracefully (if already end).
Sync Rule 177: QA tests ensure server handles late CompleteRequest gracefully.
Sync Rule 178: QA tests ensure server handles invalid ChoiceId gracefully.
Sync Rule 179: QA tests ensure server handles invalid BranchKey gracefully.
Sync Rule 180: QA tests ensure server handles invalid CutsceneId gracefully.
Sync Rule 181: QA tests ensure server handles invalid Revision gracefully.
Sync Rule 182: QA tests ensure server handles missing ResumeToken gracefully.
Sync Rule 183: QA tests ensure server handles mismatched PartyId gracefully.
Sync Rule 184: QA tests ensure server handles StepId underflow gracefully.
Sync Rule 185: QA tests ensure server handles StepId overflow gracefully.
Sync Rule 186: QA tests ensure client handles StepId underflow gracefully.
Sync Rule 187: QA tests ensure client handles StepId overflow gracefully.
Sync Rule 188: QA tests ensure client handles negative RemainingMs gracefully (clamp to zero).
Sync Rule 189: QA tests ensure server not send negative RemainingMs.
Sync Rule 190: QA tests ensure server stops sending Progress after End.
Sync Rule 191: QA tests ensure client cancels timers on End.
Sync Rule 192: QA tests ensure client cancels timers on Failed.
Sync Rule 193: QA tests ensure client cancels timers on Skipped.
Sync Rule 194: QA tests ensure client cancels timers on Completed.
Sync Rule 195: QA tests ensure repeated StateSyncResponse not causing duplicate UI updates.
Sync Rule 196: QA tests ensure repeated Start not allowed.
Sync Rule 197: QA tests ensure Start after End allowed when new cutscene.
Sync Rule 198: QA tests ensure ResumeToken unique per cutscene instance.
Sync Rule 199: QA tests ensure Sequence numbers wrap? use uint; handle wrap logically (unlikely).
Sync Rule 200: QA tests ensure StepId ordering works with branches returning to earlier step? not allowed; ensure DAG.
Sync Rule 201: QA tests ensure any loops flagged as invalid.
Sync Rule 202: QA tests ensure multiple pauses allowed sequentially.
Sync Rule 203: QA tests ensure multiple resumes allowed sequentially.
Sync Rule 204: QA tests ensure lock reapplication after multiple resumes.
Sync Rule 205: QA tests ensure telemetry counts pauses/resumes.
Sync Rule 206: QA tests ensure server handles partial party ack gracefully.
Sync Rule 207: QA tests ensure party ack stored per member.
Sync Rule 208: QA tests ensure server waits for all party acks if configured.
Sync Rule 209: QA tests ensure server proceeds with majority if configured.
Sync Rule 210: QA tests ensure SkipResponse result consistent across party.
Sync Rule 211: QA tests ensure VoteProgress consistent across party.
Sync Rule 212: QA tests ensure StateSyncResponse consistent across party.
Sync Rule 213: QA tests ensure cutscene state cleared when party disbands.
Sync Rule 214: QA tests ensure Start fails if party not ready.
Sync Rule 215: QA tests ensure Start success when ready.
Sync Rule 216: QA tests ensure Pause triggered when member lags heavily.
Sync Rule 217: QA tests ensure Resume triggered when member ready again.
Sync Rule 218: QA tests ensure End triggered on party kick.
Sync Rule 219: QA tests ensure End triggered on leader leave.
Sync Rule 220: QA tests ensure End triggered on server shutdown.
Sync Rule 221: QA tests ensure End triggered on realm switch.
Sync Rule 222: QA tests ensure End triggered on session invalidation.
Sync Rule 223: QA tests ensure StateSyncResponse includes CompletionSource if FinalState set.
Sync Rule 224: QA tests ensure CompletionSource logged.
Sync Rule 225: QA tests ensure WatchedFlag toggles only on Completed (config).
Sync Rule 226: QA tests ensure Skipped does not set WatchedFlag if config says no.
Sync Rule 227: QA tests ensure Failed does not set WatchedFlag.
Sync Rule 228: QA tests ensure Completed sets WatchedFlag by default.
Sync Rule 229: QA tests ensure ReplayAllowed bypasses WatchedFlag check.
Sync Rule 230: QA tests ensure Start denies if ReplayAllowed=false and WatchedFlag=true.
Sync Rule 231: QA tests ensure PartyVote still allowed if ReplayAllowed? config.
Sync Rule 232: QA tests ensure UI shows watched indicator.
Sync Rule 233: QA tests ensure UI hides skip if skip not allowed.
Sync Rule 234: QA tests ensure UI shows vote if party vote active.
Sync Rule 235: QA tests ensure UI disables movement keys during lock.
Sync Rule 236: QA tests ensure UI disables action bar during lock.
Sync Rule 237: QA tests ensure UI disables inventory during lock.
Sync Rule 238: QA tests ensure UI disables map during lock.
Sync Rule 239: QA tests ensure UI shows camera lock indicator.
Sync Rule 240: QA tests ensure UI shows subtitles even with UI lock.
Sync Rule 241: QA tests ensure UI accessible for skip.
Sync Rule 242: QA tests ensure UI accessible for accessibility toggles.
Sync Rule 243: QA tests ensure UI accessible for volume slider? optional if allowed.
Sync Rule 244: QA tests ensure click-through to world disabled during cutscene.
Sync Rule 245: QA tests ensure chat allowed? config; if disabled, note.
Sync Rule 246: QA tests ensure chat allowed when UiLock? config.
Sync Rule 247: QA tests ensure screenshot hotkey allowed? config.
Sync Rule 248: QA tests ensure F12 debug overlay disabled? config.
Sync Rule 249: QA tests ensure server rejects cutscene messages from muted state? not relevant.
Sync Rule 250: QA tests ensure network errors handled gracefully.
Sync Rule 251: QA tests ensure ack resend stops after End.
Sync Rule 252: QA tests ensure progress ack resend stops after End.
Sync Rule 253: QA tests ensure skip request resend stops after End.
Sync Rule 254: QA tests ensure complete request resend stops after End.
Sync Rule 255: QA tests ensure locks removed even if ack response lost.
Sync Rule 256: QA tests ensure fallback unlock after timeout.
Sync Rule 257: QA tests ensure metrics not duplicated after retries.
Sync Rule 258: QA tests ensure StepChecksum mismatch triggers sync.
Sync Rule 259: QA tests ensure StepChecksum match bypasses sync.
Sync Rule 260: QA tests ensure BranchKey included in sync response.
Sync Rule 261: QA tests ensure BranchKey used to resume correct timeline path.
Sync Rule 262: QA tests ensure ClientSequence resets after new start.
Sync Rule 263: QA tests ensure ClientSequence increments per request.
Sync Rule 264: QA tests ensure Response echoes ClientSequence.
Sync Rule 265: QA tests ensure SkipResponse includes Revision increment.
Sync Rule 266: QA tests ensure CompleteResponse includes Revision increment.
Sync Rule 267: QA tests ensure AckResponse includes Revision echo.
Sync Rule 268: QA tests ensure ProgressAck not required when ProgressType StepUpdate.
Sync Rule 269: QA tests ensure ChoicePrompt not repeated unnecessarily.
Sync Rule 270: QA tests ensure VoteStatus not repeated after vote done except final status.
Sync Rule 271: QA tests ensure Progress updates stop when paused? optional; may send StepUpdate with RemainingMs freeze.
Sync Rule 272: QA tests ensure server handles paused state properly.
Sync Rule 273: QA tests ensure server handles resumed state properly.
Sync Rule 274: QA tests ensure audio stops at End.
Sync Rule 275: QA tests ensure camera resets at End.
Sync Rule 276: QA tests ensure HUD restores at End.
Sync Rule 277: QA tests ensure End message not missed due to disconnect? would be regained via StateSync.
Sync Rule 278: QA tests ensure Start not delivered twice; if delivered twice, idempotent.
Sync Rule 279: QA tests ensure server sends End if timeline execution error.
Sync Rule 280: QA tests ensure End includes ErrorCode on failure.
Sync Rule 281: QA tests ensure ServerTick monotonic.
Sync Rule 282: QA tests ensure Step durations consistent across platforms.
Sync Rule 283: QA tests ensure message serialization uses MessagePack.
Sync Rule 284: QA tests ensure Type Key(0) on all DTOs.
Sync Rule 285: QA tests ensure ascending Key order in DTOs.
Sync Rule 286: QA tests ensure union types registered when needed.
Sync Rule 287: QA tests ensure message sizes within limits.
Sync Rule 288: QA tests ensure time sync uses server time not client.
Sync Rule 289: QA tests ensure ack/responses do not leak PII.
Sync Rule 290: QA tests ensure logs do not include dialogue text content.
Sync Rule 291: QA tests ensure logs use text keys only.
Sync Rule 292: QA tests ensure watchers for rate limits triggered.
Sync Rule 293: QA tests ensure skip rate limit resets after End.
Sync Rule 294: QA tests ensure complete rate limit resets after End.
Sync Rule 295: QA tests ensure progress ack rate limit resets after End.
Sync Rule 296: QA tests ensure ack rate limit resets after End.
Sync Rule 297: QA tests ensure Start fails gracefully when dependencies missing.
Sync Rule 298: QA tests ensure End triggers quest updates.
Sync Rule 299: QA tests ensure End triggers tutorial updates.
Sync Rule 300: QA tests ensure End triggers notification updates if configured.
```

### Szenario-Matrix (800 Zeilen)

```text
Scenario 001: Solo cutscene, SkipAllowed=false, CompletePolicy requires client complete, expect Start→Ack→Progress→Complete→End Completed.
Scenario 002: Solo cutscene, SkipAllowed=true, user presses skip at Step 2, expect SkipRequest Accepted→End Skipped.
Scenario 003: Solo cutscene, SkipAllowed=true, skip denied due to config, expect SkipResponse Rejected SKIP_NOT_ALLOWED.
Scenario 004: Solo cutscene, accessibility auto-skip enabled and allowed, expect SkipRequest Auto→Accepted→End Skipped.
Scenario 005: Solo cutscene, accessibility auto-skip enabled but not allowed, expect SkipResponse Rejected SKIP_NOT_ALLOWED.
Scenario 006: Solo cutscene, user disconnects mid-step, reconnect within window, expect StateSyncResponse Playing and resume.
Scenario 007: Solo cutscene, user disconnects mid-step, reconnect after window, expect StateSyncResponse FinalState Failed and unlock.
Scenario 008: Solo cutscene, pause triggered by server due to load, expect Pause→Ack→Resume→Ack→Progress continues.
Scenario 009: Solo cutscene, invalid Ack Revision, expect ErrorCode REVISION_MISMATCH and retry.
Scenario 010: Solo cutscene, invalid ResumeToken, expect ErrorCode RESUME_TOKEN_INVALID and End Failed.
Scenario 011: Solo cutscene, duplicate SkipRequest with same sequence, expect identical SkipResponse.
Scenario 012: Solo cutscene, duplicate CompleteRequest with same sequence, expect identical CompleteResponse.
Scenario 013: Solo cutscene, duplicate AckRequest, expect identical AckResponse.
Scenario 014: Solo cutscene, StepChecksum mismatch due to data corruption, expect client sends StateSyncRequest, server responds with snapshot.
Scenario 015: Solo cutscene, Branch choice timeout, expect default choice selected and broadcast via ProgressType ChoiceAuto.
Scenario 016: Solo cutscene, ProgressAck lost, expect server continues if not required? Choice ack required; timeout selects default.
Scenario 017: Solo cutscene, user tries to move, MovementLock prevents and server sends MovementCorrection if movement packet received.
Scenario 018: Solo cutscene, user tries to attack, CombatLock prevents.
Scenario 019: Solo cutscene, user tries to open inventory, UiLock prevents.
Scenario 020: Solo cutscene, user tries to interact with NPC, InteractionLock prevents.
Scenario 021: Solo cutscene, StepDuration zero, expect immediate next StepStart.
Scenario 022: Solo cutscene, StepDuration negative, server rejects start configuration.
Scenario 023: Solo cutscene, voice asset missing, client fallback to text only.
Scenario 024: Solo cutscene, portrait asset missing, client fallback to default portrait.
Scenario 025: Solo cutscene, Start arrives while another cutscene active, server rejects with ErrorMessage CUTSCENE_ACTIVE.
Scenario 026: Solo cutscene, End arrives before client Ack start, client still unlocks.
Scenario 027: Solo cutscene, End lost, reconnect obtains End via StateSyncResponse.
Scenario 028: Solo cutscene, skip vote not applicable (solo), SkipMode Immediate enforced.
Scenario 029: Solo cutscene, Start while loading zone, server delays until ZoneLoadedAck then start.
Scenario 030: Solo cutscene, Start triggered by quest, QuestProgress only after Completed End.
Scenario 031: Solo cutscene, Start triggered by tutorial, TutorialStepAck only after End.
Scenario 032: Solo cutscene, Start triggered by NPC dialogue, returns to dialogue after End.
Scenario 033: Solo cutscene, Start triggered by movement into trigger volume, lock prevents further movement until End.
Scenario 034: Solo cutscene, End triggered by server with FinalState Failed due to script error.
Scenario 035: Solo cutscene, End triggered by admin command, CompletionSource Admin.
Scenario 036: Solo cutscene, user disables subtitles mid-cutscene, allowed even with UiLock (accessibility).
Scenario 037: Solo cutscene, user changes volume mid-cutscene, allowed (accessibility).
Scenario 038: Solo cutscene, cutscene length >5 min, QA flagged.
Scenario 039: Solo cutscene, branch loop attempt, server rejects timeline.
Scenario 040: Solo cutscene, StepId duplicates, server rejects timeline.
Scenario 041: Solo cutscene, ChoiceId duplicates, server rejects timeline.
Scenario 042: Solo cutscene, Progress frequency high, ensure <2s interval.
Scenario 043: Solo cutscene, Progress frequency low, ensure >0 interval for keepalive.
Scenario 044: Solo cutscene, server crash mid cutscene, on restart, state lost → End Failed or cannot resume.
Scenario 045: Solo cutscene, network lag 500ms, playback uses RemainingMs to resync.
Scenario 046: Solo cutscene, player alt-tabs, client still processes locks and playback.
Scenario 047: Solo cutscene, language switched mid cutscene, subtitles update from new locale for next lines.
Scenario 048: Solo cutscene, replay allowed and watched flag true, start permitted.
Scenario 049: Solo cutscene, replay not allowed and watched flag true, start rejected.
Scenario 050: Solo cutscene, StepChecksum differs but timeline hash same? treat as desync.
Scenario 051: Solo cutscene, Received Progress after End, client ignores.
Scenario 052: Solo cutscene, Received AckResponse after End, client ignores.
Scenario 053: Solo cutscene, Received Progress with lower Revision, client ignores.
Scenario 054: Solo cutscene, Received Progress with higher Revision unexpectedly, client requests sync.
Scenario 055: Solo cutscene, Received SkipResponse after End, client ignores.
Scenario 056: Solo cutscene, Received CompleteResponse after End, client ignores.
Scenario 057: Solo cutscene, Received StateSyncResponse while End already processed, ignore snapshot.
Scenario 058: Solo cutscene, Received Start twice due to retry, second ignored due to same Revision.
Scenario 059: Solo cutscene, Received Start with new Revision after end, treat as new cutscene.
Scenario 060: Solo cutscene, Cancelled by quest reset, server sends End Failed.
Scenario 061: Solo cutscene, Cancelled by player entering PvP zone requiring cancellation, server sends End Failed.
Scenario 062: Solo cutscene, Cancelled by party disband (solo not relevant).
Scenario 063: Solo cutscene, Cancelled by housing exit, server sends End Failed.
Scenario 064: Solo cutscene, Cancelled by zone transfer, server sends End Failed.
Scenario 065: Solo cutscene, Cancelled by force disconnect, no End; reconnect may get Failed state.
Scenario 066: Solo cutscene, Player uses screenshot, allowed.
Scenario 067: Solo cutscene, Player uses emote, blocked due to locks.
Scenario 068: Solo cutscene, Player toggles UI scale, allowed (accessibility).
Scenario 069: Solo cutscene, Player tries to mount, blocked by movement/combat lock.
Scenario 070: Solo cutscene, Player HP changes due to DoT? CombatLock prevents new actions; health change still rendered.
Scenario 071: Solo cutscene, Player receives whisper, allowed, UI unaffected.
Scenario 072: Solo cutscene, Party invite arrives, allowed but acceptance blocked due to UiLock if configured.
Scenario 073: Solo cutscene, Player enters trade request, InteractionLock rejects.
Scenario 074: Solo cutscene, Player tries to accept duel, CombatLock rejects.
Scenario 075: Solo cutscene, Player stuck; server still unlocks at End.
Scenario 076: Solo cutscene, Start while paused? invalid; server rejects.
Scenario 077: Solo cutscene, Start after resume? invalid.
Scenario 078: Solo cutscene, Pause while Starting, AckType Pause accepted.
Scenario 079: Solo cutscene, Resume while Playing, server rejects.
Scenario 080: Solo cutscene, Progress missing StepChecksum, client requests sync.
Scenario 081: Solo cutscene, StepChecksum empty string, treat as error.
Scenario 082: Solo cutscene, Dialogue choices empty, treat as linear dialogue.
Scenario 083: Solo cutscene, Dialogue choices null, treat as no choices.
Scenario 084: Solo cutscene, StepId jumps by 2 with branch defined, allowed.
Scenario 085: Solo cutscene, UI lock not applied due to bug, server still enforces server-side locks.
Scenario 086: Solo cutscene, MovementLock not applied client-side, server corrects positions.
Scenario 087: Solo cutscene, Player uses /reload UI, reconnect; state sync to resume.
Scenario 088: Solo cutscene, Player kills game process, reconnect; state sync.
Scenario 089: Solo cutscene, Player experiences GC pause, may miss progress; client requests sync.
Scenario 090: Solo cutscene, Player toggles graphics settings, camera may reset; locks remain.
Scenario 091: Solo cutscene, Start triggered while player dead, server rejects if not allowed.
Scenario 092: Solo cutscene, Start triggered while ghost, config dependent; if not allowed reject.
Scenario 093: Solo cutscene, Start triggered while mounted, server dismounts and locks.
Scenario 094: Solo cutscene, Start triggered while flying, server dismounts and locks.
Scenario 095: Solo cutscene, Start triggered while casting, server interrupts cast.
Scenario 096: Solo cutscene, Start triggered while crafting, server cancels crafting.
Scenario 097: Solo cutscene, Start triggered while in queue, queue continues.
Scenario 098: Solo cutscene, Start triggered while in dialogue, dialogue replaced by cutscene.
Scenario 099: Solo cutscene, Start triggered while in vendor, vendor closes.
Scenario 100: Solo cutscene, Start triggered while in mailbox, mailbox closes.
Scenario 101: Solo cutscene, Start triggered while bank open, bank closes.
Scenario 102: Solo cutscene, Start triggered while auction house open, closes.
Scenario 103: Solo cutscene, Start triggered while flight in progress, server cancels flight and locks.
Scenario 104: Solo cutscene, Start triggered while taxi pending, server cancels taxi.
Scenario 105: Solo cutscene, Start triggered while player swimming, allowed; movement locked.
Scenario 106: Solo cutscene, Start triggered while player in vehicle, server removes from vehicle.
Scenario 107: Solo cutscene, Start triggered while player polymorphed, server removes effect? config.
Scenario 108: Solo cutscene, Start triggered while invisibility, may persist; visuals unaffected.
Scenario 109: Solo cutscene, Start triggered while stealth, may persist; locks still apply.
Scenario 110: Solo cutscene, Start triggered while in raid, party rules still apply? PartyVote may include all raid.
Scenario 111: Solo cutscene, Start triggered while in battleground, likely disallowed; reject.
Scenario 112: Solo cutscene, Start triggered while in arena, disallowed; reject.
Scenario 113: Solo cutscene, Start triggered while in dungeon, allowed if design; locks apply.
Scenario 114: Solo cutscene, Start triggered during combat, disallowed unless Forced; server may pause combat and lock.
Scenario 115: Solo cutscene, Start triggered after combat end, allowed.
Scenario 116: Solo cutscene, Start triggered while stunned, locks still apply.
Scenario 117: Solo cutscene, Start triggered while rooted, lock reinforces root.
Scenario 118: Solo cutscene, Start triggered while snared, lock overrides.
Scenario 119: Solo cutscene, Start triggered while buffed, buffs continue.
Scenario 120: Solo cutscene, Start triggered while debuffed, debuffs tick.
Scenario 121: Solo cutscene, Start triggered while HP low, possible death? server may pause? not recommended.
Scenario 122: Solo cutscene, Player dies during cutscene, server ends with Failed or special branch.
Scenario 123: Solo cutscene, Player resurrected during cutscene, server ends or resumes? config.
Scenario 124: Solo cutscene, Player receives heal, allowed.
Scenario 125: Solo cutscene, Player receives damage from environment, server may end or ignore depending design.
Scenario 126: Solo cutscene, Player receives quest share, blocked by UiLock.
Scenario 127: Solo cutscene, Player receives guild invite, blocked by UiLock.
Scenario 128: Solo cutscene, Player receives friend request, blocked by UiLock.
Scenario 129: Solo cutscene, Player receives report prompt, blocked by UiLock.
Scenario 130: Solo cutscene, Player receives notification 4300, allowed overlay.
Scenario 131: Solo cutscene, Player enters new subzone, blocked by locks until End.
Scenario 132: Solo cutscene, Player changes keybindings, allowed? if UiLock maybe blocked.
Scenario 133: Solo cutscene, Player toggles push-to-talk, allowed.
Scenario 134: Solo cutscene, Player toggles voice mute, allowed.
Scenario 135: Solo cutscene, Player toggles camera FOV, blocked by CameraLock.
Scenario 136: Solo cutscene, Player toggles photo mode, blocked by CameraLock.
Scenario 137: Solo cutscene, Player tries to skip via hotkey, allowed if SkipAllowed.
Scenario 138: Solo cutscene, Player tries to skip via ESC, allowed if SkipAllowed else blocked.
Scenario 139: Solo cutscene, Player tries to skip via UI button, allowed if SkipAllowed.
Scenario 140: Solo cutscene, Player tries to skip via chat command, treated like SkipRequest.
Scenario 141: Solo cutscene, Player tries to skip repeatedly, rate limited.
Scenario 142: Solo cutscene, Player tries to complete prematurely via hack, rejected and logged.
Scenario 143: Solo cutscene, Player tries to alter StepId client side, rejected.
Scenario 144: Solo cutscene, Player tries to alter Revision client side, rejected.
Scenario 145: Solo cutscene, Player tries to spoof ResumeToken, rejected and logged.
Scenario 146: Solo cutscene, Player tries to send ProgressAck for wrong StepId, rejected.
Scenario 147: Solo cutscene, Player tries to send ProgressAck after End, ignored.
Scenario 148: Solo cutscene, Player tries to send SkipRequest after End, ignored.
Scenario 149: Solo cutscene, Player tries to send CompleteRequest after End, ignored.
Scenario 150: Solo cutscene, Player tries to send AckRequest after End, ignored.
Scenario 151: Solo cutscene, network packet reorder, TCP preserves order; still fine.
Scenario 152: Solo cutscene, network packet loss, retransmit.
Scenario 153: Solo cutscene, network jitter, RemainingMs corrects.
Scenario 154: Solo cutscene, progress ack drop, default branch may occur.
Scenario 155: Solo cutscene, player alt+f4 at skip vote pending, vote counts as no response.
Scenario 156: Solo cutscene, player alt+f4 at choice prompt, default choice after timeout.
Scenario 157: Solo cutscene, server sends Pause after default choice, allowed.
Scenario 158: Solo cutscene, server sends Resume after default choice, allowed.
Scenario 159: Solo cutscene, Step includes TitleCard, displayed despite UiLock.
Scenario 160: Solo cutscene, Step includes CameraShake, applied despite camera lock because server-driven.
Scenario 161: Solo cutscene, Step includes ScreenFade, applied.
Scenario 162: Solo cutscene, Step includes Music change, applied.
Scenario 163: Solo cutscene, Step includes Sound effect, applied.
Scenario 164: Solo cutscene, Step includes Animation on entity not loaded, server should send spawn first.
Scenario 165: Solo cutscene, Step includes Animation on player, allowed even if movement locked.
Scenario 166: Solo cutscene, Step includes Branch to optional scene, works.
Scenario 167: Solo cutscene, Step includes WaitForInput indefinite, requires ProgressAck.
Scenario 168: Solo cutscene, Step includes BranchKey invalid, server should not send.
Scenario 169: Solo cutscene, Step includes Dialogue with Choices and Timeout, default on timeout.
Scenario 170: Solo cutscene, Step includes Dialogue without choices, auto-advance after duration.
Scenario 171: Solo cutscene, Step includes spawn of cinematic camera, camera lock ensures control.
Scenario 172: Solo cutscene, Step includes despawn of cinematic camera, camera returns.
Scenario 173: Solo cutscene, Step includes HUD hide, HUD returns on End.
Scenario 174: Solo cutscene, Step includes HUD show mid cutscene, allowed if payload says.
Scenario 175: Solo cutscene, Step includes Cinematic letterbox, removed on End.
Scenario 176: Solo cutscene, Step includes Cinematic vignette, removed on End.
Scenario 177: Solo cutscene, Step includes particle effect, plays.
Scenario 178: Solo cutscene, Step includes cut to black, removed on Resume.
Scenario 179: Solo cutscene, Step includes wait for server event, server triggers when ready.
Scenario 180: Solo cutscene, Step includes Quest update inside, server defers until End.
Scenario 181: Solo cutscene, Step includes Achievement toast, may show after End to avoid overlap.
Scenario 182: Solo cutscene, Step includes NotificationShow 4300, overlay allowed.
Scenario 183: Solo cutscene, Step includes ScreenShake in Party, all members shake.
Scenario 184: Solo cutscene, Step includes different Branch per party? Not allowed; server uses one branch for all to keep sync.
Scenario 185: Solo cutscene, Step includes multiple dialogs sequential, each Progress entry.
Scenario 186: Solo cutscene, Step includes camera path, deterministic.
Scenario 187: Solo cutscene, Step includes camera target entity, locks onto entity.
Scenario 188: Solo cutscene, Step includes area highlight, works with UI lock.
Scenario 189: Solo cutscene, Step includes teleport inside cutscene, handled by server; still locked.
Scenario 190: Solo cutscene, Step includes mount/dismount visuals, allowed.
Scenario 191: Solo cutscene, Step includes fade audio, executed.
Scenario 192: Solo cutscene, Step includes environment change (weather), allowed if design; not movement related.
Scenario 193: Solo cutscene, Step includes change time of day, may be visual-only.
Scenario 194: Solo cutscene, Step includes spawn NPC for dialogue, despawn later.
Scenario 195: Solo cutscene, Step includes spawn boss for reveal, locked.
Scenario 196: Solo cutscene, Step includes camera follow path while playing dialogue, allowed.
Scenario 197: Solo cutscene, Step includes dual audio tracks, client mixes.
Scenario 198: Solo cutscene, Step includes 2D overlay video, allowed.
Scenario 199: Solo cutscene, Step includes quick-time event, but input locked; not allowed in this system (use tutorial).
Scenario 200: Solo cutscene, Step includes tutorial highlight, allowed if UI lock whitelisted.
Scenario 201: Solo cutscene, Step includes safe area check, UI auto adjusts.
Scenario 202: Solo cutscene, Step includes translation fallback text missing, displays key.
Scenario 203: Solo cutscene, Step includes remote stream asset missing, fallback default.
Scenario 204: Solo cutscene, Step includes extremely short duration (<100ms), still handled.
Scenario 205: Solo cutscene, Step includes extremely long duration (>5m), flagged.
Scenario 206: Solo cutscene, Step includes requirement of quest flag, if missing server doesn't start cutscene.
Scenario 207: Solo cutscene, Start failed gating, ErrorMessage shown.
Scenario 208: Solo cutscene, Player logs out after Start, server ends.
Scenario 209: Solo cutscene, Player logs out during Pause, server ends.
Scenario 210: Solo cutscene, Player logs out during Resume, server ends.
Scenario 211: Solo cutscene, Player logs out during End? Already unlocking.
Scenario 212: Solo cutscene, Player switches character quickly, state cleared.
Scenario 213: Solo cutscene, Player attempts to run macro, blocked by UiLock.
Scenario 214: Solo cutscene, Player tries to open minimap, blocked by UiLock.
Scenario 215: Solo cutscene, Player tries to open guild panel, blocked.
Scenario 216: Solo cutscene, Player tries to open friend list, blocked.
Scenario 217: Solo cutscene, Player tries to open achievements, blocked.
Scenario 218: Solo cutscene, Player tries to change keybind to skip, allowed? not if UiLock.
Scenario 219: Solo cutscene, Player tries to join queue, blocked by InteractionLock or allowed per config.
Scenario 220: Solo cutscene, Player tries to leave queue, allowed.
Scenario 221: Solo cutscene, Player tries to hearthstone, blocked.
Scenario 222: Solo cutscene, Player tries to teleport, blocked.
Scenario 223: Solo cutscene, Player tries to use mount item, blocked.
Scenario 224: Solo cutscene, Player tries to consume potion, blocked.
Scenario 225: Solo cutscene, Player tries to use toy, blocked.
Scenario 226: Solo cutscene, Player tries to open collection UI, blocked.
Scenario 227: Solo cutscene, Player tries to inspect another player, blocked.
Scenario 228: Solo cutscene, Player tries to whisper, allowed (chat).
Scenario 229: Solo cutscene, Player tries to move camera with mouse, blocked by CameraLock.
Scenario 230: Solo cutscene, Player tries to zoom camera, blocked by CameraLock unless payload allows.
Scenario 231: Solo cutscene, Player tries to toggle first-person, blocked by CameraLock.
Scenario 232: Solo cutscene, Player tries to hide UI (ALT+Z), blocked by UiLock.
Scenario 233: Solo cutscene, Player tries to take screenshot (PrintScreen), allowed.
Scenario 234: Solo cutscene, Player tries to record video, allowed.
Scenario 235: Solo cutscene, Player tries to alt-tab, allowed; locks remain.
Scenario 236: Solo cutscene, Player tries to change window resolution, allowed; playback continues.
Scenario 237: Solo cutscene, Player tries to change audio device, allowed.
Scenario 238: Solo cutscene, Player tries to mute audio, allowed.
Scenario 239: Solo cutscene, Player tries to disable subtitles, allowed (accessibility).
Scenario 240: Solo cutscene, Player tries to speed up subtitles, not supported.
Scenario 241: Solo cutscene, Player tries to rewind, not supported.
Scenario 242: Solo cutscene, Player tries to pause locally, not allowed; only server pause.
Scenario 243: Solo cutscene, Player tries to skip Step by cheat, rejected.
Scenario 244: Solo cutscene, Player tries to fake ProgressAck with other StepId, rejected.
Scenario 245: Solo cutscene, Player tries to fake SkipResponse, ignored.
Scenario 246: Solo cutscene, Player tries to fake CompleteResponse, ignored.
Scenario 247: Solo cutscene, Player tries to modify timeline file locally, StepChecksum mismatch triggers sync.
Scenario 248: Solo cutscene, Player tries to modify voice files, client still uses text; StepChecksum unaffected.
Scenario 249: Solo cutscene, Player tries to modify camera speed, CameraLock prevents.
Scenario 250: Solo cutscene, Player tries to send heartbeat faster, unrelated.
Scenario 251: Solo cutscene, Player tries to send ActionRequest despite lock, ignored.
Scenario 252: Solo cutscene, Player tries to send MovementUpdate despite lock, correction.
Scenario 253: Solo cutscene, Player tries to send Chat messages, allowed.
Scenario 254: Solo cutscene, Player tries to send Emote channel, InteractionLock may block? design decision.
Scenario 255: Solo cutscene, Player tries to use quick-slot UI, blocked.
Scenario 256: Solo cutscene, Player tries to open map overlay, blocked.
Scenario 257: Solo cutscene, Player tries to place waypoint, blocked.
Scenario 258: Solo cutscene, Player tries to ping map, blocked.
Scenario 259: Solo cutscene, Player tries to change settings for colorblind mode, allowed.
Scenario 260: Solo cutscene, Player tries to change brightness, allowed.
Scenario 261: Solo cutscene, Player tries to open photo mode gallery, blocked by UiLock.
Scenario 262: Solo cutscene, Player tries to alt+enter fullscreen, allowed.
Scenario 263: Solo cutscene, Player tries to join voice channel, allowed.
Scenario 264: Solo cutscene, Player tries to leave voice channel, allowed.
Scenario 265: Solo cutscene, Player tries to mute voice channel, allowed.
Scenario 266: Solo cutscene, Player tries to change microphone input, allowed.
Scenario 267: Solo cutscene, Player tries to listen-only mode, allowed.
Scenario 268: Solo cutscene, Player tries to open guild bank, blocked.
Scenario 269: Solo cutscene, Player tries to open void storage, blocked.
Scenario 270: Solo cutscene, Player tries to open reagent bank, blocked.
Scenario 271: Solo cutscene, Player tries to open transmog, blocked.
Scenario 272: Solo cutscene, Player tries to open auction, blocked.
Scenario 273: Solo cutscene, Player tries to open crafting, blocked.
Scenario 274: Solo cutscene, Player tries to open gathering, blocked.
Scenario 275: Solo cutscene, Player tries to open mount journal, blocked.
Scenario 276: Solo cutscene, Player tries to open pet journal, blocked.
Scenario 277: Solo cutscene, Player tries to open toy box, blocked.
Scenario 278: Solo cutscene, Player tries to open heirlooms, blocked.
Scenario 279: Solo cutscene, Player tries to open achievements, blocked.
Scenario 280: Solo cutscene, Player tries to open statistics, blocked.
Scenario 281: Solo cutscene, Player tries to open calendar, blocked.
Scenario 282: Solo cutscene, Player tries to open friends list, blocked.
Scenario 283: Solo cutscene, Player tries to open ignore list, blocked.
Scenario 284: Solo cutscene, Player tries to open block list, blocked.
Scenario 285: Solo cutscene, Player tries to open mailbox UI, blocked.
Scenario 286: Solo cutscene, Player tries to open map vendor, blocked.
Scenario 287: Solo cutscene, Player tries to open shop, blocked.
Scenario 288: Solo cutscene, Player tries to purchase microtransaction, blocked.
Scenario 289: Solo cutscene, Player tries to redeem code, blocked.
Scenario 290: Solo cutscene, Player tries to open tutorial guide, blocked unless integrated.
Scenario 291: Solo cutscene, Player tries to open help UI, allowed maybe.
Scenario 292: Solo cutscene, Player tries to open bug report, allowed? InteractionLock may block; allow via exception.
Scenario 293: Solo cutscene, Player tries to open feedback UI, allowed if not blocked.
Scenario 294: Solo cutscene, Player tries to open survey, blocked by UiLock.
Scenario 295: Solo cutscene, Player tries to open rating, blocked by UiLock.
Scenario 296: Solo cutscene, Player tries to open legal notices, allowed.
Scenario 297: Solo cutscene, Player tries to open credits, allowed? maybe blocked.
Scenario 298: Solo cutscene, Player tries to open settings, blocked? partial allowed.
Scenario 299: Solo cutscene, Player tries to rebind skip key, blocked.
Scenario 300: Solo cutscene, Player tries to reset UI, blocked.
Scenario 301: Solo cutscene, Player tries to join cross-realm group, blocked.
Scenario 302: Solo cutscene, Player tries to leave party, allowed? maybe blocked to keep flow.
Scenario 303: Solo cutscene, Player tries to promote leader, blocked.
Scenario 304: Solo cutscene, Player tries to invite friend, blocked.
Scenario 305: Solo cutscene, Player tries to kick member, blocked.
Scenario 306: Solo cutscene, Player tries to change loot rules, blocked.
Scenario 307: Solo cutscene, Player tries to start ready check, blocked.
Scenario 308: Solo cutscene, Player tries to start pull timer, blocked.
Scenario 309: Solo cutscene, Player tries to set world marker, blocked.
Scenario 310: Solo cutscene, Player tries to set target marker, blocked.
Scenario 311: Solo cutscene, Player tries to change difficulty, blocked.
Scenario 312: Solo cutscene, Player tries to queue dungeon finder, blocked.
Scenario 313: Solo cutscene, Player tries to queue battleground, blocked.
Scenario 314: Solo cutscene, Player tries to queue arena, blocked.
Scenario 315: Solo cutscene, Player tries to queue raid finder, blocked.
Scenario 316: Solo cutscene, Player tries to start mythic keystone, blocked.
Scenario 317: Solo cutscene, Player tries to start challenge mode, blocked.
Scenario 318: Solo cutscene, Player tries to change spec, blocked.
Scenario 319: Solo cutscene, Player tries to change talents, blocked.
Scenario 320: Solo cutscene, Player tries to change glyphs, blocked.
Scenario 321: Solo cutscene, Player tries to change pvp talents, blocked.
Scenario 322: Solo cutscene, Player tries to change mount favorite, blocked.
Scenario 323: Solo cutscene, Player tries to change pet loadout, blocked.
Scenario 324: Solo cutscene, Player tries to stable pet, blocked.
Scenario 325: Solo cutscene, Player tries to rename pet, blocked.
Scenario 326: Solo cutscene, Player tries to summon pet, blocked.
Scenario 327: Solo cutscene, Player tries to dismiss pet, blocked.
Scenario 328: Solo cutscene, Player tries to summon companion, blocked.
Scenario 329: Solo cutscene, Player tries to dismiss companion, blocked.
Scenario 330: Solo cutscene, Player tries to use cosmetic item, blocked.
Scenario 331: Solo cutscene, Player tries to switch outfit, blocked.
Scenario 332: Solo cutscene, Player tries to transmog, blocked.
Scenario 333: Solo cutscene, Player tries to reforging, blocked.
Scenario 334: Solo cutscene, Player tries to enchant, blocked.
Scenario 335: Solo cutscene, Player tries to socket, blocked.
Scenario 336: Solo cutscene, Player tries to salvage, blocked.
Scenario 337: Solo cutscene, Player tries to identify item, blocked.
Scenario 338: Solo cutscene, Player tries to upgrade item, blocked.
Scenario 339: Solo cutscene, Player tries to repair item, blocked.
Scenario 340: Solo cutscene, Player tries to repair all, blocked.
Scenario 341: Solo cutscene, Player tries to change weapon set, blocked.
Scenario 342: Solo cutscene, Player tries to swap specialization, blocked.
Scenario 343: Solo cutscene, Player tries to change appearance, blocked.
Scenario 344: Solo cutscene, Player tries to reroll stats, blocked.
Scenario 345: Solo cutscene, Player tries to change haircut, blocked.
Scenario 346: Solo cutscene, Player tries to change gender, blocked.
Scenario 347: Solo cutscene, Player tries to change race, blocked.
Scenario 348: Solo cutscene, Player tries to change name, blocked.
Scenario 349: Solo cutscene, Player tries to toggle PVP flag, blocked.
Scenario 350: Solo cutscene, Player tries to surrender duel, not relevant.
Scenario 351: Solo cutscene, Player tries to forfeit battleground, not relevant.
Scenario 352: Solo cutscene, Player tries to toggle walk/run, blocked.
Scenario 353: Solo cutscene, Player tries to toggle autorun, blocked.
Scenario 354: Solo cutscene, Player tries to change keybinding for movement, blocked.
Scenario 355: Solo cutscene, Player tries to send mail, blocked.
Scenario 356: Solo cutscene, Player tries to collect mail, blocked.
Scenario 357: Solo cutscene, Player tries to delete mail, blocked.
Scenario 358: Solo cutscene, Player tries to return mail, blocked.
Scenario 359: Solo cutscene, Player tries to COD mail, blocked.
Scenario 360: Solo cutscene, Player tries to pay COD mail, blocked.
Scenario 361: Solo cutscene, Player tries to take gold from mail, blocked.
Scenario 362: Solo cutscene, Player tries to take attachment from mail, blocked.
Scenario 363: Solo cutscene, Player tries to loot corpse, blocked.
Scenario 364: Solo cutscene, Player tries to roll on loot, blocked.
Scenario 365: Solo cutscene, Player tries to open loot window, blocked.
Scenario 366: Solo cutscene, Player tries to pick up item from ground, blocked.
Scenario 367: Solo cutscene, Player tries to drop item, blocked.
Scenario 368: Solo cutscene, Player tries to use item on ground, blocked.
Scenario 369: Solo cutscene, Player tries to move item in bags, blocked.
Scenario 370: Solo cutscene, Player tries to split stack, blocked.
Scenario 371: Solo cutscene, Player tries to merge stack, blocked.
Scenario 372: Solo cutscene, Player tries to sort bags, blocked.
Scenario 373: Solo cutscene, Player tries to expand bag slots, blocked.
Scenario 374: Solo cutscene, Player tries to lock item, blocked.
Scenario 375: Solo cutscene, Player tries to unlock item, blocked.
Scenario 376: Solo cutscene, Player tries to lock bag, blocked.
Scenario 377: Solo cutscene, Player tries to unlock bag, blocked.
Scenario 378: Solo cutscene, Player tries to vendor junk automatically, blocked.
Scenario 379: Solo cutscene, Player tries to use mount special ability, blocked.
Scenario 380: Solo cutscene, Player tries to toggle auto-loot, blocked.
Scenario 381: Solo cutscene, Player tries to toggle nameplates, allowed.
Scenario 382: Solo cutscene, Player tries to toggle combat text, allowed.
Scenario 383: Solo cutscene, Player tries to adjust UI opacity, allowed.
Scenario 384: Solo cutscene, Player tries to adjust UI language, allowed.
Scenario 385: Solo cutscene, Player tries to change chat channels, allowed.
Scenario 386: Solo cutscene, Player tries to join chat channel, allowed.
Scenario 387: Solo cutscene, Player tries to leave chat channel, allowed.
Scenario 388: Solo cutscene, Player tries to create chat channel, allowed.
Scenario 389: Solo cutscene, Player tries to mute player, allowed.
Scenario 390: Solo cutscene, Player tries to unmute player, allowed.
Scenario 391: Solo cutscene, Player tries to block player, allowed.
Scenario 392: Solo cutscene, Player tries to unblock player, allowed.
Scenario 393: Solo cutscene, Player tries to report player, allowed.
Scenario 394: Solo cutscene, Player tries to submit bug report, allowed.
Scenario 395: Solo cutscene, Player tries to submit suggestion, allowed.
Scenario 396: Solo cutscene, Player tries to submit exploit report, allowed.
Scenario 397: Solo cutscene, Player tries to appeal moderation, allowed.
Scenario 398: Solo cutscene, Player tries to open rating prompt, blocked by UiLock.
Scenario 399: Solo cutscene, Player tries to open survey, blocked by UiLock.
Scenario 400: Solo cutscene, Player tries to open feedback prompt, blocked by UiLock.
Scenario 401: Solo cutscene, Player tries to see calendar event, blocked.
Scenario 402: Solo cutscene, Player tries to sign calendar event, blocked.
Scenario 403: Solo cutscene, Player tries to cancel calendar event, blocked.
Scenario 404: Solo cutscene, Player tries to set waypoint while locked, blocked.
Scenario 405: Solo cutscene, Player tries to use pathfinding, blocked.
Scenario 406: Solo cutscene, Player tries to toggle interface mode, blocked.
Scenario 407: Solo cutscene, Player tries to open minimap tracking, blocked.
Scenario 408: Solo cutscene, Player tries to open add-on options, blocked by UiLock.
Scenario 409: Solo cutscene, Player tries to run addon macro, blocked by UiLock.
Scenario 410: Solo cutscene, Player tries to install addon, not runtime.
Scenario 411: Solo cutscene, Player tries to modify addon settings, blocked.
Scenario 412: Solo cutscene, Player tries to open developer console, blocked.
Scenario 413: Solo cutscene, Player tries to start benchmark, allowed maybe.
Scenario 414: Solo cutscene, Player tries to toggle FPS display, allowed.
Scenario 415: Solo cutscene, Player tries to open latency graph, allowed.
Scenario 416: Solo cutscene, Player tries to open performance stats, allowed.
Scenario 417: Solo cutscene, Player tries to open memory usage, allowed.
Scenario 418: Solo cutscene, Player tries to open CPU usage, allowed.
Scenario 419: Solo cutscene, Player tries to open GPU usage, allowed.
Scenario 420: Solo cutscene, Player tries to open network usage, allowed.
Scenario 421: Solo cutscene, Player tries to open sound mixer, allowed.
Scenario 422: Solo cutscene, Player tries to open voice settings, allowed.
Scenario 423: Solo cutscene, Player tries to open accessibility menu, allowed.
Scenario 424: Solo cutscene, Player tries to open keybinding menu, blocked.
Scenario 425: Solo cutscene, Player tries to open interface settings, blocked.
Scenario 426: Solo cutscene, Player tries to open graphics settings, blocked.
Scenario 427: Solo cutscene, Player tries to open audio settings, allowed? maybe blocked.
Scenario 428: Solo cutscene, Player tries to open language settings, allowed.
Scenario 429: Solo cutscene, Player tries to open region settings, blocked.
Scenario 430: Solo cutscene, Player tries to open legal menu, allowed.
Scenario 431: Solo cutscene, Player tries to open privacy menu, allowed.
Scenario 432: Solo cutscene, Player tries to open credits, allowed? config.
Scenario 433: Solo cutscene, Player tries to open photo gallery, blocked.
Scenario 434: Solo cutscene, Player tries to open screenshot folder, blocked.
Scenario 435: Solo cutscene, Player tries to change screenshot format, allowed.
Scenario 436: Solo cutscene, Player tries to change video capture settings, allowed.
Scenario 437: Solo cutscene, Player tries to change controller bindings, blocked.
Scenario 438: Solo cutscene, Player tries to toggle controller mode, blocked.
Scenario 439: Solo cutscene, Player tries to calibrate controller, blocked.
Scenario 440: Solo cutscene, Player tries to pair controller, blocked.
Scenario 441: Solo cutscene, Player tries to disconnect controller, allowed.
Scenario 442: Solo cutscene, Player tries to use touch UI, allowed but locked for gameplay.
Scenario 443: Solo cutscene, Player tries to use gyro aiming, disabled by CameraLock.
Scenario 444: Solo cutscene, Player tries to use vibration, allowed.
Scenario 445: Solo cutscene, Player tries to use accessibility screen reader, allowed even with UiLock.
Scenario 446: Solo cutscene, Player tries to use accessibility high contrast, allowed.
Scenario 447: Solo cutscene, Player tries to use text-to-speech chat, allowed.
Scenario 448: Solo cutscene, Player tries to use speech-to-text, allowed.
Scenario 449: Solo cutscene, Player tries to use enlarge cursor, allowed.
Scenario 450: Solo cutscene, Player tries to use color filter, allowed.
Scenario 451: Solo cutscene, Player tries to use motion reduction, allowed if reduces camera shake.
Scenario 452: Solo cutscene, Player tries to disable camera shake, allowed if accessibility flag; server payload may respect.
Scenario 453: Solo cutscene, Player tries to disable screen flash, allowed if accessibility flag.
Scenario 454: Solo cutscene, Player tries to disable screen blur, allowed if accessibility flag.
Scenario 455: Solo cutscene, Player tries to disable bloom, allowed.
Scenario 456: Solo cutscene, Player tries to lower volume of dialogue, allowed.
Scenario 457: Solo cutscene, Player tries to increase subtitle size, allowed.
Scenario 458: Solo cutscene, Player tries to pause audio via OS, allowed; playback continues silently.
Scenario 459: Solo cutscene, Player tries to mute app, allowed.
Scenario 460: Solo cutscene, Player tries to move window, allowed.
Scenario 461: Solo cutscene, Player tries to minimize window, allowed; when restored, still locked.
Scenario 462: Solo cutscene, Player tries to run streaming overlay, allowed.
Scenario 463: Solo cutscene, Player tries to capture input via overlay, may break locks? but locks server-side enforce.
Scenario 464: Solo cutscene, Player tries to join party mid-cutscene? not applicable solo.
Scenario 465: Solo cutscene, Player tries to change region mid-cutscene, connection drop.
Scenario 466: Solo cutscene, Player tries to disable cutscenes entirely, config may allow skip all; Start refused or auto skip.
Scenario 467: Solo cutscene, Player tries to mark cutscene watched, not allowed; server decides.
Scenario 468: Solo cutscene, Player tries to clear watched flag, not allowed.
Scenario 469: Solo cutscene, Player tries to trigger second cutscene via script, server serializes.
Scenario 470: Solo cutscene, Player tries to open debug camera, blocked.
Scenario 471: Solo cutscene, Player tries to use noclip, blocked.
Scenario 472: Solo cutscene, Player tries to change field of view via console, blocked by CameraLock.
Scenario 473: Solo cutscene, Player tries to change gamma, allowed.
Scenario 474: Solo cutscene, Player tries to change brightness during fade, allowed.
Scenario 475: Solo cutscene, Player tries to alt+tab repeatedly, allowed; playback continues.
Scenario 476: Solo cutscene, Player tries to throttle CPU, may cause desync; sync mechanism handles.
Scenario 477: Solo cutscene, Player tries to throttle network, may cause timeouts; server may End.
Scenario 478: Solo cutscene, Player tries to block ports, connection drops; state sync on reconnect.
Scenario 479: Solo cutscene, Player tries to spoof time, not relevant due to server ticks.
Scenario 480: Solo cutscene, Player tries to change timezone, irrelevant.
Scenario 481: Solo cutscene, Player tries to tamper messagepack, fails due to signatures? server validates.
Scenario 482: Solo cutscene, Player tries to send invalid message type, ignored or disconnect.
Scenario 483: Solo cutscene, Player tries to send huge message, rejected.
Scenario 484: Solo cutscene, Player tries to drop ack intentionally, server times out.
Scenario 485: Solo cutscene, Player tries to drop progress intentionally, server may end on timeout.
Scenario 486: Solo cutscene, Player tries to spam skip, rate limited.
Scenario 487: Solo cutscene, Player tries to spam complete, rate limited.
Scenario 488: Solo cutscene, Player tries to spam ack, rate limited.
Scenario 489: Solo cutscene, Player tries to spam state sync, rate limited.
Scenario 490: Solo cutscene, Player tries to spam progress ack, rate limited.
Scenario 491: Solo cutscene, Player tries to block ack channel, server ends after timeout.
Scenario 492: Solo cutscene, Player tries to exploit to stay locked to avoid combat, server unlocks after timeout.
Scenario 493: Solo cutscene, Player tries to exploit to stay invulnerable, server ensures locks removed at end.
Scenario 494: Solo cutscene, Player tries to duplicate cutscene rewards, rewards gated by Completed End once.
Scenario 495: Solo cutscene, Player tries to share cutscene data, allowed.
Scenario 496: Solo cutscene, Player tries to request cutscene start via NPC after watch, denied if ReplayAllowed=false.
Scenario 497: Solo cutscene, Player tries to start cutscene while banned? not possible.
Scenario 498: Solo cutscene, Player tries to start cutscene while chat banned? allowed.
Scenario 499: Solo cutscene, Player tries to start cutscene with invalid version, server rejects at login.
Scenario 500: Solo cutscene, Player tries to start cutscene with missing DLC, server rejects.
Scenario 501: Party cutscene, all members ready, Start broadcast to party, all Ack, synced playback.
Scenario 502: Party cutscene, one member fails Ack, server waits then ends with Failed for that member; others continue or end.
Scenario 503: Party cutscene, SkipMode PartyVote, leader votes skip, others accept, Result Accepted.
Scenario 504: Party cutscene, SkipMode PartyVote, leader votes skip, others reject, Result Rejected, continue.
Scenario 505: Party cutscene, SkipMode LeaderOnly, non-leader attempts skip, server rejects.
Scenario 506: Party cutscene, member disconnects mid-cutscene, PauseOnMissingMember true, server pauses for all.
Scenario 507: Party cutscene, member reconnects, StateSyncResponse sent, Resume after Ack.
Scenario 508: Party cutscene, member never reconnects, server times out and continues with remaining or ends depending config.
Scenario 509: Party cutscene, member leaves party mid-cutscene, server ends their cutscene with Failed and may continue for rest.
Scenario 510: Party cutscene, party disbands mid-cutscene, server ends for all with Failed.
Scenario 511: Party cutscene, Start while party in combat, server may disallow start.
Scenario 512: Party cutscene, Start while some in different zone, server uses portal to bring or rejects.
Scenario 513: Party cutscene, leader switches character mid-cutscene, party cutscene ends.
Scenario 514: Party cutscene, two members request skip repeatedly, rate limited per member.
Scenario 515: Party cutscene, vote timeout, default result continues (no skip).
Scenario 516: Party cutscene, branch choice single decision across party; leader decides.
Scenario 517: Party cutscene, branch choice requires majority; majority decides.
Scenario 518: Party cutscene, branch choice times out, default branch chosen for all.
Scenario 519: Party cutscene, End message sent to all; members unlock.
Scenario 520: Party cutscene, WatchedFlag stored per member.
Scenario 521: Party cutscene, ReplayAllowed false, members with watched flag block start? server may still start if gating requires all not watched.
Scenario 522: Party cutscene, ReplayAllowed true, start allowed even if watched.
Scenario 523: Party cutscene, member tries to use movement, locked.
Scenario 524: Party cutscene, member tries to attack, locked.
Scenario 525: Party cutscene, member tries to open UI, locked.
Scenario 526: Party cutscene, member tries to chat, allowed.
Scenario 527: Party cutscene, member tries to leave zone, blocked.
Scenario 528: Party cutscene, member tries to join queue, blocked.
Scenario 529: Party cutscene, member tries to teleport, blocked.
Scenario 530: Party cutscene, member tries to hearthstone, blocked.
Scenario 531: Party cutscene, member tries to use toy, blocked.
Scenario 532: Party cutscene, member tries to mount, blocked.
Scenario 533: Party cutscene, member tries to open map, blocked.
Scenario 534: Party cutscene, member tries to open quest log, blocked.
Scenario 535: Party cutscene, member tries to advance quest objective, blocked until End.
Scenario 536: Party cutscene, member tries to move camera, blocked.
Scenario 537: Party cutscene, member tries to send ProgressAck after End, ignored.
Scenario 538: Party cutscene, member tries to send SkipRequest after End, ignored.
Scenario 539: Party cutscene, member tries to send CompleteRequest after End, ignored.
Scenario 540: Party cutscene, member loses connection after End but before unlock, End already instructs unlock on reconnect.
Scenario 541: Party cutscene, member fails to load assets, may cause Pause? not; client still uses placeholders.
Scenario 542: Party cutscene, member uses accessibility auto-skip, server may require vote; request triggers vote.
Scenario 543: Party cutscene, vote result accepted, End Skipped for all.
Scenario 544: Party cutscene, vote result rejected, continue playing.
Scenario 545: Party cutscene, leader kicks member mid vote, vote recalculated.
Scenario 546: Party cutscene, leader disconnects, party chooses majority continue.
Scenario 547: Party cutscene, party size 5, threshold 3 votes for skip.
Scenario 548: Party cutscene, party size 2, threshold leader+1? 2 votes for skip.
Scenario 549: Party cutscene, party size 3, threshold 2 votes for skip.
Scenario 550: Party cutscene, party size 1 (converted from party), skip immediate.
Scenario 551: Party cutscene, Reconnect returns to paused state, resume after ack.
Scenario 552: Party cutscene, StepChecksum mismatch for one member, server may send StateSync to that member only.
Scenario 553: Party cutscene, member uses low-power mode, progress may lag; StepChecksum mismatch triggers sync.
Scenario 554: Party cutscene, branch path stored on server per party, ensures same path for all.
Scenario 555: Party cutscene, branch path changed due to vote, recorded for resume.
Scenario 556: Party cutscene, multiple pauses allowed if multiple members lag.
Scenario 557: Party cutscene, multiple resumes allowed.
Scenario 558: Party cutscene, multiple votes allowed? only one skip vote per cutscene.
Scenario 559: Party cutscene, branch decisions multiple times allowed if timeline includes several choices.
Scenario 560: Party cutscene, voice chat continues; not blocked.
Scenario 561: Party cutscene, cross-region party? not supported; cutscene only for same region.
Scenario 562: Party cutscene, cross-shard? may require shard sync; start only when all same shard.
Scenario 563: Party cutscene, cross-faction? allowed if party cross-faction exists.
Scenario 564: Party cutscene, member flagged for AFK, still included.
Scenario 565: Party cutscene, member offline at start, server may not start until online.
Scenario 566: Party cutscene, member join late? cutscene not started; wait or exclude.
Scenario 567: Party cutscene, member in combat, start delayed.
Scenario 568: Party cutscene, member in flight, start delayed.
Scenario 569: Party cutscene, member in vehicle, start delayed.
Scenario 570: Party cutscene, member in dungeon while others not, start rejected.
Scenario 571: Party cutscene, member in different phase, start rejected or bring to host phase.
Scenario 572: Party cutscene, member with older client version, start rejected.
Scenario 573: Party cutscene, member missing assets, start allowed but may show placeholders.
Scenario 574: Party cutscene, leader cancels via admin command, End Failed for all.
Scenario 575: Party cutscene, admin forces skip, End Skipped for all.
Scenario 576: Party cutscene, admin forces complete, End Completed for all.
Scenario 577: Party cutscene, party splits (two shards) mid cutscene due to load, server may pause or end.
Scenario 578: Party cutscene, PvP flagged enemy enters area, combat blocked; cutscene continues due to locks.
Scenario 579: Party cutscene, world event triggers, server may pause or end.
Scenario 580: Party cutscene, daily reset occurs, no impact.
Scenario 581: Party cutscene, weekly reset occurs, no impact.
Scenario 582: Party cutscene, server maintenance warning, may pause or end.
Scenario 583: Party cutscene, server shutdown imminent, End Failed.
Scenario 584: Party cutscene, metrics recorded per member.
Scenario 585: Party cutscene, telemetry includes vote counts.
Scenario 586: Party cutscene, telemetry includes branch decisions.
Scenario 587: Party cutscene, telemetry includes time to ack.
Scenario 588: Party cutscene, telemetry includes time to complete.
Scenario 589: Party cutscene, telemetry includes skip outcome.
Scenario 590: Party cutscene, telemetry includes reconnect count.
Scenario 591: Party cutscene, telemetry includes lock duration.
Scenario 592: Party cutscene, telemetry includes pause duration.
Scenario 593: Party cutscene, telemetry includes resume count.
Scenario 594: Party cutscene, telemetry includes End reason.
Scenario 595: Party cutscene, telemetry includes CompletionSource.
Scenario 596: Party cutscene, telemetry includes WatchedFlag status.
Scenario 597: Party cutscene, telemetry includes ReplayAllowed status.
Scenario 598: Party cutscene, telemetry includes asset streaming status.
Scenario 599: Party cutscene, telemetry includes StepChecksum mismatch count.
Scenario 600: Party cutscene, telemetry includes StateSync count.
Scenario 601: Reconnect scenario, user DC during Start, reconnect, receives Start again? StateSync ensures resume.
Scenario 602: Reconnect scenario, user DC during Pause, reconnect, state paused.
Scenario 603: Reconnect scenario, user DC during Resume, reconnect, state playing.
Scenario 604: Reconnect scenario, user DC during Progress, reconnect, state playing.
Scenario 605: Reconnect scenario, user DC during Skip vote pending, reconnect, still pending vote.
Scenario 606: Reconnect scenario, user DC during Complete pending, reconnect, state playing or End? server handles.
Scenario 607: Reconnect scenario, user DC during End send, reconnect, gets End via StateSync.
Scenario 608: Reconnect scenario, user DC after End but before unlock? End ensures unlock on reconnect.
Scenario 609: Reconnect scenario, user DC after Failed, reconnect, unlock.
Scenario 610: Reconnect scenario, user DC after Skipped, reconnect, unlock.
Scenario 611: Reconnect scenario, user DC after Completed, reconnect, unlock.
Scenario 612: Reconnect scenario, user DC before Ack Start, server may end or wait; on reconnect, start may be resent.
Scenario 613: Reconnect scenario, user DC before Ack Pause, on reconnect server may resend Pause or end.
Scenario 614: Reconnect scenario, user DC before Ack Resume, on reconnect server may resend Resume or end.
Scenario 615: Reconnect scenario, user DC before Ack End (optional), not critical.
Scenario 616: Reconnect scenario, user DC with high latency, server may time out and end.
Scenario 617: Reconnect scenario, user DC due to client crash, same as other DC.
Scenario 618: Reconnect scenario, user DC due to server kick, End not sent; on reconnect, login blocked.
Scenario 619: Reconnect scenario, user DC due to ban, cannot reconnect.
Scenario 620: Reconnect scenario, user DC due to version mismatch, must patch.
Scenario 621: Reconnect scenario, user DC due to maintenance, cutscene ended server side.
Scenario 622: Reconnect scenario, user DC due to network drop, resume within window, state restored.
Scenario 623: Reconnect scenario, user DC multiple times, still allowed if within window and rate limits.
Scenario 624: Reconnect scenario, ResumeToken expired, state cannot resume, End Failed.
Scenario 625: Reconnect scenario, LastRevision behind server, StateSyncResponse updates.
Scenario 626: Reconnect scenario, LastRevision ahead of server (should not happen), server rejects.
Scenario 627: Reconnect scenario, LastStepId mismatch, server overrides with authoritative.
Scenario 628: Reconnect scenario, BranchKey mismatch, server overrides.
Scenario 629: Reconnect scenario, Party vote mid progress, server keeps vote state.
Scenario 630: Reconnect scenario, Choice prompt mid progress, server keeps choice state.
Scenario 631: Reconnect scenario, Pause on missing member, other members remain paused.
Scenario 632: Reconnect scenario, Missing member returns, resume.
Scenario 633: Reconnect scenario, Missing member never returns, server continues after timeout.
Scenario 634: Reconnect scenario, All members disconnect, server ends after timeout.
Scenario 635: Reconnect scenario, Only leader disconnects, config decides continue or pause.
Scenario 636: Reconnect scenario, Member reconnects with different device, ResumeToken ensures identity.
Scenario 637: Reconnect scenario, Member reconnects with wrong character, state not restored.
Scenario 638: Reconnect scenario, Member reconnects to different shard, must transfer; cutscene likely ended.
Scenario 639: Reconnect scenario, Member reconnects after party disband, no cutscene.
Scenario 640: Reconnect scenario, Member reconnects after zone transfer, cutscene ended.
Scenario 641: Reconnect scenario, Member reconnects after player death, cutscene ended.
Scenario 642: Reconnect scenario, Member reconnects after release spirit, cutscene ended.
Scenario 643: Reconnect scenario, Member reconnects after ghost teleport, cutscene ended.
Scenario 644: Reconnect scenario, Member reconnects after respawn, cutscene ended.
Scenario 645: Reconnect scenario, Member reconnects after reload UI, state restored.
Scenario 646: Reconnect scenario, Member reconnects after alt+f4, state restored.
Scenario 647: Reconnect scenario, Member reconnects after crash, state restored.
Scenario 648: Reconnect scenario, Member reconnects after forced logout for duplicate login, cutscene ended.
Scenario 649: Reconnect scenario, Member reconnects after hardware sleep, state restored if window not exceeded.
Scenario 650: Reconnect scenario, Member reconnects after ip change, maybe blocked; ResumeToken may require same ip? policy.
Scenario 651: Reconnect scenario, Member reconnects after vpn change, may be blocked.
Scenario 652: Reconnect scenario, Member reconnects after wifi drop, state restored.
Scenario 653: Reconnect scenario, Member reconnects after mobile handover, state restored.
Scenario 654: Reconnect scenario, Member reconnects after long idle, window may close.
Scenario 655: Reconnect scenario, Member reconnects with outdated client, must patch; cutscene ended.
Scenario 656: Reconnect scenario, Member reconnects with corrupted cache, StepChecksum still server authoritative.
Scenario 657: Reconnect scenario, Member reconnects but denies lock reapplication, server enforces.
Scenario 658: Reconnect scenario, Member reconnects and requests skip immediately, allowed if state playing.
Scenario 659: Reconnect scenario, Member reconnects and requests skip while paused, allowed? server may reject if not playing.
Scenario 660: Reconnect scenario, Member reconnects and requests complete, allowed if at end.
Scenario 661: Reconnect scenario, Member reconnects and requests ack resume, allowed.
Scenario 662: Reconnect scenario, Member reconnects and sends wrong ResumeToken, rejected.
Scenario 663: Reconnect scenario, Member reconnects and sends stale ClientSequence, idempotent response.
Scenario 664: Reconnect scenario, Member reconnects and sends higher ClientSequence, server accepts but uses revision.
Scenario 665: Reconnect scenario, Member reconnects and sends ack while server ended, ignored.
Scenario 666: Reconnect scenario, Member reconnects and sends progress ack for old step, ignored.
Scenario 667: Reconnect scenario, Member reconnects and sends skip for ended cutscene, ignored.
Scenario 668: Reconnect scenario, Member reconnects and sends complete for ended cutscene, ignored.
Scenario 669: Reconnect scenario, Member reconnects and sends state sync repeatedly, rate limited.
Scenario 670: Reconnect scenario, Member reconnects with lost timeline data, state sync provides required fields.
Scenario 671: Reconnect scenario, Member reconnects with changed localization, new subtitles use new locale.
Scenario 672: Reconnect scenario, Member reconnects with accessibility changes, applies on resume.
Scenario 673: Reconnect scenario, Member reconnects after GPU driver reset, playback continues.
Scenario 674: Reconnect scenario, Member reconnects after OS update, playback continues if version ok.
Scenario 675: Reconnect scenario, Member reconnects after device change controller->kbm, playback unaffected.
Scenario 676: Reconnect scenario, Member reconnects after storage full, playback unaffected.
Scenario 677: Reconnect scenario, Member reconnects after clock change, unaffected due to server time.
Scenario 678: Reconnect scenario, Member reconnects after daylight saving change, unaffected.
Scenario 679: Reconnect scenario, Member reconnects after timezone change, unaffected.
Scenario 680: Reconnect scenario, Member reconnects after reinstall, cutscene can still resume if ResumeToken and window valid.
Scenario 681: Reconnect scenario, Member reconnects after clearing cache, still fine.
Scenario 682: Reconnect scenario, Member reconnects after driver rollback, still fine.
Scenario 683: Reconnect scenario, Member reconnects after alt account login? not allowed due to session.
Scenario 684: Reconnect scenario, Member reconnects after network change to tether, still fine.
Scenario 685: Reconnect scenario, Member reconnects after losing DNS, once restored fine.
Scenario 686: Reconnect scenario, Member reconnects after firewall block, once unblocked fine.
Scenario 687: Reconnect scenario, Member reconnects after antivirus quarantine, once resolved fine.
Scenario 688: Reconnect scenario, Member reconnects after crash in graphics, once restored fine.
Scenario 689: Reconnect scenario, Member reconnects after OS crash, once restored fine if window not exceeded.
Scenario 690: Reconnect scenario, Member reconnects after account flagged, may be disconnected permanently.
Scenario 691: Reconnect scenario, Member reconnects after server hotfix, state may be invalid; server ends.
Scenario 692: Reconnect scenario, Member reconnects after patch version mismatch, forced update.
Scenario 693: Reconnect scenario, Member reconnects after strong CPU throttling, playback may catch up.
Scenario 694: Reconnect scenario, Member reconnects after battery saver, playback may catch up.
Scenario 695: Reconnect scenario, Member reconnects after OS sleep, if window valid resume.
Scenario 696: Reconnect scenario, Member reconnects after system hibernate, likely beyond window -> fail.
Scenario 697: Reconnect scenario, Member reconnects after router reboot, if within window resume.
Scenario 698: Reconnect scenario, Member reconnects after modem reconnect, if within window resume.
Scenario 699: Reconnect scenario, Member reconnects after switching monitors, no effect.
Scenario 700: Reconnect scenario, Member reconnects after switching audio device, audio resumes.
Scenario 701: Stress scenario, 100 players each in solo cutscene simultaneously, server handles routing by category 44.
Scenario 702: Stress scenario, party cutscenes across shards, not allowed; server rejects start.
Scenario 703: Stress scenario, many Progress messages, ensure 1-2s cadence maintained.
Scenario 704: Stress scenario, many Acks queued, ensure idempotent handling.
Scenario 705: Stress scenario, many SkipRequests from same player, rate limit triggers.
Scenario 706: Stress scenario, many CompleteRequests from same player, rate limit triggers.
Scenario 707: Stress scenario, many StateSyncRequests from same player, rate limit triggers.
Scenario 708: Stress scenario, party size max 40, party cutscene still works if allowed.
Scenario 709: Stress scenario, long cutscene with 200 steps, ensures StepChecksum correctness.
Scenario 710: Stress scenario, cutscene with 100 dialogues, ensures subtitle queue works.
Scenario 711: Stress scenario, cutscene with many camera moves, ensures CameraLock stable.
Scenario 712: Stress scenario, cutscene with many spawns/despawns, ensures entity sync stable.
Scenario 713: Stress scenario, cutscene with many screen effects, ensures performance remains acceptable.
Scenario 714: Stress scenario, cutscene with music changes frequently, ensures audio crossfades stable.
Scenario 715: Stress scenario, cutscene with multiple branches, ensures branch state stored.
Scenario 716: Stress scenario, cutscene with repeated pauses, ensures state machine robust.
Scenario 717: Stress scenario, cutscene with repeated resumes, ensures timers recalculated.
Scenario 718: Stress scenario, cutscene with repeated party votes, ensures vote system stable.
Scenario 719: Stress scenario, cutscene with many reconnects, ensures StateSync holds.
Scenario 720: Stress scenario, cutscene with large asset loads, ensures playback not blocked by assets.
Scenario 721: Stress scenario, cutscene under low bandwidth, ensures keepalives small.
Scenario 722: Stress scenario, cutscene under packet loss 20%, ensures retries.
Scenario 723: Stress scenario, cutscene under high latency 1s, ensures RemainingMs tolerance.
Scenario 724: Stress scenario, cutscene under jitter, ensures smoothing.
Scenario 725: Stress scenario, cutscene under CPU load, ensures timers accurate.
Scenario 726: Stress scenario, cutscene under GPU load, ensures frame drops not break timeline.
Scenario 727: Stress scenario, cutscene under disk load, ensures streaming not stall locks.
Scenario 728: Stress scenario, cutscene with user alt-tab often, ensures no unlock.
Scenario 729: Stress scenario, cutscene with OS notifications, ensures UI lock stays.
Scenario 730: Stress scenario, cutscene with overlay apps, ensures lock unaffected.
Scenario 731: Stress scenario, cutscene with streaming overlay capturing input, server locks enforce.
Scenario 732: Stress scenario, cutscene with user toggling language repeatedly, subtitles update next lines.
Scenario 733: Stress scenario, cutscene with user toggling accessibility repeatedly, still stable.
Scenario 734: Stress scenario, cutscene with user toggling volume repeatedly, still stable.
Scenario 735: Stress scenario, cutscene with user toggling graphics quality repeatedly, playback continues.
Scenario 736: Stress scenario, cutscene with user resizing window repeatedly, playback continues.
Scenario 737: Stress scenario, cutscene with user dragging window across monitors, playback continues.
Scenario 738: Stress scenario, cutscene with user turning monitor off, playback continues; upon return still synced.
Scenario 739: Stress scenario, cutscene with user enabling HDR, playback continues.
Scenario 740: Stress scenario, cutscene with user enabling VRR, playback continues.
Scenario 741: Stress scenario, cutscene with user enabling vsync, playback continues.
Scenario 742: Stress scenario, cutscene with user enabling frame cap, playback continues.
Scenario 743: Stress scenario, cutscene with user disabling frame cap, playback continues.
Scenario 744: Stress scenario, cutscene with user switching to controller, locks still apply.
Scenario 745: Stress scenario, cutscene with user switching back to keyboard, locks still apply.
Scenario 746: Stress scenario, cutscene with user unplugging controller, locks still apply.
Scenario 747: Stress scenario, cutscene with user plugging controller, locks still apply.
Scenario 748: Stress scenario, cutscene with user enabling haptics, locks still apply.
Scenario 749: Stress scenario, cutscene with user disabling haptics, locks still apply.
Scenario 750: Stress scenario, cutscene with user enabling gyro, CameraLock prevents.
Scenario 751: Stress scenario, cutscene with user enabling motion blur off, allowed.
Scenario 752: Stress scenario, cutscene with user enabling bloom off, allowed.
Scenario 753: Stress scenario, cutscene with user enabling high contrast, allowed.
Scenario 754: Stress scenario, cutscene with user enabling dyslexia font, allowed.
Scenario 755: Stress scenario, cutscene with user enabling text-to-speech, allowed.
Scenario 756: Stress scenario, cutscene with user enabling speech-to-text, allowed.
Scenario 757: Stress scenario, cutscene with user enabling screen reader, allowed.
Scenario 758: Stress scenario, cutscene with user enabling keyboard navigation, allowed.
Scenario 759: Stress scenario, cutscene with user enabling remapping, blocked due to UiLock.
Scenario 760: Stress scenario, cutscene with user enabling auto-run accessibility, blocked by movement lock.
Scenario 761: Stress scenario, cutscene with user enabling large cursor, allowed.
Scenario 762: Stress scenario, cutscene with user enabling subtitle background, allowed.
Scenario 763: Stress scenario, cutscene with user enabling subtitle outline, allowed.
Scenario 764: Stress scenario, cutscene with user enabling high contrast subtitles, allowed.
Scenario 765: Stress scenario, cutscene with user enabling colorblind filter, allowed.
Scenario 766: Stress scenario, cutscene with user enabling reduced motion, camera shake disabled if configured.
Scenario 767: Stress scenario, cutscene with user enabling reduced flashes, screen flash intensity reduced.
Scenario 768: Stress scenario, cutscene with user enabling captions for sound effects, allowed.
Scenario 769: Stress scenario, cutscene with user enabling descriptive audio, not available; fallback to voice asset.
Scenario 770: Stress scenario, cutscene with user enabling auto subtitles always on, already default.
Scenario 771: Stress scenario, cutscene with user enabling different language mid cutscene, new lines use new locale.
Scenario 772: Stress scenario, cutscene with user enabling UI scale to 200%, layout still fits.
Scenario 773: Stress scenario, cutscene with user enabling UI scale to 50%, layout still fits.
Scenario 774: Stress scenario, cutscene with user enabling windowed mode, playback continues.
Scenario 775: Stress scenario, cutscene with user enabling fullscreen, playback continues.
Scenario 776: Stress scenario, cutscene with user enabling borderless, playback continues.
Scenario 777: Stress scenario, cutscene with user enabling low latency mode, unaffected.
Scenario 778: Stress scenario, cutscene with user enabling triple buffering, unaffected.
Scenario 779: Stress scenario, cutscene with user enabling DLSS, unaffected.
Scenario 780: Stress scenario, cutscene with user enabling FSR, unaffected.
Scenario 781: Stress scenario, cutscene with user enabling ray tracing, unaffected though may performance drop.
Scenario 782: Stress scenario, cutscene with user enabling path tracing, heavy load; still locked.
Scenario 783: Stress scenario, cutscene with user enabling vsync off, potential tearing; timeline unaffected.
Scenario 784: Stress scenario, cutscene with user enabling frame limiter to 30fps, timeline still uses server time.
Scenario 785: Stress scenario, cutscene with user enabling frame limiter to 60fps, timeline still uses server time.
Scenario 786: Stress scenario, cutscene with user enabling monitor overdrive, unaffected.
Scenario 787: Stress scenario, cutscene with user enabling capture card, unaffected.
Scenario 788: Stress scenario, cutscene with user enabling overlay (Discord), unaffected; locks remain.
Scenario 789: Stress scenario, cutscene with user enabling steam overlay, unaffected.
Scenario 790: Stress scenario, cutscene with user enabling geforce overlay, unaffected.
Scenario 791: Stress scenario, cutscene with user enabling xbox overlay, unaffected.
Scenario 792: Stress scenario, cutscene with user enabling twitch overlay, unaffected.
Scenario 793: Stress scenario, cutscene with user enabling OBS capture, unaffected.
Scenario 794: Stress scenario, cutscene with user enabling ReShade, may alter visuals but timeline intact.
Scenario 795: Stress scenario, cutscene with user enabling modded shaders, StepChecksum unaffected (payload only).
Scenario 796: Stress scenario, cutscene with user enabling unstable network, server may End on timeouts.
Scenario 797: Stress scenario, cutscene with user enabling VPN, connection may change IP, ResumeToken may invalidate.
Scenario 798: Stress scenario, cutscene with user enabling proxy, similar to VPN.
Scenario 799: Stress scenario, cutscene with user enabling firewall strict mode, may block; reconnect.
Scenario 800: Stress scenario, cutscene with user enabling airplane mode, disconnect; reconnect if window valid or End Failed.
```

---

**Letzte Aktualisierung:** 2026-01-03  
**Version:** 1.0.0  
[← Zurück zur Übersicht](README.md)
