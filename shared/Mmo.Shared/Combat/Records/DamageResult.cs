using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Movement.Records;

namespace Mmo.Shared.Combat.Records;

/// <summary>
///     Result of a damage calculation.
/// </summary>
public readonly record struct DamageResult(
    int RawDamage, // Original damage
    int MitigatedDamage, // After Armor/Resistances
    int AbsorbedDamage, // Absorbed by shields
    int ActualDamage, // Actual damage taken
    int Overkill, // Excess (on death)
    bool IsCritical,
    bool IsBlocked,
    bool IsDodged,
    bool IsParried,
    bool IsMiss,
    bool IsImmune,
    DamageType DamageType
);

/// <summary>
///     Result of a heal.
/// </summary>
public readonly record struct HealResult(
    int RawHeal, // Original heal
    int ActualHeal, // Actual heal
    int Overheal, // Overheal
    bool IsCritical
);

/// <summary>
///     Hearthstone/Bind-Location.
/// </summary>
public readonly record struct BindLocation(
    ushort ZoneId,
    Position Position,
    string ZoneName
);
