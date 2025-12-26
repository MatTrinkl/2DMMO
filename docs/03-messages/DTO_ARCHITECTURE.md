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

## 🏛️ Architektur-Entscheidung: Kein MessagePack auf Entities

Eine wichtige Design-Entscheidung im 2DMMO Projekt ist die **Trennung von Domain-Entities und Serialisierungs-Concerns**. MessagePack Attribute (`[MessagePackObject]`, `[Key]`) werden langfristig von den Domain-Entities entfernt.

### Warum diese Trennung?

#### 1. Single Responsibility Principle
Jede Klasse sollte genau eine Verantwortung haben:

- **Entity** = Business-Logik, Domain-Modell, Spielregeln
- **DTO** = Netzwerk-Serialisierung, Client-Synchronisation
- **DbEntity** = Datenbank-Persistierung, Queries, Indizes

#### 2. Sicherheit
Wenn Entities keine Serialisierungs-Attribute haben, können sie **nicht versehentlich** über das Netzwerk gesendet werden:

```csharp
// Mit MessagePack auf Entity - GEFAHR!
[MessagePackObject]
public class PlayerEntity
{
    [Key(0)] public long Gold { get; set; }  // Könnte versehentlich gesendet werden!
}

// Ohne MessagePack - SICHER!
public class PlayerEntity
{
    public long Gold { get; set; }  // Compile-Error wenn versucht zu serialisieren
}
```

#### 3. Datenbank-Flexibilität
EF Core kann mit queryable Spalten arbeiten statt Binary Blobs:

```csharp
// Queryable Spalten (✅)
SELECT * FROM Characters WHERE Level > 10 AND Gold > 1000;

// vs Binary Blob (❌)
SELECT * FROM Characters WHERE Data LIKE '%...%';  // Unmöglich!
```

#### 4. Refactoring-Freiheit
Keys nur in DTOs bedeutet:

- Entity-Properties können **frei umbenannt** werden
- Neue Properties können **ohne Key-Verwaltung** hinzugefügt werden
- DTO-Keys bleiben stabil für Netzwerk-Kompatibilität

### Architektur-Diagramm

```
┌─────────────────────────────────────────────────────────────────┐
│  ENTITIES (Domain Layer) - KEINE Serialisierung-Attribute       │
│                                                                  │
│  PlayerEntity.cs                                                │
│  ├── Properties (public get/set)                                │
│  ├── Business-Logik (LevelUp, TakeDamage, etc.)                │
│  ├── [ServerOnly] für Generator (kein MessagePack!)            │
│  └── Validierung                                                │
└─────────────────────────────────────────────────────────────────┘
           │                              │
           ▼                              ▼
┌─────────────────────┐    ┌─────────────────────────────────────┐
│  DTOs (Network)      │    │  DB Entities (Persistence)          │
│  [MessagePackObject] │    │  [Table], [Column] (EF Core)        │
│                      │    │                                     │
│  PlayerEntityDto     │    │  CharacterDbEntity                  │
│  ├── [Key(0-n)]      │    │  ├── [Key] Id                       │
│  ├── Nur Client-     │    │  ├── [Column] Level                 │
│  │   relevante Daten │    │  ├── [Column] Experience            │
│  └── FromEntity()    │    │  └── ToEntity() / FromEntity()     │
└─────────────────────┘    └─────────────────────────────────────┘
           │                              │
           ▼                              ▼
      Netzwerk                       Datenbank
   (MessagePack)                  (SQL Server/PostgreSQL)
```

---

## 📊 Drei-Schichten-Modell

Das 2DMMO verwendet drei separate Klassen pro Entity-Typ:

| Schicht | Klasse | Attribute | Zweck | Namespace |
|---------|--------|-----------|-------|-----------|
| **Domain** | `PlayerEntity` | `[GenerateDto]`, `[ServerOnly]` | Business-Logik, Server-State | `Mmo.Shared.Character.Entities` |
| **Network** | `PlayerEntityDto` | `[MessagePackObject]`, `[Key(n)]` | Client-Synchronisation | `Mmo.Shared.Character.Entities.Dtos` |
| **Persistence** | `CharacterDbEntity` | `[Table]`, `[Column]`, `[Key]` | Datenbank (EF Core) | `Mmo.Server.Data.Entities` |

### Verantwortlichkeiten

**PlayerEntity (Domain):**
- Spielregeln implementieren (LevelUp, GainExperience, TakeDamage)
- In-Memory State verwalten
- Validierung durchführen
- Keine Kenntnis von Serialisierung oder Persistierung

**PlayerEntityDto (Network):**
- Client-sichtbare Daten enthalten
- MessagePack-Serialisierung
- Stabile Keys für Netzwerk-Kompatibilität
- `FromEntity()` Mapping

**CharacterDbEntity (Persistence):**
- Datenbank-Schema definieren
- EF Core Mapping
- Queryable Properties
- `ToEntity()` / `FromEntity()` Mapping

---

## 🗄️ Warum keine Binary Blobs in der DB?

MessagePack-serialisierte Entities als Binary Blobs in der Datenbank zu speichern würde viele Nachteile haben:

### ❌ Mit Binary Blob (Schlecht)

```sql
CREATE TABLE Players (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Data VARBINARY(MAX)  -- MessagePack Blob
);

-- ❌ Query: Finde alle Spieler Level > 10
-- NICHT MÖGLICH ohne vollständige Deserialisierung aller Rows!

-- ❌ Query: Top 10 reichste Spieler
-- NICHT MÖGLICH - Gold ist im Blob versteckt!

-- ❌ Query: Alle Spieler einer bestimmten Klasse
-- NICHT MÖGLICH - Klasse ist im Blob!

-- ❌ Index auf Level erstellen
-- NICHT MÖGLICH - kann nicht auf Blob-Inhalt indizieren!
```

**Probleme:**
- 🔴 Keine effizienten Queries
- 🔴 Keine Indizes auf Spalten
- 🔴 Keine Joins mit anderen Tabellen
- 🔴 Keine Aggregationen (COUNT, SUM, AVG)
- 🔴 Datenmigration bei Schema-Änderungen schwierig
- 🔴 Debugging: Blob-Daten nicht lesbar in DB-Tools

### ✅ Mit EF Core Mapping (Gut)

```sql
CREATE TABLE Characters (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    Level INT NOT NULL,
    Experience BIGINT NOT NULL,
    Gold BIGINT NOT NULL,
    Race INT NOT NULL,
    Class INT NOT NULL,
    CurrentZoneId INT,
    CreatedAt DATETIME2 NOT NULL,
    LastLoginAt DATETIME2
);

-- ✅ Query: Finde alle Spieler Level > 10
SELECT * FROM Characters WHERE Level > 10;

-- ✅ Query: Top 10 reichste Spieler
SELECT TOP 10 Id, DisplayName, Gold 
FROM Characters 
ORDER BY Gold DESC;

-- ✅ Query: Durchschnitts-Level pro Klasse
SELECT Class, AVG(Level) as AvgLevel 
FROM Characters 
GROUP BY Class;

-- ✅ Index für schnelle Level-Queries
CREATE INDEX IX_Characters_Level ON Characters(Level);

-- ✅ Join mit Accounts
SELECT c.DisplayName, a.Email 
FROM Characters c 
INNER JOIN Accounts a ON c.AccountId = a.Id 
WHERE c.Level > 50;
```

**Vorteile:**
- ✅ Effiziente Queries
- ✅ Indizes auf allen Spalten
- ✅ Joins möglich
- ✅ Aggregationen einfach
- ✅ Schema-Migrationen mit EF Core
- ✅ Lesbare Daten in DB-Tools

---

## 💻 Code-Beispiele für Drei-Schichten

### Domain Entity (Clean, keine Serialisierungs-Attribute)

```csharp
namespace Mmo.Shared.Character.Entities;

[GenerateDto]
public class PlayerEntity : CombatEntity, ICharacterEntity
{
    // ⚠️ Keine [MessagePackObject], keine [Key] Attribute!
    // Diese werden in Phase 2 entfernt.
    
    public Guid CharacterId { get; set; }
    public Guid AccountId { get; set; }
    public string DisplayName { get; set; }
    public int Level { get; set; }
    public Position Position { get; set; }
    
    [ServerOnly]  // Nur für DTO-Generator, kein MessagePack!
    public long Experience { get; set; }
    
    [ServerOnly]
    public long Gold { get; set; }
    
    public Race Race { get; set; }
    public CharacterClass Class { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    
    // ═══════════════════════════════════════════════════════════════
    // Business-Logik bleibt hier
    // ═══════════════════════════════════════════════════════════════
    
    public bool GainExperience(long amount)
    {
        if (amount <= 0) return false;
        
        Experience += amount;
        
        if (Experience >= ExperienceToNextLevel)
        {
            LevelUp();
            return true;
        }
        
        return false;
    }
    
    public void LevelUp()
    {
        Experience -= ExperienceToNextLevel;
        Level++;
        
        // Stats erhöhen
        int healthGain = 10 + Level * 2;
        MaxHealth += healthGain;
        CurrentHealth = MaxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - damage);
        if (CurrentHealth == 0)
        {
            OnDeath();
        }
    }
}
```

### Network DTO (MessagePack-serialisierbar)

```csharp
namespace Mmo.Shared.Character.Entities.Dtos;

[MessagePackObject]
public sealed class PlayerEntityDto : IPlayerData
{
    // ═══════════════════════════════════════════════════════════════
    // MessagePack Keys - stabil für Netzwerk-Kompatibilität
    // ═══════════════════════════════════════════════════════════════
    
    [Key(0)] public EntityIdentity RuntimeId { get; set; }
    [Key(1)] public Guid PersistentId { get; set; }
    [Key(2)] public Position Position { get; set; }
    [Key(3)] public ushort PrefabId { get; set; }
    [Key(4)] public string DisplayName { get; set; } = "";
    [Key(5)] public int Level { get; set; }
    [Key(6)] public int CurrentHealth { get; set; }
    [Key(7)] public int MaxHealth { get; set; }
    [Key(8)] public int CurrentResource { get; set; }
    [Key(9)] public int MaxResource { get; set; }
    
    [Key(19)] public Guid CharacterId { get; set; }
    [Key(21)] public Race Race { get; set; }
    [Key(22)] public CharacterClass Class { get; set; }
    [Key(23)] public Gender Gender { get; set; }
    
    // ═══════════════════════════════════════════════════════════════
    // NICHT enthalten: Experience, Gold, AccountId (ServerOnly)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Erstellt DTO aus Entity, filtert ServerOnly Properties.
    /// </summary>
    public static PlayerEntityDto FromEntity(PlayerEntity entity) => new()
    {
        RuntimeId = entity.RuntimeId,
        PersistentId = entity.PersistentId,
        Position = entity.Position,
        PrefabId = entity.PrefabId,
        DisplayName = entity.DisplayName,
        Level = entity.Level,
        CurrentHealth = entity.CurrentHealth,
        MaxHealth = entity.MaxHealth,
        CurrentResource = entity.CurrentResource,
        MaxResource = entity.MaxResource,
        CharacterId = entity.CharacterId,
        Race = entity.Race,
        Class = entity.Class,
        Gender = entity.Gender,
        // Experience - ÜBERSPRUNGEN (ServerOnly)
        // Gold - ÜBERSPRUNGEN (ServerOnly)
        // AccountId - ÜBERSPRUNGEN (ServerOnly, sollte es zumindest sein)
    };
}
```

### DB Entity (für EF Core - geplant für später)

```csharp
namespace Mmo.Server.Data.Entities;

[Table("Characters")]
public class CharacterDbEntity
{
    // ═══════════════════════════════════════════════════════════════
    // EF Core Mapping - optimiert für Queries
    // ═══════════════════════════════════════════════════════════════
    
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [ForeignKey(nameof(Account))]
    public Guid AccountId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string DisplayName { get; set; } = "";
    
    [Required]
    public int Level { get; set; }
    
    [Required]
    public long Experience { get; set; }
    
    [Required]
    public long Gold { get; set; }
    
    [Required]
    public int Race { get; set; }  // Enum als INT
    
    [Required]
    public int Class { get; set; }  // Enum als INT
    
    [Required]
    public int Gender { get; set; }
    
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    
    // Position als separate Spalten für Queries
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public int CurrentZoneId { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation Properties
    public virtual AccountDbEntity Account { get; set; } = null!;
    
    // ═══════════════════════════════════════════════════════════════
    // Mapping-Methoden
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Konvertiert DB Entity zu Domain Entity.
    /// </summary>
    public PlayerEntity ToEntity() => new()
    {
        CharacterId = Id,
        AccountId = AccountId,
        DisplayName = DisplayName,
        Level = Level,
        Experience = Experience,
        Gold = Gold,
        Race = (Race)Race,
        Class = (CharacterClass)Class,
        Gender = (Gender)Gender,
        CurrentHealth = CurrentHealth,
        MaxHealth = MaxHealth,
        Position = new Position(PositionX, PositionY, PositionZ, (ushort)CurrentZoneId),
    };
    
    /// <summary>
    /// Erstellt DB Entity aus Domain Entity.
    /// </summary>
    public static CharacterDbEntity FromEntity(PlayerEntity entity) => new()
    {
        Id = entity.CharacterId,
        AccountId = entity.AccountId,
        DisplayName = entity.DisplayName,
        Level = entity.Level,
        Experience = entity.Experience,
        Gold = entity.Gold,
        Race = (int)entity.Race,
        Class = (int)entity.Class,
        Gender = (int)entity.Gender,
        CurrentHealth = entity.CurrentHealth,
        MaxHealth = entity.MaxHealth,
        PositionX = entity.Position.X,
        PositionY = entity.Position.Y,
        PositionZ = entity.Position.Z,
        CurrentZoneId = entity.Position.ZoneId,
        LastLoginAt = DateTime.UtcNow,
    };
    
    /// <summary>
    /// Aktualisiert DB Entity mit Daten aus Domain Entity.
    /// </summary>
    public void UpdateFromEntity(PlayerEntity entity)
    {
        DisplayName = entity.DisplayName;
        Level = entity.Level;
        Experience = entity.Experience;
        Gold = entity.Gold;
        CurrentHealth = entity.CurrentHealth;
        MaxHealth = entity.MaxHealth;
        PositionX = entity.Position.X;
        PositionY = entity.Position.Y;
        PositionZ = entity.Position.Z;
        CurrentZoneId = entity.Position.ZoneId;
        LastLoginAt = DateTime.UtcNow;
    }
}
```

---

## 🔄 Migration: MessagePack von Entities entfernen

Die Entfernung von MessagePack-Attributen von Domain-Entities erfolgt in mehreren Phasen:

### Phase 1: DTOs vollständig nutzen (aktuell)

**Status:**
- [x] `PlayerEntityDto` erstellt mit allen Client-relevanten Properties
- [x] `[ServerOnly]` Attribute definiert für Experience, Gold
- [x] Zone Messages auf DTOs umgestellt (JoinZone, ZoneState, PlayerJoinedZone)
- [ ] Alle anderen Messages auf DTOs umstellen
- [ ] Combat Messages mit DTOs
- [ ] Movement Messages mit DTOs

**Ziel:** Alle Messages verwenden DTOs statt Entities direkt.

### Phase 2: MessagePack von Entities entfernen (geplant)

**Status:** 🔵 Geplant nach Abschluss Phase 1

**Schritte:**
- [ ] `[MessagePackObject]` von `PlayerEntity` entfernen
- [ ] `[MessagePackObject]` von `CombatEntity` entfernen
- [ ] `[MessagePackObject]` von `NpcEntity` entfernen
- [ ] `[Key(n)]` Attribute von allen Entity-Properties entfernen
- [ ] `[Union]` von `IEntity` Interface entfernen (falls vorhanden)
- [ ] Source Generator anpassen: Liest Properties ohne Keys
- [ ] Entity-Properties können frei umbenannt werden

**Vorteile nach Phase 2:**
- ✅ Entities sind "clean" ohne Serialisierungs-Concerns
- ✅ Compile-Error wenn Entity versehentlich serialisiert wird
- ✅ Refactoring-Sicherheit

### Phase 3: Datenbank-Layer hinzufügen (Zukunft)

**Status:** 🔵 Zukünftig

**Schritte:**
- [ ] `CharacterDbEntity` mit EF Core Attributen erstellen
- [ ] `AccountDbEntity`, `ItemDbEntity`, etc. erstellen
- [ ] `DbContext` konfigurieren
- [ ] Repository-Pattern implementieren
- [ ] Migrations erstellen
- [ ] Indizes definieren
- [ ] Seed-Data bereitstellen

**Vorteile nach Phase 3:**
- ✅ Persistente Speicherung in SQL-Datenbank
- ✅ Effiziente Queries und Indizes
- ✅ Transaktions-Support
- ✅ Backup und Recovery

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

### MessagePack noch auf Entities

⚠️ **Aktueller Zustand:** `PlayerEntity`, `CombatEntity` und `NpcEntity` haben noch `[MessagePackObject]` und `[Key]` Attribute.

**Diese werden in Phase 2 entfernt**, sobald alle Messages auf DTOs umgestellt sind.

**Warum noch nicht entfernt?**

1. **Einige Messages verwenden noch Entities direkt**
   - Migrations-Phase: Schrittweise Umstellung auf DTOs
   - Vollständige Umstellung erforderlich vor Entfernung

2. **Source Generator liest aktuell [Key] Attribute**
   - Generator verwendet Keys für Property-Mapping
   - Muss angepasst werden um Properties ohne Keys zu lesen

3. **Migration muss schrittweise erfolgen**
   - Breaking Changes vermeiden
   - Kompatibilität während Umstellung sicherstellen
   - Tests müssen angepasst werden

**Zeitplan:**
- **Jetzt (Phase 1):** DTOs für alle Messages implementieren
- **Dann (Phase 2):** MessagePack-Attribute von Entities entfernen
- **Später (Phase 3):** DB-Layer mit EF Core hinzufügen

**Ziel:** Clean Domain Entities ohne Serialisierungs-Concerns.

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
