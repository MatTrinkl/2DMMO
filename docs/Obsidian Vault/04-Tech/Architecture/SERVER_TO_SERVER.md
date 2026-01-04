# 🌐 Server-zu-Server Kommunikation & Load-Balancing

## 2DMMO – Multi-Server Architektur

**Version:** 2.0.0  
**Letzte Aktualisierung:** 2025-12-25  
**Teil von:** [Architektur-Dokumentation](Architecture-Overview.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt die **Server-zu-Server (S2S) Kommunikation** im 2DMMO, einschließlich:

- Multi-Server Architektur mit Gateway- und Zone-Server Clustern
- S2S Message-Protokoll über Redis Pub/Sub
- Load-Balancing-Strategien für verschiedene Game-Systeme
- Security und Monitoring für S2S-Kommunikation

**Wichtig**: S2S-Messages sind **interne Server-Kommunikation** und werden **NIE** direkt von Clients gesendet oder empfangen.

---

## 1. Multi-Server Architektur

### 1.1 Architektur-Diagramm

```
┌─────────────────────────────────────────────────────────────────┐
│                    MULTI-SERVER ARCHITEKTUR                      │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    AZURE FRONT DOOR                       │   │
│  │              (TCP Load Balancer, DDoS, WAF)              │   │
│  └────────────────────────┬─────────────────────────────────┘   │
│                           │                                      │
│  ┌────────────────────────┼──────────────────────────────────┐  │
│  │                   GATEWAY CLUSTER                          │  │
│  │     ┌─────────┐   ┌─────────┐   ┌─────────┐              │  │
│  │     │Gateway-1│   │Gateway-2│   │Gateway-N│              │  │
│  │     │ (Auth)  │   │ (Auth)  │   │ (Auth)  │              │  │
│  │     └────┬────┘   └────┬────┘   └────┬────┘              │  │
│  └──────────┼─────────────┼─────────────┼────────────────────┘  │
│             │             │             │                        │
│  ┌──────────┼─────────────┼─────────────┼────────────────────┐  │
│  │          │        REDIS PUB/SUB      │                    │  │
│  │          │      (S2S Communication)  │                    │  │
│  │          ▼             ▼             ▼                    │  │
│  │     ┌─────────┐   ┌─────────┐   ┌─────────┐              │  │
│  │     │Zone-Srv │   │Zone-Srv │   │Zone-Srv │              │  │
│  │     │ Forest  │   │  Town   │   │ Dungeon │              │  │
│  │     │Shard 1-N│   │Shard 1-N│   │Instance │              │  │
│  │     └─────────┘   └─────────┘   └─────────┘              │  │
│  └───────────────────────────────────────────────────────────┘  │
│                           │                                      │
│  ┌────────────────────────┼──────────────────────────────────┐  │
│  │                 SHARED SERVICES                            │  │
│  │   ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │  │
│  │   │  Redis   │  │PostgreSQL│  │  Match-  │  │  Auction │ │  │
│  │   │ Cluster  │  │ Primary+ │  │  making  │  │  House   │ │  │
│  │   │          │  │ Replicas │  │ Service  │  │ Service  │ │  │
│  │   └──────────┘  └──────────┘  └──────────┘  └──────────┘ │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 1.2 Server-Typen

| Server-Typ | Zweck | Skalierung | Kommunikation |
|------------|-------|------------|---------------|
| **Gateway Server** | Authentifizierung, Connection-Handling | Horizontal (N Instanzen) | Client ↔ Gateway, Gateway ↔ Redis |
| **Zone Server** | Game Loop, Zone-spezifische Logik | Horizontal (Zone + Shards) | Gateway ↔ Zone (via Redis) |
| **Instance Server** | Dynamische Dungeons/PvP-Arenas | Horizontal (On-Demand) | Zone ↔ Instance (via Redis) |
| **Matchmaking Service** | Queue-Management, Match-Finding | Vertical (Single Instance*) | Zone ↔ Matchmaking (via Redis) |
| **Auction House Service** | Global Auction-System | Vertical (Master + Replicas) | Zone ↔ Auction (via Redis) |
| **Mail Service** | Async Mail-Delivery | Horizontal (Queue-Based) | Zone ↔ Mail (via Redis) |

\* Mit Redis-basierter Queue für Horizontal-Scaling in Phase 3

---

## 2. S2S Message-Kategorien

Alle S2S-Messages verwenden den **5000er-Bereich** (5000-5999) um sie klar von Client-Server-Messages zu trennen.

### 2.1 Kategorie 50: S2S Core (5000-5099)

**Zweck**: Grundlegende Server-zu-Server Kommunikation (Session, Health, Discovery)

| ID | Message | Beschreibung |
|----|---------|--------------|
| 5000 | `S2S_SessionValidate` | Token-Validierung (ersetzt SessionValidate 8) |
| 5001 | `S2S_SessionValidateResponse` | Response mit Session-Daten |
| 5002 | `S2S_PlayerLocate` | Spieler auf Server finden |
| 5003 | `S2S_PlayerLocateResponse` | Response mit Server-Location |
| 5004 | `S2S_ServerHandshake` | Server-Authentifizierung |
| 5005 | `S2S_ServerHeartbeat` | Server-Health-Check |
| 5006 | `S2S_ServerShutdownNotify` | Shutdown-Ankündigung |

**Status**: 🟡 Phase 2

### 2.2 Kategorie 51: S2S Zone Transfer (5100-5199)

**Zweck**: Spieler-Transfer zwischen Zonen und Shards

| ID | Message | Beschreibung |
|----|---------|--------------|
| 5100 | `S2S_ZoneTransferRequest` | Spieler-Transfer initiieren |
| 5101 | `S2S_ZoneTransferPrepare` | Ziel-Zone vorbereiten |
| 5102 | `S2S_ZoneTransferReady` | Ziel bereit für Transfer |
| 5103 | `S2S_ZoneTransferExecute` | Transfer durchführen |
| 5104 | `S2S_ZoneTransferComplete` | Transfer abgeschlossen |
| 5105 | `S2S_ZoneTransferFailed` | Transfer fehlgeschlagen |
| 5110 | `S2S_ShardTransfer` | Shard-Wechsel innerhalb Zone |
| 5111 | `S2S_InstanceCreate` | Neue Instanz erstellen |
| 5112 | `S2S_InstanceDestroy` | Instanz zerstören |

**Status**: 🟡 Phase 2

### 2.3 Kategorie 52: S2S Cross-Zone Features (5200-5299)

**Zweck**: Cross-Zone Funktionalität (Whisper, Party, Guild)

| ID | Message | Beschreibung |
|----|---------|--------------|
| 5200 | `S2S_CrossZoneWhisper` | Whisper an Spieler in anderer Zone |
| 5201 | `S2S_CrossZonePartyInvite` | Party-Invite über Zonen |
| 5202 | `S2S_CrossZonePartyUpdate` | Party-Status Update |
| 5203 | `S2S_CrossZoneGuildMessage` | Guild-Chat cross-zone |
| 5204 | `S2S_CrossZoneFriendStatus` | Friend online/offline |
| 5210 | `S2S_CrossZoneTrade` | Trade-Request cross-zone |
| 5211 | `S2S_CrossZoneMail` | Mail-Notification |
| 5220 | `S2S_GlobalBroadcast` | Server-weite Announcement |
| 5221 | `S2S_ZoneBroadcast` | Zone-weite Announcement |

**Status**: 🟡 Phase 2

### 2.4 Kategorie 53: S2S Matchmaking & Instances (5300-5399)

**Zweck**: Matchmaking-Service und Instance-Management

| ID | Message | Beschreibung |
|----|---------|--------------|
| 5300 | `S2S_MatchmakingQueue` | Spieler zur Queue hinzufügen |
| 5301 | `S2S_MatchmakingDequeue` | Spieler aus Queue entfernen |
| 5302 | `S2S_MatchFound` | Match gefunden |
| 5303 | `S2S_MatchAccepted` | Alle akzeptiert |
| 5304 | `S2S_MatchCancelled` | Match abgebrochen |
| 5310 | `S2S_InstanceRequest` | Instanz anfordern |
| 5311 | `S2S_InstanceReady` | Instanz bereit |
| 5312 | `S2S_InstancePlayerJoin` | Spieler joint Instanz |
| 5313 | `S2S_InstancePlayerLeave` | Spieler verlässt |
| 5314 | `S2S_InstanceComplete` | Instanz abgeschlossen |

**Status**: 🟡 Phase 2

### 2.5 Kategorie 54: S2S Economy & Auction (5400-5499)

**Zweck**: Auction-House und Economy-Services

| ID | Message | Beschreibung |
|----|---------|--------------|
| 5400 | `S2S_AuctionCreate` | Auction erstellen |
| 5401 | `S2S_AuctionBid` | Gebot abgeben |
| 5402 | `S2S_AuctionBuyout` | Sofortkauf |
| 5403 | `S2S_AuctionExpired` | Auction abgelaufen |
| 5404 | `S2S_AuctionSold` | Item verkauft |
| 5410 | `S2S_GoldTransfer` | Gold-Transfer (Mail, Trade) |
| 5411 | `S2S_ItemTransfer` | Item-Transfer (Mail) |

**Status**: 🔵 Phase 3

### 2.6 Kategorie 55: S2S Admin & Monitoring (5500-5599)

**Zweck**: Admin-Commands und Monitoring

| ID | Message | Beschreibung |
|----|---------|--------------|
| 5500 | `S2S_AdminCommand` | Remote Admin-Befehl |
| 5501 | `S2S_AdminCommandResponse` | Response |
| 5502 | `S2S_PlayerKick` | Spieler kicken (cross-zone) |
| 5503 | `S2S_PlayerBan` | Spieler bannen (global) |
| 5504 | `S2S_PlayerMute` | Spieler muten (global) |
| 5510 | `S2S_ServerMetrics` | Server-Metriken |
| 5511 | `S2S_ZoneMetrics` | Zone-Metriken |
| 5512 | `S2S_AlertTrigger` | Alert auslösen |
| 5520 | `S2S_MaintenanceSchedule` | Wartung ankündigen |
| 5521 | `S2S_MaintenanceCancel` | Wartung abbrechen |

**Status**: 🟡 Phase 2

---

## 3. Redis Pub/Sub Channel-Schema

Alle S2S-Kommunikation läuft über **Redis Pub/Sub** für Entkopplung und Skalierbarkeit.

### 3.1 Channel-Übersicht

```
# ═══════════════════════════════════════════════════════════════
# S2S CORE
# ═══════════════════════════════════════════════════════════════
s2s:session:validate          → SessionValidate Requests
s2s:session:validate:response → SessionValidate Responses
s2s:server:{serverId}         → Server-spezifische Messages
s2s:server:broadcast          → An alle Server

# ═══════════════════════════════════════════════════════════════
# ZONE TRANSFER
# ═══════════════════════════════════════════════════════════════
s2s:transfer:zone:{zoneId}    → Transfer zu Zone
s2s:transfer:shard:{shardId}  → Transfer zu Shard
s2s:transfer:instance         → Instance-Transfers

# ═══════════════════════════════════════════════════════════════
# CROSS-ZONE FEATURES
# ═══════════════════════════════════════════════════════════════
s2s:player:{playerId}         → Spieler-spezifisch (Whisper, Invite)
s2s:guild:{guildId}           → Guild-Messages
s2s:party:{partyId}           → Party-Messages

# ═══════════════════════════════════════════════════════════════
# SERVICES
# ═══════════════════════════════════════════════════════════════
s2s:matchmaking               → Matchmaking-Service
s2s:auction                   → Auction-House-Service
s2s:mail                      → Mail-Service

# ═══════════════════════════════════════════════════════════════
# ADMIN & MONITORING
# ═══════════════════════════════════════════════════════════════
s2s:admin                     → Admin-Commands
s2s:alerts                    → Monitoring-Alerts
s2s:metrics                   → Server-Metriken
```

### 3.2 Channel-Patterns

| Pattern | Beschreibung | Subscriber |
|---------|--------------|------------|
| `s2s:server:{serverId}` | Direct-to-Server Messages | Spezifischer Server |
| `s2s:player:{playerId}` | Player-spezifische Messages | Server wo Spieler ist |
| `s2s:guild:{guildId}` | Guild-weite Messages | Alle Server mit Guild-Mitgliedern |
| `s2s:transfer:*` | Zone/Shard-Transfers | Ziel-Server |
| `s2s:broadcast` | Global Broadcasts | Alle Server |

### 3.3 Message-Routing

```csharp
// Beispiel: Whisper zu Spieler in anderer Zone
public async Task SendCrossZoneWhisperAsync(long targetPlayerId, string message)
{
    // 1. Locate Player
    var location = await LocatePlayerAsync(targetPlayerId);
    if (location == null)
    {
        return; // Player offline
    }
    
    // 2. Publish to Player-Channel
    var whisper = new S2S_CrossZoneWhisper
    {
        Type = MessageType.S2S_CrossZoneWhisper,
        TargetPlayerId = targetPlayerId,
        SenderName = senderName,
        Message = message
    };
    
    await _redis.PublishAsync($"s2s:player:{targetPlayerId}", whisper);
}
```

---

## 4. Load-Balancing-Strategien

### 4.1 Gateway Load Balancing

**Strategie**: Round-Robin mit Sticky-Sessions

#### Details
- **Load-Balancer**: Azure Front Door (Layer 4 TCP)
- **Algorithmus**: Round-Robin für neue Connections
- **Sticky-Session**: Session-Token bindet Client an Gateway
- **Health-Check**: TCP-Liveness alle 10s
- **Failover**: Client reconnectet zu neuem Gateway

#### Konfiguration
```yaml
Gateway:
  LoadBalancing:
    Algorithm: RoundRobin
    StickySession: true
    SessionAffinityTimeout: 30m
    HealthCheck:
      Interval: 10s
      Timeout: 5s
      Retries: 3
```

#### Flow-Diagramm
```
Client                  Azure Front Door         Gateway Cluster
  │                           │                  ┌───────┬───────┐
  │  TCP Connect              │                  │  GW1  │  GW2  │
  │──────────────────────────►│                  └───────┴───────┘
  │                           │  Round-Robin           
  │                           │───────────────────►GW1 (50 Clients)
  │                           │                         
  │  LoginRequest             │                         
  │──────────────────────────►│──────────────────►GW1  
  │                           │                         
  │  LoginResponse            │                         
  │  (SessionToken=xyz)       │                         
  │◄──────────────────────────│◄──────────────────GW1  
  │                           │                         
  │  Alle folgenden Messages  │                         
  │  mit Token=xyz            │                         
  │──────────────────────────►│──────────────────►GW1 (Sticky!)
```

### 4.2 Zone-Server Load Balancing

**Strategie**: Content-Based Routing + Zone Sharding

#### Zone-Assignment
- **Static Zones**: Jede Zone hat dedizierte Server (z.B. "Startzone" → ZoneServer-1)
- **Sharding**: Bei > 200 Spieler → Neuer Shard wird erstellt
- **Shard-Assignment**:
  - Neue Spieler → Shard mit niedrigster Population
  - Party-Mitglieder → Gleicher Shard (bevorzugt)
  - Guild-Mitglieder → Präferenz für gleichen Shard

#### Shard-Lifecycle
```
Population < 150:  Einzelner Shard
Population 150-200: "Filling Up" (Neue Spieler noch erlaubt)
Population > 200:   Neuer Shard wird erstellt
Population < 50:    Shard-Merge (wenn 2+ Shards existieren)
```

#### Beispiel-Konfiguration
```csharp
public class ZoneShardingConfig
{
    public int MaxPlayersPerShard { get; set; } = 200;
    public int ShardWarningThreshold { get; set; } = 180;
    public int ShardMergeThreshold { get; set; } = 50;
    public bool PreferSameShardForParty { get; set; } = true;
    public bool PreferSameShardForGuild { get; set; } = true;
}
```

### 4.3 Instance-Server Load Balancing

**Strategie**: Pool-Based mit dynamischer Allokation

#### Instance-Pool
- **Pre-Warmed Instances**: 5-10 leere Instanzen vorgehalten
- **Fast Assignment**: Instanz aus Pool in < 500ms
- **Cleanup**: Leere Instanzen nach 5min → zurück in Pool
- **Scaling**: Pool-Größe skaliert mit Matchmaking-Queue-Länge

#### Pool-Sizing-Formel
```
Pool Size = BaseSize + (QueueLength / 5)
BaseSize = 5
Max Pool Size = 50
```

#### Flow
```
Matchmaking Service         Instance Pool           Zone Server
      │                          │                       │
      │  Match Found (5 Players) │                       │
      │─────────────────────────►│                       │
      │                          │  Get Free Instance    │
      │                          │  (Instance-42)        │
      │                          │                       │
      │  S2S_InstanceReady       │                       │
      │◄─────────────────────────│                       │
      │                          │                       │
      │  S2S_InstancePlayerJoin  │                       │
      │──────────────────────────┼──────────────────────►│
      │                          │                       │
      │                    Instance Running (5 Players)  │
      │                          │                       │
      │  Instance Complete       │                       │
      │◄─────────────────────────┼───────────────────────│
      │                          │                       │
      │                          │  Return to Pool       │
      │                          │  (Cleanup + Reset)    │
```

### 4.4 Auction House Service

**Strategie**: Single-Master mit Read-Replicas

#### Architektur
- **Master**: Schreiboperationen (Create, Bid, Buyout)
- **Read-Replicas**: Lesoperationen (Search, Browse)
- **Consistency**: Eventual Consistency (1-2s Delay)
- **Cache**: Redis für Hot-Items (Top 100 Auctions)

#### Datenbank-Setup
```
┌────────────────┐
│ Primary (RW)   │  ← Write-Only (Bids, Creates, Buyouts)
└────────┬───────┘
         │ Replication (< 1s)
         │
    ┌────┴─────┬─────────┐
    ▼          ▼         ▼
┌─────────┐┌─────────┐┌─────────┐
│Replica-1││Replica-2││Replica-N│  ← Read-Only (Search, Browse)
└─────────┘└─────────┘└─────────┘
```

#### Load-Distribution
| Operation | Target | Latency |
|-----------|--------|---------|
| Search | Replica (Round-Robin) | < 50ms |
| Browse | Replica (Round-Robin) | < 50ms |
| Create Auction | Master | < 100ms |
| Place Bid | Master | < 100ms |
| Buyout | Master | < 100ms |

### 4.5 Mail-Service

**Strategie**: Async Queue-Based Processing

#### Queue-System
- **Queue**: Redis-basierte Mail-Queue
- **Processing**: Batch-Processing alle 5s
- **Batch-Size**: Max 100 Mails pro Batch
- **Delivery**:
  - Empfänger online → Push via S2S
  - Empfänger offline → DB-Store

#### Flow
```
Sender (Zone-1)        Mail Queue           Mail Service        Recipient (Zone-2)
     │                      │                     │                     │
     │  Send Mail           │                     │                     │
     │─────────────────────►│                     │                     │
     │                      │  Enqueue            │                     │
     │                      │                     │                     │
     │                      │  Batch-Process      │                     │
     │                      │  (every 5s)         │                     │
     │                      │◄────────────────────│                     │
     │                      │                     │                     │
     │                      │  Check Recipient    │                     │
     │                      │  Online Status      │                     │
     │                      │                     │                     │
     │                      │  If Online:         │                     │
     │                      │  S2S_CrossZoneMail  │                     │
     │                      │─────────────────────┼────────────────────►│
     │                      │                     │                     │
     │                      │  If Offline:        │                     │
     │                      │  Store in DB        │                     │
```

---

## 5. Detaillierte Message-Dokumentation

### 5.1 S2S_SessionValidate (5000)

**Richtung:** Zone Server → Gateway (via Redis)  
**Channel:** `s2s:session:validate`  
**Timeout:** 500ms  
**Retry:** 3x mit exponential backoff

#### Beschreibung
Zone-Server validiert Session-Token beim Gateway, wenn ein Spieler die Zone betritt.

#### Wann verwendet?
- Spieler wechselt Zone (ZoneTransfer)
- Spieler reconnected zu Zone
- Periodic Re-Validation (alle 5min)

#### Request Payload
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Type | MessageType | `S2S_SessionValidate` |
| RequestId | Guid | Korrelations-ID für Response |
| SessionToken | string | Zu validierendes Token |
| RequestingServerId | string | Zone-Server ID |
| PlayerId | long | Player-ID für Lookup |

#### Response (S2S_SessionValidateResponse)
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Type | MessageType | `S2S_SessionValidateResponse` |
| RequestId | Guid | Korrelations-ID |
| Valid | bool | Token gültig? |
| AccountId | long | Account-ID (bei Valid) |
| CharacterId | long | Character-ID (bei Valid) |
| Permissions | List<string> | Account-Rechte |
| ErrorCode | string | Fehler (bei !Valid) |

#### Flow-Diagramm
```
Zone Server                 Redis                    Gateway
    │                         │                         │
    │  Publish: s2s:session:validate                   │
    │  {RequestId, Token, ...}                         │
    │─────────────────────────►│                        │
    │                          │  Message               │
    │                          │───────────────────────►│
    │                          │                        │  Validate Token
    │                          │                        │  (Memory/Redis)
    │                          │  Response              │
    │                          │◄───────────────────────│
    │  s2s:session:validate:response                   │
    │◄─────────────────────────│                        │
    │                          │                        │
    │  Cache Response locally (60s)                    │
```

#### Caching-Strategie
- Zone-Server cached erfolgreiche Validierungen für **60s**
- Cache-Key: `session:cache:{sessionToken}`
- Bei Cache-Hit: Kein Redis-Call nötig
- Bei Cache-Miss: S2S-Call + Cache-Update

#### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `SESSION_NOT_FOUND` | Token ungültig | Disconnect Player |
| `SESSION_EXPIRED` | Token abgelaufen | Disconnect Player |
| `GATEWAY_UNAVAILABLE` | Gateway nicht erreichbar | Retry (3x) |
| `TIMEOUT` | Keine Antwort in 500ms | Retry (3x) |

#### C# Beispiel
```csharp
public async Task<SessionValidationResult> ValidateSessionAsync(
    string sessionToken, 
    long playerId)
{
    // Check local cache first
    var cacheKey = $"session:cache:{sessionToken}";
    var cached = await _localCache.GetAsync<SessionValidationResult>(cacheKey);
    if (cached != null)
    {
        _logger.LogDebug("Session validation cache hit for player {PlayerId}", playerId);
        return cached;
    }
    
    // Send validation request via Redis
    var request = new S2S_SessionValidate
    {
        Type = MessageType.S2S_SessionValidate,
        RequestId = Guid.NewGuid(),
        SessionToken = sessionToken,
        RequestingServerId = _serverId,
        PlayerId = playerId
    };
    
    _logger.LogDebug("Sending S2S session validation for player {PlayerId}", playerId);
    
    var response = await _redisMessageBroker.RequestAsync<S2S_SessionValidateResponse>(
        channel: "s2s:session:validate",
        request: request,
        timeout: TimeSpan.FromMilliseconds(500),
        retries: 3
    );
    
    // Cache successful response
    if (response.Valid)
    {
        var result = new SessionValidationResult
        {
            Valid = true,
            AccountId = response.AccountId,
            CharacterId = response.CharacterId,
            Permissions = response.Permissions
        };
        
        await _localCache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(60));
        return result;
    }
    
    _logger.LogWarning("Session validation failed for player {PlayerId}: {ErrorCode}", 
        playerId, response.ErrorCode);
    
    return new SessionValidationResult { Valid = false };
}
```

### 5.2 S2S_ZoneTransferRequest (5100)

**Richtung:** Source Zone → Target Zone (via Redis)  
**Channel:** `s2s:transfer:zone:{targetZoneId}`  
**Timeout:** 2000ms  
**Retry:** 1x (dann Fallback zu Error)

#### Beschreibung
Initiiert einen Spieler-Transfer zwischen zwei Zones. Source-Zone sendet Player-Daten an Target-Zone.

#### Wann verwendet?
- Spieler läuft über Zone-Grenze
- Teleport zu anderer Zone
- Portal-Nutzung

#### Request Payload
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Type | MessageType | `S2S_ZoneTransferRequest` |
| TransferId | Guid | Transfer-Tracking-ID |
| PlayerId | long | Zu transferierender Spieler |
| TargetZoneId | string | Ziel-Zone ID |
| TargetShardId | int? | Präferierter Shard (optional) |
| EntryPoint | Vector2 | Spawn-Position in Ziel-Zone |
| PlayerState | PlayerStateSnapshot | Vollständiger Player-State |
| Reason | string | Transfer-Grund (Portal, Border, Teleport) |

#### PlayerStateSnapshot
```csharp
[MessagePackObject]
public class PlayerStateSnapshot
{
    [Key(0)] public long PlayerId { get; set; }
    [Key(1)] public string CharacterName { get; set; }
    [Key(2)] public int Level { get; set; }
    [Key(3)] public int CurrentHp { get; set; }
    [Key(4)] public int CurrentMana { get; set; }
    [Key(5)] public List<BuffData> ActiveBuffs { get; set; }
    [Key(6)] public List<ItemData> Inventory { get; set; }
    [Key(7)] public Dictionary<string, int> Cooldowns { get; set; }
}
```

#### Response (S2S_ZoneTransferReady)
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Type | MessageType | `S2S_ZoneTransferReady` |
| TransferId | Guid | Transfer-Tracking-ID |
| Success | bool | Transfer vorbereitet? |
| AssignedShardId | int | Zugewiesener Shard |
| SpawnPosition | Vector2 | Finale Spawn-Position |
| ErrorCode | string? | Fehler-Code bei !Success |

#### Flow-Diagramm
```
Source Zone            Redis             Target Zone           Player Client
    │                    │                    │                      │
    │ Player triggers    │                    │                      │
    │ zone transfer      │                    │                      │
    │                    │                    │                      │
    │ S2S_ZoneTransferRequest                 │                      │
    │───────────────────►│                    │                      │
    │                    │ Publish to         │                      │
    │                    │ s2s:transfer:      │                      │
    │                    │ zone:{targetId}    │                      │
    │                    │───────────────────►│                      │
    │                    │                    │ Validate & Prepare   │
    │                    │                    │ (Find Shard, Spawn)  │
    │                    │                    │                      │
    │                    │ S2S_ZoneTransferReady                     │
    │◄───────────────────│◄───────────────────│                      │
    │                    │                    │                      │
    │ ZoneTransferResponse                    │                      │
    │─────────────────────────────────────────┼─────────────────────►│
    │                    │                    │                      │
    │                    │                    │      Client loads    │
    │                    │                    │      new zone        │
    │                    │                    │                      │
    │ Remove Player      │                    │   JoinZone Request   │
    │ from Zone          │                    │◄─────────────────────│
    │                    │                    │                      │
    │                    │                    │ Add Player to Zone   │
    │                    │                    │                      │
    │ S2S_ZoneTransferComplete                │                      │
    │◄───────────────────│◄───────────────────│                      │
```

#### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ZONE_FULL` | Alle Shards voll | Retry später |
| `ZONE_NOT_FOUND` | Ziel-Zone existiert nicht | Error an Client |
| `INVALID_ENTRY_POINT` | Spawn-Position ungültig | Fallback zu Default |
| `TRANSFER_TIMEOUT` | Keine Antwort in 2s | Retry 1x |

#### C# Beispiel
```csharp
public async Task<bool> TransferPlayerToZoneAsync(
    long playerId, 
    string targetZoneId, 
    Vector2 entryPoint)
{
    var player = GetPlayer(playerId);
    if (player == null) return false;
    
    // Create player state snapshot
    var snapshot = new PlayerStateSnapshot
    {
        PlayerId = player.Id,
        CharacterName = player.Name,
        Level = player.Level,
        CurrentHp = player.CurrentHp,
        CurrentMana = player.CurrentMana,
        ActiveBuffs = player.GetActiveBuffs(),
        Inventory = player.GetInventorySnapshot(),
        Cooldowns = player.GetCooldownSnapshot()
    };
    
    // Send transfer request
    var request = new S2S_ZoneTransferRequest
    {
        Type = MessageType.S2S_ZoneTransferRequest,
        TransferId = Guid.NewGuid(),
        PlayerId = playerId,
        TargetZoneId = targetZoneId,
        EntryPoint = entryPoint,
        PlayerState = snapshot,
        Reason = "Portal"
    };
    
    _logger.LogInformation(
        "Initiating zone transfer for player {PlayerId} to zone {TargetZone}", 
        playerId, targetZoneId);
    
    var response = await _redisMessageBroker.RequestAsync<S2S_ZoneTransferReady>(
        channel: $"s2s:transfer:zone:{targetZoneId}",
        request: request,
        timeout: TimeSpan.FromMilliseconds(2000),
        retries: 1
    );
    
    if (!response.Success)
    {
        _logger.LogError("Zone transfer failed: {ErrorCode}", response.ErrorCode);
        await SendErrorToClient(playerId, response.ErrorCode);
        return false;
    }
    
    // Notify client to change zones
    await SendZoneTransferResponseToClient(playerId, targetZoneId, response.SpawnPosition);
    
    return true;
}
```

### 5.3 S2S_CrossZoneWhisper (5200)

**Richtung:** Source Zone → Target Zone (via Redis)  
**Channel:** `s2s:player:{targetPlayerId}`  
**Timeout:** N/A (Fire-and-Forget)  
**Retry:** Nein

#### Beschreibung
Sendet eine Whisper-Message an einen Spieler in einer anderen Zone.

#### Wann verwendet?
- Spieler sendet Whisper an Friend in anderer Zone
- GM sendet Cross-Zone Message

#### Payload
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Type | MessageType | `S2S_CrossZoneWhisper` |
| TargetPlayerId | long | Empfänger-ID |
| SenderPlayerId | long | Absender-ID |
| SenderName | string | Absender-Name |
| Message | string | Whisper-Text |
| Timestamp | long | Unix Timestamp |

#### Flow
```
Zone-A (Sender)         Redis              Zone-B (Recipient)     Client
    │                     │                       │                  │
    │ Player sends        │                       │                  │
    │ whisper             │                       │                  │
    │                     │                       │                  │
    │ S2S_CrossZoneWhisper                        │                  │
    │────────────────────►│                       │                  │
    │                     │ Publish to            │                  │
    │                     │ s2s:player:{id}       │                  │
    │                     │──────────────────────►│                  │
    │                     │                       │ Player online?   │
    │                     │                       │ Yes → Forward    │
    │                     │                       │                  │
    │                     │                       │ ChatWhisper      │
    │                     │                       │─────────────────►│
```

#### C# Beispiel
```csharp
public async Task SendCrossZoneWhisperAsync(
    long senderId, 
    string senderName,
    long targetPlayerId, 
    string message)
{
    // No need to locate player - Redis Pub/Sub handles routing
    var whisper = new S2S_CrossZoneWhisper
    {
        Type = MessageType.S2S_CrossZoneWhisper,
        TargetPlayerId = targetPlayerId,
        SenderPlayerId = senderId,
        SenderName = senderName,
        Message = message,
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };
    
    // Fire-and-forget publish
    await _redis.PublishAsync($"s2s:player:{targetPlayerId}", whisper);
    
    _logger.LogDebug(
        "Sent cross-zone whisper from {SenderId} to {TargetId}", 
        senderId, targetPlayerId);
}
```

### 5.4 S2S_MatchmakingQueue (5300)

**Richtung:** Zone Server → Matchmaking Service (via Redis)  
**Channel:** `s2s:matchmaking`  
**Timeout:** 1000ms  
**Retry:** Nein (User kann neu queuen)

#### Beschreibung
Fügt einen Spieler zur Matchmaking-Queue für PvP oder Dungeons hinzu.

#### Payload
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Type | MessageType | `S2S_MatchmakingQueue` |
| QueueId | Guid | Queue-Tracking-ID |
| PlayerId | long | Spieler-ID |
| PlayerName | string | Spieler-Name |
| MatchType | string | "Arena2v2", "Arena3v3", "Dungeon5" |
| ItemLevel | int | Gear-Score für Matching |
| Rating | int | MMR (Matchmaking-Rating) |
| Roles | List<string> | ["Tank", "Healer", "DPS"] |
| SourceServerId | string | Zone-Server ID |

#### Response (S2S_MatchFound / Timeout)
```csharp
// Option A: Match gefunden
[MessagePackObject]
public class S2S_MatchFound
{
    [Key(0)] public MessageType Type => MessageType.S2S_MatchFound;
    [Key(1)] public Guid QueueId { get; set; }
    [Key(2)] public Guid MatchId { get; set; }
    [Key(3)] public List<long> PlayerIds { get; set; }
    [Key(4)] public string InstanceId { get; set; }
    [Key(5)] public int AcceptTimeout { get; set; } // Sekunden
}

// Option B: Timeout (kein Match in X Minuten)
// Kein Response - Client UI zeigt weiterhin "In Queue"
```

#### C# Beispiel
```csharp
public async Task QueueForMatchmakingAsync(long playerId, string matchType)
{
    var player = GetPlayer(playerId);
    if (player == null) return;
    
    var queueRequest = new S2S_MatchmakingQueue
    {
        Type = MessageType.S2S_MatchmakingQueue,
        QueueId = Guid.NewGuid(),
        PlayerId = playerId,
        PlayerName = player.Name,
        MatchType = matchType,
        ItemLevel = player.GetAverageItemLevel(),
        Rating = player.Rating,
        Roles = player.GetRoles(),
        SourceServerId = _serverId
    };
    
    // Store queue info locally for tracking
    _activeQueues[playerId] = queueRequest.QueueId;
    
    await _redis.PublishAsync("s2s:matchmaking", queueRequest);
    
    _logger.LogInformation(
        "Player {PlayerId} queued for {MatchType}", 
        playerId, matchType);
}
```

### 5.5 S2S_AuctionCreate (5400)

**Richtung:** Zone Server → Auction Service (via Redis)  
**Channel:** `s2s:auction`  
**Timeout:** 1000ms  
**Retry:** 2x

#### Beschreibung
Erstellt eine neue Auction im Auction-House.

#### Payload
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Type | MessageType | `S2S_AuctionCreate` |
| AuctionId | Guid | Eindeutige Auction-ID |
| SellerId | long | Verkäufer Player-ID |
| ItemId | int | Item-Template-ID |
| ItemInstanceId | Guid | Item-Instanz-ID |
| Quantity | int | Anzahl (Stackable Items) |
| StartingBid | long | Mindestgebot (Gold) |
| BuyoutPrice | long? | Sofortkauf-Preis (optional) |
| Duration | int | Laufzeit in Stunden (12, 24, 48) |

#### Response
```csharp
[MessagePackObject]
public class S2S_AuctionCreateResponse
{
    [Key(0)] public MessageType Type => MessageType.S2S_AuctionCreateResponse;
    [Key(1)] public Guid AuctionId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public string ErrorCode { get; set; }
    [Key(4)] public long AuctionHouseFee { get; set; } // 5% von StartingBid
}
```

#### C# Beispiel
```csharp
public async Task<bool> CreateAuctionAsync(
    long playerId, 
    Guid itemInstanceId, 
    long startingBid,
    long? buyoutPrice,
    int durationHours)
{
    var player = GetPlayer(playerId);
    var item = player.Inventory.GetItem(itemInstanceId);
    
    if (item == null) return false;
    
    var request = new S2S_AuctionCreate
    {
        Type = MessageType.S2S_AuctionCreate,
        AuctionId = Guid.NewGuid(),
        SellerId = playerId,
        ItemId = item.ItemId,
        ItemInstanceId = itemInstanceId,
        Quantity = item.Quantity,
        StartingBid = startingBid,
        BuyoutPrice = buyoutPrice,
        Duration = durationHours
    };
    
    var response = await _redisMessageBroker.RequestAsync<S2S_AuctionCreateResponse>(
        channel: "s2s:auction",
        request: request,
        timeout: TimeSpan.FromSeconds(1),
        retries: 2
    );
    
    if (response.Success)
    {
        // Deduct auction house fee
        player.Gold -= response.AuctionHouseFee;
        
        // Remove item from inventory
        player.Inventory.RemoveItem(itemInstanceId);
        
        _logger.LogInformation(
            "Auction created: {AuctionId} by player {PlayerId}", 
            request.AuctionId, playerId);
        
        return true;
    }
    
    _logger.LogWarning(
        "Auction creation failed: {ErrorCode}", 
        response.ErrorCode);
    
    return false;
}
```

### 5.6 S2S_AdminCommand (5500)

**Richtung:** Admin Tool → Target Server (via Redis)  
**Channel:** `s2s:admin` oder `s2s:server:{serverId}`  
**Timeout:** 5000ms  
**Retry:** Nein (Admin muss manuell wiederholen)

#### Beschreibung
Remote-Admin-Befehl für Server-Management (Kick, Ban, Broadcast, etc.)

#### Payload
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Type | MessageType | `S2S_AdminCommand` |
| CommandId | Guid | Command-Tracking-ID |
| AdminId | long | Admin Account-ID |
| TargetServerId | string? | Ziel-Server (null = broadcast) |
| Command | string | "KICK", "BAN", "BROADCAST", etc. |
| Parameters | Dictionary<string, string> | Command-Parameter |

#### Beispiel-Commands
```csharp
// Kick Player
new S2S_AdminCommand
{
    Command = "KICK",
    Parameters = new Dictionary<string, string>
    {
        ["PlayerId"] = "12345",
        ["Reason"] = "Spamming"
    }
}

// Global Broadcast
new S2S_AdminCommand
{
    Command = "BROADCAST",
    Parameters = new Dictionary<string, string>
    {
        ["Message"] = "Server restart in 10 minutes",
        ["Color"] = "Red"
    }
}

// Ban Player
new S2S_AdminCommand
{
    Command = "BAN",
    Parameters = new Dictionary<string, string>
    {
        ["PlayerId"] = "67890",
        ["Duration"] = "86400", // Sekunden
        ["Reason"] = "Cheating"
    }
}
```

---

## 6. Security für S2S-Kommunikation

### 6.1 Server-Authentifizierung

Jeder Server hat eine **unique Server-ID** und ein **shared secret**.

```csharp
public class ServerConfig
{
    public string ServerId { get; set; } // "gateway-1", "zone-forest-shard-1"
    public string ServerSecret { get; set; } // 256-bit key
    public string ServerType { get; set; } // "Gateway", "Zone", "Instance"
}
```

### 6.2 Message-Signing

Alle S2S-Messages werden mit **HMAC-SHA256** signiert:

```csharp
[MessagePackObject]
public class SignedS2SMessage
{
    [Key(0)] public string ServerId { get; set; }
    [Key(1)] public long Timestamp { get; set; }
    [Key(2)] public byte[] MessagePayload { get; set; }
    [Key(3)] public byte[] Signature { get; set; } // HMAC-SHA256
}

// Signatur-Berechnung
public byte[] SignMessage(byte[] payload, string serverId, string secret)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    
    var dataToSign = new byte[serverId.Length + 8 + payload.Length];
    Buffer.BlockCopy(Encoding.UTF8.GetBytes(serverId), 0, dataToSign, 0, serverId.Length);
    Buffer.BlockCopy(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()), 0, 
        dataToSign, serverId.Length, 8);
    Buffer.BlockCopy(payload, 0, dataToSign, serverId.Length + 8, payload.Length);
    
    return hmac.ComputeHash(dataToSign);
}
```

### 6.3 Encryption

- **Redis TLS**: Alle Redis-Connections über TLS 1.3
- **VNet-Isolation**: Azure VNet für Server-Cluster (kein Public Internet)

```yaml
Redis:
  Connection:
    Ssl: true
    SslProtocols: Tls13
    AbortOnConnectFail: false
```

### 6.4 Rate-Limiting

Server-spezifische Rate-Limits für S2S-Messages:

| Server-Typ | Messages/Second | Burst |
|------------|----------------|-------|
| Gateway | 1000 | 2000 |
| Zone Server | 500 | 1000 |
| Instance Server | 200 | 400 |
| Services | 100 | 200 |

```csharp
public class S2SRateLimiter
{
    private readonly Dictionary<string, TokenBucket> _buckets = new();
    
    public bool AllowMessage(string serverId, string serverType)
    {
        if (!_buckets.TryGetValue(serverId, out var bucket))
        {
            bucket = CreateBucketForServerType(serverType);
            _buckets[serverId] = bucket;
        }
        
        return bucket.TryConsume();
    }
    
    private TokenBucket CreateBucketForServerType(string serverType)
    {
        return serverType switch
        {
            "Gateway" => new TokenBucket(capacity: 2000, refillRate: 1000),
            "Zone" => new TokenBucket(capacity: 1000, refillRate: 500),
            "Instance" => new TokenBucket(capacity: 400, refillRate: 200),
            _ => new TokenBucket(capacity: 200, refillRate: 100)
        };
    }
}
```

### 6.5 Payload-Validation

Alle S2S-Payloads werden validiert:

```csharp
public class S2SMessageValidator
{
    public ValidationResult Validate(object message)
    {
        // 1. Size check
        var serialized = MessagePackSerializer.Serialize(message);
        if (serialized.Length > MAX_S2S_MESSAGE_SIZE)
        {
            return ValidationResult.Error("Message too large");
        }
        
        // 2. Required fields
        if (message is IS2SMessage s2sMsg)
        {
            if (string.IsNullOrEmpty(s2sMsg.RequestingServerId))
            {
                return ValidationResult.Error("ServerId required");
            }
        }
        
        // 3. Type-specific validation
        return message switch
        {
            S2S_SessionValidate sv => ValidateSessionValidate(sv),
            S2S_ZoneTransferRequest zt => ValidateZoneTransfer(zt),
            _ => ValidationResult.Success()
        };
    }
}
```

---

## 7. Monitoring & Alerting

### 7.1 Metriken

#### S2S-Latency
```csharp
// Histogram für S2S-Message Round-Trip-Time
Metrics.CreateHistogram(
    "s2s_message_latency_ms",
    "S2S message round-trip latency",
    new HistogramConfiguration
    {
        Buckets = new[] { 10, 25, 50, 100, 250, 500, 1000, 2000 },
        LabelNames = new[] { "message_type", "source_server", "target_server" }
    }
);
```

#### S2S-Throughput
```csharp
// Counter für S2S-Messages
Metrics.CreateCounter(
    "s2s_messages_total",
    "Total S2S messages sent/received",
    new CounterConfiguration
    {
        LabelNames = new[] { "message_type", "direction", "status" }
    }
);
```

#### Error-Rate
```csharp
// Counter für S2S-Errors
Metrics.CreateCounter(
    "s2s_errors_total",
    "Total S2S errors",
    new CounterConfiguration
    {
        LabelNames = new[] { "message_type", "error_code" }
    }
);
```

### 7.2 Alerts

#### Alert-Regeln (Prometheus)
```yaml
groups:
  - name: s2s_alerts
    interval: 30s
    rules:
      # S2S Latency > 100ms
      - alert: S2SHighLatency
        expr: histogram_quantile(0.95, s2s_message_latency_ms_bucket) > 100
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High S2S latency detected"
          description: "P95 S2S latency is {{ $value }}ms"
      
      # S2S Error Rate > 1%
      - alert: S2SHighErrorRate
        expr: rate(s2s_errors_total[5m]) / rate(s2s_messages_total[5m]) > 0.01
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "High S2S error rate"
          description: "S2S error rate is {{ $value | humanizePercentage }}"
      
      # Gateway nicht erreichbar
      - alert: GatewayUnreachable
        expr: up{job="gateway"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Gateway server down"
          description: "Gateway {{ $labels.instance }} is unreachable"
      
      # Zone Server nicht antwortend
      - alert: ZoneServerNotResponding
        expr: rate(s2s_session_validate_timeout_total[5m]) > 0.1
        for: 3m
        labels:
          severity: warning
        annotations:
          summary: "Zone server not responding to session validations"
```

### 7.3 Dashboards (Grafana)

#### S2S Overview Dashboard
```json
{
  "dashboard": {
    "title": "S2S Communication Overview",
    "panels": [
      {
        "title": "S2S Message Rate",
        "targets": [
          {
            "expr": "rate(s2s_messages_total[5m])",
            "legendFormat": "{{message_type}}"
          }
        ]
      },
      {
        "title": "S2S Latency P95",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, s2s_message_latency_ms_bucket)",
            "legendFormat": "{{message_type}}"
          }
        ]
      },
      {
        "title": "S2S Error Rate",
        "targets": [
          {
            "expr": "rate(s2s_errors_total[5m]) / rate(s2s_messages_total[5m])",
            "legendFormat": "{{message_type}}"
          }
        ]
      }
    ]
  }
}
```

### 7.4 Logging

#### Structured Logging (Serilog)
```csharp
// S2S Request Logging
_logger.LogInformation(
    "S2S Request: {MessageType} from {SourceServer} to {TargetServer} - RequestId: {RequestId}",
    messageType, sourceServerId, targetServerId, requestId);

// S2S Response Logging
_logger.LogInformation(
    "S2S Response: {MessageType} - RequestId: {RequestId} - Latency: {Latency}ms - Status: {Status}",
    messageType, requestId, latency, status);

// S2S Error Logging
_logger.LogError(
    "S2S Error: {MessageType} - RequestId: {RequestId} - ErrorCode: {ErrorCode} - Message: {ErrorMessage}",
    messageType, requestId, errorCode, errorMessage);
```

---

## 8. Verwandte Dokumentation

- **[Redis-Strategie](REDIS.md)** - Redis Pub/Sub Details
- **[Server-Komponenten](SERVER_COMPONENTS.md)** - Gateway & Zone Server Architektur
- **[Skalierung](SCALING.md)** - Zone Sharding & Auto-Scaling
- **[Sicherheit](SECURITY.md)** - Security Layers
- **[Message-Spezifikation](MESSAGES.md)** - Message-System
- **[Messages: 50-server-to-server.md](../API/50-server-to-server.md)** - S2S Message Reference

---

## 9. Änderungshistorie

| Version | Datum | Änderungen |
|---------|-------|------------|
| 2.0.0 | 2025-12-25 | Initial: S2S-Kommunikation & Load-Balancing Dokumentation |

---

*Teil der [Architektur-Dokumentation](Architecture-Overview.md)*

Source: docs/02-architecture/SERVER_TO_SERVER.md
