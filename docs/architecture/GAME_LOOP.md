# 🔄 Game Loop Design

## 2DMMO – Server Game Loop

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](../ARCHITECTURE.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt das Game Loop Design für den 2DMMO Server, inklusive Tick Timing und Phasen-Verarbeitung.

---

## Server Game Loop (30 Hz)

```
┌─────────────────────────────────────────────────────────┐
│                  SERVER GAME LOOP                        │
│                   (33.33ms pro Tick)                    │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │                    TICK START                       │ │
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
    private const int TICK_RATE = 30;  // Hz
    private const double TICK_INTERVAL_MS = 1000.0 / TICK_RATE;  // 33.33ms
    
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

## Tick-Phasen im Detail

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

### 5. Persistence Phase

- Dirty-Flags für geänderte Entities prüfen
- Änderungen in Redis pushen (jede Sekunde)
- Periodisch in PostgreSQL speichern (alle 10 Sekunden)

---

## Verwandte Dokumentation

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

*Teil der [Architektur-Dokumentation](../ARCHITECTURE.md)*
