# 🌍 World Messages (2600-2699)

**Kategorie:** 26  
**Range:** 2600-2651  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

1. [World Flow](#-world-flow)
   - [Server-Authoritative Architektur](#server-authoritative-architektur)
   - [Wetter- und Zeitsystem Flow](#wetter--und-zeitsystem-flow)
   - [World Event Flow](#world-event-flow)
2. [DTOs und Enums](#-dtos-und-enums)
   - [WeatherType Enum](#weathertype-enum)
   - [SeasonType Enum](#seasontype-enum)
   - [MoonPhaseType Enum](#moonphasetype-enum)
   - [WorldEventType Enum](#worldeventtype-enum)
   - [WorldErrorCode Enum](#worlderrorcode-enum)
   - [WeatherInfoDto](#weatherinfodto)
   - [WorldEventDto](#worldeventdto)
   - [HolidayInfoDto](#holidayinfodto)
   - [ServerFirstDto](#serverfirstdto)
   - [Wichtige Konstanten](#wichtige-konstanten)
3. [Messages](#-messages)
   - [Wetter/Zeit (2600-2606)](#wetterzeit-2600-2606)
   - [World Events (2620-2626)](#world-events-2620-2626)
   - [Holidays (2630-2632)](#holidays-2630-2632)
   - [Server Firsts (2640-2641)](#server-firsts-2640-2641)
   - [Territory Control (2650-2651)](#territory-control-2650-2651)
4. [Anhang](#-anhang)
   - [MessageType Enum](#messagetype-enum)
   - [Request/Response Paare](#requestresponse-paare)

---

## 🔄 World Flow

### Server-Authoritative Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    WORLD STATE SYSTEM                            │
│                                                                  │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐         │
│  │   Weather   │    │    Time     │    │   Events    │         │
│  │   Manager   │    │   Manager   │    │   Manager   │         │
│  └──────┬──────┘    └──────┬──────┘    └──────┬──────┘         │
│         │                  │                  │                 │
│         └─────────────┬────┴────┬─────────────┘                 │
│                       │         │                               │
│                       ▼         ▼                               │
│              ┌────────────────────────┐                         │
│              │    World State Hub     │                         │
│              │  (Server-Authoritative)│                         │
│              └───────────┬────────────┘                         │
│                          │                                      │
│            ┌─────────────┼─────────────┐                        │
│            ▼             ▼             ▼                        │
│    ┌───────────┐ ┌───────────┐ ┌───────────┐                   │
│    │  Zone 1   │ │  Zone 2   │ │  Zone N   │                   │
│    │ Broadcast │ │ Broadcast │ │ Broadcast │                   │
│    └───────────┘ └───────────┘ └───────────┘                   │
└─────────────────────────────────────────────────────────────────┘
```

### Wetter- und Zeitsystem Flow

```
Server                                     Client
  │                                          │
  │  WeatherUpdate (2600)                    │
  │─────────────────────────────────────────►│
  │  (WeatherType, Intensity, Duration)      │
  │                                          │  Update weather visuals
  │                                          │  Adjust ambient sounds
  │                                          │
  │  TimeOfDayUpdate (2602)                  │
  │─────────────────────────────────────────►│
  │  (Hour, Minute, DayPhase)                │
  │                                          │  Update lighting
  │                                          │  Toggle NPC schedules
  │                                          │
  │  SeasonChange (2606)                     │
  │─────────────────────────────────────────►│
  │  (NewSeason, TransitionDuration)         │
  │                                          │  Swap zone textures
  │                                          │  Adjust spawn tables
```

### World Event Flow

```
Server                                     All Clients
  │                                          │
  │  WorldEventStart (2620)                  │
  │══════════════════════════════════════════►  (Broadcast)
  │  (EventId, EventType, StartTime)         │
  │                                          │  Show event banner
  │                                          │  Enable event UI
  │                                          │
  │  WorldEventProgress (2622)               │
  │═══════════════════════════════════════════►  (Periodic)
  │  (EventId, Progress%, Objectives)        │
  │                                          │  Update progress bar
  │                                          │
  │  WorldBossSpawn (2624)                   │
  │══════════════════════════════════════════►  (Broadcast)
  │  (BossId, ZoneId, SpawnPosition)         │
  │                                          │  Show boss marker
  │                                          │  Play alert sound
  │                                          │
  │  WorldBossKill (2625)                    │
  │══════════════════════════════════════════►  (Broadcast)
  │  (BossId, KillerGuild, Participants)     │
  │                                          │  Show kill credit
  │                                          │  Distribute loot
  │                                          │
  │  WorldEventEnd (2621)                    │
  │══════════════════════════════════════════►  (Broadcast)
  │  (EventId, FinalResults, Rewards)        │
  │                                          │  Show summary
  │                                          │  Grant rewards
```

---

## 🧱 DTOs und Enums

### WeatherType Enum

```csharp
public enum WeatherType : byte
{
    Clear = 0,         // Klarer Himmel
    Cloudy = 1,        // Bewölkt
    Rain = 2,          // Regen
    HeavyRain = 3,     // Starkregen
    Thunderstorm = 4,  // Gewitter
    Snow = 5,          // Schnee
    Blizzard = 6,      // Schneesturm
    Fog = 7,           // Nebel
    Sandstorm = 8,     // Sandsturm (Wüstenzonen)
    Ash = 9            // Ascheregen (Vulkanzonen)
}
```

### SeasonType Enum

```csharp
public enum SeasonType : byte
{
    Spring = 0,   // Frühling - erhöhte Spawn-Raten
    Summer = 1,   // Sommer - längere Tage
    Autumn = 2,   // Herbst - Ernte-Events
    Winter = 3    // Winter - Schnee, Feiertags-Events
}
```

### MoonPhaseType Enum

```csharp
public enum MoonPhaseType : byte
{
    NewMoon = 0,        // Neumond - erhöhte Undead-Aktivität
    WaxingCrescent = 1, // Zunehmende Sichel
    FirstQuarter = 2,   // Erstes Viertel
    WaxingGibbous = 3,  // Zunehmender Mond
    FullMoon = 4,       // Vollmond - Werwolf-Events
    WaningGibbous = 5,  // Abnehmender Mond
    LastQuarter = 6,    // Letztes Viertel
    WaningCrescent = 7  // Abnehmende Sichel
}
```

### WorldEventType Enum

```csharp
public enum WorldEventType : byte
{
    Invasion = 0,      // Feindliche Invasion
    Harvest = 1,       // Ernte-Festival
    Tournament = 2,    // PvP-Turnier
    WorldBoss = 3,     // World-Boss Spawn
    Treasure = 4,      // Schatzjagd
    Defense = 5,       // Stadtverteidigung
    Race = 6,          // Rennen/Wettbewerb
    Gathering = 7,     // Sammel-Wettbewerb
    Holiday = 8        // Feiertags-Event
}
```

### WorldErrorCode Enum

```csharp
public enum WorldErrorCode : byte
{
    None = 0,
    EventNotActive = 1,       // Event nicht aktiv
    EventFull = 2,            // Event voll
    LevelTooLow = 3,          // Level zu niedrig
    ZoneNotAvailable = 4,     // Zone nicht verfügbar
    AlreadyParticipating = 5, // Bereits teilnehmend
    CooldownActive = 6,       // Cooldown aktiv
    InvalidObjective = 7,     // Ungültiges Objective
    BossNotSpawned = 8,       // Boss nicht gespawnt
    TerritoryLocked = 9,      // Territory gesperrt
    FactionRequired = 10,     // Fraktion erforderlich
    NoPermission = 11,        // Keine Berechtigung
    EventEnded = 12,          // Event beendet
    QuestRequired = 13,       // Quest erforderlich
    ItemRequired = 14,        // Item erforderlich
    ServerError = 15          // Server-Fehler
}
```

### WeatherInfoDto

```csharp
[MessagePackObject]
public class WeatherInfoDto
{
    [Key(0)] public WeatherType CurrentWeather { get; set; }
    [Key(1)] public byte Intensity { get; set; }        // 0-100
    [Key(2)] public int DurationSeconds { get; set; }   // Verbleibende Dauer
    [Key(3)] public WeatherType NextWeather { get; set; }
    [Key(4)] public int TransitionSeconds { get; set; } // Übergangszeit
    [Key(5)] public float WindSpeed { get; set; }       // 0.0-1.0
    [Key(6)] public float WindDirection { get; set; }   // 0-360 Grad
}
```

### WorldEventDto

```csharp
[MessagePackObject]
public class WorldEventDto
{
    [Key(0)] public int EventId { get; set; }
    [Key(1)] public WorldEventType EventType { get; set; }
    [Key(2)] public string EventName { get; set; }
    [Key(3)] public string Description { get; set; }
    [Key(4)] public int ZoneId { get; set; }
    [Key(5)] public long StartTimeUtc { get; set; }
    [Key(6)] public long EndTimeUtc { get; set; }
    [Key(7)] public byte ProgressPercent { get; set; }
    [Key(8)] public int MinLevel { get; set; }
    [Key(9)] public int MaxParticipants { get; set; }
    [Key(10)] public int CurrentParticipants { get; set; }
    [Key(11)] public List<ObjectiveDto> Objectives { get; set; }
}
```

### HolidayInfoDto

```csharp
[MessagePackObject]
public class HolidayInfoDto
{
    [Key(0)] public int HolidayId { get; set; }
    [Key(1)] public string HolidayName { get; set; }
    [Key(2)] public string Description { get; set; }
    [Key(3)] public long StartTimeUtc { get; set; }
    [Key(4)] public long EndTimeUtc { get; set; }
    [Key(5)] public List<int> SpecialQuestIds { get; set; }
    [Key(6)] public List<int> SpecialItemIds { get; set; }
    [Key(7)] public int EventZoneId { get; set; }       // 0 = global
    [Key(8)] public string IconAsset { get; set; }
}
```

### ServerFirstDto

```csharp
[MessagePackObject]
public class ServerFirstDto
{
    [Key(0)] public int AchievementId { get; set; }
    [Key(1)] public string AchievementName { get; set; }
    [Key(2)] public string PlayerName { get; set; }
    [Key(3)] public string GuildName { get; set; }      // null wenn Solo
    [Key(4)] public long AchievedTimeUtc { get; set; }
    [Key(5)] public byte Category { get; set; }         // 0=Boss, 1=Dungeon, 2=Craft, etc.
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `WEATHER_UPDATE_INTERVAL` | 60s | Weather-Broadcast Intervall |
| `TIME_SYNC_INTERVAL` | 300s | Zeit-Sync Intervall |
| `DAY_CYCLE_MINUTES` | 120 | Echtzeit-Minuten pro In-Game-Tag |
| `SEASON_DAYS` | 28 | In-Game-Tage pro Season |
| `MOON_CYCLE_DAYS` | 8 | In-Game-Tage pro Mondzyklus |
| `EVENT_BROADCAST_INTERVAL` | 30s | Event Progress Broadcast |
| `WORLD_BOSS_ANNOUNCE_LEAD` | 300s | Boss-Ankündigung vor Spawn |
| `TERRITORY_CAPTURE_TIME` | 600s | Zeit zum Erobern eines Territory |

---

## 📩 Messages

### Wetter/Zeit (2600-2606)

---

### WeatherUpdate (2600)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Wetter-Änderung + periodisch (60s)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert Client über aktuelles Wetter in der Zone. Wird bei Wetter-Änderungen und periodisch zur Synchronisation gesendet.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Betroffene Zone | Ja |
| Weather | WeatherInfoDto | Wetter-Informationen | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var weatherUpdate = new WeatherUpdate
{
    Type = MessageType.WeatherUpdate,
    ZoneId = 101,
    Weather = new WeatherInfoDto
    {
        CurrentWeather = WeatherType.Rain,
        Intensity = 60,
        DurationSeconds = 1800,
        NextWeather = WeatherType.Cloudy,
        TransitionSeconds = 120,
        WindSpeed = 0.4f,
        WindDirection = 225.0f
    },
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

#### Notizen
- Wetter beeinflusst Sichtweite und bestimmte Abilities
- Bei Thunderstorm: Chance auf Lightning-Damage im Freien
- Bei Fog: Reduzierte Aggro-Range von NPCs

---

### WeatherForecast (2601)

**Richtung:** 📥 Server → Client  
**Frequenz:** Auf Anfrage / bei Zone-Enter  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Sendet Wetter-Vorhersage für die nächsten In-Game-Stunden. Ermöglicht Spielern, Aktivitäten zu planen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Betroffene Zone | Ja |
| Forecasts | List\<WeatherInfoDto\> | Vorhersage (nächste 6h) | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var forecast = new WeatherForecast
{
    Type = MessageType.WeatherForecast,
    ZoneId = 101,
    Forecasts = new List<WeatherInfoDto>
    {
        new() { CurrentWeather = WeatherType.Rain, Intensity = 60, DurationSeconds = 3600 },
        new() { CurrentWeather = WeatherType.Cloudy, Intensity = 30, DurationSeconds = 3600 },
        new() { CurrentWeather = WeatherType.Clear, Intensity = 0, DurationSeconds = 7200 }
    },
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### TimeOfDayUpdate (2602)

**Richtung:** 📥 Server → Client  
**Frequenz:** Alle 5 In-Game-Minuten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Aktualisiert die Tageszeit für den Client. Steuert Beleuchtung, NPC-Schedules und zeit-abhängige Events.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Hour | byte | Stunde (0-23) | Ja |
| Minute | byte | Minute (0-59) | Ja |
| DayPhase | byte | 0=Night, 1=Dawn, 2=Day, 3=Dusk | Ja |
| DayOfWeek | byte | 0-6 (für Weekly Events) | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var timeUpdate = new TimeOfDayUpdate
{
    Type = MessageType.TimeOfDayUpdate,
    Hour = 14,
    Minute = 30,
    DayPhase = 2, // Day
    DayOfWeek = 3,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

#### Notizen
- Dawn: 05:00-07:00 (Sonnenaufgang-Effekte)
- Day: 07:00-18:00 (volle Helligkeit)
- Dusk: 18:00-21:00 (Sonnenuntergang-Effekte)
- Night: 21:00-05:00 (reduzierte Sicht, Undead-Spawns)

---

### TimeOfDaySync (2603)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Zone-Enter + periodisch (300s)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Vollständige Zeit-Synchronisation. Enthält alle Zeit-Daten für Client-Interpolation.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GameTime | long | In-Game Zeit in Sekunden | Ja |
| RealTimeRatio | float | Echtzeit-zu-Spielzeit Ratio | Ja |
| DayNumber | int | Aktueller In-Game Tag | Ja |
| Season | SeasonType | Aktuelle Jahreszeit | Ja |
| MoonPhase | MoonPhaseType | Aktuelle Mondphase | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var timeSync = new TimeOfDaySync
{
    Type = MessageType.TimeOfDaySync,
    GameTime = 52200,  // 14:30 in Sekunden
    RealTimeRatio = 12.0f,  // 12x schneller als Echtzeit
    DayNumber = 142,
    Season = SeasonType.Summer,
    MoonPhase = MoonPhaseType.FullMoon,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### DayNightCycle (2604)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Zone-Enter  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Informiert über Zone-spezifische Tag/Nacht-Einstellungen. Manche Zonen haben fixe Tageszeiten (z.B. ewige Nacht in Undead-Zonen).

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Betroffene Zone | Ja |
| HasDayNightCycle | bool | Hat Zone normalen Zyklus? | Ja |
| FixedHour | byte | Fixe Stunde wenn !HasCycle | Bedingt |
| DawnStart | byte | Stunde für Dawn-Start | Bedingt |
| DuskStart | byte | Stunde für Dusk-Start | Bedingt |
| AmbientMultiplier | float | Helligkeits-Multiplikator | Ja |

#### Beispiel Payload
```csharp
// Normale Zone
var normalCycle = new DayNightCycle
{
    Type = MessageType.DayNightCycle,
    ZoneId = 101,
    HasDayNightCycle = true,
    DawnStart = 5,
    DuskStart = 19,
    AmbientMultiplier = 1.0f
};

// Ewige Nacht Zone
var darkZone = new DayNightCycle
{
    Type = MessageType.DayNightCycle,
    ZoneId = 666,
    HasDayNightCycle = false,
    FixedHour = 0,  // Mitternacht
    AmbientMultiplier = 0.3f
};
```

---

### MoonPhase (2605)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Phasenwechsel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Informiert über Mondphasen-Wechsel. Beeinflusst bestimmte Events und Spawn-Tabellen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Phase | MoonPhaseType | Neue Mondphase | Ja |
| PhaseDurationHours | int | Dauer in In-Game-Stunden | Ja |
| SpecialEffects | byte | Bitflags für Spezialeffekte | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### SpecialEffects Bitflags
| Bit | Effekt |
|-----|--------|
| 0x01 | Werwolf-Spawns aktiv |
| 0x02 | Erhöhte Kräuter-Erträge |
| 0x04 | Geister-NPCs sichtbar |
| 0x08 | Nacht-Quests verfügbar |

#### Beispiel Payload
```csharp
var moonPhase = new MoonPhase
{
    Type = MessageType.MoonPhase,
    Phase = MoonPhaseType.FullMoon,
    PhaseDurationHours = 24,
    SpecialEffects = 0x01 | 0x04,  // Werwölfe + Geister
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### SeasonChange (2606)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Bei Saisonwechsel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Informiert über Jahreszeiten-Wechsel. Client aktualisiert Zone-Texturen und Ambient-Sounds.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewSeason | SeasonType | Neue Jahreszeit | Ja |
| TransitionDurationSeconds | int | Übergangszeit | Ja |
| SeasonDayNumber | int | Tag innerhalb der Season | Ja |
| SpecialEvents | List\<int\> | Aktive saisonale Event-IDs | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var seasonChange = new SeasonChange
{
    Type = MessageType.SeasonChange,
    NewSeason = SeasonType.Winter,
    TransitionDurationSeconds = 300,
    SeasonDayNumber = 1,
    SpecialEvents = new List<int> { 1001, 1002 },  // Winter-Events
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### World Events (2620-2626)

---

### WorldEventStart (2620)

**Richtung:** 📡 Broadcast  
**Frequenz:** Bei Event-Start  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Kündigt den Start eines World Events an. Alle Spieler in relevanten Zonen erhalten diese Nachricht.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Event | WorldEventDto | Event-Details | Ja |
| JoinInstructions | string | Wie man teilnimmt | Ja |
| RewardPreview | List\<int\> | Mögliche Belohnungs-Item-IDs | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var eventStart = new WorldEventStart
{
    Type = MessageType.WorldEventStart,
    Event = new WorldEventDto
    {
        EventId = 5001,
        EventType = WorldEventType.Invasion,
        EventName = "Goblin-Invasion",
        Description = "Die Goblins greifen die Handelsroute an!",
        ZoneId = 105,
        StartTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        EndTimeUtc = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds(),
        MinLevel = 20,
        MaxParticipants = 100
    },
    JoinInstructions = "Reise zur Handelsroute in Zone 105",
    RewardPreview = new List<int> { 50001, 50002, 50003 },
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### WorldEventEnd (2621)

**Richtung:** 📡 Broadcast  
**Frequenz:** Bei Event-Ende  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Informiert über das Ende eines World Events. Enthält finale Ergebnisse und Belohnungen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | int | Event-ID | Ja |
| Success | bool | Event erfolgreich abgeschlossen? | Ja |
| FinalProgress | byte | Finaler Fortschritt (0-100) | Ja |
| TopContributors | List\<ContributorDto\> | Top 10 Teilnehmer | Ja |
| TotalParticipants | int | Gesamtteilnehmer | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var eventEnd = new WorldEventEnd
{
    Type = MessageType.WorldEventEnd,
    EventId = 5001,
    Success = true,
    FinalProgress = 100,
    TopContributors = new List<ContributorDto>
    {
        new() { PlayerName = "Hero1", Contribution = 1500 },
        new() { PlayerName = "Hero2", Contribution = 1200 }
    },
    TotalParticipants = 87,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### WorldEventProgress (2622)

**Richtung:** 📥 Server → Client  
**Frequenz:** Alle 30 Sekunden während Event  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Periodisches Update zum Event-Fortschritt. Nur an teilnehmende Spieler.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | int | Event-ID | Ja |
| ProgressPercent | byte | Gesamtfortschritt (0-100) | Ja |
| TimeRemainingSeconds | int | Verbleibende Zeit | Ja |
| CurrentPhase | byte | Aktuelle Event-Phase | Ja |
| PersonalContribution | int | Eigener Beitrag | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var progress = new WorldEventProgress
{
    Type = MessageType.WorldEventProgress,
    EventId = 5001,
    ProgressPercent = 65,
    TimeRemainingSeconds = 2400,
    CurrentPhase = 2,
    PersonalContribution = 350,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### WorldEventObjective (2623)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Objective-Update  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Update zu einzelnen Event-Objectives. Zeigt detaillierten Fortschritt.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | int | Event-ID | Ja |
| ObjectiveId | int | Objective-ID | Ja |
| ObjectiveName | string | Objective-Name | Ja |
| CurrentCount | int | Aktueller Zähler | Ja |
| TargetCount | int | Ziel-Zähler | Ja |
| IsCompleted | bool | Objective abgeschlossen? | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var objective = new WorldEventObjective
{
    Type = MessageType.WorldEventObjective,
    EventId = 5001,
    ObjectiveId = 1,
    ObjectiveName = "Goblins besiegt",
    CurrentCount = 450,
    TargetCount = 500,
    IsCompleted = false,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### WorldBossSpawn (2624)

**Richtung:** 📡 Broadcast  
**Frequenz:** Bei Boss-Spawn  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Kündigt den Spawn eines World-Bosses an. Server-weiter Broadcast.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | int | Boss Entity-ID | Ja |
| BossName | string | Boss-Name | Ja |
| ZoneId | int | Spawn-Zone | Ja |
| PositionX | float | X-Koordinate | Ja |
| PositionY | float | Y-Koordinate | Ja |
| Level | int | Boss-Level | Ja |
| EstimatedDifficulty | byte | 1=Easy bis 5=Mythic | Ja |
| DespawnTimeUtc | long | Wann Boss despawnt | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var bossSpawn = new WorldBossSpawn
{
    Type = MessageType.WorldBossSpawn,
    BossId = 90001,
    BossName = "Magmaron der Feuerdrache",
    ZoneId = 150,
    PositionX = 1250.5f,
    PositionY = 3400.2f,
    Level = 60,
    EstimatedDifficulty = 4,
    DespawnTimeUtc = DateTimeOffset.UtcNow.AddHours(3).ToUnixTimeSeconds(),
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### WorldBossKill (2625)

**Richtung:** 📡 Broadcast  
**Frequenz:** Bei Boss-Kill  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Verkündet die erfolgreiche Tötung eines World-Bosses. Server-weiter Broadcast.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | int | Boss Entity-ID | Ja |
| BossName | string | Boss-Name | Ja |
| KillerGuildId | int | Guild-ID (0 wenn PUG) | Ja |
| KillerGuildName | string | Guild-Name (null wenn PUG) | Nein |
| TopDamagers | List\<string\> | Top 3 Damage-Dealer | Ja |
| TotalParticipants | int | Anzahl Teilnehmer | Ja |
| FightDurationSeconds | int | Kampfdauer | Ja |
| IsServerFirst | bool | Erster Kill auf Server? | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var bossKill = new WorldBossKill
{
    Type = MessageType.WorldBossKill,
    BossId = 90001,
    BossName = "Magmaron der Feuerdrache",
    KillerGuildId = 42,
    KillerGuildName = "Dragon Slayers",
    TopDamagers = new List<string> { "Legolas", "Gandalf", "Aragorn" },
    TotalParticipants = 40,
    FightDurationSeconds = 480,
    IsServerFirst = true,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### WorldBossAnnounce (2626)

**Richtung:** 📡 Broadcast  
**Frequenz:** 5 Minuten vor Spawn  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Vorab-Ankündigung eines World-Boss Spawns. Gibt Spielern Zeit, sich vorzubereiten.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossName | string | Boss-Name | Ja |
| ZoneId | int | Spawn-Zone | Ja |
| ZoneName | string | Zone-Name | Ja |
| SpawnInSeconds | int | Sekunden bis Spawn | Ja |
| RecommendedLevel | int | Empfohlenes Level | Ja |
| RecommendedPlayers | int | Empfohlene Spielerzahl | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var bossAnnounce = new WorldBossAnnounce
{
    Type = MessageType.WorldBossAnnounce,
    BossName = "Magmaron der Feuerdrache",
    ZoneId = 150,
    ZoneName = "Vulkan-Gipfel",
    SpawnInSeconds = 300,
    RecommendedLevel = 55,
    RecommendedPlayers = 40,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### Holidays (2630-2632)

---

### HolidayStart (2630)

**Richtung:** 📡 Broadcast  
**Frequenz:** Bei Holiday-Start  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Kündigt den Start eines Feiertags/Festivals an. Aktiviert saisonale Inhalte.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Holiday | HolidayInfoDto | Holiday-Details | Ja |
| WelcomeMessage | string | Begrüßungstext | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var holidayStart = new HolidayStart
{
    Type = MessageType.HolidayStart,
    Holiday = new HolidayInfoDto
    {
        HolidayId = 100,
        HolidayName = "Winterfest",
        Description = "Die Zeit des Schenkens ist gekommen!",
        StartTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        EndTimeUtc = DateTimeOffset.UtcNow.AddDays(14).ToUnixTimeSeconds(),
        SpecialQuestIds = new List<int> { 10001, 10002, 10003 },
        SpecialItemIds = new List<int> { 20001, 20002 },
        EventZoneId = 0,  // Global
        IconAsset = "ui/holidays/winterfest.png"
    },
    WelcomeMessage = "Frohe Festtage! Besuche den Weihnachtsmarkt in der Hauptstadt!",
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### HolidayEnd (2631)

**Richtung:** 📡 Broadcast  
**Frequenz:** Bei Holiday-Ende  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Informiert über das Ende eines Feiertags/Festivals. Deaktiviert saisonale Inhalte.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| HolidayId | int | Holiday-ID | Ja |
| HolidayName | string | Holiday-Name | Ja |
| FarewellMessage | string | Abschiedstext | Ja |
| NextOccurrence | long | Nächstes Mal (UTC) | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var holidayEnd = new HolidayEnd
{
    Type = MessageType.HolidayEnd,
    HolidayId = 100,
    HolidayName = "Winterfest",
    FarewellMessage = "Das Winterfest endet. Bis zum nächsten Jahr!",
    NextOccurrence = DateTimeOffset.UtcNow.AddDays(351).ToUnixTimeSeconds(),
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### HolidayInfo (2632)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Login / Auf Anfrage  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Sendet Details zu aktiven und kommenden Holidays. Wird bei Login und auf Anfrage gesendet.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ActiveHolidays | List\<HolidayInfoDto\> | Aktive Feiertage | Ja |
| UpcomingHolidays | List\<HolidayInfoDto\> | Kommende Feiertage (nächste 30 Tage) | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var holidayInfo = new HolidayInfo
{
    Type = MessageType.HolidayInfo,
    ActiveHolidays = new List<HolidayInfoDto>
    {
        new() { HolidayId = 100, HolidayName = "Winterfest", /* ... */ }
    },
    UpcomingHolidays = new List<HolidayInfoDto>
    {
        new() { HolidayId = 101, HolidayName = "Neujahr", /* ... */ }
    },
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### Server Firsts (2640-2641)

---

### ServerFirstAnnounce (2640)

**Richtung:** 📡 Broadcast  
**Frequenz:** Bei Server-First Errungenschaft  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Server-weiter Broadcast bei einer Server-First Errungenschaft. Feiert besondere Leistungen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ServerFirst | ServerFirstDto | Details zur Errungenschaft | Ja |
| CelebrationMessage | string | Glückwunsch-Text | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var serverFirst = new ServerFirstAnnounce
{
    Type = MessageType.ServerFirstAnnounce,
    ServerFirst = new ServerFirstDto
    {
        AchievementId = 9001,
        AchievementName = "Erster Raid-Boss Kill: Magmaron",
        PlayerName = "Legolas",
        GuildName = "Dragon Slayers",
        AchievedTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Category = 0  // Boss
    },
    CelebrationMessage = "🎉 WELTERSTKLASSIG! Dragon Slayers haben Magmaron als Erste bezwungen!",
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### ServerFirstList (2641)

**Richtung:** 📥 Server → Client  
**Frequenz:** Auf Anfrage  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Liste aller Server-First Errungenschaften für die Hall of Fame.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ServerFirsts | List\<ServerFirstDto\> | Alle Server-Firsts | Ja |
| TotalCount | int | Gesamtanzahl | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var serverFirstList = new ServerFirstList
{
    Type = MessageType.ServerFirstList,
    ServerFirsts = new List<ServerFirstDto>
    {
        new() { AchievementId = 9001, AchievementName = "Magmaron", PlayerName = "Legolas", GuildName = "Dragon Slayers" },
        new() { AchievementId = 9002, AchievementName = "Frostwyrm", PlayerName = "Gandalf", GuildName = "Ice Warriors" }
    },
    TotalCount = 2,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### Territory Control (2650-2651)

---

### ZoneControlUpdate (2650)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Status-Änderung / Periodisch  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Update zum Territory-Control Status. Zeigt welche Fraktion/Guild eine Zone kontrolliert.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Betroffene Zone | Ja |
| ControllingFactionId | int | Kontrollierende Fraktion (0=neutral) | Ja |
| ControllingGuildId | int | Kontrollierende Guild (0=keine) | Ja |
| ControllingGuildName | string | Guild-Name | Nein |
| ControlPercent | byte | Kontroll-Prozent (0-100) | Ja |
| ContestingFactionId | int | Angreifende Fraktion | Ja |
| IsContested | bool | Wird gerade umkämpft? | Ja |
| NextBattleTimeUtc | long | Nächste geplante Schlacht | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var zoneControl = new ZoneControlUpdate
{
    Type = MessageType.ZoneControlUpdate,
    ZoneId = 200,
    ControllingFactionId = 1,
    ControllingGuildId = 42,
    ControllingGuildName = "Dragon Slayers",
    ControlPercent = 85,
    ContestingFactionId = 2,
    IsContested = true,
    NextBattleTimeUtc = DateTimeOffset.UtcNow.AddHours(6).ToUnixTimeSeconds(),
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### TerritoryCapture (2651)

**Richtung:** 📡 Broadcast  
**Frequenz:** Bei Territory-Eroberung  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Verkündet die Eroberung eines Territoriums. Server-weiter Broadcast.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Eroberte Zone | Ja |
| ZoneName | string | Zone-Name | Ja |
| OldFactionId | int | Vorherige Fraktion | Ja |
| NewFactionId | int | Neue Fraktion | Ja |
| CapturingGuildId | int | Erobernde Guild | Ja |
| CapturingGuildName | string | Guild-Name | Ja |
| CaptureTimeUtc | long | Zeitpunkt der Eroberung | Ja |
| ServerTime | long | Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var capture = new TerritoryCapture
{
    Type = MessageType.TerritoryCapture,
    ZoneId = 200,
    ZoneName = "Schwarzwald-Festung",
    OldFactionId = 2,
    NewFactionId = 1,
    CapturingGuildId = 42,
    CapturingGuildName = "Dragon Slayers",
    CaptureTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

## 📎 Anhang

### MessageType Enum

Exakte Reihenfolge aus `MessageType.cs`:

```csharp
// WORLD STATE (WEATHER, TIME, EVENTS) (2600-2699)
WeatherUpdate = 2600,
WeatherForecast = 2601,
TimeOfDayUpdate = 2602,
TimeOfDaySync = 2603,
DayNightCycle = 2604,
MoonPhase = 2605,
SeasonChange = 2606,
WorldEventStart = 2620,
WorldEventEnd = 2621,
WorldEventProgress = 2622,
WorldEventObjective = 2623,
WorldBossSpawn = 2624,
WorldBossKill = 2625,
WorldBossAnnounce = 2626,
HolidayStart = 2630,
HolidayEnd = 2631,
HolidayInfo = 2632,
ServerFirstAnnounce = 2640,
ServerFirstList = 2641,
ZoneControlUpdate = 2650,
TerritoryCapture = 2651,
```

### Request/Response Paare

| Request | Response | Beschreibung |
|---------|----------|--------------|
| - | - | Keine Client-Requests in dieser Kategorie |

### Fire-and-Forget Messages

Keine in dieser Kategorie.

### Server-initiated Messages (alle 21)

| Message | ID | Trigger |
|---------|-----|---------|
| WeatherUpdate | 2600 | Wetter-Änderung |
| WeatherForecast | 2601 | Zone-Enter |
| TimeOfDayUpdate | 2602 | Zeit-Tick (5 min) |
| TimeOfDaySync | 2603 | Zone-Enter / Sync |
| DayNightCycle | 2604 | Zone-Enter |
| MoonPhase | 2605 | Phasenwechsel |
| SeasonChange | 2606 | Saisonwechsel |
| WorldEventStart | 2620 | Event beginnt |
| WorldEventEnd | 2621 | Event endet |
| WorldEventProgress | 2622 | Periodisch (30s) |
| WorldEventObjective | 2623 | Objective-Update |
| WorldBossSpawn | 2624 | Boss spawnt |
| WorldBossKill | 2625 | Boss stirbt |
| WorldBossAnnounce | 2626 | 5 min vor Spawn |
| HolidayStart | 2630 | Holiday beginnt |
| HolidayEnd | 2631 | Holiday endet |
| HolidayInfo | 2632 | Login / Anfrage |
| ServerFirstAnnounce | 2640 | Server-First |
| ServerFirstList | 2641 | Auf Anfrage |
| ZoneControlUpdate | 2650 | Status-Änderung |
| TerritoryCapture | 2651 | Eroberung |

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   └── MessageType.cs          # Message IDs
│   ├── DTOs/
│   │   └── World/
│   │       ├── WeatherInfoDto.cs
│   │       ├── WorldEventDto.cs
│   │       ├── HolidayInfoDto.cs
│   │       └── ServerFirstDto.cs
│   └── Messages/
│       └── World/
│           ├── WeatherUpdate.cs
│           ├── WeatherForecast.cs
│           ├── TimeOfDayUpdate.cs
│           └── ...
└── Enums/
    ├── WeatherType.cs
    ├── SeasonType.cs
    ├── MoonPhaseType.cs
    ├── WorldEventType.cs
    └── WorldErrorCode.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (21 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/26-world.md
