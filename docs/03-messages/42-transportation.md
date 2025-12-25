# 🚀 Transportation Messages (4200-4299)

**Kategorie:** 42  
**Range:** 4200-4299  
**Phase:** Phase 3  
**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

1. [FlightStart (4200)](#flightstart-4200) - Flight-Path starten
2. [FlightEnd (4201)](#flightend-4201) - Flight beenden
3. [FlightCancel (4202)](#flightcancel-4202) - Flight abbrechen
4. [FlightPathUpdate (4203)](#flightpathupdate-4203) - Flight-Path Update
5. [PortalUse (4210)](#portaluse-4210) - Portal benutzen
6. [PortalCreate (4211)](#portalcreate-4211) - Portal erzeugen (Mage)
7. [PortalExpire (4212)](#portalexpire-4212) - Portal läuft ab
8. [HearthstoneUse (4220)](#hearthstoneuse-4220) - Hearthstone nutzen
9. [HearthstoneSet (4221)](#hearthstoneset-4221) - Hearthstone setzen
10. [HearthstoneCooldown (4222)](#hearthstonecooldown-4222) - Hearthstone CD
11. [SummonRequest (4230)](#summonrequest-4230) - Summon-Anfrage
12. [SummonAccept (4231)](#summonaccept-4231) - Summon annehmen
13. [SummonDecline (4232)](#summondecline-4232) - Summon ablehnen
14. [SummonComplete (4233)](#summoncomplete-4233) - Summon abgeschlossen
15. [SummonFailed (4234)](#summonfailed-4234) - Summon fehlgeschlagen
16. [MeetingStoneQueue (4235)](#meetingstonequeue-4235) - Meeting-Stone
17. [MeetingStoneResult (4236)](#meetingstoneresult-4236) - Meeting-Stone Result
18. [VehicleMount (4240)](#vehiclemount-4240) - Vehicle besteigen
19. [VehicleDismount (4241)](#vehicledismount-4241) - Vehicle verlassen
20. [VehicleControl (4242)](#vehiclecontrol-4242) - Vehicle steuern
21. [VehicleAbility (4243)](#vehicleability-4243) - Vehicle-Ability nutzen
22. [BoatArrival (4250)](#boatarrival-4250) - Boot-Ankunft
23. [BoatDeparture (4251)](#boatdeparture-4251) - Boot-Abfahrt
24. [TaxiRequest (4260)](#taxirequest-4260) - Taxi-Anfrage

---

## FlightStart (4200)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Startet einen Flight-Path (automatischer Flug zwischen zwei Flugpunkten).

### Im Scope ✅
- Flight-Path starten mit Wegpunkt-Route
- Geschwindigkeit und Dauer
- Cinematischer Flug-Modus

### Nicht im Scope ❌
- Manuelle Steuerung → use `Mount` (20xx)
- Vehicle-Control → use `VehicleControl` (4242)

---

## PortalUse (4210)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Benutzt ein Portal (Mage-Portal, Dungeon-Portal, Zone-Portal).

---

## HearthstoneUse (4220)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Nutzt Hearthstone (Teleport zum gesetzten Heim-Punkt). 10 Sekunden Cast-Time, 30 Minuten Cooldown.

---

## HearthstoneSet (4221)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Setzt den Hearthstone-Punkt (Inn-Keeper oder bestimmte Locations).

---

## SummonRequest (4230)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Summon-Anfrage erhalten (Warlock-Summon, Meeting-Stone, Group-Summon).

---

## SummonAccept (4231)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Summon annehmen (Teleport zum Summoner).

---

## VehicleMount (4240)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Besteigt ein Vehicle (Siege-Weapons, Turrets, Mechs, Dragons).

---

## VehicleControl (4242)

**Richtung:** 📤 Client → Server  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

Steuert ein Vehicle (Bewegung, Drehung).

---

## BoatArrival (4250)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein

Boot ist angekommen (Transport-Ship zwischen Kontinenten).

---

## TaxiRequest (4260)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Taxi-Anfrage (Flight-Master Interaction, zeigt verfügbare Flugpunkte).

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
