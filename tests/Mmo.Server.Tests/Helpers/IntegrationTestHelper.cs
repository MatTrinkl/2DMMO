using Mmo.Server.Entities;
using Mmo.Server.Zones;
using Mmo.Shared.Entities;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;

namespace Mmo.Server.Tests.Helpers;

public static class TestHelpers
{
    public static ZoneManager CreateZoneManagerWithZones(params (ushort id, string name)[] zones)
    {
        var defaultZone = new Zone(0, "default", new ZoneBounds(0, 0, 1000, 1000));
        var zoneManager = new ZoneManager(0, defaultZone);

        foreach (var (id, name) in zones)
        {
            if (id == 0) continue; // Default already exists
            var zone = new Zone(id, name, new ZoneBounds(0, 0, 1000, 1000));
            zoneManager.RegisterZone(id, zone);
        }

        return zoneManager;
    }

    public static ServerPlayer CreateServerPlayer(
        string name = "TestPlayer",
        Guid? persistentId = null,
        Guid? connectionId = null,
        float x = 100,
        float y = 100)
    {
        var entity = new PlayerEntity(
            persistentId ?? Guid.NewGuid(),
            name,
            new Position(x, y)
        );
        return new ServerPlayer(entity, connectionId ?? Guid.NewGuid());
    }

    public static List<ServerPlayer> CreateMultiplePlayers(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => CreateServerPlayer(name: $"Player{i}"))
            .ToList();
    }
}
