# 🏗️ Architektur-Dokumentation

## 2DMMO – Technische Architektur

**Version:** 1.3.0  
**Letzte Aktualisierung:** 2025-12-22  
**Status:** Finalisiert für Prototyp-Phase

---

## 📋 Übersicht

Diese Dokumentation beschreibt die technische Architektur des 2DMMO-Projekts. Die detaillierte Dokumentation ist in folgende Unterseiten aufgeteilt:

### Inhaltsverzeichnis

| # | Dokument | Beschreibung |
|---|----------|--------------|
| 1 | [Server-Komponenten](SERVER_COMPONENTS.md) | Gateway, Zone Server, Kommunikation |
| 2 | [Netzwerk-Protokoll](NETWORK_PROTOCOL.md) | Transport, Message Framing, Connection Flow |
| 3 | [Message-Spezifikation](MESSAGES.md) | Message Types, DTOs, Serialization |
| 4 | [Handler/Service-Pattern](HANDLER_SERVICE_PATTERN.md) | Message Handling, Business Logic, Async Operations |
| 5 | [Game Loop Design](GAME_LOOP.md) | Server Game Loop, Tick Timing |
| 6 | [Client-Server Sync](CLIENT_SERVER_SYNC.md) | Prediction, Interpolation, Reconciliation |
| 7 | [ID-System](ID_SYSTEM.md) | Entity Identity, GlobalKey, ZoneId Ranges |
| 8 | [Redis-Strategie](REDIS.md) | Key Schema, Caching, Pub/Sub |
| 9 | [Datenbank-Strategie](DATABASE.md) | PostgreSQL, Write-Strategien, Pooling |
| 10 | [Sicherheit](SECURITY.md) | Security Layers, Input Validation |
| 11 | [Azure Deployment](AZURE_DEPLOYMENT.md) | Container Apps, Networking, Services |
| 12 | [Skalierung](SCALING.md) | Zone Sharding, Metriken, Auto-Scaling |

---

## Technologie-Stack

| Komponente | Technologie | Version |
|-----------|-------------|---------|
| **Game Client** | Godot Engine (.NET) | 4.3 |
| **Programmiersprache** | C# | 14 |
| **Server Runtime** | .NET | 10 |
| **Transport** | TCP + TLS | - |
| **Serialisierung** | MessagePack | Latest |
| **Cache** | Redis | 7+ |
| **Datenbank** | PostgreSQL | 16+ |
| **Cloud** | Microsoft Azure | - |

---

## Architektur-Diagramm

```
┌────────────────────────────────────────────────────────────────────────────┐
│                              AZURE CLOUD                                    │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │                         GATEWAY LAYER                                  │ │
│  │                                                                        │ │
│  │    ┌──────────────┐         ┌──────────────┐         ┌──────────────┐ │ │
│  │    │   Gateway    │         │   Gateway    │         │   Gateway    │ │ │
│  │    │   Server 1   │         │   Server 2   │         │   Server N   │ │ │
│  │    │   (TCP+TLS)  │         │   (TCP+TLS)  │         │   (TCP+TLS)  │ │ │
│  │    └──────┬───────┘         └──────┬───────┘         └──────┬───────┘ │ │
│  │           │                        │                        │         │ │
│  └───────────┼────────────────────────┼────────────────────────┼─────────┘ │
│              │                        │                        │           │
│              └────────────────────────┼────────────────────────┘           │
│                                       │                                     │
│  ┌────────────────────────────────────┼────────────────────────────────┐   │
│  │                         REDIS CLUSTER                                │   │
│  │    ┌─────────────────────────────────────────────────────────────┐  │   │
│  │    │  • Session Store      • Zone Player Lists                   │  │   │
│  │    │  • Player Cache       • Pub/Sub (Cross-Zone Events)         │  │   │
│  │    │  • Zone Registry      • Rate Limiting                       │  │   │
│  │    └─────────────────────────────────────────────────────────────┘  │   │
│  └────────────────────────────────────┬────────────────────────────────┘   │
│                                       │                                     │
│              ┌────────────────────────┼────────────────────────┐           │
│              │                        │                        │           │
│  ┌───────────┼────────────────────────┼────────────────────────┼─────────┐ │
│  │           │       ZONE SERVER LAYER                         │         │ │
│  │  ┌────────┴─────┐  ┌───────┴────────┐  ┌────────┴─────┐              │ │
│  │  │ Zone Server  │  │  Zone Server   │  │ Zone Server  │              │ │
│  │  │              │  │                │  │              │              │ │
│  │  │  Startzone   │  │   Hauptstadt   │  │    Wald      │    ...       │ │
│  │  │              │  │                │  │              │              │ │
│  │  │ ┌─────────┐  │  │  ┌─────────┐   │  │ ┌─────────┐  │              │ │
│  │  │ │ Shard 1 │  │  │  │ Shard 1 │   │  │ │ Shard 1 │  │              │ │
│  │  │ │ Shard 2 │  │  │  │ Shard 2 │   │  │ │ Shard 2 │  │              │ │
│  │  │ │ Shard 3 │  │  │  └─────────┘   │  │ └─────────┘  │              │ │
│  │  │ └─────────┘  │  │                │  │ └─────────┘  │              │ │
│  │  └──────────────┘  └────────────────┘  └──────────────┘              │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
│                                       │                                     │
│                          ┌────────────┴────────────┐                       │
│                          │      PostgreSQL         │                       │
│                          │   (Persistente Daten)   │                       │
│                          └─────────────────────────┘                       │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔗 Nützliche Links

### Dokumentation
- [Game Design Document](../01-overview/GAME_DESIGN_DOCUMENT.md)
- [Technical Design](../03-technical-details/TECHNICAL_DESIGN.md)
- [Prototype Scope](../01-overview/PROTOTYPE_SCOPE.md)
- [Issue Updates Guide](../04-project-management/ISSUE_UPDATES_GUIDE.md) - Issue updates und Zone-Konzept Integration

### Externe Ressourcen
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Godot Engine Docs](https://docs.godotengine.org/)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)
- [Redis Documentation](https://redis.io/documentation)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)

---

## 📝 Änderungshistorie

| Version | Datum | Änderungen |
|---------|-------|------------|
| 1.3.0 | 2025-12-22 | Hinzugefügt: Handler/Service-Pattern Dokumentation |
| 1.2.0 | 2025-12-09 | Hinzugefügt: ID-System Dokumentation |
| 1.1.0 | 2025-12-02 | Refactoring: Aufteilung in Unterseiten |
| 1.0.0 | 2025-12-02 | Initiale Architektur-Dokumentation |

---

*Diese Übersicht verweist auf die detaillierte Architektur-Dokumentation. Bei Fragen oder Ergänzungen bitte ein Issue erstellen.*
