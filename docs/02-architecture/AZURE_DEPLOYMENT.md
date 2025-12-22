# ☁️ Azure Deployment

## 2DMMO – Cloud Infrastructure

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](README.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt das Azure Deployment für das 2DMMO, einschließlich verwendeter Services, Networking und Kosten-Optimierung.

---

## Azure Services

| Komponente | Azure Service | Tier |
|-----------|---------------|------|
| Gateway Server | Azure Container Apps | Consumption |
| Zone Server | Azure Container Apps | Dedicated |
| Redis | Azure Cache for Redis | Standard C1+ |
| PostgreSQL | Azure Database for PostgreSQL | Flexible Server |
| Load Balancer | Azure Load Balancer | Standard |
| Monitoring | Application Insights | - |
| Secrets | Azure Key Vault | Standard |

---

## Deployment-Diagramm

```
┌─────────────────────────────────────────────────────────┐
│                    AZURE DEPLOYMENT                      │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │              AZURE FRONT DOOR                    │    │
│  │           (DDoS Protection, WAF)                │    │
│  └────────────────────────┬────────────────────────┘    │
│                           │                              │
│  ┌────────────────────────┼────────────────────────┐    │
│  │         AZURE CONTAINER APPS ENVIRONMENT         │    │
│  │                        │                         │    │
│  │    ┌───────────────────┼───────────────────┐    │    │
│  │    │                   │                   │    │    │
│  │    ▼                   ▼                   ▼    │    │
│  │ ┌──────┐           ┌──────┐           ┌──────┐ │    │
│  │ │Gate- │           │Gate- │           │Gate- │ │    │
│  │ │way 1 │           │way 2 │           │way N │ │    │
│  │ └──┬───┘           └──┬───┘           └──┬───┘ │    │
│  │    │                  │                  │     │    │
│  │    ├──────────────────┼──────────────────┤     │    │
│  │    │                  │                  │     │    │
│  │    ▼                  ▼                  ▼     │    │
│  │ ┌──────┐          ┌──────┐          ┌──────┐  │    │
│  │ │Zone  │          │Zone  │          │Zone  │  │    │
│  │ │Start │          │Stadt │          │Wald  │  │    │
│  │ └──────┘          └──────┘          └──────┘  │    │
│  └─────────────────────────────────────────────────┘    │
│                           │                              │
│            ┌──────────────┼──────────────┐              │
│            │              │              │              │
│            ▼              ▼              ▼              │
│  ┌──────────────┐  ┌────────────┐  ┌────────────┐      │
│  │ Azure Cache  │  │  Azure DB  │  │  Key Vault │      │
│  │  for Redis   │  │ PostgreSQL │  │  (Secrets) │      │
│  └──────────────┘  └────────────┘  └────────────┘      │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## Service-Konfiguration

### Azure Container Apps

| Server-Typ | Plan | Min Replicas | Max Replicas |
|------------|------|--------------|--------------|
| Gateway | Consumption | 1 | 10 |
| Zone Server | Dedicated | 1 | 5 |

### Azure Cache for Redis

| Tier | Cache Size | Beschreibung |
|------|------------|--------------|
| Standard C1 | 1 GB | Prototyp-Phase |
| Standard C2 | 2.5 GB | Production |

### Azure PostgreSQL

| Tier | vCores | Storage |
|------|--------|---------|
| Burstable B1ms | 1 | 32 GB |

---

## Networking

- **Azure Front Door**: DDoS Protection, WAF
- **Private Endpoints**: Redis und PostgreSQL nicht öffentlich erreichbar
- **VNet Integration**: Container Apps im gleichen VNet

---

## Verwandte Dokumentation

- [Server-Komponenten](SERVER_COMPONENTS.md) - Gateway und Zone Server
- [Skalierung](SCALING.md) - Auto-Scaling Konfiguration
- [Redis-Strategie](REDIS.md) - Redis Cache Details
- [Datenbank-Strategie](DATABASE.md) - PostgreSQL Details

---

## 🔗 Nützliche Links

- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)
- [Azure Cache for Redis](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/)
- [Azure Database for PostgreSQL](https://learn.microsoft.com/en-us/azure/postgresql/)
- [Azure Front Door](https://learn.microsoft.com/en-us/azure/frontdoor/)

---

*Teil der [Architektur-Dokumentation](../ARCHITECTURE.md)*
