# 🏦 Bank Messages (4000-4099)

**Kategorie:** 40  
**Range:** 4000-4099  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🏷️ Tabs, Sorting & Search](#️-tabs-sorting--search)
- [✅ Deposit/Withdraw/Move Regeln](#-depositwithdrawmove-regeln)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🔒 Permissions & Security](#-permissions--security)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#️-regeln--sicherheit)
- [📩 Aktive Messages 4000–4099](#-aktive-messages-4000–4099)
  - [BankOpen (4000)](#bankopen-4000)
  - [BankClose (4001)](#bankclose-4001)
  - [BankDeposit (4002)](#bankdeposit-4002)
  - [BankDepositResult (4003)](#bankdepositresult-4003)
  - [BankWithdraw (4004)](#bankwithdraw-4004)
  - [BankWithdrawResult (4005)](#bankwithdrawresult-4005)
  - [BankSlotPurchase (4006)](#bankslotpurchase-4006)
  - [BankSlotPurchaseResult (4007)](#bankslotpurchaseresult-4007)
  - [BankTabPurchase (4008)](#banktabpurchase-4008)
  - [BankSync (4009)](#banksync-4009)
  - [GuildBankOpenMsg (4020)](#guildbankopenmsg-4020)
  - [GuildBankCloseMsg (4021)](#guildbankclosemsg-4021)
  - [GuildBankDepositMsg (4022)](#guildbankdepositmsg-4022)
  - [GuildBankWithdrawMsg (4023)](#guildbankwithdrawmsg-4023)
  - [GuildBankLogMsg (4024)](#guildbanklogmsg-4024)
  - [GuildBankTabInfo (4025)](#guildbanktabinfo-4025)
  - [GuildBankSyncMsg (4026)](#guildbanksyncmsg-4026)
  - [VoidStorageOpen (4040)](#voidstorageopen-4040)
  - [VoidStorageClose (4041)](#voidstorageclose-4041)
  - [VoidStorageDeposit (4042)](#voidstoragedeposit-4042)
  - [VoidStorageWithdraw (4043)](#voidstoragewithdraw-4043)
  - [VoidStorageSync (4044)](#voidstoragesync-4044)
  - [ReagentBankOpen (4050)](#reagentbankopen-4050)
  - [ReagentBankDeposit (4051)](#reagentbankdeposit-4051)
  - [ReagentBankSync (4052)](#reagentbanksync-4052)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang (MessageType Enum Updates)](#-anhang-messagetype-enum-updates)

---

## 📋 Überblick

Die Bank-Kategorie (4000-4099) beschreibt alle Nachrichten für persönliche Bank, Reagenzienbank, Void Storage sowie die Synchronisation mit inventar- und wirtschaftsrelevanten Systemen. Ziel ist ein fehlertoleranter, duplikatsicherer und revisionsbasierter Datenaustausch zwischen Client und Server. Jeder Request besitzt eine eindeutig korrelierbare Response (explizit oder implizit über `BankSync` mit identischem `ClientRequestId`). Der Umfang umfasst:

- Persönliche Bank-Container inkl. Tabs, Slots, Such- und Sortierfunktionen.
- Optionale Erweiterung auf Guild-Bank-Events (passive Updates) zur Konsistenz.
- Void Storage (skins / appearances) und Reagent Bank (stapelbare Materialien).
- Integration mit Inventory (0500er), Equipment (3900er), Loot (3100er), Economy (3700er), Trading (1100er) und Mail (1800er) für Gold- und Item-Flows.
- Vollständige Anti-Dupe-Mechanik mit atomaren Transaktionen und Revisionszählern.

---

## 🧠 Datenmodell

### BankContainer

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BankContainerId | long | Eindeutige ID des Bank-Containers (Account/Character-gebunden) | Ja |
| OwnerCharacterId | long | Charakter, der die Bank besitzt (für persönliche Bank) | Ja |
| Tabs | List<BankTab> | Sammlung aller Tabs | Ja |
| Revision | long | Monoton steigender Revisionszähler (Snapshot und Delta) | Ja |
| Capacity | int | Gesamtslots = Summe aller Tab-Slots | Ja |
| UnlockedTabs | int | Anzahl freigeschalteter Tabs | Ja |
| CurrencyBalance | long | Bank-Gold falls getrennt vom Inventar | Nein |
| IsLocked | bool | True falls Sperre (z.B. wegen Anti-Cheat) aktiv | Ja |
| LockReason | string? | Menschlich lesbarer Grund | Nein |
| LastUpdated | long | Unix Timestamp letzter Commit | Ja |

### BankTab

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TabIndex | int | 0-basiert, stabil | Ja |
| Name | string | Lokalisierter Tab-Name | Ja |
| SlotCount | int | Anzahl Slots in diesem Tab | Ja |
| Slots | List<BankSlot> | Inhalt | Ja |
| SortMode | BankSortMode | Server-seitiger Sortiermodus (falls verwendet) | Ja |
| IsLocked | bool | Temporäre Sperre bei aktiver Transaktion | Ja |
| UnlockCost | CurrencyCost? | Kosten zum Freischalten falls noch gesperrt | Nein |

### BankSlot

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | int | 0-basiert, stabil je Tab | Ja |
| ItemInstanceId | long? | Referenz auf Item-Instance | Nein |
| Quantity | int | Stack-Menge | Ja |
| MaxStack | int | Stack-Limit | Ja |
| Locked | bool | Slot-Lock während Transaktion | Ja |

### StoredItem (Ref auf Inventory Item)

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemInstanceId | long | Eindeutige Item-ID | Ja |
| TemplateId | int | Item-Vorlage | Ja |
| BoundState | ItemBindType | Bindung | Ja |
| Durability | int | Zustand | Ja |
| Meta | byte[] | Ser. Stat-Rolls | Nein |

### AccessRules

- **Personal Bank:** Zugriff nur für eingeloggten Charakter + Account-weit konsistent.
- **Shared Bank (optional):** Account-weit geteilt, aber per Character-Limit freigegeben.
- **Guild Bank Events:** Nur informativ; Aktionen über Guild-Bank-Range (0800er) autorisiert.
- **Void Storage:** Nur kosmetische Items, keine Stats, nur Appearance Flags.
- **Reagent Bank:** Nur Materialien; serverseitige Validierung gegen White-List.

---

## 🏷️ Tabs, Sorting & Search

- **Tab-Konzept:** Maximal 8 Tabs im Prototyp, jeweils 42 Slots (6x7). UnlockedTabs bestimmt Nutzbarkeit.
- **Sortierung:** Standard client-seitig. Server bietet optional `SortMode=ServerNatural` und liefert sortierte Reihenfolge im `BankSync`. Keine serverseitigen Mutationen ohne Request.
- **Suche:** Client-seitig mit lokalem Index. Server liefert vollständige Item-Metadaten in Snapshots, keine Query-Messages in Range 4000.
- **Benennung:** Tab-Namen serverseitig validiert (3-20 Zeichen, kein Markdown). Änderung via BankTabPurchase (Unlock) oder zukünftige Rename-Message (nicht Teil des Ranges).

---

## ✅ Deposit/Withdraw/Move Regeln

- **Atomicity:** Jede Operation ist eine transaktionale Einheit: Validierung → Lock → Apply → Persist → Unlock → Delta.
- **Idempotenz:** Requests enthalten `ClientRequestId` (GUID/ulong). Wiederholte Requests mit identischem `ClientRequestId` liefern identisches Ergebnis (`BankDepositResult`/`BankWithdrawResult`) ohne Doppel-Einlage.
- **Move-Within-Bank:** Abgebildet als `BankDeposit` mit `SourceContainer=Bank` und `SourceTab/SourceSlot`. Response liefert Delta via `BankSync`.
- **No Stats from Client:** Client sendet nur Referenzen (`ItemInstanceId`, `SourceInventorySlot`, `TargetTab/Slot`). Stats werden serverseitig aus DB/Inventory geladen.
- **Slot-Lock:** Während Transaktion sind betroffene Slots (Bank + Inventory) gesperrt. Bei Timeout wird Rollback ausgeführt.
- **Partial Stack:** Server kann Teilmengen akzeptieren. Bei Restmengen wird `RemainingQuantity` im Result gesetzt.
- **Validation:** Bound rules, item type compatibility (Reagent/Void restrictions), stack limits, weight (falls relevant), currency sinks.

---

## 🔄 Sync, Deltas & Revisioning

- **Snapshot on Open:** `BankOpen` → `BankSync (Full)` inkl. Revision, alle Tabs/Slots, CurrencyBalance, UnlockedTabs, SlotCounts.
- **Delta on Mutation:** Jede Mutation sendet `BankSync (Delta)` mit `ChangedSlots`, `ChangedTabs`, `NewRevision`. Deltas sind klein (nur geänderte Slots).
- **Out-of-Order Handling:** Client verwirft Deltas mit Revision ≤ currentRevision. Bei Lücke → `BankSyncRequest` (implizit via `BankOpen` Retry) kann erneut angefordert werden.
- **Reconnect-Safe:** Auf Reconnect sendet Server zuletzt persistierten Snapshot (Revision-stabil).
- **Batching:** Deltas können gebündelt werden, aber niemals per Tool-Batch; jede Response enthält korrelierbaren `ClientRequestId`.

---

## 🔒 Permissions & Security

- **Auth:** Alle Bank-Requests setzen gültige Session voraus. Unauthorized → `ErrorCode=UNAUTHORIZED` in Result.
- **Role Checks:** Guild-Bank-Events werden nur an berechtigte Mitglieder gesendet; Schreiboperationen erfolgen in Guild-Range (nicht hier).
- **Anti-Dupe:** Server-seitige Locks + Revision-Check + idempotente Result-Caches + persistence transaction (Inventory + Bank + Currency).
- **Anti-Spam:** Rate-Limits pro Charakter: 5 Bank-Mutationen pro Sekunde; bei Überschreitung `RATE_LIMITED`.
- **Input Validation:** Tab/Slot Bounds, Item ownership, bind rules, container-type compatibility, cooldown on gold transfer.
- **Auditing:** Jede Mutation schreibt Audit-Log (CharId, RequestId, ItemInstanceId, DeltaQuantity, CurrencyDelta, Timestamp, IP).

---

## 🧱 DTOs / Interfaces

```csharp
[MessagePackObject]
public class BankSlotDto
{
    [Key(0)] public int TabIndex { get; set; }
    [Key(1)] public int SlotIndex { get; set; }
    [Key(2)] public long? ItemInstanceId { get; set; }
    [Key(3)] public int Quantity { get; set; }
    [Key(4)] public int MaxStack { get; set; }
    [Key(5)] public bool Locked { get; set; }
}

[MessagePackObject]
public class BankTabDto
{
    [Key(0)] public int TabIndex { get; set; }
    [Key(1)] public string Name { get; set; } = string.Empty;
    [Key(2)] public int SlotCount { get; set; }
    [Key(3)] public List<BankSlotDto> Slots { get; set; } = new();
    [Key(4)] public BankSortMode SortMode { get; set; } = BankSortMode.Client;
    [Key(5)] public bool IsLocked { get; set; }
    [Key(6)] public long? UnlockCost { get; set; }
}

[MessagePackObject]
public class BankSnapshotDto
{
    [Key(0)] public MessageType Type => MessageType.BankSync;
    [Key(1)] public long BankContainerId { get; set; }
    [Key(2)] public long OwnerCharacterId { get; set; }
    [Key(3)] public long Revision { get; set; }
    [Key(4)] public List<BankTabDto> Tabs { get; set; } = new();
    [Key(5)] public long? CurrencyBalance { get; set; }
    [Key(6)] public int UnlockedTabs { get; set; }
    [Key(7)] public int Capacity { get; set; }
    [Key(8)] public bool FullSync { get; set; }
    [Key(9)] public string? CorrelationId { get; set; }
}
```

Alle Keys sind stabil, keine Ignorierungen, keine Interfaces in MessagePack-Attributen. Deltas nutzen dieselben DTOs, aber `Tabs` enthält nur geänderte Slots/Metadaten und `FullSync=false`.

---

## 🧩 Enums / ErrorCodes / Flags

### BankSortMode

```csharp
public enum BankSortMode : byte
{
    Client = 0,
    ServerNatural = 1,
    ByItemId = 2,
    ByQuality = 3,
    ByCategory = 4
}
```

### BankErrorCode

| Code | Bedeutung |
|------|-----------|
| SUCCESS | Operation erfolgreich |
| UNAUTHORIZED | Session ungültig |
| RATE_LIMITED | Zu viele Requests |
| INVALID_TAB | TabIndex ungültig oder gesperrt |
| INVALID_SLOT | SlotIndex ungültig |
| ITEM_NOT_FOUND | ItemInstanceId nicht im angegebenen Container |
| ITEM_MISMATCH | Item gehört nicht zum Charakter |
| STACK_LIMIT | Stack-Limit überschritten |
| CONTAINER_LOCKED | BankContainer gesperrt |
| REVISION_CONFLICT | Revision veraltet, neuer Snapshot nötig |
| CURRENCY_INSUFFICIENT | Nicht genug Gold/Währung |
| TAB_LOCKED | Tab nicht freigeschaltet |
| VOID_RESTRICTION | Item nicht Void-kompatibel |
| REAGENT_RESTRICTION | Item nicht für Reagent Bank erlaubt |
| INTERNAL_ERROR | Unerwarteter Fehler |

### BankContainerType

| Wert | Beschreibung |
|------|--------------|
| Personal | Persönliche Bank |
| Guild | Guild-Bank (nur Events) |
| VoidStorage | Appearance-Speicher |
| Reagent | Reagenzienbank |

---

## ⚙️ Regeln & Sicherheit

- **Locking:** Pro BankContainer ein Mutex. Innerhalb Transaktion zusätzlich Slot-Locks (Bank + Inventory).
- **Revision-Check:** Jeder Mutations-Request trägt `KnownRevision`. Server validiert, bei Abweichung → `BankSync (Full)` + `ErrorCode=REVISION_CONFLICT`.
- **Idempotenz-Cache:** Server speichert Result pro `(CharacterId, ClientRequestId)` für 30 Sekunden.
- **Anti-Dupe Checks:** Cross-verify InventoryDelta + BankDelta + CurrencyDelta vor Commit.
- **Flood Control:** 429 auf Transport-Ebene plus dediziertes `RATE_LIMITED` im Result.
- **Consistency with other systems:** Inventory (0500) wird immer parallel aktualisiert; Equipment (3900) darf nicht direkt aus Bank bedient werden, nur via Inventory.

---

## 📩 Aktive Messages 4000–4099

### BankOpen (4000)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (UI-Öffnen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert das Öffnen der persönlichen Bank an. Startet eine Transaktionssession und löst einen vollständigen Snapshot (`BankSync`) aus. Dient auch als Re-Sync bei Revision-Konflikten nach Reconnect oder Delta-Lücke.

### Im Scope ✅
- UI-Öffnung und Snapshoterstellung
- Locking des BankContainers während Snapshot-Build
- Übergabe von `ClientRequestId` für Korrelation

### Nicht im Scope ❌
- Guild-Bank öffnen (separate Range)
- Item-Mutationen (Deposit/Withdraw separat)
- Tab-Freischaltung (BankTabPurchase)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| KnownRevision | long | Letzte bekannte Revision | Ja |
| CharacterId | long | Aktiver Charakter | Ja |

### Erwartete Response
- `BankSync` (4009) mit `FullSync=true`, `CorrelationId=ClientRequestId`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankOpen)]
public class BankOpen : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.BankOpen;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public long KnownRevision { get; set; }
    [Key(3)] public long CharacterId { get; set; }
}
```

### Server-Verhalten
- Validiert Auth und Charakterbesitz.
- Vergleicht `KnownRevision`; bei mismatch → Full Snapshot.
- Legt Container-Lock an, generiert Snapshot, löst `BankSync` aus.
- Auditiert Zugriff (CharId, Timestamp, IP).
- Bricht ab mit `ErrorMessage (910)` bei Wartung/Lockdown.

### Client-Verhalten
- Sendet nur bei geöffneter Banker-NPC-Interaktion.
- Erwartet `BankSync` und baut UI basierend auf Tab/Slot-Daten.
- Bei Timeout: erneuter `BankOpen` mit gleichem `ClientRequestId`.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  BankOpen (4000)             │
  │─────────────────────────────►│
  │                              │ Build Snapshot + Lock
  │                              │
  │  BankSync (4009, Full)       │
  │◄─────────────────────────────│
  │                              │ Unlock Container
```

### Beispiel Payloads
```csharp
var open = new BankOpen
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    KnownRevision = 0,
    CharacterId = 12345
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| UNAUTHORIZED | Session ungültig |
| CONTAINER_LOCKED | Bank wegen laufender Prüfung gesperrt |
| INTERNAL_ERROR | Unerwarteter Fehler beim Snapshot |

### Verwandte Messages

| Message | ID  | Beziehung |
| ------- | --- | --------- |
| BankSync | 4009 | Response auf BankOpen |
| BankClose | 4001 | Session-Ende |

---

### BankClose (4001)

**Richtung:** 📤 Client → Server / 📥 Server → Client Ack  
**Frequenz:** Selten (UI-Schließen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Beendet die Bank-Session, gibt Locks frei und beendet Delta-Pushes. Server bestätigt mit gleicher Message-ID als Ack, damit jeder Request eine Response besitzt.

### Im Scope ✅
- Session-Freigabe
- Audit des Schließgrunds
- Ack mit Correlation

### Nicht im Scope ❌
- Inventar-Updates (bleiben bestehen)
- NPC-Interaktions-Ende (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| Reason | string | Optionaler Grund | Nein |

### Erwartete Response
- `BankClose` (4001) als Server-Ack mit gleichem `ClientRequestId`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankClose)]
public class BankClose : IClientMessage, IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BankClose;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public string? Reason { get; set; }
}
```

### Server-Verhalten
- Entfernt Container-Lock.
- Stoppt geplante Delta-Pushes.
- Sendet Ack zurück.
- Persistiert letzter Revision-Stand (falls schwebende Deltas).

### Client-Verhalten
- Schließt UI erst nach Ack oder Timeout.
- Löscht lokale Delta-Queue.
- Setzt Such-/Sortierstatus lokal zurück.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │  BankClose (4001)            │
  │─────────────────────────────►│
  │                              │ Release locks
  │  BankClose (Ack)             │
  │◄─────────────────────────────│
  │                              │
```

### Beispiel Payloads
```csharp
var close = new BankClose
{
    ClientRequestId = lastRequestId,
    Reason = "UI closed"
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| SUCCESS | Ack erhalten |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankOpen | 4000 | Startet Session |

---

### BankDeposit (4002)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel (abhängig vom Spieler)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Überträgt ein Item (oder Teilstack) aus Inventar oder einem Bank-Slot in einen Ziel-Bank-Slot. Unterstützt Move-Within-Bank durch Angabe des Quell-Containers.

### Im Scope ✅
- Inventar → Bank
- Bank → Bank (Move innerhalb Bank)
- Teilstack-Deposit mit `Quantity`
- Idempotenz über `ClientRequestId`

### Nicht im Scope ❌
- Bank → Inventar (Withdraw separat)
- Guild Bank (separater Range)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| SourceContainer | BankContainerType | Inventory/Bank | Ja |
| SourceTabIndex | int | Tab des Quells (bei Bank) | Nein |
| SourceSlotIndex | int | Slot im Quell-Container | Ja |
| TargetTabIndex | int | Ziel-Tab | Ja |
| TargetSlotIndex | int | Ziel-Slot | Ja |
| ItemInstanceId | long | Referenz des Items | Ja |
| Quantity | int | Menge (<= Stack) | Ja |
| KnownRevision | long | Client bekannte Revision | Ja |

### Erwartete Response
- `BankDepositResult` (4003) + Delta `BankSync` (4009, FullSync=false)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankDeposit)]
public class BankDeposit : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.BankDeposit;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public BankContainerType SourceContainer { get; set; }
    [Key(3)] public int SourceTabIndex { get; set; }
    [Key(4)] public int SourceSlotIndex { get; set; }
    [Key(5)] public int TargetTabIndex { get; set; }
    [Key(6)] public int TargetSlotIndex { get; set; }
    [Key(7)] public long ItemInstanceId { get; set; }
    [Key(8)] public int Quantity { get; set; }
    [Key(9)] public long KnownRevision { get; set; }
}
```

### Server-Verhalten
- Validiert Revision, Auth, Ownership, Container-Kompatibilität, Bind-Status.
- Reserviert Slots (lock) in Quelle/Ziel.
- Verschiebt Stack oder Teilstack; passt Restmenge an.
- Persistiert Inventory + Bank + Currency (falls Gebühr) atomar.
- Sendet Result + BankSync Delta; hebt Locks auf.

### Client-Verhalten
- Sperrt UI-Slot bis Result/Delta empfangen.
- Bei `REVISION_CONFLICT` erneut `BankOpen`.
- Bei Erfolg aktualisiert UI mit BankSync-Daten; InventoryDelta kommt aus Range 0500.

### Flow-Diagramm
```
Client                               Server
  │                                    │
  │ BankDeposit (4002)                 │
  │───────────────────────────────────►│
  │                                    │ Validate + Lock + Move
  │ BankDepositResult (4003)           │
  │◄───────────────────────────────────│
  │ BankSync (4009 Delta)              │
  │◄───────────────────────────────────│
  │                                    │ Unlock
```

### Beispiel Payloads
```csharp
var deposit = new BankDeposit
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    SourceContainer = BankContainerType.Personal,
    SourceTabIndex = 0,
    SourceSlotIndex = 5,
    TargetTabIndex = 1,
    TargetSlotIndex = 3,
    ItemInstanceId = 555000123,
    Quantity = 10,
    KnownRevision = 12
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| UNAUTHORIZED | Session ungültig |
| INVALID_TAB | Tab nicht existent oder gesperrt |
| INVALID_SLOT | Slot existiert nicht |
| ITEM_NOT_FOUND | Item nicht in Quelle |
| STACK_LIMIT | Ziel-Stack voll |
| REVISION_CONFLICT | Revision veraltet |
| CONTAINER_LOCKED | Container gelockt |
| RATE_LIMITED | Zu viele Requests |
| INTERNAL_ERROR | Unerwarteter Fehler |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankDepositResult | 4003 | Response |
| BankSync | 4009 | Delta |
| InventoryUpdate | 500 | Parallel-Delta |

---

### BankDepositResult (4003)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf `BankDeposit`. Enthält Erfolg/Fehler, Restmengen und neue Revision. Dient der Idempotenz: Wiederholte Requests mit gleicher `ClientRequestId` liefern identisches Result.

### Im Scope ✅
- Erfolg/Fehler + ErrorCode
- Neue Revision und Delta-Hinweis
- Restmenge bei Teilverschiebung

### Nicht im Scope ❌
- Inventar-Deltas (kommen separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| Success | bool | Status | Ja |
| ErrorCode | string | Fehlercode | Nein |
| ErrorMessage | string | Menschliche Beschreibung | Nein |
| NewRevision | long | Neue Revision | Bei Erfolg |
| RemainingQuantity | int | Menge, die nicht verschoben wurde | Ja |

### Erwartete Response
- Keine (Response selbst)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankDepositResult)]
public class BankDepositResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BankDepositResult;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public string? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
    [Key(5)] public long NewRevision { get; set; }
    [Key(6)] public int RemainingQuantity { get; set; }
}
```

### Server-Verhalten
- Sendet Result nach Commit.
- Bei Idempotenz liefert gecachten Result-Eintrag.
- Bei Fehler setzt `RemainingQuantity`=Original Menge.

### Client-Verhalten
- Entsperrt Slots.
- Nutzt `RemainingQuantity` um UI-Rest anzuzeigen.
- Erwartet unmittelbar folgenden `BankSync` Delta.

### Flow-Diagramm
```
Client                        Server
  │                             │
  │ BankDeposit (4002)          │
  │────────────────────────────►│
  │                             │
  │ BankDepositResult (4003)    │
  │◄────────────────────────────│
  │ BankSync (4009)             │
  │◄────────────────────────────│
```

### Beispiel Payloads
```csharp
var result = new BankDepositResult
{
    ClientRequestId = requestId,
    Success = true,
    NewRevision = 13,
    RemainingQuantity = 0
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| STACK_LIMIT | Ziel konnte Menge nicht aufnehmen |
| ITEM_NOT_FOUND | Quelle leer |
| REVISION_CONFLICT | Neuer Snapshot nötig |
| UNAUTHORIZED | Session ungültig |
| CONTAINER_LOCKED | Operation blockiert |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankDeposit | 4002 | Request |
| BankSync | 4009 | Delta |

---

### BankWithdraw (4004)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Überträgt ein Item oder Teilstack aus Bank in Inventar. Unterstützt Split und Reagent/Void-spezifische Validierungen.

### Im Scope ✅
- Bank → Inventar
- Teilstack-Withdraw
- Idempotenz und Revision-Check

### Nicht im Scope ❌
- Inventar → Bank (Deposit)
- Guild Bank (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| SourceTabIndex | int | Bank-Tab | Ja |
| SourceSlotIndex | int | Bank-Slot | Ja |
| TargetInventorySlot | int | Ziel-Slot im Inventar | Ja |
| ItemInstanceId | long | Referenz | Ja |
| Quantity | int | Menge | Ja |
| KnownRevision | long | Letzte bekannte Revision | Ja |

### Erwartete Response
- `BankWithdrawResult` (4005) + `BankSync` Delta (4009)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankWithdraw)]
public class BankWithdraw : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.BankWithdraw;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public int SourceTabIndex { get; set; }
    [Key(3)] public int SourceSlotIndex { get; set; }
    [Key(4)] public int TargetInventorySlot { get; set; }
    [Key(5)] public long ItemInstanceId { get; set; }
    [Key(6)] public int Quantity { get; set; }
    [Key(7)] public long KnownRevision { get; set; }
}
```

### Server-Verhalten
- Validiert Slot, Item, Ownership, Inventory-Kapazität.
- Sperrt betroffene Slots.
- Führt Transfer aus, persistiert atomar.
- Sendet Result + BankSync Delta + InventoryUpdate (0500).

### Client-Verhalten
- Sperrt Slots.
- Wartet auf Result; bei Erfolg aktualisiert UI mit Deltas.
- Bei Fehler zeigt ErrorCode an, entsperrt Slots.

### Flow-Diagramm
```
Client                              Server
  │                                   │
  │ BankWithdraw (4004)               │
  │──────────────────────────────────►│
  │                                   │ Validate + Lock + Move
  │ BankWithdrawResult (4005)         │
  │◄──────────────────────────────────│
  │ BankSync (4009 Delta)             │
  │◄──────────────────────────────────│
  │ InventoryUpdate (0500)            │
  │◄──────────────────────────────────│
```

### Beispiel Payloads
```csharp
var withdraw = new BankWithdraw
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    SourceTabIndex = 1,
    SourceSlotIndex = 10,
    TargetInventorySlot = 20,
    ItemInstanceId = 555000123,
    Quantity = 5,
    KnownRevision = 13
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| INVALID_SLOT | Slot leer oder ungültig |
| ITEM_NOT_FOUND | Item nicht vorhanden |
| STACK_LIMIT | Inventarstack voll |
| REVISION_CONFLICT | Snapshot erneut laden |
| UNAUTHORIZED | Session ungültig |
| RATE_LIMITED | Zu viele Requests |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankWithdrawResult | 4005 | Response |
| BankSync | 4009 | Delta |
| InventoryUpdate | 500 | Inventar-Delta |

---

### BankWithdrawResult (4005)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf `BankWithdraw`. Enthält Erfolg/Fehler, neue Revision und Restmenge.

### Im Scope ✅
- Erfolg/Fehler
- Restmenge
- Revision

### Nicht im Scope ❌
- Inventar-Sync (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| Success | bool | Status | Ja |
| ErrorCode | string | Fehlercode | Nein |
| ErrorMessage | string | Beschreibung | Nein |
| NewRevision | long | Neue Revision | Bei Erfolg |
| RemainingQuantity | int | Menge, die nicht verschoben wurde | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankWithdrawResult)]
public class BankWithdrawResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BankWithdrawResult;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public string? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
    [Key(5)] public long NewRevision { get; set; }
    [Key(6)] public int RemainingQuantity { get; set; }
}
```

### Server-Verhalten
- Liefert idempotente Antwort.
- Setzt `RemainingQuantity` korrekt.
- Stößt anschließend `BankSync` Delta an.

### Client-Verhalten
- Entsperrt Slots.
- Nutzt Revision zur UI-Aktualisierung.
- Beachtet ErrorCodes für Messaging.

### Flow-Diagramm
```
Client                        Server
  │                             │
  │ BankWithdraw (4004)         │
  │────────────────────────────►│
  │                             │
  │ BankWithdrawResult (4005)   │
  │◄────────────────────────────│
  │ BankSync (4009)             │
  │◄────────────────────────────│
```

### Beispiel Payloads
```csharp
var result = new BankWithdrawResult
{
    ClientRequestId = requestId,
    Success = true,
    NewRevision = 14,
    RemainingQuantity = 0
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| INVALID_SLOT | Quelle leer |
| STACK_LIMIT | Inventar voll |
| REVISION_CONFLICT | Snapshot neu laden |
| UNAUTHORIZED | Keine Session |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankWithdraw | 4004 | Request |
| BankSync | 4009 | Delta |

---

### BankSlotPurchase (4006)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Kauft zusätzliche Slots innerhalb eines bestehenden Tabs. Kosten steigen pro Kauf und werden aus Economy-Range validiert.

### Im Scope ✅
- Slot-Kauf im aktiven Tab
- Kostenauswertung (Gold/Token)
- Idempotente Bestätigung über Result

### Nicht im Scope ❌
- Tab-Freischaltung (BankTabPurchase)
- Guild-Bank Slots (separat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| TabIndex | int | Ziel-Tab | Ja |
| SlotsToBuy | int | Anzahl Slots | Ja |
| KnownRevision | long | Revision | Ja |

### Erwartete Response
- `BankSlotPurchaseResult` (4007) + `BankSync` Delta

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankSlotPurchase)]
public class BankSlotPurchase : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.BankSlotPurchase;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public int TabIndex { get; set; }
    [Key(3)] public int SlotsToBuy { get; set; }
    [Key(4)] public long KnownRevision { get; set; }
}
```

### Server-Verhalten
- Berechnet Kosten (progressiv).
- Prüft CurrencyBalance/InventoryGold.
- Erhöht SlotCount und passt Capacity an.
- Persistiert, sendet Result + BankSync (Delta mit TabMeta).

### Client-Verhalten
- Sperrt UI während Kauf.
- Erwartet Result + Delta; aktualisiert UI Slots als leer.
- Bei `CURRENCY_INSUFFICIENT` zeigt Fehlermeldung.

### Flow-Diagramm
```
Client                             Server
  │                                  │
  │ BankSlotPurchase (4006)          │
  │─────────────────────────────────►│
  │                                  │ Validate + Charge + Expand
  │ BankSlotPurchaseResult (4007)    │
  │◄─────────────────────────────────│
  │ BankSync (4009 Delta)            │
  │◄─────────────────────────────────│
```

### Beispiel Payloads
```csharp
var slotBuy = new BankSlotPurchase
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    TabIndex = 0,
    SlotsToBuy = 7,
    KnownRevision = 14
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| TAB_LOCKED | Tab nicht freigeschaltet |
| CURRENCY_INSUFFICIENT | Gold reicht nicht |
| INVALID_TAB | Tab ungültig |
| REVISION_CONFLICT | Snapshot erneut laden |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankSlotPurchaseResult | 4007 | Response |
| BankSync | 4009 | Delta |

---

### BankSlotPurchaseResult (4007)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf Slot-Kauf. Enthält Kosten, neue Slot-Anzahl, Revision und Fehlercodes.

### Im Scope ✅
- Kaufstatus
- Kosteninformationen
- Neue Kapazität

### Nicht im Scope ❌
- Guild-Bank Slots

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| Success | bool | Status | Ja |
| ErrorCode | string | Fehlercode | Nein |
| ErrorMessage | string | Beschreibung | Nein |
| NewSlotCount | int | Slots im Tab nach Kauf | Bei Erfolg |
| CurrencyDelta | long | Abgezogenes Gold | Bei Erfolg |
| NewRevision | long | Revision | Bei Erfolg |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankSlotPurchaseResult)]
public class BankSlotPurchaseResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BankSlotPurchaseResult;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public string? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
    [Key(5)] public int NewSlotCount { get; set; }
    [Key(6)] public long CurrencyDelta { get; set; }
    [Key(7)] public long NewRevision { get; set; }
}
```

### Server-Verhalten
- Berechnet CurrencyDelta.
- Sendet Result + BankSync Delta.
- Cacht Result für Idempotenz.

### Client-Verhalten
- Aktualisiert UI mit neuer Slotanzahl.
- Zeigt Kostenverbrauch an.
- Wartet auf BankSync für reale Slot-Liste.

### Flow-Diagramm
```
Client                                Server
  │                                     │
  │ BankSlotPurchase (4006)             │
  │────────────────────────────────────►│
  │                                     │
  │ BankSlotPurchaseResult (4007)       │
  │◄────────────────────────────────────│
  │ BankSync (4009 Delta)               │
  │◄────────────────────────────────────│
```

### Beispiel Payloads
```csharp
var result = new BankSlotPurchaseResult
{
    ClientRequestId = requestId,
    Success = true,
    NewSlotCount = 56,
    CurrencyDelta = -15000,
    NewRevision = 15
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| CURRENCY_INSUFFICIENT | Gold reicht nicht |
| TAB_LOCKED | Tab gesperrt |
| REVISION_CONFLICT | Snapshot neu laden |
| UNAUTHORIZED | Keine Session |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankSlotPurchase | 4006 | Request |
| BankSync | 4009 | Delta |

---

### BankTabPurchase (4008)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Schaltet einen neuen Bank-Tab frei. Da kein dediziertes Result vorhanden ist, wird die Response über `BankSync (FullSync=false)` mit `CorrelationId` geliefert. Jeder Request benötigt daher gültiges `ClientRequestId`.

### Im Scope ✅
- Freischaltung eines neuen Tabs
- Kostenberechnung (steigend je Tab)
- Revision-Update

### Nicht im Scope ❌
- Tab-Umbenennung (nicht im Range)
- Guild-Bank Tabs

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| KnownRevision | long | Letzte Revision | Ja |

### Erwartete Response
- `BankSync` (4009) mit neuem Tab, `CorrelationId=ClientRequestId`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankTabPurchase)]
public class BankTabPurchase : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.BankTabPurchase;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public long KnownRevision { get; set; }
}
```

### Server-Verhalten
- Berechnet Unlock-Kosten, prüft Max-Tabs.
- Zieht Währung ab, erstellt neuen Tab mit Defaults.
- Persistiert, sendet `BankSync` (Delta oder Full wenn Struktur geändert).

### Client-Verhalten
- Sperrt UI bis `BankSync` eintrifft.
- Erwartet neuen Tab in `Tabs` mit SlotCount-Default.
- Zeigt Kostenverbrauch an (CurrencyDelta aus Economy Update).

### Flow-Diagramm
```
Client                             Server
  │                                  │
  │ BankTabPurchase (4008)           │
  │─────────────────────────────────►│
  │                                  │ Validate + Charge + Add Tab
  │ BankSync (4009 Delta)            │
  │◄─────────────────────────────────│
```

### Beispiel Payloads
```csharp
var tabBuy = new BankTabPurchase
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    KnownRevision = 15
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| CURRENCY_INSUFFICIENT | Gold reicht nicht |
| TAB_LOCKED | Vorherige Tabs nicht freigeschaltet |
| REVISION_CONFLICT | Snapshot neu laden |
| INTERNAL_ERROR | Unerwarteter Fehler |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankSync | 4009 | Response |
| BankSlotPurchase | 4006 | Slot-Erweiterung |

---

### BankSync (4009)

**Richtung:** 📥 Server → Client (Snapshot/Delta)  
**Frequenz:** Mittel (bei Mutationen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Überträgt Bank-Status als Full Snapshot oder Delta. Korrelierbar über `CorrelationId`. Wird als Antwort auf `BankOpen`, `BankDeposit`, `BankWithdraw`, `BankSlotPurchase`, `BankTabPurchase` gesendet.

### Im Scope ✅
- FullSync + Delta
- Revision-Update
- Slot- und Tab-Metadaten

### Nicht im Scope ❌
- Inventar-Deltas
- Guild-Bank Inhalte

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | string | Korrelations-ID | Nein |
| BankContainerId | long | Container-ID | Ja |
| OwnerCharacterId | long | Charakter | Ja |
| Revision | long | Revision nach Mutation | Ja |
| FullSync | bool | True = kompletter Snapshot | Ja |
| Tabs | List<BankTabDto> | Tabs mit Slots | Ja |
| UnlockedTabs | int | Anzahl freigeschaltet | Ja |
| Capacity | int | Gesamtslots | Ja |
| CurrencyBalance | long? | Bank-Gold | Nein |

### Erwartete Response
- Keine (Server-Push/Response)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BankSync)]
public class BankSync : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BankSync;
    [Key(1)] public string? CorrelationId { get; set; }
    [Key(2)] public long BankContainerId { get; set; }
    [Key(3)] public long OwnerCharacterId { get; set; }
    [Key(4)] public long Revision { get; set; }
    [Key(5)] public bool FullSync { get; set; }
    [Key(6)] public List<BankTabDto> Tabs { get; set; } = new();
    [Key(7)] public int UnlockedTabs { get; set; }
    [Key(8)] public int Capacity { get; set; }
    [Key(9)] public long? CurrencyBalance { get; set; }
}
```

### Server-Verhalten
- Baut FullSync bei `BankOpen` oder Strukturänderung.
- Baut Delta mit nur geänderten Tabs/Slots und gleichem Schema.
- Stellt Revision sicher monoton.

### Client-Verhalten
- Bei `FullSync=true`: UI vollständig neu befüllen.
- Bei Delta: nur genannte Tabs/Slots aktualisieren.
- Verwift Deltas mit Revision ≤ aktuelle.

### Flow-Diagramm
```
Client                         Server
  │                              │
  │ (Request)                    │
  │─────────────────────────────►│
  │                              │
  │ BankSync (4009)              │
  │◄─────────────────────────────│
```

### Beispiel Payloads
```csharp
var sync = new BankSync
{
    CorrelationId = requestId,
    BankContainerId = 999,
    OwnerCharacterId = 12345,
    Revision = 16,
    FullSync = false,
    Tabs = new List<BankTabDto>
    {
        new BankTabDto
        {
            TabIndex = 1,
            SlotCount = 42,
            SortMode = BankSortMode.Client,
            Slots = new List<BankSlotDto>
            {
                new BankSlotDto
                {
                    TabIndex = 1,
                    SlotIndex = 3,
                    ItemInstanceId = 555000123,
                    Quantity = 5,
                    MaxStack = 20,
                    Locked = false
                }
            }
        }
    },
    UnlockedTabs = 2,
    Capacity = 84,
    CurrencyBalance = 120000
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| SUCCESS | Snapshot gültig |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| BankOpen | 4000 | Antwort |
| BankDeposit | 4002 | Delta |
| BankWithdraw | 4004 | Delta |
| BankSlotPurchase | 4006 | Delta |
| BankTabPurchase | 4008 | Delta |

---

### GuildBankOpenMsg (4020)

**Richtung:** 📥 Server → Client (Event)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Permission

### Beschreibung
Informiert Client, dass Guild-Bank geöffnet wurde (z.B. durch NPC-Interaktion). Dient als Trigger für Guild-Bank-spezifische Range (0800er). Hier nur Event für UI.

### Im Scope ✅
- Guild-Bank Öffnungs-Benachrichtigung
- Übergabe von GuildId, Tab-Info-Kurzfassung
- Korrelation mit `GuildBankSyncMsg`

### Nicht im Scope ❌
- Persönliche Bank
- Guild-Bank Mutationen (separat 0800er)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildId | long | Gilde | Ja |
| CharacterId | long | Öffnender Charakter | Ja |
| PermissionMask | int | Berechtigungen | Ja |
| CorrelationId | string | Korrelations-ID | Ja |

### Erwartete Response
- Keine (Event). Folgt `GuildBankSyncMsg`.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildBankOpenMsg)]
public class GuildBankOpenMsg : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildBankOpenMsg;
    [Key(1)] public long GuildId { get; set; }
    [Key(2)] public long CharacterId { get; set; }
    [Key(3)] public int PermissionMask { get; set; }
    [Key(4)] public string CorrelationId { get; set; } = string.Empty;
}
```

### Server-Verhalten
- Sendet Event bei erfolgreicher Guild-Permission-Prüfung.
- Startet Guild-Bank Sync Pipeline.

### Client-Verhalten
- Öffnet Guild-Bank UI.
- Wartet auf `GuildBankSyncMsg`.

### Flow-Diagramm
```
Server                     Client
  │                          │
  │ GuildBankOpenMsg (4020)  │
  │─────────────────────────►│
  │                          │ UI öffnet
```

### Beispiel Payloads
```csharp
var evt = new GuildBankOpenMsg
{
    GuildId = 77,
    CharacterId = 12345,
    PermissionMask = 0b111,
    CorrelationId = Guid.NewGuid().ToString("N")
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| SUCCESS | Event |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| GuildBankSyncMsg | 4026 | Folgesync |
| GuildBankTabInfo | 4025 | Tab-Metadaten |

---

### GuildBankCloseMsg (4021)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Permission

### Beschreibung
Schließt Guild-Bank-UI und beendet Delta-Stream. Trigger nach Timeout oder manueller Schließung.

### Im Scope ✅
- UI-Schließhinweis
- Grundangabe

### Nicht im Scope ❌
- Persönliche Bank

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildId | long | Gilde | Ja |
| Reason | string | Grund | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildBankCloseMsg)]
public class GuildBankCloseMsg : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildBankCloseMsg;
    [Key(1)] public long GuildId { get; set; }
    [Key(2)] public string? Reason { get; set; }
}
```

### Server-Verhalten
- Sendet Event beim Session-Ende.
- Beendet Delta-Pipeline.

### Client-Verhalten
- Schließt UI, verwirft Pending Ops.

### Flow-Diagramm
```
Server                        Client
  │                             │
  │ GuildBankCloseMsg (4021)    │
  │────────────────────────────►│
```

### Beispiel Payloads
```csharp
new GuildBankCloseMsg { GuildId = 77, Reason = "Timeout" };
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| GuildBankOpenMsg | 4020 | Start |

---

### GuildBankDepositMsg (4022)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Permission

### Beschreibung
Broadcast-Event, dass ein Mitglied ein Item in die Guild-Bank eingezahlt hat. Request-Flow läuft im Guild-Range; hier nur Benachrichtigung.

### Im Scope ✅
- Informiert über Item, Menge, Tab/Slot
- Korrelierbar über `CorrelationId` aus zugrundeliegendem Request

### Nicht im Scope ❌
- Persönliche Bank
- Ergebnisbestätigung (kommt im Guild-Range)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildId | long | Gilde | Ja |
| CharacterId | long | Einzahlender Charakter | Ja |
| TabIndex | int | Tab | Ja |
| SlotIndex | int | Slot | Ja |
| ItemInstanceId | long | Item | Ja |
| Quantity | int | Menge | Ja |
| CorrelationId | string | Korrelations-ID | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildBankDepositMsg)]
public class GuildBankDepositMsg : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildBankDepositMsg;
    [Key(1)] public long GuildId { get; set; }
    [Key(2)] public long CharacterId { get; set; }
    [Key(3)] public int TabIndex { get; set; }
    [Key(4)] public int SlotIndex { get; set; }
    [Key(5)] public long ItemInstanceId { get; set; }
    [Key(6)] public int Quantity { get; set; }
    [Key(7)] public string? CorrelationId { get; set; }
}
```

### Server-Verhalten
- Broadcast an alle berechtigten Mitglieder in Zone/Online.
- Loggt Transaktion.

### Client-Verhalten
- Aktualisiert Guild-Bank UI (Tab/Slot).
- Spielt optional Audit-Log im UI.

### Flow-Diagramm
```
Server                           Clients
  │                                │
  │ GuildBankDepositMsg (4022)     │
  │───────────────────────────────►│
```

### Beispiel Payloads
```csharp
new GuildBankDepositMsg
{
    GuildId = 77,
    CharacterId = 223344,
    TabIndex = 0,
    SlotIndex = 12,
    ItemInstanceId = 8880001,
    Quantity = 20
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| GuildBankSyncMsg | 4026 | Vollsync |
| GuildBankLogMsg | 4024 | Logging |

---

### GuildBankWithdrawMsg (4023)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Permission

### Beschreibung
Broadcast, dass ein Mitglied Items aus Guild-Bank entnommen hat. Dient der UI-Aktualisierung und Nachvollziehbarkeit.

### Im Scope ✅
- Tab/Slot/Quantity Info
- Character und Korrelation

### Nicht im Scope ❌
- Persönliche Bank

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildId | long | Gilde | Ja |
| CharacterId | long | Nehmender Charakter | Ja |
| TabIndex | int | Tab | Ja |
| SlotIndex | int | Slot | Ja |
| ItemInstanceId | long | Item | Ja |
| Quantity | int | Menge | Ja |
| CorrelationId | string | Korrelations-ID | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildBankWithdrawMsg)]
public class GuildBankWithdrawMsg : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildBankWithdrawMsg;
    [Key(1)] public long GuildId { get; set; }
    [Key(2)] public long CharacterId { get; set; }
    [Key(3)] public int TabIndex { get; set; }
    [Key(4)] public int SlotIndex { get; set; }
    [Key(5)] public long ItemInstanceId { get; set; }
    [Key(6)] public int Quantity { get; set; }
    [Key(7)] public string? CorrelationId { get; set; }
}
```

### Server-Verhalten
- Broadcast an berechtigte Mitglieder.
- Ergänzt Guild-Bank-Log.

### Client-Verhalten
- Aktualisiert UI-Slot (leeren/Stack reduzieren).
- Fügt Log-Eintrag hinzu.

### Flow-Diagramm
```
Server                           Clients
  │                                │
  │ GuildBankWithdrawMsg (4023)    │
  │───────────────────────────────►│
```

### Beispiel Payloads
```csharp
new GuildBankWithdrawMsg
{
    GuildId = 77,
    CharacterId = 223344,
    TabIndex = 1,
    SlotIndex = 2,
    ItemInstanceId = 8880001,
    Quantity = 5
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| GuildBankSyncMsg | 4026 | Sync |
| GuildBankLogMsg | 4024 | Logging |

---

### GuildBankLogMsg (4024)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Permission

### Beschreibung
Überträgt Audit-Logs der Guild-Bank (Deposits, Withdrawals, Tab-Changes). Hilft beim Nachvollziehen und Anti-Dupe.

### Im Scope ✅
- Log-Einträge mit Timestamp, Actor, Aktion
- Pagination via Sequence

### Nicht im Scope ❌
- Persönliche Bank Logs

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildId | long | Gilde | Ja |
| Entries | List<GuildBankLogEntry> | Log-Daten | Ja |
| SequenceStart | long | Start | Ja |
| SequenceEnd | long | Ende | Ja |

`GuildBankLogEntry`: ActorId, ActionType, TabIndex, SlotIndex, ItemInstanceId, Quantity, Timestamp.

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildBankLogMsg)]
public class GuildBankLogMsg : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildBankLogMsg;
    [Key(1)] public long GuildId { get; set; }
    [Key(2)] public List<GuildBankLogEntry> Entries { get; set; } = new();
    [Key(3)] public long SequenceStart { get; set; }
    [Key(4)] public long SequenceEnd { get; set; }
}
```

### Server-Verhalten
- Sendet Log-Segmente auf Anfrage aus Guild-Range oder bei Änderungen.
- Garantiert Reihenfolge via Sequence.

### Client-Verhalten
- Fügt Einträge in UI-Log ein.
- Nutzt Sequence zum Erkennen von Lücken.

### Flow-Diagramm
```
Server                        Client
  │                             │
  │ GuildBankLogMsg (4024)      │
  │────────────────────────────►│
```

### Beispiel Payloads
```csharp
new GuildBankLogMsg
{
    GuildId = 77,
    SequenceStart = 100,
    SequenceEnd = 105,
    Entries = new List<GuildBankLogEntry>
    {
        new GuildBankLogEntry
        {
            ActorId = 223344,
            ActionType = "Deposit",
            TabIndex = 0,
            SlotIndex = 12,
            ItemInstanceId = 8880001,
            Quantity = 20,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        }
    }
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| GuildBankSyncMsg | 4026 | Inhalts-Sync |
| GuildBankDepositMsg | 4022 | Ereignis |
| GuildBankWithdrawMsg | 4023 | Ereignis |

---

### GuildBankTabInfo (4025)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel (bei Tab-Änderungen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Permission

### Beschreibung
Metadaten zu Guild-Bank-Tabs (Namen, Rechte, Slot-Anzahl). Wird bei Open/Sync gesendet.

### Im Scope ✅
- Tab-Namen und Rechte
- Slot-Count pro Tab
- Lock-Status

### Nicht im Scope ❌
- Persönliche Bank Tabs

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildId | long | Gilde | Ja |
| Tabs | List<GuildBankTabMeta> | Tab-Metadaten | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildBankTabInfo)]
public class GuildBankTabInfo : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildBankTabInfo;
    [Key(1)] public long GuildId { get; set; }
    [Key(2)] public List<GuildBankTabMeta> Tabs { get; set; } = new();
}
```

### Server-Verhalten
- Sendet bei Open und Tab-Änderungen.

### Client-Verhalten
- Aktualisiert Tab-Liste, Rechte-Anzeige.

### Flow-Diagramm
```
Server                        Client
  │                             │
  │ GuildBankTabInfo (4025)     │
  │────────────────────────────►│
```

### Beispiel Payloads
```csharp
new GuildBankTabInfo
{
    GuildId = 77,
    Tabs = new List<GuildBankTabMeta>
    {
        new GuildBankTabMeta { TabIndex = 0, Name = "Allgemein", SlotCount = 98, IsLocked = false },
        new GuildBankTabMeta { TabIndex = 1, Name = "Raids", SlotCount = 98, IsLocked = true }
    }
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| GuildBankSyncMsg | 4026 | Inhalts-Sync |
| GuildBankOpenMsg | 4020 | Trigger |

---

### GuildBankSyncMsg (4026)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Permission

### Beschreibung
Überträgt Guild-Bank-Inhalte als Snapshot oder Delta. Analoge Struktur zu `BankSync`, aber Guild-spezifisch.

### Im Scope ✅
- Full/Deltas
- Revision
- Slots/Tabs für Guild-Bank

### Nicht im Scope ❌
- Persönliche Bank

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildId | long | Gilde | Ja |
| Revision | long | Revision | Ja |
| FullSync | bool | Vollständiger Snapshot? | Ja |
| Tabs | List<GuildBankTabDto> | Tabs/Slots | Ja |
| CorrelationId | string | Korrelations-ID | Nein |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuildBankSyncMsg)]
public class GuildBankSyncMsg : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuildBankSyncMsg;
    [Key(1)] public long GuildId { get; set; }
    [Key(2)] public long Revision { get; set; }
    [Key(3)] public bool FullSync { get; set; }
    [Key(4)] public List<GuildBankTabDto> Tabs { get; set; } = new();
    [Key(5)] public string? CorrelationId { get; set; }
}
```

### Server-Verhalten
- Sendet FullSync bei Open oder Revision-Konflikt.
- Sendet Delta nach Mutationen.

### Client-Verhalten
- Aktualisiert Guild-Bank UI.
- Verwift Revisionen ≤ aktuelle.

### Flow-Diagramm
```
Server                           Client
  │                                │
  │ GuildBankSyncMsg (4026)        │
  │───────────────────────────────►│
```

### Beispiel Payloads
```csharp
new GuildBankSyncMsg
{
    GuildId = 77,
    Revision = 10,
    FullSync = true,
    Tabs = new List<GuildBankTabDto>()
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| GuildBankOpenMsg | 4020 | Trigger |
| GuildBankDepositMsg | 4022 | Delta |
| GuildBankWithdrawMsg | 4023 | Delta |
| GuildBankTabInfo | 4025 | Metadaten |

---

### VoidStorageOpen (4040)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet Void Storage (Appearance-Lager). Funktioniert wie BankOpen, aber mit Void-spezifischen Restriktionen.

### Im Scope ✅
- Snapshot-Anforderung für Void Storage
- Revision-basierte Deltas
- Appearance-only Items

### Nicht im Scope ❌
- Persönliche Bank Items mit Stats

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| KnownRevision | long | Letzte Revision | Ja |
| CharacterId | long | Charakter | Ja |

### Erwartete Response
- `VoidStorageSync` (4044) FullSync

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoidStorageOpen)]
public class VoidStorageOpen : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoidStorageOpen;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public long KnownRevision { get; set; }
    [Key(3)] public long CharacterId { get; set; }
}
```

### Server-Verhalten
- Validiert Auth.
- Baut VoidStorage Snapshot (Appearance Items only).
- Sendet `VoidStorageSync` Full.

### Client-Verhalten
- Öffnet UI, wartet auf Sync.

### Flow-Diagramm
```
Client                          Server
  │                               │
  │ VoidStorageOpen (4040)        │
  │──────────────────────────────►│
  │                               │
  │ VoidStorageSync (4044 Full)   │
  │◄──────────────────────────────│
```

### Beispiel Payloads
```csharp
new VoidStorageOpen
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    KnownRevision = 0,
    CharacterId = 12345
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| UNAUTHORIZED | Session ungültig |
| CONTAINER_LOCKED | System gesperrt |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| VoidStorageSync | 4044 | Response |
| VoidStorageDeposit | 4042 | Mutation |
| VoidStorageWithdraw | 4043 | Mutation |

---

### VoidStorageClose (4041)

**Richtung:** 📤 Client → Server / 📥 Ack  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Schließt Void Storage Session, analog zu `BankClose`.

### Im Scope ✅
- Session-Ende
- Ack

### Nicht im Scope ❌
- Mutationen

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |

### Erwartete Response
- `VoidStorageClose` Ack (gleiche Message-ID)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoidStorageClose)]
public class VoidStorageClose : IClientMessage, IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.VoidStorageClose;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
}
```

### Server-Verhalten
- Entfernt Locks, sendet Ack.

### Client-Verhalten
- Schließt UI nach Ack.

### Flow-Diagramm
```
Client                           Server
  │                                │
  │ VoidStorageClose (4041)        │
  │───────────────────────────────►│
  │                                │
  │ VoidStorageClose (Ack)         │
  │◄───────────────────────────────│
```

### Beispiel Payloads
```csharp
new VoidStorageClose { ClientRequestId = reqId };
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| VoidStorageOpen | 4040 | Start |

---

### VoidStorageDeposit (4042)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lagert ein Item in den Void Storage ein. Nur appearance-fähige Items erlaubt. Erfordert Gebühren (Transmog-Kosten).

### Im Scope ✅
- Deposit von Inventory → Void Storage
- Kostenvalidierung
- Idempotenz

### Nicht im Scope ❌
- Void → Inventory (Withdraw)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| SourceInventorySlot | int | Inventar-Slot | Ja |
| TargetSlotIndex | int | Void-Slot | Ja |
| ItemInstanceId | long | Item | Ja |
| KnownRevision | long | Revision | Ja |

### Erwartete Response
- `VoidStorageSync` Delta + optional `ErrorMessage`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoidStorageDeposit)]
public class VoidStorageDeposit : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoidStorageDeposit;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public int SourceInventorySlot { get; set; }
    [Key(3)] public int TargetSlotIndex { get; set; }
    [Key(4)] public long ItemInstanceId { get; set; }
    [Key(5)] public long KnownRevision { get; set; }
}
```

### Server-Verhalten
- Prüft Appearance-Fähigkeit, Bind-Status, Kosten.
- Führt Transfer und Kostenabbuchung durch.
- Sendet `VoidStorageSync` Delta + InventoryUpdate.

### Client-Verhalten
- Sperrt Slots.
- Wartet auf Delta; zeigt Kostenverbrauch an.

### Flow-Diagramm
```
Client                               Server
  │                                    │
  │ VoidStorageDeposit (4042)          │
  │───────────────────────────────────►│
  │                                    │ Validate + Charge + Move
  │ VoidStorageSync (4044 Delta)       │
  │◄───────────────────────────────────│
```

### Beispiel Payloads
```csharp
new VoidStorageDeposit
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    SourceInventorySlot = 15,
    TargetSlotIndex = 2,
    ItemInstanceId = 99001234,
    KnownRevision = 3
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| VOID_RESTRICTION | Item nicht appearance-fähig |
| CURRENCY_INSUFFICIENT | Kosten nicht gedeckt |
| INVALID_SLOT | Slot ungültig |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| VoidStorageSync | 4044 | Delta |
| InventoryUpdate | 500 | Inventar-Delta |

---

### VoidStorageWithdraw (4043)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entnimmt ein Item aus dem Void Storage zurück ins Inventar.

### Im Scope ✅
- Void → Inventory Transfer
- Revision-Check
- Idempotenz

### Nicht im Scope ❌
- Deposit

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| SourceSlotIndex | int | Void-Slot | Ja |
| TargetInventorySlot | int | Inventar-Slot | Ja |
| ItemInstanceId | long | Item | Ja |
| KnownRevision | long | Revision | Ja |

### Erwartete Response
- `VoidStorageSync` Delta + InventoryUpdate

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoidStorageWithdraw)]
public class VoidStorageWithdraw : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoidStorageWithdraw;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public int SourceSlotIndex { get; set; }
    [Key(3)] public int TargetInventorySlot { get; set; }
    [Key(4)] public long ItemInstanceId { get; set; }
    [Key(5)] public long KnownRevision { get; set; }
}
```

### Server-Verhalten
- Validiert Slot & Ownership.
- Führt Transfer aus, persistiert.
- Sendet Delta + InventoryUpdate.

### Client-Verhalten
- Aktualisiert UI nach Delta.
- Bei Fehler zeigt entsprechenden Code.

### Flow-Diagramm
```
Client                                 Server
  │                                      │
  │ VoidStorageWithdraw (4043)           │
  │────────────────────────────────────► │
  │                                      │
  │ VoidStorageSync (4044 Delta)         │
  │◄──────────────────────────────────── │
  │ InventoryUpdate (0500)               │
  │◄──────────────────────────────────── │
```

### Beispiel Payloads
```csharp
new VoidStorageWithdraw
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    SourceSlotIndex = 2,
    TargetInventorySlot = 18,
    ItemInstanceId = 99001234,
    KnownRevision = 4
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| INVALID_SLOT | Slot leer/ungültig |
| STACK_LIMIT | Inventar voll |
| REVISION_CONFLICT | Snapshot neu laden |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| VoidStorageSync | 4044 | Delta |
| InventoryUpdate | 500 | Inventar |

---

### VoidStorageSync (4044)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Snapshot/Delta des Void Storage. Analoge Struktur zu BankSync, aber ausschließlich Appearance-Items.

### Im Scope ✅
- Full/Deltas
- Revision
- Slot-Daten

### Nicht im Scope ❌
- Persönliche Bank
- Reagent Bank

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | string | Korrelations-ID | Nein |
| Revision | long | Revision | Ja |
| FullSync | bool | Vollständig? | Ja |
| Slots | List<VoidStorageSlotDto> | Slots | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoidStorageSync)]
public class VoidStorageSync : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.VoidStorageSync;
    [Key(1)] public string? CorrelationId { get; set; }
    [Key(2)] public long Revision { get; set; }
    [Key(3)] public bool FullSync { get; set; }
    [Key(4)] public List<VoidStorageSlotDto> Slots { get; set; } = new();
}
```

### Server-Verhalten
- Sendet Full bei Open, Delta bei Mutationen.

### Client-Verhalten
- Aktualisiert UI entsprechend.
- Verwirft alte Revisionen.

### Flow-Diagramm
```
Server                      Client
  │                           │
  │ VoidStorageSync (4044)    │
  │──────────────────────────►│
```

### Beispiel Payloads
```csharp
new VoidStorageSync
{
    CorrelationId = requestId,
    Revision = 5,
    FullSync = true,
    Slots = new List<VoidStorageSlotDto>()
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| VoidStorageOpen | 4040 | Trigger |
| VoidStorageDeposit | 4042 | Delta |
| VoidStorageWithdraw | 4043 | Delta |

---

### ReagentBankOpen (4050)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet Reagenzienbank (Materiallager). Analog zu BankOpen, aber nur für Whitelist-Materialien.

### Im Scope ✅
- Snapshot-Anforderung
- Revision-Handling

### Nicht im Scope ❌
- Appearance Items

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| KnownRevision | long | Letzte Revision | Ja |
| CharacterId | long | Charakter | Ja |

### Erwartete Response
- `ReagentBankSync` (4052) FullSync

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReagentBankOpen)]
public class ReagentBankOpen : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReagentBankOpen;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public long KnownRevision { get; set; }
    [Key(3)] public long CharacterId { get; set; }
}
```

### Server-Verhalten
- Validiert Auth, erstellt Snapshot.
- Sendet `ReagentBankSync` Full.

### Client-Verhalten
- Öffnet UI nach Sync.

### Flow-Diagramm
```
Client                           Server
  │                                │
  │ ReagentBankOpen (4050)         │
  │───────────────────────────────►│
  │                                │
  │ ReagentBankSync (4052 Full)    │
  │◄───────────────────────────────│
```

### Beispiel Payloads
```csharp
new ReagentBankOpen
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    KnownRevision = 0,
    CharacterId = 12345
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| UNAUTHORIZED | Session ungültig |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| ReagentBankSync | 4052 | Response |
| ReagentBankDeposit | 4051 | Mutation |

---

### ReagentBankDeposit (4051)

**Richtung:** 📤 Client → Server  
**Frequenz:** Hoch (QoL Deposit All)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lagert Materialien aus Inventar in die Reagenzienbank ein. Unterstützt „Deposit All Reagents“. Anti-Dupe durch Revision- und Slot-Locks.

### Im Scope ✅
- Einzel- und Batch-Deposit
- Whitelist-Prüfung
- Idempotenz

### Nicht im Scope ❌
- Withdraw (nicht Teil des Prototyps)
- Nicht-Reagenz-Items

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientRequestId | string | Korrelations-ID | Ja |
| Items | List<ReagentDepositItem> | Liste von Items | Ja |
| KnownRevision | long | Revision | Ja |

`ReagentDepositItem`: InventorySlot, ItemInstanceId, Quantity.

### Erwartete Response
- `ReagentBankSync` (4052 Delta) + InventoryDelta

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReagentBankDeposit)]
public class ReagentBankDeposit : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ReagentBankDeposit;
    [Key(1)] public string ClientRequestId { get; set; } = string.Empty;
    [Key(2)] public List<ReagentDepositItem> Items { get; set; } = new();
    [Key(3)] public long KnownRevision { get; set; }
}
```

### Server-Verhalten
- Validiert jedes Item gegen Reagent-Whitelist.
- Führt Batch-Transfer mit Atomarität pro Request.
- Sendet Delta + InventoryUpdate.

### Client-Verhalten
- Sperrt betroffene Inventar-Slots.
- Erwartet Delta; bei Teilfehler zeigt Error pro Item.

### Flow-Diagramm
```
Client                                   Server
  │                                        │
  │ ReagentBankDeposit (4051)              │
  │───────────────────────────────────────►│
  │                                        │ Validate + Move (batch)
  │ ReagentBankSync (4052 Delta)           │
  │◄───────────────────────────────────────│
  │ InventoryUpdate (0500)                 │
  │◄───────────────────────────────────────│
```

### Beispiel Payloads
```csharp
new ReagentBankDeposit
{
    ClientRequestId = Guid.NewGuid().ToString("N"),
    KnownRevision = 1,
    Items = new List<ReagentDepositItem>
    {
        new ReagentDepositItem { InventorySlot = 5, ItemInstanceId = 7000100, Quantity = 200 },
        new ReagentDepositItem { InventorySlot = 6, ItemInstanceId = 7000101, Quantity = 50 }
    }
};
```

### Error Codes

| Code | Bedeutung |
| ---- | --------- |
| REAGENT_RESTRICTION | Item nicht erlaubt |
| REVISION_CONFLICT | Snapshot neu laden |
| RATE_LIMITED | Zu viele Requests |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| ReagentBankSync | 4052 | Delta |
| InventoryUpdate | 500 | Inventar |

---

### ReagentBankSync (4052)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel/Hoch (bei Batch)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Snapshot oder Delta der Reagenzienbank. Struktur analog zu BankSync, aber ohne Tabs (flache Slot-Liste).

### Im Scope ✅
- Full/Deltas
- Revision
- Slots

### Nicht im Scope ❌
- Personal Bank
- Void Storage

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorrelationId | string | Korrelations-ID | Nein |
| Revision | long | Revision | Ja |
| FullSync | bool | Vollständig? | Ja |
| Slots | List<ReagentSlotDto> | Slots | Ja |

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ReagentBankSync)]
public class ReagentBankSync : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ReagentBankSync;
    [Key(1)] public string? CorrelationId { get; set; }
    [Key(2)] public long Revision { get; set; }
    [Key(3)] public bool FullSync { get; set; }
    [Key(4)] public List<ReagentSlotDto> Slots { get; set; } = new();
}
```

### Server-Verhalten
- Sendet Full bei Open, Delta bei Deposit.

### Client-Verhalten
- Aktualisiert UI.
- Verwirft veraltete Revisionen.

### Flow-Diagramm
```
Server                          Client
  │                               │
  │ ReagentBankSync (4052)        │
  │──────────────────────────────►│
```

### Beispiel Payloads
```csharp
new ReagentBankSync
{
    CorrelationId = requestId,
    Revision = 2,
    FullSync = false,
    Slots = new List<ReagentSlotDto>()
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|----|-----------|
| ReagentBankOpen | 4050 | Trigger |
| ReagentBankDeposit | 4051 | Delta |

---

## 🗑️ Obsolete Messages

Aktuell sind in Range 4000-4099 keine Messages als **DEPRECATED** markiert. Sollte eine Message obsolet werden, folgt hier eine Kennzeichnung inkl. Migrationsempfehlung.

---

## 🧨 Edge Cases & Fehlerfälle

- **Revision Konflikt:** Jeder Mutation-Request prüft `KnownRevision`. Bei Konflikt: `BankSync Full` + ErrorCode `REVISION_CONFLICT`.
- **Out-of-Order Deltas:** Client verwirft Deltas mit Revision ≤ current und fordert bei Lücke über erneutes `BankOpen` neuen Snapshot an.
- **Disconnected während Transaktion:** Locks werden per Timeout (5s) freigegeben, Operation wird gerollbackt, bei Reconnect folgt FullSync.
- **Inventory Full bei Withdraw:** Fehler `STACK_LIMIT`, Bank unverändert, keine Delta außer optionalem No-Op Sync.
- **Cost Mismatch:** Wenn CurrencyBalance sich zwischen Client-Request und Server-Check ändert → `CURRENCY_INSUFFICIENT`, kein Commit.
- **Anti-Dupe Retry:** Wiederholung mit gleicher `ClientRequestId` liefert identisches Result, keine doppelten Items.
- **Search/Sort State:** Client-seitig, kein Server-Speicher; bei FullSync wird SortMode nur als Vorschlag übermittelt.
- **Bulk Deposit All Reagents:** Server limitiert Batch-Größe (z.B. 60 Items). Überschreitung → Fehler `RATE_LIMITED` oder partielle Verarbeitung mit `RemainingQuantity` in Inventar.
- **Cross-System Locks:** Wenn Equipment (3900) gerade Item nutzt, Bank-Deposit/Withdraw wird abgelehnt mit `CONTAINER_LOCKED` oder `ITEM_LOCKED`.

---

## 🧪 Testfälle & Szenario-Matrizen

Die folgenden Matritzen dienen der Verifikation der Bank-Implementierung. Jede Zeile ist ein eigenständiger Testfall, der deterministische Ergebnisse erwarten lässt. Alle Fälle nutzen echte Message-Typen; keine Platzhalter.

### BankOpen / BankClose Szenarien

1. **Happy Path:** BankOpen mit KnownRevision=0 → BankSync Full → BankClose Ack.
2. **Revision Mismatch:** BankOpen mit KnownRevision=10, Server hat 12 → FullSync geliefert, ErrorCode im Log.
3. **Unauthorized:** Session abgelaufen → ErrorMessage 910 (Server schließt Verbindung).
4. **Concurrent Open:** Zwei BankOpen parallel → zweiter Request erhält `CONTAINER_LOCKED`.
5. **Close Timeout:** Client sendet BankClose, Ack verzögert >5s → Client schließt UI, aber akzeptiert spätes Ack, keine Deltas mehr.
6. **Reconnect Restore:** Nach Disconnect sendet Reconnect → BankOpen → FullSync mit gespeicherter Revision.
7. **NPC Distance Loss:** BankOpen erfolgreich, Spieler verlässt NPC-Range → Server sendet BankClose Reason="Out of range".
8. **Rate Limit:** >5 BankOpen in 2 Sekunden → `RATE_LIMITED`.
9. **Audit Trail:** BankOpen/Close erzeugen Audit-Einträge (CharId, Time).
10. **Lock Release:** Nach BankClose sind Inventory- und Bank-Locks aufgehoben, weitere Mutationen erlaubt.

### BankDeposit Szenarien

| # | Beschreibung | Erwartung |
|---|--------------|-----------|
| 1 | Deposit voller Stack in leeren Slot | Success, RemainingQuantity=0, Delta mit Slot gefüllt |
| 2 | Deposit Teilstack in leeren Slot | Success, Rest=0, Slot Quantity=Teilmenge |
| 3 | Deposit in vorhandenen Stack (gleiche ItemId) bis MaxStack | Success, Rest=0 oder >0 falls Limit erreicht |
| 4 | Deposit in vorhandenen Stack mit MaxStack-Überschreitung | Success=false, Error=STACK_LIMIT, keine Mutation |
| 5 | Deposit mit falschem ItemInstanceId | Error=ITEM_NOT_FOUND |
| 6 | Deposit aus Bank→Bank (Move) | Erfolg, SourceSlot geleert/Rest, Target befüllt |
| 7 | Deposit während Container-Lock | Error=CONTAINER_LOCKED |
| 8 | Duplicate RequestId erneut gesendet | Idempotentes identisches Result |
| 9 | Deposit mit REVISION_CONFLICT | Server sendet FullSync, keine Mutation |
| 10 | Deposit mit TARGET Tab locked | Error=TAB_LOCKED |
| 11 | Deposit auf ungültigen SlotIndex | Error=INVALID_SLOT |
| 12 | Deposit Reagent in Personal Bank | Success (wenn erlaubt), sonst REAGENT_RESTRICTION |
| 13 | Deposit Void-only Item in Bank | VOID_RESTRICTION Error |
| 14 | Deposit Item mit ItemLock (z.B. im Trade) | Error=ITEM_LOCKED |
| 15 | Deposit während Combat (falls Policy) | Error=RATE_LIMITED oder POLICY_VIOLATION |
| 16 | Deposit All Hotkey (Batch) → mehrere BankDeposit Requests sequentiell | Alle Deltas in Reihenfolge, Revision++ je Mutation |
| 17 | Deposit mit gebundenem Item von anderem Account | Error=ITEM_MISMATCH |
| 18 | Deposit bei voller Kapazität | Error=STACK_LIMIT oder TAB_LOCKED |
| 19 | Deposit mit negativer Quantity | Error=INVALID_PAYLOAD (Transport-Ebene) |
| 20 | Deposit mit Quantity=0 | Server ignoriert, sends Error=INVALID_PAYLOAD |

### BankWithdraw Szenarien

1. Withdraw voller Stack in leeres Inventar-Slot → Success, InventoryUpdate + BankDelta.
2. Withdraw Teilstack in bestehendes Inventar-Stack → Success, Rest ggf. 0, Inventory Stack erhöht.
3. Withdraw bei Inventar voll → Error=STACK_LIMIT, keine Mutation.
4. Withdraw falsche ItemInstanceId → Error=ITEM_NOT_FOUND.
5. Withdraw mit Revision-Konflikt → FullSync statt Mutation.
6. Withdraw von gelocktem Slot → Error=CONTAINER_LOCKED.
7. Withdraw Duplicate RequestId → Idempotentes Result.
8. Withdraw aus Tab, der noch nicht freigeschaltet ist → Error=TAB_LOCKED.
9. Withdraw während Netzwerk-Lag (späte Deltas) → Client akzeptiert nur höchste Revision.
10. Withdraw während parallelem Deposit auf selben Slot → einer gewinnt, anderer erhält CONTAINER_LOCKED.
11. Withdraw mit Quantity größer als Stack → Server nimmt verfügbaren Stack, RemainingQuantity>0.
12. Withdraw bei laufendem Trade-Lock → Error=ITEM_LOCKED.
13. Withdraw bei Anti-Cheat Flag → ErrorMessage 910 + ForceDisconnect optional.
14. Withdraw bei verlorenem NPC-Range → Server sendet BankClose, Request verworfen.
15. Withdraw auf Inventory-Slot belegt von Pending Action → Error=STACK_LIMIT oder ITEM_LOCKED.

### Slot/Tab Purchase Szenarien

| Fall | Beschreibung | Ergebnis |
|------|--------------|----------|
| 1 | SlotPurchase mit ausreichend Gold | Success, NewSlotCount>old, CurrencyDelta<0 |
| 2 | SlotPurchase ohne Gold | Error=CURRENCY_INSUFFICIENT |
| 3 | SlotPurchase über MaxSlots | Error=STACK_LIMIT oder TAB_LOCKED |
| 4 | SlotPurchase Duplicate RequestId | Idempotentes Result, CurrencyDelta identisch |
| 5 | TabPurchase bei MaxTabs | Error=TAB_LOCKED |
| 6 | TabPurchase mit Währungsmangel | Error=CURRENCY_INSUFFICIENT |
| 7 | TabPurchase während FullSync-Lücke | Server liefert FullSync mit neuem Tab oder Error=REVISION_CONFLICT |
| 8 | SlotPurchase bei gesperrter Bank (maintenance) | Error=CONTAINER_LOCKED |
| 9 | TabPurchase + direktes SlotPurchase (Back-to-back) | Beide Deltas in Reihenfolge, Revision korrekt inkrementiert |
| 10 | TabPurchase mit ClientRequestId wiederholt | Idempotentes neues Tab einmalig, weitere Aufrufe liefern identischen Sync |

### BankSync Konsistenzprüfungen

1. FullSync enthält **alle** Tabs, Slots, CurrencyBalance, UnlockedTabs.
2. Delta enthält nur geänderte Tabs, FullSync=false.
3. Revision steigt immer um ≥1, nie rückwärts.
4. CorrelationId entspricht RequestId für Responses.
5. Capacity = Sum(SlotCount aller Tabs).
6. Tabs sind sortiert nach TabIndex, Slots nach SlotIndex.
7. Kein Slot hat Quantity<0 oder >MaxStack.
8. Kein Tab hat SlotCount < Slots.Count.
9. CurrencyBalance nie negativ.
10. Bei FullSync nach Konflikt stimmen Inventar-Deltas und BankSync zeitseitig überein (keine doppelte Items).

### Void Storage Szenarien

| # | Szenario | Erwartung |
|---|----------|-----------|
| 1 | VoidStorageOpen mit KnownRevision=0 | FullSync |
| 2 | Deposit Appearance-Item | Success, Delta |
| 3 | Deposit Non-Appearance | Error=VOID_RESTRICTION |
| 4 | Deposit mit Kosten ohne Gold | Error=CURRENCY_INSUFFICIENT |
| 5 | Withdraw in volles Inventar | Error=STACK_LIMIT |
| 6 | Duplicate RequestId auf Deposit | Idempotent |
| 7 | Close während laufendem Deposit | Deposit evtl. Error=CONTAINER_LOCKED, Close Ack trotzdem |
| 8 | FullSync nach Reconnect | Revision stabil, alle Items sichtbar |
| 9 | Delta out-of-order | Client verwirft alte Revision |
| 10 | Deposit All (mehrere Requests) | Revisions steigen sequentiell, keine Duplikate |

### Reagent Bank Szenarien

1. Open → FullSync mit allen Slots.
2. Deposit whitelist Item → Success, Delta.
3. Deposit non-whitelist Item → Error=REAGENT_RESTRICTION.
4. Deposit Batch > Limit → RATE_LIMITED.
5. Deposit bei voller Reagent Bank → STACK_LIMIT.
6. Duplicate RequestId → Idempotentes Ergebnis.
7. Reconnect → FullSync.
8. Lücke in Revision → erneutes Open liefert FullSync.
9. Inventory-Delta Out-of-Order → Inventar-System korrigiert mit eigener Revision.
10. Anti-Cheat Flag → ErrorMessage 910, evtl. Disconnect.

### Guild Bank Event Szenarien

- OpenMsg ohne Permission → wird nicht gesendet, Server loggt Ablehnung.
- DepositMsg broadcast nur an online berechtigte Mitglieder.
- WithdrawMsg mit Permission-Änderung mitten in Session → Event trotzdem gesendet, Sync danach.
- LogMsg Sequenz-Lücke → Client fordert nach (über Guild-System), markiert UI.
- TabInfo Änderung (Rename/Lock) → TabInfo gesendet vor Sync.
- SyncMsg Delta mit FullSync=false enthält nur geänderte Tabs, kein Currency.
- CloseMsg Reason=Maintenance → UI schließt, zeigt Hinweis.

---

## 🔄 Detaillierte Flow-Spezifikationen

### Vollständiger Sync Flow (BankOpen → BankSync Full)

1. Client initiiert BankOpen mit `KnownRevision`.
2. Server prüft Auth, NPC-Range, Lock.
3. Server bestimmt ob FullSync nötig (`KnownRevision != currentRevision` oder `ForceFull` Flag).
4. Server lädt BankContainer + Items aus DB/Cache.
5. Server berechnet CurrencyBalance (falls separat) und Tab-Metadaten.
6. Server serialisiert BankSync (FullSync=true) mit allen Tabs/Slots.
7. Server sendet BankSync, entfernt Lock.
8. Client validiert Revision, ersetzt kompletten lokalen Zustand.
9. Client initialisiert Search/Sort lokal, setzt Filter default.
10. Client sendet optionales Telemetrie-Event „BankOpened“ (nicht Bestandteil des Ranges).

### Deposit Flow (BankDeposit → BankDepositResult → BankSync Delta)

1. Client sperrt UI-Slots (Quelle/Ziel).
2. Client sendet BankDeposit mit RequestId, KnownRevision, Quell-/Ziel-Slot.
3. Server prüft RateLimit, Auth, Revision.
4. Server lockt Quelle/Ziel.
5. Server validiert ItemOwnership, BindStatus, Container-Typ, Tab/Slot Bounds.
6. Server berechnet neue Stackmengen, prüft MaxStack.
7. Server schreibt InventoryDelta (-Quantity) + BankDelta (+Quantity) atomar.
8. Server erhöht Revision, persistiert.
9. Server sendet BankDepositResult (Success/Failure, RemainingQuantity, NewRevision).
10. Server sendet BankSync Delta (FullSync=false) mit geänderten Slots.
11. Server sendet InventoryUpdate (0500) mit neuem Stack.
12. Client entsperrt Slots, aktualisiert UI mittels BankSync und InventoryUpdate.
13. Client loggt Audit Event lokal (optional).

### Withdraw Flow (BankWithdraw → Result → Delta)

1. Client sperrt Slots.
2. BankWithdraw Request mit RequestId, KnownRevision.
3. Server validiert, lockt Slots.
4. Server prüft Inventar-Kapazität/Stack.
5. Server verschiebt Menge (Teilstack falls nötig).
6. Server persistiert, erhöht Revision.
7. Server sendet BankWithdrawResult.
8. Server sendet BankSync Delta + InventoryUpdate.
9. Client entsperrt Slots, aktualisiert UI.
10. Bei RemainingQuantity>0 zeigt UI Hinweis.

### Tab Purchase Flow (BankTabPurchase → BankSync)

1. Client sendet Request.
2. Server prüft MaxTabs, Kosten, CurrencyBalance.
3. Server erstellt neuen Tab (Name default "Tab X", SlotCount Standard 42).
4. Server zieht Currency ab (Economy Range).
5. Server erhöht Revision, persistiert.
6. Server sendet BankSync Delta mit neuem Tab + UnlockedTabs+1.
7. Client fügt Tab hinzu, zeigt Kosten an.

### Slot Purchase Flow (BankSlotPurchase → Result → Delta)

1. Client sendet Request.
2. Server prüft TabStatus, MaxSlotCount, Kosten.
3. Server erhöht SlotCount, aktualisiert Capacity.
4. Server persistiert, erhöht Revision.
5. Server sendet Result (Kosten, NewSlotCount).
6. Server sendet Delta (TabMeta + neue leere Slots).
7. Client rendert neue Slots leer.

### Void Storage Flow (Open → Deposit/Withdraw → Sync)

1. Open → VoidStorageSync Full.
2. Deposit: Validate appearance, Kosten; Delta + InventoryUpdate.
3. Withdraw: Validate target slot; Delta + InventoryUpdate.
4. Close: Ack, Locks frei.

### Reagent Bank Flow (Open → Deposit → Sync)

1. Open → ReagentBankSync Full.
2. Deposit Batch: Validate whitelist; Delta + InventoryUpdate.
3. Close (implicit via UI) nicht gesondert im Range; Container bleibt offen bis NPC-Range verlassen.

### Guild Bank Event Flow

1. GuildSystem validiert Permission.
2. Sendet GuildBankOpenMsg + TabInfo + SyncMsg Full.
3. Bei Mutationen sendet DepositMsg/WithdrawMsg + SyncMsg Delta + LogMsg.
4. Bei Timeout/Leave sendet CloseMsg.

---

## 📦 Integrations-Checkliste

- **Inventory (0500):** Jede Mutation erzeugt konsistentes InventoryDelta. SequenceNumbers zwischen Bank-Range und Inventory-Range müssen unabhängig sein; UI muss beide verarbeiten.
- **Economy (3700):** Slot/Tab-Käufe ziehen Gold über Economy-Subsystem ab. Negative CurrencyBalance verhindern Kauf.
- **Equipment (3900):** Items in Equipment-Slots dürfen nicht direkt aus Bank gezogen werden; erst Withdraw → Inventory → EquipItem.
- **Loot (3100):** Auto-Loot kann optional direkt in Reagent Bank verschieben, aber nur über Server-Policy; sonst normal in Inventar.
- **Trading (1100):** Während aktiver Trade-Session sind Bank-Mutationen für betroffene Items gesperrt.
- **Mail (1800):** Attachments werden beim Abholen direkt ins Inventar gelegt, nicht in Bank; nachträgliche Einlagerung via BankDeposit.
- **Guild (0800):** GuildBank-Events hier nur lesend; Schreiboperationen laufen im Guild-Bereich und spiegeln sich als Events/Sync wider.
- **Anti-Cheat:** Schnelle Folgeoperationen (>10/s) triggern AntiCheatWarning (924) und ggf. Disconnect.

---

## 🧭 Qualitätssicherung & Telemetrie

- **Metriken:** `bank.open.count`, `bank.deposit.count`, `bank.deposit.latency_ms`, `bank.delta.size_bytes`, `bank.revision.gap`.
- **Logs:** Serilog-Events mit Kontext (RequestId, Revision, CharId, NPCId).
- **Tracing:** Jede Mutation trägt TraceId/SpanId; BankSync enthält `CorrelationId` = RequestId für Matching.
- **Alerts:** Alarm bei `delta.size_bytes > 64KB`, bei `revision.gap > 3`, bei `duplicate_request_rate > 0.1`.
- **Load Tests:** 50 parallele Spieler, 10 Deposits/s, erwartete Delta-Größe <5KB, 99p Latenz <150ms.
- **UI Telemetry:** Client sendet optional Frontend-Metriken (RenderTime, SlotCount), nicht Teil des Ranges.

---

## 🧭 Beispielhafte End-to-End Cases

### Case A: Neues Item aus Loot in Bank verschieben

1. LootWindow liefert ItemInstanceId=50001 in Inventar Slot 8.
2. Spieler öffnet Bank (BankOpen → BankSync Full, Revision=20).
3. Spieler zieht Item nach Bank Tab 0 Slot 4 → BankDeposit.
4. Server prüft Ownership, Lock, MaxStack.
5. BankDepositResult Success, NewRevision=21.
6. BankSync Delta: Tab0 Slot4 ItemInstanceId=50001 Quantity=1.
7. InventoryUpdate: Slot8 geleert.
8. UI zeigt Item in Bank, Inventar-Slot leer.

### Case B: Reagent Deposit All

1. Spieler hat 5 Reagenzien in Inventar Slots 1-5.
2. ReagentBankOpen → ReagentBankSync Full (Revision=2).
3. Client sendet ReagentBankDeposit mit 5 Items (RequestId X).
4. Server validiert jedes Item, bewegt alle, Revision=3.
5. ReagentBankSync Delta mit allen betroffenen Slots.
6. InventoryUpdate leert Slots 1-5.
7. Client UI aktualisiert Reagent Bank mit neuen Stacks.

### Case C: Slot Purchase ohne Gold

1. BankOpen → Revision=10.
2. BankSlotPurchase SlotsToBuy=7, Kosten 15000 Gold.
3. Economy-Check: Spieler hat 12000 Gold.
4. Result: Success=false, ErrorCode=CURRENCY_INSUFFICIENT, Revision bleibt 10.
5. Kein Delta gesendet.
6. Client zeigt Fehler, Slots bleiben unverändert.

### Case D: Void Storage Deposit mit falschem Item

1. VoidStorageOpen → FullSync Revision=1.
2. Client versucht Deposit eines Rüstungsteils (nicht appearance-fähig).
3. Server prüft Restriktion → Error VOID_RESTRICTION, keine Mutation.
4. Optionaler ErrorMessage 910 nicht nötig, da validierter Fehler.
5. Client zeigt Hinweis, UI unverändert.

### Case E: Parallel Withdraw und Deposit auf gleichen Slot

1. Zwei Requests kurz hintereinander:
   - R1: BankDeposit in Tab0 Slot2.
   - R2: BankWithdraw aus Tab0 Slot2.
2. Server lockt Slot2 für R1, R2 erhält CONTAINER_LOCKED.
3. R1 Commit → Revision+1, Delta.
4. Client von R2 erhält Fehler, bleibt bei alter Revision, kann erneut versuchen nach Delta.

### Case F: Guild Bank Event Broadcast

1. GuildMemberA deponiert Item.
2. Guild-System verarbeitet Request (nicht in 4000er Range).
3. Server sendet GuildBankDepositMsg an alle Online-Mitglieder.
4. Danach GuildBankSyncMsg Delta mit geänderten Slots.
5. Clients aktualisieren UI; LogMsg liefert Audit-Eintrag.

---

## 🧰 Entwickler-Checklisten (Server)

- [ ] Vor jeder Mutation: Revision prüfen, Rate-Limit prüfen.
- [ ] Slots locken (Quelle & Ziel) mit Timeout und Deadlock-Schutz.
- [ ] Items aus persistenter Quelle laden (kein Trust des Clients).
- [ ] Currency-Delta nur nach erfolgreicher Validierung abziehen.
- [ ] Atomic Commit für Bank + Inventory + Currency.
- [ ] Revision erhöhen erst nach Commit.
- [ ] Result + Delta senden, anschließend Locks freigeben.
- [ ] Audit-Log schreiben (CharId, RequestId, ItemInstanceId, Quantity, Delta, Success).
- [ ] Idempotenz-Cache (RequestId) 30s halten.
- [ ] Fehlerpfade: Locks immer freigeben, auch bei Exceptions.

## 🧰 Entwickler-Checklisten (Client)

- [ ] RequestIds als GUID pro Aktion.
- [ ] Slots bis Result/Delta sperren.
- [ ] Deltas nach Revision sortiert anwenden, veraltete verwerfen.
- [ ] Bei REVISION_CONFLICT → BankOpen erneut.
- [ ] Bei fehlender Response nach Timeout → Retry mit gleicher RequestId.
- [ ] UI-Search/Sort nur client-seitig, keine Rücksendung an Server.
- [ ] InventoryDelta und BankSync beide verarbeiten, bevor UI finalisiert.
- [ ] Fehlercodes in UI mappen (z.B. STACK_LIMIT → „Ziel voll“).
- [ ] Bei Close Ack abwarten, bevor UI verwirft.
- [ ] Telemetrie optional senden (nicht Teil des Protokolls).

---

## 🧱 Kompatibilitäts- und Migrationshinweise

- Der aktuelle Range nutzt ausschließlich MessagePack DTOs mit Key-Attributen. Änderungen an DTOs müssen Schlüssel-Stabilität sicherstellen.
- Neue Messages in 4000-4099 müssen in `MessageType.cs` ergänzt werden und hier dokumentiert werden.
- Obsolete Messages sind aktuell keine vorhanden; Deprecation erfolgt mit klarer Migration (z.B. BankMove → BankDeposit with SourceContainer=Bank).
- Client/Server Versioning: `BankSync` enthält keine Versionsfelder; Änderungen müssen über Feature Toggles (Connection Range) koordiniert werden.
- Binary Kompatibilität: Keine Null-Sprünge in Keys, keine Umordnung der Tabellen.
- Interop: Guild-Bank-Events nutzen identische Slot-/Tab-Konzepte für Konsistenz, aber getrennte Ranges für Schreiboperationen.

---

## 📎 Anhang (MessageType Enum Updates)

Keine neuen Enum-Einträge notwendig. Quelle: `shared/Mmo.Shared/Messaging/Enums/MessageType.cs` (Range 4000-4099 unverändert).

```csharp
// No additional entries required for category 40 in this specification.
```
