# 🔄 DTO Architecture

**Version:** 3.0.0  
**Letzte Aktualisierung:** 2025-12-28  
**Teil von:** [Message Documentation](Message-Reference.md) | [Architektur](../Architecture/Architecture-Overview.md)

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
1. CharacterEntity (Server) implements ICharacterEntity
   ├─ Experience:  1000 [ServerOnly]
   ├─ Gold: 500 [ServerOnly]
   ├─ Position: (10, 20)
   └─ DisplayName: "Alice"

2. entity.ToDto()  // Extension Method
   ├─ Position: (10, 20) ✅
   └─ DisplayName: "Alice" ✅

3. MessagePack Serialization
   [bytes]

4. Network Transfer
   Client ← Server

5. MessagePack Deserialization
   CharacterEntityDto (Client)
   ├─ Position: (10, 20)
   └─ DisplayName:  "Alice"
```

---

## 🏷️ Attribute System

### `[GenerateDto]` Attribute

Markiert ein **Interface** für die automatische DTO-Generierung durch den Source Generator.

```csharp
[GenerateDto(DtoName = "CharacterEntityDto")]
public interface ICharacterEntity : ICombatEntity
{
    // DTO wird automatisch generiert:  CharacterEntityDto
}
```

**Properties:**

| Property            | Typ     | Default | Beschreibung                                    |
| ------------------- | ------- | ------- | ----------------------------------------------- |
| `InheritInterfaces` | bool    | false   | Übernimmt alle Interfaces des Source-Interface  |
| `DtoName`           | string? | null    | Custom Name (Default: "{InterfaceName}Dto")     |
| `DtoNamespace`      | string? | null    | Custom Namespace (Default: gleicher wie Source) |
| `DtoSuffix`         | string  | "Dto"   | Suffix für generierten Namen                    |

**Beispiele:**

```csharp
// Standard:  Generiert CharacterEntityDto
[GenerateDto(DtoName = "CharacterEntityDto")]
public interface ICharacterEntity { }

// Mit Interface-Vererbung
[GenerateDto(InheritInterfaces = true)]
public interface ICharacterEntity : IPlayerData { }
// Generiert:  CharacterEntityDto :  IPlayerData

// Custom Name
[GenerateDto(DtoName = "PlayerSnapshot")]
public interface IPlayerEntity { }
// Generiert: PlayerSnapshot
```

### `[DtoImplements]` Attribute

Explizite Angabe welche Interfaces das generierte DTO implementieren soll.

```csharp
[GenerateDto(DtoName = "CharacterEntityDto")]
[DtoImplements(typeof(IPlayerData))]
[DtoImplements(typeof(IPositionable))]
public interface ICharacterEntity : IPlayerData, IPositionable, IServerInternals
{
    // Generiert: CharacterEntityDto :  IPlayerData, IPositionable
    // IServerInternals wird NICHT übernommen
}
```

### `[GenerateDtoUnion]` Attribute

**Neu in V3:** Markiert ein Interface als Wurzel einer Union-Hierarchie für polymorphe Serialisierung.

```csharp
[GenerateDto(DtoName = "EntityDto")]
[GenerateDtoUnion(UnionName = "EntityDtoUnion", Namespace = "Mmo.Shared.Entities.Dtos")]
public interface IEntity
{
    Guid PersistentId { get; init; }
    Position Position { get; set; }
}
```

**Properties:**

| Property    | Typ     | Default                     | Beschreibung                            |
| ----------- | ------- | --------------------------- | --------------------------------------- |
| `UnionName` | string? | "{InterfaceName}DtoUnion"   | Name des Union-Interfaces               |
| `Namespace` | string? | "{SourceNamespace}.Dtos"    | Namespace für Union                     |

### `[DtoUnionMember]` Attribute

**Neu in V3:** Markiert ein Interface als Member einer Union.

```csharp
[GenerateDto(DtoName = "CharacterEntityDto")]
[DtoUnionMember(0, typeof(IEntity))]
public interface ICharacterEntity : ICombatEntity
{
    Guid CharacterId { get; init; }
}

[GenerateDto(DtoName = "NpcEntityDto")]
[DtoUnionMember(1, typeof(IEntity))]
public interface INpcEntity : ICombatEntity
{
    int NpcTemplateId { get; init; }
}
```

**Generiert:**

```csharp
// EntityDtoUnion.g.cs
[Union(0, typeof(CharacterEntityDto))]
[Union(1, typeof(NpcEntityDto))]
public interface EntityDtoUnion { }

// CharacterEntityDto implementiert: ICharacterEntity, EntityDtoUnion
// NpcEntityDto implementiert: INpcEntity, EntityDtoUnion
```

**Parameter:**

- `index` (int): Union-Index für MessagePack
- `rootType` (Type): Das Root-Interface (mit `[GenerateDtoUnion]`)

### `[ServerOnly]` Attribute

Markiert Properties die **NICHT** an Clients übertragen werden sollen.

> **Hinweis:** Wird auf der konkreten Entity-Klasse verwendet, nicht auf dem Interface.

```csharp
// Concrete Entity Class
public class CharacterEntity : BaseEntity, ICharacterEntity
{
    // Interface Property - wird im DTO generiert
    public string DisplayName { get; set; } = "";
    
    // ServerOnly - wird NICHT im DTO generiert
    [ServerOnly]
    public long Experience { get; set; }

    [ServerOnly]
    public long Gold { get; set; }

    [ServerOnly]
    public Guid AccountId { get; set; }
}
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
public interface ICharacterEntity
{
    [DtoProperty(Name = "PlayerName", Key = 5)]
    string DisplayName { get; init; }
    // Generiert: [Key(5)] public string PlayerName { get; init; }
}
```

### Set vs Init Accessor Logic

**Wichtig in V3:** Der Generator respektiert die Accessor-Typen des Interfaces!

```csharp
public interface IEntity
{
    Guid PersistentId { get; init; }  // DTO: { get; init; }
    Position Position { get; set; }    // DTO: { get; set; }
    EntityType Type { get; }           // DTO: { get; init; }
}
```

**Regel:**

- Interface hat `{ get; set; }` → DTO bekommt `{ get; set; }` (mutable)
- Interface hat `{ get; init; }` → DTO bekommt `{ get; init; }` (init-only)
- Interface hat nur `{ get; }` → DTO bekommt `{ get; init; }` (readonly after construction)

**Warum wichtig?**

- Erfüllt Interface-Contract
- Ermöglicht Mutability wo nötig (z.B. Position-Updates)
- Verhindert Compilation Errors: `'CharacterEntityDto' does not implement interface member 'IEntity.Position.set'`

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
// Generated by Mmo.Generators.DtoGenerator
// Source: Mmo.Shared.Character.Interfaces.ICharacterEntity

#nullable enable

using MessagePack;
using System;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Entities.Dtos;

namespace Mmo.Shared.Character.Interfaces;

/// <summary>
/// Auto-generated DTO for <see cref="ICharacterEntity"/>.
/// </summary>
[MessagePackObject]
public sealed class CharacterEntityDto : ICharacterEntity, EntityDtoUnion
{
    [SerializationConstructor]
    public CharacterEntityDto() { }

    public CharacterEntityDto(ICharacterEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);
        PersistentId = source.PersistentId;
        DisplayName = source.DisplayName;
        Level = source.Level;
        Position = source.Position;
    }

    [Key(0)] public Guid PersistentId { get; init; }
    [Key(1)] public string DisplayName { get; init; }
    [Key(2)] public int Level { get; init; }
    [Key(3)] public Position Position { get; set; }  // 'set' weil Interface 'set' hat
    // Experience, Gold, AccountId sind NICHT enthalten (ServerOnly)
}

/// <summary>
/// Extension methods for CharacterEntityDto.
/// </summary>
public static class CharacterEntityDtoExtensions
{
    /// <summary>
    /// Converts an ICharacterEntity to CharacterEntityDto.
    /// </summary>
    public static CharacterEntityDto ToDto(this ICharacterEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new CharacterEntityDto(source);
    }

    /// <summary>
    /// Converts a list of ICharacterEntity to List&lt;CharacterEntityDto&gt;.
    /// </summary>
    public static List<CharacterEntityDto> ToDtoList(this IEnumerable<ICharacterEntity> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Select(x => x.ToDto()).ToList();
    }
}

/// <summary>
/// Extension methods for EntityDtoUnion.
/// </summary>
public static class EntityDtoUnionExtensions
{
    /// <summary>
    /// Converts an IEntity to EntityDtoUnion (polymorphic).
    /// </summary>
    public static EntityDtoUnion ToUnionDto(this IEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return source switch
        {
            ICharacterEntity x => x.ToDto(),
            INpcEntity x => x.ToDto(),
            _ => throw new ArgumentException($"Unknown IEntity type: {source.GetType().Name}")
        };
    }

    /// <summary>
    /// Converts a list of IEntity to List&lt;EntityDtoUnion&gt;.
    /// </summary>
    public static List<EntityDtoUnion> ToUnionDtoList(this IEnumerable<IEntity> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Select(x => x.ToUnionDto()).ToList();
    }
}
```

### Manuelle DTOs löschen

Wenn der Generator funktioniert, können manuell erstellte DTOs **gelöscht** werden:

```
VORHER (manuell):
Mmo.Shared/
├── Character/
│   ├── Interfaces/
│   │   └── ICharacterEntity.cs
│   └── Dtos/
│       └── CharacterEntityDto.cs    ← LÖSCHEN!

NACHHER (generiert):
Mmo.Shared/
├── Character/
│   └── Interfaces/
│       └── ICharacterEntity.cs      ← [GenerateDto]
│
└── obj/generated/
    └── CharacterEntityDto.g.cs      ← AUTO-GENERIERT
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

    // Convenience Properties (nicht serialisiert)
    [IgnoreMember] public bool IsPvPEnabled => Flags.HasFlag(ZoneFlags.PvpEnabled);
    [IgnoreMember] public bool IsInstance => Flags.HasFlag(ZoneFlags.IsInstance);
    [IgnoreMember] public bool IsCapital => Flags.HasFlag(ZoneFlags.IsCapital);
    [IgnoreMember] public bool IsSanctuary => Flags.HasFlag(ZoneFlags.NoCombat);
    [IgnoreMember] public bool HasRestXp => Flags.HasFlag(ZoneFlags.HasRestXp);
}

/// <summary>
/// Extension methods for Zone to DTO conversion.
/// </summary>
public static class ZoneDtoExtensions
{
    public static ZoneDto ToDto(this Zone zone) => new()
    {
        ZoneId = zone.Id,
        Name = zone.Name,
        Flags = zone.Flags,
        RecommendedMinLevel = zone.RecommendedMinLevel,
        RecommendedMaxLevel = zone.RecommendedMaxLevel,
        ControllingFaction = zone.ControllingFaction,
        DefaultSpawnPoint = zone.DefaultSpawnPoint,
        GraveyardPosition = zone.GraveyardPosition,
        MusicId = zone.MusicId,
        AmbienceId = zone.AmbienceId,
        Bounds = zone.Bounds
    };
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
  │           ▼ zone.ToDto()          │
  │  ZoneDto (nur bei erstem Besuch)  │
  │  ├── Name, Flags, Bounds          │
  │  └── KEINE EntityIds              │
  │           │                       │
  │  CharacterEntity (Server-State)   │
  │  ├── Experience = 45000           │
  │  ├── Gold = 1250                  │
  │  └── AccountId = xxx              │
  │           │                       │
  │           ▼ character.ToDto()     │
  │  CharacterEntityDto               │
  │  ├── Level, Health, etc.          │
  │  └── KEINE sensiblen Daten        │
  │           │                       │
  │           ▼ ZoneState             │
  │  ZoneState                        │
  │  ├── ZoneInfo = ZoneDto?          │
  │  └── MyPlayer = CharacterEntityDto│
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
│  INTERFACES - Contracts für DTOs                                │
│                                                                  │
│  ICharacterEntity.cs                                             │
│  ├── Properties (get; set; oder get; init;)                     │
│  ├── [GenerateDto] für Generator                                │
│  ├── [DtoUnionMember] für Polymorphismus                        │
│  └── Definiert Netzwerk-Contract                                │
└─────────────────────────────────────────────────────────────────┘
           │                              
           ▼                              
┌─────────────────────────────────────────────────────────────────┐
│  DOMAIN ENTITIES - Implementieren Interfaces                    │
│                                                                  │
│  CharacterEntity.cs                                              │
│  ├── implements ICharacterEntity                                │
│  ├── Business-Logik (LevelUp, TakeDamage, etc.)                │
│  ├── [ServerOnly] für geschützte Properties                     │
│  └── Validierung                                                │
└─────────────────────────────────────────────────────────────────┘
           │                              │
           ▼                              ▼
┌─────────────────────┐    ┌─────────────────────────────────────┐
│  DTOs (Network)      │    │  DB Entities (Persistence)          │
│  [MessagePackObject] │    │  [Table], [Column] (EF Core)        │
│                      │    │                                     │
│  CharacterEntityDto  │    │  CharacterDbEntity                  │
│  ├── [Key(0-n)]      │    │  ├── [Key] Id                       │
│  ├── Nur Client-     │    │  ├── [Column] Level                 │
│  │   relevante Daten │    │  ├── [Column] Experience            │
│  └── .ToDto()        │    │  └── ToEntity() / FromEntity()      │
└─────────────────────┘    └─────────────────────────────────────┘
           │                              │
           ▼                              ▼
      Netzwerk                       Datenbank
   (MessagePack)                  (SQL Server/PostgreSQL)
```

| Schicht         | Klasse              | Attribute                                 | Zweck                        |
| --------------- | ------------------- | ----------------------------------------- | ---------------------------- |
| **Interface**   | `ICharacterEntity`  | `[GenerateDto]`, `[DtoUnionMember]`       | DTO Contract                 |
| **Domain**      | `CharacterEntity`   | `[ServerOnly]` für einzelne Properties    | Business-Logik, Server-State |
| **Network**     | `CharacterEntityDto`| `[MessagePackObject]`, `[Key(n)]`         | Client-Synchronisation       |
| **Persistence** | `CharacterDbEntity` | `[Table]`, `[Column]`                     | Datenbank (EF Core)          |

---

## 💻 Code-Beispiele

### Domain Entity mit Interface (V3 Pattern)

```csharp
namespace Mmo.Shared.Character.Interfaces;

// Interface definiert den DTO Contract
[GenerateDto(DtoName = "CharacterEntityDto")]
[DtoUnionMember(0, typeof(IEntity))]
public interface ICharacterEntity : ICombatEntity
{
    Guid CharacterId { get; init; }
    string DisplayName { get; init; }
    int Level { get; init; }
    Position Position { get; set; }  // Mutable für Client-Updates
    Race Race { get; init; }
    CharacterClass Class { get; init; }
    int CurrentHealth { get; set; }
    int MaxHealth { get; init; }
}

// Concrete Entity implementiert Interface
public class CharacterEntity : CombatEntity, ICharacterEntity
{
    // Interface Properties
    public Guid CharacterId { get; set; }
    public string DisplayName { get; set; } = "";
    public int Level { get; set; }
    public Position Position { get; set; }
    public Race Race { get; set; }
    public CharacterClass Class { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    // Server-Only Properties (nicht im Interface)
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

### Server-Side: Entity → DTO (mit Extension Methods)

```csharp
// Entity auf dem Server
var character = new CharacterEntity
{
    CharacterId = Guid.NewGuid(),
    DisplayName = "Alice",
    Level = 10,
    Experience = 45000,  // ServerOnly
    Gold = 1250,         // ServerOnly
    AccountId = accountId // ServerOnly
};

// DTO für Client erstellen (Extension Method)
var dto = character.ToDto();

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

### Polymorphe Entity-Listen (mit ToUnionDtoList)

```csharp
// Server hat gemischte Entity-Liste
List<IEntity> entities = zone.GetEntities();
// entities enthält: CharacterEntity, NpcEntity, ObjectEntity, etc.

// Konvertiert zu polymorphem DTO-Union
List<EntityDtoUnion> entityDtos = entities.ToUnionDtoList();

var zoneState = new ZoneState
{
    ZoneId = 1001,
    Entities = entityDtos,  // Polymorphe Liste
    // ...
};
```

### Client-Side: DTO empfangen

```csharp
// Message empfangen
var zoneState = MessageSerializer.Deserialize<ZoneState>(bytes);

// DTO ist type-safe
CharacterEntityDto? myPlayer = zoneState.MyPlayer as CharacterEntityDto;

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

| DTO                   | Source               | Generiert  | Verwendet in                                |
| --------------------- | -------------------- | ---------- | ------------------------------------------- |
| `CharacterEntityDto`  | `ICharacterEntity`   | ✅ Ja (V3) | ZoneState, PlayerJoinedZone                 |
| `NpcEntityDto`        | `INpcEntity`         | ✅ Ja (V3) | ZoneState, EntitySpawn                      |
| `EntityDtoUnion`      | `IEntity` (Union)    | ✅ Ja (V3) | ZoneState (polymorphe Listen)               |
| `ZoneDto`             | `Zone`               | ❌ Manuell | ZoneState, ZoneDiscovered, ZoneListResponse |
| `ZoneListItemDto`     | -                    | ❌ Manuell | ZoneListResponse                            |

**Legende:**

-   ✅ Implementiert (V3: Interface-basiert mit Extension Methods)
-   ❌ Manuell (nicht generiert)

---

## 🔍 Best Practices

### DO ✅

```csharp
// Interfaces mit [GenerateDto]
[GenerateDto(DtoName = "CharacterEntityDto")]
[DtoUnionMember(0, typeof(IEntity))]
public interface ICharacterEntity : ICombatEntity { }

// Concrete Entity implementiert Interface
public class CharacterEntity : BaseEntity, ICharacterEntity { }

// ServerOnly auf Concrete Entity (nicht im Interface)
[ServerOnly]
public long Gold { get; set; }

// Extension Methods verwenden (V3)
var dto = character.ToDto();
var dtoList = characters.ToDtoList();
var unionDtos = entities.ToUnionDtoList();

// Polymorphe Listen mit Union
List<EntityDtoUnion> entities = serverEntities.ToUnionDtoList();

// Structs direkt mit MessagePackObject
[MessagePackObject]
public readonly struct Position { }

// Set/Init Accessors im Interface richtig definieren
public interface IEntity
{
    Guid Id { get; init; }      // Immutable
    Position Position { get; set; }  // Mutable
}
```

### DON'T ❌

```csharp
// Keine Klassen für DTO-Generation (V2-Pattern)
[GenerateDto]
public class Entity { }  // ❌ Use interface instead

// Entity direkt serialisieren
byte[] bytes = MessagePackSerializer.Serialize(entity); // ❌

// Keine manuellen Static Methods mehr (V2-Pattern)
var dto = PlayerEntityDto.FromPlayerEntity(entity); // ❌ Use .ToDto()

// Kein ApplyTo() mehr (V3 entfernt)
dto.ApplyTo(entity); // ❌ DTOs sind one-way Server→Client

// Properties manuell kopieren
var dto = new CharacterEntityDto
{
    DisplayName = entity.DisplayName,
    // ... vergisst leicht Properties
}; // ❌

// ServerOnly Daten im DTO setzen
dto.Experience = entity.Experience; // ❌ Compile Error (Property existiert nicht)

// Falsche Union-Hierarchie
[DtoUnionMember(0, typeof(IWrongRoot))]  // ❌ Root muss [GenerateDtoUnion] haben
public interface IEntity { }
```

---

## 🔗 Verwandte Dokumentation

-   **[Zone Messages](01-zone. md)** - ZoneState mit DTOs
-   **[Message Reference](Message-Reference.md)** - Übersicht aller Messages
-   **[Security](../Architecture/SECURITY.md)** - Warum ServerOnly wichtig ist

---

**Letzte Aktualisierung:** 2025-12-28  
**Version:** 3.0.0  
**Maintainer:** 2DMMO Team

Source: docs/03-messages/DTO_ARCHITECTURE.md
