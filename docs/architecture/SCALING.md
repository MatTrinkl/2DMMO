# 📈 Skalierung

## 2DMMO – Scaling Architecture

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](../ARCHITECTURE.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt die Skalierungsstrategie für das 2DMMO, einschließlich Zone Sharding, Metriken und Auto-Scaling.

---

## Zone Sharding

```
┌─────────────────────────────────────────────────────────┐
│                   ZONE SHARDING                          │
│                                                          │
│  Trigger: Zone hat > 200 Spieler                        │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │               VOR SHARDING                       │    │
│  │                                                  │    │
│  │    Zone "Wald"                                  │    │
│  │    ┌─────────────────────────────────┐          │    │
│  │    │  👤👤👤👤👤👤👤👤👤👤           │          │    │
│  │    │  👤👤👤👤👤👤👤👤👤👤           │          │    │
│  │    │  ... 250 Spieler ...            │          │    │
│  │    │  Server-Last: 95% ⚠️             │          │    │
│  │    └─────────────────────────────────┘          │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│                         │                                │
│                         ▼                                │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │               NACH SHARDING                      │    │
│  │                                                  │    │
│  │    Zone "Wald" Shard 1    Zone "Wald" Shard 2  │    │
│  │    ┌───────────────────┐  ┌───────────────────┐ │    │
│  │    │  👤👤👤👤👤        │  │  👤👤👤👤👤        │ │    │
│  │    │  👤👤👤👤👤        │  │  👤👤👤👤👤        │ │    │
│  │    │  125 Spieler      │  │  125 Spieler      │ │    │
│  │    │  Last: 50% ✅      │  │  Last: 50% ✅      │ │    │
│  │    └───────────────────┘  └───────────────────┘ │    │
│  │                                                  │    │
│  │    Spieler können Shard wechseln                │    │
│  │    Gruppen bleiben zusammen                     │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

---

## Skalierungs-Metriken

| Metrik | Threshold | Aktion |
|--------|-----------|--------|
| Zone Spieler | > 200 | Neuen Shard erstellen |
| Zone Spieler | < 50 | Shards zusammenlegen |
| Server CPU | > 80% | Scale Out |
| Server Memory | > 85% | Scale Out |
| Gateway Connections | > 5000 | Neue Gateway Instanz |
| Redis Memory | > 80% | Scale Up |
| DB Connections | > 80 | Scale Up |

---

## Auto-Scaling Regeln

### Gateway Server

| Bedingung | Aktion | Min | Max |
|-----------|--------|-----|-----|
| Connections > 5000 | Scale Out | 1 | 10 |
| CPU > 70% | Scale Out | 1 | 10 |
| Connections < 1000 | Scale In | 1 | 10 |

### Zone Server

| Bedingung | Aktion |
|-----------|--------|
| Spieler > 200 | Neuen Shard erstellen |
| Spieler < 50 | Shards zusammenlegen |
| CPU > 80% | Warnung an Admins |

---

## Shard-Management

### Shard-Erstellung

1. Neuer Shard wird gestartet
2. Spieler werden gleichmäßig verteilt
3. Gruppen/Parties bleiben zusammen
4. Cross-Shard Kommunikation via Redis Pub/Sub

### Shard-Zusammenlegung

1. Spieler werden in verbleibenden Shard migriert
2. Leerer Shard wird heruntergefahren
3. Ressourcen werden freigegeben

---

## Verwandte Dokumentation

- [Server-Komponenten](SERVER_COMPONENTS.md) - Zone Server Details
- [Azure Deployment](AZURE_DEPLOYMENT.md) - Container Apps Scaling
- [Redis-Strategie](REDIS.md) - Cross-Shard Communication

---

## 🔗 Nützliche Links

- [Azure Container Apps Scaling](https://learn.microsoft.com/en-us/azure/container-apps/scale-app)
- [Redis Cluster](https://redis.io/docs/management/scaling/)
- [Horizontal Pod Autoscaling](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/)

---

*Teil der [Architektur-Dokumentation](../ARCHITECTURE.md)*
