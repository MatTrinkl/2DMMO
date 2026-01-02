# 🚀 Transportation Messages (4200-4299)

**Kategorie:** 42  
**Range:** 4200-4299  
**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

### Flight Paths (4200-4203)
- [FlightStart (4200)](#flightstart-4200)
- [FlightEnd (4201)](#flightend-4201)
- [FlightCancel (4202)](#flightcancel-4202)
- [FlightPathUpdate (4203)](#flightpathupdate-4203)

### Portals (4210-4212)
- [PortalUse (4210)](#portaluse-4210)
- [PortalCreate (4211)](#portalcreate-4211)
- [PortalExpire (4212)](#portalexpire-4212)

### Hearthstone (4220-4222)
- [HearthstoneUse (4220)](#hearthstoneuse-4220)
- [HearthstoneSet (4221)](#hearthstoneset-4221)
- [HearthstoneCooldown (4222)](#hearthstonecooldown-4222)

### Summon (4230-4236)
- [SummonRequest (4230)](#summonrequest-4230)
- [SummonAccept (4231)](#summonaccept-4231)
- [SummonDecline (4232)](#summondecline-4232)
- [SummonComplete (4233)](#summoncomplete-4233)
- [SummonFailed (4234)](#summonfailed-4234)
- [MeetingStoneQueue (4235)](#meetingstonequeue-4235)
- [MeetingStoneResult (4236)](#meetingstoneresult-4236)

### Vehicles (4240-4243)
- [VehicleMount (4240)](#vehiclemount-4240)
- [VehicleDismount (4241)](#vehicledismount-4241)
- [VehicleControl (4242)](#vehiclecontrol-4242)
- [VehicleAbility (4243)](#vehicleability-4243)

### Public Transport (4250-4255)
- [BoatArrival (4250)](#boatarrival-4250)
- [BoatDeparture (4251)](#boatdeparture-4251)
- [ZeppelinArrival (4252)](#zeppelinarrival-4252)
- [ZeppelinDeparture (4253)](#zeppelindeparture-4253)
- [TramArrival (4254)](#tramarrival-4254)
- [TramDeparture (4255)](#tramdeparture-4255)

### Taxi (4260-4261)
- [TaxiRequest (4260)](#taxirequest-4260)
- [TaxiConfirm (4261)](#taxiconfirm-4261)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Transportation-Systeme** im 2DMMO.

Das Transportation-System implementiert:
- Flight Paths (automatischer Flug zwischen Flugpunkten)
- Portale (Mage-Portale, Dungeon-Portale)
- Hearthstone (Teleport zum Heimatpunkt)
- Summon (Warlock-Summon, Meeting-Stone)
- Vehicles (Siege-Weapons, Fahrzeuge)
- Public Transport (Boote, Zeppeline, Trams)
- Taxi (Flight Master)

**Server Authority**: Alle Transport-Mechanics sind server-authoritative.

---

## FlightStart (4200)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet einen Flight-Path (automatischer Flug zwischen zwei Flugpunkten).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FlightPathId | uint | Flight-Path-ID | Ja |
| Duration | float | Flugdauer in Sekunden | Ja |
| Waypoints | List<WaypointDto> | Wegpunkte | Ja |

---

## FlightEnd (4201)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Flight-Path beendet (am Ziel angekommen).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Ziel-Position X | Ja |
| Y | float | Ziel-Position Y | Ja |

---

## FlightCancel (4202)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bricht Flight-Path ab (landet am nächsten Flugpunkt).

---

## FlightPathUpdate (4203)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Flight-Path Position Update.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Aktuelle Position X | Ja |
| Y | float | Aktuelle Position Y | Ja |
| Progress | float | Fortschritt (0.0-1.0) | Ja |

---

## PortalUse (4210)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Benutzt ein Portal (Mage-Portal, Dungeon-Portal, Zone-Portal).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PortalId | uint | Portal-Entity-ID | Ja |

---

## PortalCreate (4211)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Portal wurde erstellt (Mage-Portal).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PortalId | uint | Portal-Entity-ID | Ja |
| CasterId | int | Portal-Ersteller | Ja |
| DestinationName | string | Zielort-Name | Ja |
| X | float | Portal-Position X | Ja |
| Y | float | Portal-Position Y | Ja |
| Duration | int | Lebensdauer in Sekunden | Ja |

---

## PortalExpire (4212)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Portal ist abgelaufen/verschwunden.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PortalId | uint | Portal-Entity-ID | Ja |

---

## HearthstoneUse (4220)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Nutzt Hearthstone (Teleport zum gesetzten Heim-Punkt). 10 Sekunden Cast-Time, 30 Minuten Cooldown.

---

## HearthstoneSet (4221)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Setzt den Hearthstone-Punkt (Inn-Keeper oder bestimmte Locations).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InnkeeperId | int | Innkeeper-NPC-ID | Ja |

---

## HearthstoneCooldown (4222)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Hearthstone-Cooldown Update.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CooldownRemaining | int | Verbleibende Sekunden | Ja |

---

## SummonRequest (4230)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Summon-Anfrage erhalten (Warlock-Summon, Meeting-Stone, Group-Summon).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SummonerId | int | Summoner-ID | Ja |
| SummonerName | string | Summoner-Name | Ja |
| DestinationName | string | Zielort-Name | Ja |
| Timeout | int | Sekunden bis Timeout | Ja |

---

## SummonAccept (4231)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Summon annehmen (Teleport zum Summoner).

---

## SummonDecline (4232)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Summon ablehnen.

---

## SummonComplete (4233)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Summon erfolgreich abgeschlossen.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Ziel-Position X | Ja |
| Y | float | Ziel-Position Y | Ja |
| ZoneId | uint | Ziel-Zone | Ja |

---

## SummonFailed (4234)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Summon fehlgeschlagen.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Fehlgrund | Ja |

---

## MeetingStoneQueue (4235)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Queue am Meeting Stone (Dungeon-Summon).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MeetingStoneId | int | Meeting Stone ID | Ja |

---

## MeetingStoneResult (4236)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Meeting Stone Queue Ergebnis.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| QueuePosition | int | Position in Queue | Nein |

---

## VehicleMount (4240)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Besteigt ein Vehicle (Siege-Weapons, Turrets, Mechs, Dragons).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| VehicleId | int | Vehicle-Entity-ID | Ja |
| SeatIndex | byte | Sitz-Index | Ja |

---

## VehicleDismount (4241)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Verlässt das Vehicle.

---

## VehicleControl (4242)

**Richtung:** 📤 Client → Server  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Steuert ein Vehicle (Bewegung, Drehung).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| DirectionX | float | Bewegungsrichtung X | Ja |
| DirectionY | float | Bewegungsrichtung Y | Ja |
| Rotation | float | Rotation | Ja |

---

## VehicleAbility (4243)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Nutzt Vehicle-Ability (Kanone abfeuern, etc.).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AbilityIndex | byte | Ability-Index | Ja |
| TargetX | float | Ziel-Position X | Nein |
| TargetY | float | Ziel-Position Y | Nein |

---

## BoatArrival (4250)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Boot ist angekommen (Transport-Ship zwischen Kontinenten).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BoatId | uint | Boot-ID | Ja |
| DockId | uint | Dock-ID | Ja |
| DepartureIn | int | Abfahrt in Sekunden | Ja |

---

## BoatDeparture (4251)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Boot fährt ab.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BoatId | uint | Boot-ID | Ja |
| DestinationDockId | uint | Ziel-Dock-ID | Ja |

---

## ZeppelinArrival (4252)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Zeppelin ist angekommen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZeppelinId | uint | Zeppelin-ID | Ja |
| TowerId | uint | Tower-ID | Ja |
| DepartureIn | int | Abfahrt in Sekunden | Ja |

---

## ZeppelinDeparture (4253)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Zeppelin fliegt ab.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZeppelinId | uint | Zeppelin-ID | Ja |
| DestinationTowerId | uint | Ziel-Tower-ID | Ja |

---

## TramArrival (4254)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Tram ist angekommen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TramId | uint | Tram-ID | Ja |
| StationId | uint | Station-ID | Ja |
| DepartureIn | int | Abfahrt in Sekunden | Ja |

---

## TramDeparture (4255)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Tram fährt ab.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TramId | uint | Tram-ID | Ja |
| DestinationStationId | uint | Ziel-Station-ID | Ja |

---

## TaxiRequest (4260)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Taxi-Anfrage (Flight-Master Interaction, zeigt verfügbare Flugpunkte).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FlightMasterId | int | Flight Master NPC-ID | Ja |

---

## TaxiConfirm (4261)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigt Taxi-Flug zum ausgewählten Flugpunkt.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| DestinationNodeId | uint | Ziel-Flugpunkt-ID | Ja |

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
