# 🔄 Game Loop Design

## 2DMMO – Server Game Loop

**Version:** 1.3.0  
**Letzte Aktualisierung:** 2025-12-22  
**Teil von:** [Architektur-Dokumentation](Architecture-Overview.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt das Game Loop Design für den 2DMMO Server, inklusive Tick Timing und Phasen-Verarbeitung.

---

## Server Game Loop (20 Hz)

```
┌─────────────────────────────────────────────────────────┐
│                  SERVER GAME LOOP                        │
│                   (50ms pro Tick)                    │
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
    private const int TICK_RATE = 20;  // Hz
    private const double TICK_INTERVAL_MS = 1000.0 / TICK_RATE;  // 50ms
    
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
            
            if (_currentTick % 20 == 0)  // Jede Sekunde
            {
                PersistToRedis();
            }
            
            if (_currentTick % 200 == 0)  // Alle 10 Sekunden
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

### 3. Full ZoneState (Alle Entities, alle 20 Ticks = 1 Sekunde)

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
    
    // Full Sync alle 1 Sekunde (20 Ticks)
    if (CurrentTick % 20 == 0)
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
| Delta Updates | 20 Hz (50ms) | Mittel | Laufende Änderungen |
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
         
Tick 20: Full ZoneState mit ALLEN Entities (unabhängig von Dirty)
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

### 4. Broadcast Phase (Output Phase)

Die Broadcast Phase ist in Phase 2 mit dem Chunk-Based Delta Sync System optimiert worden.

**Ablauf:**

1. **Immediate Broadcasts** (sofort, nicht gebatched)
   - `EntityAnimation`, `EntityAggro`, `EntityEmote` 
   - Critical Combat Events
   - Request/Response Messages

2. **Chunk-Based Delta Collection** (jeden Tick)
   - ChunkDirtyTracker sammelt geänderte Entities pro Chunk
   - Pro Client: Nur Chunks in Sichtweite (3x3 Grid = 9 Chunks)
   - `ZoneDelta` wird pro Client mit relevanten Änderungen erstellt

3. **Periodic Full Sync** (alle 20 Ticks = 1 Sekunde)
   - `ZoneState` mit allen Entities in sichtbaren Chunks
   - Desync-Prevention Fallback

**Detaillierte Implementierung (Phase 2):**

```csharp
protected virtual Task OutputPhaseAsync(CancellationToken cancellationToken)
{
    // 1️⃣ Immediate Broadcasts (bereits in Queue von Simulation Phase)
    // Beispiel: EntityAnimation wurde während Combat direkt geenqueued
    // → Wird sofort gesendet, nicht gebatched
    
    // 2️⃣ Chunk-Based Delta-Erstellung (jeden Tick)
    var dirtyChunks = _chunkDirtyTracker.GetDirtyChunks();
    
    if (dirtyChunks.Any())
    {
        // Pro-Client Delta sammeln
        var clientDeltas = new Dictionary<Guid, ZoneDelta>();
        
        foreach (var chunk in dirtyChunks)
        {
            // Welche Clients sehen diesen Chunk?
            var viewingClients = _clientViewService.GetClientsViewingChunk(chunk);
            
            // Entities in diesem Chunk die sich geändert haben
            var dirtyEntityIds = _chunkDirtyTracker.GetDirtyEntitiesInChunk(chunk);
            
            foreach (var clientId in viewingClients)
            {
                // Initialisiere Delta für Client falls noch nicht vorhanden
                if (!clientDeltas.ContainsKey(clientId))
                {
                    clientDeltas[clientId] = new ZoneDelta
                    {
                        Timestamp = CurrentTick,
                        ZoneId = _zoneManager.CurrentZoneId
                    };
                }
                
                // Füge Entities zu Client-Delta hinzu
                foreach (var entityId in dirtyEntityIds)
                {
                    var entity = _entityManager.GetEntity(entityId);
                    
                    if (entity.IsNewlySpawned)
                    {
                        clientDeltas[clientId].SpawnedEntities ??= new List<EntityDtoUnion>();
                        clientDeltas[clientId].SpawnedEntities.Add(entity.ToDto());
                    }
                    else if (entity.PositionChanged)
                    {
                        clientDeltas[clientId].PositionUpdates ??= new List<EntityPositionDelta>();
                        clientDeltas[clientId].PositionUpdates.Add(entity.ToPositionDelta());
                    }
                    else if (entity.StateChanged)
                    {
                        clientDeltas[clientId].StateUpdates ??= new List<EntityStateDelta>();
                        clientDeltas[clientId].StateUpdates.Add(entity.ToStateDelta());
                    }
                }
            }
        }
        
        // Sende Deltas an Clients
        foreach (var (clientId, delta) in clientDeltas)
        {
            _pendingBroadcasts.Enqueue(new BroadcastMessage
            {
                Message = delta,
                Recipients = new List<Guid> { clientId },  // Pro Client!
                Priority = BroadcastPriority.Normal
            });
        }
        
        // Dirty Flags zurücksetzen
        _chunkDirtyTracker.ClearDirtyFlags();
    }
    
    // 3️⃣ Periodic Full ZoneState (alle 20 Ticks = 1 Sekunde)
    if (CurrentTick % 20 == 0)
    {
        // Pro Client: Nur Entities in sichtbaren Chunks
        foreach (var (clientId, connection) in _connections)
        {
            var player = _playerService.GetPlayer(clientId);
            var visibleChunks = _clientViewService.GetClientVisibleChunks(clientId);
            var visibleEntities = _entityManager.GetEntitiesInChunks(visibleChunks);
            
            var fullState = new ZoneState
            {
                Timestamp = CurrentTick,
                ZoneId = _zoneManager.CurrentZoneId,
                StateType = ZoneStateType.FullSync,
                Entities = visibleEntities.ToUnionDtoList(),
                TotalEntityCount = visibleEntities.Count
            };
            
            _pendingBroadcasts.Enqueue(new BroadcastMessage
            {
                Message = fullState,
                Recipients = new List<Guid> { clientId },
                Priority = BroadcastPriority.Normal
            });
        }
        
        _log.Debug("Sent chunk-filtered ZoneState at tick {Tick}", CurrentTick);
    }
    
    // 4️⃣ Pending Broadcasts an NetworkServer übergeben
    // (NetworkServer leert die Queue und sendet an Clients)
    
    return Task.CompletedTask;
}
```

**Broadcast-Typen Übersicht:**

| Typ | Frequenz | Scope | Batched? | Beispiele |
|-----|----------|-------|----------|-----------|
| **Immediate Events** | Bei Bedarf | Zone-weit | ❌ Nein | EntityAnimation, EntityAggro, EntityEmote |
| **Chunk-Based Delta** | Jeden Tick (40ms) | Pro Client (9 Chunks) | ✅ Ja | ZoneDelta (Position, State, Spawn, Despawn) |
| **Periodic Full Sync** | Alle 20 Ticks (1s) | Pro Client (9 Chunks) | ✅ Ja | ZoneState (Desync-Prevention) |

**Bandbreiten-Optimierungen:**

- **Chunk-Filtering:** Client erhält nur Entities in sichtbaren 9 Chunks statt alle in Zone
- **Batching:** Alle Änderungen in einer `ZoneDelta` Message statt einzelne Messages
- **Delta-Only:** Nur geänderte Properties (null-Felder bei StateUpdates)
- **Pro-Client Deltas:** Jeder Client erhält individuell gefilterte Updates

**Performance-Verbesserungen:**

- ✅ **96%+ Bandbreiten-Reduktion** vs. Full Zone Broadcast
- ✅ **Konstante Bandbreite** unabhängig von Zone-Größe
- ✅ **Skaliert mit Sichtbereich**, nicht mit Entity-Count

Siehe [Chunk-Based Sync](CHUNK_BASED_SYNC.md) für vollständige Dokumentation.

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

*Teil der [Architektur-Dokumentation](Architecture-Overview.md)*

Source: docs/02-architecture/GAME_LOOP.md
