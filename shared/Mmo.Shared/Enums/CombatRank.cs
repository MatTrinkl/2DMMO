namespace Mmo.Shared.Enums;

/// <summary>
///     Kampfstärke einer Entity (beeinflusst HP, Damage, XP, Loot).
/// </summary>
public enum CombatRank : byte
{
    Trivial = 0,      // Keine XP, keine Bedrohung (Critter, graue Mobs)
    Normal = 1,       // Standard
    Elite = 2,        // ~3x HP/Damage
    Rare = 3,         // Selten, besserer Loot
    RareElite = 4,    // Selten + Elite
    Boss = 5,         // Dungeon-Boss
    WorldBoss = 6,    // Raid/World-Boss
}
