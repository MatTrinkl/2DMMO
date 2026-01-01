using Mmo.Shared.Character.Enums;
using Mmo.Shared.Generators;
using Mmo.Shared.Generators.Attributes;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Records;

namespace Mmo.Shared.Zones.Interfaces;

[GenerateDto(DtoName = "ZoneConfigDto")]
[GenerateListEntry("ZoneListEntry")]
public interface IZoneConfig
{
    [BaseData]
    ushort ZoneId { get; }

    [IgnoreData]
    string InternalName { get; }

    [BaseData]
    string DisplayName { get; }

    [IgnoreData]
    bool Contestable { get; }

    [OptionalData("ZoneListEntry")]
    PvpZoneType PvpType { get; }

    [IgnoreData]
    ZoneFlags ZoneFlags { get; }

    [BaseData]
    int MinLevel { get; }

    [BaseData]
    int MaxLevel { get; }

    [IgnoreData]
    Faction? OwningFaction { get; }

    [IgnoreData]
    ZoneBoundsConfig Bounds { get; }

    [IgnoreData]
    public int? MusicId { get; }

    [IgnoreData]
    public int? AmbienceId { get; }

    // Computed
    public bool IsPvpEnabled => PvpType != PvpZoneType.Sanctuary;
    public bool IsSanctuary => PvpType == PvpZoneType.Sanctuary;
    public bool IsInstance => ZoneFlags.HasFlag(ZoneFlags.IsInstance);
    public bool IsRaid => ZoneFlags.HasFlag(ZoneFlags.IsRaid);
    public bool IsCapital => ZoneFlags.HasFlag(ZoneFlags.IsCapital);
    public bool HasRestXp => ZoneFlags.HasFlag(ZoneFlags.HasRestXp);
    public bool AllowsMounting => !ZoneFlags.HasFlag(ZoneFlags.NoMounting);
    public bool AllowsFlying => !ZoneFlags.HasFlag(ZoneFlags.NoFlying);
}
