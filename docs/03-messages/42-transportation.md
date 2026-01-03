# 🚉 Transportation Messages (4200-4299)

**Kategorie:** 42  
**Range:** 4200-4299  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🗺️ Nodes, Routes & Discovery](#️-nodes-routes--discovery)
- [✅ Travel Start/Cancel/Complete Regeln](#-travel-startcancelcomplete-regeln)
- [⏳ Timers, Interrupts & Failures](#-timers-interrupts--failures)
- [💸 Costs, Fees & Discounts](#-costs-fees--discounts)
- [🔄 Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#️-regeln--sicherheit)
- [🛰️ Zustandsautomat & Flows](#️-zustandsautomat--flows)
- [🧪 Testfälle & Telemetrie](#-testfälle--telemetrie)
- [📩 Aktive Messages 4200–4299](#-aktive-messages-4200–4299)
  - [FlightStart (4200)](#flightstart-4200)
  - [FlightEnd (4201)](#flightend-4201)
  - [FlightCancel (4202)](#flightcancel-4202)
  - [FlightPathUpdate (4203)](#flightpathupdate-4203)
  - [PortalUse (4210)](#portaluse-4210)
  - [PortalCreate (4211)](#portalcreate-4211)
  - [PortalExpire (4212)](#portalexpire-4212)
  - [HearthstoneUse (4220)](#hearthstoneuse-4220)
  - [HearthstoneSet (4221)](#hearthstoneset-4221)
  - [HearthstoneCooldown (4222)](#hearthstonecooldown-4222)
  - [SummonRequest (4230)](#summonrequest-4230)
  - [SummonAccept (4231)](#summonaccept-4231)
  - [SummonDecline (4232)](#summondecline-4232)
  - [SummonComplete (4233)](#summoncomplete-4233)
  - [SummonFailed (4234)](#summonfailed-4234)
  - [MeetingStoneQueue (4235)](#meetingstonequeue-4235)
  - [MeetingStoneResult (4236)](#meetingstoneresult-4236)
  - [VehicleMount (4240)](#vehiclemount-4240)
  - [VehicleDismount (4241)](#vehicledismount-4241)
  - [VehicleControl (4242)](#vehiclecontrol-4242)
  - [VehicleAbility (4243)](#vehicleability-4243)
- [BoatArrival (4250)](#boatarrival-4250)
- [BoatDeparture (4251)](#boatdeparture-4251)
- [ZeppelinArrival (4252)](#zeppelinarrival-4252)
- [ZeppelinDeparture (4253)](#zeppelindeparture-4253)
- [TramArrival (4254)](#tramarrival-4254)
- [TramDeparture (4255)](#tramdeparture-4255)
- [TaxiRequest (4260)](#taxirequest-4260)
  - [TaxiConfirm (4261)](#taxiconfirm-4261)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang (MessageType Enum Updates)](#-anhang-messagetype-enum-updates)

---

## 📋 Überblick

Transportation bildet **alle Reisefunktionen** des 2D Pixelart-MMORPG ab: Taxi-/Flugrouten, Kurzstrecken-Taxis, Portale, Hearthstone/Bindpunkte, Meeting Stones, Beschwörungen, Fahrzeuge, Boote, Zeppeline und Trams.  
Jede Client→Server Message hat eine **deterministische Response** (innerhalb dieser Range oder durch Verweis auf existierende Kategorien wie `ZoneTransferResponse (107)`).  
Alle DTOs sind **stable keyed** und folgen dem Stil aus `00-connection.md` und `01-zone.md` (vollständige Tabellen, ausführliche Beschreibung, Code-Beispiele).

**Designprinzipien**
- **Autoritativ**: Server berechnet Pfade, Positionen, Kosten, Refunds, Validierungen.
- **Idempotent**: `ClientSequence` + Ticket/RequestId → gleiche Antwort bei Wiederholung.
- **Sicher**: Combat-Lock, Movement-Suppression, HMAC-Signaturen, Anti-Cheat-Logging.
- **Reconnect-Safe**: Travel-Status wird bei Reconnect konsistent beendet oder fortgesetzt.
- **Revisioniert**: Knoten & Routen werden versioniert (`Revision`) und via Deltas verteilt.

---

## 🧠 Datenmodell

### TravelNode
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NodeId | ushort | Eindeutige Travel-Node-ID | Ja |
| ZoneId | ushort | Zone, in der der Node liegt | Ja |
| Position | Position | Weltkoordinaten (X,Y,Z,ZoneId) | Ja |
| NodeType | TravelNodeType | Flightmaster, PortalAnchor, VehicleStop, MeetingStone, TaxiStop | Ja |
| Faction | Faction? | Zugriffsfraktion oder `null` | Nein |
| Requirements | TravelRequirementFlags | Level/Quest/Faction/Item | Ja |
| DiscoveryState | TravelDiscoveryState | Locked/Discovered/Restricted | Ja |
| DisplayName | string | UI Name | Ja |

### Route
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RouteId | uint | Eindeutige Route-ID | Ja |
| FromNodeId | ushort | Start-Node | Ja |
| ToNodeId | ushort | Ziel-Node | Ja |
| TravelMode | TravelMode | Flight/VehicleRail/WaterTaxi/Airship | Ja |
| Segments | List<RouteSegmentDto> | Geordnete Segmente | Ja |
| BaseDurationMs | int | Erwartete Dauer | Ja |
| Cost | TravelCostDto | Preis inkl. Surge/Discount | Ja |
| InterruptFlags | TravelInterruptFlags | Welche Events abbrechen | Ja |
| MaxPassengers | byte | Kapazität | Ja |

### TravelTicket
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TicketId | Guid | Korrelation | Ja |
| RouteId | uint | Route | Ja |
| ClientSequence | uint | Idempotenz | Ja |
| IssuedAt | long | Unix ms | Ja |
| ExpiresAt | long | Unix ms | Ja |
| Signature | byte[] | HMAC | Ja |
| Cost | TravelCostDto | Finaler Preis | Ja |
| BillingAccountId | long | Server-seitige Abrechnung | Ja |

### TeleportRequestContext
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Korrelation | Ja |
| Source | TeleportSource | Portal/Hearth/Summon/MeetingStone | Ja |
| TargetZoneId | ushort | Zielzone | Ja |
| TargetPosition | Position | Ziel-Position | Ja |
| CreatedAt | long | Unix ms | Ja |
| ExpiresAt | long | TTL (Anti-Replay) | Ja |
| Requirements | TravelRequirementFlags | Bedingungen | Ja |
| AllowCombat | bool | Erlaubt Combat-Teleport | Ja |

### VehicleSeatStateDto
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SeatIndex | byte | Sitznummer | Ja |
| OccupantId | Guid? | Spieler ID | Nein |
| SeatFlags | VehicleSeatFlags | Driver/Gunner/Passenger | Ja |
| Locked | bool | Boarding erlaubt? | Ja |
| CooldownMs | int | Sitz-spezifischer Cooldown | Ja |

### TravelPathStateDto
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Revision | uint | Snapshot/Deltas | Ja |
| FullSync | bool | Vollständiger Sync? | Ja |
| Nodes | List<TravelNodeDto> | Knoten | Ja |
| Routes | List<TravelRouteDto> | Routen | Ja |
| GeneratedAt | long | Unix ms | Ja |

---

## 🗺️ Nodes, Routes & Discovery

- **Per Character**: Flightmaster/Taxi/MeetingStone müssen entdeckt werden (`DiscoveryState=Discovered`), bevor sie als Start genutzt werden können.
- **Per Account**: Story-Portale können accountweit freigeschaltet sein (Requirements Flag).
- **Validation Pipeline**:
  1. Node existiert und ist erreichbar (Distanz < 3.0f oder InteractState).
  2. Requirements erfüllt (Quest, Level, Faction, Ticket).
  3. Route existiert und ist freigeschaltet (Phase/Event).
  4. Player nicht im Combat/CC, sofern InterruptFlags dies verlangen.
  5. Keine aktive TravelSession (`ALREADY_TRAVELING`).
- **Discovery Events**: Erfolgreiche Discovery triggert `FlightPathUpdate` (Delta) und kann Achievements (1900er Range) auslösen.
- **Caching**: Clients cachen TravelPathState nach Revision; bei verpassten Deltas sendet Server FullSync.

---

## ✅ Travel Start/Cancel/Complete Regeln

- **Start**: Erfolgreicher Request erzeugt Ticket + optional BoardingTimer. Startsignal kommt mit `FlightStart` oder `VehicleControl`.
- **Cancel**: Nur wenn `Cancelable=true` (Policy: bis Takeoff + 3s). Cancel führt zu `FlightEnd` mit `EndReason=Cancelled` und RefundPolicy.
- **Complete**: Erfolgreiche Ankunft → `FlightEnd EndReason=Arrived`. Fahrzeuge: Dismount + optional `FlightEnd` (wenn Route-basiert).
- **Server Authority**: Während Travel ist Server alleiniger Positions-Autor. Client sendet keine Movement-Inputs; Server snappt bei Desync.
- **Billing**: Kosten bei `TaxiConfirm` reserviert, Refund bei Cancel/Failure nach Policy.
- **Idempotenz**: Gleicher `ClientSequence` → identische Response.

---

## ⏳ Timers, Interrupts & Failures

- **Cast/Boarding Times**:
  - Hearthstone/Portal/Summon: definierte Cast-Zeit, Abbruch bei Move/CC.
  - Boarding: `BoardingMs` aus `TaxiConfirm`.
- **Timeouts**:
  - SummonAccept/Decline: 30s
  - MeetingStoneQueue: 120s
  - TaxiRequest Validity: 10s
  - Portal TTL: aus `PortalCreate.DurationMs`
- **Interrupt Flags**:
  - BreakOnCombat, BreakOnDamage, BreakOnMove, BreakOnCrowdControl, BreakOnDisconnect
- **Reconnect**:
  - Während Travel → `FlightEnd` mit `Disconnect` und Sicherung auf SegmentPosition.
  - Während Boarding → `TaxiConfirm` erneut (idempotent).

---

## 💸 Costs, Fees & Discounts

- **Kostenarten**: Gold, Tokens, Event-Währungen. Immer serverseitig berechnet.
- **Surge Pricing**: Optionaler Faktor bei hoher Auslastung (max 2.0x).
- **Rabatte**: Faction, Premium, Event-Buffs; Multiplikation vor Surge.
- **Refund Policy**:
  - Cancel vor Start: 90% Refund
  - Interrupt (Combat/Damage): 50% Refund
  - Server Failure/Disconnect: 100% Refund
- **Tickets**: Signiert, enthält finalen Preis; Client-Werte rein informativ.

---

## 🔄 Sync, Deltas & Revisioning

- **FlightPathUpdate** nutzt `Revision` + `FullSync`.
- **Delta-Regeln**:
  - Enthält nur geänderte Nodes/Routes.
  - `FullSync=false` → Client muss fehlende Einträge aus Cache nutzen.
- **Reconnect**:
  - Unbekannte Revision → FullSync
  - Bekannte Revision → Delta seit Revision
- **TravelState** wird zusätzlich über Zone/Movement synchronisiert, falls Travel unterbrochen.

---

## 🧱 DTOs / Interfaces

### TravelRouteDto
```csharp
[MessagePackObject]
public class TravelRouteDto
{
    [Key(0)] public uint RouteId { get; set; }
    [Key(1)] public ushort FromNodeId { get; set; }
    [Key(2)] public ushort ToNodeId { get; set; }
    [Key(3)] public TravelMode TravelMode { get; set; }
    [Key(4)] public int BaseDurationMs { get; set; }
    [Key(5)] public TravelCostDto Cost { get; set; } = null!;
    [Key(6)] public List<RouteSegmentDto> Segments { get; set; } = new();
    [Key(7)] public TravelInterruptFlags InterruptFlags { get; set; }
    [Key(8)] public TravelRequirementFlags Requirements { get; set; }
    [Key(9)] public byte MaxPassengers { get; set; }
}
```

### TravelCostDto
```csharp
[MessagePackObject]
public class TravelCostDto
{
    [Key(0)] public long Gold { get; set; }
    [Key(1)] public Dictionary<string, int>? Currencies { get; set; }
    [Key(2)] public float SurgeMultiplier { get; set; } = 1.0f;
    [Key(3)] public float DiscountMultiplier { get; set; } = 1.0f;
    [Key(4)] public long FinalGold => (long)(Gold * SurgeMultiplier * DiscountMultiplier);
}
```

### RouteSegmentDto
```csharp
[MessagePackObject]
public class RouteSegmentDto
{
    [Key(0)] public Position Position { get; set; }
    [Key(1)] public float Speed { get; set; }
    [Key(2)] public int WaitTimeMs { get; set; }
    [Key(3)] public TravelSegmentType SegmentType { get; set; }
}
```

### TravelNodeDto
```csharp
[MessagePackObject]
public class TravelNodeDto
{
    [Key(0)] public ushort NodeId { get; set; }
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public Position Position { get; set; }
    [Key(3)] public TravelNodeType NodeType { get; set; }
    [Key(4)] public TravelDiscoveryState DiscoveryState { get; set; }
    [Key(5)] public TravelRequirementFlags Requirements { get; set; }
    [Key(6)] public Faction? Faction { get; set; }
    [Key(7)] public string DisplayName { get; set; } = "";
}
```

### TravelTicketDto
```csharp
[MessagePackObject]
public class TravelTicketDto
{
    [Key(0)] public MessageType Type { get; set; }
    [Key(1)] public Guid TicketId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public uint RouteId { get; set; }
    [Key(4)] public long IssuedAt { get; set; }
    [Key(5)] public long ExpiresAt { get; set; }
    [Key(6)] public TravelCostDto Cost { get; set; } = null!;
    [Key(7)] public byte[] Signature { get; set; } = Array.Empty<byte>();
    [Key(8)] public long BillingAccountId { get; set; }
}
```

### SummonContextDto
```csharp
[MessagePackObject]
public class SummonContextDto
{
    [Key(0)] public Guid RequestId { get; set; }
    [Key(1)] public Guid SummonerId { get; set; }
    [Key(2)] public string SummonerName { get; set; } = "";
    [Key(3)] public ushort TargetZoneId { get; set; }
    [Key(4)] public Position TargetPosition { get; set; }
    [Key(5)] public long ExpiresAt { get; set; }
    [Key(6)] public bool InCombatAllowed { get; set; }
}
```

### VehicleSeatStateDto
```csharp
[MessagePackObject]
public class VehicleSeatStateDto
{
    [Key(0)] public byte SeatIndex { get; set; }
    [Key(1)] public Guid? OccupantId { get; set; }
    [Key(2)] public VehicleSeatFlags SeatFlags { get; set; }
    [Key(3)] public bool Locked { get; set; }
    [Key(4)] public int CooldownMs { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### TravelNodeType
| Wert | Beschreibung |
|------|--------------|
| Flightmaster | Klassischer Flugpunkt |
| PortalAnchor | Statischer Portalanker |
| VehicleStop | Station für Fahrzeuge/Boote/Zeppeline/Tram |
| MeetingStone | Gruppensammelpunkt |
| TaxiStop | Kurzstrecken-Taxi |

### TravelMode
| Wert | Beschreibung |
|------|--------------|
| Flight | Luftflug |
| VehicleRail | Schienenfahrzeug/Tram |
| WaterTaxi | Boot/Fähre |
| Airship | Zeppelin/Luftschiff |
| Summon | Beschwörung |
| Portal | Portal-Teleport |

### TravelSegmentType
| Wert | Beschreibung |
|------|--------------|
| Move | Bewegung mit Speed |
| Wait | Warten/Boarding |
| Teleport | Sofortversatz |

### TravelDiscoveryState
| Wert | Beschreibung |
|------|--------------|
| Locked | Nicht entdeckt |
| Discovered | Entdeckt |
| Restricted | Zeitweise gesperrt |

### TravelRequirementFlags (bitmask)
| Bit | Flag | Beschreibung |
|-----|------|--------------|
| 1 | RequiresDiscovery | Node muss entdeckt sein |
| 2 | RequiresQuest | Quest nötig |
| 4 | RequiresFaction | Faction-gebunden |
| 8 | RequiresTicket | Ticket Item |
| 16 | RequiresLevel | Min-Level |
| 32 | RequiresParty | Gruppe erforderlich |

### TravelInterruptFlags (bitmask)
| Bit | Flag | Beschreibung |
|-----|------|--------------|
| 1 | BreakOnCombat | Combat bricht ab |
| 2 | BreakOnDamage | Schaden bricht ab |
| 4 | BreakOnMove | Bewegung bricht ab |
| 8 | BreakOnCrowdControl | CC bricht ab |
| 16 | BreakOnDisconnect | Disconnect bricht ab |

### VehicleSeatFlags
| Wert | Beschreibung |
|------|--------------|
| None | Kein Spezialstatus |
| Driver | Steuersitz |
| Gunner | Kanonensitz |
| Passenger | Passagier |
| Cargo | Laderaum |

### VehicleStateFlags
| Wert | Beschreibung |
|------|--------------|
| None | Normal |
| Locked | Boarding gesperrt |
| InMotion | Fahrzeug bewegt sich |
| Damaged | Beeinträchtigte Kontrolle |

### FlightEndReason
| Wert | Beschreibung |
|------|--------------|
| Arrived | Erfolgreich angekommen |
| Cancelled | Manuell abgebrochen |
| Interrupted | Durch Combat/Move/CC |
| Failed | Server/Validation Fehler |
| Disconnect | Client getrennt |

### Error Codes (transportation-weit)
| Code | Beschreibung |
|------|--------------|
| NODE_NOT_FOUND | Node nicht vorhanden |
| ROUTE_NOT_FOUND | Route nicht vorhanden |
| NOT_DISCOVERED | Node nicht entdeckt |
| ZONE_LOCKED | Zielzone gesperrt |
| IN_COMBAT | Spieler im Kampf |
| INTERRUPTED | Abbruch durch Event |
| COOLDOWN_ACTIVE | Cooldown aktiv |
| INSUFFICIENT_FUNDS | Kosten nicht gedeckt |
| TICKET_EXPIRED | Ticket abgelaufen |
| INVALID_SIGNATURE | Ticket/Signature ungültig |
| TARGET_UNAVAILABLE | Portal/Summon Ziel nicht verfügbar |
| TIMEOUT | Zeitüberschreitung |
| ALREADY_TRAVELING | Spieler reist bereits |
| RATE_LIMITED | Zu viele Requests |
| SEAT_TAKEN | Sitz bereits besetzt |

---

## ⚙️ Regeln & Sicherheit

- **Authentifizierung**: Alle Client→Server-Requests setzen gültige Session voraus.
- **Idempotenz**: `ClientSequence` + Ticket/RequestId → identische Response bei Wiederholung.
- **Rate Limits**: 5 Travel-Requests / 30s; Summon 3 / 60s; PortalUse 5 / 60s.
- **Anti-Cheat**:
  - Movement-Suppression während Travel
  - Server-seitiger Position Snap nach Travel-Ende
  - Logging von `INVALID_SIGNATURE`, `ALREADY_TRAVELING`, `RATE_LIMITED`
- **Refund-Sicherheit**: Refund immer serverseitig berechnet, nicht Client-Input.
- **TTL**: Tickets/Requests haben `ExpiresAt`; abgelaufen → `ErrorCode=TICKET_EXPIRED`.
- **Correlation**: Jede Client-Message benennt Response (auch wenn Response außerhalb Range liegt).

---

## 🛰️ Zustandsautomat & Flows

### TravelState Machine
| Zustand | Beschreibung | Zulässige Transitionen |
|---------|--------------|------------------------|
| Idle | Kein Travel aktiv | → BoardingRequested |
| BoardingRequested | TaxiRequest gesendet | → Boarding | → Idle (Error) |
| Boarding | Ticket bestätigt, Timer läuft | → InFlight | → Cancelled |
| InFlight | Server steuert Bewegung | → Arrived | → Cancelled | → Interrupted | → Disconnect |
| Arrived | FlightEnd (Arrived) empfangen | → Idle |
| Cancelled | FlightEnd (Cancelled) empfangen | → Idle |
| Interrupted | FlightEnd (Interrupted/Failed) empfangen | → Idle |
| Disconnect | FlightEnd (Disconnect) empfangen | → Idle |

### Sequenz: Taxi → Flight
```
Client                    Server
  | TaxiRequest           |
  |---------------------> |
  |                       | Validate route, funds
  |                       | Reserve cost, create ticket
  | <-------------------- | TaxiConfirm (BoardingMs)
  |                       | Boarding timer
  | <-------------------- | FlightStart
  |                       | Server moves along route
  | <-------------------- | FlightEnd (Arrived)
```

### Sequenz: PortalUse → ZoneTransfer
```
Client                    Server
  | PortalUse             |
  |---------------------> |
  |                       | Validate portal, requirements
  |                       | Create TeleportRequestContext
  | <-------------------- | ZoneTransferResponse (107)
  | <-------------------- | ZoneState (102) (neue Zone)
```

### Sequenz: Summon
```
Server (Summoner)         Target Client
        | SummonRequest -------->|
        |                        |
        |<------- SummonAccept   |
        |                        | Validate combat/distance
        |<------- SummonFailed   | (bei Fehler)
        |<------- SummonComplete | (bei Erfolg)
```

---

## 🧪 Testfälle & Telemetrie

### Funktionale Tests (Auswahl)
- TaxiRequest:
  - Erfolgreich (Discover + Funds)
  - Error `INSUFFICIENT_FUNDS`
  - Error `NOT_DISCOVERED`
  - Error `ALREADY_TRAVELING`
- FlightCancel:
  - Cancelable=true → FlightEnd Cancelled + Refund 90%
  - Cancelable=false → FlightEnd Failed ErrorCode=INTERRUPTED (Denied)
- PortalUse:
  - Erfolgreich → ZoneTransferResponse Success
  - Gesperrte Zone → ZoneTransferResponse Error `ZONE_LOCKED`
- Summon:
  - Accept innerhalb 30s → SummonComplete
  - Timeout → SummonFailed TIMEOUT
- MeetingStoneQueue:
  - Erfolg mit PartyId
  - Timeout -> Error TIMEOUT

### Telemetrie
| Metric | Beschreibung | Tags |
|--------|--------------|------|
| travel.taxi.requests | Anzahl TaxiRequest | zoneId, fromNodeId, toNodeId, result |
| travel.flight.cancel | Anzahl FlightCancel | endReason |
| travel.portal.use | PortalUse Aufrufe | portalNodeId, result |
| travel.summon.accept | SummonAccept | success |
| travel.vehicle.mount | VehicleMount | vehicleType, seatIndex, result |
| travel.queue.time | Zeit bis MeetingStoneResult | stoneNodeId, success |

---

## 🧭 UI/UX Anforderungen

- **Progress Anzeigen**:
  - FlightStart: Fortschrittsbalken basierend auf BaseDurationMs und SegmentIndex.
  - Boat/Zeppelin/Tram: ETA Countdown im Dock/Plattform UI.
  - Hearthstone: Cast-Bar + Cooldown Overlay.
- **Warnungen**:
  - Combat Lock: roter Hinweis bei Portal/Hearth/Summon.
  - Cancel Confirm: Dialog vor FlightCancel (wenn Refund < 100%).
  - Expiry: Portal/Summon TTL Timer sichtbar.
- **Accessibility**:
  - Alle Timer/Hinweise zusätzlich als Text ausgeben.
  - Farb-Coding für Faction Locks.
  - Tastatur Shortcuts für Accept/Decline Summon.

---

## 🧪 Testmatrizen (erweitert)

### Taxi/Flight Fälle
| ID | Szenario | Erwartung |
|----|----------|-----------|
| T1 | Discovery fehlt | TaxiConfirm Error NOT_DISCOVERED |
| T2 | Funds fehlen | TaxiConfirm Error INSUFFICIENT_FUNDS |
| T3 | Combat aktiv | TaxiConfirm Error IN_COMBAT |
| T4 | Boarding Cancel | FlightEnd Cancelled Refund 90% |
| T5 | Damage während Boarding | FlightEnd Interrupted Refund 50% |
| T6 | Disconnect während Flug | FlightEnd Disconnect, Position Snap |
| T7 | Duplicate TaxiRequest | Gleiches Ticket bei gleichem ClientSequence |

### Portal Fälle
| ID | Szenario | Erwartung |
|----|----------|-----------|
| P1 | Portal abgelaufen | PortalUse -> Error TARGET_UNAVAILABLE |
| P2 | Zone gesperrt | ZoneTransferResponse Error ZONE_LOCKED |
| P3 | Faction mismatch | Error REQUIRES_FACTION |
| P4 | Reconnect während Transfer | ZoneTransferResponse erneut |

### Hearthstone Fälle
| ID | Szenario | Erwartung |
|----|----------|-----------|
| H1 | Cooldown aktiv | Error COOLDOWN_ACTIVE |
| H2 | Combat aktiv | Error IN_COMBAT |
| H3 | Bind fehlt | Error NODE_NOT_FOUND |
| H4 | Abbruch durch Bewegung | Error INTERRUPTED |

### Summon Fälle
| ID | Szenario | Erwartung |
|----|----------|-----------|
| S1 | Accept rechtzeitig | SummonComplete + ZoneTransferResponse |
| S2 | Decline | SummonFailed (Declined) |
| S3 | Timeout | SummonFailed TIMEOUT |
| S4 | Combat verboten | SummonFailed IN_COMBAT |
| S5 | Duplicate Accept | Idempotente SummonComplete |

### MeetingStone Fälle
| ID | Szenario | Erwartung |
|----|----------|-----------|
| M1 | Queue Erfolg | MeetingStoneResult Success=true |
| M2 | Queue Timeout | MeetingStoneResult Success=false TIMEOUT |
| M3 | Already queued | Error ALREADY_TRAVELING |

### Vehicle Fälle
| ID | Szenario | Erwartung |
|----|----------|-----------|
| V1 | Seat frei | VehicleControl mit Occupant |
| V2 | Seat belegt | Error SEAT_TAKEN |
| V3 | Driver Disconnect | VehicleControl ControlOwner null |
| V4 | Ability ohne Control | Error NOT_AUTHORIZED |

---

## 🛡️ Anti-Exploit Patterns

- **Server-only Movement**: Während Travel werden Movement-Inputs ignoriert und geloggt.
- **HMAC Signaturen**: Tickets enthalten Signature; ungültige Tickets führen zu Audit + Kick bei Häufung.
- **Cooldown Enforcement**: Server-side Timestamps, keine Client-Timer als Quelle.
- **Rate Limiting**: pro Account und pro IP für PortalUse/Summon/TaxiRequest.
- **Distance Checks**: For Portal/Hearth/Summon start position darf nicht weiter als 5m vom Trigger entfernt sein.
- **Refund Guards**: Refund nur wenn FlightEnd gesendet wurde; Client kann keinen Refund anfordern.

---

## 🧷 Integrationspunkte

- **ZoneTransferResponse (107)**: Verwendet von PortalUse, HearthstoneUse, SummonAccept.
- **ErrorMessage (910)**: Generische Fehlerantwort falls keine spezifische Response vorgesehen (z. B. HearthstoneSet Failure).
- **Combat System**: Combat-Lock prüft Flags bevor Teleport erlaubt.
- **Economy Service**: Reserviert/Abbucht Gold/Tokens bei TaxiConfirm.
- **Logging/Audit**: Alle Errors mit wirtschaftlichem Impact werden auditiert.

---

## 🎯 SLA & Observability Checks

| Check | Schwelle | Aktion |
|-------|----------|--------|
| TaxiConfirm P95 > 200ms | Warn | Skaliere Travel-Service |
| FlightStart Droprate > 1% | Alert | Retry Mechanismus prüfen |
| SummonFailed TIMEOUT > 5% | Alert | Matchmaking/Latency prüfen |
| FlightPathUpdate FullSync Rate > 10% | Warn | Revision Drift untersuchen |
| Invalid Signature > 0.1% | Alert | Anti-Cheat Review |

---

## 🧭 Detaillierte Schrittfolgen pro Transport

### Taxi/Flight
1. Client zeigt Taxi UI (Nodes/Routes aus FlightPathUpdate).
2. Spieler wählt Ziel → sendet `TaxiRequest`.
3. Server validiert (Discovery, Funds, Combat, Route).
4. Server reserviert Kosten, erstellt Ticket → `TaxiConfirm`.
5. Boarding Timer läuft (optional), Server prüft weiterhin Combat.
6. Server sendet `FlightStart`.
7. Server bewegt Spieler entlang Segmente; Client rendert nur.
8. Bei Cancel/Interrupt/Failure → `FlightEnd` mit Refund.
9. Bei Erfolg → `FlightEnd EndReason=Arrived` + Movement freigeben.

### Portal
1. Client interagiert mit Portal → sendet `PortalUse`.
2. Server validiert Portal aktiv, Requirements, Combat.
3. Server erstellt TeleportRequestContext.
4. Server sendet `ZoneTransferResponse` (Kategorie 1) und `ZoneState`.
5. Client lädt Assets, sendet `ZoneLoadedAck`.
6. Server startet reguläre Updates.

### Hearthstone
1. Client klickt Hearthstone → sendet `HearthstoneUse`.
2. Server prüft Cooldown/Combat/Bind.
3. Cast Time läuft (clientseitig UI, serverseitig Timer).
4. Nach Erfolg: `ZoneTransferResponse` + `ZoneState`.
5. `HearthstoneCooldown` synchronisiert neuen CD.

### Summon
1. Summoner löst Beschwörung aus (außerhalb dieses Dokuments).
2. Server sendet `SummonRequest` an Ziel.
3. Ziel antwortet mit `SummonAccept` oder `SummonDecline`.
4. Server validiert erneut Combat/Distance.
5. Bei Erfolg: `ZoneTransferResponse` an Ziel, `SummonComplete` an beide.
6. Bei Fehler/Timeout: `SummonFailed` an beide.

### MeetingStone
1. Client sendet `MeetingStoneQueue` mit Rolle.
2. Server fügt in Queue ein, Matchmaking läuft.
3. Ergebnis: `MeetingStoneResult` Success=true mit Party oder Error/Timeout.
4. Erfolgreiche Parties können Summon/Portal nutzen (außerhalb dieser Range).

### Vehicle
1. Client sendet `VehicleMount` mit VehicleId/SeatIndex.
2. Server validiert Sitz, lockt und broadcastet `VehicleControl`.
3. Fahrer kann `VehicleAbility` nutzen; Passagiere bleiben passiv.
4. `VehicleDismount` gibt Sitz frei; `VehicleControl` broadcastet.

### Boat/Zeppelin/Tram
1. Server plant Fahrplan; sendet `Arrival` mit ETA.
2. Clients zeigen Boarding UI; Boarding Server-seitig geprüft.
3. Server sendet `Departure`; Bewegung beginnt (ggf. VehicleControl separat).
4. Bei Ankunft in anderer Zone erfolgt ZoneTransfer (Kategorie 1) außerhalb dieser Range.

---

## 🧮 Payload-Beispiele (MessagePack/JSON-Äquivalent)

### TaxiConfirm Erfolg (JSON-Äquivalent)
```json
{
  "Type": "TaxiConfirm",
  "Success": true,
  "Ticket": {
    "Type": "TaxiConfirm",
    "TicketId": "7e5d6f4a-1c2b-3c4d-5e6f-708090a0b0c0",
    "ClientSequence": 12,
    "RouteId": 101,
    "IssuedAt": 1735900000000,
    "ExpiresAt": 1735900015000,
    "Cost": { "Gold": 1500, "Currencies": null, "SurgeMultiplier": 1.0, "DiscountMultiplier": 0.9 },
    "Signature": "base64-signature",
    "BillingAccountId": 12345
  },
  "Route": {
    "RouteId": 101,
    "FromNodeId": 10,
    "ToNodeId": 22,
    "TravelMode": "Flight",
    "BaseDurationMs": 60000,
    "Cost": { "Gold": 1500, "Currencies": null, "SurgeMultiplier": 1.0, "DiscountMultiplier": 0.9 },
    "Segments": [],
    "InterruptFlags": "BreakOnCombat",
    "Requirements": "RequiresDiscovery",
    "MaxPassengers": 1
  },
  "BoardingMs": 3000
}
```

### FlightEnd Interrupt (JSON-Äquivalent)
```json
{
  "Type": "FlightEnd",
  "TicketId": "7e5d6f4a-1c2b-3c4d-5e6f-708090a0b0c0",
  "EndReason": "Interrupted",
  "TargetPosition": { "X": 150.0, "Y": 220.0, "Z": 0.0, "ZoneId": 1001 },
  "RefundGold": 750,
  "ErrorCode": "INTERRUPTED"
}
```

### SummonRequest (JSON-Äquivalent)
```json
{
  "Type": "SummonRequest",
  "Context": {
    "RequestId": "11111111-2222-3333-4444-555555555555",
    "SummonerId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
    "SummonerName": "WarlockOne",
    "TargetZoneId": 2001,
    "TargetPosition": { "X": 12.5, "Y": 88.2, "Z": 0.0, "ZoneId": 2001 },
    "ExpiresAt": 1735900020000,
    "InCombatAllowed": false
  },
  "RequiresConfirmation": true
}
```

### VehicleControl (JSON-Äquivalent)
```json
{
  "Type": "VehicleControl",
  "VehicleId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "ControlOwnerId": "bbbbbbbb-cccc-dddd-eeee-ffffffffffff",
  "SeatStates": [
    { "SeatIndex": 0, "OccupantId": "bbbbbbbb-cccc-dddd-eeee-ffffffffffff", "SeatFlags": "Driver", "Locked": false, "CooldownMs": 0 },
    { "SeatIndex": 1, "OccupantId": null, "SeatFlags": "Passenger", "Locked": false, "CooldownMs": 0 }
  ],
  "StateFlags": "InMotion"
}
```

---

## 📩 Aktive Messages 4200–4299

### FlightStart (4200)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startsignal für Flug/Taxis nach erfolgreichem `TaxiConfirm`. Enthält Ticket und Route. Client wechselt in Travel-Modus, unterdrückt Movement-Inputs und zeigt Fortschritt entlang Segments an.

### Im Scope ✅
- Start eines autorisierten Fluges
- Ticket-Übergabe (signiert)
- Initialer Segmentindex

### Nicht im Scope ❌
- Kostenberechnung (TaxiConfirm)
- Flugende (FlightEnd)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.FlightStart` | Ja |
| Ticket | TravelTicketDto | Signiertes Ticket | Ja |
| Route | TravelRouteDto | Vollständige Route | Ja |
| SegmentIndex | int | Startsegment (0) | Ja |
| Cancelable | bool | Darf FlightCancel? | Ja |

### Server-Validierung
- Ticket ist gültig, nicht abgelaufen.
- Player steht an FromNodeId (Distanz < 3.0f).
- Player nicht im Combat, sofern InterruptFlags dies fordern.
- Keine parallele TravelSession.

### Client-Verhalten
- Movement-Eingaben deaktivieren.
- Kamera folgt vordefiniertem Pfad.
- Progress UI basierend auf SegmentIndex und BaseDuration anzeigen.

### Fehlercodes (kontext)
| Code | Bedeutung | Serveraktion |
|------|-----------|--------------|
| INVALID_SIGNATURE | Ticket HMAC ungültig | Abbruch, Audit |
| ALREADY_TRAVELING | Schon in Travel | FlightEnd Failed |

### Flow
```
TaxiConfirm (Boarding) -> FlightStart -> Server bewegt Spieler -> FlightEnd
```

### Erwartete Response
- Keine. Cancel → FlightCancel → FlightEnd.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.FlightStart)]
public class FlightStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.FlightStart;
    [Key(1)] public TravelTicketDto Ticket { get; init; } = null!;
    [Key(2)] public TravelRouteDto Route { get; init; } = null!;
    [Key(3)] public int SegmentIndex { get; init; }
    [Key(4)] public bool Cancelable { get; init; }
}
```

---

### FlightEnd (4201)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Beendet den Flug mit finaler Position, EndReason und Refund-Info. Wird bei Ankunft, Cancel, Interrupt, Disconnect oder Failure gesendet.

### Im Scope ✅
- Abschluss-Event mit Refund
- Ziel-Position
- Fehlercodierung

### Nicht im Scope ❌
- Neue Travel-Buchung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.FlightEnd` | Ja |
| TicketId | Guid | TravelTicket | Ja |
| EndReason | FlightEndReason | Arrived/Cancelled/Interrupted/Failed/Disconnect | Ja |
| TargetPosition | Position | Endposition | Ja |
| RefundGold | long | Rückerstattung | Ja |
| ErrorCode | string? | Fehler | Nein |

### Server-Validierung
- TravelSession existiert.
- EndReason konsistent (z. B. Cancel nur wenn Cancelable=true).
- Refund berechnet nach Policy.

### Client-Verhalten
- Movement wieder aktivieren.
- UI Meldung nach EndReason (Arrived/Cancelled/Interrupted).
- Position snap auf TargetPosition.

### Fehlercodes (kontext)
| Code | Bedeutung | Maßnahme |
|------|-----------|----------|
| INTERRUPTED | Abbruch durch Combat/Move/CC | UI Hinweis |
| TICKET_EXPIRED | Ticket lief ab | Back to Idle |
| INVALID_SIGNATURE | HMAC ungültig | Audit/Anti-Cheat |

### Flow
```
FlightStart -> ... -> FlightEnd (Arrived|Cancelled|Interrupted|Failed|Disconnect)
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.FlightEnd)]
public class FlightEnd : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.FlightEnd;
    [Key(1)] public Guid TicketId { get; init; }
    [Key(2)] public FlightEndReason EndReason { get; init; }
    [Key(3)] public Position TargetPosition { get; init; }
    [Key(4)] public long RefundGold { get; init; }
    [Key(5)] public string? ErrorCode { get; init; }
}
```

---

### FlightCancel (4202)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client bittet um Abbruch eines laufenden Fluges (nur wenn `Cancelable=true`). Server antwortet mit `FlightEnd`.

### Im Scope ✅
- Nutzer-abbruch
- Idempotente Cancel-Anfragen

### Nicht im Scope ❌
- Neue Buchung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.FlightCancel` | Ja |
| TicketId | Guid | Ticket | Ja |
| ClientSequence | uint | Idempotenz | Ja |
| Reason | string? | UI Grund | Nein |

### Server-Validierung
- Ticket aktiv und Cancelable.
- Nicht doppelt verarbeitet (ClientSequence).

### Client-Verhalten
- Nach Senden: UI wartet auf FlightEnd.
- Falls keine Antwort innerhalb Timeout → erneutes FlightCancel mit gleicher ClientSequence.

### Fehlercodes (kontext)
| Code | Bedeutung |
|------|-----------|
| ALREADY_TRAVELING | Travel läuft, Cancel wird erneut verarbeitet |
| INVALID_SIGNATURE | Ticket ungültig |

### Flow
```
FlightCancel -> FlightEnd (Cancelled|Failed)
```

### Erwartete Response
- `FlightEnd` (4201)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.FlightCancel)]
public class FlightCancel : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.FlightCancel;
    [Key(1)] public Guid TicketId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public string? Reason { get; set; }
}
```

---

### FlightPathUpdate (4203)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Full), Mittel (Delta)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert Travel-Nodes und Routen. Nutzt `Revision` und `FullSync` zur Differenzierung.

### Im Scope ✅
- Discovery Updates
- Preis-/Dauer Updates
- Delta/Full Sync

### Nicht im Scope ❌
- Travel-Start/-Ende

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.FlightPathUpdate` | Ja |
| Revision | uint | Aktuelle Revision | Ja |
| FullSync | bool | Full oder Delta | Ja |
| Nodes | List<TravelNodeDto>? | Geänderte oder alle Nodes | Nein |
| Routes | List<TravelRouteDto>? | Geänderte oder alle Routen | Nein |
| GeneratedAt | long | Unix ms | Ja |

### Server-Validierung
- Revision++ bei jeder Änderung.
- FullSync enthält alle Nodes/Routes.

### Client-Verhalten
- Bei FullSync: Cache ersetzen.
- Bei Delta: Cache mergen; bei Lücke → FullSync anfordern.

### Flow
```
Login/Rejoin -> FlightPathUpdate (FullSync) -> spätere Deltas bei Änderungen
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.FlightPathUpdate)]
public class FlightPathUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.FlightPathUpdate;
    [Key(1)] public uint Revision { get; init; }
    [Key(2)] public bool FullSync { get; init; }
    [Key(3)] public List<TravelNodeDto>? Nodes { get; init; }
    [Key(4)] public List<TravelRouteDto>? Routes { get; init; }
    [Key(5)] public long GeneratedAt { get; init; }
}
```

---

### PortalUse (4210)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Nutzt Portal-Anker. Server validiert Requirements, erstellt TeleportRequestContext und antwortet mit `ZoneTransferResponse (107)` aus Kategorie 1. Position/Zone werden dort gesetzt.

### Im Scope ✅
- Portal-Nutzung
- Teleport-Kontext

### Nicht im Scope ❌
- Portal-Erstellung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PortalUse` | Ja |
| PortalNodeId | ushort | Portal | Ja |
| ClientSequence | uint | Idempotenz | Ja |
| RequestId | Guid | Korrelation | Ja |

### Server-Validierung
- Portal existiert und aktiv (kein PortalExpire).
- Requirements erfüllt.
- Player nicht im Combat (außer AllowCombat=true im Kontext).

### Client-Verhalten
- Nach Send: UI zeigt "Teleporting".
- Wartet auf ZoneTransferResponse + ZoneState.

### Fehlercodes (kontext)
| Code | Bedeutung |
|------|-----------|
| TARGET_UNAVAILABLE | Portal nicht mehr aktiv |
| ZONE_LOCKED | Zielzone gesperrt |
| NOT_DISCOVERED | Portal nicht entdeckt |

### Flow
```
PortalUse -> ZoneTransferResponse -> ZoneState -> ZoneLoadedAck
```

### Erwartete Response
- `ZoneTransferResponse` (107)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PortalUse)]
public class PortalUse : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.PortalUse;
    [Key(1)] public ushort PortalNodeId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
    [Key(3)] public Guid RequestId { get; set; }
}
```

---

### PortalCreate (4211)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server kündigt neues Portal an (Spell/World Event). Enthält Kontext, TTL und Ersteller.

### Im Scope ✅
- Portal Spawn Info
- TTL

### Nicht im Scope ❌
- Teleport selbst

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PortalCreate` | Ja |
| PortalNodeId | ushort | Node | Ja |
| Context | TeleportRequestContext | Teleport Kontext | Ja |
| CreatedBy | Guid | Ersteller | Ja |
| DurationMs | int | Lebensdauer | Ja |

### Server-Validierung
- Node registriert.
- DurationMs > 0.

### Client-Verhalten
- UI für Portal anzeigen.
- Countdown anhand DurationMs darstellen.

### Flow
```
PortalCreate -> PortalUse (Clients) -> PortalExpire (TTL)
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PortalCreate)]
public class PortalCreate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PortalCreate;
    [Key(1)] public ushort PortalNodeId { get; init; }
    [Key(2)] public TeleportRequestContext Context { get; init; } = null!;
    [Key(3)] public Guid CreatedBy { get; init; }
    [Key(4)] public int DurationMs { get; init; }
}
```

---

### PortalExpire (4212)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Portal läuft ab oder wird manuell entfernt. Clients entfernen UI/Interact Prompts; neue PortalUse Requests werden abgelehnt.

### Im Scope ✅
- Deaktivierung kommunizieren
- Grund angeben

### Nicht im Scope ❌
- Teleport revert

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PortalExpire` | Ja |
| PortalNodeId | ushort | Portal | Ja |
| Reason | string | Grund | Ja |

### Server-Validierung
- PortalNodeId aktiv.

### Client-Verhalten
- UI ausblenden.
- Pending PortalUse Anfragen verwerfen.

### Flow
```
PortalCreate -> (TTL) -> PortalExpire
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PortalExpire)]
public class PortalExpire : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PortalExpire;
    [Key(1)] public ushort PortalNodeId { get; init; }
    [Key(2)] public string Reason { get; init; } = "";
}
```

---

### HearthstoneUse (4220)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Nutzt Hearthstone, um zum Bindpunkt zu teleportieren. Server prüft Cooldown/Combat und startet ZoneTransfer.

### Im Scope ✅
- TeleportRequestContext erstellen
- Cooldown Prüfung

### Nicht im Scope ❌
- Bindpunkt setzen

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.HearthstoneUse` | Ja |
| ClientSequence | uint | Idempotenz | Ja |
| RequestId | Guid | Korrelation | Ja |

### Server-Validierung
- Cooldown frei.
- Player nicht im Combat (Standard).
- Bindpunkt vorhanden.

### Client-Verhalten
- Cast-Bar anzeigen.
- Nach Erfolg → ZoneTransferResponse abwarten.

### Fehlercodes (kontext)
| Code | Bedeutung |
|------|-----------|
| COOLDOWN_ACTIVE | Hearthstone auf CD |
| IN_COMBAT | Combat aktiv |

### Flow
```
HearthstoneUse -> ZoneTransferResponse -> ZoneState -> ZoneLoadedAck
```

### Erwartete Response
- `ZoneTransferResponse` (107)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HearthstoneUse)]
public class HearthstoneUse : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.HearthstoneUse;
    [Key(1)] public uint ClientSequence { get; set; }
    [Key(2)] public Guid RequestId { get; set; }
}
```

---

### HearthstoneSet (4221)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Setzt neuen Hearthstone-Bindpunkt. Erfolgreiche Aktion führt zu Cooldown-Sync.

### Im Scope ✅
- Bindpunkt speichern
- Optionaler Cooldown Reset

### Nicht im Scope ❌
- Teleport

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.HearthstoneSet` | Ja |
| InnkeeperNodeId | ushort | Node | Ja |
| ClientSequence | uint | Idempotenz | Ja |

### Server-Validierung
- Node ist Innkeeper und erreichbar.
- Faction erlaubt.

### Client-Verhalten
- UI bestätigt neuen Bindpunkt.
- Erwartet HearthstoneCooldown.

### Fehlercodes (kontext)
| Code | Bedeutung |
|------|-----------|
| NODE_NOT_FOUND | Innkeeper ungültig |
| ZONE_LOCKED | Zone gesperrt |

### Flow
```
HearthstoneSet -> HearthstoneCooldown
```

### Erwartete Response
- `HearthstoneCooldown` (4222) oder `ErrorMessage (910)`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HearthstoneSet)]
public class HearthstoneSet : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.HearthstoneSet;
    [Key(1)] public ushort InnkeeperNodeId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
}
```

---

### HearthstoneCooldown (4222)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Teilt aktuellen Hearthstone Cooldown und Bind-Position mit. Wird nach Nutzung oder Setzen gesendet.

### Im Scope ✅
- Cooldown Start/Remaining
- Bind-Position

### Nicht im Scope ❌
- Teleport selbst

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.HearthstoneCooldown` | Ja |
| CooldownMs | int | Gesamter CD | Ja |
| RemainingMs | int | Verbleibend | Ja |
| BindPosition | Position | Bindpunkt | Ja |

### Client-Verhalten
- UI Timer setzen.
- Bindpunkt im UI aktualisieren.

### Flow
```
HearthstoneUse/HearthstoneSet -> HearthstoneCooldown
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HearthstoneCooldown)]
public class HearthstoneCooldown : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.HearthstoneCooldown;
    [Key(1)] public int CooldownMs { get; init; }
    [Key(2)] public int RemainingMs { get; init; }
    [Key(3)] public Position BindPosition { get; init; }
}
```

---

### SummonRequest (4230)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert Zielspieler über eingehende Beschwörung. Client muss reagieren.

### Im Scope ✅
- Summon Dialog
- TTL/Expires

### Nicht im Scope ❌
- Teleport Ausführung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SummonRequest` | Ja |
| Context | SummonContextDto | Kontext | Ja |
| RequiresConfirmation | bool | Muss bestätigt werden | Ja |

### Client-Verhalten
- Dialog anzeigen mit Accept/Decline.
- Countdown basierend auf ExpiresAt.

### Flow
```
SummonRequest -> SummonAccept|SummonDecline|Timeout -> SummonComplete|SummonFailed
```

### Erwartete Response
- `SummonAccept` (4231) oder `SummonDecline` (4232)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SummonRequest)]
public class SummonRequest : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SummonRequest;
    [Key(1)] public SummonContextDto Context { get; init; } = null!;
    [Key(2)] public bool RequiresConfirmation { get; init; }
}
```

---

### SummonAccept (4231)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler akzeptiert Summon. Server prüft erneut Combat/Distance und startet Teleport.

### Im Scope ✅
- Zustimmung
- Idempotenz

### Nicht im Scope ❌
- Teleport Ergebnis

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SummonAccept` | Ja |
| RequestId | Guid | Summon Request | Ja |
| ClientSequence | uint | Idempotenz | Ja |

### Server-Validierung
- RequestId existiert und nicht abgelaufen.
- Target darf teleportiert werden (Combat?).

### Client-Verhalten
- Dialog schließen.
- Erwartet SummonComplete/Failed + ZoneTransferResponse.

### Flow
```
SummonAccept -> ZoneTransferResponse -> SummonComplete|SummonFailed
```

### Erwartete Response
- `SummonComplete` (4233) oder `SummonFailed` (4234) und `ZoneTransferResponse (107)`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SummonAccept)]
public class SummonAccept : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SummonAccept;
    [Key(1)] public Guid RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
}
```

---

### SummonDecline (4232)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lehnt Summon ab. Summoner erhält SummonFailed mit Reason Declined.

### Im Scope ✅
- Ablehnung
- Idempotenz

### Nicht im Scope ❌
- Teleport

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SummonDecline` | Ja |
| RequestId | Guid | Summon Request | Ja |
| ClientSequence | uint | Idempotenz | Ja |

### Flow
```
SummonDecline -> SummonFailed (Declined)
```

### Erwartete Response
- `SummonFailed` (4234)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SummonDecline)]
public class SummonDecline : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SummonDecline;
    [Key(1)] public Guid RequestId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
}
```

---

### SummonComplete (4233)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Summon erfolgreich abgeschlossen. Informiert Summoner und Ziel.

### Im Scope ✅
- Abschlussinfo
- Zielzone/-Position

### Nicht im Scope ❌
- Movement Steuerung (ZoneState übernimmt)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SummonComplete` | Ja |
| RequestId | Guid | Summon | Ja |
| TargetZoneId | ushort | Zielzone | Ja |
| TargetPosition | Position | Ziel | Ja |

### Flow
```
SummonAccept -> SummonComplete -> ZoneState (neue Zone)
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SummonComplete)]
public class SummonComplete : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SummonComplete;
    [Key(1)] public Guid RequestId { get; init; }
    [Key(2)] public ushort TargetZoneId { get; init; }
    [Key(3)] public Position TargetPosition { get; init; }
}
```

---

### SummonFailed (4234)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Summon schlug fehl (Timeout, Combat, Decline). Enthält ErrorCode und optional RefundItem.

### Im Scope ✅
- Fehlerkommunikation
- Refund-Info

### Nicht im Scope ❌
- Teleport

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SummonFailed` | Ja |
| RequestId | Guid | Summon | Ja |
| ErrorCode | string | Grund | Ja |
| RefundItemId | int? | Optional | Nein |

### Flow
```
SummonDecline|Timeout|ValidationFail -> SummonFailed
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SummonFailed)]
public class SummonFailed : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SummonFailed;
    [Key(1)] public Guid RequestId { get; init; }
    [Key(2)] public string ErrorCode { get; init; } = "";
    [Key(3)] public int? RefundItemId { get; init; }
}
```

---

### MeetingStoneQueue (4235)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client reiht sich an Meeting Stone ein. Server matcht und sendet Result.

### Im Scope ✅
- Matchmaking Request
- Rollenwahl

### Nicht im Scope ❌
- Teleport (über Summon/Portal)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MeetingStoneQueue` | Ja |
| StoneNodeId | ushort | Node | Ja |
| DesiredRole | GroupRole | Rolle | Ja |
| ClientSequence | uint | Idempotenz | Ja |

### Flow
```
MeetingStoneQueue -> Matching -> MeetingStoneResult
```

### Erwartete Response
- `MeetingStoneResult` (4236)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MeetingStoneQueue)]
public class MeetingStoneQueue : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.MeetingStoneQueue;
    [Key(1)] public ushort StoneNodeId { get; set; }
    [Key(2)] public GroupRole DesiredRole { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
}
```

---

### MeetingStoneResult (4236)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Ergebnis des Meeting Stone Matchmaking. Enthält PartyId/Members oder Error.

### Im Scope ✅
- Erfolg/Fehler
- Party-Daten

### Nicht im Scope ❌
- Teleport

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MeetingStoneResult` | Ja |
| Success | bool | Erfolg? | Ja |
| ErrorCode | string? | Fehler | Nein |
| PartyId | Guid? | Party | Nein |
| Members | List<Guid>? | Mitglieder | Nein |

### Flow
```
MeetingStoneQueue -> MeetingStoneResult (Success|Failure)
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MeetingStoneResult)]
public class MeetingStoneResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.MeetingStoneResult;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public string? ErrorCode { get; init; }
    [Key(3)] public Guid? PartyId { get; init; }
    [Key(4)] public List<Guid>? Members { get; init; }
}
```

---

### VehicleMount (4240)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler betritt Fahrzeug und fordert Sitzzuweisung.

### Im Scope ✅
- Sitz reservieren
- Übergang in VehicleState

### Nicht im Scope ❌
- Fahrzeugbewegung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VehicleMount` | Ja |
| VehicleId | Guid | Fahrzeug | Ja |
| SeatIndex | byte | Sitz | Ja |
| ClientSequence | uint | Idempotenz | Ja |

### Flow
```
VehicleMount -> VehicleControl (SeatStates)
```

### Erwartete Response
- `VehicleControl` (4242) oder `ErrorMessage (910)`

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VehicleMount)]
public class VehicleMount : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VehicleMount;
    [Key(1)] public Guid VehicleId { get; set; }
    [Key(2)] public byte SeatIndex { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
}
```

---

### VehicleDismount (4241)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler verlässt Fahrzeug. Sitz wird freigegeben, Position auf sichere Dismount-Position gesetzt.

### Im Scope ✅
- Sitz freigeben
- Dismount Position

### Nicht im Scope ❌
- Fahrzeugbewegung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VehicleDismount` | Ja |
| VehicleId | Guid | Fahrzeug | Ja |
| ClientSequence | uint | Idempotenz | Ja |

### Flow
```
VehicleDismount -> VehicleControl (Seat frei)
```

### Erwartete Response
- `VehicleControl` (4242)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VehicleDismount)]
public class VehicleDismount : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VehicleDismount;
    [Key(1)] public Guid VehicleId { get; set; }
    [Key(2)] public uint ClientSequence { get; set; }
}
```

---

### VehicleControl (4242)

**Richtung:** 📡 Broadcast (Server → Vehicle Occupants)  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert Sitzbelegung, ControlOwner und StateFlags.

### Im Scope ✅
- SeatStates
- ControlOwner
- StateFlags

### Nicht im Scope ❌
- Bewegungsbahn

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VehicleControl` | Ja |
| VehicleId | Guid | Fahrzeug | Ja |
| ControlOwnerId | Guid? | Steuermann | Nein |
| SeatStates | List<VehicleSeatStateDto> | Sitze | Ja |
| StateFlags | VehicleStateFlags | Zustand | Ja |

### Flow
```
VehicleMount/Dismount/Ability -> VehicleControl (Broadcast)
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VehicleControl)]
public class VehicleControl : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.VehicleControl;
    [Key(1)] public Guid VehicleId { get; init; }
    [Key(2)] public Guid? ControlOwnerId { get; init; }
    [Key(3)] public List<VehicleSeatStateDto> SeatStates { get; init; } = new();
    [Key(4)] public VehicleStateFlags StateFlags { get; init; }
}
```

---

### VehicleAbility (4243)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Löst Fahrzeugfähigkeit aus. Server prüft ControlOwner/Seat, Cooldowns, und sendet Ergebnis via VehicleControl oder Combat-Messages.

### Im Scope ✅
- Ability Aufruf
- Target Position optional

### Nicht im Scope ❌
- Schadenberechnung (Combat)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VehicleAbility` | Ja |
| VehicleId | Guid | Fahrzeug | Ja |
| AbilityId | int | Fähigkeit | Ja |
| TargetPosition | Position? | Optional | Nein |
| ClientSequence | uint | Idempotenz | Ja |

### Flow
```
VehicleAbility -> Validation -> VehicleControl/CombatResult
```

### Erwartete Response
- `VehicleControl` (4242) oder Combat-Result

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VehicleAbility)]
public class VehicleAbility : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VehicleAbility;
    [Key(1)] public Guid VehicleId { get; set; }
    [Key(2)] public int AbilityId { get; set; }
    [Key(3)] public Position? TargetPosition { get; set; }
    [Key(4)] public uint ClientSequence { get; set; }
}
```

---

### BoatArrival (4250)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Ankunft eines Bootes/Fähre am Dock. Dient UI/Audio und Boarding-Anzeige.

### Im Scope ✅
- Arrival Notice
- ETA

### Nicht im Scope ❌
- Boarding-Validierung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BoatArrival` | Ja |
| VehicleId | Guid | Boot | Ja |
| DockNodeId | ushort | Dock | Ja |
| RouteId | uint | Route | Ja |
| EtaMs | int | Zeit bis Abfahrt | Ja |

### Flow
```
BoatArrival -> (Boarding) -> BoatDeparture
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BoatArrival)]
public class BoatArrival : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BoatArrival;
    [Key(1)] public Guid VehicleId { get; init; }
    [Key(2)] public ushort DockNodeId { get; init; }
    [Key(3)] public uint RouteId { get; init; }
    [Key(4)] public int EtaMs { get; init; }
}
```

---

### BoatDeparture (4251)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Abfahrt der Fähre. Boarding schließt, Fahrt beginnt.

### Im Scope ✅
- Departure Notice

### Nicht im Scope ❌
- Fahrpositionsupdates

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.BoatDeparture` | Ja |
| VehicleId | Guid | Boot | Ja |
| DockNodeId | ushort | Dock | Ja |
| RouteId | uint | Route | Ja |

### Flow
```
BoatArrival -> BoatDeparture -> (Fahrt)
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.BoatDeparture)]
public class BoatDeparture : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.BoatDeparture;
    [Key(1)] public Guid VehicleId { get; init; }
    [Key(2)] public ushort DockNodeId { get; init; }
    [Key(3)] public uint RouteId { get; init; }
}
```

---

### ZeppelinArrival (4252)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Ankunft eines Zeppelins/Luftschiffs auf Plattform.

### Im Scope ✅
- Arrival Notice
- ETA

### Nicht im Scope ❌
- Boarding-Check

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ZeppelinArrival` | Ja |
| VehicleId | Guid | Zeppelin | Ja |
| PlatformNodeId | ushort | Plattform | Ja |
| RouteId | uint | Route | Ja |
| EtaMs | int | Zeit bis Abfahrt | Ja |

### Flow
```
ZeppelinArrival -> Boarding -> ZeppelinDeparture
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZeppelinArrival)]
public class ZeppelinArrival : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZeppelinArrival;
    [Key(1)] public Guid VehicleId { get; init; }
    [Key(2)] public ushort PlatformNodeId { get; init; }
    [Key(3)] public uint RouteId { get; init; }
    [Key(4)] public int EtaMs { get; init; }
}
```

---

### ZeppelinDeparture (4253)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Abfahrt eines Zeppelins. Boarding geschlossen.

### Im Scope ✅
- Departure Notice

### Nicht im Scope ❌
- Flugposition

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ZeppelinDeparture` | Ja |
| VehicleId | Guid | Zeppelin | Ja |
| PlatformNodeId | ushort | Plattform | Ja |
| RouteId | uint | Route | Ja |

### Flow
```
ZeppelinDeparture -> (Fahrt)
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZeppelinDeparture)]
public class ZeppelinDeparture : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZeppelinDeparture;
    [Key(1)] public Guid VehicleId { get; init; }
    [Key(2)] public ushort PlatformNodeId { get; init; }
    [Key(3)] public uint RouteId { get; init; }
}
```

---

### TramArrival (4254)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Ankunft einer Tram an Station.

### Im Scope ✅
- Arrival Notice
- ETA

### Nicht im Scope ❌
- Boarding-Validierung

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TramArrival` | Ja |
| VehicleId | Guid | Tram | Ja |
| StationNodeId | ushort | Station | Ja |
| RouteId | uint | Route | Ja |
| EtaMs | int | Zeit bis Abfahrt | Ja |

### Flow
```
TramArrival -> Boarding -> TramDeparture
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TramArrival)]
public class TramArrival : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TramArrival;
    [Key(1)] public Guid VehicleId { get; init; }
    [Key(2)] public ushort StationNodeId { get; init; }
    [Key(3)] public uint RouteId { get; init; }
    [Key(4)] public int EtaMs { get; init; }
}
```

---

### TramDeparture (4255)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Abfahrt einer Tram. Boarding geschlossen, Fahrt beginnt.

### Im Scope ✅
- Departure Notice

### Nicht im Scope ❌
- Fahrpositionsupdates

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TramDeparture` | Ja |
| VehicleId | Guid | Tram | Ja |
| StationNodeId | ushort | Station | Ja |
| RouteId | uint | Route | Ja |

### Flow
```
TramDeparture -> (Fahrt)
```

### Erwartete Response
- Keine

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TramDeparture)]
public class TramDeparture : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TramDeparture;
    [Key(1)] public Guid VehicleId { get; init; }
    [Key(2)] public ushort StationNodeId { get; init; }
    [Key(3)] public uint RouteId { get; init; }
}
```

---

### TaxiRequest (4260)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client bucht Taxi/Flugroute. Server validiert Discovery, Funds, Combat und erstellt Ticket.

### Im Scope ✅
- Route Auswahl
- Kostenberechnung
- Ticket erstellen

### Nicht im Scope ❌
- Flugstart (FlightStart)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TaxiRequest` | Ja |
| FromNodeId | ushort | Start | Ja |
| ToNodeId | ushort | Ziel | Ja |
| ClientSequence | uint | Idempotenz | Ja |

### Server-Validierung
- Nodes existieren & discovered.
- Route erlaubt.
- Player nicht im Combat/CC.
- Funds ausreichend.

### Flow
```
TaxiRequest -> TaxiConfirm -> FlightStart -> FlightEnd
```

### Erwartete Response
- `TaxiConfirm` (4261)

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TaxiRequest)]
public class TaxiRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TaxiRequest;
    [Key(1)] public ushort FromNodeId { get; set; }
    [Key(2)] public ushort ToNodeId { get; set; }
    [Key(3)] public uint ClientSequence { get; set; }
}
```

---

### TaxiConfirm (4261)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf TaxiRequest. Enthält Ticket, Route, BoardingMs. FlightStart folgt bei Erfolg nach Boarding.

### Im Scope ✅
- Kostenfinalisierung
- Ticket-Zustellung
- Boarding Timer

### Nicht im Scope ❌
- Flugende

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TaxiConfirm` | Ja |
| Success | bool | Erfolg | Ja |
| ErrorCode | string? | Fehler | Nein |
| Ticket | TravelTicketDto? | Ticket | Nein |
| Route | TravelRouteDto? | Route | Nein |
| BoardingMs | int? | Boarding Zeit | Nein |

### Flow
```
TaxiRequest -> TaxiConfirm (Success) -> Boarding -> FlightStart
TaxiRequest -> TaxiConfirm (Failure) -> Idle
```

### Erwartete Response
- Keine; FlightStart folgt bei Erfolg.

### Code-Beispiel
```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TaxiConfirm)]
public class TaxiConfirm : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TaxiConfirm;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public string? ErrorCode { get; init; }
    [Key(3)] public TravelTicketDto? Ticket { get; init; }
    [Key(4)] public TravelRouteDto? Route { get; init; }
    [Key(5)] public int? BoardingMs { get; init; }
}
```

---

## 🗑️ Obsolete Messages

Derzeit sind keine Transportation-Messages (4200–4299) als obsolet markiert. Künftige Deprecations müssen explizit in diesem Abschnitt ergänzt werden.

---

## 🧨 Edge Cases & Fehlerfälle

- **Doppelte Requests**: Gleicher `ClientSequence` → identische Response, kein Doppel-Billing.
- **Combat während Boarding**: `FlightEnd` mit `EndReason=Interrupted`, Refund 50%.
- **Disconnect während Travel**: `FlightEnd` mit `EndReason=Disconnect`, Player wird auf sicheren Segmentpunkt gesetzt.
- **Summon Timeout**: 30s → `SummonFailed` ErrorCode=`TIMEOUT`.
- **MeetingStone Timeout**: 120s → `MeetingStoneResult` Success=false, ErrorCode=`TIMEOUT`.
- **Seat Race**: Mehrere Spieler wählen gleichen Sitz → erster gewinnt, andere `ErrorMessage` `SEAT_TAKEN`.
- **Portal Expired**: PortalExpire gesendet; PortalUse danach → `ErrorMessage` `TARGET_UNAVAILABLE`.
- **Invalid Signature**: `TaxiConfirm` Success=false, ErrorCode=`INVALID_SIGNATURE`, Audit-Log.
- **Insufficient Funds**: `TaxiConfirm` Success=false, ErrorCode=`INSUFFICIENT_FUNDS`.
- **Rate Limit**: `ErrorMessage` `RATE_LIMITED`, Cooldown 10s.
- **Zone Locked**: `ZoneTransferResponse` ErrorCode=`ZONE_LOCKED`.
- **Hearthstone in Instanz Combat**: `ZoneTransferResponse` ErrorCode=`IN_COMBAT`.
- **Vehicle Damage**: `VehicleControl` mit StateFlags.Damaged, bestimmte Abilities deaktiviert.
- **Boat/Zeppelin Desync**: Server sendet neues Arrival/Departure mit korrigiertem `EtaMs`.

---

## 🔍 Validierungsmatrix pro Transporttyp

### Taxi/Flight
| Check | Server-Quelle | ErrorCode bei Fail | Idempotenzwirkung |
|-------|---------------|--------------------|-------------------|
| Node existiert | TravelPathState | NODE_NOT_FOUND | Antwort reuse |
| Route existiert | TravelPathState | ROUTE_NOT_FOUND | Antwort reuse |
| Nodes discovered | PlayerTravelData | NOT_DISCOVERED | Antwort reuse |
| Funds ausreichend | EconomyService | INSUFFICIENT_FUNDS | Antwort reuse |
| Combat frei | CombatState | IN_COMBAT | Antwort reuse |
| Cooldown frei | TravelCooldowns | COOLDOWN_ACTIVE | Antwort reuse |
| Already traveling | TravelSession | ALREADY_TRAVELING | Antwort reuse |

### Portal
| Check | Quelle | ErrorCode |
|-------|--------|-----------|
| PortalNode aktiv | PortalManager | TARGET_UNAVAILABLE |
| Zone unlocked | ZoneService | ZONE_LOCKED |
| Faction passt | PlayerFaction | REQUIRES_FACTION |
| Discovery (falls nötig) | PlayerTravelData | NOT_DISCOVERED |

### Hearthstone
| Check | Quelle | ErrorCode |
|-------|--------|-----------|
| Cooldown frei | TravelCooldowns | COOLDOWN_ACTIVE |
| Combat frei | CombatState | IN_COMBAT |
| Bind existiert | PlayerBindData | NODE_NOT_FOUND |
| Zone unlocked | ZoneService | ZONE_LOCKED |

### Summon
| Check | Quelle | ErrorCode |
|-------|--------|-----------|
| RequestId gültig | SummonSession | TIMEOUT |
| Target in Reichweite/valid | PositionService | TARGET_UNAVAILABLE |
| Combat erlaubt? | CombatState | IN_COMBAT |
| ExpiresAt nicht überschritten | SummonSession | TIMEOUT |

### MeetingStone
| Check | Quelle | ErrorCode |
|-------|--------|-----------|
| Stone existiert | TravelPathState | NODE_NOT_FOUND |
| Role gültig | PartyService | INVALID_ROLE |
| Already queued | QueueState | ALREADY_TRAVELING |

### Vehicle
| Check | Quelle | ErrorCode |
|-------|--------|-----------|
| Vehicle existiert | VehicleService | TARGET_UNAVAILABLE |
| Seat frei | VehicleState | SEAT_TAKEN |
| SeatFlags erlaubt | VehicleState | NOT_AUTHORIZED |
| ControlOwner gültig | VehicleState | NOT_AUTHORIZED |

---

## 🛠️ Recovery Playbooks

### Reconnect während Flight
1. Server erkennt Disconnect → sendet `FlightEnd EndReason=Disconnect`.
2. TravelSession wird beendet, Player auf sicheren Segmentpunkt gesetzt.
3. Bei Reconnect erhält Spieler `ZoneState` mit neuer Position.
4. Optional: Offer `TaxiRequest` re-try via UI suggestion.

### Crash während Portal/Hearth/Summon
1. TeleportRequestContext bleibt 10s gültig (`ExpiresAt`).
2. Bei Reconnect prüft Server offene Teleport-Kontexte:
   - Falls noch gültig → sendet `ZoneTransferResponse` erneut.
   - Falls abgelaufen → schließt mit `ErrorCode=TIMEOUT`.

### MeetingStone Timeout
1. Queue überschreitet 120s → `MeetingStoneResult Success=false TIMEOUT`.
2. Client leert lokale Queue-UI, kann direkt neuen `MeetingStoneQueue` senden.

### Vehicle Control Loss
1. ControlOwner disconnects → Server weist ControlOwner auf null und broadcastet `VehicleControl`.
2. Sitze bleiben belegt; nach 5s Leerlauf werden Passagiere gedismountet (serverseitig Movement Snap).

### Boat/Zeppelin/Tram Schedule Drift
1. Scheduler vergleicht reale Serverzeit mit Fahrplan.
2. Bei Drift > 5s → sendet neues `Arrival` + `Departure` mit aktualisiertem ETA.
3. Clients passen UI automatisch an.

---

## 📑 Nichtfunktionale Anforderungen

| Kategorie | Ziel | Messgröße |
|-----------|------|-----------|
| Latenz | < 150ms P95 für TaxiConfirm | Server Timer/Tracing |
| Payload-Größe | FlightStart < 3 KB | MessagePack Size |
| Verfügbarkeit | 99.5% Transport-Services | Uptime SLI |
| Reconnect-Sicherheit | 100% Konsistenz nach DC | TravelSession Audit |
| Idempotenz | Kein Doppel-Billing | Billing Audit |
| Bandbreite | FlightPathUpdate Deltas < 10% Full | Revision Vergleich |

### Performance-Hinweise
- Deltas bevorzugen: FullSync nur bei Revision-Mismatch.
- Keine string Interpolation in Hot Paths (use templates).
- Entity/Seat Lists begrenzen (max 64 Seats).

---

## 📊 Datenpersistenz

- **TravelSession**: flüchtig, endet mit FlightEnd.
- **TravelPathState**: serverseitig versioniert; Clients cachen pro Account.
- **Hearthstone Bind**: persistent pro Charakter (DB).
- **Tickets**: nicht persistent; werden nach EndReason gelöscht.
- **Summon Sessions**: im Memory + optional Redis für Multi-Server.

---

## 🧾 Audit-Events

| Event | Trigger | Payload |
|-------|---------|---------|
| travel.taxi.requested | TaxiRequest erhalten | accountId, fromNodeId, toNodeId, routeId |
| travel.taxi.confirmed | TaxiConfirm success | ticketId, cost |
| travel.flight.ended | FlightEnd | endReason, refund |
| travel.portal.used | PortalUse | portalNodeId, result |
| travel.hearth.used | HearthstoneUse | bindZoneId, result |
| travel.summon.accepted | SummonAccept | requestId, summonerId |
| travel.summon.failed | SummonFailed | requestId, errorCode |
| travel.vehicle.mount | VehicleMount | vehicleId, seatIndex, result |
| travel.vehicle.dismount | VehicleDismount | vehicleId |

---

## 📎 Anhang (MessageType Enum Updates)

Quelle: `shared/Mmo.Shared/Messaging/Enums/MessageType.cs` – Transport Range 4200–4299 (keine neuen Werte hinzugefügt, Reihenfolge unverändert).

```csharp
// ═══════════════════════════════════════════════════════════════
// TRANSPORTATION (4200-4299)
// ═══════════════════════════════════════════════════════════════
FlightStart = 4200,
FlightEnd = 4201,
FlightCancel = 4202,
FlightPathUpdate = 4203,
PortalUse = 4210,
PortalCreate = 4211,
PortalExpire = 4212,
HearthstoneUse = 4220,
HearthstoneSet = 4221,
HearthstoneCooldown = 4222,
SummonRequest = 4230,
SummonAccept = 4231,
SummonDecline = 4232,
SummonComplete = 4233,
SummonFailed = 4234,
MeetingStoneQueue = 4235,
MeetingStoneResult = 4236,
VehicleMount = 4240,
VehicleDismount = 4241,
VehicleControl = 4242,
VehicleAbility = 4243,
BoatArrival = 4250,
BoatDeparture = 4251,
ZeppelinArrival = 4252,
ZeppelinDeparture = 4253,
TramArrival = 4254,
TramDeparture = 4255,
TaxiRequest = 4260,
TaxiConfirm = 4261,
```
