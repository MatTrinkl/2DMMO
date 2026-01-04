# 💎 Loot Messages (3100-3199)

**Kategorie:** 31  
**Range:** 3100-3199  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

### 🔄 Flow-Diagramme
- [Loot-System Architektur](#-loot-system-architektur)
- [Entity Death → Loot Flow](#entity-death--loot-flow)
- [Group Loot Roll Flow](#group-loot-roll-flow)
- [Master Looter Flow](#master-looter-flow)

### 🧠 Datenmodell
- [LootSourceType Enum](#lootsourcetype-enum)
- [LootContainerDto](#lootcontainerdto)
- [LootItemEntryDto](#lootitementrydto)
- [LootOwnership Konzept](#lootownership-konzept)

### 🎲 Drop Generation & Tables
- [Drop-Generierung](#drop-generierung)
- [Loot Tables](#loot-tables)
- [Rarity System](#rarity-system)

### 👥 Group Loot Regeln
- [LootMethod Enum](#lootmethod-enum)
- [Need/Greed/Pass Regeln](#needgreedpass-regeln)
- [Round Robin](#round-robin)
- [Master Looter](#master-looter)
- [Personal Loot](#personal-loot)

### 📦 Loot Window & Pickup
- [Loot Window Lifecycle](#loot-window-lifecycle)
- [Claim/Pickup Atomicity](#claimpickup-atomicity)

### ⏳ Despawn, Locking & Anti-Dupe
- [Despawn Timer](#despawn-timer)
- [Anti-Dupe Mechanismen](#anti-dupe-mechanismen)

### 🧱 DTOs / Interfaces
- [ILootMessage Interface](#ilootmessage-interface)
- [LootContainerDto](#lootcontainerdto-1)
- [LootItemEntryDto](#lootitementrydto-1)
- [LootRollDto](#lootrolldto)

### 🧩 Enums / ErrorCodes / Flags
- [LootErrorCode Enum](#looterrorcode-enum)
- [LootMethod Enum](#lootmethod-enum-1)
- [ItemQualityThreshold Enum](#itemqualitythreshold-enum)
- [RollType Enum](#rolltype-enum)

### ⚙️ Regeln & Validierung
- [Server-Autorität](#server-autorität)
- [Rate Limiting](#rate-limiting)
- [Security](#security)

### 📩 Aktive Messages (3100-3142)
- [LootWindowOpen (3100)](#lootwindowopen-3100)
- [LootWindowClose (3101)](#lootwindowclose-3101)
- [LootItem (3102)](#lootitem-3102)
- [LootItemResult (3103)](#lootitemresult-3103)
- [LootGold (3104)](#lootgold-3104)
- [LootAll (3105)](#lootall-3105)
- [LootWindowCloseResponse (3106)](#lootwindowcloseresponse-3106)
- [LootGoldResult (3107)](#lootgoldresult-3107)
- [LootAllResult (3108)](#lootallresult-3108)
- [LootRollStart (3110)](#lootrollstart-3110)
- [LootRollNeed (3111)](#lootrollneed-3111)
- [LootRollGreed (3112)](#lootrollgreed-3112)
- [LootRollPass (3113)](#lootrollpass-3113)
- [LootRollResult (3114)](#lootrollresult-3114)
- [LootRollWinner (3115)](#lootrollwinner-3115)
- [LootRollVoteResponse (3116)](#lootrollvoteresponse-3116)
- [LootMasterAssign (3120)](#lootmasterassign-3120)
- [LootRulesChange (3121)](#lootruleschange-3121)
- [LootThresholdChange (3122)](#lootthresholdchange-3122)
- [LootMasterAssignResult (3123)](#lootmasterassignresult-3123)
- [LootRulesChangeResult (3124)](#lootruleschangeresult-3124)
- [LootThresholdChangeResult (3125)](#lootthresholdchangeresult-3125)
- [PersonalLoot (3130)](#personalloot-3130)
- [BonusRollPrompt (3131)](#bonusrollprompt-3131)
- [BonusRollUse (3132)](#bonusrolluse-3132)
- [BonusRollResult (3133)](#bonusrollresult-3133)
- [RewardChoicePrompt (3140)](#rewardchoiceprompt-3140)
- [RewardChoiceSelect (3141)](#rewardchoiceselect-3141)
- [RewardChoiceResult (3142)](#rewardchoiceresult-3142)

### 🗑️ Obsolete Messages
- Keine

### 🧨 Edge Cases & Fehlerfälle
- [Inventory Full](#inventory-full)
- [Disconnect während Roll](#disconnect-während-roll)
- [Container Despawn](#container-despawn)

### 📎 Anhang
- [MessageType Enum Updates](#messagetype-enum-updates)
- [Request/Response Paare](#requestresponse-paare)
- [Integrationshinweise](#integrationshinweise)

---

## 📋 Überblick

Das Loot-System verwaltet die Verteilung von Items und Gold aus verschiedenen Quellen (Mob-Kills, Chests, Quest-Rewards). Das System ist **vollständig server-authoritative** - der Client kann niemals direkt bestimmen, welche Items er erhält.

### Kernprinzipien

1. **Server-Autorität**: Alle Loot-Entscheidungen (Drop-Tabellen, Roll-Ergebnisse, Zuweisung) werden ausschließlich vom Server getroffen
2. **Fairness**: Group-Loot-Systeme garantieren faire Verteilung (Round-Robin, Need/Greed)
3. **Atomarität**: Claim/Pickup-Operationen sind atomar - kein doppeltes Looten möglich
4. **Ownership**: Nur berechtigte Spieler können auf Loot zugreifen (Kill-Contribution, Party-Zugehörigkeit)

---

## 🔄 Loot-System Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    LOOT SYSTEM ARCHITEKTUR                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Loot Sources                    Loot Container                   │
│  ┌─────────────┐                 ┌─────────────────────────┐     │
│  │ Mob Death   │───────────────►│ ContainerId: Guid        │     │
│  │ Chest Open  │                 │ SourceType: EntityKill   │     │
│  │ Quest Reward│                 │ OwnerId: Guid            │     │
│  │ Fishing     │                 │ PartyId: Guid?           │     │
│  │ Gathering   │                 │ Items: List<LootItem>    │     │
│  └─────────────┘                 │ Gold: long               │     │
│                                   │ ExpiresAt: DateTime      │     │
│                                   └─────────────────────────┘     │
│                                              │                     │
│                                              ▼                     │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    LOOT DISTRIBUTION                         │ │
│  ├─────────────┬─────────────┬─────────────┬─────────────────┤ │
│  │ FreeForAll  │ RoundRobin  │ NeedGreed   │ MasterLooter    │ │
│  │ Wer zuerst  │ Reihum      │ Würfeln     │ Leader weist zu │ │
│  │ klickt      │ automatisch │ Need>Greed  │                 │ │
│  └─────────────┴─────────────┴─────────────┴─────────────────┘ │
│                                              │                     │
│                                              ▼                     │
│                                   ┌─────────────────────────┐     │
│                                   │ Player Inventory        │     │
│                                   │ (via InventorySlotUpdate)│     │
│                                   └─────────────────────────┘     │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

WICHTIG: Client kann NIEMALS direkt Loot-Inhalt bestimmen!
         Server generiert Drops, validiert Claims, verteilt Items.
```

### Entity Death → Loot Flow

```
Killer(s)               Zone Server                    Looter
  │                          │                           │
  │  [Entity HP = 0]         │                           │
  │─────────────────────────►│                           │
  │                          │  ┌───────────────────┐    │
  │                          │  │ 1. Generate Loot  │    │
  │                          │  │ 2. Create Container│   │
  │                          │  │ 3. Set Ownership  │    │
  │                          │  │ 4. Start Despawn  │    │
  │                          │  └───────────────────┘    │
  │                          │                           │
  │                          │  LootableSpawn (1430)     │
  │◄─────────────────────────│──────────────────────────►│
  │                          │  (Sparkle-Effect visible) │
  │                          │                           │
  │                          │         [Looter clicks]   │
  │                          │◄──────────────────────────│
  │                          │  Interact with Lootable   │
  │                          │                           │
  │                          │  ┌───────────────────┐    │
  │                          │  │ Validate Owner    │    │
  │                          │  │ Check Distance    │    │
  │                          │  └───────────────────┘    │
  │                          │                           │
  │                          │  LootWindowOpen (3100)    │
  │                          │──────────────────────────►│
  │                          │                           │
  │                          │  LootItem (3102)          │
  │                          │◄──────────────────────────│
  │                          │                           │
  │                          │  ┌───────────────────┐    │
  │                          │  │ ATOMIC OPERATION  │    │
  │                          │  │ 1. Remove from    │    │
  │                          │  │    Container      │    │
  │                          │  │ 2. Add to Inv     │    │
  │                          │  │ 3. Persist        │    │
  │                          │  └───────────────────┘    │
  │                          │                           │
  │                          │  LootItemResult (3103)    │
  │                          │──────────────────────────►│
  │                          │                           │
  │                          │  InventorySlotUpdate(501) │
  │                          │──────────────────────────►│
```

### Group Loot Roll Flow

```
Party Members (A,B,C)        Zone Server                    
  │                               │
  │  [Rare+ Item drops]           │
  │                               │
  │  LootRollStart (3110)         │
  │◄──────────────────────────────│ (Broadcast to all party)
  │  ├── ContainerId              │
  │  ├── SlotIndex                │
  │  ├── Item Info                │
  │  └── RollDuration: 30s        │
  │                               │
  │  [Player A: Need]             │
  │  LootRollNeed (3111)          │
  │──────────────────────────────►│
  │                               │
  │  LootRollVoteResponse (3116)  │
  │◄──────────────────────────────│
  │                               │
  │  LootRollResult (3114)        │
  │◄──────────────────────────────│ (Broadcast: A rolled Need)
  │                               │
  │  [Player B: Greed]            │
  │  LootRollGreed (3112)         │
  │──────────────────────────────►│
  │                               │
  │  LootRollResult (3114)        │
  │◄──────────────────────────────│ (Broadcast: B rolled Greed)
  │                               │
  │  [Player C: Pass]             │
  │  LootRollPass (3113)          │
  │──────────────────────────────►│
  │                               │
  │  LootRollResult (3114)        │
  │◄──────────────────────────────│ (Broadcast: C passed)
  │                               │
  │  [Timer ends OR all voted]    │
  │                               │
  │  ┌───────────────────────┐    │
  │  │ Evaluate Winner:      │    │
  │  │ 1. Need rolls highest │    │
  │  │ 2. Else Greed highest │    │
  │  │ 3. Tie: Random        │    │
  │  └───────────────────────┘    │
  │                               │
  │  LootRollWinner (3115)        │
  │◄──────────────────────────────│ (Broadcast: A wins)
  │                               │
  │  [Winner: InventorySlotUpdate]│
  │◄──────────────────────────────│
```

### Master Looter Flow

```
Master Looter              Zone Server              Party Member
  │                             │                        │
  │  [Loot drops]               │                        │
  │                             │                        │
  │  LootWindowOpen (3100)      │                        │
  │◄────────────────────────────│                        │
  │  (Only Master sees items)   │                        │
  │                             │                        │
  │  LootMasterAssign (3120)    │                        │
  │  ├── ContainerId            │                        │
  │  ├── SlotIndex              │                        │
  │  └── TargetPlayerId         │                        │
  │────────────────────────────►│                        │
  │                             │  ┌─────────────────┐   │
  │                             │  │ Validate:       │   │
  │                             │  │ - Is Master?    │   │
  │                             │  │ - Target valid? │   │
  │                             │  │ - Item exists?  │   │
  │                             │  └─────────────────┘   │
  │                             │                        │
  │  LootMasterAssignResult     │                        │
  │◄────────────────────────────│                        │
  │                             │                        │
  │                             │  InventorySlotUpdate   │
  │                             │───────────────────────►│
  │                             │  (Item to target)      │
```

---

## 🧠 Datenmodell

### LootSourceType Enum

```csharp
public enum LootSourceType : byte
{
    /// <summary>Loot from killed creature/NPC.</summary>
    EntityKill = 1,
    
    /// <summary>Loot from opened chest/container.</summary>
    Chest = 2,
    
    /// <summary>Loot from fishing.</summary>
    Fishing = 3,
    
    /// <summary>Loot from gathering node.</summary>
    Gathering = 4,
    
    /// <summary>Loot from pickpocket.</summary>
    Pickpocket = 5,
    
    /// <summary>Quest reward loot.</summary>
    QuestReward = 6,
    
    /// <summary>Mail attachment loot.</summary>
    MailAttachment = 7,
    
    /// <summary>Personal loot drop.</summary>
    PersonalDrop = 8
}
```

### LootContainerDto

```csharp
[MessagePackObject]
public class LootContainerDto
{
    [Key(0)] public Guid ContainerId { get; set; }
    [Key(1)] public LootSourceType SourceType { get; set; }
    [Key(2)] public Guid SourceEntityId { get; set; }
    [Key(3)] public Guid OwnerId { get; set; }
    [Key(4)] public Guid? PartyId { get; set; }
    [Key(5)] public List<LootItemEntryDto> Items { get; set; } = new();
    [Key(6)] public long GoldCopper { get; set; }
    [Key(7)] public long CreatedAtTicks { get; set; }
    [Key(8)] public long ExpiresAtTicks { get; set; }
    [Key(9)] public bool IsEmpty { get; set; }
}
```

### LootItemEntryDto

```csharp
[MessagePackObject]
public class LootItemEntryDto
{
    [Key(0)] public byte SlotIndex { get; set; }
    [Key(1)] public uint ItemId { get; set; }
    [Key(2)] public int Quantity { get; set; }
    [Key(3)] public ItemQuality Quality { get; set; }
    [Key(4)] public bool IsLocked { get; set; }
    [Key(5)] public Guid? LockedForPlayerId { get; set; }
    [Key(6)] public bool RequiresRoll { get; set; }
    [Key(7)] public Guid? RollId { get; set; }
}
```

### LootOwnership Konzept

```
┌─────────────────────────────────────────────────────────────────┐
│                     LOOT OWNERSHIP RULES                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Solo Player Kill:                                                │
│  ├── Owner = Killer                                               │
│  ├── Exclusive Loot Window für 60 Sekunden                        │
│  └── Nach 60s: FreeForAll (falls noch Items vorhanden)            │
│                                                                   │
│  Party Kill (Contribution-based):                                 │
│  ├── Owner = Party (PartyId gesetzt)                              │
│  ├── Jedes Party-Member mit Contribution kann looten              │
│  ├── LootMethod bestimmt Verteilung                               │
│  └── Contribution = Damage dealt OR Healing done                  │
│                                                                   │
│  Raid Kill:                                                       │
│  ├── Owner = Raid (RaidId gesetzt)                                │
│  ├── LootMethod: Group Loot ODER Master Looter                    │
│  └── Threshold bestimmt ab welcher Quality gerollt wird           │
│                                                                   │
│  Chest/Gathering:                                                 │
│  ├── Owner = First Interactor                                     │
│  └── Exclusive für 30 Sekunden                                    │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎲 Drop Generation & Tables

### Drop-Generierung

Der Server verwendet deterministische Loot-Tabellen. Die Generierung erfolgt beim Entity Death:

```csharp
public class LootGenerator
{
    public LootContainer GenerateLoot(
        IEntity source, 
        IReadOnlyList<DamageContribution> contributors,
        LootMethod partyLootMethod)
    {
        var container = new LootContainer
        {
            ContainerId = Guid.NewGuid(),
            SourceEntityId = source.PersistentId,
            SourceType = LootSourceType.EntityKill,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        
        // 1. Lookup Loot Table für Entity
        var lootTable = _lootTableRegistry.GetTable(source.TemplateId);
        
        // 2. Roll für jeden Eintrag in der Table
        foreach (var entry in lootTable.Entries)
        {
            if (_random.NextDouble() <= entry.DropChance)
            {
                var quantity = _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);
                container.Items.Add(new LootItemEntry
                {
                    SlotIndex = (byte)container.Items.Count,
                    ItemId = entry.ItemId,
                    Quantity = quantity,
                    Quality = _itemRegistry.GetQuality(entry.ItemId)
                });
            }
        }
        
        // 3. Gold berechnen (Level-based)
        container.GoldCopper = CalculateGoldDrop(source.Level);
        
        // 4. Ownership setzen
        SetOwnership(container, contributors, partyLootMethod);
        
        return container;
    }
}
```

### Loot Tables

```csharp
[MessagePackObject]
public class LootTableEntry
{
    [Key(0)] public uint ItemId { get; set; }
    [Key(1)] public float DropChance { get; set; }    // 0.0 - 1.0
    [Key(2)] public int MinQuantity { get; set; }
    [Key(3)] public int MaxQuantity { get; set; }
    [Key(4)] public ItemQuality? MinQualityOverride { get; set; }
}
```

### Rarity System

| Quality | Drop Chance Modifier | Color |
|---------|---------------------|-------|
| Poor (Grey) | 80-100% | #9D9D9D |
| Common (White) | 50-80% | #FFFFFF |
| Uncommon (Green) | 15-30% | #1EFF00 |
| Rare (Blue) | 3-10% | #0070DD |
| Epic (Purple) | 0.5-3% | #A335EE |
| Legendary (Orange) | 0.01-0.5% | #FF8000 |

---

## 👥 Group Loot Regeln

### LootMethod Enum

```csharp
public enum LootMethod : byte
{
    /// <summary>First come, first served.</summary>
    FreeForAll = 0,
    
    /// <summary>Items distributed in rotation.</summary>
    RoundRobin = 1,
    
    /// <summary>Roll Need/Greed for items above threshold.</summary>
    GroupLoot = 2,
    
    /// <summary>Party leader assigns items manually.</summary>
    MasterLooter = 3,
    
    /// <summary>Each player gets personal loot (not shared).</summary>
    PersonalLoot = 4
}
```

### Need/Greed/Pass Regeln

```
┌─────────────────────────────────────────────────────────────────┐
│                    NEED/GREED/PASS RULES                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  NEED (Höchste Priorität):                                        │
│  ├── Spieler benötigt Item für aktuellen Character                │
│  ├── Class-Restriction: Nur wenn Item für Class nutzbar          │
│  ├── Armor-Type: Nur wenn Armor-Typ nutzbar                       │
│  └── Roll: 1-100                                                  │
│                                                                   │
│  GREED (Mittlere Priorität):                                      │
│  ├── Spieler will Item für Transmog/Verkauf/Alt                   │
│  ├── Keine Class-Restriction                                      │
│  └── Roll: 1-100                                                  │
│                                                                   │
│  PASS (Keine Teilnahme):                                          │
│  └── Spieler verzichtet auf Item                                  │
│                                                                   │
│  WINNER DETERMINATION:                                            │
│  1. Highest Need roll wins                                        │
│  2. If no Need: Highest Greed roll wins                           │
│  3. Tie-Breaker: Random selection                                 │
│  4. All Pass: Item stays in container (next opener)               │
│                                                                   │
│  TIMEOUT (30 Sekunden):                                           │
│  └── Nicht gewählt = automatisch PASS                             │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Round Robin

Bei Round Robin erhält automatisch der nächste berechtigte Spieler in der Rotation das Item:

```csharp
public Guid GetNextRoundRobinLooter(Party party, LootContainer container)
{
    // Get eligible members (alive, in range, not full inventory)
    var eligible = party.Members
        .Where(m => m.IsAlive && IsInRange(m, container) && !m.IsInventoryFull)
        .ToList();
    
    // Get current rotation index
    var currentIndex = party.RoundRobinIndex;
    
    // Find next eligible
    for (int i = 0; i < eligible.Count; i++)
    {
        var index = (currentIndex + i) % eligible.Count;
        var member = eligible[index];
        
        party.RoundRobinIndex = (index + 1) % eligible.Count;
        return member.CharacterId;
    }
    
    // Fallback: Return to container owner
    return container.OwnerId;
}
```

### Master Looter

Master Looter ist der Party/Raid-Leader oder ein designierter Spieler. Nur er sieht Items über dem Threshold und kann sie zuweisen.

```csharp
public bool IsMasterLooter(Guid playerId, Party party)
{
    return party.LootMethod == LootMethod.MasterLooter 
        && (party.LeaderId == playerId || party.MasterLooterId == playerId);
}
```

### Personal Loot

Bei Personal Loot generiert der Server für jeden berechtigten Spieler separaten Loot:

```csharp
public Dictionary<Guid, LootContainer> GeneratePersonalLoot(
    IEntity source,
    IReadOnlyList<DamageContribution> contributors)
{
    var personalLoot = new Dictionary<Guid, LootContainer>();
    
    foreach (var contributor in contributors)
    {
        if (contributor.DamagePercent < 0.05f) continue; // Min 5% contribution
        
        var container = new LootContainer
        {
            ContainerId = Guid.NewGuid(),
            SourceType = LootSourceType.PersonalDrop,
            OwnerId = contributor.PlayerId,
            // ... generate loot with personal modifiers
        };
        
        personalLoot[contributor.PlayerId] = container;
    }
    
    return personalLoot;
}
```

---

## 📦 Loot Window & Pickup

### Loot Window Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                   LOOT WINDOW LIFECYCLE                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  1. SPAWN                                                         │
│     Entity stirbt → LootableSpawn (1430) → Sparkle-Effect         │
│                                                                   │
│  2. OPEN                                                          │
│     Spieler interagiert → Server validiert Ownership              │
│     → LootWindowOpen (3100) mit Container-Snapshot                │
│                                                                   │
│  3. ACTIVE                                                        │
│     Spieler kann Items/Gold looten                                │
│     Container-Updates via LootWindowOpen (refresh)                │
│                                                                   │
│  4. CLOSE (explicit)                                              │
│     LootWindowClose (3101) → Server cleanup                       │
│     → LootWindowCloseResponse (3106)                              │
│                                                                   │
│  4b. CLOSE (implicit)                                             │
│     - Distance > MAX_LOOT_DISTANCE (10 Einheiten)                 │
│     - Container empty                                             │
│     - Container despawned                                         │
│     - Player disconnected                                         │
│                                                                   │
│  5. DESPAWN                                                       │
│     Timer abgelaufen (5 Minuten) ODER alle Items gelooted         │
│     → LootableDespawn (1431)                                      │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Claim/Pickup Atomicity

Jede Loot-Operation ist atomar und wird in einer Transaktion ausgeführt:

```csharp
public async Task<LootItemResult> ClaimItemAsync(
    Guid playerId, 
    Guid containerId, 
    byte slotIndex)
{
    await using var transaction = await _db.BeginTransactionAsync();
    
    try
    {
        // 1. Lock container row
        var container = await _lootRepository.GetForUpdateAsync(containerId);
        if (container == null)
            return new LootItemResult { ErrorCode = LootErrorCode.ContainerNotFound };
        
        // 2. Validate ownership
        if (!IsEligibleLooter(playerId, container))
            return new LootItemResult { ErrorCode = LootErrorCode.NotEligible };
        
        // 3. Get item
        var item = container.Items.FirstOrDefault(i => i.SlotIndex == slotIndex);
        if (item == null)
            return new LootItemResult { ErrorCode = LootErrorCode.ItemNotFound };
        
        if (item.IsLocked && item.LockedForPlayerId != playerId)
            return new LootItemResult { ErrorCode = LootErrorCode.ItemLocked };
        
        // 4. Check inventory space
        var inventory = await _inventoryRepository.GetForUpdateAsync(playerId);
        if (!inventory.HasSpace(item.ItemId, item.Quantity))
            return new LootItemResult { ErrorCode = LootErrorCode.InventoryFull };
        
        // 5. ATOMIC: Remove from container + Add to inventory
        container.Items.Remove(item);
        inventory.AddItem(item.ItemId, item.Quantity);
        
        // 6. Persist both
        await _lootRepository.UpdateAsync(container);
        await _inventoryRepository.UpdateAsync(inventory);
        
        await transaction.CommitAsync();
        
        return new LootItemResult 
        { 
            Success = true,
            ItemId = item.ItemId,
            Quantity = item.Quantity
        };
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## ⏳ Despawn, Locking & Anti-Dupe

### Despawn Timer

| Source Type | Despawn Timer | Notes |
|-------------|---------------|-------|
| EntityKill | 5 Minuten | Nach letztem Zugriff |
| Chest | 3 Minuten | Nach Öffnen |
| Fishing | 30 Sekunden | Sofort sichtbar |
| Gathering | 10 Sekunden | Nach Abschluss |
| QuestReward | Unbegrenzt | Bis akzeptiert |

### Anti-Dupe Mechanismen

```
┌─────────────────────────────────────────────────────────────────┐
│                    ANTI-DUPE PROTECTION                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  1. SERVER-SIDE VALIDATION                                        │
│     - Client sendet nur ContainerId + SlotIndex                   │
│     - Server bestimmt ItemId und Quantity                         │
│     - Client kann NIEMALS Item-Details überschreiben              │
│                                                                   │
│  2. ATOMIC TRANSACTIONS                                           │
│     - Remove from Container + Add to Inventory in einer TX        │
│     - Bei Fehler: Rollback auf beiden Seiten                      │
│                                                                   │
│  3. IDEMPOTENCY                                                   │
│     - Doppelte Claim-Requests werden abgelehnt                    │
│     - Item bereits gelooted → ITEM_ALREADY_LOOTED Error           │
│                                                                   │
│  4. CONTAINER LOCKING                                             │
│     - Container wird während Claim gelockt                        │
│     - Concurrent Claims auf gleichen Slot → nur einer erfolgreich │
│                                                                   │
│  5. SEQUENCE NUMBERS                                              │
│     - Jede Loot-Operation hat eine Sequence Number                │
│     - Replay-Attacks werden erkannt und abgelehnt                 │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🧱 DTOs / Interfaces

### ILootMessage Interface

```csharp
// Mmo.Shared/Messaging/Interfaces/ILootMessage.cs
public interface ILootMessage : INetworkMessage
{
    Guid ContainerId { get; }
}
```

### LootContainerDto

```csharp
[MessagePackObject]
public class LootContainerDto
{
    [Key(0)] public Guid ContainerId { get; set; }
    [Key(1)] public LootSourceType SourceType { get; set; }
    [Key(2)] public Guid SourceEntityId { get; set; }
    [Key(3)] public Guid OwnerId { get; set; }
    [Key(4)] public Guid? PartyId { get; set; }
    [Key(5)] public List<LootItemEntryDto> Items { get; set; } = new();
    [Key(6)] public long GoldCopper { get; set; }
    [Key(7)] public long CreatedAtTicks { get; set; }
    [Key(8)] public long ExpiresAtTicks { get; set; }
    [Key(9)] public bool IsEmpty { get; set; }
}
```

### LootItemEntryDto

```csharp
[MessagePackObject]
public class LootItemEntryDto
{
    [Key(0)] public byte SlotIndex { get; set; }
    [Key(1)] public uint ItemId { get; set; }
    [Key(2)] public int Quantity { get; set; }
    [Key(3)] public ItemQuality Quality { get; set; }
    [Key(4)] public bool IsLocked { get; set; }
    [Key(5)] public Guid? LockedForPlayerId { get; set; }
    [Key(6)] public bool RequiresRoll { get; set; }
    [Key(7)] public Guid? RollId { get; set; }
}
```

### LootRollDto

```csharp
[MessagePackObject]
public class LootRollDto
{
    [Key(0)] public Guid RollId { get; set; }
    [Key(1)] public Guid ContainerId { get; set; }
    [Key(2)] public byte SlotIndex { get; set; }
    [Key(3)] public uint ItemId { get; set; }
    [Key(4)] public ItemQuality Quality { get; set; }
    [Key(5)] public int RollDurationSeconds { get; set; }
    [Key(6)] public long StartedAtTicks { get; set; }
    [Key(7)] public List<LootRollVoteDto> Votes { get; set; } = new();
}

[MessagePackObject]
public class LootRollVoteDto
{
    [Key(0)] public Guid PlayerId { get; set; }
    [Key(1)] public string PlayerName { get; set; } = "";
    [Key(2)] public RollType RollType { get; set; }
    [Key(3)] public int? RollValue { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### LootErrorCode Enum

```csharp
public enum LootErrorCode : byte
{
    None = 0,
    
    // Container Errors
    ContainerNotFound = 1,
    ContainerExpired = 2,
    ContainerEmpty = 3,
    
    // Eligibility Errors
    NotEligible = 10,
    NotInRange = 11,
    NotOwner = 12,
    NotInParty = 13,
    
    // Item Errors
    ItemNotFound = 20,
    ItemLocked = 21,
    ItemAlreadyLooted = 22,
    ItemRequiresRoll = 23,
    
    // Inventory Errors
    InventoryFull = 30,
    UniqueItemAlreadyOwned = 31,
    
    // Roll Errors
    RollNotActive = 40,
    RollAlreadyVoted = 41,
    RollExpired = 42,
    CannotNeedItem = 43,
    
    // Permission Errors
    NotMasterLooter = 50,
    NotPartyLeader = 51,
    
    // General
    RateLimited = 90,
    InternalError = 99
}
```

### LootMethod Enum

```csharp
public enum LootMethod : byte
{
    FreeForAll = 0,
    RoundRobin = 1,
    GroupLoot = 2,
    MasterLooter = 3,
    PersonalLoot = 4
}
```

### ItemQualityThreshold Enum

```csharp
public enum ItemQualityThreshold : byte
{
    Uncommon = 2,   // Green+
    Rare = 3,       // Blue+
    Epic = 4,       // Purple+
    Legendary = 5   // Orange only
}
```

### RollType Enum

```csharp
public enum RollType : byte
{
    Pass = 0,
    Greed = 1,
    Need = 2,
    Disenchant = 3
}
```

---

## ⚙️ Regeln & Validierung

### Server-Autorität

| Aktion | Client darf | Server entscheidet |
|--------|-------------|-------------------|
| Drop-Inhalt | ❌ Niemals | ✅ Immer |
| Roll-Ergebnis | ❌ Niemals | ✅ Immer |
| Item-Zuweisung | ❌ Niemals | ✅ Immer |
| Window öffnen | ✅ Request | ✅ Validiert |
| Item claimen | ✅ Request | ✅ Validiert |

### Rate Limiting

| Aktion | Rate Limit | Cooldown |
|--------|------------|----------|
| LootItem | 10/Sekunde | - |
| LootAll | 1/Sekunde | - |
| LootWindowOpen | 5/Sekunde | - |
| LootRollVote | 1/Roll | 30s |

### Security

```
┌─────────────────────────────────────────────────────────────────┐
│                    SECURITY CONSIDERATIONS                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  CLIENT CANNOT:                                                   │
│  ├── Specify ItemId or Quantity (only SlotIndex)                  │
│  ├── Override Ownership rules                                     │
│  ├── Bypass distance checks                                       │
│  ├── Modify roll results                                          │
│  └── Access locked items without permission                       │
│                                                                   │
│  SERVER VALIDATES:                                                │
│  ├── PlayerId from Session (not from message)                     │
│  ├── Container exists and not expired                             │
│  ├── Player is eligible looter                                    │
│  ├── Player is within MAX_LOOT_DISTANCE (10 units)                │
│  ├── Item slot exists and not already looted                      │
│  ├── Item not locked for another player                           │
│  └── Player has inventory space                                   │
│                                                                   │
│  LOGGING:                                                         │
│  ├── All loot operations logged with PlayerId, ItemId, Quantity   │
│  ├── Suspicious patterns flagged (too fast, too much)             │
│  └── GM tools can review loot history                             │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📩 Aktive Messages (3100-3142)

---

## LootWindowOpen (3100)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server öffnet Loot-Window für den Spieler. Enthält vollständigen Container-Snapshot mit allen lootvaren Items und Gold.

### Im Scope ✅

- Container-Snapshot beim Öffnen
- Item-Liste mit Qualität und Quantity
- Gold-Betrag
- Locking-Status pro Item
- Roll-Status für Group-Loot Items

### Nicht im Scope ❌

- Loot-Generierung → Server-intern bei Entity Death
- Item-Detaildaten → verwende `ItemTooltipRequest` (533)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootWindowOpen` | Ja |
| Container | LootContainerDto | Vollständiger Container-Snapshot | Ja |

### Erwartete Response

- Keine (Server-initiiert)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootWindowOpen)]
public class LootWindowOpen : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LootWindowOpen;
    [Key(1)] public required LootContainerDto Container { get; init; }
}
```

### Server-Verhalten

1. Validate Ownership (Player ist berechtigt)
2. Validate Distance (MAX_LOOT_DISTANCE = 10 Einheiten)
3. Create Container-Snapshot
4. Track Open Window (für implicit close detection)
5. Send LootWindowOpen

### Client-Verhalten

1. Receive Container-Snapshot
2. Open Loot UI
3. Display Items mit Qualität-Farben
4. Display Gold-Betrag
5. Mark locked/rolling Items

### Flow-Diagramm

```
Client                    Server
  │                          │
  │  [Interact with Lootable]│
  │─────────────────────────►│
  │                          │  ┌─────────────────┐
  │                          │  │ Validate:       │
  │                          │  │ - Ownership     │
  │                          │  │ - Distance      │
  │                          │  │ - Not expired   │
  │                          │  └─────────────────┘
  │                          │
  │  LootWindowOpen (3100)   │
  │◄─────────────────────────│
  │                          │
  │  [Display Loot UI]       │
```

### Beispiel Payload

```csharp
var msg = new LootWindowOpen
{
    Container = new LootContainerDto
    {
        ContainerId = Guid.Parse("a1b2c3d4-..."),
        SourceType = LootSourceType.EntityKill,
        SourceEntityId = killedMobId,
        OwnerId = playerId,
        PartyId = partyId,
        Items = new List<LootItemEntryDto>
        {
            new() { SlotIndex = 0, ItemId = 1001, Quantity = 1, Quality = ItemQuality.Rare },
            new() { SlotIndex = 1, ItemId = 5020, Quantity = 5, Quality = ItemQuality.Common }
        },
        GoldCopper = 15000, // 1 Gold 50 Silver
        CreatedAtTicks = DateTime.UtcNow.Ticks,
        ExpiresAtTicks = DateTime.UtcNow.AddMinutes(5).Ticks
    }
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootWindowClose` | 3101 | Client schließt Window |
| `LootItem` | 3102 | Client lootet Item |
| `LootableSpawn` | 1430 | Lootable erscheint |

### Notizen

- Container-Snapshot ist zum Zeitpunkt des Öffnens aktuell
- Bei Änderungen (anderer Spieler lootet) wird erneut LootWindowOpen gesendet
- Implicit Close bei Distanz > 10 Einheiten

---

## LootWindowClose (3101)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client informiert Server dass Loot-Window geschlossen wurde.

### Im Scope ✅

- Explicit Window Close
- Server Cleanup

### Nicht im Scope ❌

- Implicit Close (Distance, Disconnect) → Server handled intern

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootWindowClose` | Ja |
| ContainerId | Guid | Container-ID | Ja |

### Erwartete Response

- `LootWindowCloseResponse` (3106)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootWindowClose)]
public class LootWindowClose : IClientMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootWindowClose;
    [Key(1)] public Guid ContainerId { get; set; }
}
```

### Server-Verhalten

1. Validate ContainerId
2. Remove Player from Open Window Tracking
3. Send LootWindowCloseResponse

### Client-Verhalten

1. Close UI
2. Send LootWindowClose
3. Wait for Response

### Beispiel Payload

```csharp
var msg = new LootWindowClose
{
    ContainerId = containerGuid
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootWindowOpen` | 3100 | Öffnen |
| `LootWindowCloseResponse` | 3106 | Response |

---

## LootItem (3102)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client versucht ein einzelnes Item aus dem Loot-Container zu looten.

### Im Scope ✅

- Single Item Loot Request
- Slot-based (Client sendet NUR SlotIndex)

### Nicht im Scope ❌

- Item-ID/Quantity Spezifikation → Server bestimmt
- Loot All → verwende `LootAll` (3105)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootItem` | Ja |
| ContainerId | Guid | Container-ID | Ja |
| SlotIndex | byte | Slot im Container | Ja |

### Erwartete Response

- `LootItemResult` (3103)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootItem)]
public class LootItem : IClientMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootItem;
    [Key(1)] public Guid ContainerId { get; set; }
    [Key(2)] public byte SlotIndex { get; set; }
}
```

### Server-Verhalten

1. Validate ContainerId exists
2. Validate Player Eligibility
3. Validate SlotIndex exists
4. Check Item not locked for other player
5. Check Item not requires roll
6. Check Inventory Space
7. ATOMIC: Remove from Container + Add to Inventory
8. Send LootItemResult
9. Send InventorySlotUpdate
10. If container empty → despawn

### Client-Verhalten

1. User clicks item in Loot UI
2. Send LootItem Request
3. Wait for LootItemResult
4. Update UI based on result

### Flow-Diagramm

```
Client                    Server
  │                          │
  │  LootItem (3102)         │
  │  SlotIndex: 0            │
  │─────────────────────────►│
  │                          │  ┌─────────────────┐
  │                          │  │ ATOMIC TX:      │
  │                          │  │ 1. Remove Item  │
  │                          │  │ 2. Add to Inv   │
  │                          │  │ 3. Persist      │
  │                          │  └─────────────────┘
  │                          │
  │  LootItemResult (3103)   │
  │  Success: true           │
  │◄─────────────────────────│
  │                          │
  │  InventorySlotUpdate(501)│
  │◄─────────────────────────│
```

### Beispiel Payload

```csharp
var msg = new LootItem
{
    ContainerId = containerGuid,
    SlotIndex = 0
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `CONTAINER_NOT_FOUND` | Container existiert nicht | Refresh UI |
| `NOT_ELIGIBLE` | Kein Loot-Recht | Zeige Fehler |
| `ITEM_NOT_FOUND` | Slot leer | Refresh Window |
| `ITEM_LOCKED` | Für anderen Spieler gelockt | Warte auf Roll |
| `ITEM_REQUIRES_ROLL` | Muss gewürfelt werden | Zeige Roll UI |
| `INVENTORY_FULL` | Kein Platz | Zeige Inventory Full |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootItemResult` | 3103 | Response |
| `InventorySlotUpdate` | 501 | Item in Inventory |
| `LootAll` | 3105 | Alle Items looten |

### Notizen

- Client sendet NUR ContainerId + SlotIndex
- Server bestimmt ItemId und Quantity aus Container
- Anti-Dupe: Doppelte Requests werden als ITEM_NOT_FOUND abgelehnt

---

## LootItemResult (3103)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server antwortet auf LootItem Request mit Erfolg oder Fehler.

### Im Scope ✅

- Success/Failure Status
- Gelooted Item Details (bei Erfolg)
- Error Code (bei Fehler)

### Nicht im Scope ❌

- Inventory Update → verwende `InventorySlotUpdate` (501)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootItemResult` | Ja |
| ContainerId | Guid | Container-ID | Ja |
| Success | bool | Loot erfolgreich? | Ja |
| SlotIndex | byte | Betroffener Slot | Ja |
| ItemId | uint | Item-ID | Bei Erfolg |
| Quantity | int | Erhaltene Menge | Bei Erfolg |
| ErrorCode | LootErrorCode | Fehlercode | Bei Fehler |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootItemResult)]
public class LootItemResult : IServerMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootItemResult;
    [Key(1)] public Guid ContainerId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public byte SlotIndex { get; set; }
    [Key(4)] public uint ItemId { get; set; }
    [Key(5)] public int Quantity { get; set; }
    [Key(6)] public LootErrorCode ErrorCode { get; set; }
}
```

### Beispiel Payloads

```csharp
// Erfolg
var success = new LootItemResult
{
    ContainerId = containerGuid,
    Success = true,
    SlotIndex = 0,
    ItemId = 1001,
    Quantity = 1,
    ErrorCode = LootErrorCode.None
};

// Fehler
var error = new LootItemResult
{
    ContainerId = containerGuid,
    Success = false,
    SlotIndex = 0,
    ErrorCode = LootErrorCode.InventoryFull
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootItem` | 3102 | Request |
| `InventorySlotUpdate` | 501 | Folgt bei Erfolg |

---

## LootGold (3104)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client lootet Gold aus dem Container.

### Im Scope ✅

- Gold Loot Request
- Split für Party (automatisch bei Group)

### Nicht im Scope ❌

- Item Loot → verwende `LootItem` (3102)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootGold` | Ja |
| ContainerId | Guid | Container-ID | Ja |

### Erwartete Response

- `LootGoldResult` (3107)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootGold)]
public class LootGold : IClientMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootGold;
    [Key(1)] public Guid ContainerId { get; set; }
}
```

### Server-Verhalten

1. Validate Container
2. Validate Eligibility
3. Calculate Gold Split (Party/Raid)
4. Add Gold to Player(s)
5. Remove Gold from Container
6. Send LootGoldResult
7. Send GoldUpdate to all recipients

### Beispiel Payload

```csharp
var msg = new LootGold
{
    ContainerId = containerGuid
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootGoldResult` | 3107 | Response |
| `GoldUpdate` | 3703 | Gold Balance Update |

---

## LootAll (3105)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client lootet alle verfügbaren Items und Gold aus dem Container.

### Im Scope ✅

- Batch Loot aller unlocked Items
- Batch Gold Loot
- Skipped Items (locked, requires roll)

### Nicht im Scope ❌

- Forciertes Looten gelockter Items → nicht möglich

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootAll` | Ja |
| ContainerId | Guid | Container-ID | Ja |

### Erwartete Response

- `LootAllResult` (3108)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootAll)]
public class LootAll : IClientMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootAll;
    [Key(1)] public Guid ContainerId { get; set; }
}
```

### Server-Verhalten

1. Validate Container
2. Validate Eligibility
3. For each unlocked item:
   - Check inventory space
   - Add to inventory
   - Track looted items
4. Loot Gold (with split)
5. Send LootAllResult
6. Send InventorySlotUpdate for each item
7. Send GoldUpdate

### Beispiel Payload

```csharp
var msg = new LootAll
{
    ContainerId = containerGuid
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootAllResult` | 3108 | Response |
| `InventorySlotUpdate` | 501 | Pro Item |
| `GoldUpdate` | 3703 | Gold Balance |

---

## LootWindowCloseResponse (3106)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt das Schließen des Loot-Windows.

### Im Scope ✅

- Close Confirmation
- Cleanup Status

### Nicht im Scope ❌

- Container Status nach Close → verwende LootWindowOpen für neues Öffnen

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootWindowCloseResponse` | Ja |
| ContainerId | Guid | Container-ID | Ja |
| Success | bool | Close erfolgreich? | Ja |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootWindowCloseResponse)]
public class LootWindowCloseResponse : IServerMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootWindowCloseResponse;
    [Key(1)] public Guid ContainerId { get; set; }
    [Key(2)] public bool Success { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootWindowClose` | 3101 | Request |

---

## LootGoldResult (3107)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server antwortet auf LootGold Request.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootGoldResult` | Ja |
| ContainerId | Guid | Container-ID | Ja |
| Success | bool | Loot erfolgreich? | Ja |
| GoldLootedCopper | long | Erhaltenes Gold (in Copper) | Bei Erfolg |
| ErrorCode | LootErrorCode | Fehlercode | Bei Fehler |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootGoldResult)]
public class LootGoldResult : IServerMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootGoldResult;
    [Key(1)] public Guid ContainerId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public long GoldLootedCopper { get; set; }
    [Key(4)] public LootErrorCode ErrorCode { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootGold` | 3104 | Request |
| `GoldUpdate` | 3703 | Balance Update |

---

## LootAllResult (3108)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server antwortet auf LootAll Request mit Zusammenfassung.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootAllResult` | Ja |
| ContainerId | Guid | Container-ID | Ja |
| Success | bool | Mindestens ein Item gelooted? | Ja |
| ItemsLooted | int | Anzahl gelooteter Items | Ja |
| GoldLootedCopper | long | Erhaltenes Gold | Ja |
| SkippedSlots | List<byte> | Übersprungene Slots (locked/roll) | Ja |
| ErrorCode | LootErrorCode | Fehlercode falls komplett fehlgeschlagen | Bei Fehler |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootAllResult)]
public class LootAllResult : IServerMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootAllResult;
    [Key(1)] public Guid ContainerId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public int ItemsLooted { get; set; }
    [Key(4)] public long GoldLootedCopper { get; set; }
    [Key(5)] public List<byte> SkippedSlots { get; set; } = new();
    [Key(6)] public LootErrorCode ErrorCode { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootAll` | 3105 | Request |

---

## LootRollStart (3110)

**Richtung:** 📡 Broadcast (Server → Party/Raid)  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung

Server startet einen Loot-Roll für ein Item das über dem Quality-Threshold liegt.

### Im Scope ✅

- Roll-Start Broadcast an alle berechtigten Spieler
- Item-Information
- Roll-Timer (30 Sekunden)

### Nicht im Scope ❌

- Roll-Ergebnis → verwende `LootRollWinner` (3115)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRollStart` | Ja |
| Roll | LootRollDto | Roll-Informationen | Ja |

### Erwartete Response

- `LootRollNeed` (3111), `LootRollGreed` (3112), oder `LootRollPass` (3113)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRollStart)]
public class LootRollStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRollStart;
    [Key(1)] public required LootRollDto Roll { get; init; }
}
```

### Beispiel Payload

```csharp
var msg = new LootRollStart
{
    Roll = new LootRollDto
    {
        RollId = Guid.NewGuid(),
        ContainerId = containerGuid,
        SlotIndex = 0,
        ItemId = 1001,
        Quality = ItemQuality.Rare,
        RollDurationSeconds = 30,
        StartedAtTicks = DateTime.UtcNow.Ticks
    }
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRollNeed` | 3111 | Spieler will Item |
| `LootRollGreed` | 3112 | Spieler will verkaufen |
| `LootRollPass` | 3113 | Spieler passt |
| `LootRollResult` | 3114 | Einzelne Vote-Ergebnisse |
| `LootRollWinner` | 3115 | Finales Ergebnis |

---

## LootRollNeed (3111)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Spieler wählt "Need" für einen aktiven Loot-Roll.

### Im Scope ✅

- Need Vote
- Class-Restriction Check

### Nicht im Scope ❌

- Roll-Ergebnis → Server berechnet nach Timer/All Voted

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRollNeed` | Ja |
| RollId | Guid | Roll-ID | Ja |

### Erwartete Response

- `LootRollVoteResponse` (3116)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRollNeed)]
public class LootRollNeed : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRollNeed;
    [Key(1)] public Guid RollId { get; set; }
}
```

### Server-Verhalten

1. Validate Roll active
2. Validate Player eligible
3. Validate Player can Need (class restriction)
4. Generate Roll Value (1-100)
5. Record Vote
6. Broadcast LootRollResult
7. Send LootRollVoteResponse

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `ROLL_NOT_ACTIVE` | Roll nicht aktiv |
| `ROLL_ALREADY_VOTED` | Bereits gewählt |
| `CANNOT_NEED_ITEM` | Class kann Item nicht nutzen |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRollStart` | 3110 | Roll-Start |
| `LootRollVoteResponse` | 3116 | Response |
| `LootRollResult` | 3114 | Broadcast |

---

## LootRollGreed (3112)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Spieler wählt "Greed" für einen aktiven Loot-Roll.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRollGreed` | Ja |
| RollId | Guid | Roll-ID | Ja |

### Erwartete Response

- `LootRollVoteResponse` (3116)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRollGreed)]
public class LootRollGreed : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRollGreed;
    [Key(1)] public Guid RollId { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRollStart` | 3110 | Roll-Start |
| `LootRollVoteResponse` | 3116 | Response |

---

## LootRollPass (3113)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Spieler verzichtet auf Item (Pass).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRollPass` | Ja |
| RollId | Guid | Roll-ID | Ja |

### Erwartete Response

- `LootRollVoteResponse` (3116)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRollPass)]
public class LootRollPass : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRollPass;
    [Key(1)] public Guid RollId { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRollStart` | 3110 | Roll-Start |
| `LootRollVoteResponse` | 3116 | Response |

---

## LootRollResult (3114)

**Richtung:** 📡 Broadcast (Server → Party/Raid)  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung

Server broadcasted dass ein Spieler gewählt hat (Need/Greed/Pass).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRollResult` | Ja |
| RollId | Guid | Roll-ID | Ja |
| PlayerId | Guid | Spieler der gewählt hat | Ja |
| PlayerName | string | Spielername | Ja |
| RollType | RollType | Need/Greed/Pass | Ja |
| RollValue | int? | Gewürfelter Wert (null bei Pass) | Nein |

### Erwartete Response

- Keine (Broadcast)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRollResult)]
public class LootRollResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRollResult;
    [Key(1)] public Guid RollId { get; set; }
    [Key(2)] public Guid PlayerId { get; set; }
    [Key(3)] public string PlayerName { get; set; } = "";
    [Key(4)] public RollType RollType { get; set; }
    [Key(5)] public int? RollValue { get; set; }
}
```

### Beispiel Payload

```csharp
var msg = new LootRollResult
{
    RollId = rollGuid,
    PlayerId = playerGuid,
    PlayerName = "Aragorn",
    RollType = RollType.Need,
    RollValue = 87
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRollStart` | 3110 | Roll-Start |
| `LootRollWinner` | 3115 | Finales Ergebnis |

---

## LootRollWinner (3115)

**Richtung:** 📡 Broadcast (Server → Party/Raid)  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung

Server verkündet den Gewinner eines Loot-Rolls. Gewinner erhält Item automatisch.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRollWinner` | Ja |
| RollId | Guid | Roll-ID | Ja |
| WinnerId | Guid | Gewinner-ID | Ja |
| WinnerName | string | Gewinner-Name | Ja |
| WinningRollType | RollType | Need/Greed | Ja |
| WinningRollValue | int | Höchster Roll | Ja |
| ItemId | uint | Gewonnenes Item | Ja |

### Erwartete Response

- Keine (Broadcast)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRollWinner)]
public class LootRollWinner : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRollWinner;
    [Key(1)] public Guid RollId { get; set; }
    [Key(2)] public Guid WinnerId { get; set; }
    [Key(3)] public string WinnerName { get; set; } = "";
    [Key(4)] public RollType WinningRollType { get; set; }
    [Key(5)] public int WinningRollValue { get; set; }
    [Key(6)] public uint ItemId { get; set; }
}
```

### Server-Verhalten

1. Timer abgelaufen ODER alle haben gewählt
2. Evaluate Winner (Need > Greed > Tie-Breaker)
3. Add Item to Winner Inventory
4. Remove Item from Container
5. Broadcast LootRollWinner
6. Send InventorySlotUpdate to Winner

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRollResult` | 3114 | Einzelne Votes |
| `InventorySlotUpdate` | 501 | Item an Gewinner |

---

## LootRollVoteResponse (3116)

**Richtung:** 📥 Server → Client  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt die Vote-Abgabe des Spielers.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRollVoteResponse` | Ja |
| RollId | Guid | Roll-ID | Ja |
| Success | bool | Vote akzeptiert? | Ja |
| RollType | RollType | Registrierte Vote | Bei Erfolg |
| RollValue | int? | Gewürfelter Wert | Bei Erfolg |
| ErrorCode | LootErrorCode | Fehlercode | Bei Fehler |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRollVoteResponse)]
public class LootRollVoteResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRollVoteResponse;
    [Key(1)] public Guid RollId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public RollType RollType { get; set; }
    [Key(4)] public int? RollValue { get; set; }
    [Key(5)] public LootErrorCode ErrorCode { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRollNeed` | 3111 | Request |
| `LootRollGreed` | 3112 | Request |
| `LootRollPass` | 3113 | Request |

---

## LootMasterAssign (3120)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Master Looter

### Beschreibung

Master Looter weist ein Item einem Spieler zu.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootMasterAssign` | Ja |
| ContainerId | Guid | Container-ID | Ja |
| SlotIndex | byte | Item-Slot | Ja |
| TargetPlayerId | Guid | Empfänger | Ja |

### Erwartete Response

- `LootMasterAssignResult` (3123)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootMasterAssign)]
public class LootMasterAssign : IClientMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootMasterAssign;
    [Key(1)] public Guid ContainerId { get; set; }
    [Key(2)] public byte SlotIndex { get; set; }
    [Key(3)] public Guid TargetPlayerId { get; set; }
}
```

### Server-Verhalten

1. Validate Player is Master Looter
2. Validate Target is in Party/Raid
3. Validate Target has inventory space
4. Add Item to Target Inventory
5. Remove Item from Container
6. Send LootMasterAssignResult
7. Send InventorySlotUpdate to Target

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `NOT_MASTER_LOOTER` | Kein Master Looter |
| `NOT_IN_PARTY` | Target nicht in Gruppe |
| `INVENTORY_FULL` | Target Inventory voll |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootMasterAssignResult` | 3123 | Response |
| `InventorySlotUpdate` | 501 | Item an Target |

---

## LootRulesChange (3121)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung

Party-Leader ändert die Loot-Methode.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRulesChange` | Ja |
| NewLootMethod | LootMethod | Neue Loot-Methode | Ja |
| MasterLooterId | Guid? | Master Looter (wenn MasterLooter) | Nein |

### Erwartete Response

- `LootRulesChangeResult` (3124)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRulesChange)]
public class LootRulesChange : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRulesChange;
    [Key(1)] public LootMethod NewLootMethod { get; set; }
    [Key(2)] public Guid? MasterLooterId { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRulesChangeResult` | 3124 | Response |
| `PartyLootChange` | 707 | Party Update |

---

## LootThresholdChange (3122)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung

Party-Leader ändert den Quality-Threshold für Group Loot.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootThresholdChange` | Ja |
| NewThreshold | ItemQualityThreshold | Neuer Threshold | Ja |

### Erwartete Response

- `LootThresholdChangeResult` (3125)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootThresholdChange)]
public class LootThresholdChange : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.LootThresholdChange;
    [Key(1)] public ItemQualityThreshold NewThreshold { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootThresholdChangeResult` | 3125 | Response |

---

## LootMasterAssignResult (3123)

**Richtung:** 📥 Server → Client  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server antwortet auf LootMasterAssign.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootMasterAssignResult` | Ja |
| ContainerId | Guid | Container-ID | Ja |
| Success | bool | Zuweisung erfolgreich? | Ja |
| SlotIndex | byte | Zugewiesener Slot | Ja |
| TargetPlayerId | Guid | Empfänger | Ja |
| ErrorCode | LootErrorCode | Fehlercode | Bei Fehler |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootMasterAssignResult)]
public class LootMasterAssignResult : IServerMessage, ILootMessage
{
    [Key(0)] public MessageType Type => MessageType.LootMasterAssignResult;
    [Key(1)] public Guid ContainerId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public byte SlotIndex { get; set; }
    [Key(4)] public Guid TargetPlayerId { get; set; }
    [Key(5)] public LootErrorCode ErrorCode { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootMasterAssign` | 3120 | Request |

---

## LootRulesChangeResult (3124)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server antwortet auf LootRulesChange.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootRulesChangeResult` | Ja |
| Success | bool | Änderung erfolgreich? | Ja |
| NewLootMethod | LootMethod | Neue aktive Methode | Bei Erfolg |
| ErrorCode | LootErrorCode | Fehlercode | Bei Fehler |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootRulesChangeResult)]
public class LootRulesChangeResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LootRulesChangeResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public LootMethod NewLootMethod { get; set; }
    [Key(3)] public LootErrorCode ErrorCode { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootRulesChange` | 3121 | Request |

---

## LootThresholdChangeResult (3125)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** �� Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server antwortet auf LootThresholdChange.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LootThresholdChangeResult` | Ja |
| Success | bool | Änderung erfolgreich? | Ja |
| NewThreshold | ItemQualityThreshold | Neuer Threshold | Bei Erfolg |
| ErrorCode | LootErrorCode | Fehlercode | Bei Fehler |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LootThresholdChangeResult)]
public class LootThresholdChangeResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LootThresholdChangeResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public ItemQualityThreshold NewThreshold { get; set; }
    [Key(3)] public LootErrorCode ErrorCode { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LootThresholdChange` | 3122 | Request |

---

## PersonalLoot (3130)

**Richtung:** 📥 Server → Client  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung

Server informiert Spieler über personal Loot Drop. Bei Personal Loot Mode erhält jeder Spieler separaten, individuellen Loot.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PersonalLoot` | Ja |
| SourceEntityId | Guid | Kill-Source | Ja |
| ItemId | uint | Erhaltenes Item | Ja |
| Quantity | int | Menge | Ja |
| Quality | ItemQuality | Qualität | Ja |
| AddedToInventory | bool | Direkt in Inv? | Ja |

### Erwartete Response

- Keine (Server-initiiert)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PersonalLoot)]
public class PersonalLoot : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PersonalLoot;
    [Key(1)] public Guid SourceEntityId { get; set; }
    [Key(2)] public uint ItemId { get; set; }
    [Key(3)] public int Quantity { get; set; }
    [Key(4)] public ItemQuality Quality { get; set; }
    [Key(5)] public bool AddedToInventory { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `InventorySlotUpdate` | 501 | Item in Inventory |

---

## BonusRollPrompt (3131)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung

Server fragt Spieler ob er einen Bonus-Roll verwenden möchte (nach Boss-Kill).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BonusRollPrompt` | Ja |
| BossId | uint | Boss-ID | Ja |
| BossName | string | Boss-Name | Ja |
| CurrencyRequired | uint | Benötigte Währung-ID | Ja |
| CurrencyAmount | int | Benötigte Menge | Ja |
| TimeoutSeconds | int | Entscheidungs-Timer | Ja |

### Erwartete Response

- `BonusRollUse` (3132) oder Timeout

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BonusRollPrompt)]
public class BonusRollPrompt : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BonusRollPrompt;
    [Key(1)] public uint BossId { get; set; }
    [Key(2)] public string BossName { get; set; } = "";
    [Key(3)] public uint CurrencyRequired { get; set; }
    [Key(4)] public int CurrencyAmount { get; set; }
    [Key(5)] public int TimeoutSeconds { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `BonusRollUse` | 3132 | Spieler nutzt Roll |
| `BonusRollResult` | 3133 | Ergebnis |

---

## BonusRollUse (3132)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Spieler entscheidet ob er Bonus-Roll nutzen möchte.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BonusRollUse` | Ja |
| BossId | uint | Boss-ID | Ja |
| UseRoll | bool | Roll nutzen? | Ja |

### Erwartete Response

- `BonusRollResult` (3133)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BonusRollUse)]
public class BonusRollUse : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.BonusRollUse;
    [Key(1)] public uint BossId { get; set; }
    [Key(2)] public bool UseRoll { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `BonusRollPrompt` | 3131 | Prompt |
| `BonusRollResult` | 3133 | Response |

---

## BonusRollResult (3133)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server teilt Ergebnis des Bonus-Rolls mit.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BonusRollResult` | Ja |
| BossId | uint | Boss-ID | Ja |
| Success | bool | Item erhalten? | Ja |
| ItemId | uint | Erhaltenes Item | Bei Erfolg |
| ItemQuality | ItemQuality | Qualität | Bei Erfolg |
| GoldReceived | long | Gold (falls kein Item) | Bei Gold |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BonusRollResult)]
public class BonusRollResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BonusRollResult;
    [Key(1)] public uint BossId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public uint ItemId { get; set; }
    [Key(4)] public ItemQuality ItemQuality { get; set; }
    [Key(5)] public long GoldReceived { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `BonusRollUse` | 3132 | Request |
| `InventorySlotUpdate` | 501 | Item in Inventory |

---

## RewardChoicePrompt (3140)

**Richtung:** 📥 Server → Client  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung

Server bietet Spieler Auswahl zwischen mehreren Belohnungen (Quest, Boss, Event).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.RewardChoicePrompt` | Ja |
| PromptId | Guid | Prompt-ID | Ja |
| SourceType | RewardSourceType | Quest/Boss/Event | Ja |
| SourceId | uint | Source-ID | Ja |
| Choices | List\<RewardChoiceDto\> | Verfügbare Belohnungen | Ja |
| TimeoutSeconds | int | Entscheidungs-Timer | Ja |

### Erwartete Response

- `RewardChoiceSelect` (3141)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RewardChoicePrompt)]
public class RewardChoicePrompt : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.RewardChoicePrompt;
    [Key(1)] public Guid PromptId { get; set; }
    [Key(2)] public RewardSourceType SourceType { get; set; }
    [Key(3)] public uint SourceId { get; set; }
    [Key(4)] public List<RewardChoiceDto> Choices { get; set; } = new();
    [Key(5)] public int TimeoutSeconds { get; set; }
}

[MessagePackObject]
public class RewardChoiceDto
{
    [Key(0)] public byte ChoiceIndex { get; set; }
    [Key(1)] public uint ItemId { get; set; }
    [Key(2)] public int Quantity { get; set; }
    [Key(3)] public ItemQuality Quality { get; set; }
}

public enum RewardSourceType : byte
{
    Quest = 1,
    Boss = 2,
    Event = 3,
    Achievement = 4
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `RewardChoiceSelect` | 3141 | Spieler wählt |
| `RewardChoiceResult` | 3142 | Ergebnis |

---

## RewardChoiceSelect (3141)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Spieler wählt eine der angebotenen Belohnungen.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.RewardChoiceSelect` | Ja |
| PromptId | Guid | Prompt-ID | Ja |
| ChoiceIndex | byte | Gewählte Option | Ja |

### Erwartete Response

- `RewardChoiceResult` (3142)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RewardChoiceSelect)]
public class RewardChoiceSelect : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.RewardChoiceSelect;
    [Key(1)] public Guid PromptId { get; set; }
    [Key(2)] public byte ChoiceIndex { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `RewardChoicePrompt` | 3140 | Prompt |
| `RewardChoiceResult` | 3142 | Response |

---

## RewardChoiceResult (3142)

**Richtung:** 📥 Server → Client  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt die Belohnungswahl.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.RewardChoiceResult` | Ja |
| PromptId | Guid | Prompt-ID | Ja |
| Success | bool | Auswahl erfolgreich? | Ja |
| ChoiceIndex | byte | Gewählte Option | Ja |
| ItemId | uint | Erhaltenes Item | Bei Erfolg |
| Quantity | int | Erhaltene Menge | Bei Erfolg |
| ErrorCode | LootErrorCode | Fehlercode | Bei Fehler |

### Erwartete Response

- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RewardChoiceResult)]
public class RewardChoiceResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.RewardChoiceResult;
    [Key(1)] public Guid PromptId { get; set; }
    [Key(2)] public bool Success { get; set; }
    [Key(3)] public byte ChoiceIndex { get; set; }
    [Key(4)] public uint ItemId { get; set; }
    [Key(5)] public int Quantity { get; set; }
    [Key(6)] public LootErrorCode ErrorCode { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `RewardChoiceSelect` | 3141 | Request |
| `InventorySlotUpdate` | 501 | Item in Inventory |

---

## 🗑️ Obsolete Messages

Keine obsoleten Messages in dieser Kategorie.

---

## 🧨 Edge Cases & Fehlerfälle

### Inventory Full

Bei vollem Inventar wird der Loot-Versuch mit `INVENTORY_FULL` abgelehnt. Das Item verbleibt im Container und kann später erneut gelooted werden.

### Disconnect während Roll

Bei Disconnect während eines aktiven Rolls wird der Spieler als "Pass" gewertet. Falls der disconnected Spieler dennoch gewinnt, wird das Item per Mail zugestellt.

### Container Despawn

Nach 5 Minuten oder wenn alle Items gelooted wurden, despawned der Container. Offene Loot-Windows werden automatisch geschlossen. Aktive Rolls werden abgebrochen.

---

## 📎 Anhang

### MessageType Enum Updates

Die folgenden Enum-Einträge wurden für vollständige Request/Response-Abdeckung hinzugefügt:

```csharp
// LOOT / REWARDS (3100-3199) - NEU HINZUGEFÜGT
LootWindowCloseResponse = 3106,
LootGoldResult = 3107,
LootAllResult = 3108,
LootRollVoteResponse = 3116,
LootMasterAssignResult = 3123,
LootRulesChangeResult = 3124,
LootThresholdChangeResult = 3125,
RewardChoiceResult = 3142,
```

### Request/Response Paare

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| LootWindowClose | 3101 | LootWindowCloseResponse | 3106 |
| LootItem | 3102 | LootItemResult | 3103 |
| LootGold | 3104 | LootGoldResult | 3107 |
| LootAll | 3105 | LootAllResult | 3108 |
| LootRollNeed | 3111 | LootRollVoteResponse | 3116 |
| LootRollGreed | 3112 | LootRollVoteResponse | 3116 |
| LootRollPass | 3113 | LootRollVoteResponse | 3116 |
| LootMasterAssign | 3120 | LootMasterAssignResult | 3123 |
| LootRulesChange | 3121 | LootRulesChangeResult | 3124 |
| LootThresholdChange | 3122 | LootThresholdChangeResult | 3125 |
| BonusRollUse | 3132 | BonusRollResult | 3133 |
| RewardChoiceSelect | 3141 | RewardChoiceResult | 3142 |

### Integrationshinweise

- **Entity-System**: `LootableSpawn` (1430) / `LootableDespawn` (1431) sind in Kategorie 14
- **Inventory-System**: `InventorySlotUpdate` (501) wird nach jedem erfolgreichen Loot gesendet
- **Party-System**: `PartyLootChange` (707) synchronisiert Loot-Methode Änderungen
- **Currency-System**: `GoldUpdate` (3703) wird bei Gold-Loot gesendet

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Vollständig dokumentiert (29 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/31-loot.md
