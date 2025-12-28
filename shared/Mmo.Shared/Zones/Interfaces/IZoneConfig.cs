using Mmo.Shared.Character.Enums;
using Mmo.Shared.Generators;
using Mmo.Shared.Zones.Enums;
using Mmo.Shared.Zones.Records;

namespace Mmo.Shared.Zones.Interfaces;

[GenerateDto(DtoName = "ZoneConfigDto")]
public interface IZoneConfig
{
    ushort ZoneId { get; }
    string InternalName { get; }
    string DisplayName { get; }
    bool Contestable { get; }
    PvpZoneType PvpType { get; }
    ZoneFlags ZoneFlags { get; }
    int MinLevel { get; }
    int MaxLevel { get; }
    Faction? OwningFaction { get; }
    ZoneBoundsConfig Bounds { get; }

    public int? MusicId { get; }
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
