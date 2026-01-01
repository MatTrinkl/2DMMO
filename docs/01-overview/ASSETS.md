# 🎨 Assets & Resources

## 2DMMO – Asset Documentation

**Version:** 1.1.0  
**Last Updated:** 2026-01-01  
**Status:** Prototype Phase (Placeholder Assets)

---

## 📋 Overview

This document lists all asset requirements and sources for the project. For the **prototype** we use free placeholder assets. Final assets will be created or commissioned later.

---

## 🎯 Asset Requirements (Prototype)

### Minimum Requirements

| Asset Type | Needed for Prototype | Priority |
|------------|---------------------|----------|
| **Player Sprite** | Yes | 🔴 High |
| **Basic Tileset** (Grass) | Yes | 🔴 High |
| **UI Font** | Yes (Godot Default OK) | 🟡 Medium |
| **Water Tiles** | Optional | 🟢 Low |
| **Tree/Obstacle** | Optional | 🟢 Low |
| **Sound Effects** | No | ⚪ Later |
| **Music** | No | ⚪ Later |

### Technical Specifications

| Property | Value |
|----------|-------|
| **Tile Size** | 64x64 pixels |
| **Sprite Size** | 64x64 pixels (character) |
| **Color Depth** | 32-bit (RGBA) |
| **Format** | PNG (transparent) |
| **Style** | Pixel Art |

---

## 🆓 Free Asset Sources

### Top Recommendations for Prototype

#### 1. Kenney.nl (⭐ Recommended)
> High-quality, consistent assets. Completely free, no attribution needed (CC0).

| Asset Pack | Link | Suitable For |
|------------|------|--------------|
| **Tiny Town** | [kenney.nl/assets/tiny-town](https://kenney.nl/assets/tiny-town) | Tiles, buildings |
| **Tiny Dungeon** | [kenney.nl/assets/tiny-dungeon](https://kenney.nl/assets/tiny-dungeon) | Characters, dungeons |
| **Roguelike RPG Pack** | [kenney.nl/assets/roguelike-rpg-pack](https://kenney.nl/assets/roguelike-rpg-pack) | RPG tiles & sprites |

**Note:** Kenney assets are often 16x16. Can be scaled to 64x64 (nearest neighbor).

---

#### 2. OpenGameArt.org
> Community-driven platform. Note the different licenses!

| Asset Pack | Link | License | Suitable For |
|------------|------|---------|--------------|
| **LPC Sprite Base** | [opengameart.org/content/liberated-pixel-cup-lpc-base-assets](https://opengameart.org/content/liberated-pixel-cup-lpc-base-assets-sprites-702) | CC-BY-SA 3.0 | Characters |
| **Tiny 16 Basic** | [opengameart.org/content/tiny-16-basic](https://opengameart.org/content/tiny-16-basic) | CC0 | Tiles |

---

#### 3. itch.io (Free Assets)
> Large selection, often from indie developers.

| Asset Pack | Link | Price | Suitable For |
|------------|------|-------|--------------|
| **Ninja Adventure** | [pixel-boy.itch.io/ninja-adventure-asset-pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack) | Free | Complete package |

**Filter for free assets:** [itch.io/game-assets/free/tag-pixel-art](https://itch.io/game-assets/free/tag-pixel-art)

---

## 🎮 Recommended Assets for Prototype

### Quick Start Package

**Ninja Adventure Asset Pack** (All-in-One):
👉 [https://pixel-boy.itch.io/ninja-adventure-asset-pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack)

Includes:
- ✅ Characters with animations
- ✅ Tilesets (nature, dungeon, city)
- ✅ Items & icons
- ✅ UI elements
- ✅ **Free** (donation optional)

---

## 📐 Asset Scaling

Since many free assets are 16x16, we need to scale to 64x64:

### In Godot
1. Open import settings
2. Filter: "Nearest" (not Linear!)
3. Scaling in node: Scale = (4, 4)

---

## ⚖️ Watch the Licenses!

| License | Attribution Required? | Commercial OK? |
|---------|----------------------|----------------|
| **CC0** | ❌ No | ✅ Yes |
| **CC-BY** | ✅ Yes | ✅ Yes |
| **CC-BY-SA** | ✅ Yes | ✅ Yes (same license) |
| **CC-BY-NC** | ✅ Yes | ❌ No |

---

## 📝 Asset Tracking

### Used Assets (Prototype)

| Asset | Source | License | Used For |
|-------|--------|---------|----------|
| *None yet* | - | - | - |

---

## 🔗 Useful Tools

| Tool | Link | Purpose |
|------|------|---------|
| **Aseprite** | [aseprite.org](https://www.aseprite.org/) | Pixel art editor |
| **Pixilart** | [pixilart.com](https://www.pixilart.com/) | Online pixel editor (free) |
| **Tiled** | [mapeditor.org](https://www.mapeditor.org/) | Tilemap editor |

---

*This document will be updated when new assets are added.*