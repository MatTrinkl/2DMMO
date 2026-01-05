# ☠️ Death Messages (4100-4199)

**Kategorie:** 41  
**Range:** 4100-4199  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [✅ Death/Downed State Machine](#-deathdowned-state-machine)
- [🧬 Penalties & Recovery](#-penalties--recovery)
- [🏁 Respawn Regeln](#-respawn-regeln)
- [✨ Resurrection Regeln](#-resurrection-regeln)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#-regeln--sicherheit)
- [📩 Aktive Messages 4100–4199](#-aktive-messages-4100–4199)
  - [DeathNotification (4100)](#deathnotification-4100)
  - [DeathRecap (4101)](#deathrecap-4101)
  - [GhostModeStart (4110)](#ghostmodestart-4110)
  - [GhostModeEnd (4111)](#ghostmodeend-4111)
  - [GhostPosition (4112)](#ghostposition-4112)
  - [CorpseLocation (4113)](#corpselocation-4113)
  - [CorpseRevive (4114)](#corpserevive-4114)
  - [RespawnRequest (4120)](#respawnrequest-4120)
  - [RespawnAtGraveyard (4121)](#respawnatgraveyard-4121)
  - [RespawnAtCheckpoint (4122)](#respawnatcheckpoint-4122)
  - [RespawnTimer (4123)](#respawntimer-4123)
  - [RespawnComplete (4124)](#respawncomplete-4124)
  - [ResurrectOffer (4130)](#resurrectoffer-4130)
  - [ResurrectAccept (4131)](#resurrectaccept-4131)
  - [ResurrectDecline (4132)](#resurrectdecline-4132)
  - [ResurrectComplete (4133)](#resurrectcomplete-4133)
  - [SoulstoneResurrect (4134)](#soulstoneresurrect-4134)
  - [BattleResurrect (4135)](#battleresurrect-4135)
  - [ReleaseSpirit (4140)](#releasespirit-4140)
  - [RetrieveCorpse (4141)](#retrievecorpse-4141)
  - [SpiritHealerRevive (4142)](#spirithealerrevive-4142)
  - [ResurrectionSickness (4143)](#resurrectionsickness-4143)
- [🗑️ Obsolete Messages](#-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang (MessageType Enum Updates)](#-anhang-messagetype-enum-updates)

---

## 📋 Überblick

Scope dieser Kategorie: **death, downed, respawn, resurrection, penalties**. Die Messages bilden den vollständigen Lebenszyklus eines Charakters im Kampf und außerhalb, inklusive Broadcasts an Mitspieler, UI-Rez-Flows, Anti-Exploit-Schutz und Recovery nach Reconnect. Jede Client→Server Nachricht hat eine korrelierbare Response (RequestId/ClientSequence). Server ist authoritative für Status-Transitions (Alive → Downed → Dead → Ghost → Alive).

- Abdeckung: PvE/PvP, Open World, Instanz, Duell, Raid.
- Integration: Combat (0300), Zone (0100), Inventory/Equipment (0500/3900), Buff/Aura (1500), Cooldowns (3200), Party (0700), Admin (2300) für GM-Resurrections.
- Netzwerk: MessagePack, feste Key-Order, Delta-Syncs für UI, reconnect-sicher durch Revision-IDs.

---

## 🧠 Datenmodell

**DeathStateDto**  
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Entity/Character Id | Ja |
| State | DeathState | Alive/Downed/Dead/Ghost/Respawning | Ja |
| HitTimestamp | long | Server Unix ms des tödlichen Treffers | Ja |
| KillerEntityId | long? | Letzter Aggressor (oder null) | Nein |
| KillerType | KillerType | Player/Npc/Environment | Ja |
| CorpseId | long? | Corpse Entity Id falls Dead | Nein |
| Revision | uint | Monotone State Revision für Deltas | Ja |
| DownedExpiresAt | long? | Unix ms wann Downed auto-zu-Dead | Nein |
| RespawnAvailableAt | long? | Unix ms ab wann Respawn erlaubt | Nein |
| Flags | DeathFlags | Bitmask für Sonderfälle | Ja |

**CorpseDto**  
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorpseId | long | Server-generierte Id | Ja |
| CharacterId | long | Zugehöriger Charakter | Ja |
| Position | Vector2 | Weltposition (X,Y) | Ja |
| ZoneId | ushort | Zone der Leiche | Ja |
| Lootable | bool | Kann von Gegnern gelootet werden (PvP/PK) | Ja |
| DecayAt | long | Unix ms wann Corpse despawnt | Ja |
| BindPointId | int? | Fallback Respawn Point | Nein |
| IsReleased | bool | Ob Spirit bereits freigesetzt | Ja |

**RespawnPointDto**  
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RespawnPointId | int | Graveyard/Checkpoint Id | Ja |
| ZoneId | ushort | Zone | Ja |
| Position | Vector2 | Spawn Position | Ja |
| Type | RespawnPointType | Graveyard/Checkpoint/InstanceEntrance/Bind | Ja |
| SafeBubbleRadius | float | Radius der Schutzblase | Ja |
| SafeBubbleDurationMs | int | Dauer der Unverwundbarkeit | Ja |
| RequiresCorpse | bool | Ob Corpse-Abholung nötig | Ja |

**ResurrectionOfferDto**  
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| OfferId | Guid | Eindeutige Offer-ID | Ja |
| CasterId | long | Rez-Anbieter (Player/NPC) | Ja |
| TargetId | long | Rez-Empfänger (Dead/Ghost) | Ja |
| SpellId | int | Quelle (Spell/Item) | Ja |
| ExpiresAt | long | Unix ms Expiry | Ja |
| Range | float | Maximale Distanz beim Offer | Ja |
| RequiresLoS | bool | Line-of-Sight Pflicht | Ja |
| Flags | ResurrectionOfferFlags | BattleRez/Soulstone/etc. | Ja |
| CooldownApplied | bool | Ob Cooldown bereits gebucht | Ja |
| AllowedStates | DeathState[] | Erlaubte Ziel-States (Dead/Ghost) | Ja |

**GhostStateDto**  
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Geist-Charakter | Ja |
| CorpseId | long? | Ziel-Leiche | Nein |
| ZoneId | ushort | Zone | Ja |
| Position | Vector2 | Ghost Position | Ja |
| MovementSpeed | float | Angepasste Ghost-Speed | Ja |
| Visibility | GhostVisibility | Wer sieht den Geist | Ja |
| Timestamp | long | Server-Zeit des Snapshots | Ja |

**Vector2**  
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| X | float | Weltkoordination X |
| Y | float | Weltkoordination Y |

---

## ✅ Death/Downed State Machine

**States:** Alive → Downed → Dead → Ghost → Respawning → Alive  
Sonderpfad: Alive → Dead (One-Shot, z. B. Fall Damage)  
Sonderpfad: Dead → Alive (Instant GM/Script Resurrect)

**Transitions & Timer**

- Alive → Downed: HP ≤ 0, aber DownedShield aktiv. `DownedExpiresAt` startet (standard 15s). Bewegung eingeschränkt, Skills begrenzt.
- Downed → Dead: Timer abgelaufen oder Finishing Hit. Corpse wird erzeugt, DeathNotification broadcast.
- Downed → Alive: CombatRez (BattleResurrect) erfolgreich, innerhalb Timer.
- Alive → Dead: One-Shot (z. B. KillVolume, Execute). Corpse sofort erstellt.
- Dead → Ghost: Client sendet `ReleaseSpirit (4140)` oder Auto-Release nach Timeout; GhostModeStart gesendet.
- Ghost → Respawning: `RespawnRequest (4120)` oder `RetrieveCorpse (4141)` success oder `SpiritHealerRevive (4142)`.
- Respawning → Alive: `RespawnComplete (4124)` mit Invulnerability-Bubble.

**Authority & Validation**

- Server bestimmt Transition; Client zeigt nur UI.  
- Jede Transition erhöht Revision.  
- Downed/Dead Timer serverseitig, keine Client-Seitige Manipulation.  
- Resurrection Offer nur gültig, wenn Target State Dead oder Ghost und nicht bereits Respawning.  
- Respawn Location wird vom Server validiert (navmesh, safe area).

**ASCII State Machine**

```
Alive --(HP<=0, DownedEnabled)--> Downed --(timer/finish)--> Dead --(ReleaseSpirit)--> Ghost
   \                                      \                                   \
    \--(one-shot/kill volume)--> Dead      \--(BattleRez)--> Alive             \--(RespawnRequest/RetrieveCorpse/SpiritHealer)--> Respawning --> Alive
```

---

## 🧬 Penalties & Recovery

- **Durability Loss:** Standard 10% Durability on equipped items on Death. Additional 10% wenn SpiritHealerRevive genutzt (ResurrectionSickness).
- **XP Debt:** Optional XP-Schulden werden aufgezeichnet (nicht verfallen). Bei DeathEvent wird XP Debt berechnet, `DeathRecap` enthält Delta. Wird über Zeit beim Spielen abgebaut.
- **Death Timer:** `RespawnAvailableAt` sperrt Früh-Respawn (PvP Anti-Spam). PvE Default 10s, PvP 20s, Boss Wipe 30s.
- **Debuffs:** `ResurrectionSickness (4143)` angewandt bei SpiritHealer oder BindPoint-Notfallspawn.
- **Inventory Safety:** Keine Items droppen (Prototyp). In PvP-Flagged Zonen: optional Lootable Corpse via `Lootable=true`.
- **Recovery Integration:** Buffs (1500) entfernt beim Death; persistent Auras mit `PersistThroughDeath` Flag bleiben.

---

## 🏁 Respawn Regeln

- **RespawnPoint Auswahl:** Reihenfolge: ActiveCheckpoint (Instance/Encounter) → BindPoint/Inn → Graveyard im selben SubZone → Zone-Graveyard → Fallback InstanceEntrance.  
- **Safe Spawn Bubble:** Radius ≥ 6m, Dauer 5s; bricht bei offensiver Aktion, nicht bei Bewegung. Server markiert mit Aura `Invulnerable` und `NoCollisionDamage`.
- **Position Validation:** Server snappt Position auf NavMesh, prüft Anti-Teleport (max 15m Abweichung von Point), blockiert falls Combat-State noch aktiv (PvP-Exploit-Schutz).
- **Ghost Walk:** Ghost darf nur in erlaubter Zone bewegen; Cross-Zone Teleport via `RespawnAtGraveyard` wenn Corpse in anderer Zone.
- **Corpse Retrieval:** `RetrieveCorpse (4141)` teleportiert Ghost zur Corpse-Position ±1m; prüft Hindernisse und Line-of-Sight. Gelingt nur wenn Corpse existiert und nicht decayed.
- **Bind Point:** `InnkeeperBindResult (1341)` setzt RespawnPointType Bind. Wird bei Respawn bevorzugt, falls in gleicher Welt-Region.

---

## ✨ Resurrection Regeln

- **Offers:** Nur Heiler-Klassen/Items/NPCs erzeugen `ResurrectOffer (4130)` Events. Offer enthält Range, LoS, Expiry (8s PvE, 5s PvP), Flags (BattleRez/Soulstone).
- **Accept/Decline:** Client sendet `ResurrectAccept (4131)` oder `ResurrectDecline (4132)` mit OfferId und optional ClientSequence. Idempotent: Wiederholte Accept/Decline nach Abschluss werden mit `ErrorCode=ALREADY_RESOLVED` beantwortet.
- **Range/LoS Checks:** Server prüft Caster↔Corpse Distanz (≤ Range) und Line-of-Sight bei Accept. Offer kann serverseitig gecancelt, dann `ResurrectComplete` mit `Success=false`.
- **Cooldowns:** BattleResurrect hat Encounter-basierten Cooldown. Soulstone verzehrt sich beim Auto-Res. SpiritHealer frei von Offer-Flow.
- **Combat Restrictions:** PvP: Rez im Combat nur wenn Flag `BattleRez` gesetzt. PvE Boss: limitierte Anzahl pro Encounter.
- **Immunität nach Rez:** 3s Unverwundbarkeit + 50% HP/MP baseline; kann durch Spell definiert werden.

---

## 🔄 Sync, Deltas & Revisioning

- **DeathState Revision:** Jede Statusänderung erhöht `Revision`. `DeathNotification` und `DeathStateSync` Felder enthalten Revision. Clients discard ältere Revisions.
- **Snapshot vs Delta:** `DeathNotification` liefert Full Snapshot; `RespawnTimer` sendet nur Timer-Deltas. Bei Reconnect sendet Client `DeathStateSyncRequest` (implizit via `RespawnRequest` mit `KnownRevision`). Server antwortet mit vollständigem Snapshot falls mismatch.
- **Reliable Delivery:** Alle Messages sind reliable (TCP). Client muss dennoch Duplikate tolerieren (idempotent Accept/Decline).
- **Sequencing:** Request/Response koppeln über `ClientSequence` oder `RequestId`. Responses spiegeln Sequenz zurück.
- **Reconnect-Safe:** Bei Reconnect während Dead/Ghost: Server sendet `DeathNotification` + `CorpseLocation` + `RespawnTimer` erneut.

### Multi-Step Flows (ASCII)

**Death Broadcast → DeathState Sync**
```
Server (Combat)                Clients in Zone
   │ Damage/Death Event         │
   │────────────────────────────▶
   │ DeathNotification (4100)   │
   │────────────────────────────▶
   │ RespawnTimer (4123)        │
   │────────────────────────────▶
```

**Resurrection Offer → Accept/Decline → Completion**
```
Caster          Server                     Target
  │ Cast Rez      │                         │
  │──────────────►│                         │
  │               │ ResurrectOffer (4130)   │
  │               │────────────────────────►│
  │               │ ResurrectAccept/Decline │
  │               │◄────────────────────────│
  │               │ ResurrectComplete (4133)│
  │◄──────────────│─────────────────────────│
```

**Respawn → Spawn → Position Sync**
```
Client (Ghost)                 Server
   │ RespawnRequest (4120)      │
   │───────────────────────────►│
   │ RespawnAtGraveyard/CP      │
   │◄───────────────────────────│
   │ RespawnComplete (4124)     │
   │◄───────────────────────────│
   │ Movement/ZoneDelta follow  │
```

**Reconnect im Death-State**
```
Client (Reconnect)             Server
   │ Login/Reconnect Flow       │
   │                            │
   │ RespawnRequest (KnownRev)  │
   │───────────────────────────►│
   │ DeathNotification (4100)   │
   │◄───────────────────────────│
   │ CorpseLocation (4113)      │
   │◄───────────────────────────│
   │ RespawnTimer (4123)        │
   │◄───────────────────────────│
```

---

## 🧱 DTOs / Interfaces

```csharp
[MessagePackObject]
public class DeathStateDto
{
    [Key(0)] public MessageType Type => MessageType.DeathNotification; // Nur bei Messages nutzen
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public DeathState State { get; set; }
    [Key(3)] public long HitTimestamp { get; set; }
    [Key(4)] public long? KillerEntityId { get; set; }
    [Key(5)] public KillerType KillerType { get; set; }
    [Key(6)] public long? CorpseId { get; set; }
    [Key(7)] public uint Revision { get; set; }
    [Key(8)] public long? DownedExpiresAt { get; set; }
    [Key(9)] public long? RespawnAvailableAt { get; set; }
    [Key(10)] public DeathFlags Flags { get; set; }
}

[MessagePackObject]
public class CorpseDto
{
    [Key(0)] public long CorpseId { get; set; }
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public Vector2 Position { get; set; }
    [Key(3)] public ushort ZoneId { get; set; }
    [Key(4)] public bool Lootable { get; set; }
    [Key(5)] public long DecayAt { get; set; }
    [Key(6)] public int? BindPointId { get; set; }
    [Key(7)] public bool IsReleased { get; set; }
}
```

Alle DTOs nutzen **stabile Key-Order**, `Type` immer Key(0) bei Messages. Interfaces: `IClientMessage` für Client→Server, `IServerMessage` für Server→Client, `INetworkMessage` für gemeinsame Basis.

---

## 🧩 Enums / ErrorCodes / Flags

```csharp
public enum DeathState : byte
{
    Alive = 0,
    Downed = 1,
    Dead = 2,
    Ghost = 3,
    Respawning = 4
}

public enum KillerType : byte
{
    Unknown = 0,
    Player = 1,
    Npc = 2,
    Environment = 3,
    Trap = 4,
    Scripted = 5
}

[Flags]
public enum DeathFlags : byte
{
    None = 0,
    PvP = 1 << 0,
    Hardcore = 1 << 1,
    InInstance = 1 << 2,
    BattleResImmune = 1 << 3,
    SoulstoneAvailable = 1 << 4,
    ForcedRelease = 1 << 5
}

public enum RespawnPointType : byte
{
    Graveyard = 0,
    Checkpoint = 1,
    InstanceEntrance = 2,
    BindPoint = 3
}

public enum GhostVisibility : byte
{
    SelfAndParty = 0,
    PartyAndRaid = 1,
    OnlySelf = 2,
    NearbyGhosts = 3
}

[Flags]
public enum ResurrectionOfferFlags : byte
{
    None = 0,
    BattleRez = 1 << 0,
    Soulstone = 1 << 1,
    Instant = 1 << 2,
    RequiresCorpse = 1 << 3,
    RequiresLineOfSight = 1 << 4
}
```

**Error Codes (Death Range)**

| Code | Bedeutung |
|------|-----------|
| `RESPAWN_LOCKED` | Respawn vor `RespawnAvailableAt` angefragt |
| `NO_CORPSE` | Corpse existiert nicht oder decayed |
| `INVALID_OFFER` | OfferId unbekannt oder abgelaufen |
| `OUT_OF_RANGE` | Distanz/LoS verletzt |
| `ALREADY_ALIVE` | Target ist nicht Dead/Ghost |
| `ALREADY_RESOLVED` | Offer bereits angenommen/abgelehnt |
| `NOT_OWNER` | Client nicht Besitzer der Corpse |
| `IN_COMBAT` | Respawn/Rez im unzulässigen Combat-Status |
| `RATE_LIMITED` | Zu viele Respawn Requests |
| `LOCKED_BY_ENCOUNTER` | Encounter-Mechanik blockiert Respawn |

---

## ⚙️ Regeln & Sicherheit

- **Anti-Exploit:**  
  - Position Validation bei Respawn & RetrieveCorpse (NavMesh, no-clipping).  
  - Rate Limit: RespawnRequest max 1 alle 3s; ResurrectAccept/Decline max 1/s.  
  - LoS & Range Checks bei Rez; serverseitiges Audit-Log mit CasterId/SpellId.  
  - Prevent corpse dragging exploit: CorpseLocation immutable nach Erstellung.
- **Idempotency:**  
  - Accept/Decline mehrfach erlaubt, Server antwortet deterministisch mit letztem Status.  
  - RespawnRequest mit gleicher `ClientSequence` → gleiche Response.
- **Consistency:**  
  - Revision-basierte Deltas; Client verwirft alte Revisionen.  
  - All server events (DeathNotification, RespawnTimer) enthalten `Revision` und `Timestamp`.
- **Privacy:**  
  - DeathRecap enthält keine genauen Gegnernamen im Hardcore/PvP-Privacy-Modus (nur Typ).  
  - GhostVisibility kontrolliert, wer den Geist sieht.
- **Integration:**  
  - CombatLog (320) ergänzt Entries mit DeathRecap.  
  - Party (0700) erhält Marker-Updates bei Death/Ghost.  
  - Buffs (1500) reset außer persistenten Auren.  
  - Cooldowns (3200) pausieren nicht; Rez-Spezifische Cooldowns werden gesetzt.

---

## 📩 Aktive Messages 4100–4199

### DeathNotification (4100)

**Richtung:** 📡 Event (Server → Client Broadcast)  
**Frequenz:** Mittel (bei jedem Tod)  
**Authentifizierung:** 🔒 Ja (Session)  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server-broadcastetes Death-Snapshot, wenn ein Character in der Zone stirbt oder downed wird. Enthält vollständigen DeathState inkl. Revision, Killer-Info, CorpseId und Respawn-Lock. Dient als Basis für UI, CombatLog, Party Frames und Anti-Cheat Audits.

### Im Scope ✅
- Zonenweiter Broadcast (Sichtbarkeits-Filtered)
- Downed/Dead Distinktion
- Killer-Attribution inkl. Typ

### Nicht im Scope ❌
- Respawn-Ort Auswahl (separate Messages)
- Loot/Victim Drops (Inventory Range)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DeathNotification` | Ja |
| CharacterId | long | Gestorbener Charakter | Ja |
| DeathState | DeathState | Downed/Dead | Ja |
| HitTimestamp | long | Unix ms | Ja |
| KillerEntityId | long? | Aggressor | Nein |
| KillerType | KillerType | Player/Npc/... | Ja |
| CorpseId | long? | Zugehörige Corpse | Nein |
| Revision | uint | DeathState Revision | Ja |
| DownedExpiresAt | long? | Timer-Ende bei Downed | Nein |
| RespawnAvailableAt | long? | Sperre für Respawn | Nein |
| Flags | DeathFlags | Kontext-Flags | Ja |

### Erwartete Response
- Keine (Broadcast). Clients können optional `DeathRecap (4101)` anfordern, falls nicht automatisch folgt.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DeathNotification)]
public class DeathNotification : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.DeathNotification;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public DeathState DeathState { get; set; }
    [Key(3)] public long HitTimestamp { get; set; }
    [Key(4)] public long? KillerEntityId { get; set; }
    [Key(5)] public KillerType KillerType { get; set; }
    [Key(6)] public long? CorpseId { get; set; }
    [Key(7)] public uint Revision { get; set; }
    [Key(8)] public long? DownedExpiresAt { get; set; }
    [Key(9)] public long? RespawnAvailableAt { get; set; }
    [Key(10)] public DeathFlags Flags { get; set; }
}
```

### Server-Verhalten
- Erzeugt Corpse, speichert Position, setzt Decay Timer.
- Broadcast an Sichtbarkeits-Cluster; Party/Raid immer informiert.
- Startet RespawnAvailable Timer; sendet `RespawnTimer (4123)` tick-basiert.
- Loggt Killer Attribution für Achievements/PvP Honor.

### Client-Verhalten
- UI: Zeigt Downed/Dead Panel, Countdown, Rez-Angebot Platzhalter.
- Stoppt Movement/Abilities; nur erlaubte Downed-Actions aktiv.
- Speichert Revision; spätere Deltas nur anwenden, wenn Revision höher.

### Flow-Diagramm

```
Client A/B/C                   Server
   │                             │
   │                             │ Damage Event
   │                             │───────────────
   │                             │ Create Corpse
   │                             │
   │ DeathNotification (4100)    │
   │◄────────────────────────────│
   │                             │
```

### Beispiel Payloads

```csharp
var notif = new DeathNotification
{
    CharacterId = 9001,
    DeathState = DeathState.Dead,
    HitTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    KillerEntityId = 5002,
    KillerType = KillerType.Npc,
    CorpseId = 12001,
    Revision = 42,
    RespawnAvailableAt = DateTimeOffset.UtcNow.AddSeconds(12).ToUnixTimeMilliseconds(),
    Flags = DeathFlags.InInstance
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Broadcast hat keine Error Codes |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `DeathRecap` | 4101 | Detail-Recap optional |
| `RespawnTimer` | 4123 | Timer Update |
| `CorpseLocation` | 4113 | Folge-Info für Ghost |

---

### DeathRecap (4101)

**Richtung:** 📥 Server → Client (auf Anfrage oder auto)  
**Frequenz:** Mittel (nach Death)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Liefert detaillierten Schadens-Recap der letzten X Sekunden vor dem Tod (Standard 10 Events). Dient für UI "Warum bin ich gestorben?" und Audit. Kann auto gesendet werden oder als Antwort auf implizite Anfrage (DeathNotification).

### Im Scope ✅
- Letzte Treffer mit Quelle, Schaden, Typ
- Mit/ohne Resist/Block/Absorb
- Zusammenfassung (Top Source, Damage per Type)

### Nicht im Scope ❌
- Vollständiges CombatLog (separat)
- Future Predictions/Analytics

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DeathRecap` | Ja |
| CharacterId | long | Betroffener Charakter | Ja |
| Revision | uint | Revision identisch zu DeathNotification | Ja |
| Entries | List<DeathRecapEntryDto> | Trefferliste | Ja |
| TotalDamage | int | Summe Schaden | Ja |
| DurationMs | int | Betrachtetes Zeitfenster | Ja |

**DeathRecapEntryDto**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Timestamp | long | Unix ms |
| SourceId | long | Aggressor |
| SourceType | KillerType | Typ |
| SpellId | int | Fähigkeit/Item |
| Amount | int | Schaden |
| Overkill | int | Überkill |
| Mitigation | int | Block/Absorb |
| DamageType | byte | Elementar/Physisch |

### Erwartete Response
- Keine (Response auf implizite Anfrage); wenn separat angefragt, ist dies die Response.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DeathRecap)]
public class DeathRecap : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.DeathRecap;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public List<DeathRecapEntryDto> Entries { get; set; } = new();
    [Key(4)] public int TotalDamage { get; set; }
    [Key(5)] public int DurationMs { get; set; }
}
```

### Server-Verhalten
- Generiert aus CombatLog Buffer; capped auf 20 Einträge.
- Redigiert SourceId/SpellId je nach Privacy (Hardcore/PvP).
- Sendet automatisch an Opfer; Party-Anzeige optional.

### Client-Verhalten
- Zeigt Liste mit Zeitstrahl, markiert Killing Blow.
- Speichert lokal für "Show Recap" Button.
- Verwirft wenn Revision ungleich aktuell.

### Flow-Diagramm

```
Client                       Server
  │                            │
  │ DeathNotification (4100)   │
  │◄───────────────────────────│
  │                            │ Build recap
  │ DeathRecap (4101)          │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var recap = new DeathRecap
{
    CharacterId = 9001,
    Revision = 42,
    TotalDamage = 1280,
    DurationMs = 8000,
    Entries =
    {
        new DeathRecapEntryDto { Timestamp = now-3000, SourceId = 5002, SourceType = KillerType.Npc, SpellId = 12001, Amount = 400, Overkill = 0, Mitigation = 50, DamageType = 1 },
        new DeathRecapEntryDto { Timestamp = now-500, SourceId = 5002, SourceType = KillerType.Npc, SpellId = 12002, Amount = 950, Overkill = 70, Mitigation = 0, DamageType = 1 }
    }
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `REVISION_MISMATCH` | Client wollte ältere Revision |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `DeathNotification` | 4100 | Basis-Event |
| `CombatLogEntry` | 320 | Quelle der Daten |

---

### GhostModeStart (4110)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (bei Release)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet den Geistmodus nach `ReleaseSpirit`. Setzt Ghost-Visuals, MovementSpeed, Visibility und bindet CorpseId. Wird nur an den betroffenen Client gesendet.

### Im Scope ✅
- GhostState initial
- MovementSpeed Anpassung
- Link zur Corpse

### Nicht im Scope ❌
- RespawnPoint Auswahl (separat)
- Party Broadcast (Party erhält DeathNotification, nicht GhostModeStart)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GhostModeStart` | Ja |
| CharacterId | long | Spieler | Ja |
| CorpseId | long? | Ziel-Corpse | Nein |
| ZoneId | ushort | Aktuelle Zone | Ja |
| Position | Vector2 | Ghost Start | Ja |
| MovementSpeed | float | Ghost Speed | Ja |
| Visibility | GhostVisibility | Wer sieht den Geist | Ja |
| Revision | uint | DeathState Revision | Ja |

### Erwartete Response
- Keine. Folgemessages: `GhostPosition (4112)` heartbeat.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GhostModeStart)]
public class GhostModeStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GhostModeStart;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public long? CorpseId { get; set; }
    [Key(3)] public ushort ZoneId { get; set; }
    [Key(4)] public Vector2 Position { get; set; }
    [Key(5)] public float MovementSpeed { get; set; }
    [Key(6)] public GhostVisibility Visibility { get; set; }
    [Key(7)] public uint Revision { get; set; }
}
```

### Server-Verhalten
- Setzt Ghost Flags auf Entity, disables Collision mit Lebenden.
- Startet Ghost heartbeat (optional).
- Passt MovementSpeed an (z. B. 120% run speed).

### Client-Verhalten
- Aktiviert Graustufen/Shader, deaktiviert Combat UI.
- Setzt MovementSpeed lokal, sendet `GhostPosition` Updates.
- Zeigt Corpse-Marker auf Map.

### Flow-Diagramm

```
Client                        Server
  │ ReleaseSpirit (4140)       │
  │───────────────────────────►│
  │                            │
  │  GhostModeStart (4110)     │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var start = new GhostModeStart
{
    CharacterId = 9001,
    CorpseId = 12001,
    ZoneId = 1001,
    Position = new Vector2 { X = 102.5f, Y = 84.3f },
    MovementSpeed = 6.8f,
    Visibility = GhostVisibility.SelfAndParty,
    Revision = 43
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `ReleaseSpirit` | 4140 | Request, der GhostModeStart auslöst |
| `GhostPosition` | 4112 | Positions-Heartbeat |
| `GhostModeEnd` | 4111 | Beendet Ghost |

---

### GhostModeEnd (4111)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (bei Respawn/Rez)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Signalisiert das Ende des Geistmodus (durch Respawn, Rez, CorpseRetrieve). Entfernt Ghost-Visuals und stoppt GhostPosition Heartbeats.

### Im Scope ✅
- Ghost deaktivieren
- Corpse-Link lösen

### Nicht im Scope ❌
- Respawn-Position (kommt via RespawnComplete)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GhostModeEnd` | Ja |
| CharacterId | long | Spieler | Ja |
| Revision | uint | Neue Revision | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GhostModeEnd)]
public class GhostModeEnd : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GhostModeEnd;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public uint Revision { get; set; }
}
```

### Server-Verhalten
- Entfernt Ghost Flags, stoppt Ghost heartbeat.
- Sendet nachfolgend `RespawnComplete` oder `ResurrectComplete`.

### Client-Verhalten
- Schaltet Visuals zurück, aktiviert normale UI.
- Stoppt GhostPosition Sender.

### Flow-Diagramm

```
Client                         Server
  │                             │
  │ RespawnComplete (4124)      │
  │◄────────────────────────────│
  │                             │
  │ GhostModeEnd (4111)         │
  │◄────────────────────────────│
```

### Beispiel Payloads

```csharp
var end = new GhostModeEnd { CharacterId = 9001, Revision = 44 };
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `GhostModeStart` | 4110 | Vorgänger |
| `RespawnComplete` | 4124 | Hauptabschluss |

---

### GhostPosition (4112)

**Richtung:** 📤 Client → Server (Heartbeat)  
**Frequenz:** Häufig (10 Hz)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Position-Updates des Geistes für Server-Validierung und Map-Sync. Wird nur im Ghost-Mode gesendet. Server validiert Speed und Bounds, um Teleport-Exploit zu verhindern.

### Im Scope ✅
- Ghost Position und Facing
- Server Validierung

### Nicht im Scope ❌
- Alive Movement (Movement Range 0200)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GhostPosition` | Ja |
| CharacterId | long | Spieler | Ja |
| Position | Vector2 | Ghost Pos | Ja |
| Facing | float | Blickrichtung | Ja |
| Timestamp | long | Client Timestamp | Ja |

### Erwartete Response
- Keine direkte Response. Server kann `GhostModeEnd` oder Korrektur via `MovementCorrection (202)` senden.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GhostPosition)]
public class GhostPosition : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.GhostPosition;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public Vector2 Position { get; set; }
    [Key(3)] public float Facing { get; set; }
    [Key(4)] public long Timestamp { get; set; }
}
```

### Server-Verhalten
- Validiert Speed (max GhostSpeed * 1.1).
- Teleport-Detect: Abweichung > 10m → snap zurück, loggt AntiCheat.
- Aktualisiert Corpse Marker Distanz für UI.

### Client-Verhalten
- Sendet 10 Hz, pausiert bei Idle.
- Erwartet keine Ack; toleriert Packet Loss.

### Flow-Diagramm

```
Client (Ghost)                 Server
  │ GhostPosition (4112)        │
  │────────────────────────────►│
  │ ... (repeated)              │
```

### Beispiel Payloads

```csharp
var ghostPos = new GhostPosition
{
    CharacterId = 9001,
    Position = new Vector2 { X = 110.3f, Y = 90.2f },
    Facing = 1.57f,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `TELEPORT_DETECTED` | Server snappt Position |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `GhostModeStart` | 4110 | Aktiviert Sender |
| `MovementCorrection` | 202 | Korrekturpfad |

---

### CorpseLocation (4113)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nach Death/Release)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Gibt exakte Position der Corpse und Zone an. Wird an Besitzer gesendet sowie an Gruppenmitglieder für Minimap-Ping.

### Im Scope ✅
- Corpse Position/Zone
- Decay Timer

### Nicht im Scope ❌
- Lootinformationen (Inventory Range)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `CorpseLocation` | Ja |
| CorpseId | long | Corpse | Ja |
| CharacterId | long | Besitzer | Ja |
| ZoneId | ushort | Zone | Ja |
| Position | Vector2 | Position | Ja |
| DecayAt | long | Unix ms | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CorpseLocation)]
public class CorpseLocation : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CorpseLocation;
    [Key(1)] public long CorpseId { get; set; }
    [Key(2)] public long CharacterId { get; set; }
    [Key(3)] public ushort ZoneId { get; set; }
    [Key(4)] public Vector2 Position { get; set; }
    [Key(5)] public long DecayAt { get; set; }
}
```

### Server-Verhalten
- Sendet nach DeathNotification und nach ReleaseSpirit.
- Aktualisiert Decay Timer bei Reconnect.

### Client-Verhalten
- Zeigt Map Marker; Pathfinding optional.
- Warnt kurz vor Decay.

### Flow-Diagramm

```
Client                        Server
  │ ReleaseSpirit (4140)       │
  │───────────────────────────►│
  │                            │
  │ CorpseLocation (4113)      │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var loc = new CorpseLocation
{
    CorpseId = 12001,
    CharacterId = 9001,
    ZoneId = 1001,
    Position = new Vector2 { X = 103, Y = 88 },
    DecayAt = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeMilliseconds()
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `DeathNotification` | 4100 | Erstinformation |
| `RetrieveCorpse` | 4141 | Nutzt CorpseId |

---

### CorpseRevive (4114)

**Richtung:** 📡 Event (Server → Client Besitzer)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server teilt mit, dass Corpse per Mechanik (Script, Timer, GM) wiederbelebt wurde und Spieler direkt an Corpse aufsteht. Dient für Spezial-Encounter.

### Im Scope ✅
- Auto-Revive am Corpse
- Entfernt Corpse Entity

### Nicht im Scope ❌
- Spielerinitiierte Respawn (RespawnRequest)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `CorpseRevive` | Ja |
| CharacterId | long | Spieler | Ja |
| CorpseId | long | Corpse | Ja |
| Position | Vector2 | Aufsteh-Position | Ja |
| Revision | uint | Neue Revision | Ja |

### Erwartete Response
- Keine. Folge: `RespawnComplete` wird gesendet.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CorpseRevive)]
public class CorpseRevive : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CorpseRevive;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public long CorpseId { get; set; }
    [Key(3)] public Vector2 Position { get; set; }
    [Key(4)] public uint Revision { get; set; }
}
```

### Server-Verhalten
- Entfernt Corpse vom Server, setzt Alive State.
- Sendet `RespawnComplete` mit Invulnerability.

### Client-Verhalten
- Stoppt Ghost UI (falls aktiv).
- Teleportiert zu Position, spielt Animation.

### Flow-Diagramm

```
Client                        Server
  │                            │ Script Trigger
  │                            │───────────────
  │ CorpseRevive (4114)        │
  │◄───────────────────────────│
  │ RespawnComplete (4124)     │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var revive = new CorpseRevive
{
    CharacterId = 9001,
    CorpseId = 12001,
    Position = new Vector2 { X = 104f, Y = 87.5f },
    Revision = 45
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `RespawnComplete` | 4124 | Abschluss |

---

### RespawnRequest (4120)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client fordert Respawn an (Ghost oder Dead mit Auto-Respawn). Server wählt geeigneten RespawnPoint und bestätigt mit `RespawnComplete`. Request enthält gewünschte Präferenz (Graveyard/Checkpoint/Bind) und bekannte Revision.

### Im Scope ✅
- Respawn Initiierung
- Revision Sync
- Auswahlpräferenz

### Nicht im Scope ❌
- Corpse Retrieval (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `RespawnRequest` | Ja |
| CharacterId | long | Spieler | Ja |
| PreferredPointType | RespawnPointType? | Wunsch-Point | Nein |
| KnownRevision | uint | Client bekannte Death-Revision | Ja |
| ClientSequence | uint | Für Response-Korrelation | Ja |

### Erwartete Response
- `RespawnComplete (4124)` (Success/Failure) mit gleicher ClientSequence.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RespawnRequest)]
public class RespawnRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.RespawnRequest;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public RespawnPointType? PreferredPointType { get; set; }
    [Key(3)] public uint KnownRevision { get; set; }
    [Key(4)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Prüft RespawnAvailableAt; bei Verstoß → `Success=false, ErrorCode=RESPAWN_LOCKED`.
- Wählt RespawnPoint gemäß Regeln; validiert NavMesh.
- Erhöht Revision bei Erfolg, setzt Invulnerability Aura.

### Client-Verhalten
- Sperrt Button bis Response.
- Bei Error zeigt UI Timer/Fehler.

### Flow-Diagramm

```
Client                        Server
  │ RespawnRequest (4120)      │
  │───────────────────────────►│
  │                            │ Validate + choose point
  │ RespawnComplete (4124)     │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var req = new RespawnRequest
{
    CharacterId = 9001,
    PreferredPointType = RespawnPointType.Graveyard,
    KnownRevision = 45,
    ClientSequence = 7
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `RESPAWN_LOCKED` | Zu früh |
| `LOCKED_BY_ENCOUNTER` | Encounter verhindert |
| `IN_COMBAT` | PvP-Lock |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `RespawnComplete` | 4124 | Response |
| `RespawnTimer` | 4123 | Info zu Lock |

---

### RespawnAtGraveyard (4121)

**Richtung:** 📡 Event (Server → Client)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server teilt mit, dass der ausgewählte RespawnPoint ein Graveyard ist. Dient für UI-Text/Loading. Wird im Rahmen des `RespawnComplete` Flows gesendet.

### Im Scope ✅
- Graveyard-Identifikation
- Position/Zone Info

### Nicht im Scope ❌
- Abschluss des Respawns (RespawnComplete)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `RespawnAtGraveyard` | Ja |
| CharacterId | long | Spieler | Ja |
| RespawnPoint | RespawnPointDto | Ziel | Ja |
| ClientSequence | uint | Korrelations-Id | Ja |

### Erwartete Response
- Keine separate Response; `RespawnComplete` folgt.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RespawnAtGraveyard)]
public class RespawnAtGraveyard : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.RespawnAtGraveyard;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public RespawnPointDto RespawnPoint { get; set; } = null!;
    [Key(3)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Sendet direkt vor Position-Set.
- Markiert SafeBubble je nach RespawnPoint.

### Client-Verhalten
- Zeigt Loading/Graveyard-Text.
- Wartet auf RespawnComplete für finalen Spawn.

### Flow-Diagramm

```
Client                        Server
  │ RespawnRequest (4120)      │
  │───────────────────────────►│
  │                            │
  │ RespawnAtGraveyard (4121)  │
  │◄───────────────────────────│
  │ RespawnComplete (4124)     │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var gy = new RespawnAtGraveyard
{
    CharacterId = 9001,
    RespawnPoint = new RespawnPointDto
    {
        RespawnPointId = 300,
        ZoneId = 1001,
        Position = new Vector2 { X = 50, Y = 120 },
        Type = RespawnPointType.Graveyard,
        SafeBubbleRadius = 6f,
        SafeBubbleDurationMs = 5000,
        RequiresCorpse = false
    },
    ClientSequence = 7
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `RespawnRequest` | 4120 | Auslöser |
| `RespawnComplete` | 4124 | Abschluss |

---

### RespawnAtCheckpoint (4122)

**Richtung:** 📡 Event (Server → Client)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Wie RespawnAtGraveyard, aber für aktive Checkpoints/Instance Entrances. Priorisiert wenn Encounter Checkpoint gesetzt.

### Im Scope ✅
- Checkpoint Info

### Nicht im Scope ❌
- Graveyard (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `RespawnAtCheckpoint` | Ja |
| CharacterId | long | Spieler | Ja |
| RespawnPoint | RespawnPointDto | Checkpoint | Ja |
| ClientSequence | uint | Korrelations-Id | Ja |

### Erwartete Response
- `RespawnComplete (4124)`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RespawnAtCheckpoint)]
public class RespawnAtCheckpoint : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.RespawnAtCheckpoint;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public RespawnPointDto RespawnPoint { get; set; } = null!;
    [Key(3)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Wählt Checkpoint, sendet vor Teleport.

### Client-Verhalten
- Zeigt Checkpoint Banner, lädt Assets falls andere SubZone.

### Flow-Diagramm

```
Client                        Server
  │ RespawnRequest (4120)      │
  │───────────────────────────►│
  │ RespawnAtCheckpoint (4122) │
  │◄───────────────────────────│
  │ RespawnComplete (4124)     │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var cp = new RespawnAtCheckpoint
{
    CharacterId = 9001,
    RespawnPoint = new RespawnPointDto
    {
        RespawnPointId = 901,
        ZoneId = 3001,
        Position = new Vector2 { X = 10, Y = 15 },
        Type = RespawnPointType.Checkpoint,
        SafeBubbleRadius = 8f,
        SafeBubbleDurationMs = 5000,
        RequiresCorpse = false
    },
    ClientSequence = 7
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `RespawnRequest` | 4120 | Auslöser |
| `RespawnComplete` | 4124 | Abschluss |

---

### RespawnTimer (4123)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel (1 Hz)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Timer-Update während Respawn Lock. Hält Client UI in Sync und dient als Delta (nur Timer-Felder). Wird beendet, sobald RespawnAvailable erreicht oder RespawnComplete gesendet.

### Im Scope ✅
- Countdown Updates
- Revision Sync für Timer

### Nicht im Scope ❌
- Respawn Start (RespawnRequest)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `RespawnTimer` | Ja |
| CharacterId | long | Spieler | Ja |
| RespawnAvailableAt | long | Unix ms | Ja |
| Revision | uint | Death Revision | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RespawnTimer)]
public class RespawnTimer : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.RespawnTimer;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public long RespawnAvailableAt { get; set; }
    [Key(3)] public uint Revision { get; set; }
}
```

### Server-Verhalten
- Sendet jede Sekunde, endet bei Unlock.

### Client-Verhalten
- Aktualisiert UI Timer; verhindert Button vor Unlock.

### Flow-Diagramm

```
Client                        Server
  │                            │
  │ RespawnTimer (4123)        │
  │◄───────────────────────────│ (every 1s)
```

### Beispiel Payloads

```csharp
var timer = new RespawnTimer
{
    CharacterId = 9001,
    RespawnAvailableAt = DateTimeOffset.UtcNow.AddSeconds(8).ToUnixTimeMilliseconds(),
    Revision = 45
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `RespawnRequest` | 4120 | löst Timer-Check aus |
| `RespawnComplete` | 4124 | stoppt Timer |

---

### RespawnComplete (4124)

**Richtung:** 📥 Server → Client (Response)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf `RespawnRequest`. Enthält Erfolgsstatus, finalen Spawnpunkt, Invulnerability-Dauer und neue Revision. Beendet GhostMode, startet Alive State.

### Im Scope ✅
- Erfolg/Fehler
- Spawn Position/Zone
- Invulnerability Infos

### Nicht im Scope ❌
- Rez durch Caster (ResurrectComplete)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `RespawnComplete` | Ja |
| Success | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |
| CharacterId | long | Spieler | Ja |
| ZoneId | ushort | Zielzone | Bei Erfolg |
| Position | Vector2 | Spawn Position | Bei Erfolg |
| SafeBubbleRadius | float | Schutzblase | Bei Erfolg |
| SafeBubbleDurationMs | int | Dauer | Bei Erfolg |
| Revision | uint | Neue Revision | Bei Erfolg |
| ClientSequence | uint | Echo | Ja |

### Erwartete Response
- Keine (ist Response). Client sendet keine Ack.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RespawnComplete)]
public class RespawnComplete : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.RespawnComplete;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public string? ErrorCode { get; set; }
    [Key(3)] public long CharacterId { get; set; }
    [Key(4)] public ushort ZoneId { get; set; }
    [Key(5)] public Vector2 Position { get; set; }
    [Key(6)] public float SafeBubbleRadius { get; set; }
    [Key(7)] public int SafeBubbleDurationMs { get; set; }
    [Key(8)] public uint Revision { get; set; }
    [Key(9)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Bei Erfolg: Setzt Alive, entfernt Corpse, sendet GhostModeEnd.
- Bei Fehler: Lässt Ghost aktiv, ErrorCode gesetzt.

### Client-Verhalten
- Bei Erfolg: Teleport, UI wieder normal, bricht GhostPosition ab.
- Bei Fehler: Zeigt Fehler und wartet weiter.

### Flow-Diagramm

```
Client                        Server
  │ RespawnRequest (4120)      │
  │───────────────────────────►│
  │ RespawnComplete (4124)     │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var res = new RespawnComplete
{
    Success = true,
    CharacterId = 9001,
    ZoneId = 1001,
    Position = new Vector2 { X = 52, Y = 122 },
    SafeBubbleRadius = 6f,
    SafeBubbleDurationMs = 5000,
    Revision = 46,
    ClientSequence = 7
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `RESPAWN_LOCKED` | Timer nicht abgelaufen |
| `LOCKED_BY_ENCOUNTER` | Encounter verhindert |
| `IN_COMBAT` | PvP-Lock |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `RespawnRequest` | 4120 | Request |
| `GhostModeEnd` | 4111 | Folge |

---

### ResurrectOffer (4130)

**Richtung:** 📡 Event (Server → Target Client, optional Party UI)  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server sendet Rez-Angebot an Dead/Ghost Ziel, initiiert durch Caster Spell/Item. Enthält OfferId, Range, Expiry, Flags.

### Im Scope ✅
- Offer erstellen und zustellen
- Expiry Handling
- Flags (BattleRez, Soulstone)

### Nicht im Scope ❌
- Accept/Decline (separate)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ResurrectOffer` | Ja |
| OfferId | Guid | Offer | Ja |
| CasterId | long | Rez-Anbieter | Ja |
| TargetId | long | Empfänger | Ja |
| SpellId | int | Quelle | Ja |
| ExpiresAt | long | Unix ms | Ja |
| Range | float | Max Distanz | Ja |
| RequiresLoS | bool | LoS Pflicht | Ja |
| Flags | ResurrectionOfferFlags | BattleRez etc. | Ja |
| AllowedStates | DeathState[] | Dead/Ghost | Ja |

### Erwartete Response
- `ResurrectAccept (4131)` oder `ResurrectDecline (4132)`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ResurrectOffer)]
public class ResurrectOffer : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ResurrectOffer;
    [Key(1)] public Guid OfferId { get; set; }
    [Key(2)] public long CasterId { get; set; }
    [Key(3)] public long TargetId { get; set; }
    [Key(4)] public int SpellId { get; set; }
    [Key(5)] public long ExpiresAt { get; set; }
    [Key(6)] public float Range { get; set; }
    [Key(7)] public bool RequiresLoS { get; set; }
    [Key(8)] public ResurrectionOfferFlags Flags { get; set; }
    [Key(9)] public DeathState[] AllowedStates { get; set; } = Array.Empty<DeathState>();
}
```

### Server-Verhalten
- Validiert Caster/Target Distance + LoS; lehnt Offer ab, wenn invalid.
- Startet Offer Expiry Timer; bei Ablauf sendet `ResurrectComplete` mit Success=false.

### Client-Verhalten
- Zeigt Rez-Dialog, Countdown.
- Sperrt Accept-Button nach Expiry.

### Flow-Diagramm

```
Caster                        Server                        Target
  │                             │                             │
  │ Cast Rez                    │                             │
  │────────────────────────────►│                             │
  │                             │ ResurrectOffer (4130)       │
  │                             │────────────────────────────►│
```

### Beispiel Payloads

```csharp
var offer = new ResurrectOffer
{
    OfferId = Guid.NewGuid(),
    CasterId = 7001,
    TargetId = 9001,
    SpellId = 20021,
    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(8).ToUnixTimeMilliseconds(),
    Range = 8f,
    RequiresLoS = true,
    Flags = ResurrectionOfferFlags.BattleRez,
    AllowedStates = new[] { DeathState.Dead, DeathState.Ghost }
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `ResurrectAccept` | 4131 | Request |
| `ResurrectDecline` | 4132 | Request |
| `ResurrectComplete` | 4133 | Abschluss |

---

### ResurrectAccept (4131)

**Richtung:** 📤 Client → Server (Request)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client akzeptiert Rez-Angebot. Server validiert Offer (Expiry, Range, LoS, State) und führt Rez aus oder antwortet mit Fehler.

### Im Scope ✅
- Accept idempotent
- Range/LoS Validierung

### Nicht im Scope ❌
- Rez-Visuals (Server/Client Scripts)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ResurrectAccept` | Ja |
| OfferId | Guid | Offer | Ja |
| TargetId | long | Spieler (self) | Ja |
| ClientSequence | uint | Korrelations-Id | Ja |

### Erwartete Response
- `ResurrectComplete (4133)` mit Success/Failure und Echo ClientSequence.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ResurrectAccept)]
public class ResurrectAccept : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ResurrectAccept;
    [Key(1)] public Guid OfferId { get; set; }
    [Key(2)] public long TargetId { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Prüft Offer Map; wenn abgelaufen → Success=false, ErrorCode=INVALID_OFFER.
- Range/LoS Prüfung; wenn fail → Success=false, ErrorCode=OUT_OF_RANGE.
- Bei Erfolg: Setzt Alive, wendet Heal/Shield, sendet `ResurrectComplete`.

### Client-Verhalten
- Sperrt Buttons bis Response.
- Bei Success: Schließt Dialog, spielt Animation; bei Failure: zeigt Grund.

### Flow-Diagramm

```
Client                        Server
  │ ResurrectAccept (4131)     │
  │───────────────────────────►│
  │ ResurrectComplete (4133)   │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var acc = new ResurrectAccept
{
    OfferId = offerId,
    TargetId = 9001,
    ClientSequence = 11
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `INVALID_OFFER` | Offer ungültig/abgelaufen |
| `OUT_OF_RANGE` | Distanz/LoS fail |
| `ALREADY_RESOLVED` | Bereits angenommen/abgelehnt |
| `ALREADY_ALIVE` | Spieler schon lebendig |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `ResurrectOffer` | 4130 | Offer |
| `ResurrectComplete` | 4133 | Response |

---

### ResurrectDecline (4132)

**Richtung:** 📤 Client → Server (Request)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lehnt Rez-Angebot ab. Idempotent; Server informiert Caster optional via Notification.

### Im Scope ✅
- Ablehnung mit Grund
- Idempotenz

### Nicht im Scope ❌
- UI Entscheidungen

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ResurrectDecline` | Ja |
| OfferId | Guid | Offer | Ja |
| TargetId | long | Spieler | Ja |
| ClientSequence | uint | Korrelations-Id | Ja |

### Erwartete Response
- `ResurrectComplete (4133)` mit Success=false, ErrorCode=DECLINED.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ResurrectDecline)]
public class ResurrectDecline : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ResurrectDecline;
    [Key(1)] public Guid OfferId { get; set; }
    [Key(2)] public long TargetId { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Markiert Offer resolved, sendet `ResurrectComplete` (Success=false, ErrorCode=DECLINED).
- Optional: Notifies Caster via ChatSystem (410) or Toast.

### Client-Verhalten
- Schließt Rez-Dialog, bleibt Ghost/Dead.

### Flow-Diagramm

```
Client                        Server
  │ ResurrectDecline (4132)    │
  │───────────────────────────►│
  │ ResurrectComplete (4133)   │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var dec = new ResurrectDecline
{
    OfferId = offerId,
    TargetId = 9001,
    ClientSequence = 12
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `INVALID_OFFER` | Offer ungültig |
| `ALREADY_RESOLVED` | Bereits entschieden |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `ResurrectOffer` | 4130 | Offer |
| `ResurrectComplete` | 4133 | Response |

---

### ResurrectComplete (4133)

**Richtung:** 📥 Server → Client (Response)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf `ResurrectAccept` oder `ResurrectDecline`. Signalisiert Ergebnis (Success/Failure) und liefert Spawn-Infos bei Erfolg.

### Im Scope ✅
- Ergebnis + ErrorCode
- Position/Zone bei Erfolg
- Invulnerability Info

### Nicht im Scope ❌
- Respawn ohne Caster (RespawnComplete)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ResurrectComplete` | Ja |
| Success | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |
| CharacterId | long | Ziel | Ja |
| ZoneId | ushort | Zone | Bei Erfolg |
| Position | Vector2 | Aufsteh-Position | Bei Erfolg |
| SafeBubbleRadius | float | Schutzblase | Bei Erfolg |
| SafeBubbleDurationMs | int | Dauer | Bei Erfolg |
| Revision | uint | Neue Revision | Bei Erfolg |
| ClientSequence | uint | Echo | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ResurrectComplete)]
public class ResurrectComplete : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ResurrectComplete;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public string? ErrorCode { get; set; }
    [Key(3)] public long CharacterId { get; set; }
    [Key(4)] public ushort ZoneId { get; set; }
    [Key(5)] public Vector2 Position { get; set; }
    [Key(6)] public float SafeBubbleRadius { get; set; }
    [Key(7)] public int SafeBubbleDurationMs { get; set; }
    [Key(8)] public uint Revision { get; set; }
    [Key(9)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Bei Erfolg: Entfernt Corpse (optional), setzt Alive, gibt Buffs/Auren zurück falls Spell definiert.
- Bei Failure: Belässt Zustand, Offer als resolved.

### Client-Verhalten
- Bei Erfolg: Teleportiert, beendet Ghost, startet Invulnerability Timer.
- Bei Failure: Zeigt Error, bleibt Ghost/Dead.

### Flow-Diagramm

```
Client                        Server
  │ ResurrectAccept/Decline    │
  │───────────────────────────►│
  │ ResurrectComplete (4133)   │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var comp = new ResurrectComplete
{
    Success = true,
    CharacterId = 9001,
    ZoneId = 1001,
    Position = new Vector2 { X = 103, Y = 88 },
    SafeBubbleRadius = 5f,
    SafeBubbleDurationMs = 3000,
    Revision = 47,
    ClientSequence = 11
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `INVALID_OFFER` | Ungültig/abgelaufen |
| `DECLINED` | Abgelehnt |
| `OUT_OF_RANGE` | Distanz/LoS |
| `ALREADY_ALIVE` | Spieler bereits lebendig |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `ResurrectAccept` | 4131 | Request |
| `ResurrectDecline` | 4132 | Request |
| `ResurrectOffer` | 4130 | Ursprung |

---

### SoulstoneResurrect (4134)

**Richtung:** 📡 Event (Server → Client)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Automatische Selbst-Resurrection durch Soulstone/Item. Server triggert ohne Offer-Dialog; sendet direkt dieses Event gefolgt von `ResurrectComplete`.

### Im Scope ✅
- Auto-Res ohne Offer
- Verbrauch des Soulstones

### Nicht im Scope ❌
- BattleRez Limit (anderes Flag)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `SoulstoneResurrect` | Ja |
| CharacterId | long | Spieler | Ja |
| ItemId | int | Auslösendes Item | Ja |
| Revision | uint | Neue Revision | Ja |

### Erwartete Response
- Keine. `ResurrectComplete` folgt.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SoulstoneResurrect)]
public class SoulstoneResurrect : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SoulstoneResurrect;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public int ItemId { get; set; }
    [Key(3)] public uint Revision { get; set; }
}
```

### Server-Verhalten
- Prüft Item Buff aktiv; verbraucht ihn.
- Setzt Rez-Position = aktuelle Corpse Position.

### Client-Verhalten
- Spielt spezielles FX; kein Dialog.
- Erwartet ResurrectComplete.

### Flow-Diagramm

```
Client                        Server
  │                            │ Soulstone triggers
  │ SoulstoneResurrect (4134)  │
  │◄───────────────────────────│
  │ ResurrectComplete (4133)   │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var ss = new SoulstoneResurrect
{
    CharacterId = 9001,
    ItemId = 88001,
    Revision = 48
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `ResurrectComplete` | 4133 | Abschluss |

---

### BattleResurrect (4135)

**Richtung:** 📡 Event (Server → Target)  
**Frequenz:** Selten (Encounter)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Serverseitiger Battle-Res (Encounter begrenzt). Kann ohne Offer laufen (AutoAccept) oder Offer voraussetzen je nach EncounterFlag. Enthält Encounter-Lockout Info.

### Im Scope ✅
- Encounter Cooldown
- Optional AutoAccept

### Nicht im Scope ❌
- Soulstone (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattleResurrect` | Ja |
| CharacterId | long | Ziel | Ja |
| EncounterId | int | Encounter | Ja |
| AutoAccept | bool | Ob ohne Dialog | Ja |
| Revision | uint | Neue Revision | Ja |

### Erwartete Response
- Wenn AutoAccept=false: `ResurrectAccept/Decline` vom Client.  
- Wenn AutoAccept=true: Server sendet direkt `ResurrectComplete`.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BattleResurrect)]
public class BattleResurrect : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BattleResurrect;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public int EncounterId { get; set; }
    [Key(3)] public bool AutoAccept { get; set; }
    [Key(4)] public uint Revision { get; set; }
}
```

### Server-Verhalten
- Zieht Encounter BattleRez Charge ab.
- Wenn AutoAccept: wählt Position (Corpse), sendet ResurrectComplete.

### Client-Verhalten
- Bei AutoAccept: Sofort aufstehen.  
- Sonst: Zeigt Offer UI (kann identisch zu ResurrectOffer genutzt werden).

### Flow-Diagramm

```
Client                        Server
  │ BattleResurrect (4135)     │
  │◄───────────────────────────│
  │ [AutoAccept?]              │
  │ ResurrectComplete (4133)   │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var br = new BattleResurrect
{
    CharacterId = 9001,
    EncounterId = 501,
    AutoAccept = true,
    Revision = 49
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `ResurrectComplete` | 4133 | Abschluss |

---

### ReleaseSpirit (4140)

**Richtung:** 📤 Client → Server (Request)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert das Freisetzen des Geists an. Nach Zustimmung startet GhostModeStart. Idempotent; erneut senden führt zu gleicher Folge.

### Im Scope ✅
- Transition Dead → Ghost
- Corpse Link

### Nicht im Scope ❌
- Respawn (RespawnRequest)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ReleaseSpirit` | Ja |
| CharacterId | long | Spieler | Ja |
| ClientSequence | uint | Korrelations-Id | Ja |

### Erwartete Response
- `GhostModeStart (4110)`  
- `CorpseLocation (4113)`  
- Optional `RespawnTimer (4123)`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReleaseSpirit)]
public class ReleaseSpirit : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReleaseSpirit;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Markiert Corpse `IsReleased=true`.
- Sendet GhostModeStart + CorpseLocation.

### Client-Verhalten
- Sendet nur wenn Dead; sperrt Button nach Senden.

### Flow-Diagramm

```
Client                        Server
  │ ReleaseSpirit (4140)       │
  │───────────────────────────►│
  │ GhostModeStart (4110)      │
  │◄───────────────────────────│
  │ CorpseLocation (4113)      │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var rel = new ReleaseSpirit { CharacterId = 9001, ClientSequence = 5 };
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `ALREADY_RELEASED` | Bereits Ghost |
| `ALREADY_ALIVE` | Nicht tot |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `GhostModeStart` | 4110 | Response |
| `CorpseLocation` | 4113 | Ergänzung |

---

### RetrieveCorpse (4141)

**Richtung:** 📤 Client → Server (Request)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Teleportiert Geist zur Corpse und setzt Alive wenn in Reichweite. Nutzt Anti-Exploit Checks (Distanz ≤ 2m nach Teleport). Erfolgreich → RespawnComplete an Corpse-Position.

### Im Scope ✅
- Corpse Pickup
- Teleport Snap

### Nicht im Scope ❌
- Graveyard Respawn

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `RetrieveCorpse` | Ja |
| CharacterId | long | Spieler | Ja |
| CorpseId | long | Corpse | Ja |
| ClientSequence | uint | Korrelations-Id | Ja |

### Erwartete Response
- `RespawnComplete (4124)` oder Error in `RespawnComplete`.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RetrieveCorpse)]
public class RetrieveCorpse : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.RetrieveCorpse;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public long CorpseId { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Prüft Besitz, Corpse Exists, Decay.
- Teleportiert Ghost zur Corpse (snap), setzt Alive + Invulnerability, sendet `RespawnComplete`.

### Client-Verhalten
- Erwartet Teleport/Loading; falls Error zeigt UI.

### Flow-Diagramm

```
Client                        Server
  │ RetrieveCorpse (4141)      │
  │───────────────────────────►│
  │ RespawnComplete (4124)     │
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var rc = new RetrieveCorpse
{
    CharacterId = 9001,
    CorpseId = 12001,
    ClientSequence = 8
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `NO_CORPSE` | Corpse fehlt |
| `NOT_OWNER` | Falscher Spieler |
| `RESPAWN_LOCKED` | Timer nicht abgelaufen |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `CorpseLocation` | 4113 | Infos |
| `RespawnComplete` | 4124 | Response |

---

### SpiritHealerRevive (4142)

**Richtung:** 📤 Client → Server (Request)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Respawn beim Spirit Healer (NPC). Fügt `ResurrectionSickness` Debuff hinzu und extra Durability-Loss. Wird in Hub/Graveyard angeboten.

### Im Scope ✅
- NPC-gebundener Respawn
- Sickness Debuff

### Nicht im Scope ❌
- Standard Respawn ohne Debuff

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `SpiritHealerRevive` | Ja |
| CharacterId | long | Spieler | Ja |
| NpcId | long | Spirit Healer | Ja |
| ClientSequence | uint | Korrelations-Id | Ja |

### Erwartete Response
- `RespawnComplete (4124)` (Success/Failure)
- `ResurrectionSickness (4143)` bei Erfolg

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SpiritHealerRevive)]
public class SpiritHealerRevive : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SpiritHealerRevive;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public long NpcId { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
}
```

### Server-Verhalten
- Validiert NpcId (Spirit Healer).  
- Setzt RespawnPoint = Npc Position, wendet Sickness Debuff, erhöht Durability-Loss (extra 10%).
- Sendet RespawnComplete + ResurrectionSickness.

### Client-Verhalten
- UI: Warnung über Sickness/Durability.  
- Nach Erfolg: Zeigt Debuff Timer.

### Flow-Diagramm

```
Client                        Server
  │ SpiritHealerRevive (4142)  │
  │───────────────────────────►│
  │ RespawnComplete (4124)     │
  │◄───────────────────────────│
  │ ResurrectionSickness (4143)│
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var shr = new SpiritHealerRevive
{
    CharacterId = 9001,
    NpcId = 30001,
    ClientSequence = 9
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `INVALID_NPC` | Kein Spirit Healer |
| `RESPAWN_LOCKED` | Timer |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `RespawnComplete` | 4124 | Response |
| `ResurrectionSickness` | 4143 | Debuff |

---

### ResurrectionSickness (4143)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Benachrichtigt Client über angewandten Debuff nach SpiritHealerRevive. Enthält Dauer, Stat-Mali und zusätzlichen Durability-Loss.

### Im Scope ✅
- Debuff Parameter
- Anzeige für UI

### Nicht im Scope ❌
- Andere Debuffs (Aura System allgemein)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ResurrectionSickness` | Ja |
| CharacterId | long | Spieler | Ja |
| DurationMs | int | Dauer | Ja |
| DurabilityPenaltyPercent | byte | Extra Durability Schaden | Ja |
| StatPenaltyPercent | byte | Stat-Malus | Ja |

### Erwartete Response
- Keine.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ResurrectionSickness)]
public class ResurrectionSickness : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ResurrectionSickness;
    [Key(1)] public long CharacterId { get; set; }
    [Key(2)] public int DurationMs { get; set; }
    [Key(3)] public byte DurabilityPenaltyPercent { get; set; }
    [Key(4)] public byte StatPenaltyPercent { get; set; }
}
```

### Server-Verhalten
- Fügt Aura/Status hinzu, synchronisiert mit Buff System.

### Client-Verhalten
- Zeigt Debuff Icon + Timer, passt Stats lokal an.

### Flow-Diagramm

```
Client                        Server
  │ RespawnComplete (4124)     │
  │◄───────────────────────────│
  │ ResurrectionSickness (4143)│
  │◄───────────────────────────│
```

### Beispiel Payloads

```csharp
var sick = new ResurrectionSickness
{
    CharacterId = 9001,
    DurationMs = 600000,
    DurabilityPenaltyPercent = 10,
    StatPenaltyPercent = 75
};
```

### Error Codes

| Code | Bedeutung |
|------|-----------|
| – | Keine |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| `SpiritHealerRevive` | 4142 | Auslöser |

---

## 🗑️ Obsolete Messages

Keine Messages in 4100–4199 sind derzeit als **Obsolete** markiert. Historische Felder werden über Flags gesteuert; bei Deprecation neue Sektion hinzufügen.

---

## 🧨 Edge Cases & Fehlerfälle

- **Reconnect während Downed/Dead:** Server sendet `DeathNotification`, `CorpseLocation`, `RespawnTimer` erneut. Client muss alte Revisionen verwerfen.
- **Encounter-Wipe:** `BattleResurrect` Offers verfallen sofort, `RespawnRequest` gesperrt bis EncounterReset. ErrorCode `LOCKED_BY_ENCOUNTER`.
- **Cross-Zone Corpse:** ReleaseSpirit in anderer Zone triggert `RespawnAtGraveyard` in aktueller Zone; Corpse bleibt im Ursprungs-Zone, RetrieveCorpse nicht möglich, ErrorCode `NO_CORPSE`.
- **Hardcore Mode:** Flag `DeathFlags.Hardcore` setzt Account-Lock; Respawn/Rez nicht erlaubt. Server sendet `RespawnComplete` mit Success=false, ErrorCode=`HARDCORE_DEATH`. Wird von UI speziell behandelt.
- **PvP Safe-Bubble Abuse:** Wenn Spieler offensiv handelt während SafeBubble, Aura entfernt. Server überwacht CombatStart; auf Regelbruch sofort SafeBubble entfernen.
- **Multiple Offers:** Nur zuletzt angenommene Offer gewinnt. Server speichert resolved state per OfferId und CharacterId, sendet `ResurrectComplete` passend; andere Offers erhalten `ALREADY_RESOLVED`.
- **Corpse Decay während Accept:** Wenn Corpse decayed zwischen Accept und Completion → ErrorCode `NO_CORPSE`, Target bleibt Ghost.
- **Rate Limits:** RespawnRequest >3x in 10s → `RATE_LIMITED`, Cooldown 5s.
- **Teleport Exploit GhostPosition:** Delta > 10m führt zu MovementCorrection (202) + AntiCheatWarning (924).
- **Zone Transfer in Ghost:** Nicht erlaubt; ZoneTransferRequest (106) wird mit ErrorMessage (910) abgelehnt.

---

## 📎 Anhang (MessageType Enum Updates)

Es wurden **keine neuen MessageTypes** im Bereich 4100–4199 hinzugefügt. Bestehende Enum-Werte werden genutzt:

```csharp
// shared/Mmo.Shared/Messaging/Enums/MessageType.cs
// DEATH / RESPAWN / GHOST (4100-4199)
DeathNotification = 4100,
DeathRecap = 4101,
GhostModeStart = 4110,
GhostModeEnd = 4111,
GhostPosition = 4112,
CorpseLocation = 4113,
CorpseRevive = 4114,
RespawnRequest = 4120,
RespawnAtGraveyard = 4121,
RespawnAtCheckpoint = 4122,
RespawnTimer = 4123,
RespawnComplete = 4124,
ResurrectOffer = 4130,
ResurrectAccept = 4131,
ResurrectDecline = 4132,
ResurrectComplete = 4133,
SoulstoneResurrect = 4134,
BattleResurrect = 4135,
ReleaseSpirit = 4140,
RetrieveCorpse = 4141,
SpiritHealerRevive = 4142,
ResurrectionSickness = 4143
```

---

**Letzte Aktualisierung:** 2026-01-03  
**Version:** 1.0.0

[← Zurück zur Übersicht](README.md)
