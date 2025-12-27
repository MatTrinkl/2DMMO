# 🔄 DTO Architecture

**Version:** 2.0.0  
**Letzte Aktualisierung:** 2025-12-27  
**Teil von:** [Message Documentation](README.md) | [Architektur](../02-architecture/README.md)

---

## 📋 Übersicht

Das **DTO (Data Transfer Object) Pattern** wird im 2DMMO verwendet um Server→Client Kommunikation sicher und effizient zu gestalten. DTOs sind spezialisierte Klassen die nur die Daten enthalten, die der Client sehen darf.

### Warum DTOs?

```
Entity (Server)  →  DTO (Network)  →  Client
   ↓                     ↓                ↓
[Gold:  1000]        [EXCLUDED]       [no access]
[Experience: 500]   [EXCLUDED]       [no access]
[Position: X,Y]     [Position: X,Y]  [Position: X,Y]
[AccountId: xxx]    [EXCLUDED]       [no access]
```

**Vorteile:**

-   🔒 **Sicherheit**: Server-only Daten werden nicht übertragen
-   ✅ **Compile-Time Safety**: Type-sichere Mappings
-   📊 **Performance**: Optimierte Serialisierung ohne unnötige Daten
-   🎯 **Explizit**: Klare Trennung zwischen Server-State und Client-State
-   🔄 **Caching**: DTOs wie `ZoneDto` können client-seitig gecached werden

---

## 🏗️ DTO Pattern Flow

### Von Entity zu Client

```
1. PlayerEntity (Server)
   ├─ Experience:  1000 [ServerOnly]
   ├─ Gold: 500 [ServerOnly]
   ├─ Position: (10, 20)
   └─ DisplayName: "Alice"

2. PlayerEntityDto. FromPlayerEntity(entity)
   ├─ Position: (10, 20) ✅
   └─ DisplayName: "Alice" ✅

3. MessagePack Serialization
   [bytes]

4. Network Transfer
   Client ← Server

5. MessagePack Deserialization
   PlayerEntityDto (Client)
   ├─ Position: (10, 20)
   └─ DisplayName:  "Alice"
```

---

## 🏷️ Attribute System

### `[GenerateDto]` Attribute

Markiert eine Entity-Klasse für die automatische DTO-Generierung durch den Source Generator.

```csharp
[GenerateDto]
public class PlayerEntity :  CombatEntity
{
    // DTO wird automatisch generiert:  PlayerEntityDto
}
```

**Properties:**

| Property            | Typ     | Default | Beschreibung                                    |
| ------------------- | ------- | ------- | ----------------------------------------------- |
| `InheritInterfaces` | bool    | false   | Übernimmt alle Interfaces der Source-Class      |
| `DtoName`           | string? | null    | Custom Name (Default: "{ClassName}Dto")         |
| `DtoNamespace`      | string? | null    | Custom Namespace (Default: gleicher wie Source) |
| `DtoSuffix`         | string  | "Dto"   | Suffix für generierten Namen                    |

**Beispiele:**

```csharp
// Standard:  Generiert PlayerEntityDto
[GenerateDto]
public class PlayerEntity { }

// Mit Interface-Vererbung
[GenerateDto(InheritInterfaces = true)]
public class PlayerEntity : IPlayerData { }
// Generiert:  PlayerEntityDto :  IPlayerData

// Custom Name
[GenerateDto(DtoName = "PlayerSnapshot")]
public class PlayerEntity { }
// Generiert: PlayerSnapshot
```

### `[DtoImplements]` Attribute

Explizite Angabe welche Interfaces das generierte DTO implementieren soll.

```csharp
[GenerateDto]
[DtoImplements(typeof(IPlayerData))]
[DtoImplements(typeof(IPositionable))]
public class PlayerEntity : IPlayerData, IPositionable, IServerInternals
{
    // Generiert: PlayerEntityDto :  IPlayerData, IPositionable
    // IServerInternals wird NICHT übernommen
}
```

### `[ServerOnly]` Attribute

Markiert Properties die **NICHT** an Clients übertragen werden sollen.

```csharp
[ServerOnly]
public long Experience { get; set; }

[ServerOnly]
public long Gold { get; set; }

[ServerOnly]
public Guid AccountId { get; set; }
```

**Wichtig:** ServerOnly Properties werden beim DTO-Mapping übersprungen.

### `[DtoIgnore]` Attribute

Alias für `[ServerOnly]` mit klarerer Semantik für Properties die einfach nicht im DTO sein sollen.

```csharp
[DtoIgnore]
public DateTime LastUpdated { get; set; }

[DtoIgnore]
public HashSet<Guid> InternalCache { get; set; }
```

### `[DtoProperty]` Attribute

Optionale Konfiguration für einzelne Properties im generierten DTO.

```csharp
[DtoProperty(Name = "PlayerName", Key = 5)]
public string DisplayName { get; set; }
// Generiert: [Key(5)] public string PlayerName { get; set; }
```

---

## 🔧 Source Generator

### Projekt-Setup

Der Source Generator ist ein separates Projekt das zur Compile-Zeit DTOs generiert.

**Projekt-Struktur:**

```
2DMMO/
├── Mmo. Generators/                 ← Generator Projekt
│   ├── Mmo.Generators.csproj       ← netstandard2.0!
│   └── DtoGenerator.cs
│
├── Mmo. Shared/                     ← Shared Projekt
│   ├── Mmo.Shared.csproj           ← Referenziert Generator
│   └── Generators/
│       └── Attributes/
│           ├── GenerateDtoAttribute.cs
│           ├── DtoImplementsAttribute. cs
│           ├── ServerOnlyAttribute.cs
│           ├── DtoIgnoreAttribute. cs
│           └── DtoPropertyAttribute.cs
```

**Generator csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <!-- MUSS netstandard2.0 sein für Roslyn -->
        <TargetFramework>netstandard2.0</TargetFramework>
        <LangVersion>latest</LangVersion>
        <Nullable>enable</Nullable>
        <IsRoslynComponent>true</IsRoslynComponent>
        <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
        <IncludeBuildOutput>false</IncludeBuildOutput>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.CodeAnalysis. Analyzers" Version="3.3.4">
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
        <PackageReference Include="PolySharp" Version="1.14.1">
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
    </ItemGroup>
</Project>
```

**Einbindung in Mmo.Shared:**

```xml
<ItemGroup>
    <ProjectReference Include="..\Mmo.Generators\Mmo.Generators. csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
</ItemGroup>
```

### Generierte Dateien

Generierte DTOs erscheinen unter:

-   **IDE:** Solution Explorer → Dependencies → Analyzers → Mmo.Generators
-   **Dateisystem:** `obj/Debug/net8.0/generated/Mmo.Generators/`

**Beispiel generiertes DTO:**

```csharp
// <auto-generated/>
// This file was generated by Mmo.Generators.DtoGenerator

#nullable enable

using MessagePack;
using System;
using Mmo.Shared. Entities;

namespace Mmo. Shared.Entities;

/// <summary>
/// Auto-generated DTO for <see cref="PlayerEntity"/>.
/// </summary>
[MessagePackObject]
public sealed class PlayerEntityDto :  IPlayerData
{
    [Key(0)] public Guid PersistentId { get; set; }
    [Key(1)] public string DisplayName { get; set; }
    [Key(2)] public int Level { get; set; }
    [Key(3)] public Position Position { get; set; }
    // Experience, Gold, AccountId sind NICHT enthalten (ServerOnly)

    /// <summary>
    /// Creates a PlayerEntityDto from a PlayerEntity.
    /// </summary>
    public static PlayerEntityDto FromPlayerEntity(PlayerEntity source)
    {
        return new PlayerEntityDto
        {
            PersistentId = source. PersistentId,
            DisplayName = source.DisplayName,
            Level = source.Level,
            Position = source.Position,
        };
    }

    /// <summary>
    /// Applies this DTO's values to an existing PlayerEntity.
    /// </summary>
    public void ApplyTo(PlayerEntity target)
    {
        target. DisplayName = this.DisplayName;
        target. Level = this.Level;
        target.Position = this.Position;
    }
}
```

### Manuelle DTOs löschen

Wenn der Generator funktioniert, können manuell erstellte DTOs **gelöscht** werden:

```
VORHER (manuell):
Mmo.Shared/
├── Entities/
│   ├── PlayerEntity.cs
│   └── Dtos/
│       └── PlayerEntityDto.cs    ← LÖSCHEN!

NACHHER (generiert):
Mmo.Shared/
├── Entities/
│   └── PlayerEntity.cs           ← [GenerateDto]
│
└── obj/generated/
    └── PlayerEntityDto.g.cs      ← AUTO-GENERIERT
```

---

## 📦 Serialisierbare Structs

Structs die übers Netzwerk gesendet werden müssen direkt `[MessagePackObject]` haben:

### Position

```csharp
[MessagePackObject]
public readonly struct Position
{
    [Key(0)] public float X { get; init; }
    [Key(1)] public float Y { get; init; }
    [Key(2)] public float Z { get; init; }
    [Key(3)] public ushort ZoneId { get; init; }

    public Position(float x, float y, float z, ushort zoneId)
    {
        X = x;
        Y = y;
        Z = z;
        ZoneId = zoneId;
    }
}
```

### ZoneBounds

```csharp
[MessagePackObject]
public readonly struct ZoneBounds
{
    [Key(0)] public float MinX { get; init; }
    [Key(1)] public float MaxX { get; init; }
    [Key(2)] public float MinY { get; init; }
    [Key(3)] public float MaxY { get; init; }

    public ZoneBounds(float minX, float maxX, float minY, float maxY)
    {
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
    }
}
```

**Regel:** Structs die in DTOs verwendet werden → `[MessagePackObject]` direkt drauf.

---

## 🗺️ ZoneDto - Statische Zone-Daten

`ZoneDto` ist ein **manuell erstelltes** DTO für Zone-Metadaten, da Zone komplexen Server-only State hat.

### Warum manuell statt generiert?

| Aspekt                                     | Generator             | Manuell               |
| ------------------------------------------ | --------------------- | --------------------- |
| Zone hat HashSet<Guid> für Entities        | ❌ Schwer zu excluden | ✅ Einfach weglassen  |
| Zone hat dynamischen State (Weather, Time) | ❌ Alles oder nichts  | ✅ Selektiv           |
| ZoneBounds als nested Struct               | ✅ Funktioniert       | ✅ Funktioniert       |
| Convenience Properties                     | ❌ Nicht generiert    | ✅ Manuell hinzufügen |

### ZoneDto Definition

```csharp
[MessagePackObject]
public class ZoneDto
{
    [Key(0)] public ushort ZoneId { get; set; }
    [Key(1)] public string Name { get; set; } = "";
    [Key(2)] public ZoneFlags Flags { get; set; }
    [Key(3)] public int RecommendedMinLevel { get; set; }
    [Key(4)] public int RecommendedMaxLevel { get; set; }
    [Key(5)] public Faction?  ControllingFaction { get; set; }
    [Key(6)] public Position DefaultSpawnPoint { get; set; }
    [Key(7)] public Position?  GraveyardPosition { get; set; }
    [Key(8)] public string MusicId { get; set; } = "";
    [Key(9)] public string AmbienceId { get; set; } = "";
    [Key(10)] public ZoneBounds Bounds { get; set; }

    public static ZoneDto FromZone(Zone zone) => new()
    {
        ZoneId = zone.Id,
        Name = zone.Name,
        Flags = zone. Flags,
        RecommendedMinLevel = zone.RecommendedMinLevel,
        RecommendedMaxLevel = zone. RecommendedMaxLevel,
        ControllingFaction = zone.ControllingFaction,
        DefaultSpawnPoint = zone. DefaultSpawnPoint,
        GraveyardPosition = zone.GraveyardPosition,
        MusicId = zone.MusicId,
        AmbienceId = zone.AmbienceId,
        Bounds = zone.Bounds
    };

    // Convenience Properties (nicht serialisiert)
    [IgnoreMember] public bool IsPvPEnabled => Flags.HasFlag(ZoneFlags.PvpEnabled);
    [IgnoreMember] public bool IsInstance => Flags.HasFlag(ZoneFlags.IsInstance);
    [IgnoreMember] public bool IsCapital => Flags.HasFlag(ZoneFlags.IsCapital);
    [IgnoreMember] public bool IsSanctuary => Flags.HasFlag(ZoneFlags.NoCombat);
    [IgnoreMember] public bool HasRestXp => Flags.HasFlag(ZoneFlags.HasRestXp);
}
```

### Client-Caching

`ZoneDto` wird nur bei **erstem Besuch** einer Zone gesendet:

```csharp
public class ZoneCache
{
    private readonly Dictionary<ushort, ZoneDto> _cache = new();

    public void CacheZone(ZoneDto zone) => _cache[zone.ZoneId] = zone;
    public ZoneDto?  GetZone(ushort zoneId) => _cache.GetValueOrDefault(zoneId);
    public bool HasZone(ushort zoneId) => _cache.ContainsKey(zoneId);
}

// Verwendung
public void OnZoneState(ZoneState state)
{
    // Nur bei erstem Besuch vorhanden
    if (state. ZoneInfo != null)
    {
        _zoneCache.CacheZone(state.ZoneInfo);
    }

    // Immer aus Cache holen
    var zoneInfo = _zoneCache.GetZone(state.ZoneId);
}
```

---

## 🔄 Zone Loading Integration

### ZoneState enthält DTOs

```csharp
[MessagePackObject]
public class ZoneState :  ITimestampedServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneState;
    [Key(1)] public long Timestamp { get; set; }
    [Key(2)] public ushort ZoneId { get; set; }

    // Statische Zone-Daten (nur bei erstem Besuch, sonst null)
    [Key(3)] public ZoneDto? ZoneInfo { get; set; }

    // Dynamischer State
    [Key(4)] public WeatherType CurrentWeather { get; set; }
    [Key(5)] public float TimeOfDay { get; set; }
    [Key(6)] public ZoneStateType StateType { get; set; }

    // Dein Character als DTO
    [Key(7)] public PlayerEntityDto?  MyPlayer { get; set; }

    // Alle anderen Entities als DTOs
    [Key(8)] public List<IEntityDto> Entities { get; set; } = new();
    [Key(9)] public bool HasMoreEntities { get; set; }
    [Key(10)] public int TotalEntityCount { get; set; }
}
```

### Flow mit DTOs

```
Server                              Client
  │                                   │
  │  Zone (Server-State)              │
  │  ├── EntityIds HashSet            │
  │  ├── CurrentWeather               │
  │  └── TimeOfDay                    │
  │           │                       │
  │           ▼ ZoneDto. FromZone()    │
  │  ZoneDto (nur bei erstem Besuch)  │
  │  ├── Name, Flags, Bounds          │
  │  └── KEINE EntityIds              │
  │           │                       │
  │  PlayerEntity (Server-State)      │
  │  ├── Experience = 45000           │
  │  ├── Gold = 1250                  │
  │  └── AccountId = xxx              │
  │           │                       │
  │           ▼ FromPlayerEntity()    │
  │  PlayerEntityDto                  │
  │  ├── Level, Health, etc.          │
  │  └── KEINE sensiblen Daten        │
  │           │                       │
  │           ▼ ZoneState             │
  │  ZoneState                        │
  │  ├── ZoneInfo = ZoneDto?           │
  │  └── MyPlayer = PlayerEntityDto   │
  │──────────────────────────────────►│
  │                                   │
  │                                   │  Client cached ZoneDto
  │                                   │  Client spawnt MyPlayer
```

---

## 🏛️ Drei-Schichten-Modell

Das 2DMMO verwendet drei separate Klassen pro Entity-Typ:

```
┌─────────────────────────────────────────────────────────────────┐
│  DOMAIN ENTITIES - KEINE Serialisierungs-Attribute              │
│                                                                  │
│  PlayerEntity. cs                                                │
│  ├── Properties (public get/set)                                │
│  ├── Business-Logik (LevelUp, TakeDamage, etc.)                │
│  ├── [GenerateDto] für Generator                               │
│  ├── [ServerOnly] für geschützte Properties                    │
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

| Schicht         | Klasse              | Attribute                         | Zweck                        |
| --------------- | ------------------- | --------------------------------- | ---------------------------- |
| **Domain**      | `PlayerEntity`      | `[GenerateDto]`, `[ServerOnly]`   | Business-Logik, Server-State |
| **Network**     | `PlayerEntityDto`   | `[MessagePackObject]`, `[Key(n)]` | Client-Synchronisation       |
| **Persistence** | `CharacterDbEntity` | `[Table]`, `[Column]`             | Datenbank (EF Core)          |

---

## 💻 Code-Beispiele

### Domain Entity (mit Generator-Attributen)

```csharp
namespace Mmo.Shared. Entities;

[GenerateDto(InheritInterfaces = true)]
public class PlayerEntity : CombatEntity, IPlayerData
{
    public Guid CharacterId { get; set; }
    public string DisplayName { get; set; } = "";
    public int Level { get; set; }
    public Position Position { get; set; }
    public Race Race { get; set; }
    public CharacterClass Class { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    [ServerOnly]
    public Guid AccountId { get; set; }

    [ServerOnly]
    public long Experience { get; set; }

    [ServerOnly]
    public long Gold { get; set; }

    [DtoIgnore]
    public DateTime LastUpdated { get; set; }

    // Business-Logik
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
        MaxHealth += 10 + Level * 2;
        CurrentHealth = MaxHealth;
    }
}
```

### Server-Side: Entity → DTO

```csharp
// Entity auf dem Server
var player = new PlayerEntity
{
    CharacterId = Guid.NewGuid(),
    DisplayName = "Alice",
    Level = 10,
    Experience = 45000,  // ServerOnly
    Gold = 1250,         // ServerOnly
    AccountId = accountId // ServerOnly
};

// DTO für Client erstellen (generierte Methode)
var dto = PlayerEntityDto.FromPlayerEntity(player);

// In Message verwenden
var zoneState = new ZoneState
{
    ZoneId = 1001,
    MyPlayer = dto,
    // ...
};

// Serialisieren und senden
byte[] bytes = MessageSerializer.Serialize(zoneState);
await client.SendAsync(bytes);
```

### Client-Side: DTO empfangen

```csharp
// Message empfangen
var zoneState = MessageSerializer.Deserialize<ZoneState>(bytes);

// DTO ist type-safe
PlayerEntityDto myPlayer = zoneState.MyPlayer;

// Client hat KEINEN Zugriff auf:
// - myPlayer.Experience (existiert nicht)
// - myPlayer.Gold (existiert nicht)
// - myPlayer.AccountId (existiert nicht)

// Client hat Zugriff auf:
Console.WriteLine($"Player:  {myPlayer.DisplayName}");
Console.WriteLine($"Level: {myPlayer.Level}");
Console.WriteLine($"Health: {myPlayer.CurrentHealth}/{myPlayer.MaxHealth}");
```

---

## 🗄️ Warum keine Binary Blobs in der DB?

### ❌ Mit Binary Blob (Schlecht)

```sql
CREATE TABLE Players (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Data VARBINARY(MAX)  -- MessagePack Blob
);

-- Queries NICHT MÖGLICH:
-- SELECT * FROM Players WHERE Level > 10;
-- SELECT TOP 10 * FROM Players ORDER BY Gold DESC;
```

### ✅ Mit EF Core Mapping (Gut)

```sql
CREATE TABLE Characters (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DisplayName NVARCHAR(100) NOT NULL,
    Level INT NOT NULL,
    Experience BIGINT NOT NULL,
    Gold BIGINT NOT NULL,
    Race INT NOT NULL,
    Class INT NOT NULL
);

-- Effiziente Queries:
SELECT * FROM Characters WHERE Level > 10;
SELECT TOP 10 * FROM Characters ORDER BY Gold DESC;
CREATE INDEX IX_Characters_Level ON Characters(Level);
```

---

## 📚 Verfügbare DTOs

| DTO               | Source         | Generiert  | Verwendet in                                |
| ----------------- | -------------- | ---------- | ------------------------------------------- |
| `PlayerEntityDto` | `PlayerEntity` | ✅ Ja      | ZoneState, PlayerJoinedZone                 |
| `ZoneDto`         | `Zone`         | ❌ Manuell | ZoneState, ZoneDiscovered, ZoneListResponse |
| `ZoneListItemDto` | -              | ❌ Manuell | ZoneListResponse                            |
| `NpcEntityDto`    | `NpcEntity`    | 🟡 Geplant | ZoneState, EntitySpawn                      |
| `IEntityDto`      | Interface      | 🟡 Geplant | ZoneState (polymorphe Liste)                |

**Legende:**

-   ✅ Implementiert
-   ❌ Manuell (nicht generiert)
-   🟡 Geplant

---

## 🔍 Best Practices

### DO ✅

```csharp
// Generator-Attribute verwenden
[GenerateDto(InheritInterfaces = true)]
public class PlayerEntity : IPlayerData { }

// ServerOnly für sensible Daten
[ServerOnly]
public long Gold { get; set; }

// Generierte Methode verwenden
var dto = PlayerEntityDto.FromPlayerEntity(entity);

// Structs direkt mit MessagePackObject
[MessagePackObject]
public readonly struct Position { }
```

### DON'T ❌

```csharp
// Entity direkt serialisieren
byte[] bytes = MessagePackSerializer.Serialize(entity); // ❌

// Properties manuell kopieren
var dto = new PlayerEntityDto
{
    DisplayName = entity.DisplayName,
    // ... vergisst leicht Properties
}; // ❌

// ServerOnly Daten im DTO setzen
dto.Experience = entity.Experience; // ❌ Compile Error (Property existiert nicht)
```

---

## 🔗 Verwandte Dokumentation

-   **[Zone Messages](01-zone. md)** - ZoneState mit DTOs
-   **[Message Reference](README.md)** - Übersicht aller Messages
-   **[Security](../02-architecture/SECURITY.md)** - Warum ServerOnly wichtig ist

---

**Letzte Aktualisierung:** 2025-12-27  
**Version:** 2.0.0  
**Maintainer:** 2DMMO Team
