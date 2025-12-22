# 🔒 Sicherheit

## 2DMMO – Security Architecture

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](README.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt die Sicherheitsarchitektur für das 2DMMO, einschließlich Security Layers, Input Validation und Anti-Cheat Maßnahmen.

---

## Security Layers

```
┌─────────────────────────────────────────────────────────┐
│                  SECURITY LAYERS                         │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 1: TRANSPORT (TLS 1.3)                   │    │
│  │                                                  │    │
│  │  • Verschlüsselte Verbindung                    │    │
│  │  • Server-Authentifizierung (Zertifikat)        │    │
│  │  • Man-in-the-Middle Schutz                     │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 2: AUTHENTICATION                        │    │
│  │                                                  │    │
│  │  • Passwort-Hashing (Argon2id)                  │    │
│  │  • Session-Tokens (UUIDv4)                      │    │
│  │  • Session-Timeout (30 Minuten)                 │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 3: AUTHORIZATION                         │    │
│  │                                                  │    │
│  │  • Spieler kann nur eigene Aktionen senden      │    │
│  │  • Zone-Zugehörigkeit wird geprüft              │    │
│  │  • PvP-Flag Status wird validiert               │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 4: VALIDATION (Anti-Cheat)               │    │
│  │                                                  │    │
│  │  • Speed-Hack Detection                         │    │
│  │  • Teleport Detection                           │    │
│  │  • Action-Rate Limiting                         │    │
│  │  • Damage-Plausibility Checks                   │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 5: RATE LIMITING                         │    │
│  │                                                  │    │
│  │  • Connection Rate: 5/min pro IP                │    │
│  │  • Login Rate: 3/min pro Account                │    │
│  │  • Chat Rate: 10/min pro Spieler                │    │
│  │  • Action Rate: 20/sec pro Spieler              │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

---

## Input Validation

```csharp
public class MovementValidator
{
    private const float MAX_SPEED = 10f;  // Einheiten/Sekunde
    private const float TOLERANCE = 1.2f;  // 20% Toleranz für Latenz
    
    public ValidationResult ValidateMovement(
        PlayerState current,
        PositionUpdate update,
        float deltaTime)
    {
        // 1. Besitzt der Spieler diese Session?
        if (update.PlayerId != current.PlayerId)
            return ValidationResult.Fail("Invalid player ID");
        
        // 2. Sequenznummer aufsteigend?
        if (update.SequenceNumber <= current.LastSequence)
            return ValidationResult.Fail("Stale sequence number");
        
        // 3. Bewegungsgeschwindigkeit plausibel?
        float distance = Vector2.Distance(
            new Vector2(current.X, current.Y),
            new Vector2(update.X, update.Y));
        
        float maxDistance = MAX_SPEED * deltaTime * TOLERANCE;
        
        if (distance > maxDistance)
            return ValidationResult.Fail("Speed hack detected");
        
        // 4. Position innerhalb der Welt?
        if (!WorldBounds.Contains(update.X, update.Y))
            return ValidationResult.Fail("Position out of bounds");
        
        // 5. Keine Kollision mit Wänden/Hindernissen?
        if (CollisionSystem.CheckCollision(update.X, update.Y))
            return ValidationResult.Fail("Collision detected");
        
        return ValidationResult.Success();
    }
}
```

---

## Rate Limiting Details

| Aktion | Limit | Zeitraum | Konsequenz |
|--------|-------|----------|------------|
| Connection | 5 | 1 Minute | IP temporär gesperrt |
| Login | 3 | 1 Minute | Account temporär gesperrt |
| Chat | 10 | 1 Minute | Message wird verworfen |
| Action | 20 | 1 Sekunde | Action wird verworfen |

---

## Anti-Cheat Maßnahmen

### Speed-Hack Detection

- Maximale Bewegungsgeschwindigkeit wird server-seitig geprüft
- 20% Toleranz für Netzwerk-Latenz
- Bei Überschreitung: Position wird zurückgesetzt

### Teleport Detection

- Plötzliche Positionsänderungen werden erkannt
- Distanz zwischen zwei Updates darf Maximum nicht überschreiten
- Bei Überschreitung: Kick mit Warnung

### Damage Validation

- Server berechnet Schaden selbst
- Client-Angaben werden nur als Input verwendet
- Unmögliche Schadenswerte werden ignoriert

---

## Verwandte Dokumentation

- [Netzwerk-Protokoll](NETWORK_PROTOCOL.md) - TLS Implementation
- [Messages](MESSAGES.md) - Input Format
- [Redis-Strategie](REDIS.md) - Rate Limiting Storage

---

## 🔗 Nützliche Links

- [OWASP Game Security Framework](https://owasp.org/www-project-game-security-framework/)
- [Argon2 Password Hashing](https://github.com/P-H-C/phc-winner-argon2)
- [TLS 1.3 Best Practices](https://wiki.mozilla.org/Security/Server_Side_TLS)

---

*Teil der [Architektur-Dokumentation](README.md)*
