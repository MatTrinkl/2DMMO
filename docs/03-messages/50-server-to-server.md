# 🌐 Server-to-Server (S2S) Messages (5000-5999)

**Kategorien:** 50-55  
**Range:** 5000-5999  

**Status:** 📋 Geplant (noch nicht im MessageType enum)

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst **alle Server-zu-Server (S2S) Messages** im 2DMMO.

**Wichtig**: Diese Messages sind **AUSSCHLIESSLICH für Server-zu-Server Kommunikation**. Clients senden oder empfangen diese Messages **NIEMALS**.

### S2S-Kommunikation
- **Transport**: Redis Pub/Sub
- **Serialisierung**: MessagePack (wie Client-Messages)
- **Security**: HMAC-SHA256 Signing + TLS
- **Timeout**: Je nach Message-Typ (siehe Details)

### Kategorien

| Kategorie | Range | Zweck | Status |
|-----------|-------|-------|--------|
| **S2S Core** | 5000-5099 | Session, Health, Discovery | 📋 Geplant |
| **S2S Transfer** | 5100-5199 | Zone/Shard-Transfers | 📋 Geplant |
| **S2S Cross-Zone** | 5200-5299 | Cross-Zone Features | 📋 Geplant |
| **S2S Matchmaking** | 5300-5399 | Matchmaking & Instances | 📋 Geplant |
| **S2S Economy** | 5400-5499 | Auction & Economy | 📋 Geplant |
| **S2S Admin** | 5500-5599 | Admin & Monitoring | 📋 Geplant |

---

## 📋 Inhaltsverzeichnis

### Kategorie 50: S2S Core (5000-5099)
- [S2S_SessionValidate (5000)](#s2s_sessionvalidate-5000)
- [S2S_SessionValidateResponse (5001)](#s2s_sessionvalidateresponse-5001)
- [S2S_PlayerLocate (5002)](#s2s_playerlocate-5002)
- [S2S_PlayerLocateResponse (5003)](#s2s_playerlocateresponse-5003)
- [S2S_ServerHandshake (5004)](#s2s_serverhandshake-5004)
- [S2S_ServerHeartbeat (5005)](#s2s_serverheartbeat-5005)
- [S2S_ServerShutdownNotify (5006)](#s2s_servershutdownnotify-5006)

### Kategorie 51: S2S Zone Transfer (5100-5199)
- [S2S_ZoneTransferRequest (5100)](#s2s_zonetransferrequest-5100)
- [S2S_ZoneTransferPrepare (5101)](#s2s_zonetransferprepare-5101)
- [S2S_ZoneTransferReady (5102)](#s2s_zonetransferready-5102)
- [S2S_ZoneTransferExecute (5103)](#s2s_zonetransferexecute-5103)
- [S2S_ZoneTransferComplete (5104)](#s2s_zonetransfercomplete-5104)
- [S2S_ZoneTransferFailed (5105)](#s2s_zonetransferfailed-5105)
- [S2S_ShardTransfer (5110)](#s2s_shardtransfer-5110)
- [S2S_InstanceCreate (5111)](#s2s_instancecreate-5111)
- [S2S_InstanceDestroy (5112)](#s2s_instancedestroy-5112)

### Kategorie 52: S2S Cross-Zone Features (5200-5299)
- [S2S_CrossZoneWhisper (5200)](#s2s_crosszonewhisper-5200)
- [S2S_CrossZonePartyInvite (5201)](#s2s_crosszonepartyinvite-5201)
- [S2S_CrossZonePartyUpdate (5202)](#s2s_crosszonepartyupdate-5202)
- [S2S_CrossZoneGuildMessage (5203)](#s2s_crosszoneguildmessage-5203)
- [S2S_CrossZoneFriendStatus (5204)](#s2s_crosszonefriendstatus-5204)
- [S2S_CrossZoneTrade (5210)](#s2s_crosszonetrade-5210)
- [S2S_CrossZoneMail (5211)](#s2s_crosszonemail-5211)
- [S2S_GlobalBroadcast (5220)](#s2s_globalbroadcast-5220)
- [S2S_ZoneBroadcast (5221)](#s2s_zonebroadcast-5221)

### Kategorie 53: S2S Matchmaking & Instances (5300-5399)
- [S2S_MatchmakingQueue (5300)](#s2s_matchmakingqueue-5300)
- [S2S_MatchmakingDequeue (5301)](#s2s_matchmakingdequeue-5301)
- [S2S_MatchFound (5302)](#s2s_matchfound-5302)
- [S2S_MatchAccepted (5303)](#s2s_matchaccepted-5303)
- [S2S_MatchCancelled (5304)](#s2s_matchcancelled-5304)
- [S2S_InstanceRequest (5310)](#s2s_instancerequest-5310)
- [S2S_InstanceReady (5311)](#s2s_instanceready-5311)
- [S2S_InstancePlayerJoin (5312)](#s2s_instanceplayerjoin-5312)
- [S2S_InstancePlayerLeave (5313)](#s2s_instanceplayerleave-5313)
- [S2S_InstanceComplete (5314)](#s2s_instancecomplete-5314)

### Kategorie 54: S2S Economy & Auction (5400-5499)
- [S2S_AuctionCreate (5400)](#s2s_auctioncreate-5400)
- [S2S_AuctionBid (5401)](#s2s_auctionbid-5401)
- [S2S_AuctionBuyout (5402)](#s2s_auctionbuyout-5402)
- [S2S_AuctionExpired (5403)](#s2s_auctionexpired-5403)
- [S2S_AuctionSold (5404)](#s2s_auctionsold-5404)
- [S2S_GoldTransfer (5410)](#s2s_goldtransfer-5410)
- [S2S_ItemTransfer (5411)](#s2s_itemtransfer-5411)

### Kategorie 55: S2S Admin & Monitoring (5500-5599)
- [S2S_AdminCommand (5500)](#s2s_admincommand-5500)
- [S2S_AdminCommandResponse (5501)](#s2s_admincommandresponse-5501)
- [S2S_PlayerKick (5502)](#s2s_playerkick-5502)
- [S2S_PlayerBan (5503)](#s2s_playerban-5503)
- [S2S_PlayerMute (5504)](#s2s_playermute-5504)
- [S2S_ServerMetrics (5510)](#s2s_servermetrics-5510)
- [S2S_ZoneMetrics (5511)](#s2s_zonemetrics-5511)
- [S2S_AlertTrigger (5512)](#s2s_alerttrigger-5512)
- [S2S_MaintenanceSchedule (5520)](#s2s_maintenanceschedule-5520)
- [S2S_MaintenanceCancel (5521)](#s2s_maintenancecancel-5521)

---

## Kategorie 50: S2S Core (5000-5099)

### S2S_SessionValidate (5000)

**Richtung:** 📤 Zone Server → Gateway Server  
**Channel:** `s2s:session:validate`  
**Frequenz:** Häufig  
**Timeout:** 500ms  
**Retry:** 3x (exponential backoff)

#### Beschreibung
Zone-Server validiert Session-Token beim Gateway, wenn ein Spieler die Zone betritt. Ersetzt die veraltete `SessionValidate (8)` Message.

#### Im Scope ✅
- Token-Validierung zwischen Servern
- Session-Informationen Transfer
- Request-Response Pattern mit Korrelations-ID

#### Nicht im Scope ❌
- Client-Server Kommunikation (Client sendet diese Message NIEMALS)
- Session-Erstellung (verwende `LoginRequest`)

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_SessionValidate` | Ja |
| RequestId | Guid | Korrelations-ID für Response | Ja |
| SessionToken | string | Zu validierendes Token | Ja |
| RequestingServerId | string | Zone-Server ID | Ja |
| PlayerId | long | Player-ID für Lookup | Ja |

#### Erwartete Response
- **Bei Erfolg:** `S2S_SessionValidateResponse (5001)` mit Valid=true
- **Bei Fehler:** `S2S_SessionValidateResponse (5001)` mit ErrorCode

#### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `S2S_SessionValidateResponse` | 5001 | Response zu diesem Request |
| `SessionValidate` | 8 | **DEPRECATED** - Alt-Version |

#### Beispiel Payload
```csharp
var request = new S2S_SessionValidate
{
    Type = MessageType.S2S_SessionValidate,
    RequestId = Guid.NewGuid(),
    SessionToken = "abc123...",
    RequestingServerId = "zone-forest-shard-1",
    PlayerId = 12345
};

var response = await _redisMessageBroker.RequestAsync<S2S_SessionValidateResponse>(
    channel: "s2s:session:validate",
    request: request,
    timeout: TimeSpan.FromMilliseconds(500),
    retries: 3
);
```

#### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `SESSION_NOT_FOUND` | Token ungültig | Disconnect Player |
| `SESSION_EXPIRED` | Token abgelaufen | Disconnect Player |
| `GATEWAY_UNAVAILABLE` | Gateway nicht erreichbar | Retry |
| `TIMEOUT` | Keine Antwort in 500ms | Retry |

#### Notizen
- Cached erfolgreiche Validierungen für 60s
- Cache-Key: `session:cache:{sessionToken}`
- Siehe [SERVER_TO_SERVER.md](../../02-architecture/SERVER_TO_SERVER.md#51-s2s_sessionvalidate-5000) für Details

---

### S2S_SessionValidateResponse (5001)

**Richtung:** 📥 Gateway Server → Zone Server  
**Channel:** `s2s:session:validate:response`  
**Frequenz:** Häufig  
**Timeout:** N/A (Response)

#### Beschreibung
Response auf `S2S_SessionValidate (5000)`. Enthält Session-Daten oder Error-Code.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_SessionValidateResponse` | Ja |
| RequestId | Guid | Korrelations-ID | Ja |
| Valid | bool | Token gültig? | Ja |
| AccountId | long | Account-ID (bei Valid) | Conditional |
| CharacterId | long | Character-ID (bei Valid) | Conditional |
| Permissions | List<string> | Account-Rechte | Conditional |
| ErrorCode | string | Fehler (bei !Valid) | Conditional |

#### Beispiel Payload
```csharp
// Erfolgreiche Validierung
var response = new S2S_SessionValidateResponse
{
    Type = MessageType.S2S_SessionValidateResponse,
    RequestId = requestId,
    Valid = true,
    AccountId = 67890,
    CharacterId = 12345,
    Permissions = new List<string> { "Player", "Premium" }
};

// Fehlgeschlagene Validierung
var errorResponse = new S2S_SessionValidateResponse
{
    Type = MessageType.S2S_SessionValidateResponse,
    RequestId = requestId,
    Valid = false,
    ErrorCode = "SESSION_EXPIRED"
};
```

---

### S2S_PlayerLocate (5002)

**Richtung:** 📤 Server → All Servers (Broadcast)  
**Channel:** `s2s:server:broadcast`  
**Frequenz:** Selten  
**Timeout:** 1000ms  
**Retry:** Nein

#### Beschreibung
Findet einen Spieler über alle Server hinweg. Broadcast-Request, erster Server der den Spieler hat antwortet.

#### Im Scope ✅
- Spieler-Suche über alle Zones
- Cross-Zone Whisper
- Party-Invites

#### Nicht im Scope ❌
- Persistente Player-Location (verwende Redis `player:location:{playerId}`)

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_PlayerLocate` | Ja |
| RequestId | Guid | Korrelations-ID | Ja |
| PlayerId | long | Gesuchter Spieler | Ja |
| RequestingServerId | string | Anfragender Server | Ja |

#### Erwartete Response
- **Bei Erfolg:** `S2S_PlayerLocateResponse (5003)` vom Server wo Spieler ist
- **Bei Fehler:** Timeout (Spieler offline)

#### Beispiel Payload
```csharp
var request = new S2S_PlayerLocate
{
    Type = MessageType.S2S_PlayerLocate,
    RequestId = Guid.NewGuid(),
    PlayerId = 12345,
    RequestingServerId = "zone-town-shard-2"
};

await _redis.PublishAsync("s2s:server:broadcast", request);
```

---

### S2S_PlayerLocateResponse (5003)

**Richtung:** 📥 Server → Requesting Server  
**Channel:** `s2s:server:{requestingServerId}`  
**Frequenz:** Selten

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_PlayerLocateResponse` | Ja |
| RequestId | Guid | Korrelations-ID | Ja |
| PlayerId | long | Gefundener Spieler | Ja |
| ServerId | string | Server wo Spieler ist | Ja |
| ZoneId | string | Zone-ID | Ja |
| ShardId | int | Shard-ID | Ja |

---

### S2S_ServerHandshake (5004)

**Richtung:** 📤 Server → Server  
**Channel:** `s2s:server:broadcast`  
**Frequenz:** Einmalig pro Server-Start

#### Beschreibung
Server authentifiziert sich beim Cluster-Start. Teilt Server-ID, Typ und Capabilities mit.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ServerHandshake` | Ja |
| ServerId | string | Unique Server-ID | Ja |
| ServerType | string | "Gateway", "Zone", "Instance" | Ja |
| Version | string | Server-Version | Ja |
| Capabilities | List<string> | Features | Ja |
| MaxPlayers | int | Kapazität | Ja |

---

### S2S_ServerHeartbeat (5005)

**Richtung:** 📤 Server → Monitoring  
**Channel:** `s2s:metrics`  
**Frequenz:** Alle 30s

#### Beschreibung
Server-Health-Check. Zeigt an dass Server noch läuft.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ServerHeartbeat` | Ja |
| ServerId | string | Server-ID | Ja |
| Timestamp | long | Unix Timestamp | Ja |
| PlayerCount | int | Aktuelle Spielerzahl | Ja |
| CpuUsage | float | CPU % (0-100) | Ja |
| MemoryUsage | long | Memory in Bytes | Ja |
| TickRate | float | Ticks/Second | Ja |

---

### S2S_ServerShutdownNotify (5006)

**Richtung:** 📤 Server → All Servers  
**Channel:** `s2s:server:broadcast`  
**Frequenz:** Einmalig (vor Shutdown)

#### Beschreibung
Server kündigt Shutdown an. Andere Server können Spieler umleiten.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ServerShutdownNotify` | Ja |
| ServerId | string | Server der runterfahren wird | Ja |
| Reason | string | "Maintenance", "Crash", "Upgrade" | Ja |
| ShutdownIn | int | Sekunden bis Shutdown | Ja |
| Graceful | bool | Graceful Shutdown? | Ja |

---

## Kategorie 51: S2S Zone Transfer (5100-5199)

### S2S_ZoneTransferRequest (5100)

**Richtung:** 📤 Source Zone → Target Zone  
**Channel:** `s2s:transfer:zone:{targetZoneId}`  
**Frequenz:** Häufig  
**Timeout:** 2000ms  
**Retry:** 1x

#### Beschreibung
Initiiert einen Spieler-Transfer zwischen zwei Zones. Source-Zone sendet vollständigen Player-State an Target-Zone.

#### Im Scope ✅
- Spieler-Transfer zwischen Zones
- Player-State Transfer (HP, Buffs, Inventory, Cooldowns)
- Shard-Assignment im Ziel

#### Nicht im Scope ❌
- Shard-Transfer innerhalb Zone → verwende `S2S_ShardTransfer (5110)`
- Instance-Transfer → verwende `S2S_InstanceRequest (5310)`

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ZoneTransferRequest` | Ja |
| TransferId | Guid | Transfer-Tracking-ID | Ja |
| PlayerId | long | Zu transferierender Spieler | Ja |
| TargetZoneId | string | Ziel-Zone ID | Ja |
| TargetShardId | int? | Präferierter Shard | Nein |
| EntryPoint | Vector2 | Spawn-Position | Ja |
| PlayerState | PlayerStateSnapshot | Vollständiger State | Ja |
| Reason | string | "Portal", "Border", "Teleport" | Ja |

#### PlayerStateSnapshot
```csharp
[MessagePackObject]
public class PlayerStateSnapshot
{
    [Key(0)] public long PlayerId { get; set; }
    [Key(1)] public string CharacterName { get; set; }
    [Key(2)] public int Level { get; set; }
    [Key(3)] public int CurrentHp { get; set; }
    [Key(4)] public int MaxHp { get; set; }
    [Key(5)] public int CurrentMana { get; set; }
    [Key(6)] public int MaxMana { get; set; }
    [Key(7)] public List<BuffData> ActiveBuffs { get; set; }
    [Key(8)] public List<ItemData> Inventory { get; set; }
    [Key(9)] public Dictionary<string, int> Cooldowns { get; set; }
    [Key(10)] public float MountSpeed { get; set; }
    [Key(11)] public bool InCombat { get; set; }
}
```

#### Erwartete Response
- **Bei Erfolg:** `S2S_ZoneTransferReady (5102)`
- **Bei Fehler:** `S2S_ZoneTransferFailed (5105)`

#### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `S2S_ZoneTransferReady` | 5102 | Success Response |
| `S2S_ZoneTransferFailed` | 5105 | Error Response |
| `S2S_ZoneTransferComplete` | 5104 | Finaler Ack |

#### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ZONE_FULL` | Alle Shards voll | Retry später |
| `ZONE_NOT_FOUND` | Ziel-Zone existiert nicht | Error an Client |
| `INVALID_ENTRY_POINT` | Spawn-Position ungültig | Fallback zu Default |
| `TRANSFER_TIMEOUT` | Keine Antwort in 2s | Retry 1x |

#### Beispiel Payload
```csharp
var request = new S2S_ZoneTransferRequest
{
    Type = MessageType.S2S_ZoneTransferRequest,
    TransferId = Guid.NewGuid(),
    PlayerId = 12345,
    TargetZoneId = "zone_forest",
    EntryPoint = new Vector2(100, 200),
    PlayerState = CreatePlayerSnapshot(player),
    Reason = "Portal"
};

var response = await _redisMessageBroker.RequestAsync<S2S_ZoneTransferReady>(
    channel: $"s2s:transfer:zone:{request.TargetZoneId}",
    request: request,
    timeout: TimeSpan.FromMilliseconds(2000),
    retries: 1
);
```

#### Notizen
- Siehe [SERVER_TO_SERVER.md](../../02-architecture/SERVER_TO_SERVER.md#52-s2s_zonetransferrequest-5100) für Flow-Diagramm

---

### S2S_ZoneTransferPrepare (5101)

**Richtung:** 📥 Target Zone → Source Zone  
**Channel:** `s2s:transfer:prepare`  
**Frequenz:** Häufig

#### Beschreibung
Target-Zone bestätigt Empfang und beginnt Vorbereitung (Shard-Selection, Spawn-Validation).

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ZoneTransferPrepare` | Ja |
| TransferId | Guid | Transfer-Tracking-ID | Ja |
| EstimatedReadyTime | int | Millisekunden bis Ready | Ja |

---

### S2S_ZoneTransferReady (5102)

**Richtung:** 📥 Target Zone → Source Zone  
**Channel:** Direct Response  
**Frequenz:** Häufig

#### Beschreibung
Target-Zone ist bereit für Transfer. Spieler kann jetzt wechseln.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ZoneTransferReady` | Ja |
| TransferId | Guid | Transfer-Tracking-ID | Ja |
| Success | bool | Transfer vorbereitet? | Ja |
| AssignedShardId | int | Zugewiesener Shard | Conditional |
| SpawnPosition | Vector2 | Finale Spawn-Position | Conditional |
| ErrorCode | string? | Fehler-Code bei !Success | Conditional |

---

### S2S_ZoneTransferExecute (5103)

**Richtung:** 📤 Source Zone → Target Zone  
**Channel:** `s2s:transfer:execute`  
**Frequenz:** Häufig

#### Beschreibung
Source-Zone gibt grünes Licht - Transfer wird jetzt durchgeführt.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ZoneTransferExecute` | Ja |
| TransferId | Guid | Transfer-Tracking-ID | Ja |
| ClientConnected | bool | Client hat Zone geladen? | Ja |

---

### S2S_ZoneTransferComplete (5104)

**Richtung:** 📥 Target Zone → Source Zone  
**Channel:** `s2s:transfer:complete`  
**Frequenz:** Häufig

#### Beschreibung
Transfer erfolgreich abgeschlossen. Source-Zone kann Spieler entfernen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ZoneTransferComplete` | Ja |
| TransferId | Guid | Transfer-Tracking-ID | Ja |
| PlayerId | long | Transferierter Spieler | Ja |
| FinalShardId | int | Finaler Shard | Ja |

---

### S2S_ZoneTransferFailed (5105)

**Richtung:** 📥 Target Zone → Source Zone  
**Channel:** Direct Response  
**Frequenz:** Selten

#### Beschreibung
Transfer fehlgeschlagen. Spieler bleibt in Source-Zone.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ZoneTransferFailed` | Ja |
| TransferId | Guid | Transfer-Tracking-ID | Ja |
| ErrorCode | string | Fehler-Grund | Ja |
| ErrorMessage | string | Detail-Message | Ja |
| Recoverable | bool | Kann Retry helfen? | Ja |

---

### S2S_ShardTransfer (5110)

**Richtung:** 📤 Source Shard → Target Shard (gleiche Zone)  
**Channel:** `s2s:transfer:shard:{targetShardId}`  
**Frequenz:** Selten  
**Timeout:** 1000ms

#### Beschreibung
Spieler-Transfer zwischen Shards innerhalb der gleichen Zone (z.B. Shard-Balancing).

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ShardTransfer` | Ja |
| PlayerId | long | Zu transferierender Spieler | Ja |
| SourceShardId | int | Aktueller Shard | Ja |
| TargetShardId | int | Ziel-Shard | Ja |
| PlayerState | PlayerStateSnapshot | Player-State | Ja |
| Reason | string | "Balancing", "PartyJoin", "Manual" | Ja |

---

### S2S_InstanceCreate (5111)

**Richtung:** 📤 Zone Server → Instance Pool  
**Channel:** `s2s:transfer:instance`  
**Frequenz:** Häufig  
**Timeout:** 2000ms

#### Beschreibung
Fordert Erstellung einer neuen Instance an (Dungeon, Arena, etc.).

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_InstanceCreate` | Ja |
| InstanceId | Guid | Eindeutige Instance-ID | Ja |
| InstanceType | string | "Dungeon", "Arena", "Raid" | Ja |
| TemplateId | string | "dungeon_forest_easy" | Ja |
| MaxPlayers | int | Max. Spielerzahl | Ja |
| RequestingServerId | string | Anfragender Server | Ja |
| CreatorPlayerId | long | Ersteller | Ja |

---

### S2S_InstanceDestroy (5112)

**Richtung:** 📤 Instance Server → Zone Server  
**Channel:** `s2s:transfer:instance`  
**Frequency:** Selten

#### Beschreibung
Instance wird zerstört. Spieler müssen zurück in normale Zone.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_InstanceDestroy` | Ja |
| InstanceId | Guid | Instance-ID | Ja |
| Reason | string | "Complete", "Timeout", "Failure" | Ja |
| PlayerIds | List<long> | Spieler in Instance | Ja |
| ReturnZoneId | string | Return-Zone | Ja |

---

## Kategorie 52: S2S Cross-Zone Features (5200-5299)

### S2S_CrossZoneWhisper (5200)

**Richtung:** 📤 Source Zone → Target Zone  
**Channel:** `s2s:player:{targetPlayerId}`  
**Frequenz:** Häufig  
**Timeout:** N/A (Fire-and-Forget)

#### Beschreibung
Sendet eine Whisper-Message an einen Spieler in einer anderen Zone.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_CrossZoneWhisper` | Ja |
| TargetPlayerId | long | Empfänger-ID | Ja |
| SenderPlayerId | long | Absender-ID | Ja |
| SenderName | string | Absender-Name | Ja |
| Message | string | Whisper-Text | Ja |
| Timestamp | long | Unix Timestamp | Ja |

#### Beispiel
```csharp
var whisper = new S2S_CrossZoneWhisper
{
    Type = MessageType.S2S_CrossZoneWhisper,
    TargetPlayerId = 67890,
    SenderPlayerId = 12345,
    SenderName = "PlayerOne",
    Message = "Hello from another zone!",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

await _redis.PublishAsync($"s2s:player:{whisper.TargetPlayerId}", whisper);
```

---

### S2S_CrossZonePartyInvite (5201)

**Richtung:** 📤 Source Zone → Target Zone  
**Channel:** `s2s:player:{targetPlayerId}`  
**Frequenz:** Häufig

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_CrossZonePartyInvite` | Ja |
| PartyId | Guid | Party-ID | Ja |
| InviterPlayerId | long | Einlader | Ja |
| InviterName | string | Einlader-Name | Ja |
| TargetPlayerId | long | Eingeladener | Ja |
| InviteTimeout | int | Sekunden bis Ablauf | Ja |

---

### S2S_CrossZonePartyUpdate (5202)

**Richtung:** 📤 Any Zone → All Party Members  
**Channel:** `s2s:party:{partyId}`  
**Frequenz:** Häufig

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_CrossZonePartyUpdate` | Ja |
| PartyId | Guid | Party-ID | Ja |
| UpdateType | string | "MemberJoined", "MemberLeft", "LeaderChanged" | Ja |
| PlayerId | long | Betroffener Spieler | Ja |
| PartyMembers | List<PartyMemberInfo> | Aktuelle Members | Ja |

---

### S2S_CrossZoneGuildMessage (5203)

**Richtung:** 📤 Any Zone → All Guild Members  
**Channel:** `s2s:guild:{guildId}`  
**Frequenz:** Häufig

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_CrossZoneGuildMessage` | Ja |
| GuildId | long | Guild-ID | Ja |
| SenderPlayerId | long | Absender | Ja |
| SenderName | string | Absender-Name | Ja |
| MessageType | string | "Chat", "Event", "Announcement" | Ja |
| Message | string | Message-Text | Ja |
| Timestamp | long | Unix Timestamp | Ja |

---

### S2S_CrossZoneFriendStatus (5204)

**Richtung:** 📤 Zone → Friends' Zones  
**Channel:** `s2s:player:{friendPlayerId}`  
**Frequenz:** Selten

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_CrossZoneFriendStatus` | Ja |
| PlayerId | long | Spieler dessen Status sich ändert | Ja |
| Status | string | "Online", "Offline", "Away", "DND" | Ja |
| ZoneId | string? | Aktuelle Zone (bei Online) | Conditional |
| CustomStatus | string? | Custom-Status-Text | Nein |

---

### S2S_CrossZoneTrade (5210)

**Richtung:** 📤 Source Zone → Target Zone  
**Channel:** `s2s:player:{targetPlayerId}`  
**Frequenz:** Selten

#### Beschreibung
Trade-Request zwischen Spielern in verschiedenen Zones (via Mail-System).

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_CrossZoneTrade` | Ja |
| TradeId | Guid | Trade-Tracking-ID | Ja |
| SenderPlayerId | long | Verkäufer | Ja |
| TargetPlayerId | long | Käufer | Ja |
| ItemOffered | ItemData | Angebotenes Item | Ja |
| GoldRequested | long | Gefordertes Gold | Ja |

---

### S2S_CrossZoneMail (5211)

**Richtung:** 📤 Mail Service → Target Zone  
**Channel:** `s2s:player:{recipientPlayerId}`  
**Frequenz:** Häufig

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_CrossZoneMail` | Ja |
| MailId | Guid | Mail-ID | Ja |
| RecipientPlayerId | long | Empfänger | Ja |
| SenderName | string | Absender-Name | Ja |
| Subject | string | Betreff | Ja |
| HasAttachments | bool | Items/Gold angehängt? | Ja |
| Timestamp | long | Versand-Zeit | Ja |

---

### S2S_GlobalBroadcast (5220)

**Richtung:** 📤 Admin/System → All Servers  
**Channel:** `s2s:server:broadcast`  
**Frequenz:** Selten

#### Beschreibung
Server-weite Announcement (z.B. Maintenance-Warnung).

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_GlobalBroadcast` | Ja |
| Message | string | Broadcast-Text | Ja |
| Color | string | "Red", "Yellow", "Blue" | Ja |
| Duration | int | Sekunden anzeigen | Ja |
| Priority | int | 1-10 (10 = highest) | Ja |

---

### S2S_ZoneBroadcast (5221)

**Richtung:** 📤 Admin/System → Zone Servers  
**Channel:** `s2s:transfer:zone:{zoneId}`  
**Frequenz:** Selten

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ZoneBroadcast` | Ja |
| ZoneId | string | Ziel-Zone | Ja |
| Message | string | Broadcast-Text | Ja |
| Color | string | Message-Color | Ja |
| Duration | int | Sekunden anzeigen | Ja |

---

## Kategorie 53: S2S Matchmaking & Instances (5300-5399)

### S2S_MatchmakingQueue (5300)

**Richtung:** 📤 Zone Server → Matchmaking Service  
**Channel:** `s2s:matchmaking`  
**Frequenz:** Häufig  
**Timeout:** 1000ms

#### Beschreibung
Fügt einen Spieler zur Matchmaking-Queue hinzu.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_MatchmakingQueue` | Ja |
| QueueId | Guid | Queue-Tracking-ID | Ja |
| PlayerId | long | Spieler-ID | Ja |
| PlayerName | string | Spieler-Name | Ja |
| MatchType | string | "Arena2v2", "Arena3v3", "Dungeon5" | Ja |
| ItemLevel | int | Gear-Score | Ja |
| Rating | int | MMR | Ja |
| Roles | List<string> | ["Tank", "Healer", "DPS"] | Ja |
| SourceServerId | string | Zone-Server ID | Ja |

#### Erwartete Response
- **Match gefunden:** `S2S_MatchFound (5302)`
- **Timeout:** Kein Response (bleibt in Queue)

---

### S2S_MatchmakingDequeue (5301)

**Richtung:** 📤 Zone Server → Matchmaking Service  
**Channel:** `s2s:matchmaking`  
**Frequenz:** Selten

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_MatchmakingDequeue` | Ja |
| QueueId | Guid | Queue-ID | Ja |
| PlayerId | long | Spieler-ID | Ja |
| Reason | string | "PlayerCancelled", "Timeout", "Offline" | Ja |

---

### S2S_MatchFound (5302)

**Richtung:** 📥 Matchmaking Service → Zone Servers  
**Channel:** `s2s:server:{sourceServerId}`  
**Frequenz:** Häufig

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_MatchFound` | Ja |
| MatchId | Guid | Match-ID | Ja |
| QueueIds | List<Guid> | Queue-IDs aller Spieler | Ja |
| PlayerIds | List<long> | Matched Spieler | Ja |
| InstanceId | string | Zugewiesene Instance | Ja |
| AcceptTimeout | int | Sekunden für Accept | Ja |

---

### S2S_MatchAccepted (5303)

**Richtung:** 📤 Zone Server → Matchmaking Service  
**Channel:** `s2s:matchmaking`  
**Frequenz:** Häufig

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_MatchAccepted` | Ja |
| MatchId | Guid | Match-ID | Ja |
| PlayerId | long | Akzeptierender Spieler | Ja |
| Accepted | bool | Akzeptiert? | Ja |

---

### S2S_MatchCancelled (5304)

**Richtung:** 📥 Matchmaking Service → Zone Servers  
**Channel:** `s2s:server:{sourceServerId}`  
**Frequenz:** Selten

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_MatchCancelled` | Ja |
| MatchId | Guid | Match-ID | Ja |
| Reason | string | "PlayerDeclined", "Timeout" | Ja |
| AffectedPlayerIds | List<long> | Betroffene Spieler | Ja |

---

### S2S_InstanceRequest (5310)

**Richtung:** 📤 Zone/Matchmaking → Instance Pool  
**Channel:** `s2s:transfer:instance`  
**Timeout:** 2000ms

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_InstanceRequest` | Ja |
| RequestId | Guid | Request-Tracking-ID | Ja |
| InstanceType | string | "Dungeon", "Arena", "Raid" | Ja |
| TemplateId | string | Instance-Template | Ja |
| MaxPlayers | int | Max. Spielerzahl | Ja |
| RequestingServerId | string | Anfragender Server | Ja |

---

### S2S_InstanceReady (5311)

**Richtung:** 📥 Instance Pool → Requesting Server  
**Channel:** Direct Response

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_InstanceReady` | Ja |
| RequestId | Guid | Request-Tracking-ID | Ja |
| InstanceId | Guid | Instance-ID | Ja |
| InstanceServerId | string | Instance-Server | Ja |
| EntryPoint | Vector2 | Spawn-Position | Ja |
| Success | bool | Erfolg? | Ja |
| ErrorCode | string? | Error bei !Success | Conditional |

---

### S2S_InstancePlayerJoin (5312)

**Richtung:** 📤 Zone Server → Instance Server  
**Channel:** `s2s:transfer:instance`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_InstancePlayerJoin` | Ja |
| InstanceId | Guid | Instance-ID | Ja |
| PlayerId | long | Joinender Spieler | Ja |
| PlayerState | PlayerStateSnapshot | Player-State | Ja |

---

### S2S_InstancePlayerLeave (5313)

**Richtung:** 📤 Instance Server → Zone Server  
**Channel:** `s2s:transfer:zone:{returnZoneId}`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_InstancePlayerLeave` | Ja |
| InstanceId | Guid | Instance-ID | Ja |
| PlayerId | long | Leavender Spieler | Ja |
| Reason | string | "Complete", "Disconnect", "Kick" | Ja |
| ReturnZoneId | string | Return-Zone | Ja |
| PlayerState | PlayerStateSnapshot | Updated State | Ja |

---

### S2S_InstanceComplete (5314)

**Richtung:** 📤 Instance Server → Zone Servers  
**Channel:** `s2s:transfer:instance`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_InstanceComplete` | Ja |
| InstanceId | Guid | Instance-ID | Ja |
| Success | bool | Erfolgreich abgeschlossen? | Ja |
| PlayerIds | List<long> | Teilnehmer | Ja |
| CompletionTime | int | Sekunden | Ja |
| Rewards | Dictionary<long, RewardData> | Belohnungen pro Spieler | Ja |

---

## Kategorie 54: S2S Economy & Auction (5400-5499)

### S2S_AuctionCreate (5400)

**Richtung:** 📤 Zone Server → Auction Service  
**Channel:** `s2s:auction`  
**Timeout:** 1000ms  
**Retry:** 2x

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_AuctionCreate` | Ja |
| AuctionId | Guid | Eindeutige Auction-ID | Ja |
| SellerId | long | Verkäufer Player-ID | Ja |
| ItemId | int | Item-Template-ID | Ja |
| ItemInstanceId | Guid | Item-Instanz-ID | Ja |
| Quantity | int | Anzahl | Ja |
| StartingBid | long | Mindestgebot (Gold) | Ja |
| BuyoutPrice | long? | Sofortkauf-Preis | Nein |
| Duration | int | Laufzeit in Stunden | Ja |

---

### S2S_AuctionBid (5401)

**Richtung:** 📤 Zone Server → Auction Service  
**Channel:** `s2s:auction`  
**Timeout:** 1000ms

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_AuctionBid` | Ja |
| AuctionId | Guid | Auction-ID | Ja |
| BidderId | long | Bieter Player-ID | Ja |
| BidAmount | long | Gebots-Betrag | Ja |
| Timestamp | long | Unix Timestamp | Ja |

---

### S2S_AuctionBuyout (5402)

**Richtung:** 📤 Zone Server → Auction Service  
**Channel:** `s2s:auction`  
**Timeout:** 1000ms

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_AuctionBuyout` | Ja |
| AuctionId | Guid | Auction-ID | Ja |
| BuyerId | long | Käufer Player-ID | Ja |
| BuyoutPrice | long | Gezahlter Preis | Ja |
| Timestamp | long | Unix Timestamp | Ja |

---

### S2S_AuctionExpired (5403)

**Richtung:** 📥 Auction Service → Zone Servers  
**Channel:** `s2s:player:{sellerId}`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_AuctionExpired` | Ja |
| AuctionId | Guid | Auction-ID | Ja |
| SellerId | long | Verkäufer | Ja |
| ItemInstanceId | Guid | Item zurück an Verkäufer | Ja |
| HighestBid | long? | Höchstes Gebot (null = keine Bids) | Nein |

---

### S2S_AuctionSold (5404)

**Richtung:** 📥 Auction Service → Zone Servers  
**Channel:** `s2s:player:{sellerId}` und `s2s:player:{buyerId}`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_AuctionSold` | Ja |
| AuctionId | Guid | Auction-ID | Ja |
| SellerId | long | Verkäufer | Ja |
| BuyerId | long | Käufer | Ja |
| FinalPrice | long | Verkaufs-Preis | Ja |
| ItemInstanceId | Guid | Verkauftes Item | Ja |
| AuctionHouseCut | long | AH-Gebühr (5%) | Ja |

---

### S2S_GoldTransfer (5410)

**Richtung:** 📤 Zone Server → Target Zone  
**Channel:** `s2s:player:{targetPlayerId}`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_GoldTransfer` | Ja |
| TransferId | Guid | Transfer-Tracking-ID | Ja |
| SourcePlayerId | long | Absender | Ja |
| TargetPlayerId | long | Empfänger | Ja |
| Amount | long | Gold-Betrag | Ja |
| Reason | string | "Mail", "Trade", "Gift" | Ja |

---

### S2S_ItemTransfer (5411)

**Richtung:** 📤 Zone Server → Target Zone  
**Channel:** `s2s:player:{targetPlayerId}`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ItemTransfer` | Ja |
| TransferId | Guid | Transfer-Tracking-ID | Ja |
| SourcePlayerId | long | Absender | Ja |
| TargetPlayerId | long | Empfänger | Ja |
| ItemData | ItemData | Transferiertes Item | Ja |
| Reason | string | "Mail", "Trade", "Gift" | Ja |

---

## Kategorie 55: S2S Admin & Monitoring (5500-5599)

### S2S_AdminCommand (5500)

**Richtung:** 📤 Admin Tool → Target Server  
**Channel:** `s2s:admin` oder `s2s:server:{serverId}`  
**Timeout:** 5000ms

#### Beschreibung
Remote-Admin-Befehl für Server-Management.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_AdminCommand` | Ja |
| CommandId | Guid | Command-Tracking-ID | Ja |
| AdminId | long | Admin Account-ID | Ja |
| TargetServerId | string? | Ziel-Server (null = broadcast) | Nein |
| Command | string | "KICK", "BAN", "BROADCAST" | Ja |
| Parameters | Dictionary<string, string> | Command-Parameter | Ja |

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
```

---

### S2S_AdminCommandResponse (5501)

**Richtung:** 📥 Server → Admin Tool  
**Channel:** Direct Response

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_AdminCommandResponse` | Ja |
| CommandId | Guid | Command-Tracking-ID | Ja |
| Success | bool | Erfolg? | Ja |
| Result | string | Result-Message | Ja |
| ErrorCode | string? | Error bei !Success | Conditional |

---

### S2S_PlayerKick (5502)

**Richtung:** 📤 Admin → Target Zone  
**Channel:** `s2s:admin`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_PlayerKick` | Ja |
| PlayerId | long | Zu kickender Spieler | Ja |
| Reason | string | Kick-Grund | Ja |
| AdminId | long | Admin-ID | Ja |

---

### S2S_PlayerBan (5503)

**Richtung:** 📤 Admin → All Servers  
**Channel:** `s2s:server:broadcast`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_PlayerBan` | Ja |
| PlayerId | long | Zu bannender Spieler | Ja |
| Duration | int | Sekunden (0 = permanent) | Ja |
| Reason | string | Ban-Grund | Ja |
| AdminId | long | Admin-ID | Ja |

---

### S2S_PlayerMute (5504)

**Richtung:** 📤 Admin → All Servers  
**Channel:** `s2s:server:broadcast`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_PlayerMute` | Ja |
| PlayerId | long | Zu mutender Spieler | Ja |
| Duration | int | Sekunden | Ja |
| Reason | string | Mute-Grund | Ja |
| AdminId | long | Admin-ID | Ja |

---

### S2S_ServerMetrics (5510)

**Richtung:** 📤 Server → Monitoring  
**Channel:** `s2s:metrics`  
**Frequenz:** Alle 30s

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ServerMetrics` | Ja |
| ServerId | string | Server-ID | Ja |
| Timestamp | long | Unix Timestamp | Ja |
| CpuUsage | float | CPU % (0-100) | Ja |
| MemoryUsage | long | Memory in Bytes | Ja |
| NetworkIn | long | Bytes/s eingehend | Ja |
| NetworkOut | long | Bytes/s ausgehend | Ja |
| ActiveConnections | int | TCP-Connections | Ja |

---

### S2S_ZoneMetrics (5511)

**Richtung:** 📤 Zone Server → Monitoring  
**Channel:** `s2s:metrics`  
**Frequenz:** Alle 30s

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_ZoneMetrics` | Ja |
| ServerId | string | Server-ID | Ja |
| ZoneId | string | Zone-ID | Ja |
| Timestamp | long | Unix Timestamp | Ja |
| PlayerCount | int | Spieler in Zone | Ja |
| ShardCount | int | Aktive Shards | Ja |
| TickRate | float | Ticks/Second | Ja |
| AverageLatency | float | Avg. Player-Latency (ms) | Ja |

---

### S2S_AlertTrigger (5512)

**Richtung:** 📤 Server → Monitoring  
**Channel:** `s2s:alerts`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_AlertTrigger` | Ja |
| ServerId | string | Server-ID | Ja |
| AlertType | string | "HighCpu", "MemoryLeak", "SlowTick" | Ja |
| Severity | string | "Warning", "Error", "Critical" | Ja |
| Message | string | Alert-Details | Ja |
| Value | float | Metric-Wert | Ja |
| Threshold | float | Schwellwert | Ja |

---

### S2S_MaintenanceSchedule (5520)

**Richtung:** 📤 Admin → All Servers  
**Channel:** `s2s:server:broadcast`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_MaintenanceSchedule` | Ja |
| MaintenanceId | Guid | Maintenance-ID | Ja |
| ScheduledTime | long | Unix Timestamp | Ja |
| EstimatedDuration | int | Minuten | Ja |
| Message | string | Info-Message | Ja |
| AffectedServers | List<string> | Server-IDs (null = all) | Nein |

---

### S2S_MaintenanceCancel (5521)

**Richtung:** 📤 Admin → All Servers  
**Channel:** `s2s:server:broadcast`

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `S2S_MaintenanceCancel` | Ja |
| MaintenanceId | Guid | Maintenance-ID | Ja |
| Reason | string | Cancellation-Grund | Ja |

---

## 🔗 Verwandte Dokumentation

- **[SERVER_TO_SERVER.md](../../02-architecture/SERVER_TO_SERVER.md)** - Vollständige S2S-Architektur-Dokumentation
- **[REDIS.md](../../02-architecture/REDIS.md)** - Redis Pub/Sub Details
- **[SERVER_COMPONENTS.md](../../02-architecture/SERVER_COMPONENTS.md)** - Server-Architektur
- **[MESSAGES.md](../../02-architecture/MESSAGES.md)** - Message-System
- **[Connection Messages](00-connection.md)** - Client-Server Connection Messages

---

## 📊 Statistiken

- **Gesamt S2S Messages**: 70
- **Status**: Geplant (nicht im MessageType enum)
- **Kategorien**: 6 (50-55)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0  
**Maintainer**: 2DMMO Team
