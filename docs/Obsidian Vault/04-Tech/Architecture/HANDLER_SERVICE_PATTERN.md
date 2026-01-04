# 🔌 Handler/Service Pattern

## 2DMMO – Message Handling Architecture

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-22  
**Teil von:** [Architektur-Dokumentation](Architecture-Overview.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt das Handler/Service-Pattern, das im 2DMMO Server verwendet wird, um Netzwerk-Nachrichten zu verarbeiten und Business-Logik zu kapseln.

### Architektur-Prinzipien

Das Handler/Service-Pattern trennt **Message-Verarbeitung** (Handler) von **Business-Logik** (Services):

- ✅ **Handler** empfangen Messages, validieren Input und delegieren an Services
- ✅ **Services** enthalten die Business-Logik und sind testbar
- ✅ **Dependency Injection** ermöglicht lose Kopplung und Testbarkeit
- ✅ **Async-Handling** über `IAsyncTaskService` hält den Game Loop reaktionsfähig

```
┌─────────────────────────────────────────────────────────────┐
│                     MESSAGE FLOW                             │
│                                                              │
│  Client Message                                              │
│      │                                                       │
│      ▼                                                       │
│  MessageRouter ──► O(1) Category Lookup                     │
│      │                                                       │
│      ▼                                                       │
│  CategoryHandler ──► O(1) Type Lookup                       │
│      │                                                       │
│      ▼                                                       │
│  Handler Method (synchron)                                   │
│      │                                                       │
│      ├──► Validation (RequireAuthenticated, etc.)           │
│      │                                                       │
│      ├──► Service Call (über DI)                            │
│      │       │                                               │
│      │       ├──► Synchrone Logik                           │
│      │       │                                               │
│      │       └──► Async via IAsyncTaskService               │
│      │             • Task läuft auf ThreadPool              │
│      │             • Callback im Game Loop Thread           │
│      │                                                       │
│      └──► Response (ctx.Send oder IBroadcastService)        │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Handler-Architektur

### BaseCategoryHandler

Die Basisklasse für alle Category-Handler bietet O(1) Message-Routing innerhalb einer Kategorie.

**Implementierung:**

```csharp
public abstract class BaseCategoryHandler : ICategoryHandler
{
    private readonly Action<MessageContext, INetworkMessage>?[] _handlers =
        new Action<MessageContext, INetworkMessage>?[100];
    
    private readonly ILog _log;
    
    protected BaseCategoryHandler(ILog log)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        RegisterHandlers();
    }
    
    public abstract MessageCategory Category { get; }
    
    public bool CanHandle(MessageType type)
    {
        int index = (ushort)type % 100;
        return index < 100 && _handlers[index] != null;
    }
    
    public void Handle(MessageContext ctx, MessageType type, INetworkMessage message)
    {
        int index = (ushort)type % 100;
        Action<MessageContext, INetworkMessage>? handler = _handlers[index];
        
        if (handler == null)
        {
            _log.Warn("No handler registered for {MessageType}", type);
            return;
        }
        
        try
        {
            handler(ctx, message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error in handler for {MessageType}", type);
            ctx.GetService<IBroadcastService>()
                .SendError(ctx.Connection, "INTERNAL_ERROR", 
                          "An error occurred processing your request", null, null);
        }
    }
    
    protected abstract void RegisterHandlers();
    
    protected void Register<TMessage>(MessageType type, 
                                     Action<MessageContext, TMessage> handler)
        where TMessage : INetworkMessage
    {
        int index = (ushort)type % 100;
        _handlers[index] = (ctx, msg) => handler(ctx, (TMessage)msg);
    }
}
```

**Wichtige Aspekte:**

- **O(1) Array-Lookup:** MessageType % 100 = Index im Array
- **Type-Safety:** Generic `Register<TMessage>()` verhindert Typ-Fehler
- **Error Handling:** Exceptions werden gefangen und als Error-Message gesendet
- **Abstrakte Registrierung:** Subklassen implementieren `RegisterHandlers()`

### ICategoryHandler Interface

```csharp
public interface ICategoryHandler
{
    MessageCategory Category { get; }
    bool CanHandle(MessageType type);
    void Handle(MessageContext ctx, MessageType type, INetworkMessage message);
}
```

### Handler-Registrierung

Handler werden in `Program.cs` via Dependency Injection registriert:

```csharp
private static void RegisterHandlers(IServiceProvider serviceProvider, MessageRouter router)
{
    Type[] handlerTypes =
    [
        typeof(ConnectionHandler),
        typeof(ZoneHandler),
        typeof(MovementHandler),
        typeof(CombatHandler),
        typeof(ChatHandler),
        typeof(PingHandler)
    ];

    foreach (Type handlerType in handlerTypes)
    {
        var handler = (ICategoryHandler)serviceProvider.GetRequiredService(handlerType);
        router.RegisterHandler(handler);
    }
}
```

**Aktuell implementierte Handler:**

| Handler | Category | Range | Beschreibung |
|---------|----------|-------|--------------|
| `ConnectionHandler` | Connection | 0-99 | Login, Logout, Character Selection |
| `ZoneHandler` | Zone | 100-199 | Zone Transfer, Zone Events |
| `MovementHandler` | Movement | 200-299 | Position Updates, Movement Validation |
| `CombatHandler` | Combat | 300-399 | Combat Actions (Placeholder) |
| `ChatHandler` | Chat | 400-499 | Chat Messages (Placeholder) |
| `PingHandler` | Ping | 900-999 | Ping/Pong, Latency Measurement |

---

## 🏗️ Service-Architektur

### Interface-basierte Services

Services sind über Interfaces definiert und über Dependency Injection verfügbar:

```csharp
// ════════════════════════════════════════════════════════════
// SERVICE INTERFACES
// ════════════════════════════════════════════════════════════

// Authentication
public interface IAuthenticationService
{
    Task<AuthResult> AuthenticateAsync(string username, string password);
}

// Zone Management
public interface IZoneService
{
    Task<ZoneTransferResult> RequestZoneTransferAsync(Guid persistentId, int targetZoneId);
}

// Entity Management
public interface IEntityService
{
    EntityData? GetEntity(EntityIdentity entityId);
}

// Broadcasting
public interface IBroadcastService
{
    void SendToPlayer(ClientConnection connection, INetworkMessage message);
    void SendToZone(int zoneId, INetworkMessage message);
    void SendError(ClientConnection connection, string code, string message, 
                   string? detail, string? resolution);
}

// Player Management
public interface IPlayerService
{
    Task<ServerPlayerCharacter> SpawnPlayerAsync(Guid accountId, string username, 
                                                 ClientConnection connection);
}

// Async Task Execution
public interface IAsyncTaskService
{
    void Run<TResult>(Guid connectionId, Func<Task<TResult>> asyncTask, 
                     Action<MessageContext, TResult> onComplete);
    void Run(Guid connectionId, Func<Task> asyncTask, 
            Action<MessageContext> onComplete);
}
```

### Dependency Injection in Handlern

Handler erhalten Services über Constructor Injection:

```csharp
public class ZoneHandler(IZoneService zoneService, ILog log) 
    : BaseCategoryHandler(log)
{
    public override MessageCategory Category => MessageCategory.Zone;
    
    protected override void RegisterHandlers() =>
        Register<ZoneTransferRequest>(MessageType.ZoneTransferRequest, HandleZoneTransfer);
    
    private void HandleZoneTransfer(MessageContext ctx, ZoneTransferRequest request)
    {
        // Service wird über Constructor injiziert
        ctx.GetService<IAsyncTaskService>().Run(ctx.ConnectionId, 
            async () => zoneService.RequestZoneTransferAsync(
                ctx.ServerPlayer!.Entity.PersistentId,
                request.TargetZoneId
            ), 
            (ctx, result) =>
            {
                ctx.GetService<IBroadcastService>().SendToPlayer(ctx.Connection, 
                    result.Success
                        ? ZoneTransferResponse.Succeeded(request.TargetZoneId)
                        : ZoneTransferResponse.Failed(result.Error!));
            });
    }
}
```

### Service-Registrierung in Program.cs

```csharp
private static ServiceCollection ConfigureServices()
{
    var services = new ServiceCollection();
    
    // ════════════════════════════════════════════════════════════
    // SERVICES (Business Logic)
    // ════════════════════════════════════════════════════════════
    
    services.AddSingleton<IAuthenticationService, AuthenticationService>();
    services.AddSingleton<IPlayerService, PlayerService>();
    services.AddSingleton<IBroadcastService, BroadcastService>();
    services.AddSingleton<IAsyncTaskService, AsyncTaskService>();
    services.AddSingleton<IZoneService, ZoneService>();
    services.AddSingleton<IEntityService, EntityService>();
    
    // ════════════════════════════════════════════════════════════
    // HANDLERS
    // ════════════════════════════════════════════════════════════
    
    services.AddSingleton<ConnectionHandler>();
    services.AddSingleton<ZoneHandler>();
    services.AddSingleton<MovementHandler>();
    services.AddSingleton<CombatHandler>();
    services.AddSingleton<ChatHandler>();
    services.AddSingleton<PingHandler>();
    
    return services;
}
```

---

## ⚡ Async-Handling

### Wichtiger Grundsatz: Handler sind SYNCHRON

**Handler-Methoden sind IMMER synchron (`void`, nicht `async Task`).**

Warum?
- ✅ Game Loop läuft in einem Single-Thread
- ✅ Vermeidet Race Conditions und Deadlocks
- ✅ Deterministische Verarbeitung
- ✅ Einfaches Debugging

### IAsyncTaskService Pattern

Für asynchrone Operationen (Datenbank, API-Calls) verwenden Handler das `IAsyncTaskService`:

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

### Async-Flow

```
┌─────────────────────────────────────────────────────────────┐
│                     ASYNC FLOW                               │
│                                                              │
│  1. Handler Method (Game Loop Thread)                       │
│      │                                                       │
│      └──► ctx.GetService<IAsyncTaskService>().Run(...)      │
│              │                                               │
│              ├──► async Task startet auf ThreadPool         │
│              │     • DB Query                                │
│              │     • HTTP Request                            │
│              │     • File I/O                                │
│              │                                               │
│              └──► Task.ContinueWith() enqueued Callback     │
│                    in Completion Queue                       │
│                                                              │
│  2. Game Loop - Completion Phase (nächster Tick)            │
│      │                                                       │
│      └──► Completion Queue abarbeiten                       │
│              │                                               │
│              └──► Callback(freshContext, result)            │
│                    • freshContext hat aktuellen State       │
│                    • Callback läuft im Game Loop Thread     │
│                    • Kann sicher ctx.Send() aufrufen        │
└─────────────────────────────────────────────────────────────┘
```

### Beispiel: Login mit Async-Handling

```csharp
private void HandleLoginRequest(MessageContext ctx, LoginRequest request)
{
    ctx.GetService<IAsyncTaskService>().Run(
        ctx.ConnectionId,
        
        // 1️⃣ ASYNC TEIL (läuft auf ThreadPool)
        async () =>
        {
            // Authentifizierung (DB-Zugriff)
            AuthResult authResult = await _authService.AuthenticateAsync(
                request.Username,
                request.Password
            );
            
            if (!authResult.Success)
                return new LoginTaskResult(false, Error: authResult.Error);
            
            // Session erstellen
            Guid sessionToken = IdRegistry.Instance.GeneratePersistentId();
            ctx.Connection.SetAuthenticated(
                authResult.AccountId!.Value,
                authResult.Username!,
                authResult.Flags,
                sessionToken
            );
            
            // Player spawnen (auch async)
            ServerPlayerCharacter player = await _playerService.SpawnPlayerAsync(
                authResult.AccountId.Value,
                request.Username,
                ctx.Connection
            );
            
            return new LoginTaskResult(true, player);
        },
        
        // 2️⃣ CALLBACK (läuft im Game Loop mit frischem Context)
        (freshCtx, result) =>
        {
            if (result.Success)
            {
                _broadcast.SendToPlayer(freshCtx.Connection,
                    new LoginResponse(true, freshCtx.ConnectionId, 
                                     freshCtx.ServerPlayer!.RuntimeId.ZoneId, null));
                
                _log.Info("Player {Name} logged in", freshCtx.ServerPlayer!.Name);
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

- **Frischer Context:** Der Callback erhält einen `freshContext` mit dem aktuellen State
- **Thread-Safety:** Callback läuft im Game Loop Thread (Single-Threaded)
- **Error Handling:** Exceptions im async-Teil werden gefangen und geloggt
- **Connection-Validation:** IAsyncTaskService prüft ob Connection noch existiert

---

## 📝 Best Practices

### Handler Best Practices

1. **Validierung zuerst**
   ```csharp
   private void HandleAction(MessageContext ctx, ActionRequest request)
   {
       if (!RequireInGame(ctx)) return;
       if (!RequireNotMuted(ctx)) return;
       
       // Logik hier
   }
   ```

2. **Delegation an Services**
   ```csharp
   // ❌ FALSCH - Logik im Handler
   private void HandleAttack(MessageContext ctx, AttackRequest request)
   {
       float damage = ctx.Player.Attack * 1.5f;
       target.Health -= damage;
       // ...
   }
   
   // ✅ RICHTIG - Delegation an Service
   private void HandleAttack(MessageContext ctx, AttackRequest request)
   {
       var combatService = ctx.GetService<ICombatService>();
       var result = combatService.ExecuteAttack(ctx.Player, request.TargetId);
       
       if (result.Success)
           _broadcast.SendToZone(ctx.ZoneId, new DamageEvent(result));
   }
   ```

3. **Async für I/O, Sync für Game-Logik**
   ```csharp
   // ✅ Async für DB/API
   ctx.RunAsync(
       async () => await _database.SavePlayerAsync(player),
       (ctx, result) => _log.Info("Saved")
   );
   
   // ✅ Sync für Game-Logik
   player.Position = newPosition;
   player.MarkDirty();
   ```

4. **Error Messages sind User-Friendly**
   ```csharp
   // ❌ FALSCH - Technische Details
   _broadcast.SendError(ctx.Connection, "NPE", 
       "NullReferenceException in line 42", null, null);
   
   // ✅ RICHTIG - Verständliche Nachricht
   _broadcast.SendError(ctx.Connection, "TARGET_NOT_FOUND", 
       "The target is no longer available", 
       "The target may have logged out or moved to another zone",
       "Please select a new target");
   ```

### Service Best Practices

1. **Services sind zustandslos oder Thread-Safe**
   ```csharp
   // ✅ RICHTIG - Zustandslos
   public class CombatService : ICombatService
   {
       private readonly ILog _log;
       
       public CombatResult ExecuteAttack(PlayerEntity attacker, Guid targetId)
       {
           // Nutzt nur Parameter, kein interner State
       }
   }
   ```

2. **Ein Service, eine Verantwortung**
   ```csharp
   // ❌ FALSCH - God Service
   public interface IGameService
   {
       Task AuthenticateAsync();
       void ProcessCombat();
       void HandleChat();
   }
   
   // ✅ RICHTIG - Focused Services
   public interface IAuthenticationService { ... }
   public interface ICombatService { ... }
   public interface IChatService { ... }
   ```

3. **Interface Segregation**
   ```csharp
   // ✅ Kleine, fokussierte Interfaces
   public interface IPlayerSpawner
   {
       Task<ServerPlayerCharacter> SpawnAsync(Guid accountId);
   }
   
   public interface IPlayerRepository
   {
       Task<PlayerData?> LoadAsync(Guid accountId);
       Task SaveAsync(PlayerData data);
   }
   ```

### MessageContext Best Practices

1. **Nutze Helper-Methoden**
   ```csharp
   // ✅ Helper-Methoden von BaseCategoryHandler
   if (!RequireInGame(ctx)) return;
   if (!RequireGameMaster(ctx)) return;
   if (!RequireNotMuted(ctx)) return;
   ```

2. **Services über GetService<T>() holen**
   ```csharp
   var broadcast = ctx.GetService<IBroadcastService>();
   var combat = ctx.GetService<ICombatService>();
   ```

3. **Context nicht speichern**
   ```csharp
   // ❌ FALSCH - Context außerhalb Handler speichern
   private MessageContext _savedContext;
   
   private void HandleMessage(MessageContext ctx, Message msg)
   {
       _savedContext = ctx;  // NIEMALS TUN!
   }
   
   // ✅ RICHTIG - Nur für Scope des Handlers nutzen
   private void HandleMessage(MessageContext ctx, Message msg)
   {
       // Context nur hier verwenden
       ctx.Send(...);
   }
   ```

---

## 🔗 Verwandte Dokumentation

- [Message-Spezifikation](MESSAGES.md) - MessageType Enum und Categories
- [Game Loop Design](GAME_LOOP.md) - Tick Phasen und Message Processing
- [Netzwerk-Protokoll](NETWORK_PROTOCOL.md) - Message Framing und Transport
- [Client-Server Sync](CLIENT_SERVER_SYNC.md) - Wie Messages verarbeitet werden

---

## 📚 Code-Referenzen

### Handler-Implementierungen

- `server/Mmo.Server/MessageRouting/Handler/BaseCategoryHandler.cs` - Basisklasse
- `server/Mmo.Server/MessageRouting/Interfaces/ICategoryHandler.cs` - Interface
- `server/Mmo.Server/Connections/MessageHandler/ConnectionHandler.cs` - Beispiel-Handler
- `server/Mmo.Server/Zones/MessageHandler/ZoneHandler.cs` - Beispiel mit Async

### Service-Implementierungen

- `server/Mmo.Server/AsyncTask/Interface/IAsyncTaskService.cs` - Async Task Interface
- `server/Mmo.Server/AsyncTask/Services/AsyncTaskService.cs` - Async Task Implementierung
- `server/Mmo.Server/Zones/Interfaces/IZoneService.cs` - Zone Service Interface
- `server/Mmo.Server/Network/Interfaces/IBroadcastService.cs` - Broadcast Service Interface

### Dependency Injection

- `server/Mmo.Server/Core/Program.cs` - DI-Container Konfiguration und Handler-Registrierung

---

*Teil der [Architektur-Dokumentation](Architecture-Overview.md)*

Source: docs/02-architecture/HANDLER_SERVICE_PATTERN.md
