# 🎉 Event Messages (4600-4699)

**Kategorie:** 46  
**Range:** 4600-4699  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#ueberblick)
- [🧠 Datenmodell](#datenmodell)
- [🗺️ Discovery & Subscription](#discovery)
- [✅ Join/Leave/Contribution Regeln](#joinleave)
- [📈 Progress Updates & Phases](#progress)
- [🏆 Rewards & Claiming](#rewards)
- [🔄 Sync, Deltas & Revisioning](#sync)
- [🧱 DTOs / Interfaces](#dtos)
- [🧩 Enums / ErrorCodes / Flags](#enums)
- [⚙️ Regeln & Sicherheit](#regeln)
- [📩 Aktive Messages 4600–4699](#aktive)
- [🗑️ Obsolete Messages](#obsolete)
- [🧨 Edge Cases & Fehlerfälle](#edge-cases)
- [📎 Anhang](#anhang)

---

<a id="ueberblick"></a>

## 📋 Überblick

Scope umfasst dynamische World Events, Seasonal Events und instanzierte Event-Instanzen. Client entdeckt Events AoI-basiert, abonniert interessierende EventInstances, joint validiert, liefert Contributions (Damage/Heal/Objective), erhält Progress/Phase-Updates, Rewards und Claim-Flow. Server ist immer authoritative: EventInstanceId, Phase, Objectives, Progress, Reward-Eligibility werden serverseitig geführt und versioniert.

---

<a id="datenmodell"></a>

## 🧠 Datenmodell

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| EventId | ushort | Statische Event-Definition (Design-Daten) |
| EventInstanceId | ulong | Laufende Instanz pro Zone/Shard |
| ZoneId | ushort | Zone oder InstanceId wenn instanziert |
| Phase | byte | Aktuelle Phase (0-n) |
| PhaseRevision | uint | Phase-Revision erhöht bei Wechsel |
| ProgressPercent | float | 0-100% Aggregat |
| ProgressRevision | uint | Delta/Snapshot Version |
| Objectives | List<EventObjectiveDto> | Ziele der aktuellen Phase |
| Participants | List<EventParticipantDto> | Laufende Teilnehmer |
| ThrottleHintMs | int | Server-aufgelegtes Minimum-Intervall |
| ServerTimeMs | long | Zeitstempel für Sync |
| ContributionScore | int | Punkte pro Spieler |

EventInstances sind keyed per (EventId, EventInstanceId, ZoneId). Client speichert SubscriptionState mit letztem ProgressRevision und PhaseRevision, sowie ClientSequence zur Korrelation aller Requests.

---

<a id="discovery"></a>

## 🗺️ Discovery & Subscription

- Discovery erfolgt AoI-basiert: ZoneServer pusht `EventDiscoveryEvent` (4607) sobald Player in Reichweite (Zone/Shard).  
- Subscription erfolgt explizit: `EventSubscribeRequest` (4608) → `EventSubscribeResponse` (4609) → `EventSubscribedEvent` (4610) als best-effort Push mit Snapshot.  
- Unsubscribe: `EventUnsubscribeRequest` (4611) → `EventUnsubscribeResponse` (4612); Server kann zusätzlich `EventSubscriptionDroppedEvent` (4613) senden (Kick, AoI verlassen, Timeout).  
- Heartbeat für Subscriptions: `EventSubscriptionHeartbeat` (4635) → `EventSubscriptionAck` (4636) (alle 20s). Fehlt Ack dreimal → Drop.

---

<a id="joinleave"></a>

## ✅ Join/Leave/Contribution Regeln

- Join: `EventJoinRequest` (4614) erfordert authentifizierten Spieler, offene Phase, Slot verfügbar, Position im Event-AoI. Antwort `EventJoinResponse` (4615).  
- Leave: `EventLeaveRequest` (4616) → `EventLeaveResponse` (4617); Server sendet `EventParticipantUpdateEvent` (4618) an übrige Teilnehmer.  
- Contribution: jede Aktion (Damage/Heal/Objective) wird serverseitig validiert, auf Anti-Leech geprüft (Minimum-Aktion pro Phase, Anti-AFK), und als `EventContributionUpdateEvent` (4629) aggregiert.  
- Atomicity: Join/Leave sind transaktional pro EventInstanceId; Doppel-Join abgewiesen mit `ErrorCode=ALREADY_JOINED`.  
- Rights: Kein Client kann Phase/Progress setzen; nur Beiträge melden via Combat/Objective Systeme → serverseitige Aggregation.

---

<a id="progress"></a>

## 📈 Progress Updates & Phases

- High-frequency Deltas: `EventProgressDeltaEvent` (4620) mit ProgressRevision, ServerTimeMs, Delta für Objectives/Percent. Throttle serverseitig (min 250ms).  
- Snapshots: `EventProgressSnapshot` (4621) vollständige EventInstance-Daten; gesendet bei Subscribe, PhaseChange, Reconnect.  
- Phase-Wechsel: `EventPhaseChangedEvent` (4619) mit PhaseRevision, neuer Phase, erwarteten Objectives; Clients invalidieren PendingActions, reload UI.  
- Objective-Details: `EventObjectiveUpdateEvent` (4622) liefert Status (Started/Completed/Failed) für einzelne Objectives.  
- Mismatch: Bei Revision-Abweichung sendet Server `EventRevisionMismatch` (4631) und fordert `EventStateSyncRequest` (4623) vom Client an.

---

<a id="rewards"></a>

## 🏆 Rewards & Claiming

- Reward Eligibility: Server prüft ContributionScore ≥ Threshold, aktive Teilnahme (kein AFK-Flag), PhaseCompleted=true.  
- Availability: `EventRewardAvailableEvent` (4625) kündigt claimbare Rewards an; enthält LootTableId, Currency, Exp.  
- Claim: `EventRewardClaimRequest` (4626) → `EventRewardClaimResponse` (4627); bei Erfolg folgt `EventRewardDeliveredEvent` (4628) plus integrative Messages aus Loot/Economy (`LootItemResult`, `CurrencyUpdate`).  
- Anti-Exploit: idempotent per RewardGrantId; doppelte Claims liefern `ErrorCode=ALREADY_CLAIMED`.

---

<a id="sync"></a>

## 🔄 Sync, Deltas & Revisioning

- Snapshots tragen `SnapshotRevision` (ProgressRevision) und `PhaseRevision`.  
- Deltas nur anwenden wenn `DeltaRevision == CurrentRevision + 1`, sonst SyncRequest.  
- Reconnect: `EventStateSyncRequest` (4623) mit letztem bekannten Revision; Response `EventStateSyncResponse` (4624) sendet Snapshot und optional verpasste Deltas.  
- Retry/Rate Limit: Client max 3 SyncRequests pro 10s, sonst `EventThrottleNotice` (4630).

---

<a id="dtos"></a>

## 🧱 DTOs / Interfaces

```csharp
[MessagePackObject]
public class EventObjectiveDto
{
    [Key(0)] public ushort EventId { get; set; }
    [Key(1)] public ulong EventInstanceId { get; set; }
    [Key(2)] public ushort ObjectiveId { get; set; }
    [Key(3)] public string Description { get; set; } = string.Empty;
    [Key(4)] public EventObjectiveState State { get; set; }
    [Key(5)] public float ProgressPercent { get; set; }
    [Key(6)] public uint Revision { get; set; }
}

[MessagePackObject]
public class EventParticipantDto
{
    [Key(0)] public long CharacterId { get; set; }
    [Key(1)] public string Name { get; set; } = string.Empty;
    [Key(2)] public int Level { get; set; }
    [Key(3)] public int ContributionScore { get; set; }
    [Key(4)] public bool IsActive { get; set; }
    [Key(5)] public bool EligibleForRewards { get; set; }
    [Key(6)] public uint LastContributionRevision { get; set; }
}
```

---

<a id="enums"></a>

## 🧩 Enums / ErrorCodes / Flags

```csharp
public enum EventObjectiveState : byte
{
    Pending = 0,
    Active = 1,
    Completed = 2,
    Failed = 3
}

public enum EventErrorCode
{
    NONE,
    NOT_FOUND,
    NOT_IN_AOI,
    CLOSED,
    ALREADY_JOINED,
    NOT_SUBSCRIBED,
    NOT_PARTICIPANT,
    REWARD_NOT_AVAILABLE,
    ALREADY_CLAIMED,
    THROTTLED,
    REVISION_MISMATCH
}
```

Flags für Client: `IsSubscribed`, `IsParticipant`, `HasPendingReward`, `NeedsResync`.

---

<a id="regeln"></a>

## ⚙️ Regeln & Sicherheit

- Authentifizierung zwingend für alle Requests (Join, Leave, Subscribe, Claim).  
- Rate Limits: Subscribe/Unsubscribe max 5/min; Join/Leave max 3/min pro EventInstanceId.  
- Anti-Cheat: Contributions akzeptieren nur, wenn Aktionen aus legitimen Combat/Objective Pipelines stammen; Cross-validate Position/ZoneId.  
- Idempotenz: Jede Response enthält `RequestId` (uint) aus Request; Server speichert letzte 20 RequestIds pro Client.  
- Correlation: Messages mit ClientSequence; Responses spiegeln Sequence und RequestId.  
- Transport: MessagePack, Key-Order stabil, keine optionalen Keys ohne Default.

---

<a id="aktive"></a>

## 📩 Aktive Messages 4600–4699

### EventStart (4600)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmal pro EventInstance (Start-Broadcast)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server kündigt Start einer EventInstance an. Enthält minimale Meta-Daten für UI-Banner. Dient als Trigger, um EventList UI zu aktualisieren und Subscriptions anzubieten.

### Im Scope ✅
- Ankündigung neuer Instanz
- Basis-Metadaten (EventId, Instance, Zone)
- Phase 0 Initialisierung

### Nicht im Scope ❌
- Vollständige Objectives (werden im Snapshot bereitgestellt)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | MessageType.EventStart | Ja |
| EventId | ushort | Statischer Event-Schlüssel | Ja |
| EventInstanceId | ulong | Laufende Instanz | Ja |
| ZoneId | ushort | Zone/Shard | Ja |
| Phase | byte | Startphase | Ja |
| ServerTimeMs | long | Zeitstempel | Ja |

### Erwartete Response
- Keine direkte Response (Broadcast); Clients können `EventSubscribeRequest` senden.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EventStart)]
public class EventStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EventStart;
    [Key(1)] public ushort EventId { get; init; }
    [Key(2)] public ulong EventInstanceId { get; init; }
    [Key(3)] public ushort ZoneId { get; init; }
    [Key(4)] public byte Phase { get; init; }
    [Key(5)] public long ServerTimeMs { get; init; }
}
```

### Server-Verhalten
- Sendet Broadcast an alle Spieler in relevanter Zone/Shard.  
- Registriert EventInstance in Registry; beginnt Phase 0 Timer.  
- Fügt Event in Discovery-Cache für neue Joiner ein.

### Client-Verhalten
- Zeigt UI-Banner.  
- Optional Auto-Subscribe nach Client-Setting.  
- Speichert EventInstanceId für spätere Requests.

### Flow-Diagramm
```
Server                Clients in Zone
  │                         │
  │  EventStart (4600)      │
  │────────────────────────►│
  │                         │
```

### Beispiel Payloads
```csharp
var start = new EventStart
{
    EventId = 120,
    EventInstanceId = 887766,
    ZoneId = 1001,
    Phase = 0,
    ServerTimeMs = NowMs()
};
```

### Error Codes
- Keine (Broadcast).

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventProgress | 4602 | Folge-Progress |
| EventPhaseChangedEvent | 4619 | Phasewechsel |
| EventEnd | 4601 | Abschluss |

---

### EventEnd (4601)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Signalisiert das Ende einer EventInstance, erfolgreich oder fehlgeschlagen. Stellt sicher, dass Clients ihre UI schließen und Subscriptions entfernen.

### Im Scope ✅
- Endstatus (Success/Fail/Timeout)
- PhaseFinal
- Reward-Hinweis

### Nicht im Scope ❌
- Reward-Ausgabe (separate Messages)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | Event | Ja |
| EventInstanceId | ulong | Instanz | Ja |
| Result | string | "Success" / "Fail" / "Timeout" | Ja |
| FinalPhase | byte | Letzte Phase | Ja |
| ServerTimeMs | long | Zeitstempel | Ja |

### Erwartete Response
- Keine; Client sendet ggf. `EventStateSyncRequest` bei fehlenden Rewards.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EventEnd)]
public class EventEnd : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EventEnd;
    [Key(1)] public ushort EventId { get; init; }
    [Key(2)] public ulong EventInstanceId { get; init; }
    [Key(3)] public string Result { get; init; } = string.Empty;
    [Key(4)] public byte FinalPhase { get; init; }
    [Key(5)] public long ServerTimeMs { get; init; }
}
```

### Server-Verhalten
- Broadcast an Subscribed + Teilnehmer.  
- Markiert EventInstance als geschlossen.  
- Triggert Reward-Berechnung und sendet `EventRewardAvailableEvent` an Eligible Players.

### Client-Verhalten
- Entfernt UI-Overlay.  
- Zeigt Result-Toast.  
- Erwartet RewardAvailable oder schickt SyncRequest.

### Flow-Diagramm
```
Server                        Client
  │                             │
  │  EventEnd (4601)            │
  │────────────────────────────►│
  │                             │
  │  EventRewardAvailable?      │
  │────────────────────────────►│
```

### Error Codes
- Keine (Broadcast).

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventRewardAvailableEvent | 4625 | Reward Info |
| EventProgress | 4602 | Laufender Fortschritt |

---

### EventProgress (4602)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (Throttle 250ms)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Legacy-Aggregat für Progress; bleibt als Aggregat vorhanden, wird jedoch durch Delta/Snapshot ersetzt. Enthält Gesamtsumme percent, phasenunabhängig.

### Im Scope ✅
- Gesamtfortschritt Prozent
- EventId/Instance
- Revision

### Nicht im Scope ❌
- Objective-spezifische Details (DeltaEvent nutzen)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| ProgressPercent | float | | Ja |
| ProgressRevision | uint | | Ja |
| ServerTimeMs | long | | Ja |

### Erwartete Response
- Keine; Client nutzt für UI-Update.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EventProgress)]
public class EventProgress : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EventProgress;
    [Key(1)] public ushort EventId { get; init; }
    [Key(2)] public ulong EventInstanceId { get; init; }
    [Key(3)] public float ProgressPercent { get; init; }
    [Key(4)] public uint ProgressRevision { get; init; }
    [Key(5)] public long ServerTimeMs { get; init; }
}
```

### Server-Verhalten
- Aggregiert intern; sendet wenn Revision erhöht.  
- Überspringt Send wenn Client nicht subscribed.

### Client-Verhalten
- UI-Balken aktualisieren; falls Revision Gap → SyncRequest.

### Flow-Diagramm
```
Server                     Client
  │                          │
  │ EventProgress            │
  │─────────────────────────►│
  │                          │
```

### Error Codes
- Keine (Broadcast).

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventProgressDeltaEvent | 4620 | Feingranular |
| EventProgressSnapshot | 4621 | Vollsync |

---

### SeasonalStart (4603)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Startet einen Seasonal Event Block (z. B. Winterfest). Informiert über Saison-IDs und Startzeiten.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SeasonId | ushort | Saison-Schlüssel | Ja |
| Name | string | Titel | Ja |
| StartTime | long | Unix ms | Ja |
| EndTime | long | Unix ms | Ja |
| Theme | string | Farb-/Asset-Thema | Nein |

### Erwartete Response
- Keine.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| SeasonalEnd | 4604 | Ende |
| EventSeasonInfoRequest | 4637 | Detail Pull |

---

### SeasonalEnd (4604)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Markiert Ende einer Saison. Clients sollen saisonale UI schließen und laufende Events der Saison deaktivieren.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SeasonId | ushort | Saison | Ja |
| EndTime | long | Unix ms | Ja |
| Reason | string | Optional (Maintenance) | Nein |

### Erwartete Response
- Keine.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| SeasonalStart | 4603 | Start |
| EventSeasonInfoResponse | 4638 | Infos |

---

### EventListRequest (4605)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (UI open)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Liste aller sichtbaren Events (AoI + global) inkl. minimaler Metadaten. Unterstützt Filter (Season, Zone, Participation).

### Im Scope ✅
- Filter: ZoneId, SeasonId, OnlyOpen
- Pagination (cursor)
- Correlation via RequestId

### Nicht im Scope ❌
- Vollständige Snapshots (EventProgressSnapshot nutzen)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Korrelation | Ja |
| ZoneId | ushort? | Filter | Nein |
| SeasonId | ushort? | Filter | Nein |
| OnlyOpen | bool | Nur aktive? | Ja |
| Cursor | string? | Pagination | Nein |

### Erwartete Response
- `EventListResponse` (4606)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EventListRequest)]
public class EventListRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.EventListRequest;
    [Key(1)] public uint RequestId { get; set; }
    [Key(2)] public ushort? ZoneId { get; set; }
    [Key(3)] public ushort? SeasonId { get; set; }
    [Key(4)] public bool OnlyOpen { get; set; } = true;
    [Key(5)] public string? Cursor { get; set; }
}
```

### Server-Verhalten
- Validiert Auth, Rate Limit.  
- Liefert Liste aus Registry gefiltert.  
- Füllt Cursor falls mehr als 50 Einträge.

### Client-Verhalten
- Zeigt Liste; bei Cursor sendet Folge-Request.  
- Setzt UI-Filter zurück, wenn Server Filter trimmt.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │ EventListRequest (4605)      │
  │─────────────────────────────►│
  │                              │
  │ EventListResponse (4606)     │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var listReq = new EventListRequest
{
    RequestId = 1,
    ZoneId = 1001,
    OnlyOpen = true
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| THROTTLED | Zu viele Requests |
| NOT_FOUND | Ungültiger Cursor |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventListResponse | 4606 | Response |
| EventDiscoveryEvent | 4607 | Push |

---

### EventListResponse (4606)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf EventListRequest. Enthält eine Liste von EventSummary DTOs.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Echo | Ja |
| Success | bool | Status | Ja |
| ErrorCode | string? | Bei Fehler | Nein |
| Events | List<EventSummaryDto> | Auflistung | Ja |
| Cursor | string? | Nächste Seite | Nein |

### Beispiel Payloads
```csharp
var resp = new EventListResponse
{
    RequestId = 1,
    Success = true,
    Events = new()
    {
        new EventSummaryDto
        {
            EventId = 120,
            EventInstanceId = 887766,
            ZoneId = 1001,
            Phase = 0,
            ProgressPercent = 3.4f,
            SeasonId = 7,
            EndsAtMs = NowMs() + 600000
        }
    }
};
```

### Server-Verhalten
- Liefert maximal 50 Einträge.  
- Erfolgsfall Success=true; Fehler THROTTLED/NOT_FOUND.

### Client-Verhalten
- Erstellt UI-Einträge; bei Cursor → Lazy Load.  
- Speichert RequestId für Logs.

### Erwartete Response
- Keine (Response selbst).

### Error Codes
| Code | Bedeutung |
|------|-----------|
| THROTTLED | Rate Limit |
| NOT_FOUND | Cursor ungültig |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventListRequest | 4605 | Request |
| EventDiscoveryEvent | 4607 | Push |

---

### EventDiscoveryEvent (4607)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Push sobald Spieler AoI eines Events betritt. Dient als Proximity-Signal; enthält Kurzinfos.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| ZoneId | ushort | | Ja |
| Distance | float | Luftlinie zum Fokus | Ja |
| Recommended | bool | UI-Hinweis | Ja |

### Erwartete Response
- Keine; Client kann `EventSubscribeRequest` senden.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventSubscribeRequest | 4608 | Folge |
| EventSubscriptionDroppedEvent | 4613 | AoI verlassen |

---

### EventSubscribeRequest (4608)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Subscription auf EventInstance an. Liefert Response + optional SubscribedEvent Push.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Korrelation | Ja |
| EventId | ushort | Ziel | Ja |
| EventInstanceId | ulong | Instanz | Ja |
| ClientSequence | uint | Sequenz | Ja |

### Erwartete Response
- `EventSubscribeResponse` (4609)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventSubscribeResponse | 4609 | Response |
| EventSubscribedEvent | 4610 | Snapshot Push |

---

### EventSubscribeResponse (4609)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf SubscribeRequest. Bei Erfolg wird Snapshot via SubscribedEvent gepusht.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Echo | Ja |
| Success | bool | Status | Ja |
| ErrorCode | string? | Bei Fehler | Nein |
| ClientSequence | uint | Echo | Ja |
| ThrottleHintMs | int | Mindestintervall | Ja |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventSubscribedEvent | 4610 | Snapshot |
| EventSubscriptionDroppedEvent | 4613 | Drop |

---

### EventSubscribedEvent (4610)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel (nach Subscribe)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Push nach erfolgreicher Subscription. Enthält Snapshot (Progress, Objectives, Participants).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| Snapshot | EventSnapshotDto | Vollsync | Ja |
| ProgressRevision | uint | Revision | Ja |
| PhaseRevision | uint | Revision | Ja |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventProgressSnapshot | 4621 | Snapshot |
| EventProgressDeltaEvent | 4620 | Deltas |

---

### EventUnsubscribeRequest (4611)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Beendet Subscription. Entfernt Pending Deltas.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |

### Erwartete Response
- `EventUnsubscribeResponse` (4612)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventSubscriptionDroppedEvent | 4613 | Server Drop |

---

### EventUnsubscribeResponse (4612)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf Unsubscribe. Success=false wenn nicht subscribed.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Echo | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventUnsubscribeRequest | 4611 | Request |
| EventSubscriptionDroppedEvent | 4613 | Drop |

---

### EventSubscriptionDroppedEvent (4613)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Informiert Client, dass Subscription serverseitig beendet wurde (AoI verlassen, Timeout, Kick).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| Reason | string | "AoI", "Timeout", "Admin" | Ja |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventSubscribeRequest | 4608 | Re-Subscribe |

---

### EventJoinRequest (4614)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert aktive Teilnahme am Event an. Überprüft Slot, Level, Position, Party/Guild Regeln.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| ClientSequence | uint | | Ja |
| Position | Vector2Dto | Letzte bekannte Position | Ja |

### Erwartete Response
- `EventJoinResponse` (4615)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventParticipantUpdateEvent | 4618 | Broadcast |

---

### EventJoinResponse (4615)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf JoinRequest. Bei Erfolg wird Participant-Liste aktualisiert.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Echo | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |
| Participant | EventParticipantDto? | Bei Erfolg | Nein |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventParticipantUpdateEvent | 4618 | Broadcast |
| EventProgressSnapshot | 4621 | Sync |

---

### EventLeaveRequest (4616)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Verlässt EventInstance freiwillig.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| Reason | string | "Leave" / "Logout" | Ja |

### Erwartete Response
- `EventLeaveResponse` (4617)

---

### EventLeaveResponse (4617)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf LeaveRequest. Informiert über Erfolg und aktualisierte Teilnahmeflags.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Echo | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| EventParticipantUpdateEvent | 4618 | Broadcast |

---

### EventParticipantUpdateEvent (4618)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Broadcast der aktuellen Teilnehmerliste oder Einzeländerung (Join/Leave/Kick).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| Participants | List<EventParticipantDto> | | Ja |
| Revision | uint | Teilnehmer-Revision | Ja |

---

### EventPhaseChangedEvent (4619)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Signalisiert Phasewechsel. Enthält neue Phase, Objectives, Timer.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| Phase | byte | Neue Phase | Ja |
| PhaseRevision | uint | | Ja |
| Objectives | List<EventObjectiveDto> | | Ja |
| PhaseEndsAtMs | long | | Ja |

---

### EventProgressDeltaEvent (4620)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (250ms)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Feingranularer Fortschritts-Delta. Enthält Revision und nur veränderte Objectives.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| ProgressRevision | uint | | Ja |
| DeltaPercent | float | Änderung | Ja |
| ObjectiveDeltas | List<EventObjectiveDto> | Optionale Deltas | Nein |
| ServerTimeMs | long | | Ja |

---

### EventProgressSnapshot (4621)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Vollständiger Snapshot für Resync. Wird bei Subscribe, Reconnect, PhaseChange gesendet.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| ProgressPercent | float | | Ja |
| ProgressRevision | uint | | Ja |
| Phase | byte | | Ja |
| PhaseRevision | uint | | Ja |
| Objectives | List<EventObjectiveDto> | | Ja |
| Participants | List<EventParticipantDto> | | Ja |
| ServerTimeMs | long | | Ja |

---

### EventObjectiveUpdateEvent (4622)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Ändert Status einzelner Objectives (Start/Complete/Fail). UI kann gezielt aktualisieren.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| Objectives | List<EventObjectiveDto> | | Ja |
| ProgressRevision | uint | | Ja |

---

### EventStateSyncRequest (4623)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf (Mismatch/Rejoin)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Snapshot + fehlende Deltas an, wenn Revision Gap erkannt oder nach Reconnect.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| KnownProgressRevision | uint | | Ja |
| KnownPhaseRevision | uint | | Ja |

### Erwartete Response
- `EventStateSyncResponse` (4624)

---

### EventStateSyncResponse (4624)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwortet mit Snapshot und optionalen Deltas (für letzte Revisionen).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |
| Snapshot | EventSnapshotDto? | | Nein |
| MissingDeltas | List<EventProgressDeltaEvent>? | | Nein |

---

### EventRewardAvailableEvent (4625)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Informiert Spieler, dass Rewards claimbar sind. Enthält RewardGrantId für Idempotenz.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| RewardGrantId | Guid | Idempotenz | Ja |
| LootTableId | int | Loot | Ja |
| Currency | int | Soft Currency | Ja |
| Exp | int | Erfahrung | Ja |
| ExpiresAtMs | long | Claim-Deadline | Ja |

---

### EventRewardClaimRequest (4626)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Claimt Rewards für RewardGrantId. Verlangt Participation + Eligibility.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| RewardGrantId | Guid | | Ja |

### Erwartete Response
- `EventRewardClaimResponse` (4627)

---

### EventRewardClaimResponse (4627)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwortet mit Status zum Claim. Bei Erfolg triggert `EventRewardDeliveredEvent`.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Echo | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |
| RewardGrantId | Guid | | Ja |

---

### EventRewardDeliveredEvent (4628)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt Zustellung der Rewards. Enthält referenzielle IDs für Loot/Economy Deltas.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RewardGrantId | Guid | | Ja |
| DeliveryTokens | string | Referenzen auf Loot/Economy Messages | Ja |
| ServerTimeMs | long | | Ja |

---

### EventContributionUpdateEvent (4629)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Aggregiert ContributionScore pro Teilnehmer (Damage/Heal/Objective). Dient UI und Anti-Leech.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| ContributionRevision | uint | | Ja |
| Participants | List<EventParticipantDto> | | Ja |
| ServerTimeMs | long | | Ja |

---

### EventThrottleNotice (4630)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Hinweis, dass Client Requests zu schnell sendet (Subscribe/Sync). Enthält RetryAfter.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Context | string | "Subscribe" / "Sync" | Ja |
| RetryAfterMs | int | | Ja |

---

### EventRevisionMismatch (4631)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Benachrichtigt Client über Revision Lücke; fordert SyncRequest.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| ServerProgressRevision | uint | | Ja |
| ServerPhaseRevision | uint | | Ja |

---

### EventRejoinRequest (4632)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Versucht Rejoin nach Disconnect ohne erneutes Join, nutzt frühere Participant-Session.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| LastParticipationRevision | uint | | Ja |

### Erwartete Response
- `EventRejoinResponse` (4633)

---

### EventRejoinResponse (4633)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwortet auf Rejoin. Bei Erfolg: Participant bleibt registriert, Snapshot folgt.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |

---

### EventInstanceDisbandEvent (4634)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Teilt mit, dass EventInstance vorzeitig abgebrochen wurde (Admin, Stability).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| Reason | string | "Admin", "Stability", "Duplicate" | Ja |

---

### EventSubscriptionHeartbeat (4635)

**Richtung:** 📤 Client → Server  
**Frequenz:** Alle 20s  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Keepalive für Subscription, damit Server Teilnahme weiter sendet.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| ClientSequence | uint | | Ja |

### Erwartete Response
- `EventSubscriptionAck` (4636)

---

### EventSubscriptionAck (4636)

**Richtung:** 📥 Server → Client  
**Frequenz:** Alle 20s  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Ack für SubscriptionHeartbeat. Enthält nächste Heartbeat-Deadline.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| NextHeartbeatDueMs | long | | Ja |

---

### EventSeasonInfoRequest (4637)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fragt Detailinfos zu Season an (Belohnungen, Kalender).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| SeasonId | ushort | | Ja |

### Erwartete Response
- `EventSeasonInfoResponse` (4638)

---

### EventSeasonInfoResponse (4638)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwortet mit Detailinfos, inkl. laufender Events der Season.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |
| Season | SeasonInfoDto? | | Nein |

---

### EventMilestoneUnlockedEvent (4639)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Benachrichtigt über globales/instanzweites Milestone (z. B. 50% Fortschritt).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| MilestoneId | ushort | | Ja |
| Description | string | | Ja |

---

### EventPhasePreviewRequest (4640)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fragt Preview der nächsten Phase (Mechaniken, Timer) an, falls freigegeben.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |

### Erwartete Response
- `EventPhasePreviewResponse` (4641)

---

### EventPhasePreviewResponse (4641)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Liefert Preview-Daten (Beschreibung, benötigte Rollen, Buff-Hinweise).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |
| PhaseInfo | string | Textuelle Beschreibung | Nein |

---

### EventParticipationSummaryRequest (4642)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (UI)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Zusammenfassung der eigenen Beiträge und Ranking an.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |

### Erwartete Response
- `EventParticipationSummaryResponse` (4643)

---

### EventParticipationSummaryResponse (4643)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwortet mit persönlicher Contribution, Rang, Reward-Einschätzung.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |
| ContributionScore | int | | Ja |
| Rank | int | | Ja |
| Eligible | bool | | Ja |

---

### EventAdminCommand (4644)

**Richtung:** 📤 Client → Server (GM)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
GM-Tool-Befehl für Events (Start/Stop/ForcePhase). Nur mit GM-Rechten.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| Command | string | "force_phase", "close" | Ja |
| EventId | ushort | | Ja |
| EventInstanceId | ulong | | Ja |
| Parameter | string? | Zusatz | Nein |

### Erwartete Response
- `EventAdminCommandResponse` (4645)

---

### EventAdminCommandResponse (4645)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwortet auf GM-Befehle. Echo der Aktion und Ergebnis.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | | Ja |
| Success | bool | | Ja |
| ErrorCode | string? | | Nein |
| Message | string? | | Nein |

---

<a id="obsolete"></a>

## 🗑️ Obsolete Messages

Aktuell keine Messages in 4600-4699 als obsolet markiert. Frühere Legacy-Progress-Aggregate bleiben aktiv, werden aber mit Delta/Snapshot kombiniert.

---

<a id="edge-cases"></a>

## 🧨 Edge Cases & Fehlerfälle

- Subscribe ohne AoI → Error NOT_IN_AOI.  
- Join während Phase=End → Error CLOSED.  
- Revision Gap >5 → EventRevisionMismatch → Client sendet SyncRequest.  
- RewardClaim nach Ablauf → Error REWARD_NOT_AVAILABLE.  
- Heartbeat verpasst → SubscriptionDropped mit Reason "Timeout".  
- Party-Lead Join erzwingt Party-Members? Server validiert individuell; keine implizite Join-Propagation.  
- InstanceDisband während Claim → ClaimResponse Error CLOSED.  
- Contribution Exploit (Macro Spam) → Rate Limit pro Skill, Anti-Leech Flag → Eligibility=false.

---

<a id="anhang"></a>

## 📎 Anhang

### MessageType Enum Updates

```csharp
    EventListRequest = 4605,
    EventListResponse = 4606,
    EventDiscoveryEvent = 4607,
    EventSubscribeRequest = 4608,
    EventSubscribeResponse = 4609,
    EventSubscribedEvent = 4610,
    EventUnsubscribeRequest = 4611,
    EventUnsubscribeResponse = 4612,
    EventSubscriptionDroppedEvent = 4613,
    EventJoinRequest = 4614,
    EventJoinResponse = 4615,
    EventLeaveRequest = 4616,
    EventLeaveResponse = 4617,
    EventParticipantUpdateEvent = 4618,
    EventPhaseChangedEvent = 4619,
    EventProgressDeltaEvent = 4620,
    EventProgressSnapshot = 4621,
    EventObjectiveUpdateEvent = 4622,
    EventStateSyncRequest = 4623,
    EventStateSyncResponse = 4624,
    EventRewardAvailableEvent = 4625,
    EventRewardClaimRequest = 4626,
    EventRewardClaimResponse = 4627,
    EventRewardDeliveredEvent = 4628,
    EventContributionUpdateEvent = 4629,
    EventThrottleNotice = 4630,
    EventRevisionMismatch = 4631,
    EventRejoinRequest = 4632,
    EventRejoinResponse = 4633,
    EventInstanceDisbandEvent = 4634,
    EventSubscriptionHeartbeat = 4635,
    EventSubscriptionAck = 4636,
    EventSeasonInfoRequest = 4637,
    EventSeasonInfoResponse = 4638,
    EventMilestoneUnlockedEvent = 4639,
    EventPhasePreviewRequest = 4640,
    EventPhasePreviewResponse = 4641,
    EventParticipationSummaryRequest = 4642,
    EventParticipationSummaryResponse = 4643,
    EventAdminCommand = 4644,
    EventAdminCommandResponse = 4645,
```

### Integrationshinweise

- Quest (1000): EventObjectives können QuestProgress triggern.  
- Zone (01): AoI und Shard-Wechsel invalidieren Subscription.  
- Instance (24): EventInstanceId kann InstanceId sein; Progress bleibt innerhalb Instanz.  
- Loot (31) & Economy (37): Rewards liefern Folge-Deltas.  
- Party/Guild (07/08): Keine automatische Join-Weitergabe, aber UI-Broadcasts empfohlen.  
- Notifications (43): Toasts für Milestones/PhaseChange.

---

Source: docs/03-messages/46-event.md
