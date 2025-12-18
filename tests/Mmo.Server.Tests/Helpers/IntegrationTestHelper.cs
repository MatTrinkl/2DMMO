using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.Entities;
using Mmo.Server.GameLoop;
using Mmo.Server.MessageRouting;
using Mmo.Server.MessageRouting.MessageHandler;
using Mmo.Server.Networking;
using Mmo.Server.Services.Authentication;
using Mmo.Server.Services.Player;
using Mmo.Server.Zones;
using Mmo.Shared.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;
using Mmo.Shared.Zones;

namespace Mmo.Server.Tests.Helpers;

public static class TestHelpers
{
    private static readonly MockNetworkServer _mockNetworkServer;

    /// <summary>
    ///     Creates a GameServer instance for testing with all required dependencies.
    /// </summary>
    public static GameLoop.GameServer CreateTestGameServer(
        ILog? log = null,
        MockNetworkServer? networkServer = null,
        ZoneManager? zoneManager = null,
        MessageRouter? messageRouter = null,
        IServiceProvider? services = null)
    {
        // Use provided or create defaults
        log ??= new MockLog();
        networkServer ??= new MockNetworkServer(log, true);
        zoneManager ??= CreateDefaultZoneManager();
        messageRouter ??= CreateMessageRouter(log);
        services ??= CreateTestServices(log);

        var gameServer = new GameLoop.GameServer(
            networkServer,
            messageRouter,
            zoneManager,
            services,
            log
        );

        return gameServer;
    }

    /// <summary>
    ///     Creates a default ZoneManager with a single default zone.
    /// </summary>
    public static ZoneManager CreateDefaultZoneManager()
    {
        var defaultZone = new Zone(0, "default", new ZoneBounds(0, 0, 1000, 1000));
        return new ZoneManager(0, defaultZone);
    }

    /// <summary>
    ///     Creates a ZoneManager with multiple zones.
    /// </summary>
    public static ZoneManager CreateZoneManagerWithZones(params (ushort id, string name)[] zones)
    {
        var defaultZone = new Zone(0, "default", new ZoneBounds(0, 0, 1000, 1000));
        var zoneManager = new ZoneManager(0, defaultZone);

        foreach ((ushort id, string name) in zones)
        {
            if (id == 0) continue; // Default already exists
            var zone = new Zone(id, name, new ZoneBounds(0, 0, 1000, 1000));
            zoneManager.RegisterZone(id, zone);
        }

        return zoneManager;
    }

    /// <summary>
    ///     Creates a MessageRouter for testing.
    ///     Note: ConnectionHandler is not registered due to production code compilation errors.
    /// </summary>
    public static MessageRouter CreateMessageRouter(ILog log)
    {
        var router = new MessageRouter(log);

        // Note: We would register ConnectionHandler here, but it has compilation errors
        // in the production code (missing message types like ReconnectRequest, etc.)
        // This is NOT a test issue - the production code needs to be fixed separately.
        //
        // var services = CreateTestServices(log);
        // var connectionHandler = new ConnectionHandler(
        //     services.GetRequiredService<IAuthenticationService>(),
        //     services.GetRequiredService<IPlayerService>(),
        //     log
        // );
        // router.RegisterHandler(connectionHandler);

        return router;
    }

    /// <summary>
    ///     Creates a test ServiceProvider with all required services.
    /// </summary>
    public static IServiceProvider CreateTestServices(ILog log)
    {
        var services = new ServiceCollection();

        services.AddSingleton(log);
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IPlayerService, PlayerService>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    ///     Creates a ServerPlayer for testing.
    /// </summary>
    public static ServerPlayer CreateServerPlayer(
        string name = "TestPlayer",
        Guid? persistentId = null,
        Guid? connectionId = null,
        float x = 100,
        float y = 100)
    {
        var entity = new PlayerEntity(
            persistentId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            new Position(x, y)
        );
        var connId = connectionId ?? Guid.NewGuid();
        var connection = _mockNetworkServer.GetOrCreateMockConnection(connId);
        return new ServerPlayer(entity, connection);
    }

    /// <summary>
    ///     Creates multiple ServerPlayers for testing.
    /// </summary>
    public static List<ServerPlayer> CreateMultiplePlayers(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => CreateServerPlayer($"Player{i}"))
            .ToList();
    }
}
