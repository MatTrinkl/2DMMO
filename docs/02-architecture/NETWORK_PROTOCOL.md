# 🌐 Netzwerk-Protokoll

## 2DMMO – Network Protocol Specification

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](../ARCHITECTURE.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt das Netzwerk-Protokoll für die Client-Server-Kommunikation im 2DMMO.

---

## Transport Layer

| Eigenschaft | Wert |
|-------------|------|
| **Protokoll** | TCP |
| **Verschlüsselung** | TLS 1.3 |
| **Port** | 7777 (konfigurierbar) |
| **Encoding** | MessagePack (Binary) |

---

## Message Framing

Jede Nachricht über TCP hat folgendes Format:

```
┌─────────────────────────────────────────────────────────┐
│                    MESSAGE FRAME                         │
│                                                          │
│  ┌──────────┬──────────┬─────────────────────────────┐  │
│  │  1 Byte  │  4 Bytes │       N Bytes               │  │
│  │   Type   │  Length  │       Payload               │  │
│  │          │ (uint32) │   (MessagePack Data)        │  │
│  └──────────┴──────────┴─────────────────────────────┘  │
│                                                          │
│  Type:    Message-Typ (siehe MessageType enum)          │
│  Length:  Länge des Payloads in Bytes (Little-Endian)   │
│  Payload: MessagePack-serialisierte Daten               │
│                                                          │
│  Beispiel: PositionUpdate                               │
│  ┌────┬────────────┬────────────────────────────────┐   │
│  │ 10 │ 17 00 00 00│ 94 CD 30 39 CA 43 16 80 00 ... │   │
│  └────┴────────────┴────────────────────────────────┘   │
│    │        │                    │                       │
│    │        │                    └─ MessagePack Payload  │
│    │        └─ 17 Bytes Payload-Länge                   │
│    └─ Type 10 = PositionUpdate                          │
└─────────────────────────────────────────────────────────┘
```

---

## Connection Flow

```
┌─────────────────────────────────────────────────────────┐
│                  CONNECTION FLOW                         │
│                                                          │
│  Client                    Gateway           Zone Server │
│    │                          │                    │     │
│    │──── TCP Connect ────────►│                    │     │
│    │◄─── TLS Handshake ──────►│                    │     │
│    │                          │                    │     │
│    │──── LoginRequest ───────►│                    │     │
│    │                          │── Validate ───────►│     │
│    │                          │   (Redis/DB)       │     │
│    │                          │◄── Session ────────│     │
│    │◄─── LoginResponse ───────│                    │     │
│    │     (SessionId, Zone)    │                    │     │
│    │                          │                    │     │
│    │──── JoinZone ───────────►│                    │     │
│    │                          │── RegisterPlayer ─►│     │
│    │                          │◄── Ack ───────────│     │
│    │◄─── ZoneState ───────────│◄───────────────────│     │
│    │     (Players, NPCs)      │                    │     │
│    │                          │                    │     │
│    │◄════ Game Loop ═════════►│◄══════════════════►│     │
│    │                          │                    │     │
└─────────────────────────────────────────────────────────┘
```

---

## Verwandte Dokumentation

- [Server-Komponenten](SERVER_COMPONENTS.md) - Gateway und Zone Server Details
- [Messages](MESSAGES.md) - Message Types und Serialisierung
- [Client-Server Sync](CLIENT_SERVER_SYNC.md) - Prediction und Interpolation

---

## 🔗 Nützliche Links

- [TCP/TLS in .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services)
- [TLS 1.3 Specification](https://datatracker.ietf.org/doc/html/rfc8446)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)

---

*Teil der [Architektur-Dokumentation](../ARCHITECTURE.md)*
