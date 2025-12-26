# 🏗️ Architektur-Dokumentation

## 2DMMO – Technische Architektur

**Version:** 2.1.0  
**Letzte Aktualisierung:** 2025-12-26  
**Status:** Finalisiert für Prototyp-Phase + S2S-Planung + Login-to-Play Flow

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
| 7 | [Login-to-Play Flow](LOGIN_TO_PLAY_FLOW.md) | **NEU** - Kompletter Message Flow von Login bis Ready-to-Play |
| 8 | [ID-System](ID_SYSTEM.md) | Entity Identity, GlobalKey, ZoneId Ranges |
| 9 | [Redis-Strategie](REDIS.md) | Key Schema, Caching, Pub/Sub |
| 10 | [Datenbank-Strategie](DATABASE.md) | PostgreSQL, Write-Strategien, Pooling |
| 11 | [Sicherheit](SECURITY.md) | Security Layers, Input Validation |
| 12 | [Rate-Limiting](RATE_LIMITING.md) | Rate-Limit Tiers, Algorithmen, Anti-Spam, Anti-DoS |
| 13 | [Azure Deployment](AZURE_DEPLOYMENT.md) | Container Apps, Networking, Services |
| 14 | [Skalierung](SCALING.md) | Zone Sharding, Metriken, Auto-Scaling |
| 15 | [Server-zu-Server Kommunikation](SERVER_TO_SERVER.md) | S2S Messages, Load-Balancing, Multi-Server Architektur |

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
| 2.1.0 | 2025-12-26 | Hinzugefügt: Login-to-Play Flow Dokumentation mit GetZone Messages |
| 2.0.0 | 2025-12-25 | Hinzugefügt: Server-zu-Server Kommunikation & Load-Balancing Dokumentation |
| 1.4.0 | 2025-12-23 | Hinzugefügt: Rate-Limiting Dokumentation |
| 1.3.0 | 2025-12-22 | Hinzugefügt: Handler/Service-Pattern Dokumentation |
| 1.2.0 | 2025-12-09 | Hinzugefügt: ID-System Dokumentation |
| 1.1.0 | 2025-12-02 | Refactoring: Aufteilung in Unterseiten |
| 1.0.0 | 2025-12-02 | Initiale Architektur-Dokumentation |

---

*Diese Übersicht verweist auf die detaillierte Architektur-Dokumentation. Bei Fragen oder Ergänzungen bitte ein Issue erstellen.*
