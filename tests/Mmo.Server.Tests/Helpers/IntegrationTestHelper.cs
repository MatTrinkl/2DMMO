using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.AuthenticationService.Interfaces;
using Mmo.Server.Connections;
using Mmo.Server.Connections.Handler;
using Mmo.Server.MessageRouting;
using Mmo.Server.PlayerService;
using Mmo.Server.PlayerService.Interfaces;
using Mmo.Server.Zones;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.Tests.Helpers;

public static class TestHelpers
{
    private static readonly MockLog _mockLog = new();
    private static readonly MockNetworkServer _mockNetworkServer = new(_mockLog, true);

    /// <summary>
    ///     Creates a GameServer instance for testing with all required dependencies.
    /// </summary>
    public static Core.GameServer CreateTestGameServer(
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
        messageRouter ??= CreateMessageRouter(log, zoneManager);
        services ??= CreateTestServices(log, zoneManager);

        var gameServer = new Core.GameServer(
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
            zoneManager.RegisterZone(zone);
        }

        return zoneManager;
    }

    /// <summary>
    ///     Creates a MessageRouter for testing with ConnectionHandler registered.
    /// </summary>
    public static MessageRouter CreateMessageRouter(ILog log, ZoneManager? zoneManager = null)
    {
        var router = new MessageRouter(log);

        // Register ConnectionHandler to handle login and connection messages
        zoneManager ??= CreateDefaultZoneManager();
        IServiceProvider services = CreateTestServices(log, zoneManager);
        var connectionHandler = new ConnectionHandler(
            services.GetRequiredService<IAuthenticationService>(),
            services.GetRequiredService<IPlayerService>(),
            zoneManager,
            log
        );
        router.RegisterHandler(connectionHandler);

        return router;
    }

    /// <summary>
    ///     Creates a test ServiceProvider with all required services.
    /// </summary>
    public static IServiceProvider CreateTestServices(ILog log, ZoneManager? zoneManager = null)
    {
        var services = new ServiceCollection();

        services.AddSingleton(log);
        services.AddSingleton(zoneManager ?? CreateDefaultZoneManager());
        services.AddSingleton<IAuthenticationService, AuthenticationService.AuthenticationService>();
        services.AddSingleton<IPlayerService, PlayerService.PlayerService>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    ///     Creates a ServerPlayer for testing.
    /// </summary>
    public static ServerPlayerCharacter CreateServerPlayer(
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
        Guid connId = connectionId ?? Guid.NewGuid();
        ClientConnection connection = _mockNetworkServer.GetOrCreateMockConnection(connId);
        return new ServerPlayerCharacter(entity, connection);
    }

    /// <summary>
    ///     Creates multiple ServerPlayers for testing.
    /// </summary>
    public static List<ServerPlayerCharacter> CreateMultiplePlayers(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => CreateServerPlayer($"Player{i}"))
            .ToList();
    }
}
