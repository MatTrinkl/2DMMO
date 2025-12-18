# Copilot Code Review Instructions

## About This Document

These instructions apply to Copilot during code reviews in Pull Requests.
Copilot should review the code against our project documentation and report deviations.

**Relevant Documentation:**
- `docs/02-architecture/README.md` - Technical Architecture
- `docs/03-technical-details/TECHNICAL_DESIGN.md` - Detailed Technical Decisions
- `docs/01-overview/PROTOTYPE_SCOPE.md` - Prototype Scope
- `docs/01-overview/GAME_DESIGN_DOCUMENT.md` - Gameplay Design

---

## 🔗 Pull Request & Issue Linking

### Issue Reference Check

For EVERY Pull Request Review:

1. **Check if issues are linked:**
   - Search for `Closes #`, `Fixes #`, `Resolves #` in PR description
   - Search for issue references in commit messages
   - If NO issue reference found:
   
   > ❓ **Missing Issue Reference**
   > 
   > This PR does not reference an issue. Please specify which issues this PR closes:
   > - Add `Closes #XX` to the PR description
   > - Or explain why no issue exists
   > 
   > **Open issues that could be relevant:**
   > _(List relevant open issues based on PR content)_

2. **Check issue completeness:**
   - Fetch the linked issue
   - Check ALL tasks (checkboxes `- [ ]`) in the issue
   - Check ALL acceptance criteria in the issue
   - Compare with the code in the PR

3. **For missing implementations:**

   > ⚠️ **Incomplete Implementation**
   > 
   > Issue #XX has the following tasks/acceptance criteria that are not implemented in the PR:
   > 
   > **Missing Tasks:**
   > - [ ] Task 1 from the issue
   > - [ ] Task 2 from the issue
   > 
   > **Missing Acceptance Criteria:**
   > - [ ] Criterion 1
   > - [ ] Criterion 2
   > 
   > **Options:**
   > 1. Implement the missing points in this PR
   > 2. Create follow-up issues for the open points
   > 3. Explain why these points are no longer relevant

### Issue Checklist for Reviews

For every PR with linked issue:

- [ ] PR description contains `Closes #XX` or similar?
- [ ] All `- [ ]` tasks in the issue are implemented in the code?
- [ ] All acceptance criteria in the issue are met?
- [ ] Tests for the acceptance criteria exist?
- [ ] Documentation updated (if mentioned in the issue)?

### Automatic Issue Search

If a PR does not reference an issue, search for matching issues based on:
- Filenames in the PR (e.g., `NetworkManager.cs` → Issues with "NetworkManager")
- Folder structure (e.g., `server/` → Issues with label `area:server`)
- Commit messages (keywords)
- PR title

Then suggest matching issues:

> 💡 **Possible Related Issues:**
> - #42 - NetworkManager Autoload (Godot Singleton)
> - #38 - TCP Server with TcpListener
> 
> Please link the appropriate issue with `Closes #XX` in the PR description.

---

## 🏗️ Architecture Conformity

### Server-Client Separation
- [ ] Code is in the correct project (Server/Client/Shared)?
- [ ] Shared Library contains ONLY Messages, Enums, Constants, Interfaces
- [ ] No server logic in client and vice versa
- [ ] Game logic belongs on the server (authoritative)

### Component Structure
- [ ] Gateway Server: Only Connection-Handling, Auth, Routing
- [ ] Zone Server: Game Loop, Validation, Broadcasting
- [ ] Client: Rendering, Input, Prediction

### Communication
- [ ] Client communicates only with Gateway (never directly with Zone Server)
- [ ] Cross-Zone Events via Redis Pub/Sub (not directly)

---

## 📡 Network Protocol

### Message Format
- [ ] Frame format adhered to: `[1 Byte Type][4 Bytes Length][N Bytes Payload]`
- [ ] Length is Little-Endian uint32
- [ ] Payload is MessagePack-serialized

### Message Types
- [ ] New Message-Types registered in `MessageType` enum?
- [ ] Enum values in correct range? (Connection: 1-9, Zone: 10-19, Movement: 20-29, etc.)
- [ ] No duplicates or gaps without reason

### DTOs (Data Transfer Objects)
- [ ] `[MessagePackObject]` attribute present?
- [ ] All properties have `[Key(n)]` attributes with ascending indices?
- [ ] **IMPORTANT:** `MessageType Type` property has `[Key(0)]` (NOT IgnoreMember!)
- [ ] DTOs are in the Shared Library?
- [ ] DTOs implement `INetworkMessage` interface?

### MessagePackObject Validation (CRITICAL)

For EVERY class with `[MessagePackObject]` attribute, check:

1. **MessageType Property MUST be present:**
   ```csharp
   [Key(0)]
   public MessageType Type => MessageType.XXX;
   ```

2. **Type MUST be Key(0):**
   - ❌ WRONG: `[IgnoreMember]` on Type
   - ❌ WRONG: Type without `[Key(0)]`
   - ❌ WRONG: Type is not Key(0) but Key(1) or higher
   - ✅ CORRECT: `[Key(0)] public MessageType Type => MessageType.XXX;`

3. **All other properties with ascending keys:**
   ```csharp
   [Key(1)] public string Username { get; set; }
   [Key(2)] public int PlayerId { get; set; }
   // etc.
   ```

4. **Why not on Interface/Base class?**
   - MessagePack ignores `[Key]` attributes on interfaces
   - Even with inheritance, `[Key(0)]` must be in each concrete class
   - The `INetworkMessage` interface serves only for type safety, not serialization

5. **Error message for violation:**

   > ⚠️ **MessagePack Convention Violated**
   > 
   > The class `{ClassName}` has `[MessagePackObject]` but:
   > - ❌ No `MessageType Type` property found
   > - OR: ❌ `Type` has `[IgnoreMember]` instead of `[Key(0)]`
   > - OR: ❌ `Type` is not `[Key(0)]`
   > 
   > **Correction:**
   > ```csharp
   > [MessagePackObject]
   > public class {ClassName} : INetworkMessage
   > {
   >     [Key(0)]
   >     public MessageType Type => MessageType.{TypeName};
   >     
   >     [Key(1)]
   >     public string Property1 { get; set; }
   >     // ...
   > }
   > ```

6. **Enum does NOT need MessagePackObject:**
   - `enum MessageType : byte` is automatically serialized as byte
   - ❌ WRONG: `[MessagePackObject]` on enum
   - ✅ CORRECT: Only `public enum MessageType : byte { ... }`

### MessagePack Inheritance

For classes that inherit from a `[MessagePackObject]` class, check:

1. **Analyze parent class:**
   - Has `[MessagePackObject]`?
   - What is the highest key? (e.g., `[Key(3)]` → highest = 3)

2. **Check child class:**
   - [ ] Also has `[MessagePackObject]`?
   - [ ] First key starts at **highest parent key + 1**?
   - [ ] No key collisions with parent class?

3. **Example - CORRECT:**
   ```csharp
   [MessagePackObject]
   public class EntityData  // Parent
   {
       [Key(0)] public int Id { get; set; }
       [Key(1)] public float X { get; set; }
       [Key(2)] public float Y { get; set; }
       [Key(3)] public EntityType Type { get; set; }
       // Highest key: 3
   }

   [MessagePackObject]
   public class PlayerData : EntityData  // Child
   {
       // ✅ Starts at Key(4) = highest parent key (3) + 1
       [Key(4)] public string Username { get; set; }
       [Key(5)] public float VelocityX { get; set; }
       [Key(6)] public float VelocityY { get; set; }
   }
   ```

4. **Example - WRONG:**
   ```csharp
   [MessagePackObject]
   public class PlayerData : EntityData
   {
       // ❌ WRONG - Key(0) collides with EntityData.Id!
       [Key(0)] public string Username { get; set; }
   }
   ```

5. **Error message for violation:**

   > ⚠️ **MessagePack Inheritance - Key Collision**
   > 
   > The class `{ClassName}` inherits from `{BaseClassName}`:
   > - `{BaseClassName}` uses keys 0 to {highestKey}
   > - `{ClassName}` must start at Key({highestKey + 1})
   > - Found: Key({actualFirstKey}) ❌
   > 
   > **Correction:** Change first key to `[Key({highestKey + 1})]`.

6. **Multi-level inheritance:**
   ```
   EntityData:     [Key(0-3)]
        ↓
   PlayerData:     [Key(4-6)]  ← starts at 4
        ↓
   AdminPlayer:    [Key(7-8)]  ← starts at 7
   ```

### Serialization
- [ ] MessagePack used for all network messages?
- [ ] No JSON/XML for game traffic
- [ ] MessageSerializer helper used?

---

## 🔄 Game Loop & Timing

### Tick Rate
- [ ] Server tick rate is 25 Hz (40ms per tick)
- [ ] No assumptions about higher/lower tick rate
- [ ] `TICK_RATE` constant from Shared Library used?

### Tick Phases
- [ ] Correct order: Input → Update → Output → Wait
- [ ] No I/O blocking operations in tick
- [ ] Tick overrun is logged (> 40ms)

### Timing
- [ ] Fixed timestep used (not variable)
- [ ] Timestamps for messages are server time
- [ ] Sequence numbers for input ordering

---

## 🎮 Client-Side Prediction & Sync

### Prediction (own player)
- [ ] Client moves immediately locally (Prediction)
- [ ] Input is sent to server with sequence number
- [ ] Pending inputs are stored until server confirms

### Reconciliation
- [ ] On server correction: Snap or interpolation to server position
- [ ] Re-apply pending inputs after correction
- [ ] No hard teleports (smooth correction)

### Interpolation (other players)
- [ ] Other players are interpolated (not predicted)
- [ ] Buffer of ~100ms for jitter compensation
- [ ] Extrapolation only briefly on packet loss

### Server Authority
- [ ] Server is ALWAYS authoritative
- [ ] Client cannot force anything
- [ ] All validation on the server

---

## 💥 Collision

### Shared Collision Data
- [ ] Collision data is in Shared Library?
- [ ] Client and server use the same data
- [ ] `CollisionData` class used?

### Validation
- [ ] Server validates all movements
- [ ] Speed check (MAX_SPEED * deltaTime * TOLERANCE)
- [ ] Bounds check (within the world)
- [ ] Collision check (no walls/obstacles)

---

## 👾 Entity System

### IEntity Interface
- [ ] New entities implement `IEntity`?
- [ ] Properties: Id, Type, X, Y present?

### EntityType Enum
- [ ] New entity type registered in enum?
- [ ] Correct value range? (Player: 1, NPCs: 10+, Interactive: 20+)

### Interactions
- [ ] InteractRequest/Response pattern used?
- [ ] Server validates: Distance, entity exists, permission

---

## 🔒 Security

### Input Validation
- [ ] ALL client inputs are validated
- [ ] PlayerId from session, not from message trust
- [ ] Plausibility checks (Damage, Speed, Position)

### Rate Limiting
- [ ] Actions are rate-limited? (e.g., Chat: 10/min)
- [ ] No possibility for DoS through message spam

### Sensitive Data
- [ ] No passwords in plaintext
- [ ] No sensitive data in logs
- [ ] No server secrets in client code
- [ ] Session tokens are UUIDs, not predictable

### Anti-Cheat
- [ ] Speed-Hack Detection
- [ ] Teleport Detection
- [ ] Action-Rate Limiting
- [ ] Damage-Plausibility Checks

---

## 🌐 Connection Handling

### Timeouts
- [ ] CONNECTION_TIMEOUT: 10s for login
- [ ] HEARTBEAT_INTERVAL: 5s Client → Server
- [ ] HEARTBEAT_TIMEOUT: 15s → Disconnect
- [ ] RECONNECT_WINDOW: 30s keep state

### Disconnect
- [ ] Graceful disconnect handling
- [ ] PlayerLeft is broadcast
- [ ] Resources are cleaned up (Dispose)

### Reconnect
- [ ] ReconnectToken system used?
- [ ] Exponential backoff for retries
- [ ] Max 10 attempts, then give up

### Error Codes
- [ ] ErrorCode enum from documentation used?
- [ ] Appropriate error code for each error case
- [ ] Error messages are not too detailed (Security)

---

## 📝 Logging

### Log Levels
- [ ] TRACE: Only for debugging (every tick, every message)
- [ ] DEBUG: State changes, flow
- [ ] INFO: Login, disconnect, important events (production standard)
- [ ] WARNING: Timeouts, retries, anomalies
- [ ] ERROR: Errors, server continues
- [ ] FATAL: Critical, server stops

### What to log?
- [ ] Structured logging (Serilog)
- [ ] No sensitive data (passwords, tokens)
- [ ] Player IDs and session IDs for debugging
- [ ] Timestamps for all entries

### Performance
- [ ] No string interpolation in hot paths when log level is off
- [ ] Log level is configurable

---

## ⚡ Performance

### Game Loop
- [ ] No allocations in game loop (object pooling)
- [ ] Observe tick budget of 40ms
- [ ] No blocking I/O in tick

### Async/Await
- [ ] async/await used correctly (no .Result or .Wait())
- [ ] ConfigureAwait(false) in library code
- [ ] CancellationToken is passed through

### Memory
- [ ] Large objects are pooled
- [ ] No memory leaks (unsubscribe event handlers)
- [ ] Span<T> for buffer operations where possible

---

## 🎮 Godot Client (C#)

### Scene Structure
- [ ] Scene hierarchy from TECHNICAL_DESIGN.md followed?
- [ ] Main.tscn → Login.tscn → Game.tscn flow
- [ ] Player.tscn as prefab for players

### Autoloads (Singletons)
- [ ] NetworkManager for all network operations
- [ ] GameManager for game state
- [ ] No other global singletons without reason

### Signals
- [ ] Godot signals instead of direct method calls
- [ ] Events for UI updates
- [ ] Loose coupling between components

### Layers
- [ ] Layer 0: Ground (no collision)
- [ ] Layer 1: Collision (water, trees, walls)
- [ ] Layer 2: Entities (NPCs, interactives)
- [ ] Layer 3: Players
- [ ] Layer 4: UI (CanvasLayer)

### Input
- [ ] Input actions defined (not hardcoded keys)
- [ ] _Process for rendering, _PhysicsProcess for movement
- [ ] Input is sent to server, not processed locally (except prediction)

---

## 🗄️ Redis (Phase 2+)

### Key Schema
- [ ] Key naming from documentation followed?
- [ ] `session:{sessionId}`, `player:{playerId}`, etc.
- [ ] TTL for sessions (30 min)

### Pub/Sub
- [ ] Channels correct: `channel:zone:{zoneId}`, `channel:player:{playerId}`
- [ ] No large payloads over Pub/Sub

---

## 🗃️ Database (Phase 2+)

### Write Strategies
- [ ] IMMEDIATE: Character creation, item transactions, gold, level-up
- [ ] BATCHED (10-30s): Positions, HP/Mana, quest progress
- [ ] LOGOUT: Complete state

### Connection Pooling
- [ ] Npgsql connection pooling enabled
- [ ] MinPoolSize: 5, MaxPoolSize: 100

---

## 🎯 Prototype Scope

### In Scope
- [ ] Is the feature in the prototype scope (`docs/01-overview/PROTOTYPE_SCOPE.md`)?
- [ ] TCP Server/Client, MessagePack, game loop, movement, chat

### Out of Scope
- [ ] Combat system, NPCs, quests, inventory → Phase 2+
- [ ] Real auth, database, Redis, Azure → Phase 2+
- [ ] If out of scope: Is it marked as "later"?

---

## 📊 Tests

### Unit Tests
- [ ] New logic has unit tests?
- [ ] xUnit + Moq + FluentAssertions used?
- [ ] Tests are in the correct test project?

### What to test?
- [ ] Message serialization/deserialization
- [ ] Validation logic (movement, actions)
- [ ] Game loop logic (without I/O)

### Godot Tests
- [ ] GdUnit4 for client tests
- [ ] NetworkManager tests (with mocks)

---

## 📝 Feedback Format

For deviations from documentation:

1. **Quote** the relevant section from the documentation
2. **Explain** the deviation clearly and precisely
3. **Suggest** a concrete correction
4. **Reference** the relevant document with path

### Example:

> ⚠️ **Architecture Deviation**
> 
> According to `docs/03-technical-details/TECHNICAL_DESIGN.md`, MessagePack should be used for serialization:
> > "Serialization: MessagePack"
> 
> However, this code uses JSON:
> ```csharp
> var json = JsonSerializer.Serialize(message);
> ```
> 
> **Suggestion:** Use instead:
> ```csharp
> var bytes = MessagePackSerializer.Serialize(message);
> ```

---

## 🔗 Relevant Documentation

- [Documentation Index](docs/README.md) - Central overview
- [Architecture Overview](docs/02-architecture/README.md) - Technical architecture
- [Technical Design](docs/03-technical-details/TECHNICAL_DESIGN.md) - Detailed technical decisions
- [Prototype Scope](docs/01-overview/PROTOTYPE_SCOPE.md) - What is in the prototype
- [Game Design Document](docs/01-overview/GAME_DESIGN_DOCUMENT.md) - Gameplay design
- [Assets](docs/01-overview/ASSETS.md) - Asset specifications

---

*Diese Anweisungen helfen dabei, die Code-Qualität und Architektur-Konsistenz zu gewährleisten.*