# 🛡️ Equipment Messages (3900-3999)

**Kategorie:** 39  
**Range:** 3900-3999  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [✅ Equip/Unequip/Swap Regeln](#-equipunequipswap-regeln)
- [🧪 Stat Recalculation & Sync](#-stat-recalculation--sync)
- [🎨 Cosmetics / Transmog](#-cosmetics--transmog)
- [🔧 Durability & Repair](#-durability--repair)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#-regeln--sicherheit)
- [📩 Aktive Messages 3900–3999](#-aktive-messages-3900–3999)
  - [EquipItem (3900)](#equipitem-3900)
  - [EquipItemResult (3901)](#equipitemresult-3901)
  - [UnequipItem (3902)](#unequipitem-3902)
  - [UnequipItemResult (3903)](#unequipitemresult-3903)
  - [EquipmentSync (3904)](#equipmentsync-3904)
  - [EquipmentSlotUpdate (3905)](#equipmentslotupdate-3905)
  - [DurabilityUpdate (3910)](#durabilityupdate-3910)
  - [DurabilityWarning (3911)](#durabilitywarning-3911)
  - [ItemBroken (3912)](#itembroken-3912)
  - [GemSocket (3920)](#gemsocket-3920)
  - [GemSocketResult (3921)](#gemsocketresult-3921)
  - [GemRemove (3922)](#gemremove-3922)
  - [EnchantApply (3930)](#enchantapply-3930)
  - [EnchantApplyResult (3931)](#enchantapplyresult-3931)
  - [EnchantRemove (3932)](#enchantremove-3932)
  - [ReforgeOpen (3940)](#reforgeopen-3940)
  - [ReforgePreview (3941)](#reforgepreview-3941)
  - [ReforgeConfirm (3942)](#reforgeconfirm-3942)
  - [ReforgeResult (3943)](#reforgeresult-3943)
  - [SetBonusUpdate (3950)](#setbonusupdate-3950)
  - [SetBonusActivate (3951)](#setbonusactivate-3951)
  - [SetBonusDeactivate (3952)](#setbonusdeactivate-3952)
  - [WeaponSwapRequest (3960)](#weaponswaprequest-3960)
  - [WeaponSwapResult (3961)](#weaponswapresult-3961)
  - [OutfitSave (3970)](#outfitsave-3970)
  - [OutfitLoad (3971)](#outfitload-3971)
  - [OutfitDelete (3972)](#outfitdelete-3972)
  - [OutfitList (3973)](#outfitlist-3973)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang (MessageType Enum Updates)](#-anhang-messagetype-enum-updates)

---

## 📋 Überblick

- Scope umfasst alle Vorgänge rund um Ausrüsten, Entfernen, Tauschen und kosmetische Loadouts.  
- Client sendet **nie** statische Item- oder Statdaten, sondern referenziert nur `ItemInstanceId` und Ziel-`EquipmentSlot`.  
- Server ist autoritativ, wendet Validierung, Anti-Dupe-Mechaniken und Stat-Recalculation atomar an.  
- Jede Client→Server Nachricht hat eine explizite Response oder ein dokumentiertes Event-Antwortmuster.  
- Deltas und Snapshots unterstützen Reconnect-Sicherheit und Revisionierung.  
- Durability, Gems, Enchants, Reforge und Sets werden konsistent über diese Range übertragen.  
- Cosmetics/Outfits nutzen dieselbe Slot-Matrix, aber kennzeichnen sich durch `IsCosmetic=true`.  
- Alle Messages halten die Reihenfolge aus `MessageType.cs` strikt ein.

---

## 🧠 Datenmodell

### EquipmentSlot
- Enthält die klar definierte Slot-Matrix (Head, Chest, Legs, MainHand, OffHand, TwoHand, Ring1, Ring2, Trinket1, Trinket2, Neck, Shoulders, Waist, Feet, Hands, Wrist, Back, Tabard, Shirt, BeltEnchant, WeaponCosmetic, HeadCosmetic).
- Jeder Slot hat eine `AllowedItemCategory`-Liste (z.B. MainHand: OneHand, Dagger, Wand; OffHand: Shield, Offhand, Tome; TwoHand sperrt OffHand).
- Slot kennt einen `MaxStack` von 1 (keine Stapelung) und `UniqueEquipTags` (z.B. „Unique-Equipped: Trinket“).

### EquippedItem
- Felder: `ItemInstanceId`, `TemplateId`, `OwnerCharacterId`, `BoundState`, `DurabilityCurrent`, `DurabilityMax`, `Gems[]`, `Enchant`, `TransmogRef`, `CosmeticOnly`, `SetId`, `Revision`.
- `Revision` erhöht bei jeder serverseitigen Änderung (Durability, Gem, Enchant, Transmog, Reforge).
- `BoundState` (BoP/BoE/BoA) beeinflusst Handel aber nicht Equip-Regeln.

### Loadout
- Besteht aus Dictionary `EquipmentSlot -> EquippedItem`.
- Enthält `CosmeticLayer` (override visuals ohne Stats) und `GameplayLayer` (Stats).
- `LoadoutRevision` incremented bei jeder Equip/Unequip/Swap/OutfitLoad.

### Durability
- `DurabilityCurrent` >= 0; 0 bedeutet **broken** (Stat disable), aber Item bleibt ausgerüstet bis entfernt/repariert.
- Sinkt bei Tod, Treffer, falscher Level/Skill-Nutzung (wenn konfiguriert).

### GemSockets
- Jeder Item-Template definiert Socket-Slots (Color, Meta, Prismatic).
- Gem-Kompatibilität prüft Farbe, Unique-Gem Einschränkung und Level/Klasse.

### Enchants
- Single active enchant pro Item; überschreiben ist erlaubt, entfernt alte.
- Enchant Effekte werden in Stat-Recalc berücksichtigt und triggern `SetBonusUpdate` falls relevant.

### Reforge
- Temporärer Stattausch: `SourceStat`, `TargetStat`, `SourceValue`, `TargetValue`, `CostCurrency`, `PreviewHash`.
- Reforge speichert Hash, um Confirm gegen Preview zu validieren.

### SetBonuses
- Trackt pro `SetId` die Anzahl aktiver Teile und aktive Bonus-Stufen.
- Aktivierungsstatus wird über `SetBonusUpdate/Activate/Deactivate` verteilt.

### Outfits / Cosmetics
- Outfits speichern nur Slot→AppearanceRefs; keine Stats, keine Enchants.
- OutfitOperationen sind idempotent; `OutfitList` liefert Kanonical state.

---

## ✅ Equip/Unequip/Swap Regeln

- Atomarität: Server führt Item-Move, Stat-Recalc, Persistence, Event-Broadcast in einer Transaktion durch.  
- Idempotenz: Wiederholtes Equip derselben Kombination wird als Erfolgs-Idempotent behandelt, liefert gleiche `EquipmentSlotUpdate`.  
- Allowed Types: Slot akzeptiert nur kompatible ItemCategories; Two-Hand blockiert OffHand.  
- Unique-Equip: Server verhindert doppelte „Unique-Equipped“ Items (z.B. zwei gleiche Legendäre Ringe).  
- Level/Class/Spec Check: Item-Template Requirements müssen erfüllt sein; sonst `EquipItemResult` mit `REQUIREMENT_NOT_MET`.  
- Combat Restrictions: Optional Flag verhindert Equip in Combat; resultiert in `IN_COMBAT`.  
- Cooldown: Equip/Unequip Rate-Limit z.B. 2 pro Sekunde; bei Überschreitung `RATE_LIMITED`.  
- Swap: Client sendet zwei Slots oder ItemInstanceId + Slot; Server führt Doppel-Move atomar aus (keine Zwischenzustände).  
- Locking: Inventory und Equipment Slot werden gelockt während Operation, um Dupe-Rennen zu verhindern.  
- Ownership: Item muss dem Character gehören und nicht geliehen sein (LoanFlag).  
- Durability: Items mit `DurabilityCurrent=0` dürfen nicht neu equippt werden, bleiben aber im Slot bis Unequip/Repair.  
- PvP/Arena: Konfigurierbare Verbote (z.B. kein Waffenwechsel in Ranked Arena).  
- Appearance-only Items können nur in CosmeticLayer; GameplaySlot darf nicht CosmeticOnly sein.

---

## 🧪 Stat Recalculation & Sync

- Server recalculates Stats nach jeder Equipment-Änderung: BaseStats + ItemStats + Enchants + Gems + SetBonuses + Buffs.  
- Stat-Pipeline: Collect → Validate → Aggregate → Clamp → Publish (`StatUpdate` 602) → Persist.  
- Delta-Sync: `EquipmentSlotUpdate` liefert nur geänderte Slots; `EquipmentSync` liefert Snapshot.  
- Revisioning: Jede Änderung erhöht `LoadoutRevision` und `StatRevision`; Client verwirft ältere Deltas.  
- Combat Snapshot: Während Kampfrunden werden Statänderungen sofort angewandt, aber Damage-Replays nutzen die Post-Equip Stat-Revisionsnummer.  
- Server sends `SetBonusUpdate` bei Änderung der aktiven Set-Anzahl; wenn Bonus-State wechselt, sendet `SetBonusActivate/Deactivate`.

---

## 🎨 Cosmetics / Transmog

- Cosmetics nutzen identische Slots, aber `CosmeticLayer` Flag sorgt für reines Visual Update.  
- Cosmetic Equip überschreibt nur Aussehen; Stats bleiben aus Gameplay-Layer.  
- OutfitSave/Load/Delete arbeiten auf CosmeticLayer und GameplayLayer getrennt (Load unterstützt Parameter).  
- Transmog (über Enchant/ItemTransmog) referenziert existierende AppearanceItemIds; keine Statänderung.  
- Validation: Cosmetic Items müssen freigeschaltet und nicht gesperrt sein; Unique-Cosmetic Tags analog Unique-Equip.  
- Sync: `EquipmentSlotUpdate` enthält `CosmeticRef` und `IsCosmetic`; Clients rendern zweilagig.  
- Anti-Flicker: Server bündelt kosmetische Updates pro Tick, sendet gesammelt in `EquipmentSlotUpdate`.

---

## 🔧 Durability & Repair

- Durability sinkt serverseitig nur über Combat/Death/Environment Hooks, nie durch Client-Angaben.  
- `DurabilityUpdate` sendet delta und optional TriggerReason (Damage, Death, UseFailure).  
- `DurabilityWarning` wird bei Schwellwerten (25%, 10%, 0%) gesendet; client zeigt UI Warnung.  
- `ItemBroken` markiert Items mit 0 Haltbarkeit, Stats werden deaktiviert bis Repair/Unequip.  
- Reparatur erfolgt via Inventory/Vendor Messages (nicht in diesem Range), aber Durability-Events bleiben hier dokumentiert.  
- Persistenz: Durability wird nach jedem relevanten Event sofort gespeichert; verhindert Dupe/Desync nach Crash.

---

## 🔄 Sync, Deltas & Revisioning

- Snapshot: `EquipmentSync` liefert komplettes Loadout inkl. CosmeticLayer, Durability, Gems, Enchants, SetStatus.  
- Delta: `EquipmentSlotUpdate` enthält nur betroffene Slots mit Revision.  
- Correlation: Alle Requests tragen `ClientSequence`; Responses spiegeln ihn, damit UI korrelieren kann.  
- Reconnect: Client sendet `EquipmentSyncRequest` implizit via `EquipmentSync` Trigger (Server push beim Reconnect/ZoneState).  
- Ordering: Server garantiert Monotonie per `LoadoutRevision`; Client verwirft rückständige Deltas.  
- Compression: Messages können Kompression nutzen, sind aber klein gehalten durch Deltas.

---

## 🧱 DTOs / Interfaces

- Alle DTOs nutzen `[MessagePackObject]`, `[NetworkMessage(MessageType.X)]`, `[Key(0)] public MessageType Type => ...`.  
- Key-Order stabil: Keys beginnen bei 0 für Type, danach strikt aufsteigend.  
- Requests implementieren `IClientMessage`, Responses/Events implementieren `IServerMessage`.  
- Korrelation: Responses enthalten `ClientSequence` oder `RequestId` (ulong).  
- Items referenzieren `Guid ItemInstanceId` und `int TemplateId`; keine Statwerte vom Client.  
- Slot wird als `EquipmentSlot` Enum übertragen; CosmeticLayer als bool.

---

## 🧩 Enums / ErrorCodes / Flags

### EquipmentSlot (Auszug)
- Head, Chest, Legs, Feet, Hands, Wrist, Waist, Back, Neck, Shoulders, MainHand, OffHand, TwoHand, Ring1, Ring2, Trinket1, Trinket2, Tabard, Shirt, BeltEnchant, WeaponCosmetic, HeadCosmetic.

### EquipErrorCode
- SUCCESS, ITEM_NOT_FOUND, SLOT_INVALID, TYPE_MISMATCH, UNIQUE_VIOLATION, REQUIREMENT_NOT_MET, IN_COMBAT, COOLDOWN, ALREADY_EQUIPPED, DURABILITY_ZERO, RATE_LIMITED, OWNERSHIP_INVALID, BUSY, INTERNAL_ERROR.

### DurabilityWarningLevel
- THRESHOLD_25, THRESHOLD_10, BROKEN.

### ReforgeErrorCode
- SUCCESS, INVALID_ITEM, INVALID_STATS, COST_MISSING, PREVIEW_MISMATCH, NOT_ALLOWED, RATE_LIMITED, BUSY, INTERNAL_ERROR.

### GemResultCode
- SUCCESS, SOCKET_MISMATCH, GEM_NOT_FOUND, UNIQUE_GEM_VIOLATION, LEVEL_REQ_NOT_MET, ITEM_LOCKED, DURABILITY_ZERO, INTERNAL_ERROR.

### EnchantResultCode
- SUCCESS, INVALID_SCROLL, TYPE_MISMATCH, ITEM_LOCKED, DURABILITY_ZERO, CONFLICT, RATE_LIMITED, INTERNAL_ERROR.

### OutfitResultCode
- SUCCESS, NAME_TAKEN, NOT_FOUND, LIMIT_REACHED, INVALID_SLOT, NOT_UNLOCKED, INTERNAL_ERROR.

---

## ⚙️ Regeln & Sicherheit

- Anti-Dupe: Slot- und Inventory-Locks; serverseitige Compare-And-Swap auf `LoadoutRevision`.  
- Idempotenz: Wiederholte Requests mit gleicher `RequestId` liefern gleiche `EquipItemResult`.  
- Rate Limits: Konfigurierbar per Player; Default 2 Equip-Operationen pro Sekunde, 1 Reforge alle 5 Sekunden.  
- Authorization: Alle Client→Server Messages erfordern aktive Session (🔒) und Eigentum der Items.  
- Validation: Server überprüft Slot, ItemState, BoundState, Level/Class/Spec, CombatState, Durability.  
- Logging: Jede Änderung wird auditgeloggt mit `RequestId`, `ItemInstanceId`, `OldSlot`, `NewSlot`, `Revision`.  
- Security: Client sendet keine Statwerte; Server ignoriert unbekannte Felder dank MessagePack compatibility.  
- Error Handling: Fehlermeldungen sind generisch, keine internen Details (z.B. nicht „DB deadlock“).  
- Rollback: Bei Fehler rollback der gesamten Operation inkl. Stat-Recalc.  
- Concurrency: WeaponSwap und Swap-ähnliche Flows sind transaktional; keine Zwischen-Broadcasts.

---

## 📩 Aktive Messages 3900–3999

### EquipItem (3900)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel (UI-Driven)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client bittet den Server, ein Item aus dem Inventar in einen EquipmentSlot zu bewegen. Server validiert Slot-Kompatibilität, Requirements, Anti-Dupe und führt Stat-Recalc atomar aus. Unterstützt optionales Swap mit bereits belegtem Slot.

### Im Scope ✅
- Inventory→Equipment Move
- Optionaler Swap mit anderem Slot
- Validation gegen Slot-Regeln, Unique, Level/Class
- Idempotente Wiederholung via `RequestId`

### Nicht im Scope ❌
- Durability-Repair (Vendor/ItemRepair Messages)
- Cosmetic-Only Equip (separate CosmeticLayer Flag)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.EquipItem` | Ja |
| RequestId | ulong | Client-generierte Idempotenz-ID | Ja |
| ClientSequence | uint | Sequenz zur Korrelation | Ja |
| ItemInstanceId | Guid | Instanz im Inventar | Ja |
| TargetSlot | EquipmentSlot | Ziel-Slot | Ja |
| SwapWithSlot | EquipmentSlot? | Optionaler Slot für Swap | Nein |
| CosmeticOnly | bool | True = nur Visual Layer | Nein |

### Erwartete Response
- `EquipItemResult` (3901)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EquipItem)]
public class EquipItem : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.EquipItem;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid ItemInstanceId { get; set; }
    [Key(4)] public EquipmentSlot TargetSlot { get; set; }
    [Key(5)] public EquipmentSlot? SwapWithSlot { get; set; }
    [Key(6)] public bool CosmeticOnly { get; set; }
}
```

### Server-Verhalten
- Prüft Ownership, Cooldown, Combat-Flag, Slot-Kompatibilität, Unique-Einschränkungen, Durability>0.  
- Lockt Inventory Slot + TargetSlot (und SwapSlot falls vorhanden).  
- Bei Swap: Tauscht Items atomar, sonst verschiebt und räumt Inventory-Slot.  
- Rechnet Stats neu, aktualisiert Set-Boni, sendet `EquipmentSlotUpdate` und ggf. `SetBonusUpdate`.  
- Persistiert Loadout + Durability und hebt Locks.  
- Antwortet mit `EquipItemResult` inkl. finalem Revision.

### Client-Verhalten
- Disable UI während Pending-Request, zeigt Busy-State.  
- Bei Erfolg: Aktualisiert UI mit Response-Daten und erwartet `EquipmentSlotUpdate`.  
- Bei Fehler: Zeigt generischen Fehlercode, Inventar bleibt unverändert.  
- Wiederholte Requests mit gleicher `RequestId` nicht doppelt senden.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  EquipItem (3900)            │
  │─────────────────────────────►│
  │                              │ Validate + Lock + Move
  │                              │ Recalc Stats + Persist
  │  EquipItemResult (3901)      │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
  │  StatUpdate (602)            │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var equip = new EquipItem
{
    RequestId = 42,
    ClientSequence = 1001,
    ItemInstanceId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
    TargetSlot = EquipmentSlot.MainHand,
    SwapWithSlot = EquipmentSlot.OffHand,
    CosmeticOnly = false
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Equip durchgeführt |
| `ITEM_NOT_FOUND` | Item im Inventar fehlt |
| `SLOT_INVALID` | Slot existiert nicht für Klasse/Spezialisierung |
| `TYPE_MISMATCH` | Item-Typ passt nicht zum Slot |
| `UNIQUE_VIOLATION` | Unique-Equip verletzt |
| `REQUIREMENT_NOT_MET` | Level/Klasse/Spec nicht erfüllt |
| `IN_COMBAT` | Equip im Kampf verboten |
| `COOLDOWN` | Rate Limit überschritten |
| `DURABILITY_ZERO` | Item ist kaputt |
| `OWNERSHIP_INVALID` | Item gehört nicht dem Spieler |
| `BUSY` | Slot/Inventory gelockt |
| `INTERNAL_ERROR` | Generischer Serverfehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `EquipItemResult` | 3901 | Response |
| `EquipmentSlotUpdate` | 3905 | Delta nach Erfolg |
| `StatUpdate` | 602 | Stat-Delta |
| `DurabilityUpdate` | 3910 | Falls Durability angepasst wurde |
| `SetBonusUpdate` | 3950 | Bei Set-Änderung |

---

### EquipItemResult (3901)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein (Response auf authentifizierten Request)  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf `EquipItem`. Transportiert Erfolg/Fehler, finalen Slot-Status, Revision und optionale deltas (Durability, SetBonuses).

### Im Scope ✅
- Ack/Fail mit ErrorCode
- Korrelation über RequestId/ClientSequence
- Liefert aktuelle Revision
- Enthält optionales Snapshot-Segment des betroffenen Slots

### Nicht im Scope ❌
- Vollständiger Loadout-Snapshot (nutze `EquipmentSync`)
- Statdetails (separate StatUpdate 602)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `EquipItemResult` | Ja |
| RequestId | ulong | Spiegel des Requests | Ja |
| ClientSequence | uint | Spiegel des Requests | Ja |
| Success | bool | Ergebnis | Ja |
| ErrorCode | EquipErrorCode? | Fehler wenn Success=false | Nein |
| Slot | EquipmentSlot? | Betroffener Slot bei Erfolg | Nein |
| ItemInstanceId | Guid? | Ausgerüstete Instanz | Nein |
| LoadoutRevision | uint | Neue Revision bei Erfolg | Ja |
| DurabilityCurrent | int? | Aktueller Wert | Nein |
| DurabilityMax | int? | Maximalwert | Nein |
| CosmeticOnly | bool? | Ob Operation nur visuell war | Nein |

### Erwartete Response
- Keine (ist Response)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EquipItemResult)]
public class EquipItemResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EquipItemResult;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public bool Success { get; set; }
    [Key(4)] public EquipErrorCode? ErrorCode { get; set; }
    [Key(5)] public EquipmentSlot? Slot { get; set; }
    [Key(6)] public Guid? ItemInstanceId { get; set; }
    [Key(7)] public uint LoadoutRevision { get; set; }
    [Key(8)] public int? DurabilityCurrent { get; set; }
    [Key(9)] public int? DurabilityMax { get; set; }
    [Key(10)] public bool? CosmeticOnly { get; set; }
}
```

### Server-Verhalten
- Mappt RequestId -> Result; bei Idempotenz liefert gespeichertes Ergebnis.  
- Befüllt Slot/Item nur bei Success.  
- Fügt Durability-Felder hinzu, wenn der Move Durability beeinflusst (z.B. Re-bind).  
- Loggt Audit mit Revision.

### Client-Verhalten
- Korrelieren via RequestId/ClientSequence.  
- Bei Erfolg UI aktualisieren, auf nachfolgende `EquipmentSlotUpdate`/`StatUpdate` warten.  
- Bei Fehler UI Meldung + kein Optimistic Update mehr.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  EquipItem (3900)            │
  │─────────────────────────────►│
  │                              │
  │  EquipItemResult (3901)      │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resultOk = new EquipItemResult
{
    RequestId = 42,
    ClientSequence = 1001,
    Success = true,
    Slot = EquipmentSlot.MainHand,
    ItemInstanceId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
    LoadoutRevision = 12,
    DurabilityCurrent = 95,
    DurabilityMax = 100
};

var resultFail = new EquipItemResult
{
    RequestId = 43,
    ClientSequence = 1002,
    Success = false,
    ErrorCode = EquipErrorCode.UNIQUE_VIOLATION,
    LoadoutRevision = 11
};
```

### Error Codes
Verweise auf Tabelle bei `EquipItem`.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `EquipItem` | 3900 | Request |
| `EquipmentSlotUpdate` | 3905 | Folgedelta |
| `StatUpdate` | 602 | Stats nach Erfolg |

---

### UnequipItem (3902)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entfernt ein Item aus einem EquipmentSlot zurück ins Inventar. Kann auch Richtung anderer Inventory-Slot (Auto-Pick) erfolgen. Optional `ForceCosmeticOnly` für visuelle Layer.

### Im Scope ✅
- Equipment→Inventory Move
- Auto-Placement ins erste freie Inventar-Slot
- Idempotenz via RequestId
- Validierung gegen Combat/Restrictions

### Nicht im Scope ❌
- Item-Delete (separate Message)
- Reparatur

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `UnequipItem` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| SourceSlot | EquipmentSlot | Slot der entfernt wird | Ja |
| CosmeticOnly | bool | Nur CosmeticLayer | Nein |

### Erwartete Response
- `UnequipItemResult` (3903)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.UnequipItem)]
public class UnequipItem : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.UnequipItem;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public EquipmentSlot SourceSlot { get; set; }
    [Key(4)] public bool CosmeticOnly { get; set; }
}
```

### Server-Verhalten
- Prüft Slot-Inhalt, Combat, Cooldown, Inventory-Space.  
- Lockt Slot + Ziel-Inventory-Slot.  
- Verschiebt Item, leert Slot, recalculates Stats, sendet `EquipmentSlotUpdate`.  
- Persistiert LoadoutRevision.  
- Antwortet mit Result.

### Client-Verhalten
- UI sperrt Slot während pending.  
- Bei Erfolg: erwarte `EquipmentSlotUpdate` + `InventorySlotUpdate`.  
- Bei Fehler: UI revert.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  UnequipItem (3902)          │
  │─────────────────────────────►│
  │                              │
  │  UnequipItemResult (3903)    │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var unequip = new UnequipItem
{
    RequestId = 77,
    ClientSequence = 2001,
    SourceSlot = EquipmentSlot.OffHand,
    CosmeticOnly = false
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Unequip durchgeführt |
| `SLOT_INVALID` | Slot existiert nicht |
| `ITEM_NOT_FOUND` | Slot leer |
| `IN_COMBAT` | Unequip nicht erlaubt |
| `COOLDOWN` | Rate Limit |
| `INVENTORY_FULL` | Kein Zielplatz |
| `BUSY` | Lock aktiv |
| `INTERNAL_ERROR` | Generischer Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `UnequipItemResult` | 3903 | Response |
| `EquipmentSlotUpdate` | 3905 | Delta |
| `InventorySlotUpdate` | 501 | Zielplatz |

---

### UnequipItemResult (3903)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf Unequip. Bestätigt Erfolg und gibt Ziel-Inventory-Slot sowie Revision aus.

### Im Scope ✅
- Erfolg/Fehler
- Ziel-Inventory-Slot bei Erfolg
- Revision

### Nicht im Scope ❌
- Vollständiger Inventory-Snapshot

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `UnequipItemResult` | Ja |
| RequestId | ulong | Spiegel | Ja |
| ClientSequence | uint | Spiegel | Ja |
| Success | bool | Ergebnis | Ja |
| ErrorCode | EquipErrorCode? | Fehler | Nein |
| SourceSlot | EquipmentSlot? | Slot | Nein |
| TargetInventorySlot | int? | Zielindex | Nein |
| ItemInstanceId | Guid? | Item | Nein |
| LoadoutRevision | uint | Revision | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.UnequipItemResult)]
public class UnequipItemResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.UnequipItemResult;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public bool Success { get; set; }
    [Key(4)] public EquipErrorCode? ErrorCode { get; set; }
    [Key(5)] public EquipmentSlot? SourceSlot { get; set; }
    [Key(6)] public int? TargetInventorySlot { get; set; }
    [Key(7)] public Guid? ItemInstanceId { get; set; }
    [Key(8)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Füllt SourceSlot/TargetInventorySlot nur bei Erfolg.  
- Sendet nachgelagert `EquipmentSlotUpdate` und `InventorySlotUpdate`.  
- Setzt LoadoutRevision.

### Client-Verhalten
- Verwendet Revision, um alte Deltas zu verwerfen.  
- Aktualisiert UI Slot und Inventar.  
- Zeigt Fehlermeldung bei ErrorCode.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  UnequipItem (3902)          │
  │─────────────────────────────►│
  │                              │
  │  UnequipItemResult (3903)    │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var res = new UnequipItemResult
{
    RequestId = 77,
    ClientSequence = 2001,
    Success = true,
    SourceSlot = EquipmentSlot.OffHand,
    TargetInventorySlot = 12,
    ItemInstanceId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"),
    LoadoutRevision = 13
};
```

### Error Codes
Siehe `UnequipItem`.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `UnequipItem` | 3902 | Request |
| `EquipmentSlotUpdate` | 3905 | Delta |

---

### EquipmentSync (3904)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login, Reconnect, ZoneChange)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Vollständiger Snapshot des Loadouts (Gameplay + Cosmetic), inkl. Durability, Gems, Enchants, SetStatus und Revision. Wird bei Reconnect, ZoneState oder expliziter Sync-Anforderung gesendet.

### Im Scope ✅
- Komplettes Loadout
- Both layers (Gameplay/Cosmetic)
- Durability und Slot-States
- Revision und StatRevision

### Nicht im Scope ❌
- Inventory-Details (separate Inventory Messages)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `EquipmentSync` | Ja |
| LoadoutRevision | uint | Aktuelle Revision | Ja |
| StatRevision | uint | Stat Revision | Ja |
| Items | List<EquippedSlotDto> | Slot → Item | Ja |
| CosmeticItems | List<EquippedSlotDto> | Slot → Cosmetic | Nein |

**EquippedSlotDto**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Slot | EquipmentSlot | Slot |
| ItemInstanceId | Guid | Ausgerüstet |
| TemplateId | int | Template |
| DurabilityCurrent | int | Haltbarkeit |
| DurabilityMax | int | Max |
| Gems | Guid[] | Socket-Gems |
| EnchantId | int? | Aktiver Enchant |
| CosmeticRef | int? | Visual override |
| SetId | int? | Setzugehörigkeit |
| Revision | uint | Slot Revision |

### Erwartete Response
- Keine (Server-Push)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EquipmentSync)]
public class EquipmentSync : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EquipmentSync;
    [Key(1)] public uint LoadoutRevision { get; set; }
    [Key(2)] public uint StatRevision { get; set; }
    [Key(3)] public List<EquippedSlotDto> Items { get; set; } = new();
    [Key(4)] public List<EquippedSlotDto>? CosmeticItems { get; set; }
}
```

### Server-Verhalten
- Senden nach Login/ZoneState/Reconnect und nach OutfitLoad.  
- Enthält nur Slots mit Items; leere Slots werden ausgelassen oder mit `ItemInstanceId = Guid.Empty` markiert (konfigurierbar).  
- Garantiert monotone Revision.

### Client-Verhalten
- Ersetzt lokale Ausrüstungsdaten vollständig.  
- Validiert Revision gegen bisherige Deltas.  
- Triggert Render-Refresh und Stat-Kalkulation auf Clientseite (nur Anzeige).

### Flow-Diagramm
```
Server State Change
   │
   │  EquipmentSync (3904)
   │──────────────────────► Client
   │
```

### Beispiel Payloads
```csharp
var sync = new EquipmentSync
{
    LoadoutRevision = 15,
    StatRevision = 101,
    Items =
    {
        new EquippedSlotDto
        {
            Slot = EquipmentSlot.Head,
            ItemInstanceId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            TemplateId = 9001,
            DurabilityCurrent = 80,
            DurabilityMax = 100,
            Gems = new[] { Guid.Parse("99999999-aaaa-bbbb-cccc-dddddddddddd") },
            EnchantId = 301,
            SetId = 12,
            Revision = 7
        }
    }
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `EquipmentSlotUpdate` | 3905 | Delta |
| `StatFullSync` | 603 | Stat Snapshot |
| `ZoneState` | 102 | Liefert Sync an |

---

### EquipmentSlotUpdate (3905)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (bei jeder Änderung)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Delta-Update für einzelne oder mehrere Slots nach Equip/Unequip/Swap/Reforge/CosmeticChange. Enthält Revision, ItemRefs, Durability und CosmeticRef.

### Im Scope ✅
- Mehrere Slots in einer Nachricht
- Gameplay + Cosmetic Layer
- Optionale StatRevision

### Nicht im Scope ❌
- Vollständiger Snapshot (EquipmentSync)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `EquipmentSlotUpdate` | Ja |
| LoadoutRevision | uint | Neue Revision | Ja |
| Slots | List<EquippedSlotDto> | Änderungen | Ja |
| StatRevision | uint? | Optional neue StatRevision | Nein |

### Erwartete Response
- Keine (Event)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EquipmentSlotUpdate)]
public class EquipmentSlotUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EquipmentSlotUpdate;
    [Key(1)] public uint LoadoutRevision { get; set; }
    [Key(2)] public List<EquippedSlotDto> Slots { get; set; } = new();
    [Key(3)] public uint? StatRevision { get; set; }
}
```

### Server-Verhalten
- Bündelt alle Slot-Änderungen eines Ticks.  
- Setzt LoadoutRevision auf höchste nach Operation.  
- Optional StatRevision wenn Stats sich änderten.

### Client-Verhalten
- Verarbeitet nur, wenn Revision >= lokale.  
- Aktualisiert UI Slots und CosmeticLayer.  
- Bei StatRevision: Erwartet `StatUpdate` oder nutzt Delta sofort.

### Flow-Diagramm
```
Server
  │  EquipmentSlotUpdate (3905)
  │───────────────────────────► Client
```

### Beispiel Payloads
```csharp
var delta = new EquipmentSlotUpdate
{
    LoadoutRevision = 16,
    Slots =
    {
        new EquippedSlotDto
        {
            Slot = EquipmentSlot.MainHand,
            ItemInstanceId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            TemplateId = 9100,
            DurabilityCurrent = 92,
            DurabilityMax = 100,
            Gems = Array.Empty<Guid>(),
            EnchantId = 0,
            SetId = null,
            Revision = 9
        }
    },
    StatRevision = 102
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `EquipmentSync` | 3904 | Snapshot |
| `EquipItemResult` | 3901 | führt zu Delta |
| `UnequipItemResult` | 3903 | führt zu Delta |

---

### DurabilityUpdate (3910)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel (Kampf, Tod)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Informiert Client über Haltbarkeitsänderungen einzelner Items. Wird gepusht bei Treffer, Tod, Nutzung, Reparatur.

### Im Scope ✅
- Delta für Durability
- Mehrere Slots pro Nachricht
- TriggerReason optional

### Nicht im Scope ❌
- Repair-Anfrage (Vendor/ItemRepair)
- Equip-Operationen (separate Messages)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DurabilityUpdate` | Ja |
| Entries | List<DurabilityEntry> | Änderungen | Ja |

**DurabilityEntry**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Slot | EquipmentSlot | Betroffener Slot |
| ItemInstanceId | Guid | Item |
| DurabilityCurrent | int | Neuer Wert |
| DurabilityMax | int | Max |
| Trigger | string? | z.B. "Combat", "Death" |

### Erwartete Response
- Keine (Event)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DurabilityUpdate)]
public class DurabilityUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.DurabilityUpdate;
    [Key(1)] public List<DurabilityEntry> Entries { get; set; } = new();
}
```

### Server-Verhalten
- Aggregiert mehrere Quellen pro Tick.  
- Wenn Durability auf 0 fällt, zusätzlich `ItemBroken`.  
- Persistiert neue Werte.

### Client-Verhalten
- Aktualisiert UI (Farbbalken).  
- Prüft Trigger, zeigt Warnungen oder Sounds.  
- Wenn 0: markiert Item als broken lokal.

### Flow-Diagramm
```
Server
  │  DurabilityUpdate (3910)
  │────────────────────────► Client
```

### Beispiel Payloads
```csharp
var dur = new DurabilityUpdate
{
    Entries =
    {
        new DurabilityEntry
        {
            Slot = EquipmentSlot.Chest,
            ItemInstanceId = Guid.Parse("cccccccc-dddd-eeee-ffff-111111111111"),
            DurabilityCurrent = 24,
            DurabilityMax = 100,
            Trigger = "Death"
        }
    }
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `DurabilityWarning` | 3911 | Schwelle erreicht |
| `ItemBroken` | 3912 | Bei 0 |

---

### DurabilityWarning (3911)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Schwellen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Warnung, wenn Haltbarkeit definierte Schwellwerte unterschreitet. Hilft UI, Warnbanner oder Audio abzuspielen.

### Im Scope ✅
- Schwellwerte 25%, 10%, 0%
- Enthält Slot, Item, Level
- Kann gebündelt mehrere Items enthalten

### Nicht im Scope ❌
- Reparaturkostenberechnung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `DurabilityWarning` | Ja |
| Entries | List<DurabilityWarningEntry> | Warnungen | Ja |

**DurabilityWarningEntry**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Slot | EquipmentSlot | Slot |
| ItemInstanceId | Guid | Item |
| Level | DurabilityWarningLevel | Schwelle |
| DurabilityCurrent | int | Wert |
| DurabilityMax | int | Max |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.DurabilityWarning)]
public class DurabilityWarning : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.DurabilityWarning;
    [Key(1)] public List<DurabilityWarningEntry> Entries { get; set; } = new();
}
```

### Server-Verhalten
- Sendet einmal pro Schwelle (25/10/0) pro Item; erneute Warnung erst nach Reparatur/Threshold Crossing.  
- Wenn Level=BROKEN, zusätzlich `ItemBroken`.

### Client-Verhalten
- UI Banner, Icon-Farbe Rot, optional Auto-Open Repair hint.  
- Speichert letzte Warnung pro Item, um Spam zu verhindern.

### Flow-Diagramm
```
Server
  │  DurabilityWarning (3911)
  │────────────────────────► Client
```

### Beispiel Payloads
```csharp
var warn = new DurabilityWarning
{
    Entries =
    {
        new DurabilityWarningEntry
        {
            Slot = EquipmentSlot.Head,
            ItemInstanceId = Guid.Parse("99999999-0000-1111-2222-333333333333"),
            Level = DurabilityWarningLevel.THRESHOLD_10,
            DurabilityCurrent = 9,
            DurabilityMax = 100
        }
    }
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `DurabilityUpdate` | 3910 | löst Warnungen aus |
| `ItemBroken` | 3912 | wenn 0 erreicht |

---

### ItemBroken (3912)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Signalisiert, dass die Haltbarkeit eines Items 0 erreicht hat. Item bleibt ausgerüstet, aber Stats deaktiviert bis Reparatur/Unequip.

### Im Scope ✅
- Kennzeichnet broken Status
- Liefert Slot und Item
- Kann mit DurabilityUpdate gemeinsam gesendet werden

### Nicht im Scope ❌
- Reparaturprozess

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ItemBroken` | Ja |
| Slot | EquipmentSlot | Betroffener Slot | Ja |
| ItemInstanceId | Guid | Item | Ja |
| LoadoutRevision | uint | Revision | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ItemBroken)]
public class ItemBroken : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ItemBroken;
    [Key(1)] public EquipmentSlot Slot { get; set; }
    [Key(2)] public Guid ItemInstanceId { get; set; }
    [Key(3)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Setzt Item-StatContribution auf 0 bis Reparatur.  
- Stößt StatRecalc an und sendet `StatUpdate`.  
- Persistiert broken flag.

### Client-Verhalten
- Markiert Item rot, blendet Statverlust ein.  
- Optional Auto-Open Repair hint.

### Flow-Diagramm
```
Server
  │  ItemBroken (3912)
  │──────────────────► Client
```

### Beispiel Payloads
```csharp
var broken = new ItemBroken
{
    Slot = EquipmentSlot.Chest,
    ItemInstanceId = Guid.Parse("44444444-5555-6666-7777-888888888888"),
    LoadoutRevision = 17
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `DurabilityUpdate` | 3910 | Vorheriges Delta |
| `DurabilityWarning` | 3911 | Schwelle |

---

### GemSocket (3920)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fügt einen Edelstein in einen bestimmten Socket eines Items im EquipmentSlot oder Inventar ein. Server validiert Socket-Farbe, Unique-Gem, Level-Anforderungen und setzt Stat-Recalc.

### Im Scope ✅
- Gem einsetzen in angegebenen SocketIndex
- Optional `IsCosmetic` für reine Visual-Gems (falls unterstützt)
- Idempotenz via RequestId

### Nicht im Scope ❌
- Gem-Entfernung (separate GemRemove)
- Gem-Crafting

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GemSocket` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| ItemInstanceId | Guid | Ziel-Item | Ja |
| SocketIndex | byte | Index | Ja |
| GemInstanceId | Guid | Gem | Ja |
| FromInventorySlot | int? | Quelle im Inventar | Nein |

### Erwartete Response
- `GemSocketResult` (3921)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GemSocket)]
public class GemSocket : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.GemSocket;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid ItemInstanceId { get; set; }
    [Key(4)] public byte SocketIndex { get; set; }
    [Key(5)] public Guid GemInstanceId { get; set; }
    [Key(6)] public int? FromInventorySlot { get; set; }
}
```

### Server-Verhalten
- Validiert Socket existiert, Farbe passt, Gem einzigartig, Durability>0.  
- Entfernt Gem aus Inventory Slot, setzt in Item, erhöht Slot-Revision, recalculates Stats.  
- Persistiert und sendet `GemSocketResult` + `EquipmentSlotUpdate`.

### Client-Verhalten
- Sperrt UI-Slot während Operation.  
- Bei Erfolg: aktualisiert UI und erwartet Delta.  
- Bei Fehler: belässt Gem im Inventar.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  GemSocket (3920)            │
  │─────────────────────────────►│
  │                              │ Validate + Apply
  │  GemSocketResult (3921)      │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var socket = new GemSocket
{
    RequestId = 5001,
    ClientSequence = 3001,
    ItemInstanceId = Guid.Parse("aaaaaaaa-0000-0000-0000-aaaaaaaaaaaa"),
    SocketIndex = 1,
    GemInstanceId = Guid.Parse("bbbbbbbb-0000-0000-0000-bbbbbbbbbbbb"),
    FromInventorySlot = 15
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Gem eingesetzt |
| `SOCKET_MISMATCH` | Falsche Farbe |
| `GEM_NOT_FOUND` | Gem fehlt |
| `UNIQUE_GEM_VIOLATION` | Gleicher Unique-Gem bereits aktiv |
| `LEVEL_REQ_NOT_MET` | Anforderungen nicht erfüllt |
| `ITEM_LOCKED` | Slot gesperrt |
| `DURABILITY_ZERO` | Item kaputt |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `GemSocketResult` | 3921 | Response |
| `EquipmentSlotUpdate` | 3905 | Delta |

---

### GemSocketResult (3921)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GemSocket oder GemRemove (Operation=Add/Remove). Liefert Erfolg/Fehler, Slot-Revision und optional zurückgegebene GemInstanceId.

### Im Scope ✅
- Ack/Fail für Gem Add/Remove
- Operation-Feld zur Unterscheidung
- Revision + Slot Info

### Nicht im Scope ❌
- Vollständige Equipment-Deltas (kommen über EquipmentSlotUpdate)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GemSocketResult` | Ja |
| RequestId | ulong | Spiegel | Ja |
| ClientSequence | uint | Spiegel | Ja |
| Success | bool | Ergebnis | Ja |
| ErrorCode | GemResultCode? | Fehler | Nein |
| Operation | string | "Add" oder "Remove" | Ja |
| ItemInstanceId | Guid? | Item | Nein |
| SocketIndex | byte? | Index | Nein |
| GemInstanceId | Guid? | Gem (Add oder zurückgegeben bei Remove) | Nein |
| LoadoutRevision | uint | Revision | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GemSocketResult)]
public class GemSocketResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GemSocketResult;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public bool Success { get; set; }
    [Key(4)] public GemResultCode? ErrorCode { get; set; }
    [Key(5)] public string Operation { get; set; } = string.Empty;
    [Key(6)] public Guid? ItemInstanceId { get; set; }
    [Key(7)] public byte? SocketIndex { get; set; }
    [Key(8)] public Guid? GemInstanceId { get; set; }
    [Key(9)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Reused für Remove und Add; Operation kennzeichnet.  
- Stellt sicher Idempotenz via RequestId Cache.

### Client-Verhalten
- Unterscheidet Operation für UI.  
- Nutzt LoadoutRevision zur Delta-Verarbeitung.  
- Bei Fehler: Zeigt Fehlercode, belässt Items unverändert.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  GemSocket/GemRemove         │
  │─────────────────────────────►│
  │                              │
  │  GemSocketResult (3921)      │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var resAdd = new GemSocketResult
{
    RequestId = 5001,
    ClientSequence = 3001,
    Success = true,
    Operation = "Add",
    ItemInstanceId = Guid.Parse("aaaaaaaa-0000-0000-0000-aaaaaaaaaaaa"),
    SocketIndex = 1,
    GemInstanceId = Guid.Parse("bbbbbbbb-0000-0000-0000-bbbbbbbbbbbb"),
    LoadoutRevision = 18
};
```

### Error Codes
Siehe `GemSocket`.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `GemSocket` | 3920 | Request |
| `GemRemove` | 3922 | Request |
| `EquipmentSlotUpdate` | 3905 | Delta |

---

### GemRemove (3922)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entfernt einen Edelstein aus einem Socket und legt ihn ins Inventar zurück (oder zerstört ihn falls konfiguriert). Antwort über `GemSocketResult` mit Operation="Remove".

### Im Scope ✅
- Gem entfernen aus SocketIndex
- Optional zerstören statt zurückgeben (Flag)
- Idempotenz via RequestId

### Nicht im Scope ❌
- Gem austauschen (nutze Remove + Socket oder direkten Socket mit Replace-Flag falls konfiguriert)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GemRemove` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| ItemInstanceId | Guid | Item | Ja |
| SocketIndex | byte | Index | Ja |
| DestroyGem | bool | True = Gem löschen | Nein |

### Erwartete Response
- `GemSocketResult` (3921) mit `Operation="Remove"`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GemRemove)]
public class GemRemove : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.GemRemove;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid ItemInstanceId { get; set; }
    [Key(4)] public byte SocketIndex { get; set; }
    [Key(5)] public bool DestroyGem { get; set; }
}
```

### Server-Verhalten
- Prüft Socket besetzt, ItemStatus, Durability>0.  
- Entfernt Gem, legt ins Inventar falls DestroyGem=false und Platz vorhanden.  
- Recalc Stats, sendet EquipmentSlotUpdate und GemSocketResult(Operation Remove).

### Client-Verhalten
- Erwartet EquipmentSlotUpdate + optional InventoryUpdate.  
- Zeigt Fehler bei fehlendem Platz oder Restriktionen.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  GemRemove (3922)            │
  │─────────────────────────────►│
  │                              │
  │  GemSocketResult (3921)      │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var remove = new GemRemove
{
    RequestId = 5002,
    ClientSequence = 3002,
    ItemInstanceId = Guid.Parse("aaaaaaaa-0000-0000-0000-aaaaaaaaaaaa"),
    SocketIndex = 1,
    DestroyGem = false
};
```

### Error Codes
Siehe `GemSocket` (Operation Remove teilt Codes).

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `GemSocketResult` | 3921 | Response |
| `EquipmentSlotUpdate` | 3905 | Delta |

---

### EnchantApply (3930)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Wendet einen Enchant-Scroll auf ein Item an. Überschreibt vorhandenen Enchant. Server validiert Item-Typ, Scroll-Kompatibilität, Durability, Level.

### Im Scope ✅
- Enchant anwenden/überschreiben
- Idempotenz via RequestId
- Entfernt Scroll aus Inventar

### Nicht im Scope ❌
- Enchant entfernen (EnchantRemove)
- Permanenter Visual-Transmog (separate Cosmetic)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `EnchantApply` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| ItemInstanceId | Guid | Ziel | Ja |
| ScrollInstanceId | Guid | Enchant-Scroll | Ja |
| FromInventorySlot | int? | Scroll Quelle | Nein |

### Erwartete Response
- `EnchantApplyResult` (3931)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EnchantApply)]
public class EnchantApply : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.EnchantApply;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid ItemInstanceId { get; set; }
    [Key(4)] public Guid ScrollInstanceId { get; set; }
    [Key(5)] public int? FromInventorySlot { get; set; }
}
```

### Server-Verhalten
- Validiert Scroll kompatibel, Item nicht broken, Klasse/Level.  
- Entfernt Scroll aus Inventar, setzt Enchant, recalculates Stats.  
- Antwortet mit Result, sendet EquipmentSlotUpdate.

### Client-Verhalten
- Zeigt Cast-Bar/Progress falls benötigt.  
- Bei Erfolg: UI aktualisieren, erwartet Delta.  
- Bei Fehler: Scroll bleibt im Inventar.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  EnchantApply (3930)         │
  │─────────────────────────────►│
  │                              │
  │  EnchantApplyResult (3931)   │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var ench = new EnchantApply
{
    RequestId = 7001,
    ClientSequence = 4001,
    ItemInstanceId = Guid.Parse("aaaaaaaa-1111-2222-3333-444444444444"),
    ScrollInstanceId = Guid.Parse("dddddddd-1111-2222-3333-444444444444"),
    FromInventorySlot = 8
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Enchant angewendet |
| `INVALID_SCROLL` | Scroll-Typ falsch |
| `TYPE_MISMATCH` | Item akzeptiert keinen Enchant |
| `ITEM_LOCKED` | Slot gesperrt |
| `DURABILITY_ZERO` | Item kaputt |
| `CONFLICT` | Enchant kollidiert mit bestehendem |
| `RATE_LIMITED` | Zu viele Versuche |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `EnchantApplyResult` | 3931 | Response |
| `EnchantRemove` | 3932 | Gegenstück |

---

### EnchantApplyResult (3931)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf EnchantApply. Enthält Erfolg/Fehler, neue EnchantId und Revision.

### Im Scope ✅
- Ack/Fail
- Neue EnchantId
- LoadoutRevision

### Nicht im Scope ❌
- SetBonuses (separate Messages)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `EnchantApplyResult` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| Success | bool | Ergebnis | Ja |
| ErrorCode | EnchantResultCode? | Fehler | Nein |
| ItemInstanceId | Guid? | Item | Nein |
| EnchantId | int? | Neue ID | Nein |
| LoadoutRevision | uint | Revision | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EnchantApplyResult)]
public class EnchantApplyResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EnchantApplyResult;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public bool Success { get; set; }
    [Key(4)] public EnchantResultCode? ErrorCode { get; set; }
    [Key(5)] public Guid? ItemInstanceId { get; set; }
    [Key(6)] public int? EnchantId { get; set; }
    [Key(7)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Bei Erfolg: EnchantId gesetzt.  
- Bei Fehler: Scroll rollback.  
- Sends EquipmentSlotUpdate separately.

### Client-Verhalten
- Verknüpft mit UI; bei Erfolg zeigt neuen Enchant.  
- Bei Fehler belässt Scroll im Inventar (durch Rollback Update).

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  EnchantApply (3930)         │
  │─────────────────────────────►│
  │                              │
  │  EnchantApplyResult (3931)   │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var enchRes = new EnchantApplyResult
{
    RequestId = 7001,
    ClientSequence = 4001,
    Success = true,
    ItemInstanceId = Guid.Parse("aaaaaaaa-1111-2222-3333-444444444444"),
    EnchantId = 305,
    LoadoutRevision = 19
};
```

### Error Codes
Siehe `EnchantApply`.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `EnchantApply` | 3930 | Request |
| `EnchantRemove` | 3932 | Gegenvorgang |

---

### EnchantRemove (3932)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entfernt aktiven Enchant von einem Item. Optional Rückgewinnungs-Flag (Shard/Essence). Stat-Recalc erfolgt.

### Im Scope ✅
- Enchant löschen
- Idempotenz via RequestId
- Optional Shard-Rückgabe

### Nicht im Scope ❌
- Neues Enchant (EnchantApply)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `EnchantRemove` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| ItemInstanceId | Guid | Item | Ja |
| Refund | bool | Shard zurückgeben | Nein |

### Erwartete Response
- `EnchantApplyResult` (3931) wird wiederverwendet mit `Success/Failure` und `EnchantId=null`.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EnchantRemove)]
public class EnchantRemove : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.EnchantRemove;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid ItemInstanceId { get; set; }
    [Key(4)] public bool Refund { get; set; }
}
```

### Server-Verhalten
- Prüft Item, Enchant vorhanden, Durability>0.  
- Entfernt Enchant, gibt Shard zurück wenn Refund=true.  
- Recalc Stats, sendet EnchantApplyResult(Success, EnchantId null) + EquipmentSlotUpdate.

### Client-Verhalten
- UI aktualisiert Enchant-Icon, erwartet Delta.  
- Nutzt Result zur Anzeige von Shard-Rückgabe.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  EnchantRemove (3932)        │
  │─────────────────────────────►│
  │                              │
  │  EnchantApplyResult (3931)   │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var rem = new EnchantRemove
{
    RequestId = 7002,
    ClientSequence = 4002,
    ItemInstanceId = Guid.Parse("aaaaaaaa-1111-2222-3333-444444444444"),
    Refund = true
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Enchant entfernt |
| `ITEM_NOT_FOUND` | Item fehlt |
| `CONFLICT` | Kein Enchant vorhanden |
| `ITEM_LOCKED` | Gesperrt |
| `DURABILITY_ZERO` | Kaputt |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `EnchantApplyResult` | 3931 | Response |
| `EnchantApply` | 3930 | Gegenrichtung |

---

### ReforgeOpen (3940)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet Reforge-UI für bestimmtes Item, um verfügbare Stat-Optionen anzufragen. Dient als Request für Preview.

### Im Scope ✅
- Anfrage nach Reforge-Optionen
- Idempotent via RequestId

### Nicht im Scope ❌
- Stat-Anwendung (Preview/Confirm separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ReforgeOpen` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| ItemInstanceId | Guid | Item | Ja |

### Erwartete Response
- `ReforgePreview` (3941)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReforgeOpen)]
public class ReforgeOpen : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReforgeOpen;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid ItemInstanceId { get; set; }
}
```

### Server-Verhalten
- Prüft Item eligible, nicht in combat lock.  
- Berechnet mögliche Stat-Konvertierungen, sendet Preview.

### Client-Verhalten
- Öffnet UI bei Preview-Erhalt.  
- Speichert RequestId für spätere Confirm-Validierung.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  ReforgeOpen (3940)          │
  │─────────────────────────────►│
  │                              │
  │  ReforgePreview (3941)       │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var open = new ReforgeOpen
{
    RequestId = 8001,
    ClientSequence = 5001,
    ItemInstanceId = Guid.Parse("12121212-3434-5656-7878-909090909090")
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Preview folgt |
| `INVALID_ITEM` | Nicht reforgable |
| `BUSY` | Item gesperrt |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReforgePreview` | 3941 | Response |
| `ReforgeConfirm` | 3942 | Folge |

---

### ReforgePreview (3941)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ReforgeOpen. Enthält mögliche Stat-Konvertierungen, Kosten und PreviewHash, der bei Confirm benötigt wird.

### Im Scope ✅
- Liste der möglichen Reforges
- Kosten (Currency, Amount)
- PreviewHash

### Nicht im Scope ❌
- Anwendung der Änderung (Confirm)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ReforgePreview` | Ja |
| RequestId | ulong | Spiegel | Ja |
| ClientSequence | uint | Spiegel | Ja |
| ItemInstanceId | Guid | Item | Ja |
| Options | List<ReforgeOptionDto> | Möglichkeiten | Ja |
| PreviewHash | string | Hash | Ja |

**ReforgeOptionDto**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SourceStat | string | Abzugebender Stat |
| TargetStat | string | Zielstat |
| SourceValue | int | Verlust |
| TargetValue | int | Gewinn |
| CostCurrencyId | int | Währung |
| CostAmount | int | Kosten |

### Erwartete Response
- Keine (ist Response), aber client sendet `ReforgeConfirm`.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReforgePreview)]
public class ReforgePreview : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ReforgePreview;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid ItemInstanceId { get; set; }
    [Key(4)] public List<ReforgeOptionDto> Options { get; set; } = new();
    [Key(5)] public string PreviewHash { get; set; } = string.Empty;
}
```

### Server-Verhalten
- Generiert deterministische Optionen.  
- PreviewHash enthält Options + ItemState + Timestamp.  
- Cache zum Vergleich bei Confirm.

### Client-Verhalten
- Zeigt Optionen, ermöglicht Auswahl.  
- Speichert PreviewHash und gewählte Option für Confirm.

### Flow-Diagramm
```
Server
  │  ReforgePreview (3941)
  │──────────────────────► Client
```

### Beispiel Payloads
```csharp
var prev = new ReforgePreview
{
    RequestId = 8001,
    ClientSequence = 5001,
    ItemInstanceId = Guid.Parse("12121212-3434-5656-7878-909090909090"),
    PreviewHash = "abc123",
    Options =
    {
        new ReforgeOptionDto
        {
            SourceStat = "Crit",
            TargetStat = "Haste",
            SourceValue = 50,
            TargetValue = 50,
            CostCurrencyId = 1,
            CostAmount = 500
        }
    }
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReforgeOpen` | 3940 | Request |
| `ReforgeConfirm` | 3942 | Folge |

---

### ReforgeConfirm (3942)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigt eine ausgewählte Reforge-Option anhand PreviewHash. Server prüft Hash, Kosten und führt Reforge durch.

### Im Scope ✅
- Option bestätigen
- Kostenzahlung
- Hash-Validierung

### Nicht im Scope ❌
- Neue Preview erzeugen

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ReforgeConfirm` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| ItemInstanceId | Guid | Item | Ja |
| PreviewHash | string | Hash aus Preview | Ja |
| SelectedIndex | byte | Gewählte Option | Ja |

### Erwartete Response
- `ReforgeResult` (3943)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReforgeConfirm)]
public class ReforgeConfirm : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReforgeConfirm;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid ItemInstanceId { get; set; }
    [Key(4)] public string PreviewHash { get; set; } = string.Empty;
    [Key(5)] public byte SelectedIndex { get; set; }
}
```

### Server-Verhalten
- Validiert Hash == last PreviewHash; Option Index gültig; Kosten verfügbar.  
- Deducts cost, applies stat swap, recalculates Stats, increments Revision.  
- Sends ReforgeResult + EquipmentSlotUpdate.

### Client-Verhalten
- Zeigt Busy; nach Erfolg aktualisiert UI.  
- Bei Hash-Mismatch fordert neuen Preview an.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  ReforgeConfirm (3942)       │
  │─────────────────────────────►│
  │                              │
  │  ReforgeResult (3943)        │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var conf = new ReforgeConfirm
{
    RequestId = 8002,
    ClientSequence = 5002,
    ItemInstanceId = Guid.Parse("12121212-3434-5656-7878-909090909090"),
    PreviewHash = "abc123",
    SelectedIndex = 0
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Reforge angewendet |
| `PREVIEW_MISMATCH` | Hash passt nicht |
| `INVALID_STATS` | Option ungültig |
| `COST_MISSING` | Kosten nicht vorhanden |
| `NOT_ALLOWED` | Item nicht reforgable |
| `RATE_LIMITED` | Rate Limit |
| `BUSY` | Lock |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReforgeResult` | 3943 | Response |
| `ReforgePreview` | 3941 | Basis |

---

### ReforgeResult (3943)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ReforgeConfirm. Enthält Erfolg/Fehler, angewendete Stat-Änderung und neue Revision.

### Im Scope ✅
- Ergebnis mit Stat-Werten
- Kostenbestätigung
- Revision

### Nicht im Scope ❌
- Vollständiger Slot-Snapshot (EquipmentSlotUpdate folgt)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ReforgeResult` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| Success | bool | Ergebnis | Ja |
| ErrorCode | ReforgeErrorCode? | Fehler | Nein |
| ItemInstanceId | Guid? | Item | Nein |
| Applied | ReforgeOptionDto? | angewendete Option | Nein |
| LoadoutRevision | uint | Revision | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReforgeResult)]
public class ReforgeResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ReforgeResult;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public bool Success { get; set; }
    [Key(4)] public ReforgeErrorCode? ErrorCode { get; set; }
    [Key(5)] public Guid? ItemInstanceId { get; set; }
    [Key(6)] public ReforgeOptionDto? Applied { get; set; }
    [Key(7)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Bei Erfolg: Applied gefüllt.  
- Sendet EquipmentSlotUpdate und StatUpdate.  
- Bei Fehler keine Statänderung.

### Client-Verhalten
- UI zeigt neues Statpaar; wartet auf Delta.  
- Bei Fehler: zeigt Fehlercode, fordert ggf. neuen Preview an.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  ReforgeConfirm (3942)       │
  │─────────────────────────────►│
  │                              │
  │  ReforgeResult (3943)        │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var rres = new ReforgeResult
{
    RequestId = 8002,
    ClientSequence = 5002,
    Success = true,
    ItemInstanceId = Guid.Parse("12121212-3434-5656-7878-909090909090"),
    Applied = new ReforgeOptionDto
    {
        SourceStat = "Crit",
        TargetStat = "Haste",
        SourceValue = 50,
        TargetValue = 50,
        CostCurrencyId = 1,
        CostAmount = 500
    },
    LoadoutRevision = 20
};
```

### Error Codes
Siehe `ReforgeConfirm`.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReforgeConfirm` | 3942 | Request |
| `EquipmentSlotUpdate` | 3905 | Delta |

---

### SetBonusUpdate (3950)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel (bei Set-Wechsel)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Informiert über aktuellen Fortschritt pro Set (Teile-Anzahl, aktive Boni). Wird nach jeder Equip/Unequip/Reforge/Enchant/Gem Operation gesendet, wenn Set-Anzahl sich ändert.

### Im Scope ✅
- Liste aller Sets mit Count und aktiven Boni
- Revision

### Nicht im Scope ❌
- Einzelne Bonus-Effekte (kommen über Combat/Stat Messages)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `SetBonusUpdate` | Ja |
| Entries | List<SetBonusEntry> | Set-Infos | Ja |
| LoadoutRevision | uint | Revision | Ja |

**SetBonusEntry**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SetId | int | Set |
| EquippedCount | byte | Anzahl Teile |
| ActiveBonuses | byte[] | Aktivierte Stufen |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SetBonusUpdate)]
public class SetBonusUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SetBonusUpdate;
    [Key(1)] public List<SetBonusEntry> Entries { get; set; } = new();
    [Key(2)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Berechnet aktive Boni nach Stat-Pipeline.  
- Sendet nur Sets mit count > 0.  
- Wenn Bonus neu aktiv/inaktiv wird, sendet zusätzlich Activate/Deactivate Events.

### Client-Verhalten
- Aktualisiert UI (Set Tooltip).  
- Zeigt Popups für aktivierte Boni.

### Flow-Diagramm
```
Server
  │  SetBonusUpdate (3950)
  │──────────────────────► Client
```

### Beispiel Payloads
```csharp
var set = new SetBonusUpdate
{
    LoadoutRevision = 20,
    Entries =
    {
        new SetBonusEntry
        {
            SetId = 12,
            EquippedCount = 4,
            ActiveBonuses = new byte[]{2,4}
        }
    }
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `SetBonusActivate` | 3951 | Bonus an |
| `SetBonusDeactivate` | 3952 | Bonus aus |

---

### SetBonusActivate (3951)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Event, dass ein spezifischer Set-Bonus aktiv wurde (z.B. 2er oder 4er Bonus). Enthält SetId, BonusRank.

### Im Scope ✅
- Aktivierung einzelner Bonus-Stufe

### Nicht im Scope ❌
- Deaktivierung (separate Message)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `SetBonusActivate` | Ja |
| SetId | int | Set | Ja |
| BonusRank | byte | z.B. 2 oder 4 | Ja |
| LoadoutRevision | uint | Revision | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SetBonusActivate)]
public class SetBonusActivate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SetBonusActivate;
    [Key(1)] public int SetId { get; set; }
    [Key(2)] public byte BonusRank { get; set; }
    [Key(3)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Sendet nur bei Übergang von inactive -> active.  
- Triggert StatUpdate falls Bonus Stats gibt.

### Client-Verhalten
- Spielt Effekt/Toast.  
- Aktualisiert Tooltip.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `SetBonusUpdate` | 3950 | Hintergrundstatus |
| `SetBonusDeactivate` | 3952 | Gegenstück |

---

### SetBonusDeactivate (3952)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Event, dass ein Set-Bonus deaktiviert wurde (z.B. weil Teile entfernt). Enthält SetId, BonusRank.

### Im Scope ✅
- Deaktivierung einzelner Bonus-Stufe

### Nicht im Scope ❌
- Aktivierung (3951)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `SetBonusDeactivate` | Ja |
| SetId | int | Set | Ja |
| BonusRank | byte | Stufe | Ja |
| LoadoutRevision | uint | Revision | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SetBonusDeactivate)]
public class SetBonusDeactivate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SetBonusDeactivate;
    [Key(1)] public int SetId { get; set; }
    [Key(2)] public byte BonusRank { get; set; }
    [Key(3)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Sendet bei Verlust der Stufe.  
- Recalc Stats, sendet StatUpdate.

### Client-Verhalten
- Entfernt Bonus-Anzeige, spielt Warnung.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `SetBonusUpdate` | 3950 | Status |
| `SetBonusActivate` | 3951 | Gegenstück |

---

### WeaponSwapRequest (3960)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel (Kampfabhängig)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Ermöglicht Schnellwechsel zwischen vordefinierten Waffenpaaren (Set A/B). Server validiert Kampfregeln und führt atomaren Swap zwischen MainHand/OffHand oder TwoHand-Slot aus.

### Im Scope ✅
- Swap zwischen gespeicherten Sets
- Idempotenz via RequestId
- Cooldown/Combat-Checks

### Nicht im Scope ❌
- Equip neuer Items aus Inventar (EquipItem)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `WeaponSwapRequest` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| SwapProfile | byte | 0=A->B,1=B->A | Ja |

### Erwartete Response
- `WeaponSwapResult` (3961)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WeaponSwapRequest)]
public class WeaponSwapRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.WeaponSwapRequest;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public byte SwapProfile { get; set; }
}
```

### Server-Verhalten
- Prüft Combat-Flag, Swap-Cooldown, SetProfile vorhanden.  
- Tauscht gespeicherte Items in Slots, recalculates Stats, sendet EquipmentSlotUpdate.  
- Antwortet mit Result.

### Client-Verhalten
- Spielt Swap-Animation; auf Erfolg UI aktualisieren.  
- Bei Fehler: zeigt Code.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  WeaponSwapRequest (3960)    │
  │─────────────────────────────►│
  │                              │
  │  WeaponSwapResult (3961)     │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var swap = new WeaponSwapRequest
{
    RequestId = 9001,
    ClientSequence = 6001,
    SwapProfile = 0
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Swap erfolgt |
| `IN_COMBAT` | Kampf verbietet Swap |
| `COOLDOWN` | Swap CD aktiv |
| `PROFILE_MISSING` | Profil fehlt |
| `BUSY` | Lock |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `WeaponSwapResult` | 3961 | Response |
| `EquipmentSlotUpdate` | 3905 | Delta |

---

### WeaponSwapResult (3961)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf WeaponSwapRequest. Liefert Erfolg/Fehler, aktive Profilnummer und Revision.

### Im Scope ✅
- Ack/Fail
- Aktives Profil
- Revision

### Nicht im Scope ❌
- Slot-Deltas (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `WeaponSwapResult` | Ja |
| RequestId | ulong | Spiegel | Ja |
| ClientSequence | uint | Spiegel | Ja |
| Success | bool | Ergebnis | Ja |
| ErrorCode | EquipErrorCode? | Fehler | Nein |
| ActiveProfile | byte? | Aktives Profil | Nein |
| LoadoutRevision | uint | Revision | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WeaponSwapResult)]
public class WeaponSwapResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.WeaponSwapResult;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public bool Success { get; set; }
    [Key(4)] public EquipErrorCode? ErrorCode { get; set; }
    [Key(5)] public byte? ActiveProfile { get; set; }
    [Key(6)] public uint LoadoutRevision { get; set; }
}
```

### Server-Verhalten
- Bei Erfolg setzt ActiveProfile.  
- Sendet EquipmentSlotUpdate + StatUpdate.  
- Idempotenz via RequestId.

### Client-Verhalten
- Aktualisiert UI Profilanzeige.  
- Wartet auf Delta.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  WeaponSwapRequest (3960)    │
  │─────────────────────────────►│
  │                              │
  │  WeaponSwapResult (3961)     │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var swr = new WeaponSwapResult
{
    RequestId = 9001,
    ClientSequence = 6001,
    Success = true,
    ActiveProfile = 1,
    LoadoutRevision = 21
};
```

### Error Codes
Siehe `WeaponSwapRequest`.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `WeaponSwapRequest` | 3960 | Request |
| `EquipmentSlotUpdate` | 3905 | Delta |

---

### OutfitSave (3970)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Speichert aktuelles Cosmetic-Loadout unter einem Namen. Server validiert Limit, Name-Unique und speichert nur CosmeticLayer (keine Stats).

### Im Scope ✅
- Cosmetic Outfits speichern
- Name-Unique pro Character
- Idempotenz via RequestId

### Nicht im Scope ❌
- Gameplay-Stats (nur Cosmetic)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `OutfitSave` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| Name | string | Outfit-Name | Ja |
| Slots | List<OutfitSlotDto> | Cosmetic Slots | Ja |

**OutfitSlotDto**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Slot | EquipmentSlot | Slot |
| CosmeticRef | int | Appearance-Id |

### Erwartete Response
- `OutfitList` (3973) als bestätigender Snapshot

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.OutfitSave)]
public class OutfitSave : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.OutfitSave;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public string Name { get; set; } = string.Empty;
    [Key(4)] public List<OutfitSlotDto> Slots { get; set; } = new();
}
```

### Server-Verhalten
- Validiert Name, Limit, Ownership.  
- Speichert Outfit, aktualisiert OutfitRevision.  
- Sendet `OutfitList` Snapshot.

### Client-Verhalten
- Nach Response aktualisiert Outfit-Liste.  
- Zeigt Erfolg/Fehler basierend auf Snapshot oder Fehlercode im List-Entry.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  OutfitSave (3970)           │
  │─────────────────────────────►│
  │                              │
  │  OutfitList (3973)           │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var save = new OutfitSave
{
    RequestId = 10001,
    ClientSequence = 7001,
    Name = "DungeonCosmetic",
    Slots =
    {
        new OutfitSlotDto { Slot = EquipmentSlot.Head, CosmeticRef = 1200 },
        new OutfitSlotDto { Slot = EquipmentSlot.Back, CosmeticRef = 1300 }
    }
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Gespeichert |
| `NAME_TAKEN` | Name existiert |
| `LIMIT_REACHED` | Max Outfits erreicht |
| `INVALID_SLOT` | Slot nicht kosmetisch erlaubt |
| `NOT_UNLOCKED` | Appearance nicht freigeschaltet |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `OutfitList` | 3973 | Response/Snapshot |
| `OutfitDelete` | 3972 | Verwaltung |

---

### OutfitLoad (3971)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lädt ein gespeichertes Outfit und wendet CosmeticLayer an. Server validiert Besitz, Verfügbarkeit und sendet EquipmentSlotUpdate + OutfitList Snapshot.

### Im Scope ✅
- CosmeticLayer anwenden
- Idempotenz
- Unterstützt PartialLoad (nur ausgewählte Slots) falls Slots angegeben

### Nicht im Scope ❌
- Gameplay-Stats ändern (nur Visual)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `OutfitLoad` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| OutfitId | Guid | Ziel Outfit | Ja |
| Slots | EquipmentSlot[]? | Optional subset | Nein |

### Erwartete Response
- `OutfitList` (3973) Snapshot + `EquipmentSlotUpdate` (3905) mit CosmeticFlags

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.OutfitLoad)]
public class OutfitLoad : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.OutfitLoad;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid OutfitId { get; set; }
    [Key(4)] public EquipmentSlot[]? Slots { get; set; }
}
```

### Server-Verhalten
- Prüft Ownership, Availability.  
- Überträgt CosmeticRefs in Loadout CosmeticLayer, erhöht LoadoutRevision.  
- Sendet OutfitList + EquipmentSlotUpdate (CosmeticOnly flag).  
- StatRecalc nicht nötig (nur Visual).

### Client-Verhalten
- UI aktualisiert Cosmetics, rendert ohne Statänderung.  
- Speichert aktives Outfit.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  OutfitLoad (3971)           │
  │─────────────────────────────►│
  │                              │
  │  OutfitList (3973)           │
  │◄─────────────────────────────│
  │  EquipmentSlotUpdate (3905)  │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var load = new OutfitLoad
{
    RequestId = 10002,
    ClientSequence = 7002,
    OutfitId = Guid.Parse("22222222-3333-4444-5555-666666666666"),
    Slots = null
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Geladen |
| `NOT_FOUND` | Outfit existiert nicht |
| `NOT_UNLOCKED` | Appearance nicht verfügbar |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `OutfitList` | 3973 | Snapshot |
| `EquipmentSlotUpdate` | 3905 | Cosmetic Delta |

---

### OutfitDelete (3972)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Löscht ein gespeichertes Outfit. Server entfernt Eintrag und sendet neuen OutfitList Snapshot.

### Im Scope ✅
- Outfit entfernen
- Idempotenz

### Nicht im Scope ❌
- Statänderung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `OutfitDelete` | Ja |
| RequestId | ulong | Idempotenz | Ja |
| ClientSequence | uint | Sequenz | Ja |
| OutfitId | Guid | Outfit | Ja |

### Erwartete Response
- `OutfitList` (3973)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.OutfitDelete)]
public class OutfitDelete : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.OutfitDelete;
    [Key(1)] public ulong RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid OutfitId { get; set; }
}
```

### Server-Verhalten
- Entfernt Outfit, aktualisiert Revision.  
- Sendet OutfitList Snapshot.  
- Idempotent: erneutes Löschen ohne Fehler.

### Client-Verhalten
- Aktualisiert Liste aus Snapshot.  
- Entfernt aktive Outfit-Markierung falls gelöscht.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  OutfitDelete (3972)         │
  │─────────────────────────────►│
  │                              │
  │  OutfitList (3973)           │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var del = new OutfitDelete
{
    RequestId = 10003,
    ClientSequence = 7003,
    OutfitId = Guid.Parse("22222222-3333-4444-5555-666666666666")
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `SUCCESS` | Gelöscht |
| `NOT_FOUND` | Bereits entfernt |
| `INTERNAL_ERROR` | Fehler |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `OutfitList` | 3973 | Snapshot |

---

### OutfitList (3973)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Snapshot aller gespeicherten Outfits. Wird nach Save/Load/Delete oder auf Anfrage durch Server geschickt. Dient als Response zu OutfitSave/Load/Delete (Ack über Snapshot).

### Im Scope ✅
- Vollständige Outfit-Liste
- Enthält CosmeticSlots je Outfit
- Enthält `ActiveOutfitId` optional

### Nicht im Scope ❌
- Gameplay-Statänderungen

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `OutfitList` | Ja |
| Outfits | List<OutfitEntryDto> | Outfits | Ja |
| ActiveOutfitId | Guid? | Aktives | Nein |
| Revision | uint | OutfitRevision | Ja |

**OutfitEntryDto**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| OutfitId | Guid | ID |
| Name | string | Name |
| Slots | List<OutfitSlotDto> | Cosmetic Slots |
| CreatedAt | long | Unix |
| UpdatedAt | long | Unix |

### Erwartete Response
- Keine (Snapshot/Response)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.OutfitList)]
public class OutfitList : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.OutfitList;
    [Key(1)] public List<OutfitEntryDto> Outfits { get; set; } = new();
    [Key(2)] public Guid? ActiveOutfitId { get; set; }
    [Key(3)] public uint Revision { get; set; }
}
```

### Server-Verhalten
- Sendet nach jeder Outfit-Operation.  
- Enthält sortierte Liste nach UpdatedAt desc.  
- Revision erhöht sich pro Änderung.

### Client-Verhalten
- Ersetzt lokale Outfit-Liste.  
- Aktualisiert UI, markiert aktives Outfit.  
- Verwendet Revision zur Delta-Validierung.

### Flow-Diagramm
```
Server
  │  OutfitList (3973)
  │──────────────────► Client
```

### Beispiel Payloads
```csharp
var list = new OutfitList
{
    Revision = 5,
    ActiveOutfitId = Guid.Parse("22222222-3333-4444-5555-666666666666"),
    Outfits =
    {
        new OutfitEntryDto
        {
            OutfitId = Guid.Parse("22222222-3333-4444-5555-666666666666"),
            Name = "DungeonCosmetic",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Slots =
            {
                new OutfitSlotDto { Slot = EquipmentSlot.Head, CosmeticRef = 1200 }
            }
        }
    }
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `OutfitSave` | 3970 | Request |
| `OutfitLoad` | 3971 | Request |
| `OutfitDelete` | 3972 | Request |
| `EquipmentSlotUpdate` | 3905 | Visual Delta |

---

## 🗑️ Obsolete Messages

Aktuell keine Nachrichten in 3900-3999 als obsolet markiert. Historie wird hier dokumentiert, falls künftige Deprecations hinzukommen.

---

## 🧨 Edge Cases & Fehlerfälle

- **Reconnect während Equip:** Client kann Duplicate Request senden; Server nutzt Idempotenz-Cache und LoadoutRevision, liefert identisches EquipItemResult.  
- **Inventory Full bei Unequip:** UnequipItemResult mit ErrorCode `INVENTORY_FULL`; Slot bleibt belegt.  
- **Two-Hand → OffHand Conflict:** EquipItem prüft Two-Hand Items; wenn OffHand belegt, Swap muss angegeben sein, sonst `TYPE_MISMATCH`.  
- **Broken Item equippen:** `DURABILITY_ZERO` verhindert Equip; bestehend ausgerüstete Items bleiben, aber Stats deaktiviert.  
- **Gem Remove ohne Inventarplatz:** ErrorCode `GEM_NOT_FOUND` oder `INVENTORY_FULL` (wenn konfiguriert), Operation abgebrochen.  
- **Enchant Scroll Verlust durch Disconnect:** Server transaktional; wenn Fehler vor Commit, Scroll bleibt im Inventar.  
- **WeaponSwap in Combat:** ErrorCode `IN_COMBAT`; kein Delta versendet.  
- **OutfitLoad auf ungültige Slots:** Server filtert Slots, setzt Fehler im OutfitList Entry mit Hinweis; Load erfolgt partiell oder gar nicht je nach Config.  
- **SetBonus Oscillation:** Bei schnell hintereinander Equip/Unequip sorgt Revision für korrekte Reihenfolge; Client verwirft ältere Events.  
- **Cosmetic/Gameplay Divergenz:** EquipmentSlotUpdate enthält `CosmeticRef` und `ItemInstanceId`; Client muss beide Layer separat pflegen.

---

## 📎 Anhang (MessageType Enum Updates)

Aktuelle Range 3900-3999 wird vollständig durch bestehende Einträge abgedeckt. Es wurden **keine neuen Enum-Werte** ergänzt. Falls künftig neue Request/Response Paare notwendig werden, sind die freien Werte 3974-3999 zu verwenden.

```csharp
// Keine neuen Einträge hinzugefügt; MessageType.cs unverändert
```

Source: docs/03-messages/39-equipment.md
