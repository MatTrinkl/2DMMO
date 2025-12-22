# 🔄 Game Loop Design

## 2DMMO – Server Game Loop

**Version:** 1.3.0  
**Letzte Aktualisierung:** 2025-12-22  
**Teil von:** [Architektur-Dokumentation](README.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt das Game Loop Design für den 2DMMO Server, inklusive Tick Timing und Phasen-Verarbeitung.

---

## Server Game Loop (25 Hz)

```
┌─────────────────────────────────────────────────────────┐
│                  SERVER GAME LOOP                        │
│                   (40ms pro Tick)                    │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │                    TICK START                       │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  0. ASYNC COMPLETION PHASE                     │ │ │
│  │  │     • Async Task Callbacks abarbeiten         │ │ │
│  │  │     • Mit frischem MessageContext             │ │ │
│  │  │     • Im Game Loop Thread                     │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  1. INPUT PHASE                                │ │ │
│  │  │     • Alle Messages aus Input-Queue lesen     │ │ │
│  │  │     • Nach Timestamp sortieren                │ │ │
│  │  │     • Duplikate entfernen                     │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  2. VALIDATION PHASE                           │ │ │
│  │  │     • Movement validieren (Speed-Check)       │ │ │
│  │  │     • Action validieren (Range, Cooldown)     │ │ │
│  │  │     • Cheater-Detection                       │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  3. SIMULATION PHASE                           │ │ │
│  │  │     • Positionen updaten                      │ │ │
│  │  │     • Kampf-Berechnungen                      │ │ │
│  │  │     • AI/NPC Updates                          │ │ │
│  │  │     • Respawn-Checks                          │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  4. BROADCAST PHASE                            │ │ │
│  │  │     • Position-Broadcasts sammeln             │ │ │
│  │  │     • Event-Broadcasts sammeln                │ │ │
│  │  │     • An relevante Clients senden             │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  5. PERSISTENCE PHASE (alle N Ticks)           │ │ │
│  │  │     • Dirty-Flags checken                     │ │ │
│  │  │     • Änderungen in Redis pushen              │ │ │
│  │  │     • Periodisch in PostgreSQL speichern      │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │                    TICK END                         │ │
│  │              (Sleep bis nächster Tick)              │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

---

## Tick Timing

```csharp
public class GameLoop
{
    private const int TICK_RATE = 25;  // Hz
    private const double TICK_INTERVAL_MS = 1000.0 / TICK_RATE;  // 40ms
    
    private readonly Stopwatch _tickTimer = new();
    private long _currentTick = 0;
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _tickTimer.Restart();
            
            // ═══ TICK LOGIC ═══
            ProcessInputs();
            ValidateActions();
            SimulateWorld();
            BroadcastUpdates();
            
            if (_currentTick % 30 == 0)  // Jede Sekunde
            {
                PersistToRedis();
            }
            
            if (_currentTick % 300 == 0)  // Alle 10 Sekunden
            {
                PersistToDatabase();
            }
            
            _currentTick++;
            // ═══ END TICK ═══
            
            // Sleep für verbleibende Zeit
            double elapsed = _tickTimer.Elapsed.TotalMilliseconds;
            double sleepTime = TICK_INTERVAL_MS - elapsed;
            
            if (sleepTime > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(sleepTime), cancellationToken);
            }
            else
            {
                // Tick dauerte zu lange! Logging/Warnung
                LogTickOverrun(elapsed);
            }
        }
    }
}
```

---

## Message Queues

Das Game Loop verwendet zwei zentrale Queues für die Thread-sichere Kommunikation zwischen NetworkServer und GameServer:

### Incoming Messages Queue

```csharp
private readonly ConcurrentQueue<IncomingMessage> _incomingMessages = new();

public struct IncomingMessage
{
    public Guid ConnectionId;
    public INetworkMessage Message;
    public long Timestamp;
}
```

**Zweck:** Sammelt alle eingehenden Nachrichten von Clients zwischen Ticks.

**Flow:**
1. NetworkServer empfängt Nachricht von Client
2. NetworkServer enqueued in `_incomingMessages`
3. GameServer dequeued in **Input Phase** des Ticks
4. Nachrichten werden nach Timestamp sortiert für deterministische Verarbeitung

### Pending Broadcasts Queue

```csharp
private readonly ConcurrentQueue<BroadcastMessage> _pendingBroadcasts = new();

public struct BroadcastMessage
{
    public INetworkMessage Message;
    public List<Guid> Recipients;  // null = alle Spieler in Zone
    public BroadcastPriority Priority;
}

public enum BroadcastPriority : byte
{
    Low = 0,      // Kann batched werden
    Normal = 1,   // Standard
    High = 2,     // Sofort senden (z.B. Disconnect)
}
```

**Zweck:** Sammelt alle ausgehenden Nachrichten während des Ticks.

**Flow:**
1. GameServer erstellt Broadcast (z.B. PositionUpdate)
2. GameServer enqueued in `_pendingBroadcasts`
3. NetworkServer dequeued und sendet an Clients
4. Queue wird am Ende jedes Ticks geleert

---

## Broadcast-Strategie

Das System verwendet drei verschiedene Broadcast-Typen für optimale Netzwerk-Performance:

### 1. Event Broadcasts (Sofort)

**Wann:** Bei wichtigen Events die nicht verzögert werden dürfen.

**Beispiele:**
- `PlayerJoinedZone` - Neuer Spieler ist beigetreten
- `PlayerLeftZone` - Spieler hat Zone verlassen
- `ChatMessage` - Chat-Nachricht
- `DamageEvent` - Schaden wurde verursacht
- `DeathEvent` - Entity ist gestorben

**Implementierung:**
```csharp
// Sofort in Broadcast-Queue
public void BroadcastPlayerJoined(PlayerEntity player)
{
    var message = new PlayerJoinedZone(player);
    _pendingBroadcasts.Enqueue(new BroadcastMessage
    {
        Message = message,
        Recipients = null,  // Alle in Zone
        Priority = BroadcastPriority.High
    });
}
```

### 2. Delta Updates (Nur geänderte Entities, jeden Tick)

**Wann:** Jeden Tick werden nur Entities mit Änderungen gesendet.

**Wie:** Dirty-Tracking System markiert geänderte Entities.

**Beispiele:**
- Position geändert → `PositionBroadcast`
- Health geändert → `HealthUpdate`
- Velocity geändert → `VelocityUpdate`

**Implementierung:**
```csharp
// Output Phase - Dirty Entities
protected virtual Task OutputPhaseAsync(CancellationToken cancellationToken)
{
    var dirtyEntities = _zoneManager.GetDirtyEntities();
    
    if (dirtyEntities.Any())
    {
        var deltaUpdate = new DeltaStateUpdate(dirtyEntities);
        _pendingBroadcasts.Enqueue(new BroadcastMessage
        {
            Message = deltaUpdate,
            Recipients = null,
            Priority = BroadcastPriority.Normal
        });
    }
    
    // Dirty Flags zurücksetzen
    _zoneManager.ClearDirtyFlags();
    
    return Task.CompletedTask;
}
```

### 3. Full ZoneState (Alle Entities, alle 25 Ticks = 1 Sekunde)

**Wann:** Periodisch als Fallback und für neu verbundene Clients.

**Warum:**
- ✅ Garantiert Synchronisation (falls Delta-Updates verloren gehen)
- ✅ Kein komplexes State-Tracking auf Client nötig
- ✅ Neue Spieler erhalten sofort kompletten State

**Implementierung:**
```csharp
// Output Phase
protected virtual Task OutputPhaseAsync(CancellationToken cancellationToken)
{
    // Delta Updates (siehe oben)
    // ...
    
    // Full Sync alle 1 Sekunde (25 Ticks)
    if (CurrentTick % 25 == 0)
    {
        var allEntities = _zoneManager.GetAllEntities();
        var fullState = new ZoneState(
            timestamp: CurrentTick,
            zoneId: _zoneManager.CurrentZoneId,
            entities: allEntities
        );
        
        _pendingBroadcasts.Enqueue(new BroadcastMessage
        {
            Message = fullState,
            Recipients = null,
            Priority = BroadcastPriority.Normal
        });
    }
    
    return Task.CompletedTask;
}
```

**Frequenz-Tabelle:**

| Broadcast-Typ | Frequenz | Bandbreite | Use Case |
|---------------|----------|------------|----------|
| Event Broadcasts | Bei Bedarf | Niedrig | Wichtige Ereignisse |
| Delta Updates | 25 Hz (40ms) | Mittel | Laufende Änderungen |
| Full ZoneState | 1 Hz (1s) | Hoch | Sync-Garantie |

---

## Dirty Tracking

Das Dirty-Tracking System markiert Entities mit Änderungen, um unnötige Broadcasts zu vermeiden.

### Konzept

```csharp
public class Entity
{
    private bool _isDirty = false;
    
    private float _x;
    public float X
    {
        get => _x;
        set
        {
            if (Math.Abs(_x - value) > 0.001f)
            {
                _x = value;
                MarkDirty();
            }
        }
    }
    
    public void MarkDirty()
    {
        _isDirty = true;
    }
    
    public void ClearDirty()
    {
        _isDirty = false;
    }
    
    public bool IsDirty => _isDirty;
}
```

### ZoneManager Integration

```csharp
public class ZoneManager
{
    private readonly Dictionary<EntityIdentity, Entity> _entities = new();
    
    public List<Entity> GetDirtyEntities()
    {
        return _entities.Values
            .Where(e => e.IsDirty)
            .ToList();
    }
    
    public void ClearDirtyFlags()
    {
        foreach (var entity in _entities.Values)
        {
            entity.ClearDirty();
        }
    }
}
```

### Vorteile

- ✅ **Bandbreiten-Optimierung:** Nur geänderte Entities werden gesendet
- ✅ **Performance:** Keine unnötigen Serialisierungen
- ✅ **Skalierbarkeit:** Funktioniert auch mit vielen Entities

### Beispiel-Szenario

```
Tick 0:  Player1 bewegt sich → Dirty = true
         → Delta Update mit Player1 wird gesendet
         → Dirty = false

Tick 1:  Keine Bewegung → Dirty = false
         → Kein Delta Update

Tick 2:  Player1 bewegt sich → Dirty = true
         → Delta Update mit Player1 wird gesendet
         
Tick 25: Full ZoneState mit ALLEN Entities (unabhängig von Dirty)
```

---

## ⚡ Async Task Completion

### Übersicht

Handler sind **synchron** (siehe [Handler/Service-Pattern](HANDLER_SERVICE_PATTERN.md)), aber asynchrone Operationen (DB, APIs) werden über `IAsyncTaskService` ausgeführt. Die Callbacks dieser async Tasks werden in der **Async Completion Phase** am Anfang jedes Ticks verarbeitet.

### Completion Queue

Das `IAsyncTaskService` verwaltet eine Thread-sichere Queue von abgeschlossenen Tasks:

```csharp
public interface IAsyncTaskService
{
    /// <summary>
    /// Führt async Task aus, dann Callback im Game Loop mit frischem Context.
    /// </summary>
    void Run<TResult>(
        Guid connectionId,
        Func<Task<TResult>> asyncTask,
        Action<MessageContext, TResult> onComplete);
}
```

### Flow

```
┌─────────────────────────────────────────────────────────────┐
│                   ASYNC COMPLETION FLOW                      │
│                                                              │
│  Tick N:                                                     │
│    Handler ruft IAsyncTaskService.Run() auf                 │
│         │                                                    │
│         └──► Task läuft auf ThreadPool                      │
│                  │                                           │
│                  ├──► DB Query                               │
│                  ├──► HTTP Request                           │
│                  └──► File I/O                               │
│                                                              │
│  Tick N+1, N+2, ... (Task läuft noch)                       │
│                                                              │
│  Tick N+X: (Task fertig)                                    │
│    Async Completion Phase:                                  │
│      1. Task.ContinueWith() enqueued Callback               │
│      2. Completion Queue wird abgearbeitet                  │
│      3. Callback(freshContext, result) wird aufgerufen      │
│         • freshContext = aktueller State                    │
│         • Im Game Loop Thread (Thread-Safe!)                │
│         • Kann ctx.Send() sicher aufrufen                   │
└─────────────────────────────────────────────────────────────┘
```

### Implementierung in GameLoop

```csharp
public async Task RunAsync(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        _tickTimer.Restart();
        
        // ═══ TICK LOGIC ═══
        
        // 0️⃣ Async Completion Phase (ZUERST!)
        ProcessAsyncCompletions();
        
        // 1️⃣ Input Phase
        ProcessInputs();
        
        // 2️⃣ Validation Phase
        ValidateActions();
        
        // 3️⃣ Simulation Phase
        SimulateWorld();
        
        // 4️⃣ Broadcast Phase
        BroadcastUpdates();
        
        // 5️⃣ Persistence Phase
        if (_currentTick % 30 == 0)
            PersistToRedis();
        
        _currentTick++;
        
        // Sleep für verbleibende Zeit
        await SleepUntilNextTick(cancellationToken);
    }
}

private void ProcessAsyncCompletions()
{
    var asyncTaskService = _serviceProvider.GetRequiredService<IAsyncTaskService>();
    asyncTaskService.ProcessCompletions();  // Verarbeitet alle fertigen Tasks
}
```

### Beispiel: Login mit Async Completion

```csharp
// Handler-Methode (synchron)
private void HandleLoginRequest(MessageContext ctx, LoginRequest request)
{
    ctx.GetService<IAsyncTaskService>().Run(
        ctx.ConnectionId,
        
        // Async Teil (läuft auf ThreadPool in Tick N)
        async () => 
        {
            var authResult = await _authService.AuthenticateAsync(
                request.Username, request.Password);
            
            if (!authResult.Success)
                return new LoginTaskResult(false, authResult.Error);
            
            var player = await _playerService.SpawnPlayerAsync(
                authResult.AccountId!.Value, request.Username, ctx.Connection);
            
            return new LoginTaskResult(true, player);
        },
        
        // Callback (läuft in Tick N+X in Async Completion Phase)
        (freshCtx, result) =>
        {
            if (result.Success)
            {
                _broadcast.SendToPlayer(freshCtx.Connection,
                    new LoginResponse(true, freshCtx.ConnectionId, 
                                     freshCtx.ServerPlayer!.RuntimeId.ZoneId, null));
            }
            else
            {
                _broadcast.SendToPlayer(freshCtx.Connection,
                    new LoginResponse(false, freshCtx.ConnectionId, 0, result.Error));
            }
        }
    );
}
```

### Wichtige Aspekte

- **Frischer Context:** Callback erhält `MessageContext` mit aktuellem State
- **Thread-Safety:** Callback läuft im Game Loop Thread (Single-Threaded)
- **Connection-Validation:** IAsyncTaskService prüft ob Connection noch existiert
- **Error Handling:** Exceptions im async-Teil werden gefangen und geloggt
- **Keine Garantie:** Callback wird nur ausgeführt wenn Connection noch existiert

### Performance-Überlegungen

- **Async Completion zuerst:** Am Anfang des Ticks, damit Responses schnell gesendet werden
- **Batching:** Alle fertigen Tasks werden in einem Durchgang verarbeitet
- **Timeout:** Tasks mit Timeout verhindern hängende Callbacks

Siehe auch: [Handler/Service-Pattern](HANDLER_SERVICE_PATTERN.md#async-handling) für Details zum IAsyncTaskService-Pattern.

---

## Tick-Phasen im Detail

### 0. Async Completion Phase

- Alle fertigen async Tasks aus der Completion Queue holen
- Für jeden Task:
  - Connection-Validierung (existiert noch?)
  - Frischen `MessageContext` erstellen
  - Callback im Game Loop Thread ausführen
  - Exceptions fangen und loggen

### 1. Input Phase

- Alle eingehenden Messages aus der Input-Queue lesen
- Messages nach Timestamp sortieren für deterministische Verarbeitung
- Duplikate und ungültige Messages entfernen

### 2. Validation Phase

- Movement-Validierung (Speed-Check, Collision)
- Action-Validierung (Range, Cooldown, Ressourcen)
- Cheater-Detection aktivieren

### 3. Simulation Phase

- Spieler-Positionen aktualisieren
- Kampf-Berechnungen durchführen
- AI/NPC Updates verarbeiten
- Respawn-Checks ausführen

### 4. Broadcast Phase

- Position-Broadcasts sammeln und zusammenfassen
- Event-Broadcasts sammeln
- Updates an relevante Clients senden

**Detaillierte Implementierung:**

```csharp
protected virtual Task OutputPhaseAsync(CancellationToken cancellationToken)
{
    // 1️⃣ Dirty Entities sammeln (Delta Updates)
    var dirtyEntities = _zoneManager.GetDirtyEntities();
    
    if (dirtyEntities.Any())
    {
        // PositionBroadcast für alle geänderten Entities
        foreach (var entity in dirtyEntities)
        {
            var positionBroadcast = new PositionBroadcast(
                timestamp: CurrentTick,
                entityId: entity.EntityId,
                position: entity.Position
            );
            
            _pendingBroadcasts.Enqueue(new BroadcastMessage
            {
                Message = positionBroadcast,
                Recipients = null,  // Alle in Zone
                Priority = BroadcastPriority.Normal
            });
        }
        
        // Dirty Flags zurücksetzen
        _zoneManager.ClearDirtyFlags();
    }
    
    // 2️⃣ Full ZoneState (alle 25 Ticks = 1 Sekunde)
    if (CurrentTick % 25 == 0)
    {
        var allEntities = _zoneManager.GetAllEntities();
        var fullState = new ZoneState(
            timestamp: CurrentTick,
            zoneId: _zoneManager.CurrentZoneId,
            entities: allEntities
        );
        
        _pendingBroadcasts.Enqueue(new BroadcastMessage
        {
            Message = fullState,
            Recipients = null,
            Priority = BroadcastPriority.Normal
        });
        
        _log.Debug(
            "Sent full ZoneState: {EntityCount} entities at tick {Tick}",
            allEntities.Count,
            CurrentTick
        );
    }
    
    // 3️⃣ Pending Broadcasts an NetworkServer übergeben
    // (NetworkServer leert die Queue und sendet an Clients)
    
    return Task.CompletedTask;
}
```

**Optimierungen:**

- **Batching:** Mehrere kleine Messages können zu einer großen zusammengefasst werden
- **Interest Management:** Später nur Entities senden die für Client relevant sind (Sichtbereich)
- **Priority Queue:** High-Priority Messages (z.B. Disconnect) werden zuerst gesendet

### 5. Persistence Phase

- Dirty-Flags für geänderte Entities prüfen
- Änderungen in Redis pushen (jede Sekunde)
- Periodisch in PostgreSQL speichern (alle 10 Sekunden)

---

## Verwandte Dokumentation

- [Handler/Service-Pattern](HANDLER_SERVICE_PATTERN.md) - Message Handling und Async Operations
- [Server-Komponenten](SERVER_COMPONENTS.md) - Zone Server Details
- [Client-Server Sync](CLIENT_SERVER_SYNC.md) - Prediction und Reconciliation
- [Redis-Strategie](REDIS.md) - Persistence Phase Details
- [Datenbank-Strategie](DATABASE.md) - Write-Strategien

---

## 🔗 Nützliche Links

- [Game Loop Patterns](https://gameprogrammingpatterns.com/game-loop.html)
- [Fix Your Timestep!](https://gafferongames.com/post/fix_your_timestep/)
- [.NET High-Resolution Timing](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch)

---

*Teil der [Architektur-Dokumentation](README.md)*
