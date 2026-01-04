# 🔁 Server-to-Server Messages (5000-5099)

**Kategorie:** 50  
**Range:** 5000-5099  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🔐 Auth & Handshake](#-auth--handshake)
- [💓 Health, Heartbeats & Load](#-health-heartbeats--load)
- [🚚 Transfers](#-transfers)
- [🔒 Ownership & Leases](#-ownership--leases)
- [🛰️ Replication](#-replication)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#-regeln--sicherheit)
- [📩 Aktive Messages 5000–5099](#-aktive-messages-5000-5099)
  - [5000 ClusterHandshakeRequest](#clusterhandshakerequest-5000)
  - [5001 ClusterHandshakeResponse](#clusterhandshakeresponse-5001)
  - [5002 ClusterAuthChallengeRequest](#clusterauthchallengerequest-5002)
  - [5003 ClusterAuthChallengeResponse](#clusterauthchallengeresponse-5003)
  - [5004 ClusterKeyRotationRequest](#clusterkeyrotationrequest-5004)
  - [5005 ClusterKeyRotationResponse](#clusterkeyrotationresponse-5005)
  - [5006 HealthStatusRequest](#healthstatusrequest-5006)
  - [5007 HealthStatusResponse](#healthstatusresponse-5007)
  - [5008 LoadReportRequest](#loadreportrequest-5008)
  - [5009 LoadReportResponse](#loadreportresponse-5009)
  - [5010 TimeSyncRequest](#timesyncrequest-5010)
  - [5011 TimeSyncResponse](#timesyncresponse-5011)
  - [5012 NodeDrainRequest](#nodedrainrequest-5012)
  - [5013 NodeDrainResponse](#nodedrainresponse-5013)
  - [5014 PlayerTransferPrepareRequest](#playertransferpreparerequest-5014)
  - [5015 PlayerTransferPrepareResponse](#playertransferprepareresponse-5015)
  - [5016 PlayerTransferCommitRequest](#playertransfercommitrequest-5016)
  - [5017 PlayerTransferCommitResponse](#playertransfercommitresponse-5017)
  - [5018 PlayerTransferAbortRequest](#playertransferabortrequest-5018)
  - [5019 PlayerTransferAbortResponse](#playertransferabortresponse-5019)
  - [5020 EntityHandoffRequest](#entityhandoffrequest-5020)
  - [5021 EntityHandoffResponse](#entityhandoffresponse-5021)
  - [5022 LeaseAcquireRequest](#leaseacquirerequest-5022)
  - [5023 LeaseAcquireResponse](#leaseacquireresponse-5023)
  - [5024 LeaseRenewRequest](#leaserenewrequest-5024)
  - [5025 LeaseRenewResponse](#leaserenewresponse-5025)
  - [5026 LeaseReleaseRequest](#leasereleaserequest-5026)
  - [5027 LeaseReleaseResponse](#leasereleaseresponse-5027)
  - [5028 PartitionOwnershipQueryRequest](#partitionownershipqueryrequest-5028)
  - [5029 PartitionOwnershipQueryResponse](#partitionownershipqueryresponse-5029)
  - [5030 EventReplicationPublishRequest](#eventreplicationpublishrequest-5030)
  - [5031 EventReplicationPublishResponse](#eventreplicationpublishresponse-5031)
- [5032 GuildSyncRequest](#guildsyncrequest-5032)
- [5033 GuildSyncResponse](#guildsyncresponse-5033)
- [5034 PartySyncRequest](#partysyncrequest-5034)
- [5035 PartySyncResponse](#partysyncresponse-5035)
- [5036 ChatRouteRegisterRequest](#chatrouteregisterrequest-5036)
- [5037 ChatRouteRegisterResponse](#chatrouteregisterresponse-5037)
- [5038 ChatEnvelopeRelayRequest](#chatenveloperelayrequest-5038)
- [5039 ChatEnvelopeRelayResponse](#chatenveloperelayresponse-5039)
- [5040 AdminBroadcastRequest](#adminbroadcastrequest-5040)
- [5041 AdminBroadcastResponse](#adminbroadcastresponse-5041)
- [5042 ConfigReloadRequest](#configreloadrequest-5042)
- [5043 ConfigReloadResponse](#configreloadresponse-5043)
- [5044 CircuitBreakerStateRequest](#circuitbreakerstaterequest-5044)
- [5045 CircuitBreakerStateResponse](#circuitbreakerstateresponse-5045)
- [5046 BackpressureAlertRequest](#backpressurealertrequest-5046)
- [5047 BackpressureAlertResponse](#backpressurealertresponse-5047)
- [5048 SessionValidateS2SRequest](#sessionvalidates2srequest-5048)
- [5049 SessionValidateS2SResponse](#sessionvalidates2sresponse-5049)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang](#-anhang)

---

## 📋 Überblick

- Ziel: Vollständig definierter Server-zu-Server (S2S) Kommunikationskanal ohne Shared Memory für Gateway-, Zone- und Utility-Server.
- Vertrauensgrenze: Nur authentifizierte, autorisierte und bekannte Cluster-Teilnehmer dürfen Messages in diesem Range senden.
- Transport: mTLS oder HMAC-signierte TCP/QUIC Streams mit Replay- und Ordering-Schutz.
- Zuverlässigkeit: Alle Requests haben Responses, CorrelationId ist Pflicht, Idempotenz durch dedup Cache (TTL ≥ 120s).
- Routing: Jede Message enthält `SenderServerId` und `TargetServerId` oder nutzt deterministische Partitionierung (ShardId/ZoneId).
- Backpressure: In-Flight Limit pro Peer, Circuit Breaker je Message-Type, automatische Downgrade-Pfade.
- Telemetrie: Jede Response enthält Timing-Header (optional) für Latenzmetriken.
- Zeit: Serverzeit ist authoritative, TimeSync Messages stellen monotone Ordnung sicher.
- Sicherheit: Nonce oder TimestampMs mit ±2s Drift akzeptiert; Anti-Replay Cache (max 10k Entries).
- Rollout: Range 5000–5099 exklusiv für S2S, keine Client-Sichtbarkeit.

## 🧠 Datenmodell

- **ServerId:** `Guid` oder 128-bit ULID, eindeutig im Cluster; persistent pro Node.
- **ShardId:** `ushort`; definieren logische Welten oder Realms.
- **ZoneId:** `ushort`; Laufzeitinstanzen von Zonen; eindeutig pro Shard.
- **NodeRole:** Enum (`Gateway`, `Zone`, `Instance`, `Social`, `AdminUtility`, `ChatRouter`).
- **Lease:** Struktur mit `LeaseId (Guid)`, `OwnerServerId`, `ResourceKey`, `DurationMs`, `ExpiresAtMs`, `Version`.
- **TransferTicket:** Struktur mit `TicketId (Guid)`, `PlayerId`, `FromServerId`, `ToServerId`, `ExpiresAtMs`, `Checksum`.
- **CorrelationId:** `Guid`; unique per Request/Response pair; used for dedupe and tracing.
- **Nonce/TimestampMs:** Monotone timestamp for replay protection; accepted skew ±2000ms.
- **PayloadIntegrity:** HMAC-SHA256 mit shared secret oder TLS channel binding.
- **Revision:** Monoton steigende `Revision` (long) pro replicated entity/guild/party stream.
- **QueueDepth:** Int für Outbox/Inbox backlog; genutzt für Backpressure decisions.
- **HealthSnapshot:** CPU%, MEM%, Latency (ms), TickBehind (ms), QueueDepth, Players, Instances.
- **EventEnvelope:** `{RoutingKey, Revision, PayloadType, PayloadBytes, CorrelationId, SenderServerId}`.

## 🔐 Auth & Handshake

- Mutual Authentication via mTLS (preferred) oder HMAC mit shared cluster secret + rotated signing keys.
- Handshake Steps:
  1. `ClusterHandshakeRequest` (5000) mit `SenderServerId`, `NodeRole`, `ShardIds`, `Nonce`.
  2. `ClusterHandshakeResponse` (5001) mit `Accepted=true/false`, `ServerEpoch`, `Features`.
  3. `ClusterAuthChallengeRequest` (5002) mit `Challenge` (random bytes) signiert per HMAC/Certificate.
  4. `ClusterAuthChallengeResponse` (5003) mit `Proof`, `ExpiresAtMs`, `SessionToken`.
  5. Optional: `ClusterKeyRotationRequest` (5004) / `Response` (5005) für geplante Key-Rotation.
- Replay Protection: Challenge Nonce unique per connection; stored in replay cache for 5 minutes.
- Authorization: `NodeRole` + `AllowedActions` claims -> enforced per MessageType.
- Failure Path: On auth failure, peer is quarantined for 60s and reported to admin channel.

## 💓 Health, Heartbeats & Load

- Heartbeat Interval: 5s default; jitter ±500ms.
- Health Payload: CPU%, MEM%, TickDelay, GC Count, Outbox Queue, PlayerCount, Instances.
- Load Shedding: When queue depth > threshold, send `BackpressureAlertRequest` (5046).
- Liveness: Missed 3 consecutive HealthStatus responses -> mark peer degraded; 5 misses -> isolate.
- Topology Awareness: `LoadReportRequest` (5008) aggregates per-shard occupancy for gateway routing.
- Rate Limits: Max 1 HealthStatus per second per peer; dedupe via CorrelationId.

## 🚚 Transfers

- Supports seamless player handoff without disconnect:
  - `PlayerTransferPrepareRequest` (5014): locks player state, issues TransferTicket.
  - `PlayerTransferCommitRequest` (5016): finalizes migration to target zone.
  - `PlayerTransferAbortRequest` (5018): releases locks and reverts.
- Entity handoff for AI/NPC ownership: `EntityHandoffRequest` (5020) with state snapshot + lease swap.
- Validation: TransferTicket includes checksum + expiry; correlation used for idempotent commit.
- Rollback: If commit fails, abort message sent automatically and state rolled back.

## 🔒 Ownership & Leases

- Lease Protocol:
  - Acquire (5022) -> Granted/Denied
  - Renew (5024) -> Extends lease if holder matches
  - Release (5026) -> Voluntary release
- Lease Identifiers: `ResourceKey` format `zone:<id>` / `entity:<id>` / `partition:<id>`.
- Steady-State: Leases must be renewed before 60% of duration elapsed.
- Conflict Handling: If conflicting request arrives, use `LeaseVersion` to arbitrate and log suspicion.
- Failover: If lease expires without release, target may run conflict resolution and emit `BackpressureAlert`.

## 🛰️ Replication

- Event replication via `EventReplicationPublishRequest` (5030) using `EventEnvelope`.
- Stream Ordering: Each stream keyed by `RoutingKey` must be strictly increasing `Revision`.
- Guild/Party cross-zone replication via `GuildSyncRequest` (5032) / `PartySyncRequest` (5034).
- Chat routing metadata via `ChatRouteRegisterRequest` (5036) and `ChatEnvelopeRelayRequest` (5038).
- Retry Policy: Exponential backoff (1s, 2s, 4s, 8s, max 30s), dedupe on `(RoutingKey, Revision)`.
- Dead Letter: If peer rejects message >3 times, emit admin alert and mark stream paused.

## 🔄 Sync, Deltas & Revisioning

- `DataRevision` per logical stream; all deltas include `BaseRevision` and `NewRevision`.
- Idempotency: Apply only if `BaseRevision` matches local; otherwise request snapshot resend.
- Snapshot Path: Use `SnapshotSyncRequest` (5062) / `Response` to resend full state if divergence detected.
- Epoch: `EpochSyncRequest` (5064) used to align monotonic epoch across cluster.
- Delta Size: <64KB recommended; chunk larger payloads into multiple events with shared CorrelationId.
- Conflict Resolution: Last-Writer-Wins by Revision; tie-breaker by `SenderServerId` lexical order.

## 🧱 DTOs / Interfaces

- Baseline Interfaces:

```csharp
public interface IServerToServerMessage : INetworkMessage
{
    Guid CorrelationId { get; set; }
    Guid SenderServerId { get; set; }
    Guid TargetServerId { get; set; }
    long TimestampMs { get; set; }
    long Nonce { get; set; }
}
```

- Messages use `[MessagePackObject]` and `[Key(n)]` in stable order:
  1. `Type` (MessageType) => `[Key(0)]`
  2. `CorrelationId` => `[Key(1)]`
  3. `SenderServerId` => `[Key(2)]`
  4. `TargetServerId` => `[Key(3)]`
  5. `TimestampMs` => `[Key(4)]`
  6. Payload-specific fields start at `[Key(5)]`
- Dedupe Cache keyed by `(CorrelationId, SenderServerId, Type)`.
- Requests expect Responses with same `CorrelationId`.

## 🧩 Enums / ErrorCodes / Flags

- **NodeRole:** `Gateway`, `Zone`, `Instance`, `Social`, `AdminUtility`, `ChatRouter`.
- **TransferResultCode:** `Success`, `TicketInvalid`, `TicketExpired`, `StateLockFailed`, `AlreadyCommitted`, `Aborted`.
- **LeaseResult:** `Granted`, `Denied`, `Expired`, `Stolen`, `Conflict`.
- **ReplicationResult:** `Accepted`, `OutOfOrder`, `Duplicate`, `StreamMissing`, `Throttled`.
- **ConfigReloadTarget:** `Gameplay`, `Networking`, `Chat`, `Economy`, `All`.
- **CircuitBreakerState:** `Closed`, `Open`, `HalfOpen`, `Isolated`.
- **BackpressureLevel:** `None`, `Soft`, `Hard`.
- **SessionValidationResult:** `Valid`, `Invalid`, `Expired`, `Revoked`.
- **Generic Error Codes:** `UNAUTHORIZED`, `FORBIDDEN`, `NOT_FOUND`, `CONFLICT`, `RATE_LIMITED`, `TIMEOUT`, `INTERNAL_ERROR`.

## ⚙️ Regeln & Sicherheit

- **Authentication:** mTLS or HMAC; reject if certificate CN not whitelisted; HMAC requires rotating `KeyId`.
- **Replay Protection:** `Nonce` + `TimestampMs`; cache nonces for 120s; drift max ±2000ms.
- **Idempotenz:** Responses must be repeatable; use stored results for duplicate `CorrelationId`.
- **Rate Limits:** Per-peer message type quotas (e.g., max 10 transfer prepare per second).
- **Circuit Breaker:** Trip after 5 consecutive failures or RTT>5s; auto-half-open after 30s.
- **Backpressure:** `BackpressureAlertRequest` communicates queue thresholds; stop sending non-priority traffic when `Hard`.
- **Encryption:** All payloads on encrypted channel; no PII beyond player identifiers; no secrets in logs.
- **Logging:** Structured logs with `CorrelationId`, `Type`, `SenderServerId`, `TargetServerId`, `Result`, `DurationMs`.
- **Observability:** Emit metrics per message type: `count`, `duration_ms`, `failures`, `timeouts`, `retries`.
- **Testing:** Use integration harness to simulate multi-node topologies; ensure deterministic ordering in tests.

---

## 📩 Aktive Messages 5000-5099

### ClusterHandshakeRequest (5000)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Selten (Start/Restart)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Initialisiert den sicheren S2S-Channel zwischen zwei Knoten. Trägt Basis-Metadaten, Rollen und unterstützte Features, damit der Empfänger die Gegenstelle in die Cluster-Tabelle aufnehmen kann.

### Im Scope ✅
- Austausch von ServerId, NodeRole, Shard- und Zone-Zuständigkeiten
- Ankündigung unterstützter Features (KeyRotation, Snapshot, Compression)

### Nicht im Scope ❌
- Inhaltliche Authentifizierung (passiert in ClusterAuthChallenge)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Tracing und Dedupe | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Erwarteter Empfänger | Ja |
| TimestampMs | long | Epoch ms | Ja |
| Nonce | long | Anti-Replay | Ja |
| NodeRole | string | Rolle des sendenden Knotens | Ja |
| ShardIds | ushort[] | Verantwortliche Shards | Ja |
| ZoneIds | ushort[] | Aktive Zonen | Nein |
| SupportedFeatures | string[] | Feature-Flags | Ja |

### Erwartete Response
- `ClusterHandshakeResponse` (5001)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ClusterHandshakeRequest)]
public class ClusterHandshakeRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ClusterHandshakeRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string NodeRole { get; set; } = default!;
    [Key(7)] public ushort[] ShardIds { get; set; } = Array.Empty<ushort>();
    [Key(8)] public ushort[]? ZoneIds { get; set; }
    [Key(9)] public string[] SupportedFeatures { get; set; } = Array.Empty<string>();
}
```

### Server-Verhalten
- Validiert Nonce/Timestamp und verweigert bei Drift > 2000ms.
- Prüft, ob SenderServerId bereits bekannt ist; aktualisiert Status auf "handshake pending".
- Persistiert Feature-Flags für Routing-Entscheidungen.
- Antwortet deterministisch innerhalb von 500ms, nutzt vorhandene dedupe cache für identische CorrelationId.
- Quarantiniert Unbekannte, wenn TargetServerId nicht mit eigener Id übereinstimmt.

### Receiving Server Verhalten
- Legt Peer-Session an, setzt TLS ALPN kontext.
- Startet Challenge-Flow nach erfolgreicher Response.

### Sending Server Verhalten
- Wiederholt alle 3s bis Response oder 3 Versuche; bei Failover anderer Pfad.
- Stellt sicher, dass Shard/Zone Listen vollständig und sortiert sind.

### Flow-Diagramm
```
Server A                      Server B
  │                              │
  │  ClusterHandshakeRequest     │
  │─────────────────────────────►│
  │                              │
  │  ClusterHandshakeResponse    │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var req = new ClusterHandshakeRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = serverA,
    TargetServerId = serverB,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    NodeRole = "Gateway",
    ShardIds = new ushort[] {1},
    ZoneIds = new ushort[] {1001, 1002},
    SupportedFeatures = new[] {"KeyRotation", "SnapshotSync", "Compression"}
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| UNAUTHORIZED | Zertifikat oder HMAC ungültig |
| CONFLICT | SenderServerId bereits mit anderer Identität verbunden |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ClusterHandshakeResponse | 5001 | Response |
| ClusterAuthChallengeRequest | 5002 | Folgt bei Erfolg |

---

### ClusterHandshakeResponse (5001)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwort auf den Handshake. Bestätigt die Aufnahme des Peers in den Cluster und liefert Server-Epoch sowie akzeptierte Features.

### Im Scope ✅
- Bestätigung oder Ablehnung mit Begründung
- Synchronisation der Cluster-Epoch

### Nicht im Scope ❌
- Challenge-Proof (separat in 5002/5003)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Muss dem Request entsprechen | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Ursprünglicher Initiator | Ja |
| TimestampMs | long | Epoch ms | Ja |
| Nonce | long | Echo oder neues Nonce | Ja |
| Accepted | bool | Handshake akzeptiert | Ja |
| Reason | string? | Ablehnungsgrund | Nein |
| ServerEpoch | long | Gemeinsame Epoch | Ja |
| EnabledFeatures | string[] | Freigeschaltete Features | Ja |

### Erwartete Response
- Keine (Response)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ClusterHandshakeResponse)]
public class ClusterHandshakeResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ClusterHandshakeResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? Reason { get; set; }
    [Key(8)] public long ServerEpoch { get; set; }
    [Key(9)] public string[] EnabledFeatures { get; set; } = Array.Empty<string>();
}
```

### Server-Verhalten
- Verifiziert, dass CorrelationId bekannt und nicht verbraucht ist.
- Speichert ServerEpoch und enabled Features.
- Bei `Accepted=false` markiert Peer als `Rejected` und triggert Audit-Log.
- Antwortet mit gleicher Nonce oder neuem Nonce für nächste Challenge.
- Setzt Timeout auf 2s; wenn überschritten, Wiederholung mit gleicher Payload.

### Receiving Server Verhalten
- Stellt Verbindung auf "authenticated pending challenge".
- Wenn `Accepted=false`, schließt Kanal nach Logeintrag.

### Sending Server Verhalten
- Erwartet Response innerhalb 2s; bei Timeout: 3 Retries.
- Bei Ablehnung: wechselt auf Fallback-Server oder markiert Cluster-Alarm.

### Flow-Diagramm
```
Server B                      Server A
  │                              │
  │  ClusterHandshakeResponse    │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new ClusterHandshakeResponse
{
    CorrelationId = request.CorrelationId,
    SenderServerId = serverB,
    TargetServerId = request.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = request.Nonce,
    Accepted = true,
    ServerEpoch = 42,
    EnabledFeatures = request.SupportedFeatures
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Sender nicht whitelisted |
| CONFLICT | Shard/Zone Ownership widersprüchlich |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ClusterHandshakeRequest | 5000 | Request |
| ClusterAuthChallengeRequest | 5002 | Folgt |

---

### ClusterAuthChallengeRequest (5002)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Selten (Handshake Phase)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Sendet kryptographische Challenge (HMAC oder Signatur über Nonce) um gegenseitige Vertrauensstellung zu prüfen.

### Im Scope ✅
- Challenge Payload mit KeyId
- Channel-Bindings nutzen (TLS Exporter)

### Nicht im Scope ❌
- Key-Rotation (separat 5004/5005)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Peer | Ja |
| TimestampMs | long | Epoch ms | Ja |
| Nonce | long | Random 64bit | Ja |
| KeyId | string | Aktiver Schlüssel | Ja |
| Challenge | byte[] | Signierte Daten | Ja |

### Erwartete Response
- `ClusterAuthChallengeResponse` (5003)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ClusterAuthChallengeRequest)]
public class ClusterAuthChallengeRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ClusterAuthChallengeRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string KeyId { get; set; } = default!;
    [Key(7)] public byte[] Challenge { get; set; } = Array.Empty<byte>();
}
```

### Server-Verhalten
- Prüft KeyId gegen Trust-Store.
- Validiert Nonce uniqueness.
- Speichert Challenge im Cache (CorrelationId -> Nonce, Timestamp).
- Lehnt bei Drift >2s ab.
- Bei Fehler setzt Audit-Log und schließt Verbindung.

### Receiving Server Verhalten
- Berechnet Proof (HMAC/Signature) über Challenge + Nonce.
- Antwortet innerhalb 1s.

### Sending Server Verhalten
- Wiederholt Request nach 2s bei fehlender Response (max 2 Wiederholungen).
- Rotiert Key falls `KeyId` als deprecated markiert.

### Flow-Diagramm
```
Server A                      Server B
  │                              │
  │  ClusterAuthChallengeReq     │
  │─────────────────────────────►│
  │                              │
  │  ClusterAuthChallengeResp    │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var challengeReq = new ClusterAuthChallengeRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = serverA,
    TargetServerId = serverB,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    KeyId = "k-2026-01",
    Challenge = ComputeChallenge()
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| UNAUTHORIZED | KeyId unbekannt |
| TIMEOUT | Keine Antwort erhalten |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ClusterAuthChallengeResponse | 5003 | Response |
| ClusterKeyRotationRequest | 5004 | Folgt bei Rotation |

---

### ClusterAuthChallengeResponse (5003)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Beweist Besitz des geteilten Secrets/Zertifikats durch signierten Proof, bestätigt Nonce und etabliert Session-Level Trust.

### Im Scope ✅
- Proof-Berechnung (HMAC/Signature)
- SessionToken Ausgabe für spätere Kurz-Handshake

### Nicht im Scope ❌
- Key-Rotation Durchführung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch ms | Ja |
| Nonce | long | Echo | Ja |
| Proof | byte[] | Signatur/HMAC | Ja |
| SessionToken | string | Kurzlebiger Token | Ja |
| ProofValidUntilMs | long | Ablaufzeit | Ja |

### Erwartete Response
- Keine (Response)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ClusterAuthChallengeResponse)]
public class ClusterAuthChallengeResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ClusterAuthChallengeResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public byte[] Proof { get; set; } = Array.Empty<byte>();
    [Key(7)] public string SessionToken { get; set; } = default!;
    [Key(8)] public long ProofValidUntilMs { get; set; }
}
```

### Server-Verhalten
- Validiert Proof gegen gespeichertes Challenge.
- Speichert SessionToken für kurzlebigen Re-Handshake (TTL 10m).
- Bei Proof ungültig: markiert Peer als misconfigured, meldet Security Event.
- Aktualisiert Peer-Status auf "trusted".
- Stellt sicher, dass ProofValidUntilMs > TimestampMs.

### Receiving Server Verhalten
- Aktiviert SessionToken Nutzung für folgende Messages.
- Flushed pending outbound queue nach erfolgreicher Prüfung.

### Sending Server Verhalten
- Verwaltet Refresh vor ProofValidUntilMs.
- Loggt RTT und speichert für Gesundheitsdaten.

### Flow-Diagramm
```
Server B                      Server A
  │                              │
  │  ClusterAuthChallengeResp    │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new ClusterAuthChallengeResponse
{
    CorrelationId = challengeReq.CorrelationId,
    SenderServerId = serverB,
    TargetServerId = challengeReq.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = challengeReq.Nonce,
    Proof = ComputeProof(challengeReq),
    SessionToken = GenerateSessionToken(),
    ProofValidUntilMs = NowMs() + 600_000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| UNAUTHORIZED | Proof ungültig |
| TIMEOUT | Challenge abgelaufen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ClusterAuthChallengeRequest | 5002 | Request |
| ClusterKeyRotationRequest | 5004 | Nächster Schritt bei Rotation |

---

### ClusterKeyRotationRequest (5004)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Selten (geplante Rotation)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Kündigt an, dass ein neuer HMAC/Cert Key aktiviert wird. Stellt Rollout-Fenster und KeyId bereit, damit alle Peers synchron rotieren können.

### Im Scope ✅
- KeyId Ankündigung
- Grace-Period / ActivationAtMs

### Nicht im Scope ❌
- Distribution des privaten Keys (out of band)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Peer | Ja |
| TimestampMs | long | Epoch ms | Ja |
| Nonce | long | Anti-Replay | Ja |
| NewKeyId | string | Kommende KeyId | Ja |
| ActivationAtMs | long | Zeitpunkt Aktivierung | Ja |
| GracePeriodMs | int | Parallel nutzbare Zeit | Ja |

### Erwartete Response
- `ClusterKeyRotationResponse` (5005)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ClusterKeyRotationRequest)]
public class ClusterKeyRotationRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ClusterKeyRotationRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string NewKeyId { get; set; } = default!;
    [Key(7)] public long ActivationAtMs { get; set; }
    [Key(8)] public int GracePeriodMs { get; set; }
}
```

### Server-Verhalten
- Validiert, dass ActivationAtMs in Zukunft liegt.
- Stellt sicher, dass GracePeriodMs <= 1h.
- Plant internen Scheduler für Umschaltung.
- Lehnt ab, wenn NewKeyId unbekannt im Trust-Store.
- Protokolliert Änderung audit-sicher.

### Receiving Server Verhalten
- Hinterlegt NewKeyId als "pending".
- Synchronisiert Rollout-Fenster mit Maintenance-Plänen.

### Sending Server Verhalten
- Wiederholt Broadcast an alle bekannten Peers (fan-out).
- Validiert Ack-Rate; wenn <80%, eskalieren.

### Flow-Diagramm
```
Server A                      Server B
  │                              │
  │  ClusterKeyRotationReq       │
  │─────────────────────────────►│
  │                              │
  │  ClusterKeyRotationResp      │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var rotate = new ClusterKeyRotationRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = opsNode,
    TargetServerId = peer,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    NewKeyId = "k-2026-02",
    ActivationAtMs = NowMs() + 30_000,
    GracePeriodMs = 300_000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | NewKeyId nicht erlaubt |
| CONFLICT | Überschneidende Aktivierung geplant |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ClusterKeyRotationResponse | 5005 | Response |
| ClusterAuthChallengeRequest | 5002 | Proof für neuen Key |

---

### ClusterKeyRotationResponse (5005)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt geplante Key-Rotation und gibt lokale Planung zurück. Kann ablehnen, wenn lokale Policies dies verhindern.

### Im Scope ✅
- ACK/NACK mit Reason
- Lokale Activation-Zeit zurückmelden

### Nicht im Scope ❌
- Key-Material-Austausch

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch ms | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Zusage | Ja |
| Reason | string? | Ablehnungsgrund | Nein |
| ActivationAtMs | long | Lokaler Zeitpunkt | Ja |
| GracePeriodMs | int | Lokal akzeptierte Grace | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ClusterKeyRotationResponse)]
public class ClusterKeyRotationResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ClusterKeyRotationResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? Reason { get; set; }
    [Key(8)] public long ActivationAtMs { get; set; }
    [Key(9)] public int GracePeriodMs { get; set; }
}
```

### Server-Verhalten
- Speichert vereinbarte Zeiten.
- Wenn `Accepted=false`, setzt Alarm und fordert manuelles Eingreifen.
- Wiederverwendet dedupe für gleiche CorrelationId.
- Prüft Drift zwischen ActivationAtMs beider Seiten.
- Meldet Kennzahlen an Observability.

### Receiving Server Verhalten
- Aktualisiert lokalen Plan.
- Triggert Challenge mit neuem Key unmittelbar vor Aktivierung.

### Sending Server Verhalten
- Bei Ablehnung: rollt plan zurück oder versucht neuen Slot.

### Flow-Diagramm
```
Server B                      Server A
  │                              │
  │  ClusterKeyRotationResp      │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var ack = new ClusterKeyRotationResponse
{
    CorrelationId = rotate.CorrelationId,
    SenderServerId = peer,
    TargetServerId = rotate.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = rotate.Nonce,
    Accepted = true,
    ActivationAtMs = rotate.ActivationAtMs,
    GracePeriodMs = rotate.GracePeriodMs
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| CONFLICT | Lokale Wartung verhindert Rotation |
| FORBIDDEN | KeyId Policy verhindert Nutzung |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ClusterKeyRotationRequest | 5004 | Request |
| ClusterAuthChallengeRequest | 5002 | Proof kurz vor Aktivierung |

---

### HealthStatusRequest (5006)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Häufig (5s)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Pingt Peer mit aktuellem Gesundheitsstatus. Dient als aktiver Heartbeat und liefert Telemetrie für Routing und Rebalancing.

### Im Scope ✅
- Health Snapshot (CPU, MEM, TickDelay, QueueDepth, Players)
- Cluster Status Flag (Degraded, Healthy)

### Nicht im Scope ❌
- Load Balancing Entscheidungen (nur Input)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Peer | Ja |
| TimestampMs | long | Epoch ms | Ja |
| Nonce | long | Anti-Replay | Ja |
| CpuUsage | float | 0-100 | Ja |
| MemoryUsageMb | int | MB | Ja |
| TickBehindMs | int | Verzögerung | Ja |
| QueueDepth | int | Outbox backlog | Ja |
| PlayerCount | int | Aktive Spieler | Ja |
| InstanceCount | int | Aktive Instanzen | Ja |

### Erwartete Response
- `HealthStatusResponse` (5007)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HealthStatusRequest)]
public class HealthStatusRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.HealthStatusRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public float CpuUsage { get; set; }
    [Key(7)] public int MemoryUsageMb { get; set; }
    [Key(8)] public int TickBehindMs { get; set; }
    [Key(9)] public int QueueDepth { get; set; }
    [Key(10)] public int PlayerCount { get; set; }
    [Key(11)] public int InstanceCount { get; set; }
}
```

### Server-Verhalten
- Aggregiert Werte und aktualisiert Routing Tables.
- Triggert Alert bei TickBehindMs > 80ms.
- Antwortet innerhalb 500ms mit Echo + Bewertung.
- Speichert History (Rolling Window 1m) für Trendanalyse.
- Wenn Peer nicht authentifiziert -> Reject.

### Receiving Server Verhalten
- Aktualisiert Sicht auf Sender; kann Balancer anstoßen.
- Wenn Werte kritisch -> sendet BackpressureAlertRequest (5046).

### Sending Server Verhalten
- Sendet jittered (±500ms) um Thundering Herd zu vermeiden.
- Nutzt dedupe falls Resend nötig.

### Flow-Diagramm
```
Server A                      Server B
  │                              │
  │  HealthStatusRequest         │
  │─────────────────────────────►│
  │                              │
  │  HealthStatusResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var health = new HealthStatusRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = gateway,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    CpuUsage = 62.4f,
    MemoryUsageMb = 8123,
    TickBehindMs = 4,
    QueueDepth = 12,
    PlayerCount = 524,
    InstanceCount = 12
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| RATE_LIMITED | Zu häufig gesendet |
| UNAUTHORIZED | Peer nicht vertraut |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| HealthStatusResponse | 5007 | Response |
| BackpressureAlertRequest | 5046 | Folgt bei hoher Last |

---

### HealthStatusResponse (5007)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwortet mit Bewertung des empfangenen Health Snapshots und optionalen Steuerhinweisen (z.B. Drosseln, Drain).

### Im Scope ✅
- Bewertung: `Healthy`, `Degraded`, `Critical`
- Handlungsempfehlungen (Throttle, Drain, Pause Replication)

### Nicht im Scope ❌
- Direkte Steuerbefehle (separat NodeDrainRequest)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Status | string | Healthy/Degraded/Critical | Ja |
| Advice | string | Textuelle Empfehlung | Nein |
| RttMs | int | Roundtrip | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HealthStatusResponse)]
public class HealthStatusResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.HealthStatusResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string Status { get; set; } = default!;
    [Key(7)] public string? Advice { get; set; }
    [Key(8)] public int? RttMs { get; set; }
}
```

### Server-Verhalten
- Berechnet RttMs = now - request.TimestampMs.
- Persistiert Status in Peer-State.
- Wenn `Status=Critical`, löst CircuitBreakerStateRequest (5044) aus.
- Antwortet idempotent; Duplikate werden gedroppt.
- Logs enthalten keine sensitiven Inhalte.

### Receiving Server Verhalten
- Befolgt Advice wenn Policy erlaubt (z.B. Soft Drain).
- Aktualisiert eigene Health-Telemetrie.

### Sending Server Verhalten
- Füllt Advice basierend auf Thresholds.
- Limit: Max 1 Response pro Request.

### Flow-Diagramm
```
Server B                      Server A
  │                              │
  │  HealthStatusResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new HealthStatusResponse
{
    CorrelationId = request.CorrelationId,
    SenderServerId = gateway,
    TargetServerId = request.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = request.Nonce,
    Status = "Healthy",
    Advice = "continue",
    RttMs = (int)(NowMs() - request.TimestampMs)
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INTERNAL_ERROR | Verarbeitung fehlgeschlagen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| HealthStatusRequest | 5006 | Request |
| BackpressureAlertRequest | 5046 | Bei Critical Zustand |

---

### LoadReportRequest (5008)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel (10s)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Aggregierter Load-Report pro Shard/Zone für Gateway Routing-Entscheidungen und Auto-Scaling.

### Im Scope ✅
- PlayerCount, InstanceCount pro Shard
- QueueDepth und CPU pro Node

### Nicht im Scope ❌
- Einzelne Spielerlisten (privacy)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Empfänger (z.B. Gateway) | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| ShardLoads | LoadEntry[] | Auflistung | Ja |

**LoadEntry**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ShardId | ushort | Shard | Ja |
| ZoneCount | int | Anzahl Zonen | Ja |
| PlayerCount | int | Spieler gesamt | Ja |
| AvgTickMs | float | Durchschnitt Tick Delay | Ja |
| QueueDepth | int | Outbox Queue | Ja |

### Erwartete Response
- `LoadReportResponse` (5009)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LoadReportRequest)]
public class LoadReportRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LoadReportRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public LoadEntry[] ShardLoads { get; set; } = Array.Empty<LoadEntry>();
}

[MessagePackObject]
public class LoadEntry
{
    [Key(0)] public ushort ShardId { get; set; }
    [Key(1)] public int ZoneCount { get; set; }
    [Key(2)] public int PlayerCount { get; set; }
    [Key(3)] public float AvgTickMs { get; set; }
    [Key(4)] public int QueueDepth { get; set; }
}
```

### Server-Verhalten
- Normalisiert Werte, entfernt Ausreißer.
- Aktualisiert Routing-Table (least-loaded zone).
- Antwortet mit Bewertung und Zielkapazität.
- Bei fehlenden Daten -> setzt Fehlercode.
- Idempotent pro CorrelationId.

### Receiving Server Verhalten
- Nutzt Daten für Balancer Decision Cache.
- Kann Rebalancing planen.

### Sending Server Verhalten
- Aggregiert alle 10s; Jitter ±2s.
- Berücksichtigt nur aktive Shards.

### Flow-Diagramm
```
Server A                      Server B
  │                              │
  │  LoadReportRequest           │
  │─────────────────────────────►│
  │                              │
  │  LoadReportResponse          │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var report = new LoadReportRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = gateway,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    ShardLoads = new[]
    {
        new LoadEntry { ShardId = 1, ZoneCount = 12, PlayerCount = 1500, AvgTickMs = 6.2f, QueueDepth = 42 }
    }
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INTERNAL_ERROR | Aggregation fehlgeschlagen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| LoadReportResponse | 5009 | Response |
| BackpressureAlertRequest | 5046 | Bei Überlast |

---

### LoadReportResponse (5009)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwortet mit Einschätzung und optionalen Balancing-Anweisungen (z.B. verschiebe Spieler auf Shard X).

### Im Scope ✅
- Bewertung pro Shard
- Empfohlene Ziele für Transfers

### Nicht im Scope ❌
- Erzwungene Migration (separat Transfer-Requests)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Recommendations | LoadRecommendation[] | Transferhinweise | Nein |
| Status | string | `Healthy`/`Hot`/`Critical` | Ja |

**LoadRecommendation**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| FromShard | ushort | Quelle |
| ToShard | ushort | Ziel |
| MaxPlayers | int | Anzahl zu verlegender Spieler |
| Reason | string | Begründung |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LoadReportResponse)]
public class LoadReportResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LoadReportResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string Status { get; set; } = default!;
    [Key(7)] public LoadRecommendation[]? Recommendations { get; set; }
}

[MessagePackObject]
public class LoadRecommendation
{
    [Key(0)] public ushort FromShard { get; set; }
    [Key(1)] public ushort ToShard { get; set; }
    [Key(2)] public int MaxPlayers { get; set; }
    [Key(3)] public string Reason { get; set; } = string.Empty;
}
```

### Server-Verhalten
- Loggt Empfehlung audit-sicher.
- Bei Status=Critical, sendet BackpressureAlertRequest (5046).
- Bei Status=Hot, plant schrittweise Transfers.
- Dedupe Responses pro CorrelationId.
- Response-Time SLA 1s.

### Receiving Server Verhalten
- Legt Empfehlungen in Planner-Queue.
- Priorisiert Transfers nach Reason.

### Sending Server Verhalten
- Berechnet Status basierend auf Thresholds.
- Gibt keine PII weiter.

### Flow-Diagramm
```
Server B                      Server A
  │                              │
  │  LoadReportResponse          │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new LoadReportResponse
{
    CorrelationId = report.CorrelationId,
    SenderServerId = gateway,
    TargetServerId = report.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = report.Nonce,
    Status = "Hot",
    Recommendations = new[]
    {
        new LoadRecommendation { FromShard = 1, ToShard = 2, MaxPlayers = 200, Reason = "Shard1 queue depth high" }
    }
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INTERNAL_ERROR | Berechnung fehlgeschlagen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| LoadReportRequest | 5008 | Request |
| BackpressureAlertRequest | 5046 | Bei Critical |

---

### TimeSyncRequest (5010)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel (30s)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Synchronisiert Zeit zwischen Knoten, damit Revisionen monotone Ordnung behalten und Timers korrekt laufen.

### Im Scope ✅
- Timestamps, RTT, Offset Berechnung
- Epoch Versionierung

### Nicht im Scope ❌
- Clientzeit (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Peer | Ja |
| TimestampMs | long | Lokale Zeit | Ja |
| Nonce | long | Anti-Replay | Ja |
| Epoch | long | Lokale Epoch | Ja |
| MonotonicCounter | long | Monotone Sequenz | Ja |

### Erwartete Response
- `TimeSyncResponse` (5011)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TimeSyncRequest)]
public class TimeSyncRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TimeSyncRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public long Epoch { get; set; }
    [Key(7)] public long MonotonicCounter { get; set; }
}
```

### Server-Verhalten
- Berechnet RTT, Offset.
- Validiert Epoch; bei Unterschied >1, fordert EpochSync an.
- Aktualisiert Monotonic Drift Fenster.
- Dedupe auf CorrelationId.
- SLA 200ms Antwort.

### Receiving Server Verhalten
- Sendet Response mit eigener Zeit und Counter.
- Aktualisiert Peerliste.

### Sending Server Verhalten
- Nutzt Werte zur Korrektur eigener monotonic clock offset.

### Flow-Diagramm
```
Server A                      Server B
  │                              │
  │  TimeSyncRequest             │
  │─────────────────────────────►│
  │                              │
  │  TimeSyncResponse            │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var ts = new TimeSyncRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = zoneB,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    Epoch = 42,
    MonotonicCounter = 123456
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| TIMEOUT | Keine Antwort |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| TimeSyncResponse | 5011 | Response |
| EpochSyncRequest | 5064 | Bei epoch mismatch |

---

### TimeSyncResponse (5011)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwortet mit eigener Zeit, berechnetem Offset und gültiger Epoch; ermöglicht Drift-Kompensation.

### Im Scope ✅
- Offset Berechnung
- Echo von MonotonicCounter

### Nicht im Scope ❌
- Korrektur von persistenter Uhr (out of band)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Eigenzeit | Ja |
| Nonce | long | Echo | Ja |
| Epoch | long | Responder Epoch | Ja |
| MonotonicCounter | long | Responder Counter | Ja |
| OffsetMs | long | Berechneter Offset | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TimeSyncResponse)]
public class TimeSyncResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TimeSyncResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public long Epoch { get; set; }
    [Key(7)] public long MonotonicCounter { get; set; }
    [Key(8)] public long OffsetMs { get; set; }
}
```

### Server-Verhalten
- Berechnet Offset = responder.Timestamp - request.Timestamp - RTT/2.
- Speichert offset für spätere Message Orderedness.
- Wenn Epoch unterschiedlich -> startet EpochSyncRequest.
- Response idempotent.
- SLA 200ms.

### Receiving Server Verhalten
- Aktualisiert lokale Drift-Korrektur.
- Loggt große Offsets (>50ms).

### Sending Server Verhalten
- Berechnet Offset vor Senden.
- Nutzt Nonce um Replay zu verhindern.

### Flow-Diagramm
```
Server B                      Server A
  │                              │
  │  TimeSyncResponse            │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new TimeSyncResponse
{
    CorrelationId = req.CorrelationId,
    SenderServerId = zoneB,
    TargetServerId = req.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = req.Nonce,
    Epoch = 42,
    MonotonicCounter = 223344,
    OffsetMs = 3
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| TIMEOUT | Response zu spät |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| TimeSyncRequest | 5010 | Request |
| EpochSyncRequest | 5064 | Bei Konflikt |

---

### NodeDrainRequest (5012)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Initiiert kontrolliertes Drain eines Knotens (z.B. für Wartung). Informiert Peers, damit keine neuen Spieler/Entities dorthin geroutet werden.

### Im Scope ✅
- DrainMode Aktivierung
- Deadline für Abschluss

### Nicht im Scope ❌
- Erzwungener Shutdown (separat Admin Tools)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Peer | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| DrainReason | string | Wartung, Deploy, Incident | Ja |
| DeadlineMs | long | Zeitpunkt für Abschluss | Ja |
| AllowedNewSessions | bool | Dürfen neue Sessions rein? | Ja |

### Erwartete Response
- `NodeDrainResponse` (5013)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.NodeDrainRequest)]
public class NodeDrainRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.NodeDrainRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string DrainReason { get; set; } = default!;
    [Key(7)] public long DeadlineMs { get; set; }
    [Key(8)] public bool AllowedNewSessions { get; set; }
}
```

### Server-Verhalten
- Markiert sich als draining und stoppt neue Sessions wenn AllowedNewSessions=false.
- Informiert Router Tabellen.
- Stellt sicher DeadlineMs > now.
- Initiert Transfer von Spielern/Instanzen.
- Antwortet innerhalb 1s.

### Receiving Server Verhalten
- Aktualisiert Routing um Drain-Knoten zu vermeiden.
- Plant Transfers zu alternativen Zonen.

### Sending Server Verhalten
- Startet Monitoring, sendet periodisch Progress via LoadReport.

### Flow-Diagramm
```
Server A                      Server B
  │                              │
  │  NodeDrainRequest            │
  │─────────────────────────────►│
  │                              │
  │  NodeDrainResponse           │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var drain = new NodeDrainRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = orchestrator,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    DrainReason = "Rolling update",
    DeadlineMs = NowMs() + 900_000,
    AllowedNewSessions = false
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Peer darf Drain nicht anstoßen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| NodeDrainResponse | 5013 | Response |
| BackpressureAlertRequest | 5046 | Bei Überlast während Drain |

---

### NodeDrainResponse (5013)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt Drain-Anfrage und signalisiert, ob Routing angepasst und Transfers gestartet wurden.

### Im Scope ✅
- Bestätigung / Ablehnung
- Fortschrittsindikatoren

### Nicht im Scope ❌
- Detaillierte Transferlisten (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Drain akzeptiert | Ja |
| Reason | string? | Ablehnungsgrund | Nein |
| EstimatedCompletionMs | long | Erwartete Dauer | Ja |
| ActiveTransfers | int | Anzahl laufender Transfers | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.NodeDrainResponse)]
public class NodeDrainResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.NodeDrainResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? Reason { get; set; }
    [Key(8)] public long EstimatedCompletionMs { get; set; }
    [Key(9)] public int ActiveTransfers { get; set; }
}
```

### Server-Verhalten
- Aktualisiert Deployment Orchestrator Status.
- Bei Accepted=false -> stoppt Drain im Initiator.
- Sendet Follow-up bei Verzögerungen über DeadlineMs.
- Response ist idempotent.
- Loggt progress.

### Receiving Server Verhalten
- Plananpassung bei Ablehnung.
- Sonst: überwacht ActiveTransfers.

### Sending Server Verhalten
- Berechnet realistische EstimatedCompletionMs aus Queue/Players.

### Flow-Diagramm
```
Server B                      Server A
  │                              │
  │  NodeDrainResponse           │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new NodeDrainResponse
{
    CorrelationId = drain.CorrelationId,
    SenderServerId = orchestrator,
    TargetServerId = drain.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = drain.Nonce,
    Accepted = true,
    EstimatedCompletionMs = NowMs() + 600_000,
    ActiveTransfers = 120
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| CONFLICT | Andere Wartung läuft |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| NodeDrainRequest | 5012 | Request |
| BackpressureAlertRequest | 5046 | Falls Engpässe |

---

### PlayerTransferPrepareRequest (5014)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel (pro Transfer)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Leitet Spieler- oder Charaktertransfer ein. Reserviert Slot beim Ziel, sperrt Zustand beim Quellserver und liefert TransferTicket.

### Im Scope ✅
- TransferTicket Erstellung
- State Lock für Player

### Nicht im Scope ❌
- Finaler Commit (5016)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Quelle | Ja |
| TargetServerId | Guid | Ziel | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| PlayerId | long | Spieler | Ja |
| CharacterId | long | Charakter | Ja |
| FromZoneId | ushort | Ursprungszone | Ja |
| ToZoneId | ushort | Zielzone | Ja |
| ExpectedLoad | int | Erwartete zusätzliche Last | Nein |

### Erwartete Response
- `PlayerTransferPrepareResponse` (5015)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayerTransferPrepareRequest)]
public class PlayerTransferPrepareRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayerTransferPrepareRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public long PlayerId { get; set; }
    [Key(7)] public long CharacterId { get; set; }
    [Key(8)] public ushort FromZoneId { get; set; }
    [Key(9)] public ushort ToZoneId { get; set; }
    [Key(10)] public int? ExpectedLoad { get; set; }
}
```

### Server-Verhalten
- Lockt Player-Session (write lock).
- Prüft ToZoneId Kapazität.
- Generiert TransferTicket mit TTL 10s.
- Bei Engpass -> BackpressureAlertResponse mit Failure.
- Antwortet innerhalb 500ms.

### Receiving Server Verhalten
- Reserviert Slot, lädt notwendige Assets.
- Speichert erwarteten Arrival Zeit.

### Sending Server Verhalten
- Wartet auf Response bevor Spieler informiert wird.
- Bei Timeout -> abort.

### Flow-Diagramm
```
Server Src                    Server Dst
  │                              │
  │  PlayerTransferPrepareReq    │
  │─────────────────────────────►│
  │                              │
  │  PlayerTransferPrepareResp   │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var prepare = new PlayerTransferPrepareRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = zoneB,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    PlayerId = 1234,
    CharacterId = 5678,
    FromZoneId = 1001,
    ToZoneId = 1002,
    ExpectedLoad = 1
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| CONFLICT | Player bereits transferiert |
| RATE_LIMITED | Zu viele Transfers gleichzeit |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PlayerTransferPrepareResponse | 5015 | Response |
| PlayerTransferCommitRequest | 5016 | Folgt bei Erfolg |

---

### PlayerTransferPrepareResponse (5015)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt Reservierung und liefert TransferTicket. Bei Ablehnung liefert Grund und optional Alternative Zone.

### Im Scope ✅
- TransferTicket
- Alternative Routing

### Nicht im Scope ❌
- Commit des Transfers

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Ziel | Ja |
| TargetServerId | Guid | Quelle | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Reservierung erfolgreich | Ja |
| TransferTicket | string? | Serialisierte Ticket Daten | Bei Erfolg |
| TicketExpiresAtMs | long? | Ablauf | Bei Erfolg |
| AltZoneId | ushort? | Alternative | Nein |
| Reason | string? | Ablehnung | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayerTransferPrepareResponse)]
public class PlayerTransferPrepareResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayerTransferPrepareResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? TransferTicket { get; set; }
    [Key(8)] public long? TicketExpiresAtMs { get; set; }
    [Key(9)] public ushort? AltZoneId { get; set; }
    [Key(10)] public string? Reason { get; set; }
}
```

### Server-Verhalten
- Validiert TransferTicket strukturell.
- Wenn Accepted=false: gibt Reason und AltZoneId.
- Speichert Ticket für späteren Commit.
- Antwortet idempotent.
- Logs ohne PII.

### Receiving Server Verhalten
- Bei Accepted=true: fährt mit Commit fort.
- Bei Ablehnung: wählt alternative Zone.

### Sending Server Verhalten
- Stellt sicher, dass TicketExpiresAtMs > now+3s.
- Dedupe Response bei Retries.

### Flow-Diagramm
```
Server Dst                    Server Src
  │                              │
  │  PlayerTransferPrepareResp   │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var prepResp = new PlayerTransferPrepareResponse
{
    CorrelationId = prepare.CorrelationId,
    SenderServerId = zoneB,
    TargetServerId = prepare.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = prepare.Nonce,
    Accepted = true,
    TransferTicket = Convert.ToBase64String(CreateTicketBytes()),
    TicketExpiresAtMs = NowMs() + 10_000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| CONFLICT | Slot nicht verfügbar |
| EXPIRED | Ticket bereits abgelaufen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PlayerTransferPrepareRequest | 5014 | Request |
| PlayerTransferCommitRequest | 5016 | Nächster Schritt |

---

### PlayerTransferCommitRequest (5016)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Überträgt final den Spieler an das Ziel. Enthält State-Snapshot und TransferTicket zur Validierung.

### Im Scope ✅
- Finaler Handoff
- State Snapshot Transfer

### Nicht im Scope ❌
- Rollback (5018)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Quelle | Ja |
| TargetServerId | Guid | Ziel | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| TransferTicket | string | Ticket aus Prepare | Ja |
| PlayerState | byte[] | Serialisierter State | Ja |
| SnapshotRevision | long | Revision | Ja |

### Erwartete Response
- `PlayerTransferCommitResponse` (5017)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayerTransferCommitRequest)]
public class PlayerTransferCommitRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayerTransferCommitRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string TransferTicket { get; set; } = default!;
    [Key(7)] public byte[] PlayerState { get; set; } = Array.Empty<byte>();
    [Key(8)] public long SnapshotRevision { get; set; }
}
```

### Server-Verhalten
- Validiert Ticket und Revision.
- Wendet PlayerState an und setzt Session aktiv.
- Quellt den alten Server ab (ack commit).
- Antwortet innerhalb 1s.
- Bei Fehler -> sendet AbortResponse.

### Receiving Server Verhalten
- Startet Session im Ziel.
- Ack commit erst nach erfolgreich geladenem State.

### Sending Server Verhalten
- Pausiert Outgoing Messages zum Client bis Commit bestätigt.

### Flow-Diagramm
```
Server Src                    Server Dst
  │                              │
  │  PlayerTransferCommitReq     │
  │─────────────────────────────►│
  │                              │
  │  PlayerTransferCommitResp    │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var commit = new PlayerTransferCommitRequest
{
    CorrelationId = prepare.CorrelationId,
    SenderServerId = zoneA,
    TargetServerId = zoneB,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    TransferTicket = prepResp.TransferTicket!,
    PlayerState = SerializeState(player),
    SnapshotRevision = 99
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| TICKET_INVALID | Ticket nicht gültig |
| STATE_CONFLICT | Revision stimmt nicht |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PlayerTransferCommitResponse | 5017 | Response |
| PlayerTransferAbortRequest | 5018 | Bei Fehler |

---

### PlayerTransferCommitResponse (5017)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt finalen Transfer. Liefert neuen SessionKey und Position.

### Im Scope ✅
- Erfolgsstatus
- Neuer SessionKey

### Nicht im Scope ❌
- Rollback (separat 5018/5019)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Ziel | Ja |
| TargetServerId | Guid | Quelle | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Success | bool | Erfolg | Ja |
| ErrorCode | string? | Fehlercode | Nein |
| ErrorMessage | string? | Message | Nein |
| NewSessionKey | string? | Neuer Key | Bei Erfolg |
| SpawnPosition | float[]? | x,y | Bei Erfolg |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayerTransferCommitResponse)]
public class PlayerTransferCommitResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayerTransferCommitResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Success { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
    [Key(8)] public string? ErrorMessage { get; set; }
    [Key(9)] public string? NewSessionKey { get; set; }
    [Key(10)] public float[]? SpawnPosition { get; set; }
}
```

### Server-Verhalten
- Wenn Success=true: schließt alte Session sauber.
- Wenn Success=false: sendet AbortRequest mit Reason.
- Response idempotent.
- Loggt Duration.
- Updates metrics transfer_success/fail.

### Receiving Server Verhalten
- Bei Erfolg: bestätigt Client Reconnect auf neuem SessionKey.
- Bei Fehler: rollt zurück.

### Sending Server Verhalten
- Setzt SpawnPosition nach Zielzonenregeln.

### Flow-Diagramm
```
Server Dst                    Server Src
  │                              │
  │  PlayerTransferCommitResp    │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new PlayerTransferCommitResponse
{
    CorrelationId = commit.CorrelationId,
    SenderServerId = zoneB,
    TargetServerId = commit.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = commit.Nonce,
    Success = true,
    NewSessionKey = Guid.NewGuid().ToString(),
    SpawnPosition = new []{123.4f, 567.8f}
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| STATE_CONFLICT | SnapshotRevision mismatch |
| INTERNAL_ERROR | Laden fehlgeschlagen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PlayerTransferCommitRequest | 5016 | Request |
| PlayerTransferAbortRequest | 5018 | Bei Misserfolg |

---

### PlayerTransferAbortRequest (5018)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bricht laufenden Transfer ab, gibt Locks frei und stellt sicher, dass Player auf Quelle verbleibt.

### Im Scope ✅
- Lock Release
- TransferTicket Invalidierung

### Nicht im Scope ❌
- Retry-Planung (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Quelle/Ziel | Ja |
| TargetServerId | Guid | Gegenstelle | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| TransferTicket | string | Ticket | Ja |
| Reason | string | Grund | Ja |

### Erwartete Response
- `PlayerTransferAbortResponse` (5019)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayerTransferAbortRequest)]
public class PlayerTransferAbortRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayerTransferAbortRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string TransferTicket { get; set; } = default!;
    [Key(7)] public string Reason { get; set; } = default!;
}
```

### Server-Verhalten
- Invalidiert Ticket.
- Gibt Locks frei und stellt Player auf ursprüngliche Zone zurück.
- Loggt Reason; markiert Transfer as failed.
- Response innerhalb 500ms.
- Idempotent.

### Receiving Server Verhalten
- Stoppt lokale Transferprozesse.
- Prüft ob Spieler schon übernommen -> ggf. Rollback.

### Sending Server Verhalten
- Nutzt selbe CorrelationId wie Prepare/Commit.

### Flow-Diagramm
```
Server X                      Server Y
  │                              │
  │  PlayerTransferAbortReq      │
  │─────────────────────────────►│
  │                              │
  │  PlayerTransferAbortResp     │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var abort = new PlayerTransferAbortRequest
{
    CorrelationId = commit.CorrelationId,
    SenderServerId = zoneB,
    TargetServerId = zoneA,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    TransferTicket = commit.TransferTicket,
    Reason = "State apply failed"
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| NOT_FOUND | Ticket unbekannt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PlayerTransferAbortResponse | 5019 | Response |
| PlayerTransferCommitRequest | 5016 | Ursprünglicher Commit |

---

### PlayerTransferAbortResponse (5019)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt den Abbruch. Kommuniziert den finalen Player-Standort.

### Im Scope ✅
- Bestätigung Abbruch
- Aktueller Standort

### Nicht im Scope ❌
- Retry-Planung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Success | bool | Abbruch erfolgreich | Ja |
| PlayerLocatedAt | ushort | ZoneId | Ja |
| ErrorCode | string? | Fehlercode | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayerTransferAbortResponse)]
public class PlayerTransferAbortResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayerTransferAbortResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Success { get; set; }
    [Key(7)] public ushort PlayerLocatedAt { get; set; }
    [Key(8)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Setzt Session auf PlayerLocatedAt.
- Wenn Success=false, eskaliert Incident.
- Idempotent Response.
- Aktualisiert Routing.
- Loggt audit-sicher.

### Receiving Server Verhalten
- Finalisiert Abbruch, entfernt Ticket.
- Informiert Client via Gateway falls nötig.

### Sending Server Verhalten
- Gibt PlayerLocatedAt an.

### Flow-Diagramm
```
Server Y                      Server X
  │                              │
  │  PlayerTransferAbortResp     │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new PlayerTransferAbortResponse
{
    CorrelationId = abort.CorrelationId,
    SenderServerId = zoneA,
    TargetServerId = abort.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = abort.Nonce,
    Success = true,
    PlayerLocatedAt = 1001
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INTERNAL_ERROR | Konnte nicht zurückrollen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PlayerTransferAbortRequest | 5018 | Request |
| PlayerTransferCommitResponse | 5017 | Ausgangspunkt |

---

### EntityHandoffRequest (5020)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Übergibt Ownership eines NPC/Entity an anderes Node (z.B. Zonenübergang, Lastausgleich).

### Im Scope ✅
- Entity Snapshot
- Lease Übergabe

### Nicht im Scope ❌
- Spielertransfer (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Quelle | Ja |
| TargetServerId | Guid | Ziel | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| EntityId | long | Entity | Ja |
| EntityType | string | Typ | Ja |
| State | byte[] | Snapshot | Ja |
| LeaseId | Guid | Aktuelle Lease | Ja |

### Erwartete Response
- `EntityHandoffResponse` (5021)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EntityHandoffRequest)]
public class EntityHandoffRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EntityHandoffRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public long EntityId { get; set; }
    [Key(7)] public string EntityType { get; set; } = default!;
    [Key(8)] public byte[] State { get; set; } = Array.Empty<byte>();
    [Key(9)] public Guid LeaseId { get; set; }
}
```

### Server-Verhalten
- Validiert Lease Ownership.
- Pausiert Entity Updates bis Response.
- Antwortet innerhalb 1s.
- Bei Fehler -> behält Ownership und meldet Incident.
- Dedupe per CorrelationId.

### Receiving Server Verhalten
- Übernimmt Lease, validiert State.
- Sendet Response nach erfolgreicher Anwendung.

### Sending Server Verhalten
- Notiert Timeout für Revert (3s).

### Flow-Diagramm
```
Server Src                    Server Dst
  │                              │
  │  EntityHandoffRequest        │
  │─────────────────────────────►│
  │                              │
  │  EntityHandoffResponse       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var handoff = new EntityHandoffRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = zoneB,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    EntityId = 9001,
    EntityType = "EliteMob",
    State = SerializeNpcState(npc),
    LeaseId = currentLease
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| LEASE_INVALID | LeaseId gehört nicht Sender |
| STATE_CONFLICT | State unvollständig |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| EntityHandoffResponse | 5021 | Response |
| LeaseAcquireRequest | 5022 | Folgeaktion |

---

### EntityHandoffResponse (5021)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt Entity-Übernahme und neue Lease Ownership.

### Im Scope ✅
- Lease Transfer Ergebnis
- Fehlergründe bei Ablehnung

### Nicht im Scope ❌
- Langfristige Lease-Verwaltung (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Ziel | Ja |
| TargetServerId | Guid | Quelle | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Success | bool | Übernahme gelungen | Ja |
| NewLeaseId | Guid? | Neue Lease | Bei Erfolg |
| ErrorCode | string? | Fehler | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EntityHandoffResponse)]
public class EntityHandoffResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EntityHandoffResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Success { get; set; }
    [Key(7)] public Guid? NewLeaseId { get; set; }
    [Key(8)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Bei Success: Quelle entfernt Entity und stoppt Updates.
- Bei Failure: Quelle behält Ownership und markiert Peer als "refused".
- Idempotent.
- Logs with correlation.
- SLA 1s.

### Receiving Server Verhalten
- Setzt NewLeaseId als aktiv.
- Bei Failure: sendet BackpressureAlert falls Ursache Last.

### Sending Server Verhalten
- Wenn Failure, plant Retry oder alternative Node.

### Flow-Diagramm
```
Server Dst                    Server Src
  │                              │
  │  EntityHandoffResponse       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new EntityHandoffResponse
{
    CorrelationId = handoff.CorrelationId,
    SenderServerId = zoneB,
    TargetServerId = handoff.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = handoff.Nonce,
    Success = true,
    NewLeaseId = Guid.NewGuid()
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| LEASE_CONFLICT | Andere Lease aktiv |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| EntityHandoffRequest | 5020 | Request |
| LeaseAcquireRequest | 5022 | Folge bei Ablehnung |

---

### LeaseAcquireRequest (5022)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Fordert Lease für einen ResourceKey an, um exklusiven Zugriff zu sichern.

### Im Scope ✅
- Lease Granting
- TTL Negotiation

### Nicht im Scope ❌
- Renew/Release (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Anfragender | Ja |
| TargetServerId | Guid | Lease-Authority | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| ResourceKey | string | z.B. zone:1001 | Ja |
| RequestedDurationMs | int | Wunschdauer | Ja |

### Erwartete Response
- `LeaseAcquireResponse` (5023)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaseAcquireRequest)]
public class LeaseAcquireRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaseAcquireRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string ResourceKey { get; set; } = default!;
    [Key(7)] public int RequestedDurationMs { get; set; }
}
```

### Server-Verhalten
- Prüft ob ResourceKey frei.
- Erzeugt LeaseId und Expiry.
- Antwortet innerhalb 300ms.
- Idempotent pro ResourceKey+Sender.
- Logs for audit.

### Receiving Server Verhalten
- Speichert Lease.
- Startet Renew Timer.

### Sending Server Verhalten
- Dedupe Requests.
- Wählt angemessene Duration (<30s).

### Flow-Diagramm
```
Authority                    Requester
  │                              │
  │  LeaseAcquireRequest         │
  │─────────────────────────────►│
  │                              │
  │  LeaseAcquireResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var acquire = new LeaseAcquireRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = leaseAuthority,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    ResourceKey = "zone:1001",
    RequestedDurationMs = 15000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| LEASE_HELD | Lease bereits vergeben |
| UNAUTHORIZED | Keine Berechtigung |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| LeaseAcquireResponse | 5023 | Response |
| LeaseRenewRequest | 5024 | Folge |

---

### LeaseAcquireResponse (5023)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Liefert Ergebnis der Lease-Anfrage inkl. LeaseId, Dauer und Expiry.

### Im Scope ✅
- Lease Ergebnis
- Laufzeit-Information

### Nicht im Scope ❌
- Verlängerung (Renew)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Authority | Ja |
| TargetServerId | Guid | Antragsteller | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Granted | bool | Erfolg | Ja |
| LeaseId | Guid? | Lease Ident | Bei Erfolg |
| ExpiresAtMs | long? | Ablauf | Bei Erfolg |
| ErrorCode | string? | Fehler | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaseAcquireResponse)]
public class LeaseAcquireResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaseAcquireResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Granted { get; set; }
    [Key(7)] public Guid? LeaseId { get; set; }
    [Key(8)] public long? ExpiresAtMs { get; set; }
    [Key(9)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Bei Granted=true: persistiert LeaseMapping.
- Bei Granted=false: liefert ErrorCode (LEASE_HELD, UNAUTHORIZED).
- Idempotent.
- Logs include expiration.
- SLA 300ms.

### Receiving Server Verhalten
- Startet Renew Timer.
- Bei Ablehnung: wählt Backoff.

### Sending Server Verhalten
- Validiert request prior to respond.

### Flow-Diagramm
```
Authority                    Requester
  │                              │
  │  LeaseAcquireResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new LeaseAcquireResponse
{
    CorrelationId = acquire.CorrelationId,
    SenderServerId = leaseAuthority,
    TargetServerId = acquire.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = acquire.Nonce,
    Granted = true,
    LeaseId = Guid.NewGuid(),
    ExpiresAtMs = NowMs() + 15000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| LEASE_HELD | Andere Lease aktiv |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| LeaseAcquireRequest | 5022 | Request |
| LeaseRenewRequest | 5024 | Folge |

---

### LeaseRenewRequest (5024)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel (vor Expiry)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Verlängert bestehende Lease bevor sie abläuft.

### Im Scope ✅
- Verlängerung
- Neue Expiry Berechnung

### Nicht im Scope ❌
- Neuerwerb (Acquire)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Lease Halter | Ja |
| TargetServerId | Guid | Authority | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| LeaseId | Guid | Lease | Ja |
| RequestedDurationMs | int | Wunschdauer | Ja |

### Erwartete Response
- `LeaseRenewResponse` (5025)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaseRenewRequest)]
public class LeaseRenewRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaseRenewRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public Guid LeaseId { get; set; }
    [Key(7)] public int RequestedDurationMs { get; set; }
}
```

### Server-Verhalten
- Prüft Lease existiert und gehört Sender.
- Verlängert Expiry wenn gültig.
- Antwortet idempotent.
- SLA 200ms.
- Logs extension.

### Receiving Server Verhalten
- Aktualisiert lokale Timer.
- Bei Ablehnung: plant Acquire.

### Sending Server Verhalten
- Sendet früh (60% Laufzeit).
- Dedupes by CorrelationId.

### Flow-Diagramm
```
Authority                    Holder
  │                              │
  │  LeaseRenewRequest           │
  │─────────────────────────────►│
  │                              │
  │  LeaseRenewResponse          │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var renew = new LeaseRenewRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = leaseAuthority,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    LeaseId = leaseResp.LeaseId!.Value,
    RequestedDurationMs = 15000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| LEASE_EXPIRED | Lease nicht mehr gültig |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| LeaseRenewResponse | 5025 | Response |
| LeaseReleaseRequest | 5026 | Alternativ |

---

### LeaseRenewResponse (5025)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt erfolgreiche Verlängerung oder lehnt ab, falls Lease bereits abgelaufen.

### Im Scope ✅
- Neue Expiry
- Ablehnungsgrund

### Nicht im Scope ❌
- Neue Lease Vergabe

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Authority | Ja |
| TargetServerId | Guid | Halter | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Success | bool | Verlängerung ok | Ja |
| ExpiresAtMs | long? | Neuer Ablauf | Bei Erfolg |
| ErrorCode | string? | Fehler | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaseRenewResponse)]
public class LeaseRenewResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaseRenewResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Success { get; set; }
    [Key(7)] public long? ExpiresAtMs { get; set; }
    [Key(8)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Wenn Success=false & Error=LEASE_EXPIRED -> requires Acquire.
- Logs with lease id.
- SLA 200ms.
- Updates metrics.

### Receiving Server Verhalten
- Plant nächsten Renew basierend auf ExpiresAtMs.
- Bei Failure: versucht Acquire.

### Sending Server Verhalten
- Berechnet ExpiresAtMs = now + min(RequestedDuration, PolicyMax).

### Flow-Diagramm
```
Authority                    Holder
  │                              │
  │  LeaseRenewResponse          │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new LeaseRenewResponse
{
    CorrelationId = renew.CorrelationId,
    SenderServerId = leaseAuthority,
    TargetServerId = renew.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = renew.Nonce,
    Success = true,
    ExpiresAtMs = NowMs() + 15000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| LEASE_EXPIRED | Lease abgelaufen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| LeaseRenewRequest | 5024 | Request |
| LeaseReleaseRequest | 5026 | Optional |

---

### LeaseReleaseRequest (5026)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Gibt Lease freiwillig frei.

### Im Scope ✅
- Lease Release
- Cleanup

### Nicht im Scope ❌
- Erzwungene Entziehung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Halter | Ja |
| TargetServerId | Guid | Authority | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| LeaseId | Guid | Lease | Ja |
| Reason | string | Grund | Ja |

### Erwartete Response
- `LeaseReleaseResponse` (5027)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaseReleaseRequest)]
public class LeaseReleaseRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaseReleaseRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public Guid LeaseId { get; set; }
    [Key(7)] public string Reason { get; set; } = default!;
}
```

### Server-Verhalten
- Prüft Lease Ownership.
- Markiert Lease frei.
- Antwortet idempotent.
- SLA 200ms.
- Logs reason.

### Receiving Server Verhalten
- Entfernt Lease aus lokaler Tabelle.
- Stoppt Renew-Timer.

### Sending Server Verhalten
- Entfernt lokale Nutzung nach Response.

### Flow-Diagramm
```
Authority                    Holder
  │                              │
  │  LeaseReleaseRequest         │
  │─────────────────────────────►│
  │                              │
  │  LeaseReleaseResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var release = new LeaseReleaseRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = leaseAuthority,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    LeaseId = leaseResp.LeaseId!.Value,
    Reason = "handoff complete"
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| LEASE_EXPIRED | Lease bereits abgelaufen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| LeaseReleaseResponse | 5027 | Response |
| LeaseAcquireRequest | 5022 | Neuerwerb |

---

### LeaseReleaseResponse (5027)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt Freigabe oder meldet Fehler, falls Lease bereits ausgelaufen.

### Im Scope ✅
- Release Bestätigung
- Fehlercodes

### Nicht im Scope ❌
- Neue Vergabe

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Authority | Ja |
| TargetServerId | Guid | Halter | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Success | bool | Freigabe ok | Ja |
| ErrorCode | string? | Fehler | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaseReleaseResponse)]
public class LeaseReleaseResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaseReleaseResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Success { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Wenn Success=false -> sendet LeaseAcquireResponse mit Error.
- Logs action.
- SLA 200ms.
- Updates metrics.

### Receiving Server Verhalten
- Entfernt lokale Lease-Referenzen.
- Bei Failure: versucht erneut.

### Sending Server Verhalten
- Bestätigt Release oder liefert ErrorCode.

### Flow-Diagramm
```
Authority                    Holder
  │                              │
  │  LeaseReleaseResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new LeaseReleaseResponse
{
    CorrelationId = release.CorrelationId,
    SenderServerId = leaseAuthority,
    TargetServerId = release.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = release.Nonce,
    Success = true
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| LEASE_EXPIRED | Lease nicht mehr vorhanden |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| LeaseReleaseRequest | 5026 | Request |
| LeaseAcquireRequest | 5022 | Wiederantrag |

---

### PartitionOwnershipQueryRequest (5028)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Fragt nach aktuellem Owner einer Partition (Shard/Zone) und dessen Lease-Status.

### Im Scope ✅
- Ownership Info
- Lease Details

### Nicht im Scope ❌
- Ownership Transfer (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Anfragender | Ja |
| TargetServerId | Guid | Authority | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| PartitionKey | string | z.B. shard:1 | Ja |

### Erwartete Response
- `PartitionOwnershipQueryResponse` (5029)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PartitionOwnershipQueryRequest)]
public class PartitionOwnershipQueryRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PartitionOwnershipQueryRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string PartitionKey { get; set; } = default!;
}
```

### Server-Verhalten
- Lookup Ownership Table.
- Antwortet mit OwnerServerId, LeaseId.
- SLA 200ms.
- Idempotent.
- Logs query.

### Receiving Server Verhalten
- Nutzt Info für Routing.
- Bei Owner unbekannt -> startet Acquire.

### Sending Server Verhalten
- Stellt sicher PartitionKey valid.

### Flow-Diagramm
```
Authority                    Requester
  │                              │
  │  PartitionOwnershipQueryReq  │
  │─────────────────────────────►│
  │                              │
  │  PartitionOwnershipQueryResp │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var query = new PartitionOwnershipQueryRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = gateway,
    TargetServerId = leaseAuthority,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    PartitionKey = "shard:1"
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| NOT_FOUND | Partition unbekannt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PartitionOwnershipQueryResponse | 5029 | Response |
| LeaseAcquireRequest | 5022 | Falls frei |

---

### PartitionOwnershipQueryResponse (5029)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwortet mit aktuellem Owner und Lease Informationen.

### Im Scope ✅
- Owner Info
- Lease Expiry

### Nicht im Scope ❌
- Transfer Initiierung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Authority | Ja |
| TargetServerId | Guid | Anfragender | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| OwnerServerId | Guid? | Aktueller Besitzer | Nein |
| LeaseId | Guid? | Lease | Nein |
| ExpiresAtMs | long? | Ablauf | Nein |
| ErrorCode | string? | Fehler | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PartitionOwnershipQueryResponse)]
public class PartitionOwnershipQueryResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PartitionOwnershipQueryResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public Guid? OwnerServerId { get; set; }
    [Key(7)] public Guid? LeaseId { get; set; }
    [Key(8)] public long? ExpiresAtMs { get; set; }
    [Key(9)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Setzt ErrorCode wenn Partition unbekannt.
- Logs OwnerServerId for audit.
- SLA 200ms.
- Provides consistent data snapshot.

### Receiving Server Verhalten
- Aktualisiert Routing.
- Wenn Owner null -> versucht Acquire.

### Sending Server Verhalten
- Liefert aktuelle Information.

### Flow-Diagramm
```
Authority                    Requester
  │                              │
  │  PartitionOwnershipQueryResp │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new PartitionOwnershipQueryResponse
{
    CorrelationId = query.CorrelationId,
    SenderServerId = leaseAuthority,
    TargetServerId = query.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = query.Nonce,
    OwnerServerId = zoneA,
    LeaseId = Guid.NewGuid(),
    ExpiresAtMs = NowMs() + 10000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| NOT_FOUND | Partition unbekannt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PartitionOwnershipQueryRequest | 5028 | Request |
| LeaseAcquireRequest | 5022 | Next |

---

### EventReplicationPublishRequest (5030)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Publiziert Event-Delta (z.B. Guild/Party/Social) an andere Nodes. Stellt Ordering via Revision sicher.

### Im Scope ✅
- EventEnvelope Versand
- Ordering und Dedupe

### Nicht im Scope ❌
- Snapshot Versand (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Publisher | Ja |
| TargetServerId | Guid | Subscriber | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| RoutingKey | string | z.B. guild:123 | Ja |
| Revision | long | Monotone Revision | Ja |
| PayloadType | string | Typ-Name | Ja |
| Payload | byte[] | MessagePack Bytes | Ja |

### Erwartete Response
- `EventReplicationPublishResponse` (5031)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EventReplicationPublishRequest)]
public class EventReplicationPublishRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EventReplicationPublishRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string RoutingKey { get; set; } = default!;
    [Key(7)] public long Revision { get; set; }
    [Key(8)] public string PayloadType { get; set; } = default!;
    [Key(9)] public byte[] Payload { get; set; } = Array.Empty<byte>();
}
```

### Server-Verhalten
- Prüft Revision ordering (must be > last).
- Bei OutOfOrder -> Response mit ErrorCode OutOfOrder.
- Speichert dedupe per (RoutingKey, Revision).
- Response SLA 300ms.
- Supports batching? (not in scope).

### Receiving Server Verhalten
- Wendet Delta an.
- Bei Lücke fordert SnapshotSync.

### Sending Server Verhalten
- Retries mit Backoff bei Throttle.

### Flow-Diagramm
```
Publisher                    Subscriber
  │                              │
  │  EventReplicationPublishReq  │
  │─────────────────────────────►│
  │                              │
  │  EventReplicationPublishResp │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var publish = new EventReplicationPublishRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = socialA,
    TargetServerId = zoneB,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    RoutingKey = "guild:123",
    Revision = 101,
    PayloadType = "GuildRankChanged",
    Payload = SerializeDelta(delta)
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| OUT_OF_ORDER | Revision kleiner als erwartet |
| DUPLICATE | Bereits verarbeitet |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| EventReplicationPublishResponse | 5031 | Response |
| SnapshotSyncRequest | 5062 | Bei Lücke |

---

### EventReplicationPublishResponse (5031)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Quittiert Event-Replikation, signalisiert Erfolg, Duplikat oder Out-of-Order.

### Im Scope ✅
- Ack / Nack
- Gewünschte BaseRevision bei Fehler

### Nicht im Scope ❌
- Snapshot Zusendung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Subscriber | Ja |
| TargetServerId | Guid | Publisher | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |
| ExpectedRevision | long? | Erwartete Revision | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EventReplicationPublishResponse)]
public class EventReplicationPublishResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EventReplicationPublishResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
    [Key(8)] public long? ExpectedRevision { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Wenn Accepted=false und Error=OUT_OF_ORDER -> publisher sendet SnapshotSyncRequest.
- SLA 300ms.
- Logs result.

### Receiving Server Verhalten
- Bei ExpectedRevision gesetzt -> triggert Snapshot.

### Sending Server Verhalten
- Interpretiert ErrorCode, plant Retry.

### Flow-Diagramm
```
Subscriber                    Publisher
  │                              │
  │  EventReplicationPublishResp │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new EventReplicationPublishResponse
{
    CorrelationId = publish.CorrelationId,
    SenderServerId = zoneB,
    TargetServerId = publish.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = publish.Nonce,
    Accepted = false,
    ErrorCode = "OUT_OF_ORDER",
    ExpectedRevision = 102
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| OUT_OF_ORDER | Revision kleiner |
| DUPLICATE | Bereits verarbeitet |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| EventReplicationPublishRequest | 5030 | Request |
| SnapshotSyncRequest | 5062 | Bei Out-of-order |

---

### GuildSyncRequest (5032)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Synchronisiert Gilden-Änderungen zwischen Social/Zone Servern (Rank, MOTD, OnlineStatus).

### Im Scope ✅
- Delta-Events
- Revision Handling

### Nicht im Scope ❌
- Vollständiger Snapshot (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Social | Ja |
| TargetServerId | Guid | Zone | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| GuildId | long | Gilde | Ja |
| Revision | long | Delta Revision | Ja |
| DeltaType | string | z.B. RankUpdate | Ja |
| DeltaPayload | byte[] | Serialized | Ja |

### Erwartete Response
- `GuildSyncResponse` (5033)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildSyncRequest)]
public class GuildSyncRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildSyncRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public long GuildId { get; set; }
    [Key(7)] public long Revision { get; set; }
    [Key(8)] public string DeltaType { get; set; } = default!;
    [Key(9)] public byte[] DeltaPayload { get; set; } = Array.Empty<byte>();
}
```

### Server-Verhalten
- Prüft Revision.
- Wendet Delta an, aktualisiert caches.
- Bei Out-of-order -> fordert Snapshot.
- Antwortet innerhalb 500ms.
- Dedupe pro Guild+Revision.

### Receiving Server Verhalten
- Aktualisiert Guild Cache und Player Views.

### Sending Server Verhalten
- Sendet geordnet.
- Retries mit Backoff.

### Flow-Diagramm
```
Social                       Zone
  │                              │
  │  GuildSyncRequest            │
  │─────────────────────────────►│
  │                              │
  │  GuildSyncResponse           │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var guild = new GuildSyncRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = social,
    TargetServerId = zone,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    GuildId = 999,
    Revision = 45,
    DeltaType = "RankUpdate",
    DeltaPayload = SerializeDelta(rankDelta)
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| OUT_OF_ORDER | Revision kleiner |
| DUPLICATE | Bereits verarbeitet |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| GuildSyncResponse | 5033 | Response |
| SnapshotSyncRequest | 5062 | Bei Lücke |

---

### GuildSyncResponse (5033)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Quittiert Gilden-Sync und meldet erwartete Revisionen.

### Im Scope ✅
- Ack/Nack
- ExpectedRevision

### Nicht im Scope ❌
- Snapshot Sendung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Zone | Ja |
| TargetServerId | Guid | Social | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |
| ExpectedRevision | long? | Erwartete Rev | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildSyncResponse)]
public class GuildSyncResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildSyncResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
    [Key(8)] public long? ExpectedRevision { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Bei Ablehnung -> Publisher fordert Snapshot.
- Logs.
- SLA 500ms.
- Metrics recorded.

### Receiving Server Verhalten
- Interpretiert Error, sendet Snapshot falls nötig.

### Sending Server Verhalten
- Setzt ExpectedRevision wenn Lücke erkannt.

### Flow-Diagramm
```
Zone                         Social
  │                              │
  │  GuildSyncResponse           │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new GuildSyncResponse
{
    CorrelationId = guild.CorrelationId,
    SenderServerId = zone,
    TargetServerId = guild.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = guild.Nonce,
    Accepted = true
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| OUT_OF_ORDER | Revision niedriger |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| GuildSyncRequest | 5032 | Request |
| SnapshotSyncRequest | 5062 | Bei Fehler |

---

### PartySyncRequest (5034)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Synchronisiert Party-Status (Mitglieder, Rollen, ReadyCheck) zwischen Zonen.

### Im Scope ✅
- Party Delta
- Revision

### Nicht im Scope ❌
- Vollständige Party-Liste (Snapshot)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Social/PartyMgr | Ja |
| TargetServerId | Guid | Zone | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| PartyId | long | Party | Ja |
| Revision | long | Delta Revision | Ja |
| DeltaType | string | z.B. MemberJoin | Ja |
| DeltaPayload | byte[] | Serialized | Ja |

### Erwartete Response
- `PartySyncResponse` (5035)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PartySyncRequest)]
public class PartySyncRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PartySyncRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public long PartyId { get; set; }
    [Key(7)] public long Revision { get; set; }
    [Key(8)] public string DeltaType { get; set; } = default!;
    [Key(9)] public byte[] DeltaPayload { get; set; } = Array.Empty<byte>();
}
```

### Server-Verhalten
- Prüft Revision.
- Wendet Delta an.
- Antwortet idempotent.
- Bei Lücke -> erwartet Snapshot.
- SLA 500ms.

### Receiving Server Verhalten
- Aktualisiert Party Member local.

### Sending Server Verhalten
- Sendet in Reihenfolge.
- Backoff bei Throttle.

### Flow-Diagramm
```
PartyMgr                     Zone
  │                              │
  │  PartySyncRequest            │
  │─────────────────────────────►│
  │                              │
  │  PartySyncResponse           │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var party = new PartySyncRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = social,
    TargetServerId = zone,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    PartyId = 555,
    Revision = 12,
    DeltaType = "MemberJoin",
    DeltaPayload = SerializeDelta(joinDelta)
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| OUT_OF_ORDER | Revision niedriger |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PartySyncResponse | 5035 | Response |
| SnapshotSyncRequest | 5062 | Bei Out-of-order |

---

### PartySyncResponse (5035)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Quittiert Party Sync.

### Im Scope ✅
- Ack/Nack
- ExpectedRevision

### Nicht im Scope ❌
- Snapshot Daten

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Zone | Ja |
| TargetServerId | Guid | Social | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |
| ExpectedRevision | long? | Erwartet | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PartySyncResponse)]
public class PartySyncResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PartySyncResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
    [Key(8)] public long? ExpectedRevision { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Bei Error Out-of-order -> triggers Snapshot.
- SLA 500ms.
- Logs.

### Receiving Server Verhalten
- Interpretiert ErrorCode.
- Startet Snapshot falls nötig.

### Sending Server Verhalten
- Setzt ExpectedRevision korrekt.

### Flow-Diagramm
```
Zone                         PartyMgr
  │                              │
  │  PartySyncResponse           │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new PartySyncResponse
{
    CorrelationId = party.CorrelationId,
    SenderServerId = zone,
    TargetServerId = party.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = party.Nonce,
    Accepted = true
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| OUT_OF_ORDER | Revision niedrig |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| PartySyncRequest | 5034 | Request |
| SnapshotSyncRequest | 5062 | Bei Lücke |

---

### ChatRouteRegisterRequest (5036)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Registriert, welche Chat-Kanäle/Partitionen dieser Knoten bedienen kann. Dient als Routing-Tabelle für Chat-Events.

### Im Scope ✅
- Channel Route Registrierung
- TTL für Registrierung

### Nicht im Scope ❌
- Chat-Inhalte (nur Metadaten)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | ChatRouter | Ja |
| TargetServerId | Guid | Directory | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| Channels | string[] | Kanäle (z.B. zone:1001) | Ja |
| ExpiresAtMs | long | Ablauf | Ja |

### Erwartete Response
- `ChatRouteRegisterResponse` (5037)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChatRouteRegisterRequest)]
public class ChatRouteRegisterRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChatRouteRegisterRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string[] Channels { get; set; } = Array.Empty<string>();
    [Key(7)] public long ExpiresAtMs { get; set; }
}
```

### Server-Verhalten
- Aktualisiert Routing Registry.
- Verwirft, wenn ExpiresAtMs < now+5s.
- Antwortet innerhalb 300ms.
- Idempotent pro Channels set.
- Logs changes.

### Receiving Server Verhalten
- Nutzt Daten für Chat Dispatch.
- Plant Refresh vor ExpiresAtMs.

### Sending Server Verhalten
- Sendet Refresh alle ExpiresAtMs - 5s.

### Flow-Diagramm
```
ChatRouter                   Directory
  │                              │
  │  ChatRouteRegisterReq        │
  │─────────────────────────────►│
  │                              │
  │  ChatRouteRegisterResp       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var reg = new ChatRouteRegisterRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = chatNode,
    TargetServerId = directory,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    Channels = new[] {"zone:1001", "guild:*"},
    ExpiresAtMs = NowMs() + 60_000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Channel nicht erlaubt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ChatRouteRegisterResponse | 5037 | Response |
| ChatEnvelopeRelayRequest | 5038 | Nutzung |

---

### ChatRouteRegisterResponse (5037)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt Chat-Routing-Registrierung.

### Im Scope ✅
- Ack/Nack
- TTL Hinweis

### Nicht im Scope ❌
- Chat Nachrichten

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Directory | Ja |
| TargetServerId | Guid | ChatRouter | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |
| ExpiresAtMs | long? | Wenn akzeptiert | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChatRouteRegisterResponse)]
public class ChatRouteRegisterResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChatRouteRegisterResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
    [Key(8)] public long? ExpiresAtMs { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Wenn Accepted=false -> keine Routing-Änderung.
- SLA 300ms.
- Logs accepted channels.

### Receiving Server Verhalten
- Plant Refresh basierend auf ExpiresAtMs.
- Bei Ablehnung: versucht anderen Directory.

### Sending Server Verhalten
- Liefert ExpiresAtMs konservativ.

### Flow-Diagramm
```
Directory                    ChatRouter
  │                              │
  │  ChatRouteRegisterResp       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new ChatRouteRegisterResponse
{
    CorrelationId = reg.CorrelationId,
    SenderServerId = directory,
    TargetServerId = reg.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = reg.Nonce,
    Accepted = true,
    ExpiresAtMs = reg.ExpiresAtMs
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Channel nicht erlaubt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ChatRouteRegisterRequest | 5036 | Request |
| ChatEnvelopeRelayRequest | 5038 | Nutzung |

---

### ChatEnvelopeRelayRequest (5038)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Hoch  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Transportiert Chat-Metadaten (kein Klartext) zwischen Routern, damit Ziel-Server Nachrichten lokal ausliefern kann.

### Im Scope ✅
- Routing Metadata
- Envelope mit Channel und PayloadHash

### Nicht im Scope ❌
- Chat Klartext (bleibt auf Chat-Server)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Router | Ja |
| TargetServerId | Guid | Ziel-Router | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| Channel | string | Chat Channel | Ja |
| PayloadHash | string | Hash des Inhalts | Ja |
| SenderPlayerId | long | Absender | Ja |
| TargetPlayerIds | long[] | Ziele | Ja |
| EnvelopeId | Guid | Envelope | Ja |

### Erwartete Response
- `ChatEnvelopeRelayResponse` (5039)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChatEnvelopeRelayRequest)]
public class ChatEnvelopeRelayRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChatEnvelopeRelayRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string Channel { get; set; } = default!;
    [Key(7)] public string PayloadHash { get; set; } = default!;
    [Key(8)] public long SenderPlayerId { get; set; }
    [Key(9)] public long[] TargetPlayerIds { get; set; } = Array.Empty<long>();
    [Key(10)] public Guid EnvelopeId { get; set; }
}
```

### Server-Verhalten
- Validiert Channel Routing.
- Dedupe via EnvelopeId.
- Antwortet innerhalb 200ms.
- Keine Klartext-Daten speichern.
- Logs contain hash only.

### Receiving Server Verhalten
- Mappt auf lokale Sessions und push Notification.
- Bei unbekanntem Channel -> Nack.

### Sending Server Verhalten
- Berechnet PayloadHash deterministisch.
- Retries bei Nack?

### Flow-Diagramm
```
ChatRouter A                 ChatRouter B
  │                              │
  │  ChatEnvelopeRelayReq        │
  │─────────────────────────────►│
  │                              │
  │  ChatEnvelopeRelayResp       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var relay = new ChatEnvelopeRelayRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = chatA,
    TargetServerId = chatB,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    Channel = "zone:1001",
    PayloadHash = ComputeHash(payload),
    SenderPlayerId = 321,
    TargetPlayerIds = new []{654, 987},
    EnvelopeId = Guid.NewGuid()
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| ROUTE_UNKNOWN | Channel nicht registriert |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ChatEnvelopeRelayResponse | 5039 | Response |
| ChatRouteRegisterRequest | 5036 | Routing Basis |

---

### ChatEnvelopeRelayResponse (5039)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Hoch  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Quittiert Empfang und Zustellung der Chat-Envelope.

### Im Scope ✅
- Ack/Nack
- Delivery Count

### Nicht im Scope ❌
- Message Inhalt

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Ziel-Router | Ja |
| TargetServerId | Guid | Ursprungs-Router | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Erfolg | Ja |
| DeliveredCount | int | Anzahl zugestellt | Ja |
| ErrorCode | string? | Fehler | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChatEnvelopeRelayResponse)]
public class ChatEnvelopeRelayResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChatEnvelopeRelayResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public int DeliveredCount { get; set; }
    [Key(8)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Wenn Accepted=false -> sendet ErrorCode (ROUTE_UNKNOWN, SESSION_MISSING).
- SLA 200ms.
- Logs.

### Receiving Server Verhalten
- Bei Fehler -> meldet ChatRouteRegister refresh.

### Sending Server Verhalten
- Interpretiert DeliveredCount für Metrics.

### Flow-Diagramm
```
ChatRouter B                 ChatRouter A
  │                              │
  │  ChatEnvelopeRelayResp       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new ChatEnvelopeRelayResponse
{
    CorrelationId = relay.CorrelationId,
    SenderServerId = chatB,
    TargetServerId = relay.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = relay.Nonce,
    Accepted = true,
    DeliveredCount = 2
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| ROUTE_UNKNOWN | Channel unbekannt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ChatEnvelopeRelayRequest | 5038 | Request |
| ChatRouteRegisterRequest | 5036 | Routing |

---

### AdminBroadcastRequest (5040)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Verteilt administrative Ankündigungen (Maintenance, Incident) an alle Nodes.

### Im Scope ✅
- Cluster-weite Admin Messages
- Target Scopes

### Nicht im Scope ❌
- Clientdirekte Nachrichten (werden serverseitig verarbeitet)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Empfänger | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| Scope | string | all/gateway/zone | Ja |
| Message | string | Inhalt | Ja |
| Severity | string | info/warn/critical | Ja |

### Erwartete Response
- `AdminBroadcastResponse` (5041)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AdminBroadcastRequest)]
public class AdminBroadcastRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.AdminBroadcastRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string Scope { get; set; } = default!;
    [Key(7)] public string Message { get; set; } = default!;
    [Key(8)] public string Severity { get; set; } = default!;
}
```

### Server-Verhalten
- Verifiziert Sender Berechtigung.
- Veröffentlicht in interne Admin Topic.
- Antwortet idempotent.
- SLA 500ms.
- Logs.

### Receiving Server Verhalten
- Leitet an lokale Admin-Systeme weiter.

### Sending Server Verhalten
- Sendet fan-out.
- Optional dedupe.

### Flow-Diagramm
```
AdminNode                    Peer
  │                              │
  │  AdminBroadcastRequest       │
  │─────────────────────────────►│
  │                              │
  │  AdminBroadcastResponse      │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var admin = new AdminBroadcastRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = adminNode,
    TargetServerId = gateway,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    Scope = "all",
    Message = "Maintenance in 10 minutes",
    Severity = "warn"
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Nicht berechtigt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| AdminBroadcastResponse | 5041 | Response |
| ConfigReloadRequest | 5042 | Typischer Nachgang |

---

### AdminBroadcastResponse (5041)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Quittiert Admin Broadcast und meldet, ob Nachricht akzeptiert wurde.

### Im Scope ✅
- Ack/Nack
- Local Action Result

### Nicht im Scope ❌
- Client Push

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Empfänger | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AdminBroadcastResponse)]
public class AdminBroadcastResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.AdminBroadcastResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Logs acceptance.
- SLA 300ms.
- If Error -> escalate.

### Receiving Server Verhalten
- Uses result to confirm broadcast coverage.

### Sending Server Verhalten
- Summarizes acceptance ratio.

### Flow-Diagramm
```
Peer                         AdminNode
  │                              │
  │  AdminBroadcastResponse      │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new AdminBroadcastResponse
{
    CorrelationId = admin.CorrelationId,
    SenderServerId = gateway,
    TargetServerId = admin.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = admin.Nonce,
    Accepted = true
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INTERNAL_ERROR | Lokale Queue down |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| AdminBroadcastRequest | 5040 | Request |
| ConfigReloadRequest | 5042 | häufig nach Broadcast |

---

### ConfigReloadRequest (5042)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Fordert Remote Reload von Konfigurationsabschnitten an (Gameplay, Networking, Chat).

### Im Scope ✅
- Target-Scope
- Dry-Run Option

### Nicht im Scope ❌
- Deployment/Code Reload

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Peer | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| ConfigTarget | string | z.B. gameplay | Ja |
| DryRun | bool | Nur Validierung | Ja |

### Erwartete Response
- `ConfigReloadResponse` (5043)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ConfigReloadRequest)]
public class ConfigReloadRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ConfigReloadRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string ConfigTarget { get; set; } = default!;
    [Key(7)] public bool DryRun { get; set; }
}
```

### Server-Verhalten
- Validiert Berechtigung.
- Führt Reload oder Validation aus.
- Antwortet mit Result und Dauer.
- SLA 2s.
- Logs.

### Receiving Server Verhalten
- Führt reload wenn nicht DryRun.
- Beachtet Rate Limit (max 1/min).

### Sending Server Verhalten
- Aggregiert Responses clusterweit.

### Flow-Diagramm
```
AdminNode                    Peer
  │                              │
  │  ConfigReloadRequest         │
  │─────────────────────────────►│
  │                              │
  │  ConfigReloadResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var reload = new ConfigReloadRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = adminNode,
    TargetServerId = zone,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    ConfigTarget = "gameplay",
    DryRun = false
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Keine Berechtigung |
| INTERNAL_ERROR | Reload fehlgeschlagen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ConfigReloadResponse | 5043 | Response |
| AdminBroadcastRequest | 5040 | Kombination |

---

### ConfigReloadResponse (5043)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Bestätigt Config Reload oder Dry-Run Ergebnis.

### Im Scope ✅
- Erfolg/Fehlschlag
- Dauer

### Nicht im Scope ❌
- Deployments

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Peer | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Success | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |
| DurationMs | int | Dauer | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ConfigReloadResponse)]
public class ConfigReloadResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ConfigReloadResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Success { get; set; }
    [Key(7)] public string? ErrorCode { get; set; }
    [Key(8)] public int DurationMs { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Logs duration.
- SLA 2s.
- If Success=false -> escalate.

### Receiving Server Verhalten
- Auswertung für Admin Dashboard.

### Sending Server Verhalten
- Misst Zeit korrekt (stopwatch).

### Flow-Diagramm
```
Peer                         AdminNode
  │                              │
  │  ConfigReloadResponse        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new ConfigReloadResponse
{
    CorrelationId = reload.CorrelationId,
    SenderServerId = zone,
    TargetServerId = reload.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = reload.Nonce,
    Success = true,
    DurationMs = 350
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INTERNAL_ERROR | Reload fehlgeschlagen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| ConfigReloadRequest | 5042 | Request |

---

### CircuitBreakerStateRequest (5044)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Meldet oder fragt den Zustand eines Circuit Breakers (per MessageType) ab, um Überlast zu verhindern.

### Im Scope ✅
- Set/Get State
- Reason + TTL

### Nicht im Scope ❌
- Automatisches Heilen (Policy-spezifisch)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Peer | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| MessageTypeName | string | Betroffener Typ | Ja |
| DesiredState | string | Closed/Open/HalfOpen/Query | Ja |
| Reason | string | Grund | Nein |
| TTLms | int? | Dauer | Nein |

### Erwartete Response
- `CircuitBreakerStateResponse` (5045)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CircuitBreakerStateRequest)]
public class CircuitBreakerStateRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CircuitBreakerStateRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string MessageTypeName { get; set; } = default!;
    [Key(7)] public string DesiredState { get; set; } = default!;
    [Key(8)] public string? Reason { get; set; }
    [Key(9)] public int? TTLms { get; set; }
}
```

### Server-Verhalten
- Setzt oder liefert aktuellen Zustand.
- TTLms begrenzt; default 60s.
- Response SLA 300ms.
- Idempotent.
- Logs.

### Receiving Server Verhalten
- Passt lokale Pipeline an.

### Sending Server Verhalten
- Nutzt Reason für Audit.

### Flow-Diagramm
```
Node A                       Node B
  │                              │
  │  CircuitBreakerStateReq      │
  │─────────────────────────────►│
  │                              │
  │  CircuitBreakerStateResp     │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var cb = new CircuitBreakerStateRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = gateway,
    TargetServerId = zone,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    MessageTypeName = "PlayerTransferPrepareRequest",
    DesiredState = "Open",
    Reason = "load shedding",
    TTLms = 30000
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Keine Rechte |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| CircuitBreakerStateResponse | 5045 | Response |
| BackpressureAlertRequest | 5046 | Auslöser |

---

### CircuitBreakerStateResponse (5045)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Liefert aktuellen oder gesetzten CircuitBreaker Zustand.

### Im Scope ✅
- State + TTL
- Reason

### Nicht im Scope ❌
- Automatische Wiederöffnung (Policy)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| State | string | Closed/Open/HalfOpen | Ja |
| TTLms | int? | Restlaufzeit | Nein |
| Reason | string? | Begründung | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CircuitBreakerStateResponse)]
public class CircuitBreakerStateResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CircuitBreakerStateResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string State { get; set; } = default!;
    [Key(7)] public int? TTLms { get; set; }
    [Key(8)] public string? Reason { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- SLA 300ms.
- Logs state transitions.

### Receiving Server Verhalten
- Passt Backpressure Regeln an.

### Sending Server Verhalten
- Setzt TTLms basierend auf Policy.

### Flow-Diagramm
```
Node B                       Node A
  │                              │
  │  CircuitBreakerStateResp     │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new CircuitBreakerStateResponse
{
    CorrelationId = cb.CorrelationId,
    SenderServerId = zone,
    TargetServerId = cb.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = cb.Nonce,
    State = "Open",
    TTLms = 30000,
    Reason = cb.Reason
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INTERNAL_ERROR | Zustand konnte nicht gesetzt werden |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| CircuitBreakerStateRequest | 5044 | Request |
| BackpressureAlertRequest | 5046 | Auslöser |

---

### BackpressureAlertRequest (5046)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel/Hoch (bei Überlast)  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Signalisiert Überlast oder drohende Überlast und fordert Drosselung oder Umschichtung an.

### Im Scope ✅
- Level Soft/Hard
- Metrics Snapshot

### Nicht im Scope ❌
- Direkter Shutdown

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Initiator | Ja |
| TargetServerId | Guid | Peer | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| Level | string | None/Soft/Hard | Ja |
| QueueDepth | int | Aktuelle Tiefe | Ja |
| CpuUsage | float | CPU | Ja |
| Advice | string | Text | Nein |

### Erwartete Response
- `BackpressureAlertResponse` (5047)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BackpressureAlertRequest)]
public class BackpressureAlertRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BackpressureAlertRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string Level { get; set; } = default!;
    [Key(7)] public int QueueDepth { get; set; }
    [Key(8)] public float CpuUsage { get; set; }
    [Key(9)] public string? Advice { get; set; }
}
```

### Server-Verhalten
- Wenn Level=Hard -> stoppt non-critical traffic.
- Antwortet mit Maßnahmen.
- SLA 200ms.
- Idempotent.
- Logs.

### Receiving Server Verhalten
- Passt Sende-Rate an, öffnet CircuitBreaker.

### Sending Server Verhalten
- Ausgelöst bei thresholds.

### Flow-Diagramm
```
Node A                       Node B
  │                              │
  │  BackpressureAlertReq        │
  │─────────────────────────────►│
  │                              │
  │  BackpressureAlertResp       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var bp = new BackpressureAlertRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = zoneA,
    TargetServerId = gateway,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    Level = "Hard",
    QueueDepth = 500,
    CpuUsage = 92.5f,
    Advice = "pause transfers"
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INTERNAL_ERROR | Verarbeitung fehlgeschlagen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| BackpressureAlertResponse | 5047 | Response |
| CircuitBreakerStateRequest | 5044 | Folge |

---

### BackpressureAlertResponse (5047)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel/Hoch  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwortet mit bestätigten Gegenmaßnahmen auf Backpressure-Alert.

### Im Scope ✅
- Maßnahmenliste
- Ack/Nack

### Nicht im Scope ❌
- Dauerhafte Konfiguration

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Responder | Ja |
| TargetServerId | Guid | Initiator | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Accepted | bool | Maßnahmen akzeptiert | Ja |
| AppliedActions | string[] | Z.B. "throttle_transfers" | Nein |
| ErrorCode | string? | Fehler | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BackpressureAlertResponse)]
public class BackpressureAlertResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BackpressureAlertResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public bool Accepted { get; set; }
    [Key(7)] public string[]? AppliedActions { get; set; }
    [Key(8)] public string? ErrorCode { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Setzt AppliedActions.
- SLA 200ms.
- Logs.

### Receiving Server Verhalten
- Prüft Actions und setzt ggf. weitere Alarme.

### Sending Server Verhalten
- Berücksichtigt lokale Policies.

### Flow-Diagramm
```
Node B                       Node A
  │                              │
  │  BackpressureAlertResp       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new BackpressureAlertResponse
{
    CorrelationId = bp.CorrelationId,
    SenderServerId = gateway,
    TargetServerId = bp.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = bp.Nonce,
    Accepted = true,
    AppliedActions = new[] {"throttle_transfers", "pause_metrics_push"}
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Aktion nicht erlaubt |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| BackpressureAlertRequest | 5046 | Request |
| CircuitBreakerStateRequest | 5044 | Optional |

---

### SessionValidateS2SRequest (5048)

**Richtung:** 🖧 Server→Server / 🖧 Request  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Validiert SessionTokens zwischen Gateway und Zone ohne Client-Beteiligung.

### Im Scope ✅
- Token Prüfen
- Account/Character Bindung

### Nicht im Scope ❌
- Client Auth (Range 0-99)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Trace | Ja |
| SenderServerId | Guid | Gateway | Ja |
| TargetServerId | Guid | Auth/Session Service | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Anti-Replay | Ja |
| SessionToken | string | Token | Ja |
| ClientIp | string | Info | Nein |

### Erwartete Response
- `SessionValidateS2SResponse` (5049)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SessionValidateS2SRequest)]
public class SessionValidateS2SRequest : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SessionValidateS2SRequest;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string SessionToken { get; set; } = default!;
    [Key(7)] public string? ClientIp { get; set; }
}
```

### Server-Verhalten
- Prüft Token Signatur/Gültigkeit.
- Antwortet mit Account/Character IDs.
- Rate Limit 50/s.
- SLA 300ms.
- Logs security events.

### Receiving Server Verhalten
- Nutzt Result für Login Pipeline.

### Sending Server Verhalten
- Hashes ClientIp for privacy.

### Flow-Diagramm
```
Gateway                      SessionSvc
  │                              │
  │  SessionValidateS2SReq       │
  │─────────────────────────────►│
  │                              │
  │  SessionValidateS2SResp      │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var sreq = new SessionValidateS2SRequest
{
    CorrelationId = Guid.NewGuid(),
    SenderServerId = gateway,
    TargetServerId = auth,
    TimestampMs = NowMs(),
    Nonce = RandomNonce(),
    SessionToken = token,
    ClientIp = "203.0.113.42"
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INVALID | Token ungültig |
| EXPIRED | Abgelaufen |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| SessionValidateS2SResponse | 5049 | Response |
| LoginRequest | 1 | Ursprüngliche Client Auth |

---

### SessionValidateS2SResponse (5049)

**Richtung:** 🖧 Server→Server / 🖧 Response  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 IMMER Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Antwortet auf Session-Validierung, liefert Account/Character Daten.

### Im Scope ✅
- Validation Result
- Security Flags

### Nicht im Scope ❌
- Client-Benachrichtigung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | Guid | Echo | Ja |
| SenderServerId | Guid | Auth Service | Ja |
| TargetServerId | Guid | Gateway | Ja |
| TimestampMs | long | Epoch | Ja |
| Nonce | long | Echo | Ja |
| Result | string | Valid/Invalid/Expired | Ja |
| AccountId | long? | Bei Erfolg | Nein |
| CharacterId | long? | Optional | Nein |
| Flags | string[]? | Security Flags | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SessionValidateS2SResponse)]
public class SessionValidateS2SResponse : IServerToServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SessionValidateS2SResponse;
    [Key(1)] public Guid CorrelationId { get; set; }
    [Key(2)] public Guid SenderServerId { get; set; }
    [Key(3)] public Guid TargetServerId { get; set; }
    [Key(4)] public long TimestampMs { get; set; }
    [Key(5)] public long Nonce { get; set; }
    [Key(6)] public string Result { get; set; } = default!;
    [Key(7)] public long? AccountId { get; set; }
    [Key(8)] public long? CharacterId { get; set; }
    [Key(9)] public string[]? Flags { get; set; }
}
```

### Server-Verhalten
- Idempotent.
- Logs.
- SLA 300ms.
- On Invalid -> triggers security alert.

### Receiving Server Verhalten
- Bei Valid -> lässt Client passieren.
- Bei Invalid -> disconnect.

### Sending Server Verhalten
- Sets Flags (e.g., "TwoFactorPending").

### Flow-Diagramm
```
SessionSvc                   Gateway
  │                              │
  │  SessionValidateS2SResp      │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resp = new SessionValidateS2SResponse
{
    CorrelationId = sreq.CorrelationId,
    SenderServerId = auth,
    TargetServerId = sreq.SenderServerId,
    TimestampMs = NowMs(),
    Nonce = sreq.Nonce,
    Result = "Valid",
    AccountId = 42,
    CharacterId = 5678,
    Flags = new []{"TrustedDevice"}
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| INVALID | Token ungültig |

### Verwandte Messages
| Message | ID | Beziehung |
| ------- | -- | -------- |
| SessionValidateS2SRequest | 5048 | Request |

---

## 🗑️ Obsolete Messages

- Keine. Range 5000–5049 enthält ausschließlich aktive S2S Messages. Obsolete Einträge werden hier ergänzt, falls zukünftige Migrationen notwendig sind.

## 🧨 Edge Cases & Fehlerfälle

- **Split Brain:** Bei divergierenden Lease-Informationen (e.g., PartitionOwnershipQuery liefert widersprüchliche Owner) muss CircuitBreakerStateRequest mit `Open` gesetzt und SnapshotSync angestoßen werden.
- **Replay Attack:** Wenn Nonce erneut gesehen wird, Message verwerfen und Security Alert loggen; keine Response senden (silent drop) um Angreifer keine Information zu geben.
- **Clock Drift:** TimeSyncResponse Offset > 50ms -> markiere Peer as Degraded; >200ms -> refuse transfers.
- **Backpressure Storm:** Mehrere BackpressureAlertRequests gleichzeitig -> aggregieren und maximal eine Response je 500ms.
- **Transfer Double Commit:** Wenn CommitResponse mehrfach mit Success=false kommt, starte Abort und markiere Ticket revoked.
- **Handoff Loss:** Wenn EntityHandoffResponse nicht eintrifft, nach Timeout LeaseReleaseRequest senden und Ownership behalten.
- **Config Reload Failure:** Bei wiederholtem Failure (>3) CircuitBreaker für ConfigReloadRequest öffnen.
- **Admin Broadcast Loop:** Sicherstellen, dass broadcasts nicht an Sender zurück-echoen (SenderServerId != TargetServerId).
- **Session Validation Cache:** Negative Cache (Invalid Token) 5s; positive Cache 30s; schützt gegen Replay.
- **Data Integrity:** Immer PayloadHash loggen, nicht Klartext; kein PII in logs.

## 📎 Anhang

### MessageType Enum Updates

- Range 5000-5049 ergänzt mit neuen Server-to-Server MessageTypes in `shared/Mmo.Shared/Messaging/Enums/MessageType.cs`.
- Reihenfolge folgt Requests direkt gefolgt von Responses.

---

**Letzte Aktualisierung:** 2026-01-03  
**Version:** 1.0.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/50-server-to-server.md
