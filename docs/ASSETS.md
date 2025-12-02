# 🎨 Assets & Ressourcen

## 2DMMO – Asset-Dokumentation

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-02  
**Status:** Prototyp-Phase (Placeholder-Assets)

---

## 📋 Übersicht

Dieses Dokument listet alle Asset-Anforderungen und Quellen für das Projekt. Für den **Prototyp** verwenden wir kostenlose Placeholder-Assets. Finale Assets werden später erstellt oder in Auftrag gegeben.

---

## 🎯 Asset-Anforderungen (Prototyp)

### Minimale Anforderungen

| Asset-Typ | Benötigt für Prototyp | Priorität |
|-----------|----------------------|-----------|
| **Spieler-Sprite** | Ja | 🔴 Hoch |
| **Basis-Tileset** (Gras) | Ja | 🔴 Hoch |
| **UI-Font** | Ja (Godot Default OK) | 🟡 Mittel |
| **Wasser-Tiles** | Optional | 🟢 Niedrig |
| **Baum/Hindernis** | Optional | 🟢 Niedrig |
| **Sound-Effekte** | Nein | ⚪ Später |
| **Musik** | Nein | ⚪ Später |

### Technische Spezifikationen

| Eigenschaft | Wert |
|-------------|------|
| **Tile-Größe** | 64x64 Pixel |
| **Sprite-Größe** | 64x64 Pixel (Charakter) |
| **Farbtiefe** | 32-bit (RGBA) |
| **Format** | PNG (transparent) |
| **Stil** | Pixel Art |

---

## 🆓 Kostenlose Asset-Quellen

### Top-Empfehlungen für Prototyp

#### 1. Kenney.nl (⭐ Empfohlen)
> Hochwertige, konsistente Assets. Komplett kostenlos, keine Attribution nötig (CC0).

| Asset-Pack | Link | Passt für |
|------------|------|-----------|
| **Tiny Town** | [kenney.nl/assets/tiny-town](https://kenney.nl/assets/tiny-town) | Tiles, Gebäude |
| **Tiny Dungeon** | [kenney.nl/assets/tiny-dungeon](https://kenney.nl/assets/tiny-dungeon) | Charaktere, Dungeons |
| **Roguelike RPG Pack** | [kenney.nl/assets/roguelike-rpg-pack](https://kenney.nl/assets/roguelike-rpg-pack) | RPG Tiles & Sprites |

**Hinweis:** Kenney Assets sind oft 16x16. Können auf 64x64 skaliert werden (nearest neighbor).

---

#### 2. OpenGameArt.org
> Community-getriebene Plattform. Verschiedene Lizenzen beachten!

| Asset-Pack | Link | Lizenz | Passt für |
|------------|------|--------|-----------|
| **LPC Sprite Base** | [opengameart.org/content/liberated-pixel-cup-lpc-base-assets](https://opengameart.org/content/liberated-pixel-cup-lpc-base-assets-sprites-702) | CC-BY-SA 3.0 | Charaktere |
| **Tiny 16 Basic** | [opengameart.org/content/tiny-16-basic](https://opengameart.org/content/tiny-16-basic) | CC0 | Tiles |

---

#### 3. itch.io (Free Assets)
> Große Auswahl, oft von Indie-Entwicklern.

| Asset-Pack | Link | Preis | Passt für |
|------------|------|-------|-----------|
| **Ninja Adventure** | [pixel-boy.itch.io/ninja-adventure-asset-pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack) | Free | Komplett-Paket |

**Filter für kostenlose Assets:** [itch.io/game-assets/free/tag-pixel-art](https://itch.io/game-assets/free/tag-pixel-art)

---

## 🎮 Empfohlene Assets für Prototyp

### Schnellstart-Paket

**Ninja Adventure Asset Pack** (All-in-One):
👉 [https://pixel-boy.itch.io/ninja-adventure-asset-pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack)

Enthält:
- ✅ Charaktere mit Animationen
- ✅ Tilesets (Natur, Dungeon, Stadt)
- ✅ Items & Icons
- ✅ UI-Elemente
- ✅ **Kostenlos** (Donation optional)

---

## 📐 Asset-Skalierung

Da viele kostenlose Assets 16x16 sind, müssen wir auf 64x64 skalieren:

### In Godot
1. Import-Einstellungen öffnen
2. Filter: "Nearest" (nicht Linear!)
3. Skalierung im Node: Scale = (4, 4)

---

## ⚖️ Lizenzen beachten!

| Lizenz | Attribution nötig? | Kommerziell OK? |
|--------|-------------------|-----------------|
| **CC0** | ❌ Nein | ✅ Ja |
| **CC-BY** | ✅ Ja | ✅ Ja |
| **CC-BY-SA** | ✅ Ja | ✅ Ja (gleiche Lizenz) |
| **CC-BY-NC** | ✅ Ja | ❌ Nein |

---

## 📝 Asset-Tracking

### Verwendete Assets (Prototyp)

| Asset | Quelle | Lizenz | Verwendet für |
|-------|--------|--------|---------------|
| *Noch keine* | - | - | - |

---

## 🔗 Nützliche Tools

| Tool | Link | Zweck |
|------|------|-------|
| **Aseprite** | [aseprite.org](https://www.aseprite.org/) | Pixel Art Editor |
| **Pixilart** | [pixilart.com](https://www.pixilart.com/) | Online Pixel Editor (kostenlos) |
| **Tiled** | [mapeditor.org](https://www.mapeditor.org/) | Tilemap Editor |

---

*Dieses Dokument wird aktualisiert wenn neue Assets hinzugefügt werden.*