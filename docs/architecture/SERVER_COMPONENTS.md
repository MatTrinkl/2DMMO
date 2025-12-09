# 🖥️ Server-Komponenten

## 2DMMO – Server Architektur

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](../ARCHITECTURE.md)

---

## 📋 Übersicht

Das 2DMMO verwendet eine mehrschichtige Server-Architektur mit Gateway Servern für Verbindungen und Zone Servern für die Spiellogik.

---

## Gateway Server

**Verantwortung:** Erste Anlaufstelle für alle Client-Verbindungen.

```
┌─────────────────────────────────────────────────────────┐
│                    GATEWAY SERVER                        │
│                                                          │
│  Aufgaben:                                              │
│  ├─ TCP+TLS Verbindungen akzeptieren                   │
│  ├─ Authentication (Login-Validierung)                 │
│  ├─ Session-Erstellung in Redis                        │
│  ├─ Routing zu richtigem Zone Server                   │
│  ├─ Connection Health Monitoring                       │
│  └─ Rate Limiting (DoS-Schutz)                         │
│                                                          │
│  Skalierung: Horizontal (mehrere Gateway Instanzen)    │
│  Stateless: Ja (alle Daten in Redis)                   │
└─────────────────────────────────────────────────────────┘
```

### Eigenschaften

| Eigenschaft | Wert |
|-------------|------|
| **Stateless** | Ja |
| **Skalierung** | Horizontal |
| **Session Storage** | Redis |
| **Load Balancing** | Azure Load Balancer |

---

## Zone Server

**Verantwortung:** Verwaltet eine oder mehrere Zonen der Spielwelt.

```
┌─────────────────────────────────────────────────────────┐
│                     ZONE SERVER                          │
│                                                          │
│  Aufgaben:                                              │
│  ├─ Game Loop (25 Hz)                                  │
│  ├─ Spieler-Bewegung validieren                        │
│  ├─ Kampf-Logik                                        │
│  ├─ NPC/Monster AI                                     │
│  ├─ Loot & Drops                                       │
│  ├─ Position-Broadcasting                              │
│  └─ Zone-Chat                                          │
│                                                          │
│  Skalierung: Pro Zone, mit Sharding bei hoher Last     │
│  Stateful: Ja (Zone-State im Memory + Redis Sync)      │
└─────────────────────────────────────────────────────────┘
```

### Eigenschaften

| Eigenschaft | Wert |
|-------------|------|
| **Stateful** | Ja (Zone-State im Memory) |
| **Skalierung** | Vertikal + Sharding |
| **Tick-Rate** | 25 Hz |
| **Sync** | Redis für Cross-Zone |

---

## Komponenten-Kommunikation

```
┌─────────────────────────────────────────────────────────┐
│              KOMPONENTEN-KOMMUNIKATION                   │
│                                                          │
│  Client ──TCP+TLS──► Gateway                            │
│                         │                                │
│                         ▼                                │
│  Gateway ──Redis Pub/Sub──► Zone Server                 │
│                                                          │
│  Zone A ──Redis Pub/Sub──► Zone B  (Cross-Zone Events) │
│                                                          │
│  Zone Server ──SQL──► PostgreSQL (Persistenz)           │
│                                                          │
│  Alle Server ◄──► Redis (Session, Cache, Pub/Sub)       │
└─────────────────────────────────────────────────────────┘
```

### Kommunikationswege

| Von | Zu | Protokoll | Zweck |
|-----|-----|-----------|-------|
| Client | Gateway | TCP+TLS | Spieler-Verbindung |
| Gateway | Zone Server | Redis Pub/Sub | Routing, Events |
| Zone Server | Zone Server | Redis Pub/Sub | Cross-Zone Events |
| Zone Server | PostgreSQL | SQL | Persistenz |
| Alle | Redis | Redis Protocol | Session, Cache |

---

## Verwandte Dokumentation

- [ID-System](ID_SYSTEM.md) - Entity Identity, ZoneId, ShardId
- [Netzwerk-Protokoll](NETWORK_PROTOCOL.md) - Transport und Connection Flow
- [Game Loop](GAME_LOOP.md) - Server Tick Timing
- [Redis-Strategie](REDIS.md) - Session und Cache
- [Azure Deployment](AZURE_DEPLOYMENT.md) - Container Apps Deployment

---

## 🔗 Nützliche Links

- [.NET TCP Server](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services)
- [Redis Pub/Sub](https://redis.io/docs/manual/pubsub/)
- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)

---

*Zurück zur [Architektur-Übersicht](../ARCHITECTURE.md)*