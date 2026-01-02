# 🐴 Mount Messages (2000-2099)

**Kategorie:** 20  
**Range:** 2000-2099  

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [MountSummon (2000)](#mountsummon-2000)
- [MountSummonResult (2001)](#mountsummonresult-2001)
- [MountDismount (2002)](#mountdismount-2002)
- [MountListRequest (2003)](#mountlistrequest-2003)
- [MountListResponse (2004)](#mountlistresponse-2004)
- [MountFavorite (2005)](#mountfavorite-2005)
- [MountUnfavorite (2006)](#mountunfavorite-2006)
- [MountRandomFavorite (2007)](#mountrandomfavorite-2007)
- [PetSummon (2020)](#petsummon-2020)
- [PetSummonResult (2021)](#petsummonresult-2021)
- [PetDismiss (2022)](#petdismiss-2022)
- [PetRename (2023)](#petrename-2023)
- [PetCommand (2024)](#petcommand-2024)
- [PetCommandResult (2025)](#petcommandresult-2025)
- [PetUpdate (2026)](#petupdate-2026)
- [PetFeed (2027)](#petfeed-2027)
- [PetTrain (2028)](#pettrain-2028)
- [PetAbandon (2029)](#petabandon-2029)
- [PetStable (2030)](#petstable-2030)
- [PetUnstable (2031)](#petunstable-2031)
- [PetListRequest (2032)](#petlistrequest-2032)
- [PetListResponse (2033)](#petlistresponse-2033)
- [CompanionSummon (2050)](#companionsummon-2050)
- [CompanionDismiss (2051)](#companiondismiss-2051)
- [CompanionInteract (2052)](#companioninteract-2052)
- [CompanionListRequest (2053)](#companionlistrequest-2053)
- [CompanionListResponse (2054)](#companionlistresponse-2054)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Mount-, Pet- und Companion-System** im 2DMMO.

---

## MountSummon (2000)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client ruft Mount herbei.

---

## MountSummonResult (2001)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server bestätigt Mount-Summon.

---

## MountDismount (2002)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client steigt vom Mount ab.

---

## MountListRequest (2003)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client fordert Mount-Collection an.

---

## MountListResponse (2004)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Mount-Collection.

---

## MountFavorite (2005)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client markiert Mount als Favorit.

---

## MountUnfavorite (2006)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client entfernt Favorit-Markierung.

---

## MountRandomFavorite (2007)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client ruft zufälliges Favoriten-Mount.

---

## PetSummon (2020)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client ruft Pet herbei.

---

## PetSummonResult (2021)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server bestätigt Pet-Summon.

---

## PetDismiss (2022)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client schickt Pet weg.

---

## PetRename (2023)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client benennt Pet um.

---

## PetCommand (2024)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client gibt Pet Befehl (Attack, Follow, Stay).

---

## PetCommandResult (2025)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server bestätigt Pet-Befehl.

---

## PetUpdate (2026)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Pet-Status-Update.

---

## PetFeed (2027)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client füttert Pet.

---

## PetTrain (2028)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client trainiert Pet-Fähigkeit.

---

## PetAbandon (2029)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client gibt Pet frei.

---

## PetStable (2030)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client stellt Pet in Stall.

---

## PetUnstable (2031)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client holt Pet aus Stall.

---

## PetListRequest (2032)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client fordert Pet-Liste an.

---

## PetListResponse (2033)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Pet-Liste.

---

## CompanionSummon (2050)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client ruft kosmetischen Companion.

---

## CompanionDismiss (2051)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client schickt Companion weg.

---

## CompanionInteract (2052)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client interagiert mit Companion.

---

## CompanionListRequest (2053)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client fordert Companion-Liste an.

---

## CompanionListResponse (2054)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Companion-Liste.

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0  
**Status**: ✅ Aligned mit MessageType Enum (27 Messages)
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
