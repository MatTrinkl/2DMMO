# 🎮 Game Design Document (GDD)

## 2DMMO – High Fantasy MMO

**Version:** 0.2.0  
**Letzte Aktualisierung:** 2025-12-01  
**Status:** In Entwicklung

---

## 📋 Inhaltsverzeichnis

1. [Vision & Überblick](#1-vision--überblick)
2. [Technische Architektur](#2-technische-architektur)
3. [Gameplay-Systeme](#3-gameplay-systeme)
4. [Rassen & Klassen](#4-rassen--klassen)
5. [Welt-Design](#5-welt-design)
6. [Progression](#6-progression)
7. [Datenbank-Schema](#7-datenbank-schema)
8. [Art Direction & UI](#8-art-direction--ui)
9. [Meilensteine](#9-meilensteine)
10. [Offene Fragen](#10-offene-fragen)

---

## 1. Vision & Überblick

### 1.1 Elevator Pitch

> Ein 2D Top-Down MMO in einer High-Fantasy-Welt, inspiriert von Klassikern wie World of Warcraft und Guild Wars. Spieler erkunden eine lebendige Welt voller verschiedenster Völker, bekämpfen Monster, schließen sich Gilden an und erleben epische Abenteuer – alles in einem charmanten hochauflösenden Pixel-Art-Stil.

### 1.2 Kernfeatures

| Feature | Beschreibung | Status |
|---------|--------------|--------|
| **Multiplayer-Welt** | Unbegrenzte Spieleranzahl durch Zone-Sharding | 🔄 In Planung |
| **Rassen & Klassen** | Vielfältige spielbare Völker und Klassen | 📝 Konzept |
| **Kampfsystem** | Klassisches Tank/Healer/DPS-System | 📝 Konzept |
| **Persistente Welt** | Alle Fortschritte werden in Postgres gespeichert | 🔄 In Planung |
| **Zonen-basierte Welt** | Dynamisch ladende Zonen (WoW-Style) | 📝 Konzept |
| **PvP-Flagging** | Optionales PvP durch Flagging-System | 📝 Konzept |

### 1.3 Zielgruppe

- Fans klassischer MMORPGs
- Spieler, die nostalgischen 2D-Grafikstil schätzen
- Casual bis Mid-Core Spieler

### 1.4 Unique Selling Points (USPs)

1. **Vielfalt der Völker** – Weit mehr als nur Menschen und klassische Fantasy-Rassen
2. **2D-Charme** – Hochauflösende Pixel Art (64x64) mit modernem Gameplay
3. **Skalierbarkeit** – Von Anfang an auf große Spielerzahlen ausgelegt

---

## 2. Technische Architektur

### 2.1 Tech-Stack

| Komponente | Technologie | Version |
|-----------|-------------|---------|
| **Game Client** | Godot Engine | 4.3 (.NET Edition) |
| **Client-Sprache** | C# | 14 |
| **Game Server** | .NET | 10 |
| **Shared Library** | .NET Class Library | 10 |
| **Datenbank** | PostgreSQL | 16+ |
| **Cloud-Hosting** | Microsoft Azure | - |
| **Netzwerk-Protokoll** | TCP/WebSocket | - |

### 2.2 Architektur-Übersicht

```
┌─────────────────────────────────────────────────────────────────┐
│                         AZURE CLOUD                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │ Zone Server │  │ Zone Server │  │ Zone Server │   ...        │
│  │  (Zone A)   │  │  (Zone B)   │  │  (Zone C)   │              │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘              │
│         │                │                │                      │
│         └────────────────┼────────────────┘                      │
│                          │                                       │
│                 ┌────────┴────────┐                              │
│                 │  Load Balancer  │                              │
│                 │  (Zone Router)  │                              │
│                 └────────┬────────┘                              │
│                          │                                       │
│                 ┌────────┴────────┐                              │
│                 │   PostgreSQL    │                              │
│                 │   (Persistenz)  │                              │
│                 └─────────────────┘                              │
└─────────────────────────────────────────────────────────────────┘
                           │
              ┌────────────┼────────────┐
              │            │            │
         ┌────┴────┐  ┌────┴────┐  ┌────┴────┐
         │ Client  │  │ Client  │  │ Client  │
         │(Godot)  │  │(Godot)  │  │(Godot)  │
         └─────────┘  └─────────┘  └─────────┘
```

### 2.3 Zone-Sharding-Konzept

```
┌─────────────────────────────────────────────────────────┐
│                    SPIELWELT                             │
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐               │
│  │ Zone A   │  │ Zone B   │  │ Zone C   │               │
│  │ (Wald)   │──│ (Stadt)  │──│ (Wüste)  │               │
│  │ Shard 1  │  │ Shard 1  │  │ Shard 1  │               │
│  │ Shard 2  │  │ Shard 2  │  │          │               │
│  │ Shard 3  │  │          │  │          │               │
│  └──────────┘  └──────────┘  └──────────┘               │
│                                                          │
│  → Shards werden dynamisch erstellt bei hoher Last      │
│  → Spieler können zwischen Shards wechseln              │
│  → Gilden/Gruppen werden bevorzugt auf gleichem Shard   │
└─────────────────────────────────────────────────────────┘
```

### 2.4 Netzwerk-Nachrichten (Shared DTOs)

Bereits implementiert/geplant:
- `PlayerPositionUpdate` – Spielerposition synchronisieren
- `ChatMessage` – Chat-Nachrichten
- `ActionRequest/ActionResult` – Kampf-Aktionen

---

## 3. Gameplay-Systeme

### 3.1 Core Gameplay Loop

```
┌─────────────────────────────────────────────────────────┐
│                                                          │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐             │
│   │ ERKUNDEN│───▶│ KÄMPFEN │───▶│  LOOTEN │             │
│   └─────────┘    └─────────┘    └─────────┘             │
│        ▲                              │                  │
│        │         ┌─────────┐          │                  │
│        │         │ LEVELN/ │          │                  │
│        └─────────│ UPGRADEN│◀─────────┘                  │
│                  └─────────┘                             │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Kampfsystem

**Typ:** Klassisches Tab-Targeting (vorerst)

| Rolle | Beschreibung | Beispiel-Klassen |
|-------|--------------|------------------|
| **Tank** | Zieht Aggro, hohe Rüstung, schützt die Gruppe | Krieger, Paladin |
| **Healer** | Heilt Verbündete, Buffs, Debuff-Entfernung | Priester, Druide |
| **DPS** | Hoher Schaden, Melee oder Ranged | Magier, Schurke, Jäger |

**Kampf-Ablauf:**
1. Spieler wählt Ziel (Tab oder Klick)
2. Spieler aktiviert Fähigkeit (Hotbar)
3. Client sendet `ActionRequest` an Server
4. Server validiert (Range, Cooldown, Mana, etc.)
5. Server berechnet Schaden/Effekt
6. Server sendet `ActionResult` an alle betroffenen Clients
7. Clients zeigen Visualisierung (Animation, Damage-Zahlen)

### 3.3 PvP-System

**Typ:** Flagging-System (Optional PvP)

| Status | Beschreibung | Regeln |
|--------|--------------|--------|
| **Unflagged** | PvP deaktiviert (Standard) | Kann nicht angegriffen werden, kann nicht angreifen |
| **Flagged** | PvP aktiviert | Kann von anderen Flagged-Spielern angegriffen werden | 

**Flagging-Regeln:**
- Spieler kann PvP-Flag jederzeit aktivieren (sofort aktiv)
- Deaktivierung erst nach 5 Minuten ohne Kampf möglich
- Angriff auf Flagged-Spieler flaggt automatisch
- Spezielle PvP-Zonen können automatisches Flagging erzwingen

### 3.4 Tod & Respawn-System

```
┌─────────────────────────────────────────────────────────┐
│                    SPIELER STIRBT                        │
│                          │                               │
│                          ▼                               │
│              ┌───────────────────────┐                   │
│              │   Geist-Modus aktiv   │                   │
│              │  (Unsichtbar, kann    │                   │
│              │   nicht interagieren) │                   │
│              └───────────┬───────────┘                   │
│                          │                               │
│         ┌────────────────┼────────────────┐              │
│         ▼                                 ▼              │
│  ┌──────────────┐                 ┌──────────────┐       │
│  │ GEISTERLAUF  │                 │  FRIEDHOF-   │       │
│  │              │                 │  RESPAWN     │       │
│  │ Laufe zur    │                 │              │       │
│  │ Leiche zurück│                 │ Sofort am    │       │
│  │              │                 │ Friedhof     │       │
│  │ ✓ Kein Debuff│                 │              │       │
│  │ ✓ Volle HP   │                 │ ✗ Debuff:    │       │
│  └──────────────┘                 │   "Schwäche" │       │
│                                   │   (2 Min)    │       │
│                                   │ ✗ 50% HP     │       │
│                                   └──────────────┘       │
└─────────────────────────────────────────────────────────┘
```

**Open World:**
- **Option A: Geisterlauf** – Spieler läuft als Geist zur Leiche, volle Wiederbelebung
- **Option B: Friedhof-Respawn** – Sofort am nächsten Friedhof mit Debuff "Schwäche" (2 Min, -25% Stats)

**Instanzen & Raids:**
- Respawn immer am Instanz-Eingang
- Kein Geisterlauf möglich
- Gruppe kann wipen und neu starten

### 3.5 Geplante Systeme (Post-Prototyp)

- [ ] Gruppen-System (5er Gruppen)
- [ ] Dungeons (instanzierte Bereiche)
- [ ] PvP-Arenen
- [ ] Crafting
- [ ] Auktionshaus
- [ ] Achievements
- [ ] Mounts

---

## 4. Rassen & Klassen

### 4.1 Spielbare Rassen (Phase 1 – Klassisch)

| Rasse | Beschreibung | Rassen-Bonus (Idee) |
|-------|--------------|---------------------|
| **Menschen** | Vielseitig, gute Diplomaten | +5% XP-Gewinn |
| **Elfen** | Magisch begabt, langlebig | +5% Mana |
| **Zwerge** | Robust, Meister-Handwerker | +5% Rüstung |
| **Orks** | Kriegerisch, stark | +5% Melee-Schaden |
| **Halblinge** | Flink, glücklich | +5% Ausweichen |

### 4.2 Geplante Rassen (Phase 2+)

> Die Welt soll vielfältiger sein als klassische Fantasy. Geplante Erweiterungen:

- Tiermenschen (Katzenvolk, Echsenmenschen, etc.)
- Elementarwesen
- Feenvolk
- Untote (spielbar?)
- Konstrukte/Golems
- *Weitere basierend auf Lore-Entwicklung*

### 4.3 Klassen (Phase 1)

| Klasse | Rolle | Waffen | Kern-Mechanik |
|--------|-------|--------|---------------|
| **Krieger** | Tank/DPS | Schwert, Schild, Axt | Rage-System |
| **Magier** | DPS | Stab, Zauberbuch | Mana, Elementar-Schaden |
| **Priester** | Healer | Stab, Symbol | Mana, Heil- & Schutz-Zauber |
| **Schurke** | DPS | Dolche, Wurfwaffen | Energie, Combo-Punkte |
| **Jäger** | DPS | Bogen, Fallen | Fokus, Pet-System |

### 4.4 Rassen-Klassen-Matrix

> Noch offen – wird im Laufe der Entwicklung definiert.

---

## 5. Welt-Design

### 5.1 Welt-Struktur

Die Spielwelt besteht aus **dynamisch ladenden Zonen**, ähnlich wie in World of Warcraft.

```
                    ┌─────────────────┐
                    │   STARTGEBIET   │
                    │   (Level 1-10)  │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
     ┌────────┴────┐  ┌──────┴──────┐  ┌────┴────────┐
     │ WALDGEBIET  │  │  HAUPTSTADT │  │  KÜSTE      │
     │ (Level 5-15)│  │  (Hub)      │  │ (Level 5-15)│
     └──────┬──────┘  └──────┬──────┘  └──────┬──────┘
            │                │                │
            └────────────────┼────────────────┘
                             │
                    ┌────────┴────────┐
                    │   HOCHLEVEL-    │
                    │   GEBIETE       │
                    │   (Level 15+)   │
                    └─────────────────┘
```

### 5.2 Zonen-Typen

| Zonen-Typ | Beschreibung | Beispiele |
|-----------|--------------|-----------|
| **Startgebiete** | Rassen-spezifisch, Tutorial | Menschendorf, Elfenwald |
| **Hauptstädte** | Soziale Hubs, Händler, Gilden | Hauptstadt des Reiches |
| **Levelgebiete** | Quests, Monster, Erkundung | Düsterer Wald, Wüste |
| **Dungeons** | Instanziert, Gruppen-Content | Verfluchte Mine |
| **PvP-Zonen** | Automatisches PvP-Flagging | Grenzlande |

### 5.3 Zonen-Übergang

1. Spieler nähert sich Zonen-Grenze
2. Client lädt neue Zone im Hintergrund vor
3. Bei Übertritt: Handoff zum neuen Zone-Server
4. Nahtloser Übergang (kein Ladebildschirm wenn möglich)

### 5.4 Fraktionen

> **Status:** Noch offen – abhängig von Lore-Entwicklung

Mögliche Optionen:
- Keine Fraktionen (alle Spieler neutral)
- 2 Fraktionen (klassisch)
- Gilden-basierte Fraktionen (Sandbox)

---

## 6. Progression

### 6.1 Level-System

| Level-Range | Phase | Inhalte |
|-------------|-------|---------|
| 1-10 | Tutorial | Startgebiet, Basis-Mechaniken |
| 11-30 | Leveling | Hauptstory, erste Dungeons |
| 31-50 | Endgame-Vorbereitung | Gruppen-Content, Crafting |
| 50 | Endgame | Raids, PvP, Gear-Grind |

**XP-Quellen:**
- Monster besiegen
- Quests abschließen
- Dungeons/Raids
- Erkundung (Entdeckungs-XP)

### 6.2 Gear-System

**Qualitätsstufen:**

| Farbe | Qualität | Drop-Quelle |
|-------|----------|-------------|
| ⬜ Grau | Schrott | Überall |
| ⬛ Weiß | Normal | Normale Monster |
| 🟩 Grün | Ungewöhnlich | Elite-Monster, Quests |
| 🟦 Blau | Selten | Dungeon-Bosse |
| 🟪 Lila | Episch | Raid-Bosse |
| 🟧 Orange | Legendär | Spezielle Events/Quests |

**Ausrüstungs-Slots:**
- Kopf, Schultern, Brust, Hände, Beine, Füße
- Haupthand, Nebenhand/Schild
- 2x Ringe, 1x Amulett
- Umhang, Gürtel

### 6.3 Stat-System

| Stat | Wirkung | Primär für |
|------|---------|------------|
| **Stärke** | Melee-Schaden, Block | Krieger |
| **Beweglichkeit** | Crit, Ausweichen | Schurke, Jäger |
| **Intelligenz** | Spell-Schaden, Mana | Magier |
| **Willenskraft** | Heilkraft, Mana-Reg | Priester |
| **Ausdauer** | HP | Alle (Tanks besonders) |
| **Rüstung** | Schadensreduktion | Tanks |

---

## 7. Datenbank-Schema

### 7.1 Entity-Relationship-Diagramm (vereinfacht)

```
┌─────────────┐       ┌─────────────────┐       ┌─────────────┐
│   ACCOUNT   │       │    CHARACTER    │       │    ITEM     │
├─────────────┤       ├─────────────────┤       ├─────────────┤
│ id (PK)     │───┐   │ id (PK)         │   ┌───│ id (PK)     │
│ email       │   │   │ account_id (FK) │◀──┘   │ name        │
│ password    │   └──▶│ name            │       │ type        │
│ created_at  │       │ race            │       │ rarity      │
│ last_login  │       │ class           │       │ stats (JSON)│
│ is_banned   │       │ level           │       └─────────────┘
└─────────────┘       │ xp              │              │
                      │ position_x      │              │
                      │ position_y      │       ┌──────┴──────┐
                      │ position_zone   │       │  INVENTORY  │
                      │ pvp_flagged     │       ├─────────────┤
                      │ stats (JSON)    │       │ char_id(FK) │
                      │ created_at      │◀──────│ item_id(FK) │
                      └─────────────────┘       │ slot        │
                             │                  │ quantity    │
                             │                  └─────────────┘
                      ┌──────┴──────┐
                      │    GUILD    │
                      ├─────────────┤
                      │ id (PK)     │
                      │ name        │
                      │ leader_id   │
                      │ created_at  │
                      └─────────────┘
```

### 7.2 Tabellen (Phase 1 - Prototyp)

#### `accounts`
```sql
CREATE TABLE accounts (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email           VARCHAR(255) UNIQUE NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login      TIMESTAMP,
    is_banned       BOOLEAN DEFAULT FALSE
);
```

#### `characters`
```sql
CREATE TABLE characters (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id      UUID REFERENCES accounts(id) ON DELETE CASCADE,
    name            VARCHAR(50) UNIQUE NOT NULL,
    race            VARCHAR(20) NOT NULL,
    class           VARCHAR(20) NOT NULL,
    level           INTEGER DEFAULT 1,
    xp              BIGINT DEFAULT 0,
    position_x      FLOAT DEFAULT 0,
    position_y      FLOAT DEFAULT 0,
    position_zone   VARCHAR(50) DEFAULT 'starting_zone',
    current_hp      INTEGER,
    max_hp          INTEGER,
    current_mana    INTEGER,
    max_mana        INTEGER,
    pvp_flagged     BOOLEAN DEFAULT FALSE,
    stats           JSONB,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    played_time     INTERVAL DEFAULT '0 seconds'
);
```

### 7.3 Tabellen (Phase 2+)

```sql
-- Inventar
CREATE TABLE inventory (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    character_id    UUID REFERENCES characters(id) ON DELETE CASCADE,
    item_id         UUID REFERENCES items(id),
    slot            INTEGER NOT NULL,
    quantity        INTEGER DEFAULT 1,
    UNIQUE(character_id, slot)
);

-- Items (Template-Tabelle)
CREATE TABLE items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100) NOT NULL,
    type            VARCHAR(50) NOT NULL,
    subtype         VARCHAR(50),
    rarity          VARCHAR(20) DEFAULT 'common',
    level_req       INTEGER DEFAULT 1,
    stats           JSONB,
    icon            VARCHAR(255)
);

-- Gilden
CREATE TABLE guilds (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(50) UNIQUE NOT NULL,
    leader_id       UUID REFERENCES characters(id),
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    bank_gold       BIGINT DEFAULT 0
);

-- Gilden-Mitglieder
CREATE TABLE guild_members (
    guild_id        UUID REFERENCES guilds(id) ON DELETE CASCADE,
    character_id    UUID REFERENCES characters(id) ON DELETE CASCADE,
    rank            VARCHAR(20) DEFAULT 'member',
    joined_at       TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (guild_id, character_id)
);
```

---

## 8. Art Direction & UI

### 8.1 Grafik-Stil

**Typ:** Hochauflösende Pixel Art (64x64 Basis)

| Element | Spezifikation |
|---------|---------------|
| **Tile-Größe** | 64x64 Pixel |
| **Charakter-Sprites** | 64x64 Pixel (mit Animationen) |
| **Farbpalette** | Reich und vielfältig, Fantasy-inspiriert |
| **Animationen** | Smooth, mind. 8 Frames für Bewegung |

**Referenzen:**
- Höhere Auflösung als klassische 16-bit
- Detailliert aber noch klar als Pixel Art erkennbar
- Moderne Beleuchtung/Shader möglich

### 8.2 Audio-Konzept

> **Status:** Später zu definieren

**Richtlinien:**
- Stil muss zur Pixel Art passen
- Kann orchestral mit leichten Retro-Elementen sein
- Oder vollständig Chiptune/Synth (je nach Vision)

### 8.3 UI-Design

**Stil:** Modern Pixel Art UI

| Element | Beschreibung |
|---------|--------------|
| **Allgemein** | Clean, modern, aber im Pixel-Art-Stil |
| **Hotbar** | Unten zentriert (klassisches MMO-Layout) |
| **Minimap** | Oben rechts, eckig mit Pixel-Rahmen |
| **Chat** | Unten links, semi-transparent |
| **Inventar** | Grid-basiert, Pixel-Art-Icons |

**Referenzen:**
- Moderne Indie-RPGs mit Pixel Art UI
- Skalierbar für verschiedene Auflösungen
- Accessibility-freundlich (Lesbarkeit!)

---

## 9. Meilensteine

### Phase 1: Prototyp (Aktuell)
> **Ziel:** Zwei Spieler verbinden sich, sehen sich, können sich bewegen

- [x] Projekt-Struktur aufsetzen
- [x] Client-Server-Grundkommunikation
- [ ] Spieler-Bewegung synchronisieren
- [ ] Einfache Tilemap-Welt
- [ ] Basis-Chat

### Phase 2: Core Gameplay
> **Ziel:** Spielbarer Gameplay-Loop

- [ ] Rassen- und Klassenwahl
- [ ] Kampfsystem (Basis)
- [ ] Monster-Spawning
- [ ] Inventar-System
- [ ] Persistenz (Postgres)
- [ ] Tod/Respawn-System
- [ ] PvP-Flagging

### Phase 3: Content
> **Ziel:** Spielbare Demo mit 1-10 Leveln

- [ ] Startgebiet mit Quests
- [ ] Erste 5 Rassen/Klassen
- [ ] Erster Dungeon
- [ ] Gilden-System

### Phase 4: Polish & Scale
> **Ziel:** Beta-Release

- [ ] Zone-Sharding
- [ ] Load Balancing
- [ ] Azure-Deployment
- [ ] Performance-Optimierung

---

## 10. Offene Fragen

> Diese Fragen müssen im Laufe der Entwicklung geklärt werden:

### Gameplay
- [x] ~~Soll PvP optional oder verpflichtend sein?~~ → Flagging-System
- [x] ~~Wie funktioniert das Respawn-System?~~ → Geisterlauf + Friedhof-Option
- [ ] Soll es Fraktionen geben? → Abhängig von Lore

### Technisch
- [ ] WebSocket oder reines TCP für die Kommunikation?
- [ ] Wie oft werden Positionen synchronisiert? (Tick-Rate?)
- [ ] Caching-Strategie für häufige DB-Zugriffe? (Redis?)

### Design
- [x] ~~Grafik-Stil?~~ → Hochauflösende Pixel Art (64x64)
- [ ] Audio-Konzept? → Passend zu Pixel Art (Details offen)
- [x] ~~UI-Design-Richtlinien?~~ → Modern Pixel Art UI

---

## 📝 Änderungshistorie

| Version | Datum | Änderungen |
|---------|-------|------------|
| 0.1.0 | 2025-12-01 | Erstes GDD erstellt (Prototyp-Fokus) |
| 0.2.0 | 2025-12-01 | PvP-Flagging, Respawn-System, Art Direction hinzugefügt |

---

*Dieses Dokument ist ein lebendes Dokument und wird kontinuierlich erweitert.*
