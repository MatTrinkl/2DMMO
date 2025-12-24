# 🚦 Rate-Limiting System

## 2DMMO – Rate Limiting & Server Protection

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-23  
**Teil von:** [Architektur-Dokumentation](README.md)

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#übersicht)
2. [Rate-Limit Kategorien (Tiers)](#rate-limit-kategorien-tiers)
3. [Message-Type zu Tier Zuordnung](#message-type-zu-tier-zuordnung)
4. [Implementierungs-Strategien](#implementierungs-strategien)
5. [Konsequenzen bei Überschreitung](#konsequenzen-bei-überschreitung)
6. [Konfiguration](#konfiguration)
7. [Monitoring & Logging](#monitoring--logging)
8. [Vergleich mit anderen MMOs](#vergleich-mit-anderen-mmos)
9. [Best Practices](#best-practices)

---

## 🎯 Übersicht

### Was ist Rate-Limiting?

Rate-Limiting ist ein Sicherheitsmechanismus, der die Anzahl der Anfragen begrenzt, die ein Client in einem bestimmten Zeitraum an den Server senden kann. Es ist ein essentieller Bestandteil der Server-seitigen Sicherheit und Stabilität.

### Warum brauchen wir es?

Rate-Limiting schützt unsere MMO-Server vor verschiedenen Bedrohungen:

| Bedrohung | Beschreibung | Auswirkung ohne Rate-Limiting |
|-----------|--------------|-------------------------------|
| 🗨️ **Spam** | Wiederholtes Senden von Chat-Nachrichten | Störung anderer Spieler, Server-Überlastung |
| 💥 **DoS (Denial of Service)** | Absichtliche Überflutung mit Requests | Server wird unbenutzbar für legitime Spieler |
| 🎮 **Exploits** | Ausnutzung von Spielmechaniken | Unfaire Vorteile, Wirtschafts-Schädigung |
| 🤖 **Macros/Bots** | Automatisierte Spieler-Aktionen | Unfairer Wettbewerb, Gold-Farming |
| ⚡ **Resource Exhaustion** | Übermäßige Server-Resource-Nutzung | Erhöhte Kosten, schlechte Performance |

### Sicherheits-Layer

Rate-Limiting ist Teil von **Layer 5** in unserer [Security Architecture](SECURITY.md):

```
┌──────────────────────────────────────────────────────────────┐
│  LAYER 1: TRANSPORT (TLS 1.3)                                │
│  LAYER 2: AUTHENTICATION (Argon2id, Session-Tokens)          │
│  LAYER 3: AUTHORIZATION (Permissions, Zone-Check)            │
│  LAYER 4: VALIDATION (Anti-Cheat, Input Validation)          │
│  LAYER 5: RATE LIMITING ⚡ ← WIR SIND HIER                  │
└──────────────────────────────────────────────────────────────┘
```

---

## 📊 Rate-Limit Kategorien (Tiers)

Wir definieren verschiedene Tiers mit unterschiedlichen Limits, die auf die Natur der Messages abgestimmt sind:

| Tier | Limit | Burst | Beispiel-Messages | Algorithmus |
|------|-------|-------|-------------------|-------------|
| 🏃 **Movement** | 25/Sekunde | 30 | `PositionUpdate` | Token Bucket |
| ⚔️ **Combat** | 12/Sekunde | 15 | `ActionRequest`, `TargetSelect` | Token Bucket |
| 🔄 **Interaction** | 8/Sekunde | 10 | `ItemUse`, `NpcInteract`, `LootItem` | Token Bucket |
| 💬 **Social** | 4/Sekunde | 5 | `ChatMessage`, `EmoteRequest` | Sliding Window |
| 🔐 **Critical** | 5/Minute | 3 | `LoginRequest`, `CharacterCreate`, `CharacterDelete` | Sliding Window |
| 📡 **System** | 1/5 Sekunden | 1 | `Heartbeat`, `Ping` | Fixed Interval |
| 🌐 **Global Fallback** | 100/Sekunde | 120 | Alle Messages zusammen | Token Bucket |

### Tier-Beschreibungen

#### 🏃 Movement (25/s)
- **Zweck:** Position-Updates während der Bewegung
- **25 Hz = 40ms Tick-Rate** des Servers
- Burst erlaubt kurze Geschwindigkeitsspitzen (z.B. beim Reconnect)
- Client sollte nicht öfter senden als Server-Tick-Rate

#### ⚔️ Combat (12/s)
- **Zweck:** Kampf-Aktionen und Targeting
- Entspricht ~83ms zwischen Aktionen
- Verhindert unrealistisch schnelle Ability-Spam
- Burst für Combo-Systeme

#### 🔄 Interaction (8/s)
- **Zweck:** Item-Nutzung, NPC-Interaktionen
- Entspricht ~125ms zwischen Aktionen
- Realistisch für menschliche Reaktionszeit
- Verhindert Item-Duplication Exploits

#### 💬 Social (4/s)
- **Zweck:** Chat, Emotes, soziale Funktionen
- Entspricht ~250ms zwischen Messages
- Verhindert Chat-Spam
- Striktere Kontrolle (Sliding Window)

#### 🔐 Critical (5/min)
- **Zweck:** Login, Character-Management
- Sehr niedrige Rate für sensible Operationen
- Verhindert Brute-Force und Account-Enumeration
- IP-basiert zusätzlich zu Account-basiert

#### 📡 System (1/5s = 0.2/s)
- **Zweck:** Heartbeat, Connection-Health-Checks
- Fixed Interval statt Burst-fähig
- Client sollte exakt alle 5 Sekunden senden
- Abweichung → Disconnect

#### 🌐 Global Fallback (100/s)
- **Zweck:** Gesamt-Limit über alle Message-Typen
- Fängt unvorhergesehene Message-Kombinationen
- Schutz vor genereller Überlastung
- Sollte unter normalen Umständen nie erreicht werden

---

## 🗺️ Message-Type zu Tier Zuordnung

Die folgende Tabelle ordnet alle wichtigen MessageTypes ihren Rate-Limit Tiers zu:

### Connection / Authentication (0000-0099)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `LoginRequest` | 1 | 🔐 Critical | 5/min | IP + Account limit |
| `LoginResponse` | 2 | - | Unlimited | Server-Only |
| `LogoutRequest` | 3 | 🔄 Interaction | 8/s | Selten, aber muss schnell gehen |
| `Heartbeat` | 4 | 📡 System | 1/5s | Fixed Interval |
| `Disconnect` | 5 | - | Unlimited | Client kann immer disconnecten |
| `ReconnectRequest` | 6 | 🔐 Critical | 5/min | Wie Login |
| `CharacterSelect` | 9 | 🔐 Critical | 5/min | Account-kritisch |
| `CharacterCreate` | 10 | 🔐 Critical | 5/min | Account-kritisch |
| `CharacterDelete` | 11 | 🔐 Critical | 5/min | Account-kritisch |
| `CharacterListRequest` | 12 | 🔄 Interaction | 8/s | Kann mehrfach pro Session |

### Zone Events (0100-0199)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `JoinZone` | 100 | 🔄 Interaction | 8/s | Zone-Wechsel |
| `LeaveZone` | 101 | 🔄 Interaction | 8/s | Zone-Wechsel |
| `ZoneTransferRequest` | 105 | 🔄 Interaction | 8/s | Portal, Teleport |
| `PlayerJoinedZone` | 103 | - | Unlimited | Server-Only |
| `PlayerLeftZone` | 104 | - | Unlimited | Server-Only |

### Movement / Position (0200-0299)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `PositionUpdate` | 200 | 🏃 Movement | 25/s | Hauptbewegung |
| `PositionBroadcast` | 201 | - | Unlimited | Server-Only |
| `MovementCorrection` | 202 | - | Unlimited | Server-Only |
| `TeleportRequest` | 203 | 🔄 Interaction | 8/s | Admin oder Spell |
| `JumpRequest` | 206 | 🏃 Movement | 25/s | Teil der Bewegung |
| `StuckRequest` | 209 | 🔄 Interaction | 8/s | Selten |

### Combat (0300-0399)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `ActionRequest` | 300 | ⚔️ Combat | 12/s | Abilities, Angriffe |
| `ActionResult` | 301 | - | Unlimited | Server-Only |
| `DamageEvent` | 302 | - | Unlimited | Server-Only |
| `DeathEvent` | 303 | - | Unlimited | Server-Only |
| `HealEvent` | 304 | - | Unlimited | Server-Only |
| `ThreatListRequest` | 313 | 🔄 Interaction | 8/s | UI-Request |

### Chat (0400-0499)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `ChatMessage` | 400 | 💬 Social | 4/s | Genereller Chat |
| `ChatWhisper` | 402 | 💬 Social | 4/s | Private Messages |
| `ChatParty` | 404 | 💬 Social | 4/s | Party Chat |
| `ChatGuild` | 405 | 💬 Social | 4/s | Guild Chat |
| `ChatYell` | 411 | 💬 Social | 4/s | Zone-weiter Yell |
| `ChatEmote` | 413 | 💬 Social | 4/s | Text-Emotes |
| `ChatSpamWarning` | 430 | - | Unlimited | Server-Only |

### Inventory / Items (0500-0599)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `ItemPickup` | 502 | 🔄 Interaction | 8/s | Loot-Pickup |
| `ItemDrop` | 504 | 🔄 Interaction | 8/s | Item wegwerfen |
| `ItemUse` | 505 | 🔄 Interaction | 8/s | Consumables, etc. |
| `ItemMove` | 510 | 🔄 Interaction | 8/s | Inventory-Management |
| `ItemSwap` | 511 | 🔄 Interaction | 8/s | Inventory-Management |

### Targeting (1200-1299)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `TargetSelect` | 1200 | ⚔️ Combat | 12/s | Kampf-relevant |
| `TargetClear` | 1201 | ⚔️ Combat | 12/s | Kampf-relevant |
| `TabTarget` | 1214 | ⚔️ Combat | 12/s | Target-Cycling |

### NPC / Dialog / Vendor (1300-1399)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `NpcInteract` | 1300 | 🔄 Interaction | 8/s | Dialog öffnen |
| `NpcDialogChoice` | 1303 | 🔄 Interaction | 8/s | Dialog-Option wählen |
| `VendorBuy` | 1314 | 🔄 Interaction | 8/s | Item kaufen |
| `VendorSell` | 1316 | 🔄 Interaction | 8/s | Item verkaufen |

### Emotes / Animations (2200-2299)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `EmoteRequest` | 2200 | 💬 Social | 4/s | Animations-Emote |
| `DanceStart` | 2210 | 💬 Social | 4/s | Tanz-Emote |
| `SitRequest` | 2212 | 💬 Social | 4/s | Sitzen |

### System / Ping (0900-0999)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `Ping` | 900 | 📡 System | 1/5s | Latenz-Messung |
| `Pong` | 901 | - | Unlimited | Server-Only |
| `LatencyReport` | 902 | 📡 System | 1/5s | Client-Metrik |
| `RateLimitWarning` | 917 | - | Unlimited | Server-Only |

### Admin / GM Tools (2300-2399)

| MessageType | Wert | Tier | Limit | Anmerkungen |
|-------------|------|------|-------|-------------|
| `AdminCommand` | 2300 | 🔐 Critical | 5/min | GM-Commands |
| `AdminTeleport` | 2302 | 🔄 Interaction | 8/s | GM kann schnell TPen |
| `AdminKick` | 2304 | 🔐 Critical | 5/min | Moderation |
| `AdminBan` | 2305 | 🔐 Critical | 5/min | Moderation |

---

## 🔧 Implementierungs-Strategien

Wir verwenden zwei verschiedene Algorithmen je nach Tier:

### 1. Token Bucket (Movement, Combat, Interaction)

**Konzept:** 
- Ein "Eimer" mit Tokens wird über Zeit nachgefüllt
- Jede Action verbraucht 1 Token
- Erlaubt kurze Bursts (Eimer kann voll sein)
- Gut für Echtzeit-Aktionen

**C# Pseudocode:**

```csharp
public class TokenBucketRateLimiter
{
    private readonly int _capacity;           // Max Tokens im Bucket
    private readonly int _refillRate;         // Tokens pro Sekunde
    private readonly TimeSpan _refillInterval; // Wie oft nachfüllen
    
    private int _currentTokens;
    private DateTime _lastRefill;
    
    public TokenBucketRateLimiter(int capacity, int refillRate)
    {
        _capacity = capacity;              // z.B. 30 für Movement
        _refillRate = refillRate;          // z.B. 25 für Movement
        _refillInterval = TimeSpan.FromSeconds(1.0 / refillRate);
        _currentTokens = capacity;         // Starte mit vollem Bucket
        _lastRefill = DateTime.UtcNow;
    }
    
    public bool TryConsume(int tokens = 1)
    {
        Refill();
        
        if (_currentTokens >= tokens)
        {
            _currentTokens -= tokens;
            return true;  // ✅ Erlaubt
        }
        
        return false;  // ❌ Rate-Limit überschritten
    }
    
    private void Refill()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastRefill;
        
        // Berechne wie viele Tokens nachgefüllt werden
        var tokensToAdd = (int)(elapsed.TotalSeconds * _refillRate);
        
        if (tokensToAdd > 0)
        {
            _currentTokens = Math.Min(_capacity, _currentTokens + tokensToAdd);
            _lastRefill = now;
        }
    }
    
    public int GetAvailableTokens()
    {
        Refill();
        return _currentTokens;
    }
}

// Verwendung:
var movementLimiter = new TokenBucketRateLimiter(
    capacity: 30,      // Burst: 30 Messages
    refillRate: 25     // 25 Messages/Sekunde
);

// Bei jedem PositionUpdate:
if (!movementLimiter.TryConsume())
{
    // Rate-Limit überschritten
    SendRateLimitWarning(player, "Movement");
    return;
}

ProcessPositionUpdate(message);
```

**Vorteile:**
- ✅ Erlaubt realistische Bursts (z.B. nach Reconnect)
- ✅ Einfach und effizient
- ✅ Gut für variable Action-Rates

**Nachteile:**
- ⚠️ Kann missbraucht werden (Burst → Pause → Burst)

---

### 2. Sliding Window (Social, Critical)

**Konzept:**
- Zählt Requests in einem rollierenden Zeitfenster
- Striktere Kontrolle als Token Bucket
- Verhindert Burst-Missbrauch
- Gut für sensible Operations

**C# Pseudocode:**

```csharp
public class SlidingWindowRateLimiter
{
    private readonly int _maxRequests;
    private readonly TimeSpan _windowSize;
    private readonly Queue<DateTime> _requestTimestamps;
    private readonly object _lock = new object();
    
    public SlidingWindowRateLimiter(int maxRequests, TimeSpan windowSize)
    {
        _maxRequests = maxRequests;        // z.B. 4 für Social
        _windowSize = windowSize;          // z.B. 1 Sekunde
        _requestTimestamps = new Queue<DateTime>();
    }
    
    public bool TryConsume()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var windowStart = now - _windowSize;
            
            // Entferne alte Timestamps außerhalb des Fensters
            while (_requestTimestamps.Count > 0 && 
                   _requestTimestamps.Peek() < windowStart)
            {
                _requestTimestamps.Dequeue();
            }
            
            // Prüfe ob Limit erreicht
            if (_requestTimestamps.Count >= _maxRequests)
            {
                return false;  // ❌ Rate-Limit überschritten
            }
            
            // Füge neuen Timestamp hinzu
            _requestTimestamps.Enqueue(now);
            return true;  // ✅ Erlaubt
        }
    }
    
    public int GetRemainingRequests()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var windowStart = now - _windowSize;
            
            // Aufräumen
            while (_requestTimestamps.Count > 0 && 
                   _requestTimestamps.Peek() < windowStart)
            {
                _requestTimestamps.Dequeue();
            }
            
            return _maxRequests - _requestTimestamps.Count;
        }
    }
}

// Verwendung:
var chatLimiter = new SlidingWindowRateLimiter(
    maxRequests: 4,                    // 4 Messages
    windowSize: TimeSpan.FromSeconds(1) // pro Sekunde
);

// Bei jedem ChatMessage:
if (!chatLimiter.TryConsume())
{
    // Rate-Limit überschritten
    SendRateLimitWarning(player, "Chat");
    return;
}

ProcessChatMessage(message);
```

**Vorteile:**
- ✅ Sehr genaue Kontrolle
- ✅ Keine Burst-Exploits
- ✅ Fairere Verteilung über Zeit

**Nachteile:**
- ⚠️ Höherer Memory-Verbrauch (Queue von Timestamps)
- ⚠️ Etwas langsamer als Token Bucket

---

### 3. Fixed Interval (System Messages)

**Konzept:**
- Messages müssen in exaktem Interval gesendet werden
- Keine Flexibilität, keine Bursts
- Nur für Heartbeat/Ping

**C# Pseudocode:**

```csharp
public class FixedIntervalRateLimiter
{
    private readonly TimeSpan _interval;
    private DateTime? _lastRequest;
    private readonly TimeSpan _tolerance;  // 10% Toleranz für Netzwerk-Jitter
    
    public FixedIntervalRateLimiter(TimeSpan interval)
    {
        _interval = interval;              // z.B. 5 Sekunden für Heartbeat
        _tolerance = TimeSpan.FromMilliseconds(interval.TotalMilliseconds * 0.1);
    }
    
    public bool TryConsume()
    {
        var now = DateTime.UtcNow;
        
        if (_lastRequest == null)
        {
            _lastRequest = now;
            return true;  // ✅ Erster Request immer erlaubt
        }
        
        var elapsed = now - _lastRequest.Value;
        var expectedMin = _interval - _tolerance;
        var expectedMax = _interval + _tolerance;
        
        // Prüfe ob im erwarteten Interval
        if (elapsed >= expectedMin && elapsed <= expectedMax)
        {
            _lastRequest = now;
            return true;  // ✅ Im korrekten Interval
        }
        
        // Zu früh oder zu spät
        return false;  // ❌ Falsches Interval
    }
}

// Verwendung:
var heartbeatLimiter = new FixedIntervalRateLimiter(
    TimeSpan.FromSeconds(5)  // Exakt alle 5 Sekunden
);

// Bei jedem Heartbeat:
if (!heartbeatLimiter.TryConsume())
{
    // Heartbeat zu früh/spät
    Log.Warning($"Invalid heartbeat interval from player {playerId}");
    // Bei mehrfachen Verstößen: Disconnect
}
```

---

## ⚠️ Konsequenzen bei Überschreitung

Wir verwenden ein gestaffeltes System von Konsequenzen:

| Schweregrad | Bedingung | Aktion | Dauer | Log-Level |
|-------------|-----------|--------|-------|-----------|
| 💛 **Soft Limit** | 1x überschritten | Warning loggen, Message droppen | - | `WARNING` |
| 🧡 **Moderate** | 3x in 10 Sekunden | Temporäre Sperre für Message-Type | 5-30s | `WARNING` |
| 🔴 **Severe** | 10x in 1 Minute | Disconnect mit `DisconnectReason.RateLimited` | Session | `ERROR` |
| ⛔ **Extreme** | Wiederholte Severe | Temporärer IP-Ban | 15-60 Min | `FATAL` |

### Soft Limit (Erste Überschreitung)

```csharp
if (!rateLimiter.TryConsume())
{
    // 1. Warnung loggen
    _logger.LogWarning(
        "Player {PlayerId} exceeded rate limit for {MessageType}. " +
        "Remaining: {Remaining}, Limit: {Limit}",
        playerId, messageType, rateLimiter.GetAvailableTokens(), limit
    );
    
    // 2. Message droppen (NICHT verarbeiten)
    return;
    
    // 3. Zähler erhöhen
    _violationTracker.RecordViolation(playerId, messageType);
}
```

**Wichtig:** 
- ❌ **KEINE** automatische Message an Client (sonst Spam)
- ✅ Nur Server-seitiges Logging
- ✅ Client merkt, dass Message nicht verarbeitet wurde

---

### Moderate (3x in 10 Sekunden)

```csharp
var violations = _violationTracker.GetRecentViolations(
    playerId, 
    TimeSpan.FromSeconds(10)
);

if (violations >= 3)
{
    // 1. Temporäre Sperre für diesen Message-Type
    var blockDuration = CalculateBlockDuration(violations); // 5-30s
    
    _rateLimitBlocker.BlockMessageType(
        playerId, 
        messageType, 
        blockDuration
    );
    
    // 2. Warnung an Client senden
    SendRateLimitWarning(playerId, new RateLimitWarning
    {
        Type = MessageType.RateLimitWarning,
        MessageTypeBlocked = messageType,
        Duration = blockDuration,
        Reason = "Too many requests"
    });
    
    // 3. Logging
    _logger.LogWarning(
        "Player {PlayerId} blocked for {MessageType} for {Duration}s " +
        "due to {Violations} violations",
        playerId, messageType, blockDuration.TotalSeconds, violations
    );
}

private TimeSpan CalculateBlockDuration(int violations)
{
    // Exponentielles Backoff
    return violations switch
    {
        3 => TimeSpan.FromSeconds(5),
        4 => TimeSpan.FromSeconds(10),
        5 => TimeSpan.FromSeconds(15),
        6 => TimeSpan.FromSeconds(30),
        _ => TimeSpan.FromSeconds(30)
    };
}
```

---

### Severe (10x in 1 Minute)

```csharp
var violations = _violationTracker.GetRecentViolations(
    playerId, 
    TimeSpan.FromMinutes(1)
);

if (violations >= 10)
{
    // 1. Disconnect mit Grund
    await DisconnectPlayer(playerId, new Disconnect
    {
        Type = MessageType.Disconnect,
        Reason = DisconnectReason.RateLimited,
        Message = "Rate limit exceeded. Please try again later."
    });
    
    // 2. Session invalidieren
    await _sessionService.InvalidateSession(playerId);
    
    // 3. Logging
    _logger.LogError(
        "Player {PlayerId} disconnected due to {Violations} rate limit violations",
        playerId, violations
    );
    
    // 4. Anti-Cheat System benachrichtigen
    await _antiCheatService.ReportSuspiciousActivity(
        playerId,
        ActivityType.RateLimitAbuse,
        violations
    );
}
```

---

### Extreme (Wiederholte Severe)

```csharp
var recentDisconnects = _violationTracker.GetDisconnectCount(
    ipAddress,
    TimeSpan.FromHours(1)
);

if (recentDisconnects >= 3)
{
    // 1. IP-Ban
    var banDuration = CalculateIpBanDuration(recentDisconnects); // 15-60 min
    
    await _ipBanService.BanIp(ipAddress, new IpBan
    {
        IpAddress = ipAddress,
        Duration = banDuration,
        Reason = "Repeated rate limit violations",
        BannedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow + banDuration
    });
    
    // 2. Logging
    _logger.LogCritical(
        "IP {IpAddress} banned for {Duration} minutes due to {Disconnects} " +
        "rate-limit disconnects",
        ipAddress, banDuration.TotalMinutes, recentDisconnects
    );
    
    // 3. Alert Admins
    await _alertService.SendAdminAlert(
        AlertLevel.Critical,
        $"IP {ipAddress} auto-banned for rate limit abuse"
    );
}

private TimeSpan CalculateIpBanDuration(int disconnectCount)
{
    return disconnectCount switch
    {
        3 => TimeSpan.FromMinutes(15),
        4 => TimeSpan.FromMinutes(30),
        5 => TimeSpan.FromMinutes(45),
        _ => TimeSpan.FromMinutes(60)
    };
}
```

---

## ⚙️ Konfiguration

Rate-Limits sollten **konfigurierbar** sein, um auf verschiedene Szenarien reagieren zu können:

### appsettings.json Beispiel

```json
{
  "RateLimiting": {
    "Enabled": true,
    "GlobalLimit": {
      "MaxRequestsPerSecond": 100,
      "BurstCapacity": 120,
      "Algorithm": "TokenBucket"
    },
    "Tiers": {
      "Movement": {
        "MaxRequestsPerSecond": 25,
        "BurstCapacity": 30,
        "Algorithm": "TokenBucket",
        "Enabled": true
      },
      "Combat": {
        "MaxRequestsPerSecond": 12,
        "BurstCapacity": 15,
        "Algorithm": "TokenBucket",
        "Enabled": true
      },
      "Interaction": {
        "MaxRequestsPerSecond": 8,
        "BurstCapacity": 10,
        "Algorithm": "TokenBucket",
        "Enabled": true
      },
      "Social": {
        "MaxRequestsPerSecond": 4,
        "BurstCapacity": 5,
        "Algorithm": "SlidingWindow",
        "WindowSizeSeconds": 1,
        "Enabled": true
      },
      "Critical": {
        "MaxRequestsPerMinute": 5,
        "BurstCapacity": 3,
        "Algorithm": "SlidingWindow",
        "WindowSizeSeconds": 60,
        "Enabled": true
      },
      "System": {
        "IntervalSeconds": 5,
        "TolerancePercent": 10,
        "Algorithm": "FixedInterval",
        "Enabled": true
      }
    },
    "Violations": {
      "SoftLimit": {
        "Action": "LogAndDrop",
        "LogLevel": "Warning"
      },
      "Moderate": {
        "ThresholdCount": 3,
        "ThresholdWindowSeconds": 10,
        "Action": "TemporaryBlock",
        "BlockDurationSeconds": [5, 10, 15, 30],
        "SendClientWarning": true
      },
      "Severe": {
        "ThresholdCount": 10,
        "ThresholdWindowSeconds": 60,
        "Action": "Disconnect",
        "DisconnectReason": "RateLimited",
        "NotifyAntiCheat": true
      },
      "Extreme": {
        "ThresholdCount": 3,
        "ThresholdWindowHours": 1,
        "Action": "IpBan",
        "BanDurationMinutes": [15, 30, 45, 60],
        "AlertAdmins": true
      }
    },
    "Monitoring": {
      "LogViolations": true,
      "MetricsEnabled": true,
      "MetricsIntervalSeconds": 60,
      "AlertOnHighViolationRate": true,
      "ViolationRateThreshold": 0.05
    }
  }
}
```

### C# Configuration Model

```csharp
public class RateLimitingOptions
{
    public bool Enabled { get; set; } = true;
    public GlobalLimitConfig GlobalLimit { get; set; }
    public Dictionary<string, TierConfig> Tiers { get; set; }
    public ViolationsConfig Violations { get; set; }
    public MonitoringConfig Monitoring { get; set; }
}

public class TierConfig
{
    public int MaxRequestsPerSecond { get; set; }
    public int MaxRequestsPerMinute { get; set; }
    public int BurstCapacity { get; set; }
    public string Algorithm { get; set; }  // "TokenBucket", "SlidingWindow", "FixedInterval"
    public int WindowSizeSeconds { get; set; }
    public int IntervalSeconds { get; set; }
    public int TolerancePercent { get; set; }
    public bool Enabled { get; set; } = true;
}

// Verwendung in Startup.cs / Program.cs:
services.Configure<RateLimitingOptions>(
    configuration.GetSection("RateLimiting")
);

services.AddSingleton<IRateLimitingService, RateLimitingService>();
```

### Environment-Specific Overrides

```json
// appsettings.Development.json
{
  "RateLimiting": {
    "Enabled": false  // ← Deaktiviert für lokale Entwicklung
  }
}

// appsettings.Staging.json
{
  "RateLimiting": {
    "Tiers": {
      "Movement": {
        "MaxRequestsPerSecond": 50  // ← Höhere Limits für Tests
      }
    },
    "Violations": {
      "Severe": {
        "Action": "LogOnly"  // ← Kein Disconnect in Staging
      }
    }
  }
}

// appsettings.Production.json
{
  "RateLimiting": {
    "Enabled": true,
    "Violations": {
      "Extreme": {
        "AlertAdmins": true,
        "BanDurationMinutes": [30, 60, 120, 240]  // ← Härtere Strafen
      }
    }
  }
}
```

---

## 📊 Monitoring & Logging

### Was sollte geloggt werden?

#### 1. Violations (Verstöße)

```csharp
_logger.LogWarning(
    "RateLimit Violation: Player={PlayerId} IP={IpAddress} MessageType={MessageType} " +
    "Tier={Tier} Limit={Limit} Actual={Actual} Remaining={Remaining}",
    playerId, ipAddress, messageType, tier, limit, actual, remaining
);
```

**Felder:**
- `PlayerId` - Welcher Spieler
- `IpAddress` - Welche IP (für IP-Ban Tracking)
- `MessageType` - Welcher Message-Type
- `Tier` - Welches Tier (Movement, Combat, etc.)
- `Limit` - Konfiguriertes Limit
- `Actual` - Tatsächliche Rate
- `Remaining` - Verbleibende Tokens/Requests

#### 2. Actions (Aktionen)

```csharp
_logger.LogError(
    "RateLimit Action: Player={PlayerId} Action={Action} MessageType={MessageType} " +
    "Duration={Duration} ViolationCount={ViolationCount}",
    playerId, action, messageType, duration, violationCount
);
```

**Actions:**
- `MessageDropped` - Soft Limit
- `TemporaryBlock` - Moderate
- `Disconnect` - Severe
- `IpBan` - Extreme

#### 3. Metrics (Metriken)

```csharp
// Periodisch (z.B. alle 60 Sekunden)
_logger.LogInformation(
    "RateLimit Metrics: " +
    "TotalRequests={TotalRequests} " +
    "TotalViolations={TotalViolations} " +
    "ViolationRate={ViolationRate:P2} " +
    "ActiveBlocks={ActiveBlocks} " +
    "ActiveBans={ActiveBans}",
    totalRequests, totalViolations, violationRate, activeBlocks, activeBans
);
```

---

### Metriken für Dashboards

Folgende Metriken sollten für Monitoring-Dashboards (Prometheus, Grafana, etc.) verfügbar sein:

#### Counters (Zähler)

```csharp
// Total Requests pro Message-Type
Metrics.Counter("ratelimit_requests_total", 
    "Total rate limit checks",
    new[] { "message_type", "tier", "result" }  // Labels
);

// Total Violations
Metrics.Counter("ratelimit_violations_total",
    "Total rate limit violations",
    new[] { "message_type", "tier", "severity" }
);

// Total Actions
Metrics.Counter("ratelimit_actions_total",
    "Total rate limit actions taken",
    new[] { "action_type" }  // "drop", "block", "disconnect", "ban"
);
```

#### Gauges (Momentaufnahmen)

```csharp
// Aktuelle Blocks
Metrics.Gauge("ratelimit_active_blocks",
    "Currently active rate limit blocks"
);

// Aktuelle IP-Bans
Metrics.Gauge("ratelimit_active_bans",
    "Currently active IP bans"
);

// Violation Rate
Metrics.Gauge("ratelimit_violation_rate",
    "Current rate limit violation rate (0.0-1.0)"
);
```

#### Histograms (Verteilungen)

```csharp
// Request Rate Verteilung
Metrics.Histogram("ratelimit_request_rate",
    "Distribution of request rates",
    new[] { "message_type", "tier" }
);

// Block Duration Verteilung
Metrics.Histogram("ratelimit_block_duration_seconds",
    "Distribution of block durations"
);
```

---

### Alerting bei Anomalien

Folgende Bedingungen sollten Alerts auslösen:

#### 🚨 Critical Alerts

```
Alert: High Violation Rate
Condition: violation_rate > 0.05 (5%) for 5 minutes
Action: Page On-Call Engineer
```

```
Alert: Mass IP Bans
Condition: active_bans > 100
Action: Page On-Call Engineer
```

```
Alert: Severe Violations Spike
Condition: severe_violations > 50 in 1 minute
Action: Send Slack/Email Alert
```

#### ⚠️ Warning Alerts

```
Alert: Elevated Violation Rate
Condition: violation_rate > 0.02 (2%) for 10 minutes
Action: Send Slack/Email Alert
```

```
Alert: Many Moderate Blocks
Condition: moderate_blocks > 200 in 5 minutes
Action: Send Slack Alert
```

---

### Grafana Dashboard Beispiel

```json
{
  "dashboard": {
    "title": "Rate Limiting Overview",
    "panels": [
      {
        "title": "Requests per Second by Tier",
        "targets": [
          {
            "expr": "rate(ratelimit_requests_total[1m])",
            "legendFormat": "{{tier}}"
          }
        ]
      },
      {
        "title": "Violation Rate",
        "targets": [
          {
            "expr": "rate(ratelimit_violations_total[5m]) / rate(ratelimit_requests_total[5m])",
            "legendFormat": "Violation Rate"
          }
        ],
        "alert": {
          "conditions": [
            {
              "evaluator": { "params": [0.05], "type": "gt" },
              "query": { "params": ["A", "5m", "now"] }
            }
          ]
        }
      },
      {
        "title": "Actions Taken",
        "targets": [
          {
            "expr": "rate(ratelimit_actions_total[5m])",
            "legendFormat": "{{action_type}}"
          }
        ]
      },
      {
        "title": "Active Blocks & Bans",
        "targets": [
          {
            "expr": "ratelimit_active_blocks",
            "legendFormat": "Blocks"
          },
          {
            "expr": "ratelimit_active_bans",
            "legendFormat": "IP Bans"
          }
        ]
      }
    ]
  }
}
```

---

## 🎮 Vergleich mit anderen MMOs

Hier ein Vergleich unserer Rate-Limits mit etablierten MMOs:

| Feature | 2DMMO | WoW (Classic) | FFXIV | GW2 | Anmerkungen |
|---------|-------|---------------|-------|-----|-------------|
| **Movement Updates** | 25/s | 20/s | 30/s | 25/s | Wir: 40ms Tick-Rate |
| **Combat Actions** | 12/s | 10-15/s | 5-10/s | 10/s | FFXIV hat GCD-System |
| **Chat Messages** | 4/s | 3/s | 5/s | 10/s | Wir: Stricter für Spam-Schutz |
| **Item Interactions** | 8/s | 10/s | 5/s | Unlimited | GW2: Sehr permissiv |
| **Login Attempts** | 5/min | 3/min | 5/min | 10/min | Brute-Force Schutz |
| **Global Limit** | 100/s | 80/s | 120/s | 150/s | Gesamt über alle Types |

### Erkenntnisse

#### World of Warcraft
- ✅ Sehr strenge Limits (seit 2004 erprobt)
- ✅ Effektiver Bot-Schutz
- ⚠️ Manchmal zu restriktiv (Spieler-Beschwerden)

#### Final Fantasy XIV
- ✅ Höhere Movement-Rate (flüssigere Bewegung)
- ✅ Niedrige Combat-Rate wegen GCD-System
- ⚠️ Weniger Schutz gegen Spam-Bots

#### Guild Wars 2
- ✅ Sehr permissive Limits (gute UX)
- ⚠️ Anfällig für Bot-Probleme
- ⚠️ Mehr Server-Last

### Unsere Strategie

Wir kombinieren das Beste aus allen Welten:

1. **WoW-ähnlich** für Critical Operations (Login, Character Management)
2. **FFXIV-ähnlich** für Movement (Balance zwischen Fluidität und Schutz)
3. **Eigener Ansatz** für Combat (12/s ermöglicht schnelle Combos)
4. **Stricter als GW2** um Server-Kosten zu kontrollieren

---

## 💡 Best Practices

### 1. Pro-Connection vs Pro-Account Limits

**Empfehlung:** Kombiniere beide Ansätze!

```csharp
public class RateLimitingService
{
    // Pro Connection (für Movement, Combat)
    private readonly Dictionary<Guid, TokenBucketRateLimiter> _perConnectionLimiters;
    
    // Pro Account (für Critical Operations)
    private readonly Dictionary<int, SlidingWindowRateLimiter> _perAccountLimiters;
    
    // Pro IP (für Login Attempts)
    private readonly Dictionary<string, SlidingWindowRateLimiter> _perIpLimiters;
    
    public bool CheckRateLimit(
        Guid sessionId,
        int? accountId,
        string ipAddress,
        MessageType messageType)
    {
        var tier = GetTierForMessageType(messageType);
        
        switch (tier)
        {
            case RateLimitTier.Movement:
            case RateLimitTier.Combat:
            case RateLimitTier.Interaction:
                // Pro Connection
                return CheckConnectionLimit(sessionId, messageType);
                
            case RateLimitTier.Critical:
                // Pro Account UND Pro IP
                return CheckAccountLimit(accountId.Value, messageType) &&
                       CheckIpLimit(ipAddress, messageType);
                
            case RateLimitTier.Social:
                // Pro Account (verhindert Multi-Accounting Spam)
                return CheckAccountLimit(accountId.Value, messageType);
                
            default:
                return true;
        }
    }
}
```

**Vorteile:**
- ✅ Movement/Combat: Pro-Connection ist fair und effizient
- ✅ Critical: Pro-Account + Pro-IP verhindert Multi-Account Attacks
- ✅ Social: Pro-Account verhindert Spam von Zweit-Chars

---

### 2. Graceful Degradation

Bei hoher Server-Last sollten Rate-Limits **dynamisch angepasst** werden:

```csharp
public class AdaptiveRateLimitingService
{
    private readonly IServerLoadMonitor _loadMonitor;
    
    public RateLimitTier GetEffectiveTier(MessageType messageType)
    {
        var baseTier = GetBaseTierForMessageType(messageType);
        var serverLoad = _loadMonitor.GetCurrentLoad();
        
        // Bei hoher Last: Strengere Limits
        if (serverLoad > 0.8)  // > 80% Load
        {
            return baseTier switch
            {
                RateLimitTier.Movement => new RateLimitTier
                {
                    MaxRequestsPerSecond = 15,  // Reduziert von 25
                    BurstCapacity = 18          // Reduziert von 30
                },
                RateLimitTier.Combat => new RateLimitTier
                {
                    MaxRequestsPerSecond = 8,   // Reduziert von 12
                    BurstCapacity = 10          // Reduziert von 15
                },
                _ => baseTier
            };
        }
        
        return baseTier;
    }
}
```

**Wichtig:**
- 📢 Benachrichtige Spieler über reduzierte Limits
- 📊 Logge Degradation-Events
- ⚡ Auto-Scaling sollte zusätzlich greifen

---

### 3. Feedback an Client (RateLimitWarning Message)

Spieler sollten **proaktiv** über Rate-Limits informiert werden:

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RateLimitWarning)]
public class RateLimitWarning : INetworkMessage
{
    [Key(0)]
    public MessageType Type => MessageType.RateLimitWarning;
    
    [Key(1)]
    public MessageType BlockedMessageType { get; set; }
    
    [Key(2)]
    public RateLimitSeverity Severity { get; set; }  // Soft, Moderate, Severe
    
    [Key(3)]
    public int RemainingRequests { get; set; }
    
    [Key(4)]
    public TimeSpan? BlockDuration { get; set; }  // Null für Soft
    
    [Key(5)]
    public string Message { get; set; }
}

public enum RateLimitSeverity : byte
{
    Soft = 1,      // Nur Info, keine Sperre
    Moderate = 2,  // Temporäre Sperre
    Severe = 3     // Disconnect
}
```

**Client-Side Handling:**

```csharp
// Godot C# Client
public void OnRateLimitWarning(RateLimitWarning warning)
{
    switch (warning.Severity)
    {
        case RateLimitSeverity.Soft:
            // UI: Kleiner Hinweis (Toast)
            ShowToast($"Slow down! ({warning.BlockedMessageType})");
            break;
            
        case RateLimitSeverity.Moderate:
            // UI: Deutliche Warnung + Timer
            ShowWarningDialog(
                "Rate Limit Exceeded",
                $"You are sending too many {warning.BlockedMessageType} messages. " +
                $"You are blocked for {warning.BlockDuration?.TotalSeconds:F0} seconds."
            );
            StartCooldownTimer(warning.BlockDuration.Value);
            break;
            
        case RateLimitSeverity.Severe:
            // UI: Disconnect Screen
            ShowDisconnectScreen(
                "Disconnected: Rate Limit",
                warning.Message
            );
            break;
    }
}
```

---

### 4. Whitelist für Admins/GMs

Admins sollten **höhere Limits** oder **keine Limits** haben:

```csharp
public class RateLimitingService
{
    private readonly IPermissionService _permissionService;
    
    public bool CheckRateLimit(
        int accountId,
        Guid sessionId,
        MessageType messageType)
    {
        // Admins/GMs haben keine Rate-Limits
        if (_permissionService.HasPermission(accountId, "rate_limit_exempt"))
        {
            return true;  // ✅ Immer erlaubt
        }
        
        // Normale Spieler: Rate-Limit prüfen
        return CheckNormalRateLimit(sessionId, messageType);
    }
}
```

**Aber:** Auch Admin-Aktionen sollten geloggt werden!

---

### 5. Testing & Load Testing

Rate-Limits müssen **getestet** werden:

```csharp
[Test]
public async Task MovementRateLimit_Should_Allow_25_Per_Second()
{
    var limiter = new TokenBucketRateLimiter(capacity: 30, refillRate: 25);
    
    // Simuliere 25 Requests in 1 Sekunde
    for (int i = 0; i < 25; i++)
    {
        Assert.IsTrue(limiter.TryConsume(), $"Request {i+1} should be allowed");
        await Task.Delay(40);  // 40ms zwischen Requests (25 Hz)
    }
}

[Test]
public async Task MovementRateLimit_Should_Block_After_Burst()
{
    var limiter = new TokenBucketRateLimiter(capacity: 30, refillRate: 25);
    
    // Verbrauche alle 30 Tokens (Burst)
    for (int i = 0; i < 30; i++)
    {
        Assert.IsTrue(limiter.TryConsume());
    }
    
    // 31. Request sollte geblockt werden
    Assert.IsFalse(limiter.TryConsume());
    
    // Nach 1 Sekunde: 25 Tokens nachgefüllt
    await Task.Delay(1000);
    
    for (int i = 0; i < 25; i++)
    {
        Assert.IsTrue(limiter.TryConsume());
    }
}
```

**Load Testing:**

```bash
# Apache JMeter oder k6 für Load Tests
k6 run --vus 1000 --duration 60s rate_limit_test.js
```

---

### 6. Redis für Distributed Rate-Limiting

Für **multi-server Setups** benötigen wir verteiltes Rate-Limiting:

```csharp
public class RedisRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task<bool> TryConsumeAsync(
        string key,
        int maxRequests,
        TimeSpan window)
    {
        var db = _redis.GetDatabase();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowStart = now - (long)window.TotalMilliseconds;
        
        // Lua Script für atomare Operation
        var script = @"
            local key = KEYS[1]
            local now = tonumber(ARGV[1])
            local window_start = tonumber(ARGV[2])
            local max_requests = tonumber(ARGV[3])
            
            -- Entferne alte Einträge
            redis.call('ZREMRANGEBYSCORE', key, 0, window_start)
            
            -- Zähle aktuelle Requests
            local current = redis.call('ZCARD', key)
            
            if current < max_requests then
                -- Füge neuen Request hinzu
                redis.call('ZADD', key, now, now)
                redis.call('EXPIRE', key, 60)
                return 1
            else
                return 0
            end
        ";
        
        var result = await db.ScriptEvaluateAsync(
            script,
            keys: new RedisKey[] { key },
            values: new RedisValue[] { now, windowStart, maxRequests }
        );
        
        return (int)result == 1;
    }
}

// Verwendung:
var allowed = await _redisRateLimiter.TryConsumeAsync(
    key: $"ratelimit:player:{playerId}:movement",
    maxRequests: 25,
    window: TimeSpan.FromSeconds(1)
);
```

**Vorteile:**
- ✅ Funktioniert über mehrere Gateway-Server
- ✅ Atomare Operationen via Lua
- ✅ Automatisches Cleanup alter Daten

**Siehe auch:** [Redis-Strategie](REDIS.md) für weitere Details zu Redis-Nutzung.

---

## 🔗 Verwandte Dokumentation

- [Network Protocol](NETWORK_PROTOCOL.md) - Message Framing & Transport
- [Messages](MESSAGES.md) - MessageType Enum & DTOs
- [Security](SECURITY.md) - Gesamte Security Architecture
- [Handler/Service Pattern](HANDLER_SERVICE_PATTERN.md) - Message Processing
- [Redis](REDIS.md) - Distributed Rate-Limiting mit Redis
- [Game Loop](GAME_LOOP.md) - Server Tick Rate (wichtig für Movement Limits)

---

## 📝 Changelog

| Version | Datum | Änderungen |
|---------|-------|------------|
| 1.0.0 | 2025-12-23 | Initiale Dokumentation mit allen Tiers, Algorithmen und Best Practices |

---

*Diese Dokumentation ist Teil der 2DMMO Architektur. Bei Fragen oder Verbesserungsvorschlägen bitte ein Issue erstellen.*
