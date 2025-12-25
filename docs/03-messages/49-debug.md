# 🐛 Debug / Development Messages (4900-4999)

**Kategorie:** 49  
**Range:** 4900-4999  
**Phase:** Development  
**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

1. [DebugCommand (4900)](#debugcommand-4900) - Debug-Command ausführen
2. [DebugResponse (4901)](#debugresponse-4901) - Debug-Response
3. [DebugLog (4902)](#debuglog-4902) - Debug-Log senden
4. [DebugTeleport (4903)](#debugteleport-4903) - Debug-Teleport
5. [DebugSpawn (4904)](#debugspawn-4904) - Debug-Spawn Entity

---

## ⚠️ Warnung

**Diese Messages sind NUR für Development und MÜSSEN in Production deaktiviert sein!**

---

## DebugCommand (4900)

**Richtung:** 📤 Client → Server  
**Frequenz:** Development-Only  
**Authentifizierung:** 🔒 Ja (Admin/Developer-Only)

Führt einen Debug-Command aus (z.B. `/debug spawn_npc 100`, `/debug set_level 60`).

### Im Scope ✅
- Arbitrary Debug-Commands
- Parameter-Parsing
- Developer-Permission Check

### Nicht im Scope ❌
- Production-Environment (MUSS deaktiviert sein!)
- Regular Admin-Commands → use `AdminCommand` (23xx)

### Security ⚠️
- **NIEMALS in Production aktivieren!**
- Nur für lokale Development-Builds
- Überprüfe `IS_DEVELOPMENT` Flag

---

## DebugResponse (4901)

**Richtung:** 📥 Server → Client  
**Frequenz:** Development-Only  
**Authentifizierung:** 🔒 Ja

Response auf Debug-Command mit Ergebnis/Fehler.

---

## DebugLog (4902)

**Richtung:** 📥 Server → Client  
**Frequenz:** Development-Only  
**Authentifizierung:** 🔒 Ja

Server-Log-Message an Client senden (für Debugging ohne Server-Zugriff).

---

## DebugTeleport (4903)

**Richtung:** 📤 Client → Server  
**Frequenz:** Development-Only  
**Authentifizierung:** 🔒 Ja (Developer-Only)

Teleport zu beliebigen Koordinaten (Bypass aller Checks).

---

## DebugSpawn (4904)

**Richtung:** 📤 Client → Server  
**Frequenz:** Development-Only  
**Authentifizierung:** 🔒 Ja (Developer-Only)

Spawnt eine Entity (NPC, Monster, Object) an beliebiger Position.

---

## 🔒 Security-Hinweise

1. **Production-Check**: `if (IS_PRODUCTION) return ErrorCode.NOT_IMPLEMENTED;`
2. **Developer-Only**: Nur Developer-Accounts erlauben
3. **Rate-Limiting**: Auch in Development, um Spam zu vermeiden
4. **Logging**: Alle Debug-Commands loggen für Audit
5. **Build-Flag**: Komplett aus Production-Builds entfernen (`#if DEBUG`)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
