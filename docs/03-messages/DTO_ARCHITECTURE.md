# 🔄 DTO Architecture

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-26  
**Teil von:** [Message Documentation](README.md) | [Architektur](../02-architecture/README.md)

---

## 📋 Übersicht

Das **DTO (Data Transfer Object) Pattern** wird im 2DMMO verwendet um Server→Client Kommunikation sicher und effizient zu gestalten. DTOs sind spezialisierte Klassen die nur die Daten enthalten, die an Clients übertragen werden sollen.

### Warum DTOs?

```
Entity (Server)  →  DTO (Network)  →  Client
   ↓                     ↓                ↓
[Gold: 1000]        [EXCLUDED]       [no access]
[Experience: 500]   [EXCLUDED]       [no access]
[Position: X,Y]     [Position: X,Y]  [Position: X,Y]
[AccountId: xxx]    [EXCLUDED]       [no access]
```

**Vorteile:**
- 🔒 **Sicherheit**: Server-only Daten werden nicht übertragen
- ✅ **Compile-Time Safety**: Type-sichere Mappings
- 📊 **Performance**: Optimierte Serialisierung ohne unnötige Daten
- 🎯 **Explizit**: Klare Trennung zwischen Server-State und Client-State

---

## 🏗️ DTO Pattern Flow

### Von Entity zu Client

```
1. PlayerEntity (Server)
   ├─ Experience: 1000 [ServerOnly]
   ├─ Gold: 500 [ServerOnly]
   ├─ Position: (10, 20)
   └─ DisplayName: "Alice"

2. PlayerEntityDto.FromEntity(entity)
   ├─ Position: (10, 20) ✅
   └─ DisplayName: "Alice" ✅
   
3. MessagePack Serialization
   [bytes]

4. Network Transfer
   Client ← Server

5. MessagePack Deserialization
   PlayerEntityDto (Client)
   ├─ Position: (10, 20)
   └─ DisplayName: "Alice"
```

---

## 🏷️ Attribute System

### `[GenerateDto]` Attribute

Markiert eine Entity-Klasse für die automatische DTO-Generierung.

```csharp
[MessagePackObject]
[GenerateDto]
public class PlayerEntity : CombatEntity
{
    // DTO wird automatisch generiert
}
```

**Properties:**
- `DtoName` (optional): Custom Name für das DTO (Default: "{ClassName}Dto")
- `Exclude` (optional): Property-Namen die ausgeschlossen werden sollen

**Beispiel:**
```csharp
[GenerateDto(DtoName = "PlayerData", Exclude = new[] { "InternalState" })]
public class PlayerEntity { }
// Generiert: PlayerData.cs
```

### `[ServerOnly]` Attribute

Markiert Properties die **NICHT** an Clients übertragen werden sollen.

```csharp
[Key(25)]
[ServerOnly]
public long Experience { get; set; }

[Key(33)]
[ServerOnly]
public long Gold { get; set; }
```

**Wichtig:** ServerOnly Properties werden beim DTO-Mapping übersprungen.

### `[SyncToClient]` Attribute

Explizite Markierung dass ein Property synchronisiert werden soll (optional, per Default werden alle Properties synchronisiert außer ServerOnly).

```csharp
[Key(4)]
[SyncToClient]
public string DisplayName { get; set; } = "";
```

---

## 📦 PlayerEntityDto - Vollständige Referenz

### Property-Liste mit Keys

```csharp
[MessagePackObject]
public sealed class PlayerEntityDto : IPlayerData
{
    // ═══════════════════════════════════════════════════════════════
    // From CombatEntity (Keys 0-17)
    // ═══════════════════════════════════════════════════════════════
    
    [Key(0)]  public EntityIdentity RuntimeId { get; set; }
    [Key(1)]  public Guid PersistentId { get; set; }
    [Key(2)]  public Position Position { get; set; }
    [Key(3)]  public ushort PrefabId { get; set; }
    [Key(4)]  public string DisplayName { get; set; } = "";
    [Key(5)]  public int Level { get; set; }
    [Key(6)]  public int CurrentHealth { get; set; }
    [Key(7)]  public int MaxHealth { get; set; }
    [Key(8)]  public int CurrentResource { get; set; }
    [Key(9)]  public int MaxResource { get; set; }
    [Key(10)] public CombatResourceType CombatResourceType { get; set; }
    [Key(11)] public bool IsInCombat { get; set; }
    [Key(12)] public Guid? TargetEntityId { get; set; }
    [Key(13)] public Faction Faction { get; set; }
    [Key(14)] public int AttackPower { get; set; }
    [Key(15)] public int Armor { get; set; }
    [Key(16)] public float MovementSpeed { get; set; }
    [Key(17)] public float BaseMovementSpeed { get; set; }
    
    // ═══════════════════════════════════════════════════════════════
    // From PlayerEntity (Keys 19-35)
    // Note: Key(18) is CombatResourceType override in PlayerEntity
    // Note: Key(25)=Experience and Key(33)=Gold are EXCLUDED (ServerOnly)
    // ═══════════════════════════════════════════════════════════════
    
    [Key(19)] public Guid CharacterId { get; set; }
    [Key(20)] public Guid AccountId { get; set; }
    [Key(21)] public Race Race { get; set; }
    [Key(22)] public CharacterClass Class { get; set; }
    [Key(23)] public Gender Gender { get; set; }
    [Key(24)] public string? Title { get; set; }
    
    // Key(25) = Experience - SKIPPED (ServerOnly)
    
    [Key(26)] public bool IsPvpFlagged { get; set; }
    [Key(27)] public DateTime? PvpFlagExpires { get; set; }
    [Key(28)] public int HonorPoints { get; set; }
    [Key(29)] public int PvpKills { get; set; }
    [Key(30)] public int PvpDeaths { get; set; }
    [Key(31)] public CharacterState State { get; set; }
    [Key(32)] public MovementFlags MovementFlags { get; set; }
    
    // Key(33) = Gold - SKIPPED (ServerOnly)
    
    [Key(34)] public BindLocation? HearthstoneLocation { get; set; }
    [Key(35)] public Position? LastSafePosition { get; set; }
}
```

### ServerOnly Properties Erklärung

Diese Properties werden **NICHT** an Clients übertragen:

#### `Experience` (Key 25)
- **Warum ServerOnly?** Verhindert Client-seitige XP-Manipulation
- **Client-Bedarf:** Client zeigt XP über dedizierte `ExperienceUpdate` Messages
- **Sicherheit:** Vermeidet Exploit durch modifizierte Clients

#### `Gold` (Key 33)
- **Warum ServerOnly?** Kritische Währungs-Daten müssen geschützt sein
- **Client-Bedarf:** Client empfängt Gold über `CurrencyUpdate` Messages
- **Sicherheit:** Verhindert Gold-Duping und Wirtschafts-Exploits

#### `AccountId` (Key 20) ⚠️
- **PROBLEM:** Aktuell noch im DTO enthalten
- **Sollte sein:** ServerOnly (siehe "Bekannte Probleme")
- **Risiko:** Account-ID Exposure kann Account-Zuordnung ermöglichen

---

## 💻 Verwendung in Messages

### Server-Side: Entity → DTO Mapping

```csharp
// PlayerEntity auf dem Server
var playerEntity = new PlayerEntity(characterId, accountId, "Alice", position)
{
    Experience = 1000,
    Gold = 500,
    Level = 10,
    CurrentHealth = 100
};

// DTO für Client erstellen
var dto = PlayerEntityDto.FromEntity(playerEntity);

// In Message verwenden
var joinZoneMessage = new JoinZone
{
    Type = MessageType.JoinZone,
    ZoneId = 1001,
    ZoneName = "Elwynn Forest",
    PlayerData = dto // PlayerEntityDto
};

// Serialisieren und senden
byte[] bytes = MessageSerializer.Serialize(joinZoneMessage);
await client.SendAsync(bytes);
```

### Client-Side: DTO empfangen

```csharp
// Message empfangen und deserialisieren
var joinZone = MessageSerializer.Deserialize<JoinZone>(bytes);

// DTO ist type-safe
PlayerEntityDto playerData = joinZone.PlayerData;

// Client hat KEINEN Zugriff auf:
// - playerData.Experience (existiert nicht im DTO)
// - playerData.Gold (existiert nicht im DTO)

// Client hat Zugriff auf:
Console.WriteLine($"Player: {playerData.DisplayName}");
Console.WriteLine($"Level: {playerData.Level}");
Console.WriteLine($"Health: {playerData.CurrentHealth}/{playerData.MaxHealth}");
```

---

## 🔗 Verschachtelte DTOs für ZoneState

### IEntityDto Union-Interface

Für polymorphe Entity-Listen (Spieler + NPCs) verwenden wir ein gemeinsames Interface:

```csharp
// Geplant (noch nicht implementiert)
public interface IEntityDto
{
    EntityIdentity RuntimeId { get; }
    Guid PersistentId { get; }
    Position Position { get; }
    EntityType Type { get; }
}
```

### ZoneState Message mit DTOs

```csharp
[MessagePackObject]
public class ZoneState : INetworkMessage
{
    [Key(0)]
    public MessageType Type => MessageType.ZoneState;
    
    [Key(1)]
    public ushort ZoneId { get; set; }
    
    [Key(2)]
    public long ServerTime { get; set; }
    
    [Key(3)]
    public List<PlayerEntityDto> Players { get; set; } = new();
    
    [Key(4)]
    public List<NpcEntityDto> NPCs { get; set; } = new();
    
    // Alternative mit Union-Interface:
    // public List<IEntityDto> Entities { get; set; } = new();
}
```

**Warum separate Listen statt Union-Interface?**
- MessagePack unterstützt Polymorphie nur begrenzt
- Separate Listen sind simpler und performanter
- Type-Information ist durch Liste implizit gegeben

---

## 🛠️ Source Generator

### Warum netstandard2.0?

Source Generators müssen in **netstandard2.0** kompiliert werden:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

**Grund:**
- Roslyn Compiler läuft auf netstandard2.0
- Source Generators sind Compiler-Extensions
- Target-Framework des Projekts (net8.0) ist egal
- Generator selbst muss netstandard2.0 sein

**Dependencies:**
```xml
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
<PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" />
```

### Generator-Flow

```
1. Compilation Start
   ↓
2. Source Generator scannt Assembly
   ↓
3. Findet alle [GenerateDto] Attribute
   ↓
4. Liest Entity Properties
   ↓
5. Filtert [ServerOnly] Properties
   ↓
6. Generiert DTO-Code
   ↓
7. Fügt generierten Code zum Compilation hinzu
   ↓
8. Normal Compilation fortsetzt
```

---

## ⚠️ Bekannte Probleme

### Security Issues in PlayerEntityDto

Die folgenden Properties sollten `[ServerOnly]` sein, sind aber aktuell im DTO:

#### 🔴 Kritisch: `AccountId` (Key 20)
```csharp
[Key(20)] public Guid AccountId { get; set; }
```

**Problem:**
- Exposed Account-IDs ermöglichen Account-Tracking
- Privacy-Risiko: Mehrere Characters → ein Account
- Potentieller Angriffsvektor für Account-Targeting

**Lösung:**
```csharp
// In PlayerEntity.cs
[Key(20)]
[ServerOnly]  // ← Hinzufügen
public Guid AccountId { get; }
```

**Impacted Messages:**
- `JoinZone` (100)
- `ZoneState` (102)
- `PlayerJoinedZone` (103)

#### 🟡 Mittel: `AttackPower` (Key 14)
```csharp
[Key(14)] public int AttackPower { get; set; }
```

**Problem:**
- Server-seitige Schadens-Berechnung könnte reverse-engineered werden
- Client braucht AttackPower nicht für Rendering

**Aber:**
- Bereits durch Combat-System validiert
- Nur visuelle Information
- Niedriges Sicherheitsrisiko

#### 🟡 Mittel: `Armor` (Key 15)
```csharp
[Key(15)] public int Armor { get; set; }
```

**Problem:**
- Analog zu AttackPower
- Defense-Berechnung könnte durchschaubar werden

**Aber:**
- Combat-Validierung auf Server
- Nur informativ für Client

#### 🟡 Mittel: `BaseMovementSpeed` (Key 17)
```csharp
[Key(17)] public float BaseMovementSpeed { get; set; }
```

**Problem:**
- Server-Authority bei Movement-Validierung
- Client könnte versuchen Speed zu manipulieren

**Aber:**
- Server validiert alle Movement-Updates
- Anti-Cheat prüft Speed-Hacks
- `MovementSpeed` (Key 16) ist die aktuelle Speed (mit Buffs)

### Migration-Plan

1. **Phase 1 (Sofort):** `AccountId` auf ServerOnly setzen
2. **Phase 2 (Bald):** Stats-Refactoring für Combat-Attribute
3. **Phase 3 (Später):** Movement-Speed Redesign

---

## 📚 Verfügbare DTOs

| DTO | Entity | Status | Verwendet in |
|-----|--------|--------|--------------|
| `PlayerEntityDto` | `PlayerEntity` | ✅ Implementiert | JoinZone (100), ZoneState (102), PlayerJoinedZone (103) |
| `NpcEntityDto` | `NpcEntity` | 🟡 Geplant | ZoneState (102), EntitySpawn (1400) |
| `IEntityDto` | Union-Interface | 🟡 Geplant | ZoneState polymorphe Liste |
| `CombatEntityDto` | - | 🔵 Future | DamageEvent (302), HealEvent (303) |
| `PartyMemberDto` | - | 🔵 Future | PartyList (710), PartyUpdate (711) |
| `TargetEntityDto` | - | 🔵 Future | TargetChanged (1200), TargetFrame UI |
| `CharacterListItemDto` | - | 🔵 Future | CharacterList (601) |

**Legende:**
- ✅ Implementiert und in Verwendung
- 🟡 Geplant für aktuelle Phase
- 🔵 Zukünftig (Phase 2/3)

---

## 🔍 Best Practices

### DO ✅

```csharp
// Server: DTO erstellen
var dto = PlayerEntityDto.FromEntity(entity);

// Message mit DTO
var message = new JoinZone { PlayerData = dto };

// Serialisieren
byte[] bytes = MessageSerializer.Serialize(message);
```

### DON'T ❌

```csharp
// NIEMALS: Entity direkt senden
var message = new JoinZone { PlayerData = entity }; // ❌ Type Error

// NIEMALS: Properties manuell kopieren
var dto = new PlayerEntityDto
{
    DisplayName = entity.DisplayName,
    // ... vergisst leicht Properties
}; // ❌ Error-prone

// NIEMALS: ServerOnly Daten manuell in DTO setzen
dto.Experience = entity.Experience; // ❌ Property existiert nicht
```

### Validation

```csharp
// Server validiert immer Input
public void HandleMoveRequest(MoveRequest request)
{
    // ✅ Nie Client-Daten vertrauen
    var player = GetPlayer(request.PlayerId);
    
    // ✅ Server berechnet selbst
    bool isValidMove = ValidateMovement(player, request.TargetPosition);
    
    if (isValidMove)
    {
        player.Position = request.TargetPosition;
        
        // ✅ DTO für Broadcast erstellen
        var dto = PlayerEntityDto.FromEntity(player);
        BroadcastPlayerMove(dto);
    }
}
```

---

## 🔗 Verwandte Dokumentation

- **[Message Reference](README.md)** - Übersicht aller Messages
- **[Zone Messages](01-zone.md)** - Verwendung von PlayerEntityDto
- **[Security](../02-architecture/SECURITY.md)** - Warum ServerOnly wichtig ist
- **[MessagePack](../02-architecture/MESSAGES.md)** - Serialisierungs-Details

---

**Letzte Aktualisierung**: 2025-12-26  
**Version**: 1.0.0  
**Maintainer**: 2DMMO Team
