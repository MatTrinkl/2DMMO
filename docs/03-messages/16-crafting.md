# 🔨 Crafting Messages (1600-1699)

**Kategorie:** 16  
**Range:** 1600-1699  

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [CraftingOpen (1600)](#craftingopen-1600)
- [CraftingClose (1601)](#craftingclose-1601)
- [CraftingRecipeList (1602)](#craftingrecipelist-1602)
- [CraftingStart (1603)](#craftingstart-1603)
- [CraftingProgress (1604)](#craftingprogress-1604)
- [CraftingComplete (1605)](#craftingcomplete-1605)
- [CraftingFailed (1606)](#craftingfailed-1606)
- [CraftingCancel (1607)](#craftingcancel-1607)
- [CraftingQueue (1608)](#craftingqueue-1608)
- [CraftingQueueAdd (1609)](#craftingqueueadd-1609)
- [CraftingQueueRemove (1610)](#craftingqueueremove-1610)
- [RecipeLearn (1611)](#recipelearn-1611)
- [RecipeUnlearn (1612)](#recipeunlearn-1612)
- [RecipeDiscovery (1613)](#recipediscovery-1613)
- [ProfessionInfo (1620)](#professioninfo-1620)
- [ProfessionLevelUp (1621)](#professionlevelup-1621)
- [ProfessionSkillUp (1622)](#professionskillup-1622)
- [GatheringStart (1630)](#gatheringstart-1630)
- [GatheringProgress (1631)](#gatheringprogress-1631)
- [GatheringComplete (1632)](#gatheringcomplete-1632)
- [GatheringFailed (1633)](#gatheringfailed-1633)
- [GatheringInterrupt (1634)](#gatheringinterrupt-1634)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Crafting- und Profession-System** im 2DMMO.

---

## CraftingOpen (1600)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client öffnet Crafting-Interface für eine Profession.

---

## CraftingClose (1601)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client schließt Crafting-Interface.

---

## CraftingRecipeList (1602)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Liste verfügbarer Recipes für Profession.

---

## CraftingStart (1603)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client startet Craft-Vorgang für ein Recipe.

---

## CraftingProgress (1604)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Craft-Progress (für Progress-Bar).

---

## CraftingComplete (1605)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Crafting erfolgreich abgeschlossen. Enthält gecraftetes Item.

---

## CraftingFailed (1606)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Crafting fehlgeschlagen (Materials verloren oder teilweise).

---

## CraftingCancel (1607)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client bricht laufenden Craft-Vorgang ab.

---

## CraftingQueue (1608)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet aktuelle Crafting-Queue.

---

## CraftingQueueAdd (1609)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client fügt Item zur Crafting-Queue hinzu.

---

## CraftingQueueRemove (1610)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client entfernt Item aus Crafting-Queue.

---

## RecipeLearn (1611)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client lernt neues Recipe (von Trainer/Item).

---

## RecipeUnlearn (1612)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client verlernt Recipe (falls unterstützt).

---

## RecipeDiscovery (1613)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Spieler entdeckt neues Recipe durch Experimentieren.

---

## ProfessionInfo (1620)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Profession-Informationen (Level, XP, Recipes).

---

## ProfessionLevelUp (1621)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Profession-Level gestiegen. Neue Recipes freigeschaltet.

---

## ProfessionSkillUp (1622)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Profession-Skill gestiegen durch erfolgreichen Craft.

---

## GatheringStart (1630)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client beginnt Gathering (Mining, Herbalism, etc.).

---

## GatheringProgress (1631)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Gathering-Progress.

---

## GatheringComplete (1632)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Gathering erfolgreich. Materials gesammelt.

---

## GatheringFailed (1633)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Gathering fehlgeschlagen (Node erschöpft, unterbrochen).

---

## GatheringInterrupt (1634)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Gathering durch Damage/Movement unterbrochen.

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0  
**Status**: ✅ Aligned mit MessageType Enum (22 Messages)

[← Zurück zur Übersicht](README.md)
