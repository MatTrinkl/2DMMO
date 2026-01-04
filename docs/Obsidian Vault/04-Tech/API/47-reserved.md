# 🧩 Reserved Messages (4700-4799)

**Kategorie:** 47  
**Range:** 4700-4799  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Regeln für Reservierungen](#-regeln-für-reservierungen)
- [🔢 Range-Policy (4700–4799)](#-range-policy-4700–4799)
- [✅ Request/Response Policy](#-requestresponse-policy)
- [🔄 Kompatibilität & Deprecation](#-kompatibilität--deprecation)
- [📩 Aktueller Stand in MessageType.cs](#-aktueller-stand-in-messagetypecs)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧱 Reserved Slots](#-reserved-slots)
- [📜 Governance & Activation Checklist](#-governance--activation-checklist)
- [🧭 Routing, Telemetrie & Monitoring](#-routing-telemetrie--monitoring)
- [🔐 Security & Abuse-Prevention](#-security--abuse-prevention)
- [📦 Payload-Konventionen für zukünftige Aktivierungen](#-payload-konventionen-für-zukünftige-aktivierungen)
- [🧪 Test- und QA-Strategie bei Aktivierung](#-test--und-qa-strategie-bei-aktivierung)
- [⏳ Migrations- und Kompatibilitätsplan](#-migrations--und-kompatibilitätsplan)
- [🧮 Belegungs- und Kapazitätsplanung](#-belegungs--und-kapazitätsplanung)
- [📈 Observability-Playbook](#-observability-playbook)
- [🧩 Schnittstellen- und DTO-Richtlinien](#-schnittstellen--und-dto-richtlinien)
- [🧰 Developer Workflow](#-developer-workflow)
- [🪪 Naming & Comment Conventions](#-naming--comment-conventions)
- [🔁 Versioning & Rollback Szenarien](#-versioning--rollback-szenarien)
- [📎 Anhang (MessageType Enum Updates)](#-anhang-messagetype-enum-updates)

---

## 📋 Überblick

Diese Kategorie reserviert den Message-Type-Space **4700–4799**. Er dient als strategisches Puffer-Fenster für zukünftige Features, die **nicht** in bestehende thematische Blöcke passen oder kurzfristig aktiviert werden müssen, ohne bestehende Ranges umzustrukturieren. Alle IDs sind aktuell ungenutzt und dürfen nur nach Freigabe durch die Maintainer belegt werden. Ziel ist:

- **Versioning-Sicherheit:** spätere Aktivierung ohne Kollision mit produktiven IDs.
- **Governance:** klare Regeln für Naming, Dokumentation, Request/Response-Paare.
- **Rollout-Fähigkeit:** kontrollierte Aktivierung mit Migrationspfad und Backward-Compatibility-Plan.
- **Transparenz:** Dokumentierte Reserved-Slots mit geplantem Zweck oder Kommentar.

Diese Datei folgt dem etablierten Stil der Nachrichten-Dokumentation (siehe `00-connection.md` und `01-zone.md`) und ersetzt Placeholder-Notizen durch konkrete, umsetzbare Richtlinien.

---

## 🧠 Regeln für Reservierungen

- **Keine Silent-Nutzung:** Eine ID aus 4700–4799 darf nur nach:
  - Eintrag im `MessageType` Enum.
  - Vollständiger Doku-Erweiterung in dieser Datei (Message-Abschnitt oder Reserved-Tabellen-Update).
  - Code-Implementierung (Message-Klasse, Tests, Serializer-Checks).
- **Naming-Konvention:** `PascalCase`, präfixfrei, klarer Zweck. Beispiel: `ReservedPilotFeature` ist verboten; stattdessen `FeaturePilotRequest`.
- **Kommentar-Konvention:** Im Enum Eintrag immer ein kurzer Kommentar mit Status (`// RESERVED - <Kurzbegründung>` oder `// OBSOLETE - <Ersetzt durch …>`).
- **Request/Response-Pflicht:** Jede aktivierte Client/Server-Request-ID muss eine Response im selben Range erhalten (Ausnahme: reine Server-Broadcasts mit klarer Begründung und Monitoring).
- **Obsolete vs Reserved:**
  - **Reserved:** Noch nie produktiv verwendet, frei belegbar nach Governance.
  - **Obsolete:** Produktiv gewesen, aber ersetzt; nur reaktivieren, wenn Migrationspfad dokumentiert ist.
- **Keine Mehrfachbelegung:** Jede ID exakt ein Semantik-Owner. Kein Re-Use für andere Zwecke ohne explizite Obsolete-Markierung und Deprecation-Plan.

---

## 🔢 Range-Policy (4700–4799)

- **Scope:** General Purpose / Rapid Response Slot für kurzfristige Features (z. B. Event-Instrumentation, Debug-Probes, Transition-Messages).
- **Granularität:** 100 IDs. Empfohlen: Blöcke zu je 10 für thematische Gruppen (z. B. 4700–4709 Observability, 4710–4719 Experimentation).
- **Allocation-Prozess:**
  1. Bedarf schriftlich festhalten (Feature-Ticket).
  2. Slot aus Tabelle „Reserved Slots“ wählen oder neuen Slot anlegen.
  3. Maintainer-Freigabe einholen.
  4. Enum + Doku + Code + Tests + Monitoring in einem PR liefern.
- **Routing:** Dispatcher nutzt `category = MessageType / 100`. Category 47 muss in den Routing-Tabellen als **disabled by default** geführt werden, bis eine konkrete Message aktiviert ist.
- **Telemetry:** Neue IDs sofort in Metriken/Logs whitelisten, um „unknown type“ Noise zu vermeiden.

---

## ✅ Request/Response Policy

- **Symmetrie:** Für jeden Request in 4700–4799 existiert eine Response in derselben Kategorie, sofern die Message nicht als Broadcast markiert ist.
- **Namensschema:**
  - `XxxRequest` ↔ `XxxResponse`
  - `XxxCommand` ↔ `XxxResult`
  - Broadcasts ohne Response: `XxxEvent`, `XxxBroadcast`
- **Timeout & Error Codes:** Jeder Request definiert Timeout, Retries, ErrorCode-Set. ErrorCodes werden im Response-Dokumentationsteil gelistet.
- **Idempotenz:** Falls relevant (z. B. Retries), klar kennzeichnen und serverseitig absichern.
- **Backward-Compatibility:** Neue Responses dürfen keine breaking Änderungen an bestehenden Clients erzwingen; Feature-Gates nutzen.

---

## 🔄 Kompatibilität & Deprecation

- **Activation Path:** Reserved → Implemented (feature-flagged) → Stable → (optional) Obsolete.
- **Deprecation Flow:**
  1. Markiere Enum-Eintrag mit `[Obsolete("…")]`.
  2. Dokumentiere Ersatz-Message und Migrationsschritte in dieser Datei.
  3. Setze Telemetrie-Alert auf verbleibende Nutzung.
  4. Entferne Code erst nach Nullnutzung + mindestens eine Minor-Version.
- **Wire-Kompatibilität:** Keine Änderung an bestehenden Key-Indizes der MessagePack-Objekte. Neue Felder nur anhängen (höhere Keys).
- **Reactivation:** Obsolete IDs dürfen nicht ohne neuen Migrationsplan reaktiviert werden.

---

## 📩 Aktueller Stand in MessageType.cs

| ID  | Enum Name | Status | Kommentar |
|-----|-----------|--------|-----------|
| 4700-4799 | *(keine Einträge)* | Reserved | Komplett ungenutzt, siehe Tabelle unten |

**Analyse:** In `MessageType.cs` existieren aktuell **keine** Einträge im Bereich 4700–4799. Es liegen weder aktive noch obsolete Nachrichten vor. Alle Slots gelten als frei, müssen aber den Governance-Regeln folgen.

---

## 🗑️ Obsolete Messages

Derzeit keine Obsolete-Einträge in 4700–4799. Wenn zukünftig Messages aus diesem Range abgeschaltet werden, müssen sie hier mit Ersatz-Referenz und Deprecation-Plan gelistet werden.

---

## 🧱 Reserved Slots

| ID-Bereich | Enum Name (geplant) | Status | Kommentar/Geplant für |
|------------|---------------------|--------|-----------------------|
| 4700-4709 | – | Reserved | Schnellschalter für Observability/Diagnostics (z. B. Trace-Injection) |
| 4710-4719 | – | Reserved | Experimentelle Feature-Toggles mit Client-Acknowledge |
| 4720-4729 | – | Reserved | Lightweight Migration Helpers (Schema/State Sync) |
| 4730-4739 | – | Reserved | Event-Pipeline Bridge (Client → Gateway → Zone) |
| 4740-4749 | – | Reserved | LiveOps Tools (Temporäre Notices, Limited-Time Actions) |
| 4750-4759 | – | Reserved | Risky Ops (Kill-Switch, Rate-Limit Push) – nur Maintainer |
| 4760-4769 | – | Reserved | Debug/Profiling Payloads (High-Freq, Short-Lived) |
| 4770-4779 | – | Reserved | A/B/C Experimente mit deterministischer Bucket-Zuordnung |
| 4780-4789 | – | Reserved | Content Delivery Hints (Asset Preload, Prefetch Signals) |
| 4790-4799 | – | Reserved | Future-proof Buffer für Notfall-Hotfix-Messages |

**Hinweis:** Solange kein Enum-Eintrag existiert, bleiben die Slots logisch reserviert und dürfen nicht implizit benutzt werden.

---

## 📜 Governance & Activation Checklist

### Wer darf freigeben?
- Maintainer-Gruppe der Networking/Doku (mind. 2 Approvals).
- Bei sicherheitsrelevanten Nachrichten zusätzlich Security-Review.

### Prüfungen vor Belegung
1. **Collision-Check:** ID im `MessageType` frei, keine Überschneidung mit laufenden PRs.
2. **Request/Response-Paar:** Falls Request, Response gleichzeitig planen.
3. **Doku:** Dieser Abschnitt + spezifischer Message-Abschnitt aktualisiert.
4. **Tests:** Serializer-Roundtrip, Routing-Test, ggf. Integrationstest.
5. **Monitoring:** Neue IDs in Metrics/Logging-Whitelist aufnehmen.
6. **Rollout-Plan:** Feature-Flag, Migrationspfad, Fallback/Abort-Plan.
7. **Security:** Input-Validation, Auth-Zwang, Rate-Limits definieren.

### Activation Checklist (umsetzbar)
| Schritt | Muss | Beschreibung |
|---------|------|--------------|
| 1 | ✅ | Enum-Eintrag in `MessageType.cs` im Bereich 4700–4799 hinzufügen |
| 2 | ✅ | Message-Klasse(n) erstellen inkl. `[MessagePackObject]`, `[Key]`, `[NetworkMessage]` |
| 3 | ✅ | Request/Response-Paar dokumentieren (Payload-Tabellen, Error Codes, Flow) |
| 4 | ✅ | Tests: Serializer-Roundtrip + Routing/Handler-Test |
| 5 | ✅ | Feature-Flag + Config-Default OFF |
| 6 | ✅ | Telemetrie-Whitelist + Log-Rate-Limitierung |
| 7 | ✅ | Rollout-Plan mit Monitoring und Rollback-Strategie |
| 8 | ✅ | Backward-Compatibility Note im jeweiligen Abschnitt |

---

## 🧭 Routing, Telemetrie & Monitoring

- **Dispatcher-Regel:** Kategorie 47 ist standardmäßig deaktiviert; jede neue Message muss explizit im Router registriert werden.
- **Metrics:** Für neue IDs unmittelbar Counter/Histogram hinzufügen:
  - `messages_received_total{type=470X}`
  - `messages_invalid_total{type=470X,reason=…}`
  - `message_latency_ms{type=470X}` (für Request/Response)
- **Logging:** Nur strukturierte Logs ohne Payload-Sensitivdaten. Kein PII.
- **Alerts:** Unknown-Message-Alert in Gateway/Zone sollte 0 sein; Reservierungs-Aktivierung muss Log-Spam vermeiden.

---

## 🔐 Security & Abuse-Prevention

- **Auth-Zwang:** Standardmäßig 🔒 Authenticated, es sei denn explizit anders dokumentiert.
- **Rate Limits:** Default: 10/min pro Verbindung, enger setzen bei Debug/LiveOps Nachrichten.
- **Validation:** MessagePack DTOs validieren (null-checks, Bounds).
- **Replay-Schutz:** Sequence/Nonce verwenden, falls sicherheitsrelevante Aktion.
- **Tamper-Evident:** Kritische Messages signieren/hashten falls sie Client-seitig Entscheidungen triggern.

---

## 📦 Payload-Konventionen für zukünftige Aktivierungen

- **MessagePack Keys:** Start bei 0 für `Type`, aufsteigend, keine Lücken bei Pflichtfeldern.
- **Nullable Felder:** Nur wenn semantisch „optional“; nicht für Pflicht-Felder.
- **Enums:** Byte-basiert, klarer Wertebereich, dokumentierte Bedeutung.
- **Timestamps:** Unix ms (long). Kein Client-Zeitvertrauen.
- **IDs:** Guid für Entities, ulong/long für Account/Character, ushort für Zonen/kleine IDs.
- **Compression:** Nur wenn global Feature aktiv, niemals exklusiv pro Message in diesem Range ohne Doku.

---

## 🧪 Test- und QA-Strategie bei Aktivierung

- **Unit:** Serializer Roundtrip (MessagePack), Validation.
- **Integration:** Gateway ↔ Zone Routing, Feature-Flag OFF/ON.
- **Load:** Burst-Test auf Rate-Limits (pro Typ).
- **Backward-Compat:** Mixed-Version-Test (alter Client, neuer Server) → sollte Unknown-Message sauber handlen.
- **Chaos:** Fault-Injection (Drop/Delay) für Request/Response Timeout-Pfade.

---

## ⏳ Migrations- und Kompatibilitätsplan

- **Phased Rollout:** Staged activation (internal → canary → public).
- **Graceful Fallback:** Unknown-Message handling muss Client-safe sein (ignore + telemetry).
- **Schema Evolution:** Nur additive Felder; nie Keys recyceln.
- **Kill-Switch:** Für jede aktivierte ID gibt es einen Runtime-Toggle zum Abschalten.
- **Data Retention:** Logs/Metrics für neue IDs mind. 30 Tage behalten für Regression-Analyse.

---

## 🧮 Belegungs- und Kapazitätsplanung

- **Default Free Space:** 100 IDs. Reserve-Table oben zeigt Segmentierung.
- **Belegungsschritte:** Pro aktivierter Message reduziert sich die verfügbare Anzahl; Tabelle muss aktualisiert werden.
- **Kontingente:** Max 5 gleichzeitige „experimentelle“ Messages (Feature-Flagged) in 4700–4799, um Routing/Observability schlank zu halten.
- **Rebalancing:** Falls Range voll, neuen Reserve-Range definieren und dokumentieren (z. B. 48xx).

---

## 📈 Observability-Playbook

- **Dashboards:** Pro Message-Type Latenz, Error-Rate, Volume.
- **Sampling:** Bei Debug/Profiling-Nachrichten Sampling auf ≤10%, sonst Datenflut.
- **Tracing:** Optional Trace-Header übernehmen (`TraceId`, `SpanId`) falls Feature-Flag aktiv.
- **Alerts Schwellen:** 
  - ErrorRate > 1% für 5 Minuten → Warn
  - ErrorRate > 5% für 2 Minuten → Critical + Kill-Switch evaluieren

---

## 📚 Subrange-spezifische Szenarien (Beispiele)

### 4700–4709: Observability / Diagnostics
- **Zweck:** Temporäre Instrumentation ohne bestehende Protokolle zu beeinflussen.
- **Beispiele:**
  - `4700` ProbeRequest ↔ `4701` ProbeResponse für Roundtrip-Latenz mit Payload-Größe n.
  - `4702` TraceInjectBroadcast zum Verteilen von Trace-IDs an Clients.
- **Sicherheitsanforderungen:** Nur Authenticated Clients; Rate-Limit 1/s; kein PII im Payload.
- **Validierung:** Max Payload 512 Bytes; Pflichtfelder `TraceId`, `Timestamp`.

### 4710–4719: Experimentation / Feature Toggles
- **Zweck:** A/B/C Tests mit deterministischer Bucket-Zuordnung.
- **Beispiele:**
  - `4710` FeatureConfigRequest ↔ `4711` FeatureConfigResponse (Bucket + Variation).
  - `4712` ExperimentEventBroadcast für Live-Metriken.
- **Sicherheitsanforderungen:** Auth + Signature falls clientseitige Entscheidungen sicherheitskritisch sind.
- **Validierung:** BucketId (uint32), Variation (byte), TTL (uint32).

### 4720–4729: Migration Helpers
- **Zweck:** Schema- oder State-Migrationsschritte ohne bestehenden Traffic zu stören.
- **Beispiele:**
  - `4720` SchemaHashRequest ↔ `4721` SchemaHashResponse zur Konsistenzprüfung.
  - `4722` DataReconcileRequest ↔ `4723` DataReconcileResponse für Incremental Sync.
- **Sicherheitsanforderungen:** Auth + Replay-Schutz (Nonce).
- **Validierung:** Hash-Länge fix, Sequence monotonic.

### 4730–4739: Event-Pipeline Bridge
- **Zweck:** Übergangs-Events zwischen Gateway und Zone für temporäre Kampagnen.
- **Beispiele:**
  - `4730` BridgeEventEnvelope (Broadcast) mit dediziertem EventType-Enum.
  - `4731` BridgeAck für Delivery-Bestätigung.
- **Sicherheitsanforderungen:** Auth, Rate-Limit 5/min, dedizierte Logger-Kategorie.

### 4740–4749: LiveOps Tools
- **Zweck:** Kurzfristige Operator-Aktionen (z. B. Hinweis-Banner, Soft-Nudges).
- **Beispiele:**
  - `4740` LiveOpsNotice (Broadcast), optional mit Expiry.
  - `4741` LiveOpsAcknowledge (Client → Server) für Sichtbarkeits-Tracking.
- **Sicherheitsanforderungen:** Admin-signierte Payloads, kein Client-Eingriff in Game-State.

### 4750–4759: Risky Ops / Kill Switch
- **Zweck:** Sofortiges Abschalten oder Begrenzen von Subsystemen.
- **Beispiele:**
  - `4750` KillSwitchCommand ↔ `4751` KillSwitchResult.
  - `4752` RateLimitPush (Server → Client) mit neuen Limits.
- **Sicherheitsanforderungen:** Nur Server-initiierte oder Admin-gezeichnete Nachrichten; strenge Audit-Logs.

### 4760–4769: Debug / Profiling
- **Zweck:** Performance-Profiling, Stack/Heap Snapshots (metadatenbasiert).
- **Beispiele:**
  - `4760` ProfilerStartRequest ↔ `4761` ProfilerStartResponse.
  - `4762` ProfilerSampleBatch (Server → Client) mit SamplingData.
- **Sicherheitsanforderungen:** Feature-Flagged, nur in Staging/Canary; kein Endnutzer-Payload.

### 4770–4779: Experimente mit deterministischen Buckets
- **Zweck:** Stabile Zuweisung (Hash(AccountId)%N) für langfristige Experimente.
- **Beispiele:**
  - `4770` BucketAssignmentRequest ↔ `4771` BucketAssignmentResponse.
- **Sicherheitsanforderungen:** Keine sensitiven Daten; Hash-Funktion dokumentieren.

### 4780–4789: Content Delivery Hints
- **Zweck:** Client-Preload-Steuerung, Asset-Prefetch ohne Zwang.
- **Beispiele:**
  - `4780` PreloadHintBroadcast mit AssetIds + Priority.
  - `4781` PreloadFeedback (Client) mit Success/Failure.
- **Sicherheitsanforderungen:** Auth, Payload-Limits, kein verpflichtender Client-State-Change.

### 4790–4799: Notfall-Hotfix
- **Zweck:** Schnell verfügbare Slots für unerwartete Hotfix-Kommunikation.
- **Regel:** Nur Maintainer dürfen belegen, sofortige Doku-Aktualisierung erforderlich.

---

## 🧩 Beispiel-Flows für künftige Nachrichten

### Flow: ProbeRequest (4700) ↔ ProbeResponse (4701)
```
Client                      Gateway                      Zone
  |                           |                           |
  | ProbeRequest (4700)       |                           |
  |-------------------------->|                           |
  |                           | Forward (if required)     |
  |                           |-------------------------->|
  |                           |                           |
  |                           | ProbeResponse (4701)      |
  |                           |<--------------------------|
  | ProbeResponse (4701)      |                           |
  |<--------------------------|                           |
```
- **Timeout:** 2s
- **Retry:** Max 1 Retry, Idempotent
- **Telemetry:** `probe_rtt_ms`, `probe_payload_bytes`

### Flow: FeatureConfigRequest (4710) ↔ FeatureConfigResponse (4711)
```
Client                Gateway               Config Service
  | FeatureConfigRequest (4710) |
  |---------------------------->|
  |                             | Fetch bucket + variation
  |                             |------------------------>|
  |                             |<------------------------|
  | FeatureConfigResponse (4711)|
  |<----------------------------|
```
- **Timeout:** 1s
- **Caching:** Response TTL in payload
- **Validation:** Variation must be in allowed set

### Flow: KillSwitchCommand (4750) ↔ KillSwitchResult (4751)
```
Operator Console          Gateway                 Zones
       |                     |                      |
       | KillSwitchCommand   |                      |
       |-------------------->| Broadcast to Zones   |
       |                     |--------------------->|
       |                     |<---------------------|
       | KillSwitchResult    |                      |
       |<--------------------|                      |
```
- **Auth:** Admin token + signature
- **Effect:** Runtime disable of specified message IDs or subsystems
- **Rollback:** Command with `Action=Disable=false`

---

## 🧲 Error Handling & Validation Matrix

| Feldtyp | Validation | Reject Code | Logging |
|---------|------------|-------------|---------|
| string (identifier) | Length 1-64, UTF-8, keine Steuerzeichen | `ERR_INVALID_STRING` | Warn + trace id |
| Guid | Nicht-Empty | `ERR_INVALID_GUID` | Warn |
| ushort | Range check (≥0) | `ERR_INVALID_USHORT` | Debug |
| byte enum | In Enum? | `ERR_INVALID_ENUM` | Warn |
| Payload size | ≤ 1024 Bytes (Reserve default) | `ERR_PAYLOAD_TOO_LARGE` | Warn |
| Auth token | Pflicht wenn nicht als Public markiert | `ERR_UNAUTHORIZED` | Warn |

- **Unknown MessageType:** Drop + optional `ErrorMessage (910)` mit `UNKNOWN_TYPE`.
- **Rate-Limit:** Bei Überschreitung Antwort `RateLimitWarning (917)` + Drop der Message.
- **Replay:** Nonce/Sequence prüfen, bei Verstoß `ERR_REPLAY` loggen, Response optional.

---

## 🧱 DTO-Beispielvorlagen (für spätere Nutzung)

### Template: Request + Response
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.FeaturePilotRequest)]
public class FeaturePilotRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.FeaturePilotRequest;
    [Key(1)] public Guid SessionId { get; init; }
    [Key(2)] public string ClientVersion { get; init; } = "";
}

[MessagePackObject]
[NetworkMessage(MessageType.FeaturePilotResponse)]
public class FeaturePilotResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.FeaturePilotResponse;
    [Key(1)] public bool Enabled { get; init; }
    [Key(2)] public string Variant { get; init; } = "";
    [Key(3)] public int TtlSeconds { get; init; }
    [Key(4)] public string? ErrorCode { get; init; }
}
```

### Template: Broadcast
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LiveOpsNotice)]
public class LiveOpsNotice : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LiveOpsNotice;
    [Key(1)] public string Title { get; init; } = "";
    [Key(2)] public string Body { get; init; } = "";
    [Key(3)] public long ExpiresAt { get; init; }
    [Key(4)] public byte Priority { get; init; }
}
```

---

## 🧮 Rate-Limit & QoS Vorgaben (Standardwerte für Range 47)

| Message-Typ | Default Limit | Burst | Bucket Size | Notizen |
|-------------|---------------|-------|-------------|---------|
| Requests | 10/min/connection | 3 | Token-Bucket | Anpassbar per Feature-Flag |
| Responses | Unbegrenzt (serverseitig gesteuert) | – | – | Nur serverseitig |
| Broadcasts | 1/min/zone | 1 | N/A | Für LiveOps/Notices; höhere Frequenz nur mit Sampling |
| Debug/Profiling | 1/min/connection | 1 | Token-Bucket | Nur Staging/Canary |

- **QoS Klasse:** Standard = „Normal“; Debug = „Low“; KillSwitch = „High“ (bypass non-critical queues).

---

## 🔎 Threat-Model & Abuse Cases

- **Spoofing:** Unauthentifizierte Nutzung verhindern (Auth required).
- **Flooding:** Rate-Limits + Server-Side Filtering.
- **Downgrade:** Clients dürfen Reserved-IDs ignorieren; Server darf keine sicherheitskritischen Entscheidungen von unverifizierten Clients annehmen.
- **Replay:** Nonce/Sequence in sicherheitsrelevanten Nachrichten, 5-Minute Window.
- **Data Leakage:** Keine PII; Logs maskieren IDs wenn nötig.

---

## 🧪 Testfall-Katalog (bei Aktivierung zu instantiieren)

| ID | Testfall | Erwartung |
|----|----------|-----------|
| TC-4700-01 | Serializer Roundtrip | Payload unverändert nach Pack/Unpack |
| TC-4700-02 | Unauthenticated Request | Server antwortet mit Unauthorized/Drop |
| TC-4700-03 | Rate-Limit Exceed | 429/RateLimitWarning, kein Crash |
| TC-4700-04 | Unknown Enum Value | Validation schlägt fehl, Log Warn |
| TC-4700-05 | Replay Attack | Nonce erkannt, Message verworfen |
| TC-4700-06 | Feature-Flag Off | Message wird abgelehnt/ignoriert |
| TC-4700-07 | Feature-Flag On | Handler wird ausgeführt, Response korrekt |
| TC-4700-08 | Large Payload > Limit | Ablehnen mit ErrorCode |
| TC-4700-09 | Mixed-Version Client | Client ignoriert unbekannte Message, keine UI-Fehler |

---

## 🏗️ Implementierungs-Checkliste für neue Nachrichten

- [ ] Enum-Eintrag ergänzt (4700–4799).
- [ ] `[MessagePackObject]` + `[NetworkMessage]` Attribute gesetzt.
- [ ] `[Key(0)] public MessageType Type => …` gesetzt.
- [ ] Keys aufsteigend, keine Lücken bei Pflichtfeldern.
- [ ] Request/Response Dokumentation hinzugefügt.
- [ ] Error Codes dokumentiert.
- [ ] Tests erstellt (Roundtrip, Handler).
- [ ] Feature-Flag integriert.
- [ ] Telemetrie-Labels definiert.
- [ ] Rollout- und Rollback-Plan dokumentiert.

---

## 🧭 Edge Cases & Failure Modes

- **Out-of-Order Responses:** Clients müssen Response mit `CorrelationId` prüfen.
- **Dropped Broadcasts:** Nicht kritisch, optional Retry-Mechanismus.
- **Partial Deployments:** Server mit/ohne neuen Handlern – Messages aus Feature-Flag OFF werden verworfen.
- **Payload Truncation:** Detect via length + checksum; Response mit ErrorCode `ERR_INTEGRITY`.
- **Clock Skew:** Für Zeitfelder nur Serverzeit vertrauen; Client-Zeit darf ignoriert werden.

---

## 🧾 Logging- und Telemetrie-Felder (Empfehlung)

- `message_type` (ushort)
- `category` (uint = type/100)
- `direction` (c2s / s2c / broadcast)
- `auth` (bool)
- `payload_bytes` (int)
- `trace_id` (string, optional)
- `span_id` (string, optional)
- `latency_ms` (int, für Request/Response)
- `result` (success/fail)
- `error_code` (string)

---

## 🧠 Design-Guidelines pro Nachrichtentyp

- **Request:** Muss `CorrelationId` (Guid) enthalten, wenn Antwort asynchron kommen kann.
- **Response:** Muss `Success` + `ErrorCode`/`ErrorMessage` Felder haben, falls Fehler möglich.
- **Broadcast:** Kein Request/Response; enthält `BroadcastId` und optional `TTL`.
- **Command:** Sollte idempotent sein oder `CommandId` tragen.
- **Event:** Nur serverseitig; keine Client-Eingriffe in Authority.

---

## 🪄 Beispiel-ErrorCodes (Reserviert)

| Code | Bedeutung | Client-Action |
|------|-----------|---------------|
| `ERR_UNKNOWN` | Unbekannter Fehler | Retry nach Backoff |
| `ERR_UNAUTHORIZED` | Auth fehlt/ungültig | Re-Auth oder Abbruch |
| `ERR_RATE_LIMIT` | Rate-Limit überschritten | Backoff + UI Hinweis |
| `ERR_PAYLOAD_TOO_LARGE` | Payload zu groß | Kürzen und erneut senden |
| `ERR_INVALID_STATE` | Ungültiger Kontext | Refresh/Sync anfordern |
| `ERR_EXPERIMENT_NOT_FOUND` | Experiment nicht definiert | Fallback-Pfad nutzen |
| `ERR_KILL_SWITCH_ACTIVE` | Subsystem deaktiviert | UI Hinweis, Aktion blocken |

---

## 🧭 Beispiel-Feature-Flag-Struktur

| Flag | Default | Scope | Beschreibung |
|------|---------|-------|--------------|
| `messages.47.enabled` | false | Global | Aktiviert Routing der Kategorie 47 |
| `messages.47.probe.enabled` | false | Env/Shard | Aktiviert ProbeRequest/Response |
| `messages.47.liveops.enabled` | false | Realm | Aktiviert LiveOpsNotice Broadcast |
| `messages.47.killswitch.enabled` | false | Global | Erlaubt KillSwitchCommand |

- Flags müssen in Server-Config dokumentiert sein.

---

## 🧮 Kapazitäts- und Performance-Guides

- **Payload Budget:** Default ≤1 KB; für Profiling ≤4 KB nur in Staging.
- **Throughput Ziel:** Kategorie 47 sollte <1% des Gesamttraffics bleiben.
- **Memory:** Keine großen Buffers; Streams chunked senden.
- **CPU:** Heavy JSON/MsgPack conversion vermeiden; reuse DTOs.

---

## 🧬 Envelope-Konzept (optional für mehrere Untertypen)

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BridgeEventEnvelope)]
public class BridgeEventEnvelope : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BridgeEventEnvelope;
    [Key(1)] public byte EventType { get; init; }
    [Key(2)] public byte[] Payload { get; init; } = Array.Empty<byte>();
    [Key(3)] public Guid CorrelationId { get; init; }
    [Key(4)] public long Timestamp { get; init; }
}
```
- **Verwendung:** Für polymorphe kurzfristige Events; EventType muss dokumentiert werden.
- **Sicherheit:** Payload Größe begrenzen; EventType whitelist.

---

## 🧭 Operational Runbooks (Beispiel)

### Aktivierung ProbeRequest
1. Feature-Flag `messages.47.probe.enabled=true` in Staging setzen.
2. Tests laufen lassen (Roundtrip, Routing).
3. Canary auf 5% der Gateways.
4. Metriken prüfen: RTT, ErrorRate, PayloadSize.
5. Rollout auf 100% oder Rollback bei Alerts.

### Rollback KillSwitchCommand
1. Flag `messages.47.killswitch.enabled=false`.
2. Router-Eintrag entfernen oder droppen.
3. Telemetrie prüfen, ob keine neuen 4750er Nachrichten mehr auftauchen.

---

## 🧩 Beispiel-Szenarien für QA

1. **ProbeRequest unter Last:** 100 RPS, sicherstellen, dass Rate-Limit greift.
2. **Experiment Variation:** Gleicher Account bekommt konsistent denselben Bucket.
3. **KillSwitch Toggle:** Subsystem deaktiviert, alle nachfolgenden Messages des Zielsystems werden verworfen.
4. **LiveOpsNotice Expiry:** Notice verschwindet nach `ExpiresAt`, Client löscht UI-Element.
5. **PreloadHint Feedback:** Client meldet fehlgeschlagenen Download, Server passt Hint-Frequenz an.

---

## 🧱 Konsistenzregeln mit anderen Kategorien

- Keine Dupplikation von IDs aus bestehenden Ranges; wenn Funktionalität thematisch passt, dort implementieren.
- Bei Übergang aus Range 47 in eine thematische Range (z. B. Events → 4600er) muss:
  - Neue endgültige ID dort registriert werden.
  - Alte 47xx ID als Obsolete markiert werden.
  - Migration in beiden Dokumenten beschrieben werden.

---

## 🛰️ Client-Verhalten für unbekannte 47xx IDs

- **Default:** Client ignoriert unbekannte Nachrichten in 4700–4799 und loggt Telemetrie (`unknown_reserved_message`).
- **Option:** Konfigurierbarer „strict mode“ für QA, der eine Warnung oder Disconnect auslöst, um früh Fehler zu finden.

---

## 🧭 Server-Side Safeguards

- **Router Drop Rule:** Wenn Feature-Flag OFF → Drop + Counter++.
- **Audit Log:** Für KillSwitch/LiveOps/Debug Nachrichten Audit-Eintrag mit Actor, Timestamp, Parameters.
- **Metrics:** `reserved47_drop_total`, `reserved47_processed_total`.
- **Backpressure:** Bei Überlast Priorität für Kategorie 47 herabsetzen (außer KillSwitch).

---

## 🧠 Dokumentationspflicht bei Aktivierung

- Neuer Abschnitt in diesem Dokument mit vollständigem Template:
  - Richtung, Frequenz, Auth, Spezialrechte
  - Beschreibung, Im/Out of Scope
  - Payload-Tabelle
  - Erwartete Response
  - Verwandte Messages
  - Beispiel Payload
  - Error Codes
  - Notizen
- `MessageType Enum Updates` Abschnitt erweitern.
- Reserved Slots Tabelle aktualisieren.

---

## 🧭 Stabilitäts- und Langlebigkeitsregeln

- Keine dauerhaften Kernfeatures in Range 47 parken. Dauerhafte Funktionen gehören in passende Range.
- Jede 47xx Message soll ein geplantes „End-of-Life“ oder „Promote to proper range“ Datum besitzen.

---

## 🧪 Kompatibilitäts-Checkliste vor Merge

- [ ] Mixed-Version Test: alter Client, neuer Server.
- [ ] Downgrade Test: neuer Client, alter Server (Messages werden ignoriert, kein Crash).
- [ ] Serialization Size geprüft (<= Limit).
- [ ] Auth-Zwang getestet (ablehnen ohne Token).
- [ ] Rate-Limit getestet.
- [ ] Logging ohne PII verifiziert.

---

## 🧭 Beispiel-Metrik-Namen

- `mmo_messages_received_total{type="4700"}`
- `mmo_messages_dropped_total{type="4700",reason="feature_flag_off"}`
- `mmo_messages_latency_ms{type="4700"}`
- `mmo_messages_payload_bytes{type="4700"}`
- `mmo_messages_error_total{type="4700",code="ERR_INVALID_ENUM"}`

---

## 🧱 Feld-spezifische Validierungsrichtlinien

- **Strings:** Trim, max 256 Zeichen, keine Steuerzeichen (`<0x20` außer `\n` wenn erlaubt).
- **Arrays/Listen:** Max 100 Einträge, keine Null-Items.
- **Dictionaries:** Max 50 Keys, Schlüssel-Länge max 64.
- **Binary:** Base64 nur wenn notwendig; sonst MessagePack bin.
- **Numeric:** Prüfen auf Overflow bei Multiplikation/Zeitskalierung.

---

## 🧭 Release Notes Template für neue 47xx Messages

```
### Kategorie 47 – Neue Nachricht: <Name> (<ID>)
- Richtung: …
- Feature-Flag: <flag>
- Default: OFF
- Auswirkungen: <kurz>
- Rollout: <Stufenplan>
- Telemetrie: <Metrics>
```

---

## 🧩 Beispiel Payloads (weiterführend)

```csharp
// Experiment Bucket Response
var response = new FeatureConfigResponse
{
    Type = MessageType.FeatureConfigResponse,
    Enabled = true,
    Variant = "B",
    TtlSeconds = 3600,
    ErrorCode = null
};

// KillSwitch Command
var cmd = new KillSwitchCommand
{
    Target = "EntityUpdate",
    Disable = true,
    Reason = "High error rate",
    CorrelationId = Guid.NewGuid()
};
```

---

## 🧭 Monitoring-Alerts (Beispielregeln)

- **Unknown Reserved ID:** Wenn `reserved47_unknown_total > 0` in 5 Min → Warn.
- **High Drop Rate:** `reserved47_drop_total / reserved47_received_total > 0.1` → Warn.
- **Probe RTT High:** P95 `probe_rtt_ms` > 200ms → Warn; >500ms → Critical.

---

## 🧱 Verantwortlichkeiten

- **Maintainer:** Freigabe, Doku-Update, Enum-Update.
- **Feature Owner:** Implementierung, Tests, Rollout-Plan.
- **QA:** Testfallkatalog ausführen, Mixed-Version sicherstellen.
- **SRE:** Monitoring/Alerting konfigurieren.

---

## 🧭 Backward Compatibility Patterns

- **Graceful Ignore:** Server ignoriert unbekannte Messages, loggt metrisch.
- **Downgrade Path:** Bei Flag OFF sendet Server optional `ErrorMessage (910)` mit Hinweis.
- **Schema Evolution:** Nur additive Felder, keine Key-Änderungen.

---

## 🧰 Beispiel für deterministische Buckets

```csharp
static byte GetBucket(Guid accountId, byte bucketCount)
{
    var bytes = accountId.ToByteArray();
    var hash = XXHash32.DigestOf(bytes);
    return (byte)(hash % bucketCount);
}
```

- **Determinismus:** Gleicher Account → gleicher Bucket.
- **Config:** `bucketCount` im Response mitliefern.

---

## 🧭 Datenhaltung & Retention

- **Logs:** 30 Tage für neue 47xx Nachrichten.
- **Metrics:** 14 Tage hochauflösend, danach aggregiert.
- **Traces:** Nur Sampling bei Bedarf (≤1%).

---

## 🧪 Interop-Regeln (Client/Server Libraries)

- MessageType-Konstante muss in beiden Sprachen identisch sein.
- Kein „string based routing“; ausschließlich Enum.
- Client-SDK muss Unknown-Handler haben, um UI-Störungen zu vermeiden.

---

## 🧭 Qualitätsziele (QoS)

- **Latency Ziel:** P95 < 150ms (Request/Response innerhalb eines Rechenzentrums).
- **Error Rate Ziel:** <0.5% nach Stabilisierung.
- **Availability:** Kategorie 47 darf keine Systeminstabilität verursachen; bei Problemen Kill-Switch verwenden.

---

## 🧱 FAQ (häufige Fragen)

- **Warum eigener Range?** Um schnelle, kontrollierte Aktivierungen zu ermöglichen ohne bestehende Ranges zu riskieren.
- **Darf ich dauerhafte Features hier lassen?** Nein, nach Stabilisierung in passende Range verschieben.
- **Brauche ich immer eine Response?** Ja, außer Broadcast/Event mit klarer Begründung.
- **Was passiert bei unbekannter 47xx Message?** Wird gedroppt/ignoriert, Telemetrie erfasst.

---

## 🧭 Compliance-Matrix (Pflicht je Nachricht)

| Pflicht | Beschreibung | Erfüllt durch |
|---------|--------------|---------------|
| Auth | Ist Authentifizierung notwendig? | Doku + Handler-Check |
| RateLimit | Limit pro Verbindung festgelegt | Config + Test |
| Request/Response | Paar vorhanden oder begründet | Doku-Abschnitt |
| Telemetrie | Metrics & Logs definiert | Observability-Playbook |
| Feature-Flag | Konfigurierbarer Toggle | Config/Code |
| Security Review | Abhängig von Risiko | Review-Log |

---

## 🧭 Erweiterte QA-Checkliste

- [ ] Negative Tests (ungültige Felder, fehlende Felder, zu große Payload).
- [ ] Timeout Handling (Response fehlt).
- [ ] Duplicate Handling (gleiche Message doppelt gesendet).
- [ ] Telemetry-Assertion (Counters steigen erwartungsgemäß).
- [ ] Feature-Flag Off Behavior (kein Handler-Aufruf).
- [ ] Multi-Zone Verhalten (falls Broadcast).

---

## 🧭 Feldtypen und Schlüssel-Indices (Beispieltabellen)

| Key | Feld | Typ | Pflicht | Kommentar |
|-----|------|-----|---------|-----------|
| 0 | Type | MessageType | Ja | Immer Key 0 |
| 1 | CorrelationId | Guid | Nein | Für Requests empfohlen |
| 2 | PayloadVersion | byte | Nein | Für Upgrades nützlich |
| 3 | Data | DTO/Struct | Ja | Kerninhalt |
| 4 | TtlSeconds | int | Nein | Für Notices/Broadcasts |

---

## 🧭 Muster für Backward-Compatibility Flags

```csharp
public static class FeatureFlags
{
    public const string Reserved47 = "messages.47.enabled";
    public const string Reserved47Probe = "messages.47.probe.enabled";
    public const string Reserved47LiveOps = "messages.47.liveops.enabled";
}
```
- **Regel:** Jeder neue 47xx Typ erhält eigenen Sub-Flag.

---

## 🧭 Rollout-Stufen (Empfohlen)

1. **Dev:** Lokale Tests, Flag ON.
2. **Staging:** Flag ON für alle, aber kleine Population.
3. **Canary:** 5–10% Produktion, Monitoring engmaschig.
4. **Full:** 100%, wenn KPIs stabil.
5. **Cleanup:** Nach Stabilisierung in passende Range migrieren, 47xx obsolet erklären.

---

## 🧭 Load- & Performance-Tests (Vorlagen)

- **Szenario A:** 50 RPS ProbeRequest, Payload 128 Bytes → Ziel CPU < 5%.
- **Szenario B:** 5 RPS KillSwitchCommand → sicherstellen, dass keine Warteschlange blockiert.
- **Szenario C:** 20 RPS PreloadHintBroadcast → Client darf nicht überlasten (UI Responsiveness).

---

## 🧭 Glossar

- **Feature-Flag:** Konfigurierbarer Schalter zum Aktivieren einer Funktion.
- **Kill-Switch:** Mechanismus zum sofortigen Abschalten eines Subsystems.
- **Probe:** Kleine Messnachricht zur Überwachung von Latenzen.
- **LiveOps:** Operative Aktionen während Live-Betrieb ohne Patch.
- **Bucket:** Deterministische Zuordnung zu Experiment-Varianten.

---

## 🧭 Dokument-Beispiel für zukünftige Message-Sektion

```
## FeaturePilotRequest (4700)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert die aktuelle Experiment-Zuweisung an.

### Im Scope ✅
- Bucket-Zuordnung
- TTL für Cache

### Nicht im Scope ❌
- Experiment-Definition

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Korrelations-ID | Ja |
| ClientVersion | string | Client-Version | Ja |

### Erwartete Response
- `FeaturePilotResponse (4701)`

### Verwandte Messages
| Message | ID | Beziehung |
|---------|----|-----------|
| `FeaturePilotResponse` | 4701 | Antwort |

### Beispiel Payload
```csharp
var req = new FeaturePilotRequest
{
    CorrelationId = Guid.NewGuid(),
    ClientVersion = "0.2.0"
};
```
```

---

## 🧭 Betriebliches Notfall-Playbook (Beispiel)

1. Alert „High ErrorRate 47xx“ schlägt an.
2. Prüfe Dashboards (ErrorCode, PayloadSize, RateLimits).
3. Aktiviere Kill-Switch für betroffenen Typ.
4. Kommuniziere im Incident-Channel.
5. Erstelle Postmortem mit Root Cause, Fix, Prävention.

---

## 🧭 Checklist für Doku-Reviews

- [ ] Richtiger ID-Bereich angegeben.
- [ ] Abschnitts-Template komplett (Beschreibung, Payload, Response, Error Codes).
- [ ] Tabellen korrekt formatiert.
- [ ] Codebeispiele gültig (Key 0 = Type).
- [ ] Zurück-zur-Übersicht Link vorhanden.
- [ ] Letzte Aktualisierung/Version aktualisiert.

---

## 🧭 Beispiel-Routing-Registrierung (Server)

```csharp
router.Register(MessageType.FeatureConfigRequest, HandleFeatureConfigRequest, featureFlag: "messages.47.experiment.enabled");
router.Register(MessageType.ProbeRequest, HandleProbeRequest, featureFlag: "messages.47.probe.enabled");
```

- **Hinweis:** Feature-Flag muss zur Compile-Zeit und Deploy-Zeit bekannt sein.

---

## 🧭 Security-Review Punkte

- Eingaben sanitizen (Strings/Arrays).
- Keine PII im Logging.
- Rate-Limits geprüft.
- Auth-Zwang dokumentiert.
- Replay-Schutz vorhanden (falls relevant).
- Kein Trust in Client-Buckets ohne Server-Verifikation.

---

## 🧭 Beispiel-Testcode (Serializer)

```csharp
[Fact]
public void FeaturePilotRequest_Roundtrip()
{
    var req = new FeaturePilotRequest
    {
        CorrelationId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
        ClientVersion = "1.0.0"
    };

    var bytes = MessagePackSerializer.Serialize(req);
    var clone = MessagePackSerializer.Deserialize<FeaturePilotRequest>(bytes);

    Assert.Equal(req.CorrelationId, clone.CorrelationId);
    Assert.Equal(req.ClientVersion, clone.ClientVersion);
    Assert.Equal(MessageType.FeaturePilotRequest, clone.Type);
}
```

---

## 🧭 Beispiel-UI-Auswirkungen (wenn Client beteiligt)

- LiveOpsNotice blendet Banner oben ein, verschwindet nach `ExpiresAt`.
- PreloadHint zeigt progresslosen Spinner → darf UI nicht blockieren.
- Experiment Variation bestimmt Layout; bei fehlender Response Fallback auf Control.

---

## 🧭 Metrics-Sanity-Checks nach Rollout

- `reserved47_processed_total` steigt, `reserved47_drop_total` bleibt ~0.
- P95 Latenz < 150ms.
- Keine Erhöhung der Global Error Rate.

---

## 🧭 Datenflussbeschreibung (generisch)

```
Client --(470x Request)--> Gateway --(optional)--> Zone/Service
   ^                             |
   |                             v
Response 470x+1 <-----------------
```

- Gateway kann Validierung + Rate-Limit durchführen, bevor an Zone weitergeleitet wird.

---

## 🧭 Priorisierung bei Überlast

1. KillSwitch / Safety Commands
2. Responses
3. Requests
4. Broadcasts / Notices
5. Debug/Profiling

- Queue-Manager soll diese Reihenfolge beachten.

---

## 🧭 Transport- und Verschlüsselungshinweise

- TLS 1.3 vorausgesetzt (Handshake siehe `00-connection.md`).
- Keine Klartext-Credentials in Payload.
- Für Admin-Kommandos zusätzliche Signatur (HMAC/EdDSA) empfohlen.

---

## 🧭 Beispiel-Config-Snippet (YAML)

```yaml
messages:
  reserved47:
    enabled: false
    probe:
      enabled: false
      rate_limit_per_min: 10
    liveops:
      enabled: false
      max_broadcast_per_min: 1
    killswitch:
      enabled: false
      audit: true
```

---

## 🧭 Migrationspfad von 47xx zu endgültiger Range

1. Neue finale ID im passenden Range anlegen.
2. 47xx Nachricht als Obsolete markieren.
3. Dual-Send Phase (47xx + final) für begrenzte Zeit.
4. Client hört auf beide, bevorzugt finale ID.
5. Nach Stabilisierungsfenster 47xx deaktivieren, später entfernen.

---

## 🧭 Lizenz- und Compliance-Hinweise

- Keine Drittbibliotheken nur für Range 47 einführen ohne Audit.
- Datenschutz: Keine personenbezogenen Daten in Experiment/Probe Payloads.

---

## 🧭 Post-Deployment Validation

- Checklogs: keine UnknownType-Warnungen.
- Metrics: erwartete Volumina.
- User Reports: keine UI-Fehler.
- SRE: bestätigt keine Incident-Alarme ausgelöst.

---

## 🧭 Erweiterte Beispieltabelle für Reserved Slots (Detail)

| ID | Aktueller Status | Vorgeschlagener Owner | Voraussetzung für Aktivierung | Geplanter Ersatzrange |
|----|------------------|-----------------------|-------------------------------|-----------------------|
| 4700 | Reserved | Observability Lead | Flag `messages.47.probe.enabled` | 900-Range (System) |
| 4710 | Reserved | Experiment Owner | Bucket-Algorithmus definiert | 2900-Range (Tutorial/Guide) |
| 4720 | Reserved | Platform Lead | Schema-Versionierung fertig | 100-Range (Zone) |
| 4730 | Reserved | LiveOps Team | EventType-Enum definiert | 4600-Range (Events) |
| 4740 | Reserved | LiveOps Team | Notice-Template abgesegnet | 4300-Range (Notifications) |
| 4750 | Reserved | SRE Lead | Security-Review abgeschlossen | 900-Range (System) |
| 4760 | Reserved | Performance Team | Profiling-Sampling-Plan | 900-Range (System) |
| 4770 | Reserved | Experiment Owner | Hash-Funktion validiert | 2900-Range (Tutorial/Guide) |
| 4780 | Reserved | Content Delivery | Asset-Manifest v2 verfügbar | 3400-Range (Map/Assets) |
| 4790 | Reserved | Maintainer | Incident-Fall | tbd |

---

## 🧭 Change-Log Template (für diese Datei)

```
## [1.x.x] - YYYY-MM-DD
### Added
- Neue Nachricht <Name> (<ID>) dokumentiert.
- Tests/Telemetry ergänzt.

### Changed
- Reserved Slots Tabelle aktualisiert.

### Removed
- <ID> als Obsolete markiert und Entfernen geplant.
```

---

---

## 🧩 Schnittstellen- und DTO-Richtlinien

- **Namespace:** Neue DTOs unter `Mmo.Shared.Messaging` bzw. spezifischem Feature-Namespace.
- **Interfaces:** Implementieren passendes Marker-Interface (`IClientMessage`, `IServerMessage`, `IBroadcastMessage`).
- **Serialization Attributes:** `[MessagePackObject]` + `[NetworkMessage(MessageType.X)]` + `[Key(n)]`.
- **Base DTOs:** Nutzen bestehende shared DTOs bevor neue Strukturen erfunden werden.
- **Validation Hooks:** Server-Handler führen Feldvalidierung (Length, Ranges) durch.

---

## 🧰 Developer Workflow

1. Slot in Tabelle markieren.
2. Enum-Eintrag ergänzen (mit Kommentar `// RESERVED - <Kurztext>` bis Implementierung bereit).
3. Message-Dateien anlegen, Doku-Abschnitt ergänzen.
4. Tests schreiben und ausführen.
5. Review (Tech + Security).
6. Feature-Flag default OFF, dann gestuft aktivieren.

---

## 🪪 Naming & Comment Conventions

- **Enum:** `MeaningfulPascalCase = 470X, // RESERVED - <Kurz>` während Reserve-Phase; nach Aktivierung Kommentar auf Zweck anpassen.
- **Message-Klasse:** Gleichnamig wie Enum-Eintrag.
- **Docs:** Abschnittsname = `<MessageName> (<ID>)`.
- **Comments:** Kurz, prägnant, ohne TODO/TBD.

---

## 🔁 Versioning & Rollback Szenarien

- **Minor-Version Activation:** Neue Messages ideal in Minor-Version mit Feature-Flag.
- **Rollback:** Sofort durch Kill-Switch + Gateway-Filter (drop) möglich; Clients müssen tolerant sein.
- **Deprecation Timer:** Mindestens eine Minor-Version Vorlauf vor Entfernen.
- **Cross-Version Handling:** Server akzeptiert unbekannte Messages? → Droppen + ErrorMessage (910) optional, sofern sicher.

---

## 📎 Anhang (MessageType Enum Updates)

Aktuell sind keine neuen Enum-Einträge für 4700–4799 hinzugefügt. Sobald ein Slot aktiviert wird, MUSS hier ein Codeblock mit den neuen Zeilen ergänzt werden, z. B.:

```csharp
// RESERVED RANGE 4700-4799
// Beispiel (nach Freigabe einsetzen):
// FeaturePilotRequest = 4700,   // RESERVED - Feature-Gate Pilot
// FeaturePilotResponse = 4701,  // RESERVED - Antwort
```

Sollten konkrete Einträge hinzugefügt werden, muss die Tabelle „Reserved Slots“ oben angepasst und der Status von „Reserved“ auf „Aktiv“ oder „Obsolete“ gesetzt werden.

---

**Letzte Aktualisierung:** 2026-01-03  
**Version:** 1.0.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/47-reserved.md
