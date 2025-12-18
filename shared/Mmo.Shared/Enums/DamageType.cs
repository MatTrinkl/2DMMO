namespace Mmo.Shared.Enums;

public enum DamageType : byte
{
    Physical = 0,
    Fire = 1,
    Ice = 2,
    Lightning = 3,
    Arcane = 4,
    Holy = 5,
    Shadow = 6,
    Nature = 7,
    True = 8, // Ignoriert Armor/Resistances
}
