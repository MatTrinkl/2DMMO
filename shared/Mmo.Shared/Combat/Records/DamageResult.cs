using Mmo.Shared.Combat.Enums;
using Mmo.Shared.Core.Records;

namespace Mmo.Shared.Combat.Records;

/// <summary>
///     Ergebnis einer Schadensberechnung.
/// </summary>
public readonly record struct DamageResult(
    int RawDamage, // Ursprünglicher Schaden
    int MitigatedDamage, // Nach Armor/Resistances
    int AbsorbedDamage, // Von Schilden absorbiert
    int ActualDamage, // Tatsächlich erlittener Schaden
    int Overkill, // Überschuss (bei Tod)
    bool IsCritical,
    bool IsBlocked,
    bool IsDodged,
    bool IsParried,
    bool IsMiss,
    bool IsImmune,
    DamageType DamageType
);

/// <summary>
///     Ergebnis einer Heilung.
/// </summary>
public readonly record struct HealResult(
    int RawHeal, // Ursprüngliche Heilung
    int ActualHeal, // Tatsächliche Heilung
    int Overheal, // Überheilung
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
