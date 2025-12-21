using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.AsyncTask.Interface;
using Mmo.Server.Connections;
using Mmo.Server.Connections.MessageHandler;
using Mmo.Server.Messages;
using Mmo.Server.MessageRouting;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.Player.Interfaces;
using Mmo.Server.PlayerService;
using Mmo.Server.Zones;
using Mmo.Server.Zones.Interfaces;
using Mmo.Server.Zones.Records;
using Mmo.Shared.Authentification.Interfaces;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Interfaces;
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
            services,
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
        services.AddSingleton<IPlayerService, Player.Service.PlayerService>();
        
        // Add mock services that would normally depend on GameServer
        // These are simple implementations that do nothing for testing
        services.AddSingleton<IBroadcastService>(sp => new MockBroadcastService());
        services.AddSingleton<IAsyncTaskService>(sp => new MockAsyncTaskService());
        services.AddSingleton<IZoneService>(sp => new MockZoneService());

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

/// <summary>
///     Mock implementation of IBroadcastService for testing.
/// </summary>
internal class MockBroadcastService : IBroadcastService
{
    public void BroadcastToZone(ushort zoneId, INetworkMessage message) { }
    public void BroadcastToZoneExcept(ushort zoneId, Guid excludedClientId, INetworkMessage message) { }
    public void BroadcastInRange(ushort zoneId, Position center, float radius, INetworkMessage message) { }
    public void BroadcastInRangeExcept(ushort zoneId, Position center, float radius, Guid excludedClientId, INetworkMessage message) { }
    public void SendToPlayer(ClientConnection client, INetworkMessage message) { }
    public void SendToPlayers(IEnumerable<ClientConnection> clients, INetworkMessage message) { }
    public void SendError(ClientConnection client, string code, string message, string? details, string? field) { }
    public void BroadcastGlobal(INetworkMessage message) { }
    public void BroadcastGlobalExcept(Guid excludedClientId, INetworkMessage message) { }
    public void BroadcastToParty(ServerPlayerCharacter characterInParty, INetworkMessage message) { }
    public void BroadcastToPartyExcept(ServerPlayerCharacter characterInPartyAndToExcluded, INetworkMessage message) { }
    public void BroadcastToGuild(ServerPlayerCharacter characterInGuild, INetworkMessage message) { }
    public void BroadcastToGuildExcept(ServerPlayerCharacter characterInGuildAndToExcluded, INetworkMessage message) { }
}

/// <summary>
///     Mock implementation of IAsyncTaskService for testing.
/// </summary>
internal class MockAsyncTaskService : IAsyncTaskService
{
    public void Run<TResult>(Guid connectionId, Func<Task<TResult>> asyncTask, Action<MessageContext, TResult> onComplete)
    {
        // For testing, execute the task but don't call the callback
        // The callback requires a MessageContext which we don't have in this mock
        Task.Run(async () =>
        {
            try
            {
                await asyncTask().ConfigureAwait(false);
            }
            catch
            {
                // Suppress exceptions in mock - real tests should verify specific behavior
            }
        });
    }

    public void Run(Guid connectionId, Func<Task> asyncTask, Action<MessageContext> onComplete)
    {
        // For testing, execute the task but don't call the callback
        // The callback requires a MessageContext which we don't have in this mock
        Task.Run(async () =>
        {
            try
            {
                await asyncTask().ConfigureAwait(false);
            }
            catch
            {
                // Suppress exceptions in mock - real tests should verify specific behavior
            }
        });
    }
}

/// <summary>
///     Mock implementation of IZoneService for testing.
/// </summary>
internal class MockZoneService : IZoneService
{
    public ZoneInfo? GetZoneInfo(ushort zoneId) => null;
    public IEnumerable<ZoneInfo> GetAllZones() => Enumerable.Empty<ZoneInfo>();
    public bool ZoneExists(ushort zoneId) => true;
    public ZoneTransferResult RequestZoneTransferAsync(Guid playerId, ushort targetZoneId, Position? targetPosition = null) => 
        ZoneTransferResult.Succeeded(targetZoneId, targetPosition ?? new Position(0, 0));
    public int GetPlayerCount(ushort zoneId) => 0;
    public IEnumerable<Guid> GetPlayersInZone(ushort zoneId) => Enumerable.Empty<Guid>();
}
