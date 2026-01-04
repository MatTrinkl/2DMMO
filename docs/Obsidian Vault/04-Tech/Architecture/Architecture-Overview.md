# 🏗️ Architecture Documentation

## 2DMMO – Technical Architecture

**Version:** 2.2.0  
**Last Updated:** 2026-01-01  
**Status:** Finalized for Prototype Phase + S2S Planning + Login-to-Play Flow

---

## 📋 Overview

This documentation describes the technical architecture of the 2DMMO project. The detailed documentation is divided into the following sub-pages:

### Table of Contents

| # | Document | Description |
|---|----------|-------------|
| 1 | [Server Components](SERVER_COMPONENTS.md) | Gateway, Zone Server, communication |
| 2 | [Network Protocol](NETWORK_PROTOCOL.md) | Transport, message framing, connection flow |
| 3 | [Message Specification](MESSAGES.md) | Message types, DTOs, serialization |
| 4 | [Handler/Service Pattern](HANDLER_SERVICE_PATTERN.md) | Message handling, business logic, async operations |
| 5 | [Game Loop Design](GAME_LOOP.md) | Server game loop, tick timing |
| 6 | [Client-Server Sync](CLIENT_SERVER_SYNC.md) | Prediction, interpolation, reconciliation |
| 7 | [Chunk-Based Sync](CHUNK_BASED_SYNC.md) | **Phase 2** - AOI delta sync, chunk grid, bandwidth optimization |
| 8 | [Login-to-Play Flow](LOGIN_TO_PLAY_FLOW.md) | Complete message flow from login to ready-to-play |
| 9 | [ID System](ID_SYSTEM.md) | Entity identity, GlobalKey, ZoneId ranges |
| 10 | [Redis Strategy](REDIS.md) | Key schema, caching, Pub/Sub |
| 11 | [Database Strategy](DATABASE.md) | PostgreSQL, write strategies, pooling |
| 12 | [Security](SECURITY.md) | Security layers, input validation |
| 13 | [Rate Limiting](RATE_LIMITING.md) | Rate limit tiers, algorithms, anti-spam, anti-DoS |
| 14 | [Azure Deployment](AZURE_DEPLOYMENT.md) | Container Apps, networking, services |
| 15 | [Scaling](SCALING.md) | Zone sharding, metrics, auto-scaling |
| 16 | [Server-to-Server Communication](SERVER_TO_SERVER.md) | S2S messages, load balancing, multi-server architecture |

---

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| **Game Client** | Godot Engine (.NET) | 4.3 |
| **Programming Language** | C# | 14 |
| **Server Runtime** | .NET | 10 |
| **Transport** | TCP + TLS | - |
| **Serialization** | MessagePack | Latest |
| **Cache** | Redis | 7+ |
| **Database** | PostgreSQL | 16+ |
| **Cloud** | Microsoft Azure | - |

---

## Architecture Diagram

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
│  │  │  Start Zone  │  │    Capital     │  │   Forest     │    ...       │ │
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
│                          │   (Persistent Data)     │                       │
│                          └─────────────────────────┘                       │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔗 Useful Links

### Documentation
- [Game Design Document](../../02-Game-Design/Systems/GAME_DESIGN_DOCUMENT.md)
- [Technical Design](TECHNICAL_DESIGN.md)
- [Prototype Scope](../../01-Planning/Roadmap/PROTOTYPE_SCOPE.md)
- [Issue Updates Guide](../../01-Planning/ISSUE_UPDATES_GUIDE.md) - Issue updates and zone concept integration

### External Resources
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Godot Engine Docs](https://docs.godotengine.org/)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)
- [Redis Documentation](https://redis.io/documentation)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)

---

## 📝 Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.2.0 | 2026-01-01 | Translated to English |
| 2.1.0 | 2025-12-26 | Added: Login-to-Play Flow Documentation with GetZone Messages |
| 2.0.0 | 2025-12-25 | Added: Server-to-Server Communication & Load-Balancing Documentation |
| 1.4.0 | 2025-12-23 | Added: Rate-Limiting Documentation |
| 1.3.0 | 2025-12-22 | Added: Handler/Service Pattern Documentation |
| 1.2.0 | 2025-12-09 | Added: ID System Documentation |
| 1.1.0 | 2025-12-02 | Refactoring: Split into sub-pages |
| 1.0.0 | 2025-12-02 | Initial architecture documentation |

---

*This overview references the detailed architecture documentation. For questions or additions, please create an issue.*

Source: docs/02-architecture/README.md
