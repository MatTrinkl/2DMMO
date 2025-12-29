# 📚 2DMMO Dokumentation

**Willkommen zur zentralen Dokumentation des 2DMMO-Projekts!**

Diese Dokumentation ist modular aufgebaut und in thematische Bereiche unterteilt. Jeder Bereich enthält spezifische Dokumente zu seinem Thema.

---

## 🗂️ Dokumentations-Struktur

### 01 - Übersicht & Vision
> **High-Level Projektinformationen, Gameplay-Design und Asset-Anforderungen**

| Dokument | Beschreibung |
|----------|--------------|
| [Game Design Document](01-overview/GAME_DESIGN_DOCUMENT.md) | Gameplay-Vision, Rassen, Klassen, Welt-Design, Progression |
| [Prototyp-Scope](01-overview/PROTOTYPE_SCOPE.md) | Definition des Prototyp-Umfangs und MVP-Features |
| [Assets & Ressourcen](01-overview/ASSETS.md) | Asset-Anforderungen, Quellen und Spezifikationen |

### 02 - Architektur
> **Technische System-Architektur, Netzwerk-Design und Infrastruktur**

| Dokument | Beschreibung |
|----------|--------------|
| [Architektur-Übersicht](02-architecture/README.md) | Haupt-Architektur, Tech-Stack, System-Übersicht |
| [Server-Komponenten](02-architecture/SERVER_COMPONENTS.md) | Gateway, Zone Server, Kommunikation |
| [Netzwerk-Protokoll](02-architecture/NETWORK_PROTOCOL.md) | Transport, Message Framing, Connection Flow |
| [Message-Spezifikation](02-architecture/MESSAGES.md) | Message Types, DTOs, Serialization |
| [Game Loop Design](02-architecture/GAME_LOOP.md) | Server Game Loop, Tick Timing |
| [Client-Server Sync](02-architecture/CLIENT_SERVER_SYNC.md) | Prediction, Interpolation, Reconciliation |
| [Chunk-Based Sync](02-architecture/CHUNK_BASED_SYNC.md) | **Phase 2** - AOI Delta Sync, Chunk Grid, Bandwidth Optimization |
| [ID-System](02-architecture/ID_SYSTEM.md) | Entity Identity, GlobalKey, ZoneId Ranges |
| [Zone-Daten-Architektur](02-architecture/ZONE_DATA_ARCHITECTURE.md) | ZoneBounds, CollisionData, Datenstrukturen |
| [Redis-Strategie](02-architecture/REDIS.md) | Key Schema, Caching, Pub/Sub |
| [Datenbank-Strategie](02-architecture/DATABASE.md) | PostgreSQL, Write-Strategien, Pooling |
| [Sicherheit](02-architecture/SECURITY.md) | Security Layers, Input Validation |
| [Skalierung](02-architecture/SCALING.md) | Zone Sharding, Metriken, Auto-Scaling |
| [Azure Deployment](02-architecture/AZURE_DEPLOYMENT.md) | Container Apps, Networking, Services |

### 03 - Messages & Technische Details
> **Message-Referenz und detaillierte Implementierungs-Entscheidungen**

| Dokument | Beschreibung |
|----------|--------------|
| **[📨 Message-Referenz](03-messages/README.md)** | **Vollständige Dokumentation aller 1100+ Network-Messages** |
| [Technical Design Document](03-technical-details/TECHNICAL_DESIGN.md) | Detaillierte technische Entscheidungen, Thread-Modell, Collision System |

### 04 - Projekt-Management
> **Issue-Tracking, Roadmaps und Planungsdokumente**

| Dokument | Beschreibung |
|----------|--------------|
| [Issue-Hierarchie](04-project-management/ISSUE_HIERARCHY.md) | Issue-Beziehungen und Bearbeitungsreihenfolge |
| [Issue-Aktualisierungs-Leitfaden](04-project-management/ISSUE_UPDATES_GUIDE.md) | Prozess-Guide für Issue-Updates (Templates, Best Practices) |
| [Sub-Issues](04-project-management/SUB_ISSUES.md) | Detaillierte Sub-Issue-Vorschläge für große Issues |
| [Feature-Roadmap](04-project-management/FEATURE_ROADMAP.md) | Neue Features bis Ende Phase 4 |

---

## 🎯 Schnellstart

### Für neue Entwickler
1. Start mit dem [Game Design Document](01-overview/GAME_DESIGN_DOCUMENT.md) für die Vision
2. Lies die [Architektur-Übersicht](02-architecture/README.md) für das technische Verständnis
3. Prüfe den [Prototyp-Scope](01-overview/PROTOTYPE_SCOPE.md) für den aktuellen Entwicklungsstand

### Für bestehende Entwickler
- **Architektur-Fragen**: Siehe [02-architecture/](02-architecture/)
- **Implementierungs-Details**: Siehe [Technical Design](03-technical-details/TECHNICAL_DESIGN.md)
- **Issue-Planning**: Siehe [04-project-management/](04-project-management/)

---

## 📝 Dokumentations-Konventionen

### Struktur
- Alle Dokumente verwenden **Markdown** (.md)
- Emojis in Titeln für bessere Orientierung (z.B. 🏗️, 🎮, 📡)
- Versionsnummer und Aktualisierungsdatum im Header
- Ausführliches Inhaltsverzeichnis mit Anchor-Links

### Versionierung
- **Version**: Semantic Versioning (z.B. 1.2.0)
- **Status**: `In Entwicklung` | `Prototyp-Phase` | `Finalisiert`
- **Letzte Aktualisierung**: YYYY-MM-DD Format

### Cross-Referenzen
- Relative Pfade verwenden: `[Link](../02-architecture/README.md)`
- Anchor-Links für Abschnitte: `[Link](#section-name)`

---

## 🔄 Dokumentation erweitern

### Neue Dokumente hinzufügen

1. **Kategorie wählen**: Entscheide, in welchen Ordner das Dokument gehört
   - `01-overview`: High-level, nicht-technische Informationen
   - `02-architecture`: System-Design, Architektur-Entscheidungen
   - `03-technical-details`: Implementierungs-Spezifikationen
   - `04-project-management`: Issue-Tracking, Planung

2. **Dokument erstellen**: Folge den Konventionen (siehe oben)

3. **Index aktualisieren**: Füge einen Eintrag in dieser README.md hinzu

4. **Links aktualisieren**: Prüfe und aktualisiere alle internen Verlinkungen

### Bestehende Dokumente aktualisieren

1. Versionsnummer erhöhen (falls signifikante Änderungen)
2. Aktualisierungsdatum anpassen
3. Änderungslog am Ende des Dokuments pflegen (optional)

---

## 📊 Dokumentations-Abhängigkeiten

```
GAME_DESIGN_DOCUMENT (Vision)
    ↓
ARCHITECTURE (System-Design)
    ↓
TECHNICAL_DESIGN (Implementation)
    ↓
PROTOTYPE_SCOPE (Current MVP)
    ↓
ISSUE_HIERARCHY (Task Breakdown)
```

---

## 🆘 Hilfe & Feedback

- **Fragen zur Dokumentation**: Erstelle ein Issue mit Label `documentation`
- **Fehlende Dokumentation**: Erstelle ein Issue mit Beschreibung des benötigten Inhalts
- **Verbesserungsvorschläge**: Pull Request oder Issue erstellen

---

**Letzte Aktualisierung**: 2025-12-09  
**Struktur-Version**: 2.0.0
