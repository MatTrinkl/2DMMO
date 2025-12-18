namespace Mmo.Shared.Enums.Entities;

public enum EntityType : byte
{
    None = 0,
    Player = 1,
    Npc = 2,
    Object = 3,      // Später: Truhen, Türen
    Projectile = 4,  // Später: Feuerbälle
    Loot = 5,        // Später: Loot-Bags
}
