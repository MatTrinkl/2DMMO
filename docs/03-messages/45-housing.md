# 🏠 Housing Messages (4500-4599)

**Kategorie:** 45  
**Range:** 4500-4599  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung (Spezifikation stabil, Implementierung in Arbeit)

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🔒 Permissions & Visitors](#-permissions--visitors)
- [✅ Place/Move/Rotate/Remove Regeln](#-placemoverotateremove-regeln)
- [🧰 Storage & Decoration Inventory](#-storage--decoration-inventory)
- [🏗️ Upgrades & Unlocks](#-upgrades--unlocks)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#-regeln--sicherheit)
- [📩 Aktive Messages 4500–4599](#-aktive-messages-4500–4599)
  - [HousingEnter (4500)](#housingenter-4500)
  - [HousingLeave (4501)](#housingleave-4501)
  - [HousingEdit (4502)](#housingedit-4502)
  - [HousingPlace (4503)](#housingplace-4503)
  - [HousingRemove (4504)](#housingremove-4504)
  - [HousingSave (4505)](#housingsave-4505)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang (MessageType Enum Updates)](#-anhang-messagetype-enum-updates)

---

## 📋 Überblick

Player Housing erlaubt Spieler:innen instanzierte Grundstücke (Plots) oder Innenräume (Rooms) zu betreten, zu dekorieren und zu speichern. Kategorie 45 deckt ausschließlich Nachrichten zu Housing-Instanzen, Platzierungen, Besucher-Management und revisionssicheren Deltas ab. Scope umfasst:

- Betreten/Verlassen von Housing-Instanzen (persönlich oder als Visitor) inkl. Instanz-Lifecycle.
- Editiermodus, Platzieren/Bewegen/Rotieren/Entfernen von Dekorationen und Möbeln auf server-authoritativer Grid-/Surface-Basis.
- Persistenz (Save) mit monotoner `HousingRevision` und delta-basiertem Broadcast, sodass Reconnects konsistent bleiben.
- Permissions (Owner, CoOwner, Editor, Visitor) inkl. Kick/Ban-Flows und anti-griefing Rollbacks.
- Integration in Instancing (24xx), Zone/World Routing (01xx/26xx), Inventory (05xx), Crafting (16xx), Economy (37xx) und Social/Guild (21xx/08xx).

Nicht im Scope dieser Kategorie:

- Blaupausen-Editoren außerhalb der Instanz (separater Tooling-Channel).
- Server-seitige Asset-Pipelines; Assets werden clientseitig vorgecached (siehe Connection/Compression in 00).
- Housing-Matchmaking/Listing im öffentlichen Browser (gehört zu Social/Marketplace).

---

## 🧠 Datenmodell

### HousingInstance

```csharp
public class HousingInstance
{
    public Guid InstanceId { get; set; }
    public Guid OwnerCharacterId { get; set; }
    public string TemplateId { get; set; } // Basis-Layout (z.B. "starter_cottage")
    public int CurrentRevision { get; set; } // Monoton steigend
    public List<Room> Rooms { get; set; } = new();
    public List<Placement> Placements { get; set; } = new();
    public HousingPermissionSet Permissions { get; set; } = new();
    public DateTime LastSavedUtc { get; set; }
    public bool IsLockedForEdit { get; set; } // Single-writer Lock
}
```

### Plot

- Definiert Außenareale (x/y Bounds, erlaubte Höhenstufen).
- Enthält Spawn- und Exit-Points (für Enter/Leave Routing).
- Verweist auf Welt-Zone (ZoneId) und InstanzShard (siehe 01/24).

### Room

```csharp
public class Room
{
    public string RoomId { get; set; } // stable slug
    public string DisplayName { get; set; }
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public bool AllowCeilingPlacement { get; set; }
    public bool AllowWallPlacement { get; set; }
    public bool AllowFloorPlacement { get; set; }
}
```

### FurnitureItem

- Referenziert Inventory-ItemId (05xx) und StaticFurnitureId (definiert in content DB).
- Enthält Tags (SurfaceType, CollisionShape) für Validation.

### Placement

```csharp
public class Placement
{
    public Guid PlacementId { get; set; }          // idempotent Token
    public Guid FurnitureInstanceId { get; set; }  // references inventory instance
    public string RoomId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public float RotationDeg { get; set; }
    public PlacementSurface Surface { get; set; }  // Floor/Wall/Ceiling
    public PlacementState State { get; set; }      // Placed / PendingRemove (PendingRemove wird genutzt für gestoppte/rollbackende Removes bevor Save)
}
```

### PlacementRevision

- Snapshot of `Placements` at `HousingRevision`.
- Stored server-side for rollback/anti-grief auditing.
- Used in delta broadcast decisions.

---

## 🔒 Permissions & Visitors

- **Roles:** Owner (full), CoOwner (full except delete instance), Editor (edit placements, not permissions), Visitor (view only).
- **Permission Set:** Stored per Instance; default Owner only. Editors list (characterIds) and Visitor rules (friends/guild/public) defined.
- **Visitor Kick/Ban:** Owner/CoOwner/Editor can kick (temporary removal); ban list persistent per instance.
- **Rate Limits:** Kick/Ban actions limited (e.g., 10/min per instance) to avoid abuse; throttled via RateLimitWarning (917).
- **Moderation:** All placement mutations audited with `ActorCharacterId`, `PlacementId`, `Revision`.
- **Session Binding:** Visitor permissions resolved server-side from authenticated character; client-supplied role ignored.
- **Cross-Systems:** Guild overrides (08xx) allow guild-level edit if flag enabled.

---

## ✅ Place/Move/Rotate/Remove Regeln

- **Server-authoritativ:** Client sendet nur gewünschte Transform (`X`,`Y`,`RotationDeg`,`Surface`) + `FurnitureInstanceId` + `PlacementId`.
- **Bounds Check:** Placement muss innerhalb Room-Grid liegen (0..Width-1, 0..Height-1).
- **Collision:** Server prüft gegen bestehenden Placements (bounding boxes) und gegen Room-Static geometry.
- **Allowed Surfaces:** Validierung gegen `FurnitureItem.Tags` (Floor/Wall/Ceiling). Grid-Snap enforced.
- **Orientation:** Rotation auf definierte Snap-Stufen (0/90/180/270 oder item-spezifisch).
- **Placement Count:** Max placements per room/instance geprüft (configurable).
- **Idempotenz:** Gleiches `PlacementId` + identische Transform → noop success (dedup).
- **Move vs Rotate:** Beides via `HousingPlace` (new placement) und `HousingRemove` (old) delta oder via `HousingPlace` mit `PlacementId` bestehend (preferred).
- **Remove Regeln:** Rückgabe ins Inventory (05xx) falls Eigentum passt; sonst `ErrorCode.FORBIDDEN`.
- **Visibility:** Erfolgreiche Mutationen werden als Delta an alle Anwesenden gestreamt (HousingPlace/HousingRemove Richtung 📡).

---

## 🧰 Storage & Decoration Inventory

- Housing nutzt reguläres Inventory (05xx) für `FurnitureInstanceId`.
- Platzierte Items sind "gebunden" an Instance; im Inventory als "in use" markiert (lock) um Dupe zu verhindern.
- Entfernen sendet Item zurück ins Inventory Slot; bei vollem Inventar -> `ErrorCode.INVENTORY_FULL`.
- Bank/VoidStorage (40xx) Items können nicht direkt platziert werden; require withdraw.

---

## 🏗️ Upgrades & Unlocks

- Upgrades erweitern `Room` Anzahl oder `Grid` Größe; konsumieren Economy (37xx) oder Crafting (16xx) Tokens.
- Themes (Wallpaper/Flooring) als spezielle Placements mit Surface=Wall/Ceiling global.
- Slots für Außen-Deko (Plot) freischaltbar über Achievements (19xx) oder Quests (10xx).
- Messages für Upgrades werden derzeit nicht in 45xx geführt; Verwendung über Systems in 16xx/37xx, aber Effekte in HousingRevision dokumentiert.

---

## 🔄 Sync, Deltas & Revisioning

- **HousingRevision:** Monoton steigend integer. Jede mutierende Operation (Place/Remove/Save) erhöht Revision.
- **Optimistische Concurrency:** Requests enthalten `ClientRevision`; Server vergleicht. Bei Mismatch -> `ErrorCode.REVISION_CONFLICT`, Client holt Snapshot erneut.
- **Snapshot vs Delta:** `HousingEnter` Response liefert kompletten Snapshot (Placements + Permissions + Revision). Änderungen werden als Delta über dieselben MessageTypes (`HousingPlace`/`HousingRemove`) mit `IsDelta=true` verteilt.
- **Reconnect-Safe:** Client kann `HousingEnter` erneut senden nach Disconnect; Server liefert letztes Snapshot + Pending deltas (if any).
- **Single-Writer Lock:** Beim Edit-Start (`HousingEdit`) vergibt Server Lock an Editor; weitere Edit-Requests -> `ErrorCode.LOCKED_BY_OTHER`. Lock Timeout/Heartbeat via `HousingEdit` keepalive (same Type, flag `IsKeepAlive`).
- **Ordering:** Deltas enthalten `ServerSequence` + `Revision`; Client ignoriert Out-of-Order Revisionen (nur `Revision > current` anwenden).

---

## 🧱 DTOs / Interfaces

### HousingEnterDto

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HousingEnter)]
public class HousingEnterDto : IClientMessage, IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.HousingEnter;
    [Key(1)] public Guid InstanceId { get; set; }
    [Key(2)] public Guid OwnerCharacterId { get; set; }
    [Key(3)] public Guid RequestingCharacterId { get; set; }
    [Key(4)] public bool AsVisitor { get; set; }
    [Key(5)] public int ClientRevision { get; set; } // client-known
    [Key(6)] public int ServerRevision { get; set; } // server fills in response
    [Key(7)] public List<PlacementDto> Placements { get; set; }
    [Key(8)] public HousingPermissionSetDto Permissions { get; set; }
    [Key(9)] public List<RoomDto> Rooms { get; set; }
    [Key(10)] public HousingEnterResult Result { get; set; }
    [Key(11)] public string FailureReason { get; set; }
}
```

### PlacementDto

```csharp
[MessagePackObject]
public class PlacementDto
{
    [Key(0)] public Guid PlacementId { get; set; }
    [Key(1)] public Guid FurnitureInstanceId { get; set; }
    [Key(2)] public string RoomId { get; set; }
    [Key(3)] public int X { get; set; }
    [Key(4)] public int Y { get; set; }
    [Key(5)] public float RotationDeg { get; set; }
    [Key(6)] public PlacementSurface Surface { get; set; }
    [Key(7)] public PlacementState State { get; set; }
}
```

### HousingPermissionSetDto

```csharp
[MessagePackObject]
public class HousingPermissionSetDto
{
    [Key(0)] public Guid OwnerCharacterId { get; set; }
    [Key(1)] public List<Guid> CoOwners { get; set; }
    [Key(2)] public List<Guid> Editors { get; set; }
    [Key(3)] public List<Guid> BannedVisitors { get; set; }
    [Key(4)] public HousingVisitorPolicy VisitorPolicy { get; set; }
    [Key(5)] public bool AllowGuildVisitors { get; set; }
    [Key(6)] public bool AllowFriendsVisitors { get; set; }
    [Key(7)] public bool AllowPublicVisitors { get; set; }
}
```

### RoomDto

```csharp
[MessagePackObject]
public class RoomDto
{
    [Key(0)] public string RoomId { get; set; }
    [Key(1)] public string DisplayName { get; set; }
    [Key(2)] public int GridWidth { get; set; }
    [Key(3)] public int GridHeight { get; set; }
    [Key(4)] public bool AllowCeilingPlacement { get; set; }
    [Key(5)] public bool AllowWallPlacement { get; set; }
    [Key(6)] public bool AllowFloorPlacement { get; set; }
}
```

### HousingEditDto

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HousingEdit)]
public class HousingEditDto : IClientMessage, IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.HousingEdit;
    [Key(1)] public Guid InstanceId { get; set; }
    [Key(2)] public Guid RequestingCharacterId { get; set; }
    [Key(3)] public bool EnterEditMode { get; set; }
    [Key(4)] public bool IsKeepAlive { get; set; }
    [Key(5)] public int ClientRevision { get; set; }
    [Key(6)] public int ServerRevision { get; set; }
    [Key(7)] public HousingEditResult Result { get; set; }
    [Key(8)] public string FailureReason { get; set; }
}
```

### HousingPlaceDto

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HousingPlace)]
public class HousingPlaceDto : IClientMessage, IServerMessage, IBroadcastMessage
{
    [Key(0)] public MessageType Type => MessageType.HousingPlace;
    [Key(1)] public Guid InstanceId { get; set; }
    [Key(2)] public Guid RequestingCharacterId { get; set; }
    [Key(3)] public Guid PlacementId { get; set; }
    [Key(4)] public Guid FurnitureInstanceId { get; set; }
    [Key(5)] public string RoomId { get; set; }
    [Key(6)] public int X { get; set; }
    [Key(7)] public int Y { get; set; }
    [Key(8)] public float RotationDeg { get; set; }
    [Key(9)] public PlacementSurface Surface { get; set; }
    [Key(10)] public int ClientRevision { get; set; }
    [Key(11)] public int ServerRevision { get; set; }
    [Key(12)] public bool IsDelta { get; set; }
    [Key(13)] public PlacementResult Result { get; set; }
    [Key(14)] public string FailureReason { get; set; }
}
```

### HousingRemoveDto

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HousingRemove)]
public class HousingRemoveDto : IClientMessage, IServerMessage, IBroadcastMessage
{
    [Key(0)] public MessageType Type => MessageType.HousingRemove;
    [Key(1)] public Guid InstanceId { get; set; }
    [Key(2)] public Guid RequestingCharacterId { get; set; }
    [Key(3)] public Guid PlacementId { get; set; }
    [Key(4)] public int ClientRevision { get; set; }
    [Key(5)] public int ServerRevision { get; set; }
    [Key(6)] public bool ReturnToInventory { get; set; }
    [Key(7)] public bool IsDelta { get; set; }
    [Key(8)] public RemovalResult Result { get; set; }
    [Key(9)] public string FailureReason { get; set; }
}
```

### HousingSaveDto

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HousingSave)]
public class HousingSaveDto : IClientMessage, IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.HousingSave;
    [Key(1)] public Guid InstanceId { get; set; }
    [Key(2)] public Guid RequestingCharacterId { get; set; }
    [Key(3)] public int ClientRevision { get; set; }
    [Key(4)] public int ServerRevision { get; set; }
    [Key(5)] public bool Persisted { get; set; }
    [Key(6)] public string FailureReason { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

```csharp
public enum HousingEnterResult { Success = 0, NotOwner = 1, Banned = 2, InstanceNotFound = 3, Locked = 4 }
public enum HousingEditResult { Granted = 0, LockedByOther = 1, Forbidden = 2, RevisionConflict = 3 }
public enum PlacementSurface { Floor = 0, Wall = 1, Ceiling = 2, Plot = 3 }
public enum PlacementState { Placed = 0, PendingRemove = 1 }
public enum PlacementResult { Success = 0, InvalidSurface = 1, Collision = 2, OutOfBounds = 3, InventoryLocked = 4, Forbidden = 5, RevisionConflict = 6 }
public enum RemovalResult { Success = 0, NotFound = 1, Forbidden = 2, InventoryFull = 3, RevisionConflict = 4 }
public enum HousingVisitorPolicy { Closed = 0, FriendsOnly = 1, GuildOnly = 2, Public = 3 }
public enum HousingErrorCode { NONE = 0, INVALID_REQUEST = 1, FORBIDDEN = 2, NOT_FOUND = 3, INVENTORY_FULL = 4, REVISION_CONFLICT = 5, LOCKED_BY_OTHER = 6, RATE_LIMITED = 7 }
```

---

## ⚙️ Regeln & Sicherheit

- **Anti-Dupe:** FurnitureInstanceId wird beim Place gelockt; Save hebt Lock nur bei Persisted=true. Server prüft Inventarbesitz.
- **Anti-Grief:** Owner kann Rollback auf letzte persistierte Revision auslösen; dies erfolgt über bestehende Admin/GM-Tools (2300-2399 Admin / GM Tools Kategorie) und nicht über eine 45xx Message. Audit Log Hook pro Mutation.
- **Rate Limits:** Place/Remove/Edit enforced per instance (z.B. 20/s) → nutzt `RateLimitWarning (917)` aus der System-Kategorie zur UI-Signalisierung.
- **Auth:** Alle Messages benötigen aktive Session (00) und Character-Bindung. `RequestingCharacterId` muss Session Character sein.
- **Permission Checks:** Vor jeder Mutation: Role-Ermittlung, Ban-Liste, VisitorPolicy, Guild/Friend status.
- **Concurrency:** Revision-Check + Lock; bei Konflikt keine Änderung, Client muss resync.

---

## 📩 Aktive Messages 4500–4599

### HousingEnter (4500)

**Richtung:** 📤 Client → Server / 📥 Server → Client  
**Frequenz:** Selten (Betreten/Rejoin)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Owner/CoOwner für Edit-Lock, Visitor für Read

### Beschreibung
Anfrage zum Betreten einer Housing-Instanz oder erneute Synchronisation nach Disconnect. Server antwortet mit Snapshot (Placements, Permissions, Rooms) und legt Session-Bindung an. Dient zugleich als Reconnect-safe State-Sync.

### Im Scope ✅
- Instanz-Aufbau inkl. Spawn/Exit Points
- Revisionssynchronisation (ClientRevision/ServerRevision)
- Besucher- und Bannprüfung

### Nicht im Scope ❌
- Öffentliches Listing von Häusern
- Matchmaking für Besucher

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | Guid | Ziel-Housing-Instanz | Ja |
| OwnerCharacterId | Guid | Owner der Instanz (server fills) | Ja (Response) |
| RequestingCharacterId | Guid | Charakter des Clients | Ja |
| AsVisitor | bool | Visitor-Flag | Ja |
| ClientRevision | int | Letztbekannte Revision | Ja |
| ServerRevision | int | Revision vom Server | Response |
| Placements | List<PlacementDto> | Snapshot | Response |
| Permissions | HousingPermissionSetDto | Aktuelle Rechte | Response |
| Rooms | List<RoomDto> | Raumdefinitionen | Response |
| Result | HousingEnterResult | Ergebnis | Response |
| FailureReason | string | Fehlertext | Nein |

### Erwartete Response
- `HousingEnter` (4500) mit `Result` und Snapshot.

### Code-Beispiel
```csharp
var enter = new HousingEnterDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId,
    AsVisitor = true,
    ClientRevision = 12
};
```

### Server-Verhalten
- Prüft Session + Bannliste, erlaubt nur bekannte CharacterId.
- Lädt Instance Snapshot (Placements, Permissions, Rooms).
- Vergleicht `ClientRevision`; sendet Snapshot wenn älter, sonst Only-Reconfirm (ServerRevision echo).
- Registriert Client im Instance-Channel (für Deltas).

### Client-Verhalten
- Sendet Request nach Zone/Instance Routing (01xx/24xx).
- Bei Success: lädt Assets lokal, spawn an EntryPoint.
- Bei `Result=Banned` -> zeige Meldung, keine weiteren Requests.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  HousingEnter (4500)         │
  │─────────────────────────────►│
  │                              │ Snapshot laden, Revision prüfen
  │                              │
  │  HousingEnter (4500)         │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var response = new HousingEnterDto
{
    InstanceId = instanceId,
    OwnerCharacterId = owner,
    RequestingCharacterId = characterId,
    AsVisitor = true,
    ClientRevision = 12,
    ServerRevision = 14,
    Placements = snapshotPlacements,
    Permissions = permissions,
    Rooms = rooms,
    Result = HousingEnterResult.Success
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| BANNED | Spieler steht auf Ban-Liste |
| NOT_FOUND | Instanz existiert nicht |
| LOCKED | Instanz gesperrt (Maintenance) |
| REVISION_CONFLICT | ClientRevision ungültig |
| FORBIDDEN | Keine Permission (Owner-only) |

### Verwandte Messages
| Message | ID  | Beziehung |
| ------- | --- | --------- |
| HousingLeave | 4501 | Exit |
| HousingEdit | 4502 | Edit-Lock holen |
| HousingPlace | 4503 | Delta nach Eintritt |

---

### HousingLeave (4501)

**Richtung:** 📤 Client → Server / 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Verlässt die Housing-Instanz. Server trennt Session-Bindung und gibt Edit-Lock frei. Antwort bestätigt Erfolg.

### Im Scope ✅
- Sauberes Herausführen (ZoneTransfer zurück)
- Edit-Lock Freigabe
- Besucher Kick acknowledgement

### Nicht im Scope ❌
- Persistenz (Save) → HousingSave
- Cross-Zone Travel (Portal) außerhalb Housing

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | Guid | Zielinstanz | Ja |
| RequestingCharacterId | Guid | Charakter | Ja |
| Reason | string | optional (Logout/Kick) | Nein |

### Erwartete Response
- `HousingLeave` (4501) mit Bestätigung.

### Code-Beispiel
```csharp
new HousingLeaveDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId,
    Reason = "UserExit"
};
```

### Server-Verhalten
- Entfernt Client aus Instance-Channel.
- Gibt Edit-Lock frei falls gehalten.
- Triggert ZoneTransfer (106/107) falls Ziel-Zone notwendig.

### Client-Verhalten
- Stoppt Housing-Deltas.
- Lädt Zielzone (falls angegeben durch Gateway).

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  HousingLeave (4501)         │
  │─────────────────────────────►│
  │                              │ Lock freigeben
  │  HousingLeave (4501)         │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var leaveAck = new HousingLeaveDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| NOT_FOUND | Instanz unbekannt |
| INVALID_REQUEST | Charakter nicht im Housing |
| FORBIDDEN | Charakter wurde gebannt/kickt sich selbst |
| LOCKED | Instanz aktuell im Transfer/Ladezustand |

### Verwandte Messages
| Message | ID  | Beziehung |
| ------- | --- | --------- |
| HousingEnter | 4500 | vorheriger Eintritt |
| HousingEdit | 4502 | Lock freigabe |

---

### HousingEdit (4502)

**Richtung:** 📤 Client → Server / 📥 Server → Client  
**Frequenz:** Mittel (Enter/KeepAlive)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Owner/CoOwner/Editor

### Beschreibung
Fordert Edit-Modus an oder hält Lock aktiv. Server vergibt Single-Writer Lock. Antwort enthält Ergebnis und ggf. ServerRevision.

### Im Scope ✅
- Lock Acquisition & KeepAlive
- Edit-Freigabe für CoOwner/Editor
- Revision Echo

### Nicht im Scope ❌
- Permission-Änderung (separat in Social/Guild)
- Save Persistenz

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | Guid | Instanz | Ja |
| RequestingCharacterId | Guid | Charakter | Ja |
| EnterEditMode | bool | true=enter/false=exit | Ja |
| IsKeepAlive | bool | true für Lock-Heartbeat | Nein |
| ClientRevision | int | Client-Stand | Ja |
| ServerRevision | int | Server-Stand | Response |
| Result | HousingEditResult | Ergebnis | Response |
| FailureReason | string | Text | Nein |

### Erwartete Response
- `HousingEdit` (4502) mit `Result`.

### Code-Beispiel
```csharp
new HousingEditDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId,
    EnterEditMode = true,
    ClientRevision = 14
};
```

### Server-Verhalten
- Prüft Rolle (Owner/CoOwner/Editor).
- Vergibt Lock, setzt `IsLockedForEdit=true`.
- Bei KeepAlive aktualisiert Lock-Timeout.
- Antwortet mit `Result=LockedByOther` wenn Lock gehalten wird.

### Client-Verhalten
- Zeigt Edit-UI erst nach `Result=Granted`.
- Sendet KeepAlive alle 5s solange im Edit-Modus.
- Bei `LockedByOther` → Retry UI.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  HousingEdit (4502)          │
  │─────────────────────────────►│
  │                              │ Lock prüfen/setzen
  │  HousingEdit (4502)          │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var editGranted = new HousingEditDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId,
    EnterEditMode = true,
    ServerRevision = 15,
    Result = HousingEditResult.Granted
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| FORBIDDEN | Rolle nicht berechtigt |
| LOCKED_BY_OTHER | Lock durch anderen Editor |
| REVISION_CONFLICT | ClientRevision < ServerRevision |

### Verwandte Messages
| Message | ID  | Beziehung |
| ------- | --- | --------- |
| HousingEnter | 4500 | Voraussetzung |
| HousingPlace | 4503 | Mutationen nur mit Lock |
| HousingSave | 4505 | Persistiert nach Edit |

---

### HousingPlace (4503)

**Richtung:** 📤 Client → Server / 📥 Server → Client / 📡 Event  
**Frequenz:** Mittel-Häufig (während Edit)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Owner/CoOwner/Editor

### Beschreibung
Platzieren oder Verschieben/Rotieren eines Furniture-Items. Server validiert Bounds, Collisions, Surface und Revision. Erfolgreiche Operation wird als Delta an alle Anwesenden gestreamt (`IsDelta=true`).

### Im Scope ✅
- Idempotente PlacementId
- Server-authoritative Grid & Surface Checks
- Delta-Broadcast (Visitors sehen Änderungen)

### Nicht im Scope ❌
- Persistenz (Save) → HousingSave
- Inventar-Lagerverwaltung (Bank/Void)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | Guid | Instanz | Ja |
| RequestingCharacterId | Guid | Charakter | Ja |
| PlacementId | Guid | Idempotenter Token | Ja |
| FurnitureInstanceId | Guid | Item-Instanz | Ja |
| RoomId | string | Ziel-Raum | Ja |
| X | int | Grid-X | Ja |
| Y | int | Grid-Y | Ja |
| RotationDeg | float | 0-359 | Ja |
| Surface | PlacementSurface | Floor/Wall/... | Ja |
| ClientRevision | int | Client-Stand | Ja |
| ServerRevision | int | Server-Stand | Response |
| IsDelta | bool | True wenn Broadcast/Event | Response/Event |
| Result | PlacementResult | Ergebnis | Response/Event |
| FailureReason | string | Text | Nein |

### Erwartete Response
- `HousingPlace` (4503) mit `Result` und `ServerRevision`.

### Code-Beispiel
```csharp
new HousingPlaceDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId,
    PlacementId = Guid.NewGuid(),
    FurnitureInstanceId = chairId,
    RoomId = "living_room",
    X = 4,
    Y = 6,
    RotationDeg = 90,
    Surface = PlacementSurface.Floor,
    ClientRevision = 15
};
```

### Server-Verhalten
- Prüft Edit-Lock + Rolle.
- Validiert Surface erlaubt, Grid Bounds, Collision gegen Placements.
- Lockt FurnitureInstanceId (Inventory).
- Erhöht `ServerRevision` bei Erfolg, broadcastet Delta (IsDelta=true).

### Client-Verhalten
- Sendet Request, deaktiviert lokale Prediction bis Response.
- Bei Erfolg: übernimmt ServerRevision, aktualisiert lokales State, animiert Placement.
- Besucher erhalten Broadcast und aktualisieren Platzierung.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  HousingPlace (4503)         │
  │─────────────────────────────►│
  │                              │ Validate + Revision++
  │  HousingPlace (4503)         │
  │◄─────────────────────────────│
  │                              │
  │        Broadcast (4503 IsDelta=true)
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var broadcast = new HousingPlaceDto
{
    InstanceId = instanceId,
    PlacementId = placementId,
    FurnitureInstanceId = chairId,
    RoomId = "living_room",
    X = 4,
    Y = 6,
    RotationDeg = 90,
    Surface = PlacementSurface.Floor,
    ServerRevision = 16,
    IsDelta = true,
    Result = PlacementResult.Success
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| OUT_OF_BOUNDS | Koordinaten außerhalb Grid |
| COLLISION | Kollision mit anderem Placement |
| INVALID_SURFACE | Item darf diese Fläche nicht nutzen |
| INVENTORY_LOCKED | Item bereits platziert |
| REVISION_CONFLICT | ClientRevision veraltet |

### Verwandte Messages
| Message | ID  | Beziehung |
| ------- | --- | --------- |
| HousingEdit | 4502 | Lock |
| HousingRemove | 4504 | Entfernen |
| HousingSave | 4505 | Persistenz |

---

### HousingRemove (4504)

**Richtung:** 📤 Client → Server / 📥 Server → Client / 📡 Event  
**Frequenz:** Mittel-Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Owner/CoOwner/Editor

### Beschreibung
Entfernt ein Placement (inkl. Rückgabe ins Inventory). Server validiert Ownership, Revision und optionales ReturnToInventory Flag. Broadcastet Delta bei Erfolg.

### Im Scope ✅
- Inventory-Rückgabe
- Delta-Broadcast
- Idempotenz (PlacementId missing -> NotFound)

### Nicht im Scope ❌
- Massen-Remove (Batch) – späterer Bedarf

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | Guid | Instanz | Ja |
| RequestingCharacterId | Guid | Charakter | Ja |
| PlacementId | Guid | Ziel-Placement | Ja |
| ReturnToInventory | bool | true = Item zurück | Nein |
| ClientRevision | int | Client-Stand | Ja |
| ServerRevision | int | Server-Stand | Response |
| IsDelta | bool | Broadcast-Flag | Response/Event |
| Result | RemovalResult | Ergebnis | Response/Event |
| FailureReason | string | Text | Nein |

### Erwartete Response
- `HousingRemove` (4504) mit `Result`.

### Code-Beispiel
```csharp
new HousingRemoveDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId,
    PlacementId = placementId,
    ReturnToInventory = true,
    ClientRevision = 16
};
```

### Server-Verhalten
- Prüft Rolle + Edit-Lock.
- Prüft Platzierung existiert und gehört Instanz.
- Hebt FurnitureInstanceId-Lock auf, gibt Item zurück falls Flag true.
- Revision++ und Broadcast (IsDelta=true).

### Client-Verhalten
- Entfernt lokal nach bestätigtem Result.
- Besucher erhalten Delta und entfernen Placement in UI.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  HousingRemove (4504)        │
  │─────────────────────────────►│
  │                              │ Validate + Revision++
  │  HousingRemove (4504)        │
  │◄─────────────────────────────│
  │                              │
  │  Broadcast (4504 IsDelta=true) 
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var removeBroadcast = new HousingRemoveDto
{
    InstanceId = instanceId,
    PlacementId = placementId,
    ServerRevision = 17,
    IsDelta = true,
    Result = RemovalResult.Success
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| NOT_FOUND | PlacementId nicht bekannt |
| FORBIDDEN | Rolle nicht berechtigt |
| INVENTORY_FULL | Rückgabe nicht möglich |
| REVISION_CONFLICT | Veraltete Revision |

### Verwandte Messages
| Message | ID  | Beziehung |
| ------- | --- | --------- |
| HousingPlace | 4503 | Gegenspieler |
| HousingSave | 4505 | Persistiert Änderungen |

---

### HousingSave (4505)

**Richtung:** 📤 Client → Server / 📥 Server → Client  
**Frequenz:** Selten (nach Session)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Owner/CoOwner/Editor

### Beschreibung
Persistiert aktuellen Housing-Zustand (Placements + Permissions) in Datenbank. Server validiert Revision, schreibt Snapshot und hebt Locks. Antwort bestätigt Persistierung.

### Im Scope ✅
- Persistenz + Audit-Trail
- Revision Sync
- Lock-Release nach Erfolg

### Nicht im Scope ❌
- Server-initiierte Autosaves (intern)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | Guid | Instanz | Ja |
| RequestingCharacterId | Guid | Charakter | Ja |
| ClientRevision | int | Client-Stand | Ja |
| ServerRevision | int | Server-Stand | Response |
| Persisted | bool | Ergebnis | Response |
| FailureReason | string | Text | Nein |

### Erwartete Response
- `HousingSave` (4505) mit `Persisted=true`.

### Code-Beispiel
```csharp
new HousingSaveDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId,
    ClientRevision = 17
};
```

### Server-Verhalten
- Prüft Rolle + Revision.
- Persistiert Snapshot in Housing-DB.
- Hebt Edit-Lock und Inventory-Locks auf.
- Antwortet mit `Persisted=true` oder Fehler.

### Client-Verhalten
- Zeigt Bestätigung, schließt Edit-UI.
- Bei Fehler `RevisionConflict` -> Reenter + Reload Snapshot.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  HousingSave (4505)          │
  │─────────────────────────────►│
  │                              │ Persist snapshot
  │  HousingSave (4505)          │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var saveAck = new HousingSaveDto
{
    InstanceId = instanceId,
    RequestingCharacterId = characterId,
    ServerRevision = 18,
    Persisted = true
};
```

### Error Codes
| Code | Bedeutung |
| ---- | --------- |
| REVISION_CONFLICT | ClientRevision < ServerRevision |
| FORBIDDEN | Keine Berechtigung |
| INVALID_REQUEST | Keine Änderungen vorhanden |

### Verwandte Messages
| Message | ID  | Beziehung |
| ------- | --- | --------- |
| HousingEdit | 4502 | Lock Quelle |
| HousingPlace | 4503 | Änderungen vor Save |
| HousingRemove | 4504 | Änderungen vor Save |

---

## 🗑️ Obsolete Messages

Derzeit keine obsoleten Housing-Messages in 4500-4599.

---

## 🧨 Edge Cases & Fehlerfälle

- **Concurrent Editors:** Zweiter Editor erhält `LockedByOther`; muss warten oder wird nach Timeout automatisch granted.
- **Reconnect während Edit:** Client nutzt `HousingEnter` mit altem ClientRevision; Server liefert Snapshot, lock bleibt frei → Editor muss Lock neu holen.
- **Inventory Loss:** Wenn Item nicht mehr im Inventory (z.B. verkauft) → `PlacementResult.Invalid` mit `FailureReason="ItemMissing"`.
- **Visitors während Save:** Save führt zu kurzen Delta-Pause; Clients ignorieren Deltas < ServerRevision.
- **Kick während Edit:** Kick setzt Lock frei; Client erhält `ForceDisconnect` falls Session-Betrug.
- **Rollback:** Bei Anti-Grief Rollback (Admin) werden Deltas als `HousingPlace/HousingRemove` Broadcast mit neuem Revision gesendet.
- **Rate-Limit:** Überschreitung sendet `RateLimitWarning (917)`; Client backoff exponential.
- **Bounds Update durch Upgrade:** Wenn Room Grid erweitert wird (externe Systeme), Client muss Snapshot neu laden; alte Placements out-of-bounds -> Server verschiebt oder markiert als PendingRemove.
- **Rotation Snap:** Server rundet RotationDeg auf 0/90/180/270 (oder item-spezifische Snap-Stufen); Client sollte UI aktualisieren.

---

## 📎 Anhang (MessageType Enum Updates)

Keine neuen Enum-Einträge erforderlich; Housing nutzt bestehende MessageTypes 4500-4505.

```csharp
// Auszug zur Referenz (bereits bestehende Enum-Einträge):
HousingEnter = 4500,
HousingLeave = 4501,
HousingEdit = 4502,
HousingPlace = 4503,
HousingRemove = 4504,
HousingSave = 4505,
```
