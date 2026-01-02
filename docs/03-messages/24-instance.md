# 🏛️ Instance Messages (2400-2499)

**Kategorie:** 24  
**Range:** 2400-2499  

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [InstanceCreate (2400)](#instancecreate-2400)
- [InstanceCreateResult (2401)](#instancecreateresult-2401)
- [InstanceJoin (2402)](#instancejoin-2402)
- [InstanceJoinResult (2403)](#instancejoinresult-2403)
- [InstanceLeave (2404)](#instanceleave-2404)
- [InstanceReset (2405)](#instancereset-2405)
- [InstanceResetResult (2406)](#instanceresetresult-2406)
- [InstanceLockout (2407)](#instancelockout-2407)
- [InstanceLockoutList (2408)](#instancelockoutlist-2408)
- [InstanceDifficultySet (2409)](#instancedifficultyset-2409)
- [InstanceDifficultyVote (2410)](#instancedifficultyvote-2410)
- [InstanceSaved (2411)](#instancesaved-2411)
- [InstanceExtend (2412)](#instanceextend-2412)
- [InstanceEncounterStart (2420)](#instanceencounterstart-2420)
- [InstanceEncounterEnd (2421)](#instanceencounterend-2421)
- [InstanceEncounterUpdate (2422)](#instanceencounterupdate-2422)
- [InstanceBossKill (2423)](#instancebosskill-2423)
- [InstanceWipe (2424)](#instancewipe-2424)
- [InstanceCheckpoint (2425)](#instancecheckpoint-2425)
- [RaidConvert (2430)](#raidconvert-2430)
- [RaidDisband (2431)](#raiddisband-2431)
- [RaidGroupSet (2432)](#raidgroupset-2432)
- [RaidTargetSet (2433)](#raidtargetset-2433)
- [RaidReadyCheck (2434)](#raidreadycheck-2434)
- [RaidReadyResponse (2435)](#raidreadyresponse-2435)
- [DungeonFinderJoin (2440)](#dungeonfinderjoin-2440)
- [DungeonFinderLeave (2441)](#dungeonfinderleave-2441)
- [DungeonFinderUpdate (2442)](#dungeonfinderupdate-2442)
- [DungeonFinderProposal (2443)](#dungeonfinderproposal-2443)
- [DungeonFinderAccept (2444)](#dungeonfinderaccept-2444)
- [DungeonFinderDecline (2445)](#dungeonfinderdecline-2445)
- [RaidFinderJoin (2450)](#raidfinderjoin-2450)
- [RaidFinderLeave (2451)](#raidfinderleave-2451)
- [RaidFinderUpdate (2452)](#raidfinderupdate-2452)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Instanced Content** (Dungeons, Raids, Finder) im 2DMMO.

---

## InstanceCreate (2400)
**Richtung:** 📤 Client → Server
### Beschreibung
Client erstellt Instance.

---

## InstanceCreateResult (2401)
**Richtung:** 📥 Server → Client
### Beschreibung
Server bestätigt Instance-Erstellung.

---

## InstanceJoin (2402)
**Richtung:** 📤 Client → Server
### Beschreibung
Client betritt Instance.

---

## InstanceJoinResult (2403)
**Richtung:** 📥 Server → Client
### Beschreibung
Server bestätigt Instance-Beitritt.

---

## InstanceLeave (2404)
**Richtung:** 📤 Client → Server
### Beschreibung
Client verlässt Instance.

---

## InstanceReset (2405)
**Richtung:** 📤 Client → Server
### Beschreibung
Client resettet Instance.

---

## InstanceResetResult (2406)
**Richtung:** 📥 Server → Client
### Beschreibung
Server bestätigt Instance-Reset.

---

## InstanceLockout (2407)
**Richtung:** 📥 Server → Client
### Beschreibung
Server informiert über Lockout.

---

## InstanceLockoutList (2408)
**Richtung:** 📥 Server → Client
### Beschreibung
Server sendet Lockout-Liste.

---

## InstanceDifficultySet (2409)
**Richtung:** 📤 Client → Server
### Beschreibung
Client setzt Schwierigkeit.

---

## InstanceDifficultyVote (2410)
**Richtung:** 📤 Client → Server
### Beschreibung
Client stimmt für Schwierigkeit.

---

## InstanceSaved (2411)
**Richtung:** 📥 Server → Client
### Beschreibung
Instance wurde gespeichert.

---

## InstanceExtend (2412)
**Richtung:** 📤 Client → Server
### Beschreibung
Client verlängert Lockout.

---

## InstanceEncounterStart (2420)
**Richtung:** 📥 Server → Client
### Beschreibung
Encounter beginnt.

---

## InstanceEncounterEnd (2421)
**Richtung:** 📥 Server → Client
### Beschreibung
Encounter endet.

---

## InstanceEncounterUpdate (2422)
**Richtung:** 📥 Server → Client
### Beschreibung
Encounter-Status Update.

---

## InstanceBossKill (2423)
**Richtung:** 📥 Server → Client
### Beschreibung
Boss wurde getötet.

---

## InstanceWipe (2424)
**Richtung:** 📥 Server → Client
### Beschreibung
Gruppe ist gewipet.

---

## InstanceCheckpoint (2425)
**Richtung:** 📥 Server → Client
### Beschreibung
Checkpoint erreicht.

---

## RaidConvert (2430)
**Richtung:** 📤 Client → Server
### Beschreibung
Gruppe zu Raid konvertieren.

---

## RaidDisband (2431)
**Richtung:** 📤 Client → Server
### Beschreibung
Raid auflösen.

---

## RaidGroupSet (2432)
**Richtung:** 📤 Client → Server
### Beschreibung
Raid-Gruppen setzen.

---

## RaidTargetSet (2433)
**Richtung:** 📤 Client → Server
### Beschreibung
Raid-Target-Marker setzen.

---

## RaidReadyCheck (2434)
**Richtung:** 📤 Client → Server
### Beschreibung
Ready-Check starten.

---

## RaidReadyResponse (2435)
**Richtung:** 📤 Client → Server
### Beschreibung
Ready-Check beantworten.

---

## DungeonFinderJoin (2440)
**Richtung:** 📤 Client → Server
### Beschreibung
Client tritt Dungeon-Finder bei.

---

## DungeonFinderLeave (2441)
**Richtung:** 📤 Client → Server
### Beschreibung
Client verlässt Dungeon-Finder.

---

## DungeonFinderUpdate (2442)
**Richtung:** 📥 Server → Client
### Beschreibung
Dungeon-Finder Status Update.

---

## DungeonFinderProposal (2443)
**Richtung:** 📥 Server → Client
### Beschreibung
Dungeon gefunden - Bestätigung anfordern.

---

## DungeonFinderAccept (2444)
**Richtung:** 📤 Client → Server
### Beschreibung
Client akzeptiert Dungeon.

---

## DungeonFinderDecline (2445)
**Richtung:** 📤 Client → Server
### Beschreibung
Client lehnt Dungeon ab.

---

## RaidFinderJoin (2450)
**Richtung:** 📤 Client → Server
### Beschreibung
Client tritt Raid-Finder bei.

---

## RaidFinderLeave (2451)
**Richtung:** 📤 Client → Server
### Beschreibung
Client verlässt Raid-Finder.

---

## RaidFinderUpdate (2452)
**Richtung:** 📥 Server → Client
### Beschreibung
Raid-Finder Status Update.

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0  
**Status**: ✅ Aligned mit MessageType Enum (35 Messages)

[← Zurück zur Übersicht](README.md)
