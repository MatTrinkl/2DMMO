# 🎤 Voice Chat / Audio Messages (3500-3599)

**Kategorie:** 35  
**Range:** 3500-3599  

**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

1. [VoiceJoinChannel (3500)](#voicejoinchannel-3500)
2. [VoiceJoinResult (3501)](#voicejoinresult-3501)
3. [VoiceLeaveChannel (3502)](#voiceleavechannel-3502)
4. [VoiceChannelList (3503)](#voicechannellist-3503)
5. [VoiceMute (3510)](#voicemute-3510)
6. [VoiceUnmute (3511)](#voiceunmute-3511)
7. [VoiceDeafen (3512)](#voicedeafen-3512)
8. [VoiceUndeafen (3513)](#voiceundeafen-3513)
9. [VoiceSpeaking (3514)](#voicespeaking-3514)
10. [VoiceVolume (3515)](#voicevolume-3515)
11. [VoiceData (3520)](#voicedata-3520)
12. [AudioTrigger (3530)](#audiotrigger-3530)
13. [AudioStop (3531)](#audiostop-3531)
14. [MusicChange (3532)](#musicchange-3532)
15. [AmbienceChange (3533)](#ambiencechange-3533)

---

## VoiceJoinChannel (3500)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Tritt einem Voice-Channel bei (Party, Guild, Proximity, etc.).

### Im Scope ✅
- Voice-Channel beitreten
- Channel-Type Validierung
- Push-to-Talk vs Voice-Activation

### Nicht im Scope ❌
- Audio-Streaming → use `VoiceData` (3520)
- Mute/Deafen → use `VoiceMute` (3510)

---

## VoiceJoinResult (3501)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Bestätigung des Voice-Channel-Beitritts.

---

## VoiceLeaveChannel (3502)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Verlässt einen Voice-Channel.

---

## VoiceChannelList (3503)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Liste der verfügbaren Voice-Channels.

---

## VoiceMute (3510)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja

Muted das eigene Mikrofon.

---

## VoiceUnmute (3511)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja

Unmuted das eigene Mikrofon.

---

## VoiceDeafen (3512)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja

Deafened (hört andere nicht mehr).

---

## VoiceUndeafen (3513)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja

Undeafened (hört andere wieder).

---

## VoiceSpeaking (3514)

**Richtung:** 📡 Broadcast  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

Broadcast wenn ein Spieler spricht (Voice-Activity-Detection).

---

## VoiceVolume (3515)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Lautstärke-Anpassung für andere Spieler.

---

## VoiceData (3520)

**Richtung:** ⚡ Bidirectional  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

Voice-Data Streaming (Opus-Codec, 20ms Packets).

---

## AudioTrigger (3530)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein

Triggert einen Sound-Effect (Skills, Ambient, UI).

---

## AudioStop (3531)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein

Stoppt einen laufenden Sound-Effect.

---

## MusicChange (3532)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein

Ändert die Hintergrundmusik (Zone-Change, Combat-State).

---

## AmbienceChange (3533)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein

Ändert die Ambient-Sounds (Zone-Change, Weather, Time-of-Day).

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
