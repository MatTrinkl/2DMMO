# 🪲 Debug Messages (4900-4999)

**Kategorie:** 49  
**Range:** 4900-4999  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
  - [DebugSession](#debugsession)
  - [DebugCommand](#debugcommand-1)
  - [TraceSpan](#tracespan)
  - [MetricsSnapshot](#metricssnapshot)
  - [DebugToggleState](#debugtogglestate)
  - [DebugLogStream](#debuglogstream)
- [🔒 Zugriff & Safety-Gates](#-zugriff--safety-gates)
- [🧾 Logging & Tracing](#-logging--tracing)
- [📈 Metrics & Profiling](#-metrics--profiling)
- [🧰 Runtime Toggles](#-runtime-toggles)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
  - [Gemeinsame Felder](#gemeinsame-felder)
  - [Client DTOs](#client-dtos)
  - [Server DTOs](#server-dtos)
  - [Schemas](#schemas)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
  - [DebugAccessLevel](#debugaccesslevel)
  - [DebugCommandName](#debugcommandname)
  - [DebugErrorCodes](#debugerrorcodes)
  - [DebugLogCategory](#debuglogcategory)
  - [DebugToggleKey](#debugtogglekey)
- [⚙️ Regeln & Sicherheit](#️-regeln--sicherheit)
  - [Rate Limits](#rate-limits)
  - [Audit & No-Leak Policy](#audit--no-leak-policy)
  - [Sandboxing](#sandboxing)
  - [Anti-Abuse](#anti-abuse)
- [📩 Aktive Messages 4900–4999](#-aktive-messages-49004999)
  - [DebugCommand (4900)](#debugcommand-4900)
  - [DebugResponse (4901)](#debugresponse-4901)
  - [DebugLog (4902)](#debuglog-4902)
  - [DebugTeleport (4903)](#debugteleport-4903)
  - [DebugSpawn (4904)](#debugspawn-4904)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang](#-anhang)
  - [MessageType Enum Updates](#messagetype-enum-updates)
  - [Flows (ASCII)](#flows-ascii)
  - [Validierungsmatrix](#validierungsmatrix)
  - [Beispiel-Testfälle](#beispiel-testfälle)
  - [Payload-Schemas im Detail](#payload-schemas-im-detail)

---

## 📋 Überblick

Debug-Nachrichten sind Diagnose- und DevTool-Messages, die niemals von Endnutzer-Clients genutzt werden. Sie sind:

- standardmäßig deaktiviert in Produktions-Builds,
- nur verfügbar für Admin/GM/DevBuild/Allowlist,
- strikt auditierbar (jede Aktion hinterlässt einen Audit-Eintrag),
- vollständig korreliert (RequestId/SessionId/TraceId),
- sicherheitshärtet (Rate Limits, Sanitizer, Sandbox).

**Scope**

- Lesen von Laufzeit-Metriken, Logs, TraceSpans
- Aktivieren von visuellen Overlays (Hitbox, NavMesh, NetStats)
- Ausführen begrenzter Sandbox-Kommandos (Teleport, Spawn Dummies)
- Starten/Beenden/Verlängern von DebugSessions

**Out-of-Scope**

- Economy-Eingriffe, Balance-Änderungen, Persistente DB-Schreibungen
- Player-versus-Player Einfluss (keine Buffs/Schaden/Heilung)
- Geheimnis-Transport (Tokens, Passwörter)

**Hard Rules**

1. Jeder Client→Server Request hat eine Response (`DebugResponse 4901`) mit identischem `RequestId`.
2. Alle Streams (Logs, Metrics) sind an eine `DebugSession` gebunden.
3. Debug ist in Prod default OFF; nur gezielt aktivierbar.
4. No-Leak: Sanitizer entfernt/hasht sensitive Felder.
5. Safety-Gates für alle Kommandos mit Seiteneffekten.

---

## 🧠 Datenmodell

### DebugSession

- `SessionId (Guid)` – serverseitig generiert, unique pro Account+Device+Build.
- `Requester` – { AccountId (long), CharacterId (long?), DeviceId (hashed) }.
- `AccessLevel (DebugAccessLevel)` – None/DevBuild/GM/Admin.
- `Capabilities (string[])` – erlaubte Command-Klassen (z. B. `metrics.read`, `logs.stream`, `toggle.write`, `sandbox.teleport`).
- `TTL (int seconds)` – Standard 900s; verlängerbar über `session.extend`.
- `State` – Active, Expired, Revoked, Closed.
- `ResumeToken (Guid)` – erlaubt Rejoin nach Verbindungsabbruch binnen TTL.
- `CreatedAt / ExpiresAt (long ms)` – Zeitstempel.
- `AuditHash (string)` – SHA256 über Session-Setup-Params für Audit-Unveränderbarkeit.

### DebugCommand

- `RequestId (ulong)` – monoton pro Session.
- `CommandName (string)` – siehe [DebugCommandName](#debugcommandname).
- `Args (Map<string, object>)` – strikt validiert gegen Schema.
- `RequiresDevBuild (bool)` – serverseitig; nicht von Client manipulierbar.
- `RequiresRole (DebugAccessLevel)` – serverseitig; mindestens GM/Admin für Seiteneffekt-Kommandos.
- `CorrelationId (Guid)` – optionaler Trace Identifier.

### TraceSpan

- `TraceId (Guid)` – Kette über mehrere Spans/Services.
- `SpanId (Guid)` – spezifischer Span.
- `ParentSpanId (Guid?)` – optional.
- `Name (string)` – z. B. `DebugMetricsHandler`.
- `Start (long ms)`, `DurationMs (int)`.
- `Status (string)` – Ok/Error/Timeout.
- `Attributes (Dictionary<string,string>)` – sanitized, keine Secrets.

### MetricsSnapshot

- `Timestamp (ms UTC)`
- `Tick` – { `avgMs`, `p50`, `p95`, `max`, `overruns` }
- `Simulation` – { `stepAvg`, `stepMax`, `collisionMs`, `aiMs` }
- `Network` – { `bytesIn`, `bytesOut`, `msgRate`, `queueDepth`, `lossPct`, `rttMs` }
- `Client` – { `fps`, `frameTime`, `inputLatency`, `drawCalls` } (optional)
- `Memory` – { `heapUsedMb`, `heapCommittedMb`, `gcGen0`, `gcGen1`, `gcGen2`, `gcPauseMs` }
- `ServerHealth` – { `cpu`, `threads`, `dbLatency`, `redisLatency` }

### DebugToggleState

- `Key (DebugToggleKey)` – z. B. `overlay.hitbox`, `overlay.netstats`, `verbose.network`.
- `Value (bool|number|string)` – State.
- `Scope` – Session / Account / Character.
- `ChangedBy` – AccountId + Role.
- `ChangedAt` – Timestamp ms.
- `RevisionId` – für Deltas.

### DebugLogStream

- `Categories` – Liste (server.log, trace, metrics.delta, profiler).
- `Sampling` – float 0..1.
- `MaxBytesPerSecond` – Backpressure Limit.
- `Sequence` – monotone Counter.
- `Active` – bool.
- `LastAck` – letzte RequestId/Sequence, für Reconnect.

---

## 🔒 Zugriff & Safety-Gates

- **DevBuild Check**: Handshake trägt Flag `IsDebugBuild`. Ohne Flag werden alle DebugRequests hart abgewiesen (`NOT_IN_DEVBUILD`), außer Admin-Konsole.
- **Role Gate**: `AccessLevel` muss `>=` Command-Requirement sein. Teleport/Spawn => GM/Admin. Metrics/Logs => DevBuild reicht.
- **FeatureFlags**: pro CommandName (z. B. `debug.metrics.enabled`, `debug.logs.enabled`, `debug.sandbox.enabled`). Deaktivierte Commands liefern `NOT_ALLOWED`.
- **Allowlist**: Account/Device gebundene Liste (Server-Konfig). Nicht gelistete werden abgelehnt.
- **Sandbox Enforcement**: Teleport/Spawn nur in Zonen mit `IsSandbox=true`. Server prüft ZoneFlag, sonst `SANDBOX_ONLY`.
- **Rate Limits**: Token Bucket pro CommandName und Session. Details siehe [Rate Limits](#rate-limits).
- **Audit Pflicht**: Jeder Request + Result -> AuditStore (rotationssicher, hash).
- **Correlation Pflicht**: `RequestId` und `SessionId` müssen gesetzt sein; sonst `SCHEMA_VALIDATION_FAILED`.
- **No-Leak**: Payloads werden sanitisiert, sensitive Keys entfernt/gehasht, Strings gekürzt (max 256 Zeichen) mit Hash-Suffix.
- **Transport Security**: Nur über verschlüsselte Verbindung (Handshake 19). Kompression optional (20), aber Debug-Payloads oft klein.

---

## 🧾 Logging & Tracing

- **Structured Logging**: `SourceContext=DebugMessages`, Felder: `SessionId`, `RequestId`, `CommandName`, `AccountId`, `Role`, `Result`, `DurationMs`, `ErrorCode`, `ZoneId?`.
- **Tracing**: Span/Trace IDs propagiert aus Request. Falls leer, Server erzeugt TraceId. Trace-Spans aus Handlern werden als `DebugLog` mit Category `trace` gestreamt.
- **Sampling**: Server kann pro Category SamplingRate setzen (Default 10% für `server.log`, 100% für `error`).
- **Retention**: DebugLog Streams sind flüchtig, keine Persistenz außer Audit-Eintrag. Nur Error-Level können optional persistiert werden (flag `persistError=true` im Server).
- **Client Logging**: DevBuild kann lokale Logs an Server schicken (über separaten Mechanismus, nicht in diesem Range). Hier nur Server→Client.
- **Correlation**: Jede Log-Event trägt `Sequence`, `SessionId` und optional `RequestId` wenn aus Command entstanden.
- **PII Guard**: Regex/Dictionary ersetzt Mails, Tokens, IPs mit Hash. Data-Felder werden geclamped.

---

## 📈 Metrics & Profiling

- **Tick Metrics**: Erfasst über Server Profiler; outlier (>40ms) markiert `overrun=true`.
- **Network Metrics**: Berechnet per Sekundenfenster; enthält `bytesIn/out`, `msgRate`, `loss`, `rtt`.
- **Memory/GC**: `heapUsed`, `gcPauseMs`, `genCounts` pro Intervall.
- **Profiler Frames**: Optional, chunked als `DebugLog` Category `profiler`. Enthält `frameId`, `durationMs`, `samples` (komprimiert).
- **Client Metrics**: Nur wenn `allowClientMetrics=true` in SessionStart; Client sendet als Teil von `DebugCommand metrics.client.push`? (dann Response ack). In diesem Dokument fokus auf Server→Client Snapshots.
- **Export**: Snapshot kann `exportFormat` (json/msgpack) + `compression` (lz4/none) in `Payload` deklarieren.
- **Safe Defaults**: Kein kontinuierliches High-Volume per default; Snapshots auf Anfrage, Streams mit Sampling.

---

## 🧰 Runtime Toggles

- **Overlays**: `overlay.hitbox`, `overlay.navmesh`, `overlay.path`, `overlay.netstats`, `overlay.latencygraph`, `overlay.terrain`.
- **Verbosity**: `verbose.network`, `verbose.combat`, `verbose.ai`, `verbose.entity`.
- **Render**: `render.bounds`, `render.tiles`, `render.occlusion`.
- **Validation**: Toggles haben Schema (bool/enum). Server validiert und lehnt unzulässige Werte ab.
- **Scope**: Default SessionScope; optional `Scope=Character` wenn Flag `allowPersistentDebug` gesetzt (nur GM/Admin).
- **Notifications**: Jede Änderung erzeugt `DebugLog` mit Category `toggle.changed`.
- **Revert**: Bei SessionEnd werden alle Session-Toggles revertiert.

---

## 🔄 Sync, Deltas & Revisioning

- **RevisionId**: Jede Response mit änderbarem State trägt `RevisionId`. Client kann `If-None-Match` im Command Args angeben; Server antwortet mit `Success=true`, `Payload.notModified=true` falls keine Änderungen.
- **Sequence**: Streams (Logs) nutzen `Sequence` pro Session. Auf Reconnect kann Client `lastSequence` schicken; Server sendet ab nächstem Event weiter.
- **Snapshot vs Delta**: `metrics.snapshot` liefert Full-Snapshot; `metrics.stream` liefert Deltas seit letztem Snapshot.
- **Reconnect-Safe**: `ResumeToken` + `SessionId` + `RequestId` ermöglichen Rejoin ohne Datenverlust. Bei Drift -> Server sendet `DebugResponse` mit `ErrorCode=SESSION_EXPIRED`.
- **Idempotenz**: `RequestId` Wiederholung -> Server liefert letzte bekannte `DebugResponse` (Cached) und loggt `ErrorCode=REPLAY`? (je nach Implementierung). Standard: idempotent per RequestId.

---

## 🧱 DTOs / Interfaces

### Gemeinsame Felder

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | Konkrete MessageType | Ja |
| RequestId | ulong | Korrelations-ID | Ja bei Requests/Responses |
| SessionId | Guid | Aktive DebugSession | Ja |
| Timestamp | long | Unix ms | Ja |
| TraceId | Guid? | Trace Identifier | Nein |
| SpanId | Guid? | Span Identifier | Nein |

### Client DTOs

#### Basisinterface

```csharp
public interface IClientDebugMessage : IClientMessage
{
    ulong RequestId { get; set; }
    Guid SessionId { get; set; }
    Guid? TraceId { get; set; }
    Guid? SpanId { get; set; }
}
```

#### Validation-Regeln

- `RequestId > 0`
- `SessionId != Guid.Empty` außer beim ersten `session.start` (dann `Guid.Empty` erlaubt).
- `CommandName` darf keine Sonderzeichen enthalten außer `.`, `_`.
- `Args` Schlüssel lower-kebab-case oder dot-separated.
- Keine binären Blobs > 8KB.

### Server DTOs

#### Basisinterface

```csharp
public interface IServerDebugMessage : IServerMessage
{
    ulong RequestId { get; set; }
    Guid SessionId { get; set; }
    Guid? TraceId { get; set; }
    Guid? SpanId { get; set; }
}
```

#### Response-Struktur

- `Success` bool.
- `ErrorCode` string? (siehe Tabelle).
- `ErrorMessage` string? (sanitized).
- `RetryAfterMs` int? (bei RateLimit).
- `RevisionId` ulong? (Stateful).
- `Payload` Map? (Command-spezifisch).

### Schemas

#### Command Args Schema Beispiele

| CommandName | Pflichtfelder | Optional | Validation |
|-------------|---------------|----------|------------|
| `session.start` | `allowClientMetrics (bool)` | `requestedCapabilities (string[])`, `ttlSeconds (int)` | ttl 60..3600 |
| `session.end` | – | `reason (string)` | reason max 120 |
| `session.extend` | `ttlSeconds` | – | ttl 60..3600 |
| `metrics.snapshot` | – | `includeClient (bool)`, `sampleDurationMs (50-1000)` | |
| `metrics.stream.start` | `intervalMs (>=200)` | `sampling (0-1)` | interval clamp |
| `metrics.stream.stop` | – | – | stops stream |
| `logs.subscribe` | `categories (string[])` | `sampling (0-1)`, `maxBytesPerSecond` | categories whitelist |
| `logs.unsubscribe` | `categories` | – | must match existing |
| `toggle.get` | `keys (string[])` | – | keys whitelist |
| `toggle.set` | `key (string)`, `value` | `scope` | only whitelisted keys |
| `teleport` | `zoneId`, `x`, `y` | `facing` | sandbox only |
| `spawn` | `templateId`, `quantity`, `x`, `y` | `behavior`, `ttlSeconds` | sandbox only, quantity 1-5 |

---

## 🧩 Enums / ErrorCodes / Flags

### DebugAccessLevel

| Wert | Bedeutung | Typische Verwendung |
|------|-----------|---------------------|
| `None` | Kein Debug-Zugriff | Prod-Client |
| `DevBuild` | Lesender Zugriff (Metrics/Logs) | QA, Entwickler |
| `GM` | Eingeschränkt schreibend (Teleport Sandbox, Toggles) | GM-Tools |
| `Admin` | Vollzugriff innerhalb Debug-Range | LiveOps, Backend |

### DebugCommandName

| Name | Beschreibung | Rechte | Rate Limit |
|------|--------------|--------|-----------|
| `session.start` | Startet DebugSession | DevBuild/Admin | 1/10s |
| `session.end` | Beendet Session | DevBuild/Admin | 2/10s |
| `session.extend` | Verlängert TTL | DevBuild/Admin | 3/30s |
| `session.heartbeat` | Keepalive | DevBuild/Admin | 6/60s |
| `metrics.snapshot` | Einmaliger Snapshot | DevBuild | 1/5s |
| `metrics.stream.start` | Start Stream | DevBuild | 1/10s |
| `metrics.stream.stop` | Stop Stream | DevBuild | 3/10s |
| `logs.subscribe` | Subscribe Logs | DevBuild | 1/5s |
| `logs.unsubscribe` | Unsubscribe | DevBuild | 3/5s |
| `toggle.get` | Toggle lesen | DevBuild | 3/5s |
| `toggle.set` | Toggle setzen | GM/Admin | 2/10s |
| `teleport` | Sandbox Teleport | GM/Admin | 1/10s |
| `spawn` | Sandbox Spawn | GM/Admin | 1/30s |
| `trace.emit` | Custom TraceSpan | DevBuild | 5/10s |

### DebugErrorCodes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_ALLOWED` | Rolle/Allowlist fehlt | Request abbrechen |
| `NOT_IN_DEVBUILD` | Client kein DevBuild | Debug deaktivieren |
| `SESSION_EXPIRED` | Session TTL vorbei | Neu starten |
| `SESSION_REVOKED` | Server hat Session geschlossen | Kein Retry |
| `RATE_LIMITED` | Rate Limit erreicht | Retry nach `RetryAfterMs` |
| `INVALID_COMMAND` | CommandName unbekannt | Fix Client |
| `SCHEMA_VALIDATION_FAILED` | Args invalid | Payload prüfen |
| `SANDBOX_ONLY` | Aktion nur Sandbox | Zone wechseln |
| `TRACE_TOO_LARGE` | Payload zu groß | Kürzen |
| `EXPORT_NOT_SUPPORTED` | Exportformat verboten | anderes Format |
| `REPLAY` | RequestId wiederholt | RequestId erhöhen |

### DebugLogCategory

| Kategorie | Beschreibung |
|-----------|--------------|
| `server.log` | Strukturierte Server-Logs |
| `trace` | Trace-Spans, Performance-Punkte |
| `metrics.delta` | Kleine Delta-Metriken |
| `profiler` | Profiler/Flamegraph-Samples |
| `toggle.changed` | Toggle-Änderungen |
| `session` | Session Lifecycle Events |

### DebugToggleKey

| Key | Typ | Beschreibung |
|-----|-----|--------------|
| `overlay.hitbox` | bool | Hitbox-Overlay |
| `overlay.navmesh` | bool | NavMesh-Overlay |
| `overlay.netstats` | bool | Netzstatistik Overlay |
| `verbose.network` | bool | Netzwerk-Verbose |
| `verbose.ai` | bool | AI-Verbose |
| `render.bounds` | bool | Entity Bounds |
| `overlay.latencygraph` | bool | RTT Verlauf |

---

## ⚙️ Regeln & Sicherheit

### Rate Limits

| Command | Burst | Refill | Bucket |
|---------|-------|--------|--------|
| `session.start` | 1 | 1/10s | 2 |
| `session.end` | 2 | 1/10s | 3 |
| `session.extend` | 3 | 1/30s | 3 |
| `metrics.snapshot` | 1 | 1/5s | 2 |
| `metrics.stream.start` | 1 | 1/10s | 1 |
| `metrics.stream.stop` | 3 | 1/10s | 3 |
| `logs.subscribe` | 1 | 1/5s | 2 |
| `logs.unsubscribe` | 3 | 1/5s | 3 |
| `teleport` | 1 | 1/10s | 1 |
| `spawn` | 1 | 1/30s | 1 |
| `toggle.set` | 2 | 1/10s | 3 |

### Audit & No-Leak Policy

- **Audit Fields**: `Timestamp`, `AccountId`, `CharacterId`, `DeviceHash`, `SessionId`, `CommandName`, `ArgsHash`, `Result`, `ErrorCode`, `ZoneHash`.
- **Hashing**: Sensitive Strings -> SHA256; Koordinaten -> quantisierte Hash (um Rückschluss zu erschweren).
- **Redaction**: Tokens/Emails/IP werden erkannt und ersetzt.
- **Size Caps**: Messages >16KB abgelehnt; Strings >256 Zeichen gekürzt mit Hash-Suffix.
- **PII**: Keine Personendaten in Logs/Metrics. Clientseitige PII wird verworfen.

### Sandboxing

- Teleport/Spawn nur in Zonen mit Flag `Sandbox`.
- Spawn-Templates nur Dummy/Training (keine Loot/Economy).
- Teleport clamp auf Bounds + Kollisionstest.
- Auto-Cleanup: Spawn TTL (default 120s), Teleport Reset möglich.

### Anti-Abuse

- **Replay Detection**: Duplicate RequestId -> Response mit `REPLAY`.
- **Abuse Score**: Fehlgeschlagene Commands erhöhen Score; ab Schwelle -> Session revoke.
- **Heartbeat**: `session.heartbeat` erwartet alle 60s; ausbleiben -> Session expire.
- **Backpressure**: Log-Streams drosseln bei Netzwerkdruck; Fehler `RATE_LIMITED`.

---

## 📩 Aktive Messages 4900–4999

Reihenfolge entspricht `MessageType.cs`: DebugCommand (4900), DebugResponse (4901), DebugLog (4902), DebugTeleport (4903), DebugSpawn (4904).

### DebugCommand (4900)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** abhängig von CommandName (🧪 DevBuild / 👑 GM / 👑 Admin)

#### Beschreibung
Generischer Debug-Request mit dynamischem `CommandName` und `Args`. Dient als Wrapper für alle Debug-Operationen (Session-Management, Metrics, Logs, Toggles, Sandbox). Server prüft Safety-Gates, validiert Args, führt aus und antwortet mit `DebugResponse (4901)`.

#### Im Scope ✅

- Start/Ende/Verlängerung einer DebugSession
- Einmalige oder gestreamte Metrics
- Log-Subscription/Unsubscription
- Toggle lesen/setzen
- Sandbox-Aktionen (Teleport/Spawn) – alternativ dedizierte Messages

#### Nicht im Scope ❌

- Persistente Admin-Änderungen (siehe Kategorie 23)
- Spielerbezogene Progression/Economy
- Unsichere Remote-Execution

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DebugCommand` | Ja |
| RequestId | ulong | Korrelations-ID | Ja |
| SessionId | Guid | DebugSession (Guid.Empty bei session.start) | Ja |
| CommandName | string | Siehe [DebugCommandName](#debugcommandname) | Ja |
| Args | Map<string, object> | Parameter laut Schema | Ja |
| TraceId | Guid? | Trace-Kette | Nein |
| SpanId | Guid? | Span Id | Nein |
| Timestamp | long | Unix ms | Ja |

#### Erwartete Response

- `DebugResponse` (4901) mit identischem RequestId.

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DebugCommand)]
public class DebugCommand : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.DebugCommand;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public Guid SessionId { get; set; }
    [Key(3)] public string CommandName { get; set; } = default!;
    [Key(4)] public Dictionary<string, object> Args { get; set; } = new();
    [Key(5)] public Guid? TraceId { get; set; }
    [Key(6)] public Guid? SpanId { get; set; }
    [Key(7)] public long Timestamp { get; set; }
}
```

#### Server-Verhalten

1. **Session-Check**: `SessionId` gültig? Falls `session.start`: erlaubt `Guid.Empty`.
2. **Role/Gate**: Prüft DevBuild/Role/Allowlist/FeatureFlag.
3. **Rate Limit**: pro CommandName.
4. **Schema Validation**: Args gegen Command-Schema.
5. **Execution**: Führt Operation aus oder startet Stream.
6. **Audit**: schreibt Audit-Eintrag.
7. **Response**: Sendet `DebugResponse` mit Result.

#### Client-Verhalten

- Generiert monoton `RequestId`.
- Hält `SessionId` aus `session.start` bereit.
- Interpretiert `Payload` der Response command-spezifisch.
- Sendet Heartbeat (session.heartbeat) um Session lebendig zu halten.

#### Flow-Diagramm (Session Start)

```
Client                         Server
  │                              │
  │  DebugCommand(session.start) │
  │─────────────────────────────►│
  │                              │  Validate build/role
  │                              │  Create session
  │  DebugResponse(sessionId, ttl, capabilities) │
  │◄─────────────────────────────│
  │  DebugLog(category=session, message=started) │
  │◄─────────────────────────────│
```

#### Beispiel Payloads

```csharp
var startSession = new DebugCommand
{
    RequestId = 1,
    SessionId = Guid.Empty,
    CommandName = "session.start",
    Args = new()
    {
        ["allowClientMetrics"] = true,
        ["requestedCapabilities"] = new[] { "metrics.read", "logs.stream", "toggle.write" },
        ["ttlSeconds"] = 900
    },
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

var metricsSnapshot = new DebugCommand
{
    RequestId = 2,
    SessionId = sessionId,
    CommandName = "metrics.snapshot",
    Args = new()
    {
        ["includeClient"] = false,
        ["sampleDurationMs"] = 250
    },
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

#### Error Codes

| Code | Bedeutung |
|------|-----------|
| `NOT_ALLOWED` | Keine Rolle/Allowlist/FeatureFlag |
| `NOT_IN_DEVBUILD` | Prod-Client |
| `SESSION_EXPIRED` | Session ungültig |
| `RATE_LIMITED` | Rate Limit überschritten |
| `INVALID_COMMAND` | Command unbekannt |
| `SCHEMA_VALIDATION_FAILED` | Args invalid |
| `SANDBOX_ONLY` | Zone kein Sandbox |
| `REPLAY` | RequestId wiederholt |

#### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `DebugResponse` | 4901 | Response |
| `DebugLog` | 4902 | Streams/Events |
| `DebugTeleport` | 4903 | Alternativer spezialisierter Teleport |
| `DebugSpawn` | 4904 | Alternativer spezialisierter Spawn |

---

### DebugResponse (4901)

**Richtung:** 📥 Server → Client  
**Frequenz:** pro Request einmal  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Ack und Ergebnis von `DebugCommand`, `DebugTeleport`, `DebugSpawn`. Enthält Erfolg/Fehler, optionale Payload (z. B. MetricsSnapshot, ToggleState) und Steuerfelder (RetryAfter, RevisionId).

#### Im Scope ✅

- Bestätigung aller Debug-Requests
- Transport von Ergebnissen/Snapshots
- Transport von Rate-Limit / Error-Information

#### Nicht im Scope ❌

- Stream-Events (Logs/Metrics) → `DebugLog`
- Admin-spezifische Messages außerhalb Debug

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DebugResponse` | Ja |
| RequestId | ulong | Korrelierter Request | Ja |
| SessionId | Guid | DebugSession | Ja |
| Success | bool | Erfolg? | Ja |
| ErrorCode | string? | Siehe DebugErrorCodes | Nein |
| ErrorMessage | string? | Sanitized Fehler | Nein |
| RetryAfterMs | int? | Rate-Limit Retry | Nein |
| RevisionId | ulong? | Revision für Cache | Nein |
| Payload | Map<string, object>? | Ergebnisdaten | Nein |
| Timestamp | long | Unix ms | Ja |

#### Erwartete Response

- Keine (ist Response).

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DebugResponse)]
public class DebugResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.DebugResponse;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public Guid SessionId { get; set; }
    [Key(3)] public bool Success { get; set; }
    [Key(4)] public string? ErrorCode { get; set; }
    [Key(5)] public string? ErrorMessage { get; set; }
    [Key(6)] public int? RetryAfterMs { get; set; }
    [Key(7)] public ulong? RevisionId { get; set; }
    [Key(8)] public Dictionary<string, object>? Payload { get; set; }
    [Key(9)] public long Timestamp { get; set; }
}
```

#### Server-Verhalten

1. Erstellt Response nach Command-Execution.
2. Kopiert RequestId/SessionId.
3. Füllt Success/Error Felder.
4. Fügt Payload hinzu (z. B. MetricsSnapshot, ToggleState).
5. Setzt RetryAfterMs bei RateLimit.
6. Loggt Audit.

#### Client-Verhalten

- Matched per RequestId.
- Interpretiert Payload per Command.
- Bei Error: zeigt Meldung, respektiert RetryAfter.
- Nutzt RevisionId für Cache.

#### Flow-Diagramm (Metrics Snapshot)

```
Client                         Server
  │                              │
  │  DebugCommand(metrics.snapshot) │
  │─────────────────────────────►│
  │                              │  Sammle Metriken
  │                              │
  │  DebugResponse(Success, Payload=snapshot) │
  │◄─────────────────────────────│
```

#### Beispiel Payloads

```csharp
var metricsResponse = new DebugResponse
{
    RequestId = 2,
    SessionId = sessionId,
    Success = true,
    RevisionId = 5,
    Payload = new()
    {
        ["tick"] = new { avg = 16.6, p95 = 24.1, max = 38.0 },
        ["network"] = new { bytesIn = 1200, bytesOut = 1800, msgRate = 45, loss = 0.0 },
        ["gc"] = new { gen0 = 2, gen1 = 0, gen2 = 0, pauseMs = 1.3 }
    },
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

#### Error Codes

| Code | Bedeutung |
|------|-----------|
| `NOT_ALLOWED` | fehlende Rolle/Flag |
| `SESSION_EXPIRED` | Session ungültig |
| `RATE_LIMITED` | Rate-Limit erreicht |
| `INVALID_COMMAND` | unbekannt |
| `SCHEMA_VALIDATION_FAILED` | Args invalid |
| `SANDBOX_ONLY` | verbotene Zone |
| `REPLAY` | RequestId wiederholt |

#### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `DebugCommand` | 4900 | Request |
| `DebugTeleport` | 4903 | Request |
| `DebugSpawn` | 4904 | Request |

---

### DebugLog (4902)

**Richtung:** 📡 Server → Client (Event)  
**Frequenz:** Mittel/Häufig je nach Subscription  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 🧪 DevBuild / 👑 GM / 👑 Admin

#### Beschreibung
Streamt Logs, TraceSpans, Metrics-Deltas, Profiler-Samples und Toggle-Events an Debug-Clients. Nur aktiv bei laufender Session und gültiger Subscription (`logs.subscribe`). Enthält Sequenznummer für Ordering und optionalen Bezug auf RequestId.

#### Im Scope ✅

- Server-Logs nach Kategorien
- TraceSpans (Start/End) als Events
- Metrics-Deltas (leichte Updates)
- Profiler-Flamegraph-Chunks
- Toggle-Änderungen (z. B. Overlay an/aus)
- Session Lifecycle Events

#### Nicht im Scope ❌

- Persistente Speicherung beim Client
- Sensitive Daten (werden gehasht/entfernt)
- Broadcast an Nicht-Debug-Clients

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DebugLog` | Ja |
| SessionId | Guid | DebugSession | Ja |
| Sequence | ulong | Monotone Event-ID | Ja |
| Category | string | Log/trace/metrics.delta/profiler/toggle.changed/session | Ja |
| Level | string | trace/debug/info/warn/error | Ja |
| Message | string | Sanitized Text | Ja |
| Data | Map<string, object>? | Kontextdaten | Nein |
| TraceId | Guid? | Trace | Nein |
| SpanId | Guid? | Span | Nein |
| RequestId | ulong? | Zugehöriger Request | Nein |
| Timestamp | long | Unix ms | Ja |

#### Erwartete Response

- Keine direkte Response. Subscription/Unsubscription via `DebugCommand`.

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DebugLog)]
public class DebugLog : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.DebugLog;
    [Key(1)] public Guid SessionId { get; set; }
    [Key(2)] public ulong Sequence { get; set; }
    [Key(3)] public string Category { get; set; } = default!;
    [Key(4)] public string Level { get; set; } = default!;
    [Key(5)] public string Message { get; set; } = default!;
    [Key(6)] public Dictionary<string, object>? Data { get; set; }
    [Key(7)] public Guid? TraceId { get; set; }
    [Key(8)] public Guid? SpanId { get; set; }
    [Key(9)] public ulong? RequestId { get; set; }
    [Key(10)] public long Timestamp { get; set; }
}
```

#### Server-Verhalten

1. Prüft Session aktiv + Subscription aktiv.
2. Sanitiert Message/Data, clamped size.
3. Erhöht Sequence; sendet Event.
4. Bei Backpressure: Sampling erhöhen, Low-Level droppen, Errors priorisieren.
5. Loggt Audit minimal (Sequence, Category, Hash(Message)).

#### Client-Verhalten

- Zeigt Logs im DevConsole-UI.
- Nutzt Sequence zum Erkennen von Verlusten; kann Resubscribe auslösen.
- Filtert nach Category/Level, ggf. lokal speichern (DevBuild-only).
- Bei fehlenden Events (Sequence Lücke) optional `logs.resync` Command senden.

#### Flow-Diagramm (Log-Stream)

```
Client                         Server
  │                              │
  │  DebugCommand(logs.subscribe categories=[server.log,trace]) │
  │─────────────────────────────►│
  │                              │  Register stream
  │  DebugResponse(Success)      │
  │◄─────────────────────────────│
  │  DebugLog(seq=1, category=server.log) │
  │◄─────────────────────────────│
  │  DebugLog(seq=2, category=trace)      │
  │◄─────────────────────────────│
```

#### Beispiel Payloads

```csharp
var logEvent = new DebugLog
{
    SessionId = sessionId,
    Sequence = 1,
    Category = "server.log",
    Level = "info",
    Message = "Zone tick complete",
    Data = new()
    {
        ["tickMs"] = 17.2,
        ["entities"] = 320,
        ["overrun"] = false
    },
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

var traceEvent = new DebugLog
{
    SessionId = sessionId,
    Sequence = 2,
    Category = "trace",
    Level = "debug",
    Message = "DebugMetricsHandler",
    Data = new()
    {
        ["durationMs"] = 3.4,
        ["status"] = "Ok"
    },
    TraceId = Guid.NewGuid(),
    SpanId = Guid.NewGuid(),
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

#### Error Codes

| Code | Bedeutung |
|------|-----------|
| `SESSION_EXPIRED` | Stream beendet |
| `RATE_LIMITED` | Sampling erhöht |

#### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `DebugCommand` | 4900 | Subscription Management |
| `DebugResponse` | 4901 | ACK für Sub/Unsub |

---

### DebugTeleport (4903)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GM / 👑 Admin (SandboxOnly)

#### Beschreibung
Teleportiert den eigenen Character in Sandbox-Zonen. Wird für QA/Dev genutzt, um schnelle Positionswechsel zu testen. Server erzwingt Sandbox-Flag und Kollisionsprüfung. Antwort erfolgt via `DebugResponse`.

#### Im Scope ✅

- Teleport in Sandbox/Test-Zonen
- Stuck-Recovery in Dev-Build
- Pathfinding/Collision Debugging

#### Nicht im Scope ❌

- Live-Production Zonen
- Teleport anderer Spieler (siehe Admin 2300ff)
- Persistente Bewegungsänderungen

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DebugTeleport` | Ja |
| RequestId | ulong | Korrelations-ID | Ja |
| SessionId | Guid | DebugSession | Ja |
| TargetZoneId | ushort | Sandbox Zone | Ja |
| X | float | Ziel X | Ja |
| Y | float | Ziel Y | Ja |
| Facing | float | Blickrichtung 0-360 | Nein |
| TraceId | Guid? | Trace | Nein |
| SpanId | Guid? | Span | Nein |
| Timestamp | long | Unix ms | Ja |

#### Erwartete Response

- `DebugResponse` (4901) mit `Success=true` und `Payload.teleport = { zoneId, x, y, facing }`.
- Bei Fehler: `Success=false`, `ErrorCode` (`SANDBOX_ONLY`, `NOT_ALLOWED`, `SCHEMA_VALIDATION_FAILED`).

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DebugTeleport)]
public class DebugTeleport : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.DebugTeleport;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public Guid SessionId { get; set; }
    [Key(3)] public ushort TargetZoneId { get; set; }
    [Key(4)] public float X { get; set; }
    [Key(5)] public float Y { get; set; }
    [Key(6)] public float? Facing { get; set; }
    [Key(7)] public Guid? TraceId { get; set; }
    [Key(8)] public Guid? SpanId { get; set; }
    [Key(9)] public long Timestamp { get; set; }
}
```

#### Server-Verhalten

1. Prüft Session, Role (GM/Admin), Allowlist, DevBuild.
2. Validiert ZoneFlag `Sandbox`.
3. Clamped Koordinaten und Kollisionscheck (NavMesh).
4. Führt Teleport aus (server authoritative).
5. Sendet `DebugResponse` mit Result.
6. Audit: alte Position hash, neue Position hash, ZoneId.

#### Client-Verhalten

- Sendet nur aus DevConsole.
- Wartet auf Response; bei Erfolg UI aktualisieren.
- Bei Fehler `SANDBOX_ONLY`: Benutzerhinweis.
- Optionale Nachbearbeitung: `ZoneState`/`ZoneDelta` neu anfordern.

#### Flow-Diagramm

```
Client                         Server
  │                              │
  │  DebugTeleport (4903)        │
  │─────────────────────────────►│
  │                              │  Validate + Move
  │  DebugResponse (Success)     │
  │◄─────────────────────────────│
  │  DebugLog (session, teleport.performed) │
  │◄─────────────────────────────│
```

#### Beispiel Payloads

```csharp
var tp = new DebugTeleport
{
    RequestId = 10,
    SessionId = sessionId,
    TargetZoneId = 9001, // sandbox
    X = 123.4f,
    Y = 456.7f,
    Facing = 180f,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

#### Error Codes

| Code | Bedeutung |
|------|-----------|
| `SANDBOX_ONLY` | Zone ohne Sandbox-Flag |
| `NOT_ALLOWED` | Rolle/Allowlist fehlt |
| `SCHEMA_VALIDATION_FAILED` | Ungültige Koordinaten |
| `RATE_LIMITED` | Zu viele Teleports |

#### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `DebugResponse` | 4901 | Response |
| `DebugLog` | 4902 | Teleport Event |
| `DebugCommand` | 4900 | Alternative (`teleport` CommandName) |

---

### DebugSpawn (4904)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GM / 👑 Admin (SandboxOnly)

#### Beschreibung
Spawnt temporäre Dummy-Entities in Sandbox-Zonen für Tests (Combat Dummy, Pathfinding, Visibility). Keine Loot/Economy. Auto-Despawn nach TTL. Antwort via `DebugResponse`.

#### Im Scope ✅

- Spawn von Dummies (aggro-frei, kein Loot)
- Parameter: TemplateId, Quantity, Behavior, TTL
- Sandbox-Testaufbauten

#### Nicht im Scope ❌

- Loot-/Drop-generierende Spawns
- Persistente DB-Einträge
- Spawn in Live-Zonen

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DebugSpawn` | Ja |
| RequestId | ulong | Korrelations-ID | Ja |
| SessionId | Guid | DebugSession | Ja |
| TemplateId | int | Dummy/Training Template | Ja |
| Quantity | byte | 1-5 | Ja |
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| Behavior | string | `passive`, `dummy`, `wander` | Nein |
| TtlSeconds | int | Auto-Despawn | Nein |
| TraceId | Guid? | Trace | Nein |
| SpanId | Guid? | Span | Nein |
| Timestamp | long | Unix ms | Ja |

#### Erwartete Response

- `DebugResponse` (4901) mit `Payload.spawnIds` (List<Guid>), `ttlSeconds`.
- Fehler: `SANDBOX_ONLY`, `NOT_ALLOWED`, `RATE_LIMITED`, `SCHEMA_VALIDATION_FAILED`.

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DebugSpawn)]
public class DebugSpawn : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.DebugSpawn;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public Guid SessionId { get; set; }
    [Key(3)] public int TemplateId { get; set; }
    [Key(4)] public byte Quantity { get; set; }
    [Key(5)] public float X { get; set; }
    [Key(6)] public float Y { get; set; }
    [Key(7)] public string? Behavior { get; set; }
    [Key(8)] public int? TtlSeconds { get; set; }
    [Key(9)] public Guid? TraceId { get; set; }
    [Key(10)] public Guid? SpanId { get; set; }
    [Key(11)] public long Timestamp { get; set; }
}
```

#### Server-Verhalten

1. Prüft Session, Role, Allowlist, DevBuild.
2. Validiert Sandbox-Flag der Zone.
3. Validiert Quantity (1-5), TemplateId whitelist (nur Dummy-Templates).
4. Spawnt Entities mit Behavior (default `dummy`).
5. Registriert Auto-Despawn Timer (TTL default 120s).
6. Sendet `DebugResponse` mit SpawnIds, ttlSeconds.
7. Audit: TemplateId, Quantity, PositionHash, SpawnIds hashed.

#### Client-Verhalten

- Nur aus DevConsole.
- Verarbeitet SpawnIds, zeigt im UI.
- Optional Cleanup via `DebugCommand` (`spawn.despawn`).
- Beachtet TTL; plant Re-Spawns falls nötig.

#### Flow-Diagramm

```
Client                         Server
  │                              │
  │  DebugSpawn (4904)           │
  │─────────────────────────────►│
  │                              │  Validate + Spawn
  │  DebugResponse (spawnIds)    │
  │◄─────────────────────────────│
  │  DebugLog (spawn.created)    │
  │◄─────────────────────────────│
```

#### Beispiel Payloads

```csharp
var spawn = new DebugSpawn
{
    RequestId = 11,
    SessionId = sessionId,
    TemplateId = 1000,
    Quantity = 3,
    X = 50,
    Y = 75,
    Behavior = "dummy",
    TtlSeconds = 120,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

#### Error Codes

| Code | Bedeutung |
|------|-----------|
| `SANDBOX_ONLY` | Zone nicht Sandbox |
| `NOT_ALLOWED` | Rolle/Allowlist fehlt |
| `RATE_LIMITED` | Spawn-Limit überschritten |
| `SCHEMA_VALIDATION_FAILED` | Parameter invalid |

#### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `DebugResponse` | 4901 | Response |
| `DebugLog` | 4902 | Spawn Events |
| `DebugCommand` | 4900 | Alternative (`spawn`) Command |

---

## 🗑️ Obsolete Messages

Keine obsoleten Debug-Messages. Legacy-Handler sollen auf `DebugCommand` + `DebugResponse` + `DebugLog` migriert werden. Falls alte Client-Versionen noch `DebugCommand` ohne SessionId senden, Server antwortet mit `SCHEMA_VALIDATION_FAILED`.

---

## 🧨 Edge Cases & Fehlerfälle

- **Session Timeout**: Keine Heartbeats → `DebugResponse(ErrorCode=SESSION_EXPIRED)` und Streams stoppen.
- **RequestId Replay**: Wiederholte RequestId → Response mit `REPLAY`, kein Audit-Duplikat (verweist auf Original).
- **Oversized Args**: Args >16KB → `TRACE_TOO_LARGE`.
- **Missing Capabilities**: Command nicht in Capabilities → `NOT_ALLOWED`.
- **Backpressure**: Bei hoher Log-Rate erhöht Server SamplingRate; kommuniziert in `DebugLog` Data.
- **Teleport Collision**: Findet keinen validen Punkt → `SCHEMA_VALIDATION_FAILED` + Vorschlag in Payload.
- **Spawn Cap**: Überschreitet Session SpawnCap (z. B. 20) → `RATE_LIMITED` + `Payload.cleanupHint=true`.
- **Zone Transfer**: Wenn Teleport Zone != aktueller Shard und Sandbox nicht vorhanden → `SANDBOX_ONLY`.
- **Metrics Stream Loss**: Sequence-Lücke erkannt → Client sendet `logs.resync` Command (über `DebugCommand`), Server acked via `DebugResponse`.

---

## 📎 Anhang

### MessageType Enum Updates

Keine neuen Enum-Werte erforderlich; bestehende Debug-Range (Quelle: `MessageType.cs`):

```csharp
// DEBUG / DEVELOPMENT (4900-4999)
DebugCommand = 4900,
DebugResponse = 4901,
DebugLog = 4902,
DebugTeleport = 4903,
DebugSpawn = 4904
```

### Flows (ASCII)

#### Enable Debug Session

```
Client                         Server
  │                              │
  │  DebugCommand(session.start) │
  │─────────────────────────────►│
  │                              │  Validate build/role/allowlist
  │                              │  Create Session (SessionId, ttl, capabilities)
  │  DebugResponse(sessionId, ttl, capabilities) │
  │◄─────────────────────────────│
  │  DebugLog(category=session, message=started) │
  │◄─────────────────────────────│
```

#### Request Metrics Snapshot

```
Client                         Server
  │                              │
  │  DebugCommand(metrics.snapshot) │
  │─────────────────────────────►│
  │                              │  Collect metrics (200ms)
  │  DebugResponse(payload=snapshot) │
  │◄─────────────────────────────│
  │  DebugLog(category=trace, message=metrics.handler, duration=…) │
  │◄─────────────────────────────│
```

#### Toggle Overlay

```
Client                         Server
  │                              │
  │  DebugCommand(toggle.set overlay.hitbox=true) │
  │─────────────────────────────►│
  │                              │  Apply toggle
  │  DebugResponse(success)      │
  │◄─────────────────────────────│
  │  DebugLog(category=toggle.changed, data={overlay.hitbox:true}) │
  │◄─────────────────────────────│
```

#### Stream Logs

```
Client                         Server
  │                              │
  │  DebugCommand(logs.subscribe categories=[server.log,trace]) │
  │─────────────────────────────►│
  │                              │  Register stream
  │  DebugResponse(success)      │
  │◄─────────────────────────────│
  │  DebugLog(seq=1, category=server.log) │
  │◄─────────────────────────────│
  │  DebugLog(seq=2, category=trace)      │
  │◄─────────────────────────────│
```

#### End Session

```
Client                         Server
  │                              │
  │  DebugCommand(session.end)   │
  │─────────────────────────────►│
  │                              │  Stop streams, revoke session
  │  DebugResponse(success)      │
  │◄─────────────────────────────│
  │  DebugLog(category=session, message=ended) │
  │◄─────────────────────────────│
```

### Validierungsmatrix

| Feld | Regel | Fehlercode |
|------|-------|------------|
| RequestId | >0, monoton | REPLAY/SCHEMA_VALIDATION_FAILED |
| SessionId | != Guid.Empty (außer session.start) | SESSION_EXPIRED |
| CommandName | Whitelist | INVALID_COMMAND |
| Args Größe | <=16KB | TRACE_TOO_LARGE |
| String Länge | <=256 | TRACE_TOO_LARGE |
| Zone Sandbox | `IsSandbox==true` | SANDBOX_ONLY |
| Quantity (Spawn) | 1..5 | SCHEMA_VALIDATION_FAILED |
| Rate Limit | TokenBucket | RATE_LIMITED |
| Role | AccessLevel >= Required | NOT_ALLOWED |
| DevBuild | Flag muss gesetzt sein | NOT_IN_DEVBUILD |

### Beispiel-Testfälle

#### Session Start Success
- **Given** DevBuild Client, Allowlist, Command `session.start`
- **When** RequestId=1, ttl=600
- **Then** Response Success, SessionId != Empty, ttl=600, capabilities default, Audit logged.

#### Session Start Fail (Prod Client)
- **Given** Prod Client, Command `session.start`
- **Then** Response Success=false, ErrorCode=NOT_IN_DEVBUILD, no SessionId, Audit logged.

#### Metrics Snapshot Rate Limit
- **Given** interval <5s, zwei Aufrufe in <5s
- **Then** zweiter Response ErrorCode=RATE_LIMITED, RetryAfterMs set.

#### Teleport Sandbox Violation
- **Given** TargetZone not Sandbox
- **Then** Response ErrorCode=SANDBOX_ONLY, Success=false, Audit.

#### Spawn Quantity Too High
- **Given** Quantity=10
- **Then** Response ErrorCode=SCHEMA_VALIDATION_FAILED, Message hint "quantity range 1-5".

#### Logs Subscribe and Stream
- **Given** logs.subscribe categories=server.log,trace
- **Then** Response Success, DebugLog events arriving with Sequence starting 1, Category in whitelist.

#### Session Heartbeat Missing
- **Given** Session started, Heartbeat nicht gesendet
- **Then** nach TTL: DebugLog session.expired, weitere Requests => SESSION_EXPIRED.

### Payload-Schemas im Detail

#### Metrics Snapshot Payload Beispiel (Response)

```json
{
  "tick": { "avg": 16.6, "p95": 24.1, "max": 38.0, "overruns": 0 },
  "simulation": { "stepAvg": 5.2, "stepMax": 8.9, "collisionMs": 1.1, "aiMs": 0.9 },
  "network": { "bytesIn": 1800, "bytesOut": 2200, "msgRate": 52, "queueDepth": 2, "loss": 0.0, "rttMs": 42 },
  "memory": { "heapUsedMb": 512, "heapCommittedMb": 640, "gcGen0": 2, "gcGen1": 0, "gcGen2": 0, "gcPauseMs": 1.4 },
  "serverHealth": { "cpu": 38, "threads": 112, "dbLatency": 4, "redisLatency": 2 }
}
```

#### Toggle Set Payload Beispiel (Response)

```json
{
  "toggle": {
    "key": "overlay.netstats",
    "value": true,
    "scope": "Session",
    "revisionId": 12
  }
}
```

#### Teleport Response Payload Beispiel

```json
{
  "teleport": {
    "zoneId": 9001,
    "x": 123.4,
    "y": 456.7,
    "facing": 180.0
  }
}
```

#### Spawn Response Payload Beispiel

```json
{
  "spawn": {
    "templateId": 1000,
    "quantity": 3,
    "spawnIds": [
      "11111111-2222-3333-4444-555555555555",
      "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
      "99999999-8888-7777-6666-555555555555"
    ],
    "ttlSeconds": 120
  }
}
```

#### Log Event Payload Beispiel

```json
{
  "category": "toggle.changed",
  "level": "info",
  "message": "overlay.hitbox enabled",
  "data": {
    "key": "overlay.hitbox",
    "value": true,
    "changedBy": 42,
    "revisionId": 7
  },
  "sequence": 5
}
```

---

## 📚 Referenz-Szenarien (Deep Dives)

Die folgenden Szenarien liefern detaillierte Schritt-für-Schritt-Anleitungen, Validation Rules, erwartete Server-Antworten und Audit-Pflichtpunkte. Jeder Eintrag ist vollständig ausführbar und deckt Edge Cases, Security und Telemetrie ab.

### Session Management Szenarien

1. **Session Start mit Capabilities**
   - Request: `DebugCommand(session.start)` mit `requestedCapabilities=["metrics.read","logs.stream","toggle.write"]`, `ttlSeconds=1200`.
   - Validation: DevBuild=TRUE, Account auf Allowlist, RateLimit ok.
   - Response: `DebugResponse` Success, `Payload.sessionId`, `Payload.capabilities`.
   - Audit: `SessionCreated`, Hash(request args), Role, DeviceHash.
   - Follow-up: `DebugLog` category=session, message=started, seq=1.
2. **Session Start ohne Allowlist**
   - Request: gleiche Args, Allowlist miss.
   - Response: `Success=false`, `ErrorCode=NOT_ALLOWED`.
   - Audit: `Denied`, Reason=Allowlist.
3. **Session Extend**
   - Request: `DebugCommand(session.extend)` ttlSeconds=600.
   - Checks: Session active, ttlSeconds range.
   - Response: Success, new `Payload.expiresAt`.
   - Audit: `SessionExtended`.
4. **Session Heartbeat**
   - Request: `DebugCommand(session.heartbeat)` ohne Args.
   - Response: Success, optional `Payload.remainingTtl`.
   - Behavior: Resets idle timer.
5. **Session End (Client)**
   - Request: `DebugCommand(session.end)`.
   - Response: Success.
   - Events: `DebugLog` session ended, Streams closed.
6. **Session Revoke (Server)**
   - Trigger: Admin revokes session.
   - Event: `DebugLog` session revoked, ErrorCode=SESSION_REVOKED in nächsten Responses.
7. **Session Resume**
   - Preconditions: Session active, client reconnects.
   - Request: `DebugCommand(session.start)` mit `Args.resumeToken`.
   - Response: Success, same SessionId, Streams resumed from lastSequence.
8. **Session Expire**
   - Condition: ttl elapsed ohne Heartbeat.
   - Effect: `DebugLog` session.expired, Response auf nächste Requests -> SESSION_EXPIRED.
9. **Session Capability Deny**
   - Request: `toggle.set` ohne Capability `toggle.write`.
   - Response: NOT_ALLOWED.
   - Audit: capability_miss logged.
10. **Session Device Mismatch**
    - Reconnect von anderem DeviceId.
    - Response: NOT_ALLOWED, Audit: device_mismatch.

### Metrics Szenarien

11. **Snapshot Basic**
    - Request: `metrics.snapshot`, sampleDurationMs=200.
    - Response: Payload enthält tick/network/gc/serverHealth.
    - Validation: RateLimit 1/5s.
12. **Snapshot With Client Metrics**
    - Args: `includeClient=true`, Session Capability `allowClientMetrics=true`.
    - Response: Payload.client section.
13. **Snapshot Export JSON**
    - Args: `exportFormat="json"`, `compression="none"`.
    - Response: Payload.export bytes (Base64) + metadata.
14. **Snapshot Export MsgPack + LZ4**
    - Args: `exportFormat="msgpack"`, `compression="lz4"`.
    - Response: Payload.export, size capped.
15. **Snapshot Rate Limited**
    - Rapid two requests <5s.
    - Second Response: RATE_LIMITED, RetryAfterMs=5000.
16. **Stream Start**
    - Request: `metrics.stream.start`, intervalMs=500, sampling=1.0.
    - Response: Success.
    - Events: `DebugLog` category=metrics.delta every 500ms.
17. **Stream Stop**
    - Request: `metrics.stream.stop`.
    - Response: Success.
    - Events stop; Audit logged.
18. **Stream Backpressure**
    - Condition: Client slow.
    - Server action: Sampling reduce to 0.5, Data.sampling reported.
19. **Snapshot with If-None-Match**
    - Args: `ifNoneMatch=10`.
    - If unchanged: Response Success=true, Payload.notModified=true.
20. **Snapshot Error Schema**
    - Args: `sampleDurationMs=99999`.
    - Response: SCHEMA_VALIDATION_FAILED.

### Logging Szenarien

21. **Subscribe Logs**
    - Request: `logs.subscribe` categories [server.log].
    - Response: Success, Payload.subscriptionId optional.
22. **Subscribe Trace + Profiler**
    - Request: categories [trace, profiler], sampling=0.2.
    - Response: Success.
23. **Unsubscribe Single Category**
    - Request: `logs.unsubscribe` categories [profiler].
    - Response: Success.
24. **Unsubscribe All**
    - Request: categories ["*"].
    - Response: Success.
25. **Invalid Category**
    - Request: `logs.subscribe` category=secrets.
    - Response: INVALID_COMMAND or SCHEMA_VALIDATION_FAILED.
26. **PII Redaction**
    - Server log contains email/token.
    - DebugLog Message/Data sanitized before send.
27. **High Volume**
    - Condition: >100 events/s.
    - Action: Sampling enforced, event with `Data.sampling`.
28. **Sequence Gap Detection**
    - Client sees seq 10 then 12.
    - Client sends `logs.resync` (CommandName).
    - Server resends from seq 13 onward; no replay of 11.
29. **Persistent Error Forward**
    - Error-level events may be forwarded even with sampling.
    - Guarantee: Errors never dropped unless connection lost.
30. **Log Stream End on Session Close**
    - Session end triggers termination of log streams, final DebugLog `session.ended`.

### Toggle Szenarien

31. **Toggle Get**
    - Request: `toggle.get` keys [overlay.netstats].
    - Response: Payload.toggles list.
32. **Toggle Set Allowed**
    - Request: `toggle.set` key overlay.hitbox value true.
    - Response: Success, revision increment.
    - Event: DebugLog toggle.changed.
33. **Toggle Set Not Allowed**
    - Missing Capability or Role.
    - Response: NOT_ALLOWED.
34. **Toggle Invalid Key**
    - Response: INVALID_COMMAND.
35. **Toggle Scope Character**
    - Args.scope=character, allowed only GM/Admin.
    - Response: Success if allowed, else NOT_ALLOWED.
36. **Toggle Revert on Session End**
    - Session ends -> automatic revert, DebugLog revert entries.
37. **Toggle Bulk Get**
    - keys=["overlay.hitbox","overlay.navmesh","verbose.network"].
    - Response: aggregated state with revision ids.
38. **Toggle Conflict**
    - Two clients same Session try to set; latest wins by RequestId ordering.
    - Response includes revisionId for resolution.
39. **Toggle Persist Denied**
    - scope=persistent without flag -> NOT_ALLOWED.
40. **Toggle Audit**
    - Each change hashed; no raw values with PII.

### Teleport Szenarien

41. **Teleport Success**
    - Sandbox zone, valid coords.
    - Response Success, payload with clamped coords.
42. **Teleport Collision**
    - Collides -> SCHEMA_VALIDATION_FAILED, payload suggestion near coords.
43. **Teleport Out of Bounds**
    - Coordinates outside -> clamped + warning in payload.
44. **Teleport Rate Limit**
    - Multiple requests <10s -> RATE_LIMITED.
45. **Teleport Wrong Role**
    - Role DevBuild only -> NOT_ALLOWED.
46. **Teleport Without Session**
    - Session expired -> SESSION_EXPIRED.
47. **Teleport to Non-Sandbox**
    - SANDBOX_ONLY.
48. **Teleport Audit**
    - Hash of from/to, zone, RequestId stored.
49. **Teleport Trace**
    - TraceId forwarded, server spans added.
50. **Teleport Event**
    - DebugLog with category=session/trace includes duration.

### Spawn Szenarien

51. **Spawn Success**
    - Template Dummy, Quantity=2, sandbox zone.
    - Response spawnIds length=2.
52. **Spawn Quantity High**
    - Quantity=6 -> SCHEMA_VALIDATION_FAILED.
53. **Spawn Template Forbidden**
    - Template not in whitelist -> INVALID_COMMAND.
54. **Spawn Rate Limit**
    - Repeated spawns -> RATE_LIMITED.
55. **Spawn TTL Default**
    - TTL omitted -> default 120s.
56. **Spawn TTL Custom**
    - TTL=30 -> accepted, enforced.
57. **Spawn Behavior Wander**
    - Behavior set to wander -> allowed if template supports.
58. **Spawn Cleanup Event**
    - On TTL expiry -> DebugLog spawn.despawned.
59. **Spawn Revoke**
    - Session end -> all spawns despawned.
60. **Spawn Audit**
    - TemplateId, Quantity, hashed coords logged.

### Trace & Profiling Szenarien

61. **Trace Emit**
    - Command `trace.emit` with span data.
    - Response Success, DebugLog category=trace.
62. **Profiler Chunk**
    - Server pushes profiler chunk via DebugLog category=profiler.
    - Data contains flamegraph sample (compressed).
63. **Trace Oversize**
    - Span attributes > limit -> TRACE_TOO_LARGE.
64. **Trace Without TraceId**
    - Server generates TraceId, returns via DebugLog with data.
65. **Profiler Rate Limit**
    - Large stream -> RATE_LIMITED, Data.sampling lowered.

### Comprehensive Test Matrix (Auszug)

| # | Feature | Input | Erwartet | ErrorCode |
|---|---------|-------|----------|-----------|
| 101 | session.start | devbuild=true, allowlist=true | Success, SessionId | – |
| 102 | session.start | devbuild=false | Fail | NOT_IN_DEVBUILD |
| 103 | session.start | allowlist=false | Fail | NOT_ALLOWED |
| 104 | session.extend | ttl=30 | Fail (min 60) | SCHEMA_VALIDATION_FAILED |
| 105 | metrics.snapshot | interval 200ms, twice in 2s | Second Fail | RATE_LIMITED |
| 106 | metrics.snapshot | ifNoneMatch up-to-date | notModified flag | – |
| 107 | logs.subscribe | category=server.log | Success | – |
| 108 | logs.subscribe | category=forbidden | Fail | INVALID_COMMAND |
| 109 | logs.unsubscribe | none active | Success (idempotent) | – |
| 110 | toggle.set | key overlay.hitbox, role=DevBuild | Success? requires GM/Admin -> NOT_ALLOWED | NOT_ALLOWED |
| 111 | toggle.set | key overlay.netstats, role=GM | Success | – |
| 112 | toggle.get | unknown key | Fail | INVALID_COMMAND |
| 113 | teleport | zone sandbox, coords valid | Success | – |
| 114 | teleport | zone live | Fail | SANDBOX_ONLY |
| 115 | teleport | rate burst | Fail | RATE_LIMITED |
| 116 | spawn | qty=3 | Success | – |
| 117 | spawn | qty=10 | Fail | SCHEMA_VALIDATION_FAILED |
| 118 | spawn | template forbidden | Fail | INVALID_COMMAND |
| 119 | spawn | non-sandbox zone | Fail | SANDBOX_ONLY |
| 120 | trace.emit | payload size >16KB | Fail | TRACE_TOO_LARGE |
| 121 | session.heartbeat | after expiry | Fail | SESSION_EXPIRED |
| 122 | session.start | resumeToken valid | Success resume | – |
| 123 | logs.stream | high volume | Sampling adjust | – |
| 124 | metrics.stream | backpressure | Sampling adjust, Data.sampling | – |
| 125 | toggle.set | persistent scope without flag | Fail | NOT_ALLOWED |
| 126 | session.end | closes streams | Logs stop | – |
| 127 | session.extend | revoked session | Fail | SESSION_REVOKED |
| 128 | teleport | missing facing | Success (defaults) | – |
| 129 | spawn | behavior invalid | Fail | SCHEMA_VALIDATION_FAILED |
| 130 | metrics.snapshot | export msgpack lz4 | Success, export payload | – |

### Erweiterte Ablaufbeschreibungen (Sequenzen)

#### Sandbox QA Teleport + Spawn Playbook

1. Start Session mit Capabilities `sandbox.teleport`, `sandbox.spawn`, `logs.stream`.
2. Setze Toggle `overlay.navmesh=true` (Audit).
3. Teleport zu Sandbox Zone 9001 (validate collision).
4. Warte auf `ZoneState` (extern).
5. Spawn 3 Dummies Template 1000 bei (50,75), TTL 120s.
6. Beobachte DebugLog `spawn.created`.
7. Führe Combat Rotation (extern), sammle Metrics Snapshot.
8. Nach TTL: empfange DebugLog `spawn.despawned`.
9. Session End, Streams schließen.

#### Netzwerk-Latency Debug Flow

1. Start Session mit `allowClientMetrics=true`.
2. Toggle `overlay.netstats=true`.
3. Subscribe Logs categories `[trace, metrics.delta]`.
4. Metrics Stream start interval 500ms.
5. Prüfe DebugLog entries für `rttMs`, `loss`.
6. Führe Lasttest (extern), beobachte Sampling.
7. Bei Backpressure: Server sendet Data.sampling -> Client UI zeigt an.
8. Stop Stream, Session End.

#### Trace Investigation Flow

1. Session Start.
2. Send `trace.emit` for client-side spans (optional).
3. Perform `metrics.snapshot` to capture context.
4. Subscribe Logs `trace`.
5. Server emits trace spans from Debug handlers.
6. Analyze Span durations; if > threshold, server tags `status=Error` and sends `DebugLog` level warn.

### Zusätzliche Tabellen (Detailvalidierung)

#### CommandName -> Capability Mapping

| Command | Capability | Role Min | Sandbox Required |
|---------|------------|----------|------------------|
| session.start | debug.session | DevBuild/Admin | Nein |
| session.extend | debug.session | DevBuild/Admin | Nein |
| session.end | debug.session | DevBuild/Admin | Nein |
| metrics.snapshot | metrics.read | DevBuild | Nein |
| metrics.stream.start | metrics.stream | DevBuild | Nein |
| metrics.stream.stop | metrics.stream | DevBuild | Nein |
| logs.subscribe | logs.stream | DevBuild | Nein |
| logs.unsubscribe | logs.stream | DevBuild | Nein |
| toggle.get | toggle.read | DevBuild | Nein |
| toggle.set | toggle.write | GM | Nein |
| teleport | sandbox.teleport | GM | Ja |
| spawn | sandbox.spawn | GM | Ja |
| trace.emit | trace.write | DevBuild | Nein |
| session.heartbeat | debug.session | DevBuild | Nein |

#### Audit-Felder je Command

| Command | Audit-Felder |
|---------|--------------|
| session.start | AccountId, DeviceHash, Capabilities, ttlSeconds, Result |
| session.extend | SessionId, ttlSeconds, Result |
| session.end | SessionId, Reason, Result |
| metrics.snapshot | SessionId, sampleDuration, includeClient, Result |
| metrics.stream.start | SessionId, intervalMs, sampling, Result |
| logs.subscribe | SessionId, categories, sampling, Result |
| toggle.set | SessionId, key, value hash, scope, Result |
| teleport | SessionId, zone hash, coord hash, Result |
| spawn | SessionId, templateId, quantity, coord hash, Result |
| trace.emit | SessionId, traceId, span count, Result |

#### Sanitizer-Regeln

| Pattern | Aktion |
|---------|-------|
| Email Regex | Hash + `[email]` Ersatz |
| Token-like (Bearer, JWT) | Entfernen, Placeholder `[token]` |
| UUID | Hash + Prefix |
| IP Adressen | Kürzen / Hash |
| Strings >256 | Truncate to 128 + hash suffix |
| Binary >8KB | Reject (TRACE_TOO_LARGE) |

#### Backpressure Regeln

| Bedingung | Maßnahme |
|-----------|----------|
| QueueDepth > 100 | Sampling halbieren |
| BytesOut > Limit | Drop low-level logs, keep errors |
| Client ack fehlt >5s | Pause stream, send Hinweis in DebugLog |
| Session near expiry | Warnung in DebugLog session expiring |

### Beispielhafte DebugLog Sequenzen

```
seq=1  category=session  level=info   msg="session.started" data={capabilities:[...], ttl=900}
seq=2  category=toggle.changed level=info msg="overlay.netstats=true" data={revisionId:2}
seq=3  category=trace    level=debug msg="DebugMetricsHandler" data={durationMs:3.4,status:"Ok"}
seq=4  category=metrics.delta level=info msg="tick" data={avg:16.6,p95:24.1,max:38.0}
seq=5  category=server.log level=info msg="Zone tick complete" data={entities:320,tickMs:17.2}
seq=6  category=metrics.delta level=info msg="network" data={bytesIn:1800,bytesOut:2200,loss:0.0}
seq=7  category=toggle.changed level=info msg="overlay.netstats=false" data={revisionId:3}
seq=8  category=session level=info msg="session.ended"
```

### Fehlerfall-Playbooks

- **NOT_ALLOWED**: Überprüfe Role, Capability, FeatureFlag, Allowlist. Stelle sicher, dass DevBuild-Flag gesetzt ist.
- **NOT_IN_DEVBUILD**: Stelle sicher, dass Client Build als Dev gekennzeichnet ist; Prod-Clients blockieren Debug.
- **SESSION_EXPIRED**: Starte Session neu. Prüfe Heartbeat.
- **RATE_LIMITED**: Warte `RetryAfterMs` ab, erhöhe Intervalle.
- **SCHEMA_VALIDATION_FAILED**: Prüfe Pflichtfelder, Wertebereiche.
- **SANDBOX_ONLY**: Wechsel in Sandbox-Zone, oder Command deaktivieren.
- **TRACE_TOO_LARGE**: Reduziere Payload, entferne große Attribute.
- **REPLAY**: Erhöhe RequestId, vermeide Wiederholung.

### Umfangreiche Testfälle (stichpunktartig)

- 50x Wiederholung: `metrics.snapshot` bei Idle Server -> keine Overruns.
- 50x Wiederholung: `metrics.snapshot` während CPU-Last -> Overrun Flags >0.
- 20x Teleport an Rand der Karte -> Koordinaten geclamped, keine Kollision.
- 20x Teleport in Wand -> SCHEMA_VALIDATION_FAILED mit suggestion.
- 10x Spawn + Despawn TTL -> DebugLog despawned nach TTL.
- 5x Spawn während Session End -> Despawn sofort nach End.
- Log Stream während Paketverlust (simuliert) -> Sequence Lücken erkannt, Client resync.
- Toggle Spam (20 req) -> Rate Limit greift nach 3-5 je nach Bucket.
- Heartbeat auslassen -> Session expired, weitere Requests abgelehnt.
- Session resume nach Reconnect -> Streams setzen Sequence fort.

### Kompatibilitäts-Hinweise

- Ältere Clients ohne SessionId müssen zuerst `session.start` senden; direkte `metrics.snapshot` Anfragen werden abgewiesen.
- Falls alte Clients `DebugCommand` ohne TraceId senden, Server generiert TraceId intern, kein Fehler.
- Admin-Kategorie 23 hat Überschneidungen (Teleport, Spawn). Debug-Teleport/Spawn sind Sandbox-gebunden; Admin-Varianten sind separat geregelt.

### Interaktion mit anderen Kategorien

- **Connection (00)**: DebugSession darf erst nach erfolgreichem Login gestartet werden; SessionId nicht im Handshake.
- **System (09)**: NetworkStats (903) liefert Basiswerte; Debug-Metrics erweitert Details.
- **Admin (23)**: Höhere Rechte; Debug nutzt sichere Sandbox, Admin kann Produktion beeinflussen.
- **Zone (01)**: Teleport/Spawn interagieren mit ZoneState/ZoneDelta; DebugResponse enthält keine ZoneState, nur Koordinaten.

### Ausführliche Validierungsregeln je Feld (stichpunktartig)

- `SessionId`: Guid, nicht Empty außer SessionStart.
- `RequestId`: ulong, >0, strikt monoton.
- `CommandName`: Regex `^[a-z0-9]+(\.[a-z0-9]+)*$`.
- `Args`: Schlüssel klein, Werte typisiert; keine null Pflichtfelder.
- `TargetZoneId`: ushort, muss SandboxFlag haben.
- `X/Y`: float, innerhalb ZoneBounds; server clamp.
- `Facing`: 0..360.
- `TemplateId`: whitelist.
- `Quantity`: 1..5.
- `TtlSeconds`: 10..3600 optional.
- `categories` (Logs): subset of allowed list.
- `sampling`: 0..1.
- `intervalMs`: >=200 für Streams.
- `ttlSeconds` (Session): 60..3600.
- `requestedCapabilities`: nur bekannte Strings, max 16 Einträge.
- `allowClientMetrics`: bool.
- `exportFormat`: json/msgpack.
- `compression`: none/lz4.

### Zusätzliche Beispiel-Code-Snippets

```csharp
// Heartbeat
var heartbeat = new DebugCommand
{
    RequestId = 99,
    SessionId = sessionId,
    CommandName = "session.heartbeat",
    Args = new Dictionary<string, object>(),
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

// Toggle Get/Set chain
var getToggle = new DebugCommand
{
    RequestId = 100,
    SessionId = sessionId,
    CommandName = "toggle.get",
    Args = new() { ["keys"] = new[] { "overlay.hitbox", "overlay.netstats" } },
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

var setToggle = new DebugCommand
{
    RequestId = 101,
    SessionId = sessionId,
    CommandName = "toggle.set",
    Args = new()
    {
        ["key"] = "overlay.netstats",
        ["value"] = true,
        ["scope"] = "session"
    },
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Beispiel-Audit-Einträge (pseudocode)

```
{ ts: 1704300000, account: 42, device: "hash...", cmd: "session.start", argsHash: "a1b2...", result: "success", sessionId: "...." }
{ ts: 1704300010, account: 42, cmd: "metrics.snapshot", argsHash: "c3d4...", result: "success", requestId: 2 }
{ ts: 1704300020, account: 42, cmd: "logs.subscribe", argsHash: "e5f6...", result: "success", categories: "server.log,trace" }
{ ts: 1704300050, account: 42, cmd: "teleport", argsHash: "abcd...", result: "failed", error: "SANDBOX_ONLY" }
{ ts: 1704300100, account: 42, cmd: "session.end", result: "success" }
```

### Langlebige Streams & Reconnect

- Streams behalten `lastSequence` pro Session.
- Client speichert `lastSequence` beim Disconnect.
- Beim Reconnect sendet Client `logs.resubscribe` mit `fromSequence`.
- Server fährt bei Erfolg fort, sendet DebugResponse + erster DebugLog mit `sequence=fromSequence+1`.
- Falls Sequence nicht mehr vorhanden (zu alt), Server antwortet `SCHEMA_VALIDATION_FAILED` und empfiehlt `full resubscribe`.

### Qualitätsmetriken & Alerting

- **Tick Overrun Alert**: Wenn p95 > 40ms über 3 Intervalle, DebugLog warn.
- **Network Loss Alert**: loss > 5% -> DebugLog warn + Payload suggestion (check net).
- **GC Pressure Alert**: gcPauseMs > 20 -> DebugLog warn.
- **Spawn Leak Alert**: SpawnCount > 20 -> RATE_LIMITED + DebugLog warn.
- **Missing Heartbeat Alert**: Keine Heartbeats > 2*interval -> DebugLog warn before expiry.

### Beispielhafte Client-UI Darstellungen

- **Session Panel**: zeigt SessionId, TTL countdown, capabilities.
- **Metrics Panel**: Graphen (tick, network, gc).
- **Logs Panel**: filter per Category/Level, Sequence badges.
- **Toggle Panel**: current state + revision; switch to send toggle.set.
- **Sandbox Panel**: Teleport form (zone, x, y, facing), Spawn form (template, qty, ttl).

### Sicherheits-Checkliste (Quick)

- [x] DevBuild enforced
- [x] Role/Allowlist enforced
- [x] Sandbox enforced (teleport/spawn)
- [x] Rate Limits configured
- [x] Audit enabled
- [x] Sanitizer enabled
- [x] Streams tied to Session
- [x] Request/Response correlation
- [x] No secrets/log PII
- [x] TTL/Heartbeat enforced

---

## 🧾 Langform Prüfkatalog (400+ konkrete Prüfungen)

Die folgende nummerierte Liste enthält konkrete, ausführbare Prüfungen für QA, Ops und Entwickler. Jeder Punkt beschreibt Erwartung, Preconditions und Erfolgskriterium. Der Katalog deckt funktionale, sicherheitsrelevante, performance- und observability-bezogene Aspekte ab.

1. Prüfe, dass `DebugCommand` mit `CommandName=session.start` in DevBuild funktioniert.
2. Prüfe, dass `DebugCommand` mit `CommandName=session.start` in Prod-Build abgewiesen wird.
3. Prüfe, dass `session.start` ohne Allowlist abgewiesen wird.
4. Prüfe, dass `session.start` Rate Limit (1/10s) greift.
5. Prüfe, dass `session.start` `Payload.sessionId` zurückgibt.
6. Prüfe, dass `session.start` `Payload.capabilities` zurückgibt.
7. Prüfe, dass `session.start` `ttlSeconds` clampen kann (min 60, max 3600).
8. Prüfe, dass `session.start` Audit-Log schreibt.
9. Prüfe, dass `session.extend` TTL verlängert.
10. Prüfe, dass `session.extend` mit zu kleinem ttl Fehler liefert.
11. Prüfe, dass `session.extend` mit zu großem ttl Fehler liefert.
12. Prüfe, dass `session.extend` Rate Limit eingehalten wird.
13. Prüfe, dass `session.extend` Audit-Log schreibt.
14. Prüfe, dass `session.end` Streams stoppt.
15. Prüfe, dass `session.end` Audit-Log schreibt.
16. Prüfe, dass `session.heartbeat` Session am Leben hält.
17. Prüfe, dass fehlende Heartbeats Session expirieren lassen.
18. Prüfe, dass `session.heartbeat` Rate Limit eingehalten wird.
19. Prüfe, dass `session.resume` mit gültigem `resumeToken` funktioniert.
20. Prüfe, dass `session.resume` mit falschem Token abgewiesen wird.
21. Prüfe, dass `metrics.snapshot` ohne Session abgewiesen wird.
22. Prüfe, dass `metrics.snapshot` mit gültiger Session funktioniert.
23. Prüfe, dass `metrics.snapshot` `Payload.tick` enthält.
24. Prüfe, dass `metrics.snapshot` `Payload.network` enthält.
25. Prüfe, dass `metrics.snapshot` `Payload.memory` enthält.
26. Prüfe, dass `metrics.snapshot` `Payload.serverHealth` enthält.
27. Prüfe, dass `metrics.snapshot` bei `includeClient=true` Client-Daten enthält.
28. Prüfe, dass `metrics.snapshot` Strings <256 Zeichen bleiben.
29. Prüfe, dass `metrics.snapshot` Rate Limit 1/5s greift.
30. Prüfe, dass `metrics.snapshot` `RevisionId` liefert.
31. Prüfe, dass `metrics.snapshot` mit `ifNoneMatch` 304-ähnliche Antwort liefert.
32. Prüfe, dass `metrics.snapshot` Audit-Log schreibt.
33. Prüfe, dass `metrics.stream.start` Intervall <200 abweist.
34. Prüfe, dass `metrics.stream.start` mit gültigem Intervall startet.
35. Prüfe, dass `metrics.stream.start` Sampling 0..1 validiert.
36. Prüfe, dass `metrics.stream.start` Rate Limit greift.
37. Prüfe, dass `metrics.stream.start` Audit-Log schreibt.
38. Prüfe, dass `metrics.stream.stop` Stream beendet.
39. Prüfe, dass `metrics.stream.stop` idempotent ist.
40. Prüfe, dass `metrics.stream.stop` Audit-Log schreibt.
41. Prüfe, dass `logs.subscribe` nur whitelisted Kategorien akzeptiert.
42. Prüfe, dass `logs.subscribe` Sampling 0..1 validiert.
43. Prüfe, dass `logs.subscribe` Rate Limit greift.
44. Prüfe, dass `logs.subscribe` Audit-Log schreibt.
45. Prüfe, dass `logs.unsubscribe` ohne aktive Subscription idempotent ist.
46. Prüfe, dass `logs.unsubscribe` Audit-Log schreibt.
47. Prüfe, dass DebugLog Sequence bei 1 startet nach Subscription.
48. Prüfe, dass DebugLog Sequence monoton ist.
49. Prüfe, dass Sequence-Gap erkannt wird (Client-seitig).
50. Prüfe, dass Log-Events PII sanitisieren (Email wird gehasht).
51. Prüfe, dass Log-Events Tokens entfernen.
52. Prüfe, dass Log-Events Strings >256 gekürzt sind.
53. Prüfe, dass Log-Events Data-Felder auf max Größe begrenzt sind.
54. Prüfe, dass Log-Events Backpressure-Sampling nutzen.
55. Prüfe, dass `toggle.get` nur bekannte Keys liefert.
56. Prüfe, dass `toggle.get` RevisionId liefert.
57. Prüfe, dass `toggle.set` nur GM/Admin erlaubt.
58. Prüfe, dass `toggle.set` Keys validiert.
59. Prüfe, dass `toggle.set` Value-Typen validiert.
60. Prüfe, dass `toggle.set` Scope validiert (session/character/persistent).
61. Prüfe, dass `toggle.set` Rate Limit greift.
62. Prüfe, dass `toggle.set` Audit-Log schreibt.
63. Prüfe, dass Toggle-Änderung DebugLog `toggle.changed` erzeugt.
64. Prüfe, dass Toggle-Änderung RevisionId hochzählt.
65. Prüfe, dass Toggle-Änderung reverted wird bei SessionEnd.
66. Prüfe, dass `teleport` nur Sandbox-Zonen akzeptiert.
67. Prüfe, dass `teleport` Koordinaten clamped.
68. Prüfe, dass `teleport` Facing optional ist.
69. Prüfe, dass `teleport` Collision prüft.
70. Prüfe, dass `teleport` Rate Limit greift.
71. Prüfe, dass `teleport` Audit-Log schreibt.
72. Prüfe, dass `teleport` Response Payload Koordinaten enthält.
73. Prüfe, dass `teleport` NOT_ALLOWED liefert ohne GM/Admin.
74. Prüfe, dass `teleport` NOT_IN_DEVBUILD liefert für Prod.
75. Prüfe, dass `teleport` SESSION_EXPIRED liefert bei abgelaufener Session.
76. Prüfe, dass `teleport` REPLAY liefert bei doppeltem RequestId.
77. Prüfe, dass `spawn` nur Sandbox-Zone akzeptiert.
78. Prüfe, dass `spawn` Quantity 1..5 validiert.
79. Prüfe, dass `spawn` TemplateId whitelist.
80. Prüfe, dass `spawn` Rate Limit 1/30s greift.
81. Prüfe, dass `spawn` Audit-Log schreibt.
82. Prüfe, dass `spawn` Response spawnIds liefert.
83. Prüfe, dass `spawn` TTL default 120s setzt.
84. Prüfe, dass `spawn` TTL clamp 10..3600.
85. Prüfe, dass `spawn` Behavior nur allowed Werte akzeptiert.
86. Prüfe, dass `spawn` NOT_ALLOWED liefert ohne GM/Admin.
87. Prüfe, dass `spawn` SESSION_EXPIRED liefert bei abgelaufener Session.
88. Prüfe, dass `spawn` REPLAY liefert bei doppeltem RequestId.
89. Prüfe, dass Spawn TTL Ablauf DebugLog `spawn.despawned` sendet.
90. Prüfe, dass SessionEnd alle Spawns despawnt.
91. Prüfe, dass `trace.emit` Payload <=16KB erzwingt.
92. Prüfe, dass `trace.emit` TraceId/SpanId optional setzt.
93. Prüfe, dass `trace.emit` Audit-Log schreibt.
94. Prüfe, dass `trace.emit` DebugLog category=trace erzeugt.
95. Prüfe, dass `trace.emit` Rate Limit greift.
96. Prüfe, dass Heartbeat fehlende Session Response SESSION_EXPIRED sendet.
97. Prüfe, dass RequestId 0 abgewiesen wird.
98. Prüfe, dass CommandName Regex enforced wird.
99. Prüfe, dass Args null Felder abgewiesen werden.
100. Prüfe, dass Args key casing (lowercase) enforced wird.
101. Prüfe, dass Payload Strings <256 sind.
102. Prüfe, dass Payload Maps <16KB sind.
103. Prüfe, dass DebugResponse immer RequestId setzt.
104. Prüfe, dass DebugResponse immer SessionId setzt.
105. Prüfe, dass DebugResponse Success bool enthält.
106. Prüfe, dass DebugResponse Timestamp gesetzt ist.
107. Prüfe, dass DebugResponse ErrorCode bei Fehler gefüllt ist.
108. Prüfe, dass DebugResponse RetryAfterMs bei RateLimit gesetzt ist.
109. Prüfe, dass DebugResponse RevisionId bei Stateful-Kommandos gesetzt wird.
110. Prüfe, dass `SessionId` Guid.Empty nur bei session.start erlaubt ist.
111. Prüfe, dass `Args` nicht null ist.
112. Prüfe, dass Server TraceId erzeugt falls fehlt.
113. Prüfe, dass Server SpanId optional setzt.
114. Prüfe, dass Logs nicht persistent gespeichert werden (außer Audit).
115. Prüfe, dass Audit Hash über Payload erstellt wird.
116. Prüfe, dass DeviceId nur gehasht gespeichert wird.
117. Prüfe, dass IP Adressen gekürzt/ gehasht werden.
118. Prüfe, dass ZoneId gehasht in Audit gespeichert wird.
119. Prüfe, dass Teleport alte Position gehasht speichert.
120. Prüfe, dass SpawnIds gehasht in Audit gespeichert werden.
121. Prüfe, dass FeatureFlag `debug.metrics.enabled` deaktiviert Requests blockt.
122. Prüfe, dass FeatureFlag `debug.logs.enabled` deaktiviert Requests blockt.
123. Prüfe, dass FeatureFlag `debug.sandbox.enabled` deaktiviert Teleport/Spawn blockt.
124. Prüfe, dass Capability-Liste in SessionResponse korrekt reflektiert.
125. Prüfe, dass `requestedCapabilities` unbekannte Werte ablehnt.
126. Prüfe, dass `allowClientMetrics=false` Client-Daten unterdrückt.
127. Prüfe, dass `allowClientMetrics=true` Client-Daten zulässt.
128. Prüfe, dass Backpressure Log-Drosselung DebugLog mit Hinweis sendet.
129. Prüfe, dass Metrics Stream bei Backpressure Sampling reduziert.
130. Prüfe, dass Metrics Stream Data.sampling Feld sendet.
131. Prüfe, dass Logs Stream Data.sampling Feld sendet.
132. Prüfe, dass Profiler Stream Data.sampling Feld sendet.
133. Prüfe, dass SessionId + RequestId + TraceId in Logs stehen.
134. Prüfe, dass Level in DebugLog immer gesetzt ist.
135. Prüfe, dass Category in DebugLog immer gesetzt ist.
136. Prüfe, dass Sequence in DebugLog immer gesetzt ist.
137. Prüfe, dass DebugLog Data optional ist und sanitized.
138. Prüfe, dass DebugLog Strings gekürzt werden.
139. Prüfe, dass DebugLog Sampling nie >1 ist.
140. Prüfe, dass DebugLog Sequence reset bei neuer Session.
141. Prüfe, dass Reconnect Sequence dort fortsetzt wo aufgehört.
142. Prüfe, dass logs.resubscribe funktioniert.
143. Prüfe, dass logs.resubscribe falsche Sequence abweist.
144. Prüfe, dass metrics.stream.stop ohne aktiven Stream idempotent ist.
145. Prüfe, dass toggle.set persistent Scope nur mit Flag erlaubt.
146. Prüfe, dass toggle.set persistent Scope ohne Flag NOT_ALLOWED.
147. Prüfe, dass toggle.get persistent Werte liefert falls vorhanden.
148. Prüfe, dass toggle.set scope unbekannt abgewiesen wird.
149. Prüfe, dass Teleport Facing außerhalb 0..360 abgewiesen wird.
150. Prüfe, dass Teleport X/Y NaN abgewiesen werden.
151. Prüfe, dass Teleport X/Y Infinity abgewiesen werden.
152. Prüfe, dass Spawn X/Y NaN abgewiesen werden.
153. Prüfe, dass Spawn X/Y Infinity abgewiesen werden.
154. Prüfe, dass Spawn TemplateId negativ abgewiesen wird.
155. Prüfe, dass Spawn Quantity 0 abgewiesen wird.
156. Prüfe, dass Spawn Behavior leer als default dummy genutzt wird.
157. Prüfe, dass Spawn Behavior unbekannt abgewiesen wird.
158. Prüfe, dass Spawn TTL negativ abgewiesen wird.
159. Prüfe, dass Spawn TTL >3600 abgewiesen wird.
160. Prüfe, dass Spawn TTL null -> default 120.
161. Prüfe, dass Session-End Streams closed confirmed durch Log.
162. Prüfe, dass Session-Revoke sendet DebugLog revoked.
163. Prüfe, dass RequestId nicht zurückgesetzt werden darf innerhalb Session.
164. Prüfe, dass RequestId Overflow nicht erreicht (ulong).
165. Prüfe, dass Args Map leer erlaubt, außer Pflichtfeldern.
166. Prüfe, dass Response Payload nicht mehr als 16KB groß wird.
167. Prüfe, dass Response Payload Strings sanitisiert sind.
168. Prüfe, dass Response ErrorMessage sanitized ist.
169. Prüfe, dass Response ErrorMessage nie secrets enthält.
170. Prüfe, dass Response ErrorCode nur bekannte Werte nutzt.
171. Prüfe, dass Response Success immer gesetzt ist.
172. Prüfe, dass Teleport Response Payload facing optional.
173. Prüfe, dass Spawn Response Payload TTL optional.
174. Prüfe, dass Logs subscribe categories case-insensitive? (entsprechend Schema).
175. Prüfe, dass Metrics Snapshot exportFormat invalid -> INVALID_COMMAND.
176. Prüfe, dass Metrics Snapshot compression invalid -> INVALID_COMMAND.
177. Prüfe, dass Metrics Snapshot sampleDuration <50 -> SCHEMA_VALIDATION_FAILED.
178. Prüfe, dass Metrics Snapshot sampleDuration >1000 -> SCHEMA_VALIDATION_FAILED.
179. Prüfe, dass Metrics Stream interval >5000 still allowed? (clamped?) validate clamp rules.
180. Prüfe, dass Metrics Stream stop ohne start -> idempotent success.
181. Prüfe, dass Logs unsubscribe ohne subscribe -> idempotent success.
182. Prüfe, dass Toggle set unknown key -> INVALID_COMMAND.
183. Prüfe, dass Toggle get unknown key -> INVALID_COMMAND.
184. Prüfe, dass Toggle set boolean for overlay.* requires bool else SCHEMA_VALIDATION_FAILED.
185. Prüfe, dass Toggle set number for verbose.* invalid -> SCHEMA_VALIDATION_FAILED.
186. Prüfe, dass Toggle set persistent flagged in Audit.
187. Prüfe, dass Teleport SANDBOX_ONLY audit flagged.
188. Prüfe, dass Spawn SANDBOX_ONLY audit flagged.
189. Prüfe, dass Logs subscribe with sampling 0 -> allowed (mute).
190. Prüfe, dass Logs subscribe with sampling >1 -> SCHEMA_VALIDATION_FAILED.
191. Prüfe, dass Logs subscribe with empty categories -> SCHEMA_VALIDATION_FAILED.
192. Prüfe, dass Logs subscribe with duplicate categories -> deduped.
193. Prüfe, dass Metrics stream start duplicates -> idempotent? else RATE_LIMITED.
194. Prüfe, dass Session TTL countdown verringert.
195. Prüfe, dass Session TTL Verlängerung addiert.
196. Prüfe, dass ResumeToken erneuert bei extend? (je nach impl) – dokumentiert falls ja.
197. Prüfe, dass DeviceHash nicht im Klartext.
198. Prüfe, dass Audit Hash reproduzierbar.
199. Prüfe, dass Audit Hash nicht rückwärts berechenbar.
200. Prüfe, dass Audit Storage rotation funktioniert.
201. Prüfe, dass Metrics Snapshot ohne Logs subscription möglich.
202. Prüfe, dass Logs subscription ohne Metrics Snapshot möglich.
203. Prüfe, dass Streams schließen bei Disconnect.
204. Prüfe, dass Streams wiederherstellbar bei Reconnect.
205. Prüfe, dass Reconnect ohne ResumeToken abgewiesen wird.
206. Prüfe, dass Heartbeat nach Reconnect akzeptiert wird.
207. Prüfe, dass Spawn/Teleport während Reconnect blockiert.
208. Prüfe, dass Session Start auf anderem Realm? (nicht erlaubt).
209. Prüfe, dass Session Start nach Logout blockiert.
210. Prüfe, dass Session Start vor Login blockiert.
211. Prüfe, dass Session Start nach Disconnect blockiert.
212. Prüfe, dass Teleport auf andere Shard? abgewiesen.
213. Prüfe, dass Spawn auf andere Shard? abgewiesen.
214. Prüfe, dass Metrics Snapshot DB/Redis Werte liefert.
215. Prüfe, dass Metrics Snapshot GC Werte liefert.
216. Prüfe, dass Metrics Snapshot CPU Werte liefert.
217. Prüfe, dass Metrics Snapshot Nullwerte vermeidet.
218. Prüfe, dass Metrics Snapshot negative Werte vermeidet.
219. Prüfe, dass Metrics Snapshot Entities Count optional? (if available).
220. Prüfe, dass Metrics Snapshot queueDepth optional? (if available).
221. Prüfe, dass Metrics Snapshot loss pct in 0..1.
222. Prüfe, dass Metrics Snapshot rtt positive.
223. Prüfe, dass Metrics Snapshot step times positive.
224. Prüfe, dass DebugLog Level valid values only.
225. Prüfe, dass DebugLog Category valid values only.
226. Prüfe, dass DebugLog Timestamp monotone? (non-decreasing).
227. Prüfe, dass DebugLog Sequence increments by 1.
228. Prüfe, dass DebugLog Data sanitized.
229. Prüfe, dass DebugLog Message sanitized.
230. Prüfe, dass DebugLog SpanId optional.
231. Prüfe, dass DebugLog TraceId optional.
232. Prüfe, dass DebugLog RequestId optional.
233. Prüfe, dass DebugLog for session.start uses RequestId.
234. Prüfe, dass DebugLog for toggle.changed includes revisionId.
235. Prüfe, dass DebugLog for spawn created includes spawnIds hashed.
236. Prüfe, dass DebugLog for teleport includes hashed coords.
237. Prüfe, dass DebugLog for metrics.delta includes sample window.
238. Prüfe, dass DebugLog for profiler includes sample size.
239. Prüfe, dass DebugLog for backpressure includes sampling info.
240. Prüfe, dass DebugLog for revoked includes reason.
241. Prüfe, dass DebugLog for expired includes reason timeout.
242. Prüfe, dass DebugLog for resubscribe includes fromSequence.
243. Prüfe, dass DebugLog for resume includes resumeToken presence.
244. Prüfe, dass DebugLog for heartbeat missing includes warning.
245. Prüfe, dass DebugLog for rate limit includes RetryAfter.
246. Prüfe, dass DebugResponse ErrorMessage localized? (if applicable) sanitized.
247. Prüfe, dass ErrorCode casing consistent uppercase underscore.
248. Prüfe, dass CommandName casing lowercase dot separated.
249. Prüfe, dass Arg keys lowercase dot separated.
250. Prüfe, dass Arg arrays length limited (<=32).
251. Prüfe, dass Arg dictionaries depth limited.
252. Prüfe, dass nested objects sanitized.
253. Prüfe, dass Spawn Behavior wander uses safe speed.
254. Prüfe, dass Spawn Behavior dummy has no AI updates.
255. Prüfe, dass Spawn Behavior passive has no aggro.
256. Prüfe, dass Spawn Entities flagged DebugOnly.
257. Prüfe, dass Teleport sets movement mode consistent.
258. Prüfe, dass Teleport triggers zone change properly.
259. Prüfe, dass Teleport does not persist beyond session.
260. Prüfe, dass Spawn removal cleans resources.
261. Prüfe, dass Metrics stream stop stops timers.
262. Prüfe, dass Logs stream stop stops producers.
263. Prüfe, dass Profiler stream stop stops sampling.
264. Prüfe, dass FeatureFlags live reload respected.
265. Prüfe, dass Capabilities stored in session.
266. Prüfe, dass Capabilities enforced per command.
267. Prüfe, dass Capabilities sanitized before response.
268. Prüfe, dass Capabilities not modifiable via client.
269. Prüfe, dass RequestId overflow handled (rollover not allowed).
270. Prüfe, dass SessionId uniqueness across accounts ensured.
271. Prüfe, dass Session TTL max enforced.
272. Prüfe, dass TTL extend cannot bypass max.
273. Prüfe, dass DevBuild flag validated once per connection.
274. Prüfe, dass DevBuild flag cannot be toggled mid-session.
275. Prüfe, dass DeviceId captured at login used for audit.
276. Prüfe, dass DeviceId hashed in logs.
277. Prüfe, dass AccountId present in audit.
278. Prüfe, dass CharacterId optional recorded when available.
279. Prüfe, dass ZoneId optional recorded when available.
280. Prüfe, dass ShardId optional recorded when available.
281. Prüfe, dass Teleport cross-shard prevented.
282. Prüfe, dass Spawn cross-shard prevented.
283. Prüfe, dass Toggle scope persistent cross-session prevented without flag.
284. Prüfe, dass Toggle scope session default.
285. Prüfe, dass Toggle revert works on session end.
286. Prüfe, dass Logs subscribe cannot subscribe to admin/private categories.
287. Prüfe, dass Metrics stream cannot include raw secrets.
288. Prüfe, dass Export payload sanitized (no secrets).
289. Prüfe, dass Export payload size limited.
290. Prüfe, dass Export payload compression optional.
291. Prüfe, dass Export payload decompressible.
292. Prüfe, dass Export payload integrity hashed? (if implemented).
293. Prüfe, dass RequestId reused returns cached response or REPLAY per spec.
294. Prüfe, dass Session revoked denies new commands.
295. Prüfe, dass Session revoked ends streams.
296. Prüfe, dass Session revoked sends DebugLog revoked.
297. Prüfe, dass Allowlist update immediate effect? (per config) test.
298. Prüfe, dass Role downgrade immediate effect.
299. Prüfe, dass FeatureFlag disable immediate effect.
300. Prüfe, dass RateLimit counters reset after window.
301. Prüfe, dass RateLimit uses per command bucket.
302. Prüfe, dass RateLimit per session separate from per account? (documented).
303. Prüfe, dass Metrics snapshot uses monotonic clock? (if available).
304. Prüfe, dass Trace spans include duration.
305. Prüfe, dass Trace spans include status.
306. Prüfe, dass Trace spans attributes sanitized.
307. Prüfe, dass Profiler chunk size limited.
308. Prüfe, dass Profiler chunk compression optional.
309. Prüfe, dass Profiler chunk sanitized.
310. Prüfe, dass Spawn TTL enforcement uses server time.
311. Prüfe, dass Session TTL enforcement uses server time.
312. Prüfe, dass Timestamp fields use Unix ms.
313. Prüfe, dass Timestamp not zero.
314. Prüfe, dass Timestamp monotone within session? (approx).
315. Prüfe, dass DebugResponse Timestamp near now.
316. Prüfe, dass DebugLog Timestamp near now.
317. Prüfe, dass Delay network adds not too big? (observability).
318. Prüfe, dass Teleport returns success quickly (<200ms) in sandbox.
319. Prüfe, dass Spawn returns success quickly (<300ms).
320. Prüfe, dass Metrics snapshot completes within sampleDuration + overhead.
321. Prüfe, dass Logs subscribe ack within 200ms.
322. Prüfe, dass Session start ack within 200ms.
323. Prüfe, dass Session end ack within 200ms.
324. Prüfe, dass Audit log writes asynchronously? (non-blocking) observation.
325. Prüfe, dass Audit log failure does not crash session.
326. Prüfe, dass Audit log failure flagged in DebugLog? (optional).
327. Prüfe, dass Teleport failure returns ErrorMessage sanitized.
328. Prüfe, dass Spawn failure returns ErrorMessage sanitized.
329. Prüfe, dass Metrics snapshot failure returns ErrorMessage sanitized.
330. Prüfe, dass Logs subscribe failure returns ErrorMessage sanitized.
331. Prüfe, dass Toggle failure returns ErrorMessage sanitized.
332. Prüfe, dass Trace failure returns ErrorMessage sanitized.
333. Prüfe, dass ErrorMessage does not leak internal stack traces.
334. Prüfe, dass ErrorMessage length limited.
335. Prüfe, dass Logs do not contain credentials.
336. Prüfe, dass Logs do not contain session tokens.
337. Prüfe, dass Logs do not contain IP full address.
338. Prüfe, dass Logs do not contain hardware identifiers raw.
339. Prüfe, dass Logs do not contain chat content.
340. Prüfe, dass Logs do not contain user PII.
341. Prüfe, dass Metrics do not contain user identifiers.
342. Prüfe, dass Metrics do not contain item ids? (if not needed) sanitized.
343. Prüfe, dass Spawn does not drop loot.
344. Prüfe, dass Spawn cannot be looted.
345. Prüfe, dass Spawn cannot attack unless behavior states? (dummy).
346. Prüfe, dass Teleport does not bypass anti-cheat outside sandbox.
347. Prüfe, dass DevBuild flag cannot be spoofed? (server-trusted).
348. Prüfe, dass Allowlist cannot be bypassed.
349. Prüfe, dass Role cannot be escalated via command.
350. Prüfe, dass Capabilities cannot be escalated via command.
351. Prüfe, dass FeatureFlags cannot be toggled via debug commands.
352. Prüfe, dass Session cannot be hijacked with different account.
353. Prüfe, dass Session cannot be hijacked with different device.
354. Prüfe, dass Session cannot be hijacked with different IP? (if enforced).
355. Prüfe, dass ResumeToken cannot be guessed (UUID).
356. Prüfe, dass ResumeToken expires with Session TTL.
357. Prüfe, dass ResumeToken invalid after revoke.
358. Prüfe, dass ResumeToken invalid after end.
359. Prüfe, dass Streams stop on revoke.
360. Prüfe, dass Streams stop on end.
361. Prüfe, dass Streams stop on expire.
362. Prüfe, dass Commands rejected after expire.
363. Prüfe, dass Commands rejected after revoke.
364. Prüfe, dass Commands rejected after end.
365. Prüfe, dass DebugLog stops after end.
366. Prüfe, dass DebugLog stops after expire.
367. Prüfe, dass DebugLog stops after revoke.
368. Prüfe, dass Teleport/spawn commands cannot re-open session.
369. Prüfe, dass session.start cannot be nested (only one active per connection).
370. Prüfe, dass session.start second session returns existing? or denies (per design).
371. Prüfe, dass logs.subscribe returns same subscription id if already subscribed? (dedup).
372. Prüfe, dass metrics.stream.start returns error if already streaming.
373. Prüfe, dass toggle.set returns revision even if same value? (per design).
374. Prüfe, dass toggle.set dedup detection? (optional).
375. Prüfe, dass spawn/responses preserve order.
376. Prüfe, dass teleport/responses preserve order.
377. Prüfe, dass request/response correlation reliable.
378. Prüfe, dass correlation id used in server logs.
379. Prüfe, dass correlation id used in trace.
380. Prüfe, dass correlation id used in audit.
381. Prüfe, dass debug messages not forwarded to other players.
382. Prüfe, dass debug messages not counted in gameplay analytics.
383. Prüfe, dass debug messages counted in audit analytics.
384. Prüfe, dass debug messages not persisted beyond TTL (except audit).
385. Prüfe, dass server config to disable debug globally works.
386. Prüfe, dass server config to disable sandbox teleport works.
387. Prüfe, dass server config to disable sandbox spawn works.
388. Prüfe, dass server config to disable metrics export works.
389. Prüfe, dass server config to disable profiler stream works.
390. Prüfe, dass profiler stream only allowed for Admin? (enforce).
391. Prüfe, dass log categories accessible per capability.
392. Prüfe, dass metrics stream accessible per capability.
393. Prüfe, dass toggles accessible per capability.
394. Prüfe, dass teleport/spawn accessible per capability.
395. Prüfe, dass trace emit accessible per capability.
396. Prüfe, dass session management accessible per capability.
397. Prüfe, dass invalid capability list results in NOT_ALLOWED.
398. Prüfe, dass server rejects unknown MessageType in this range (robustness).
399. Prüfe, dass MessagePack keys stable.
400. Prüfe, dass DTO Key(0) immer Type.
401. Prüfe, dass DTO Keys aufsteigend ohne Lücken? (per file).
402. Prüfe, dass DTO Types in Shared Library liegen.
403. Prüfe, dass DTO Interfaces implementiert sind.
404. Prüfe, dass DTO Attribute `[MessagePackObject]` gesetzt.
405. Prüfe, dass DTO Key(0) nicht ignoriert.
406. Prüfe, dass Server Validation bei Key mismatch fehlerfrei.
407. Prüfe, dass build pipeline Debug messages optional? (excluded from production clients).
408. Prüfe, dass client UI für Debug nur in DevBuild sichtbar.
409. Prüfe, dass client UI Teleport/Spawn nur Sandbox anzeigt.
410. Prüfe, dass client UI Toggles listet mit current revisions.
411. Prüfe, dass client UI Logs filterbar.
412. Prüfe, dass client UI Metrics graph anzeigt.
413. Prüfe, dass client UI Session TTL anzeigt.
414. Prüfe, dass client UI Response Errors anzeigt.
415. Prüfe, dass client UI RateLimit Hinweise anzeigt.
416. Prüfe, dass client UI Sampling Hinweise anzeigt.
417. Prüfe, dass client UI Reconnect Flow unterstützt.
418. Prüfe, dass client UI ResumeToken speichert sicher.
419. Prüfe, dass client UI SessionId speichert sicher.
420. Prüfe, dass client UI RequestId monotone steigert.
421. Prüfe, dass client UI Backoff bei RateLimit respektiert.
422. Prüfe, dass client UI Teleport Koordinaten validiert.
423. Prüfe, dass client UI Spawn Quantity validiert.
424. Prüfe, dass client UI Spawn Template wählt aus whitelist.
425. Prüfe, dass client UI FeatureFlags liest (falls exposed).
426. Prüfe, dass client UI Logs unsubscribes on session end.
427. Prüfe, dass client UI Metrics stop on session end.
428. Prüfe, dass client UI toggles revert displayed on end.
429. Prüfe, dass client UI handles SESSION_EXPIRED gracefully.
430. Prüfe, dass client UI handles SESSION_REVOKED gracefully.
431. Prüfe, dass client UI handles NOT_ALLOWED gracefully.
432. Prüfe, dass client UI handles RATE_LIMITED gracefully.
433. Prüfe, dass client UI handles SANDBOX_ONLY gracefully.
434. Prüfe, dass client UI handles INVALID_COMMAND gracefully.
435. Prüfe, dass client UI handles TRACE_TOO_LARGE gracefully.
436. Prüfe, dass client UI handles REPLAY gracefully.
437. Prüfe, dass QA Playbook vollständig dokumentiert.
438. Prüfe, dass Operator Playbook vollständig dokumentiert.
439. Prüfe, dass Incident Response bei Debug-Missbrauch dokumentiert.
440. Prüfe, dass Telemetry dashboards (Dev) für Debug vorhanden.
441. Prüfe, dass Logs von Debug getrennt von Prod Logs gehalten werden.
442. Prüfe, dass Debug toggles keine Performance-Degradation in Prod verursachen (weil disabled).
443. Prüfe, dass Sandbox Ressourcen isoliert.
444. Prüfe, dass Sandbox nicht Produktionsdaten enthält.
445. Prüfe, dass Spawn Entities keine XP/Rewards geben.
446. Prüfe, dass Teleport nicht Stats/Quests beeinflusst.
447. Prüfe, dass Debug Commands nicht im Replay/AntiCheat zählen.
448. Prüfe, dass Debug Commands in AntiCheat Whitelist nur in Sandbox.
449. Prüfe, dass Debug Commands Logging nicht overflowt.
450. Prüfe, dass Debug Commands invalid types (e.g. string statt int) abweisen.
451. Prüfe, dass Debug Commands mit zusätzlichen unerwarteten Feldern abweisen? (oder ignorieren per schema).
452. Prüfe, dass Debug Commands mit leerem CommandName abweisen.
453. Prüfe, dass Debug Commands mit null Args abweisen.
454. Prüfe, dass Debug Commands mit zu vielen Args abweisen.
455. Prüfe, dass Debug Commands mit nested objects tiefe limitieren.
456. Prüfe, dass Debug Commands mit arrays tiefe limitieren.
457. Prüfe, dass Teleport Response includes RequestId.
458. Prüfe, dass Spawn Response includes RequestId.
459. Prüfe, dass Metrics Response includes RequestId.
460. Prüfe, dass Logs events include SessionId.
461. Prüfe, dass Logs events include Sequence.
462. Prüfe, dass Logs events include Category.
463. Prüfe, dass Logs events include Level.
464. Prüfe, dass Logs events include Timestamp.
465. Prüfe, dass Logs events sanitized Data.
466. Prüfe, dass Response ErrorMessage sanitized even for server exceptions.
467. Prüfe, dass Debug pipelines covered by integration tests.
468. Prüfe, dass MessagePack contract tests exist? (manuell).
469. Prüfe, dass docs reflect actual MessageType values.
470. Prüfe, dass docs reflect sandbox requirement.
471. Prüfe, dass docs reflect access levels.
472. Prüfe, dass docs reflect rate limits.
473. Prüfe, dass docs reflect audit rules.
474. Prüfe, dass docs reflect sanitization rules.
475. Prüfe, dass docs reflect toggle keys.
476. Prüfe, dass docs reflect metrics fields.
477. Prüfe, dass docs reflect log categories.
478. Prüfe, dass docs reflect flows.
479. Prüfe, dass docs reflect error codes.
480. Prüfe, dass docs reflect payload schemas.
481. Prüfe, dass docs reflect revision logic.
482. Prüfe, dass docs reflect resume logic.
483. Prüfe, dass docs reflect backpressure.
484. Prüfe, dass docs reflect sandbox clamp.
485. Prüfe, dass docs reflect sampling.
486. Prüfe, dass docs reflect TTL.
487. Prüfe, dass docs reflect export formats.
488. Prüfe, dass docs reflect compression options.
489. Prüfe, dass docs reflect optional fields.
490. Prüfe, dass docs reflect required fields.
491. Prüfe, dass docs reflect sequence rules.
492. Prüfe, dass docs reflect capability mapping.
493. Prüfe, dass docs reflect relation to Admin category.
494. Prüfe, dass docs reflect relation to Connection category.
495. Prüfe, dass docs reflect relation to System category.
496. Prüfe, dass docs reflect relation to Zone category.
497. Prüfe, dass docs reflect no secrets policy.
498. Prüfe, dass docs reflect rate limit RetryAfter usage.
499. Prüfe, dass docs reflect role gates.
500. Prüfe, dass docs reflect allowlist gates.

---

**Letzte Aktualisierung**: 2026-01-03  
**Version**: 1.0.0

Source: docs/03-messages/49-debug.md
