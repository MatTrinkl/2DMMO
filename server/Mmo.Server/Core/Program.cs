using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.AsyncTask.Interface;
using Mmo.Server.Connections.MessageHandler;
using Mmo.Server.Logging;
using Mmo.Server.MessageRouting;
using Mmo.Server.MessageRouting.Handler;
using Mmo.Server.MessageRouting.Interfaces;
using Mmo.Server.Network;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.Network.Services;
using Mmo.Server.Player.Interfaces;
using Mmo.Server.Zones;
using Mmo.Server.Zones.Interfaces;
using Mmo.Server.Zones.MessageHandler;
using Mmo.Server.Zones.Services;
using Mmo.Shared.Authentification.Interfaces;
using Mmo.Shared.Core.Constants;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Entities.Interfaces;
using Mmo.Shared.Movement.Interfaces;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.Core;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("  MMO Server Starting...");
        Console.WriteLine("═══════════════════════════════════════════");

        // ════════════════════════════════════════════════════════════
        // 1. configure DI-Container
        // ════════════════════════════════════════════════════════════
        ServiceCollection services = ConfigureServices();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // ════════════════════════════════════════════════════════════
        // 2. Get Services from DI
        // ════════════════════════════════════════════════════════════
        ILog log = serviceProvider.GetRequiredService<ILog>();
        NetworkServer networkServer = serviceProvider.GetRequiredService<NetworkServer>();
        GameServer gameServer = serviceProvider.GetRequiredService<GameServer>();
        MessageRouter messageRouter = serviceProvider.GetRequiredService<MessageRouter>();

        // ════════════════════════════════════════════════════════════
        // 3.Register Handler
        // ════════════════════════════════════════════════════════════
        RegisterHandlers(serviceProvider, messageRouter);

        // ════════════════════════════════════════════════════════════
        // 4. Starting Server
        // ════════════════════════════════════════════════════════════
        networkServer.Start();
        gameServer.Start();

        log.Info("Server is running.  Press Ctrl+C to stop.");

        // ════════════════════════════════════════════════════════════
        // 5. Graceful Shutdown
        // ════════════════════════════════════════════════════════════
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            log.Info("Shutdown signal received.. .");
            // ReSharper disable once AccessToDisposedClosure
            cts.Cancel();
        };

        // Wait until Ctrl+C
        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (TaskCanceledException)
        {
            // Normal shutdown
        }

        // ════════════════════════════════════════════════════════════
        // 6. Cleanup
        // ════════════════════════════════════════════════════════════
        log.Info("Shutting down...");

        gameServer.Stop();
        networkServer.Stop();

        // Dispose ServiceProvider (calls Dispose on all services)
        if (serviceProvider is IDisposable disposable) disposable.Dispose();

        log.Info("Server stopped.  Goodbye!");
    }

    /// <summary>
    ///     Configures all services for the DI-Container.
    /// </summary>
    private static ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // ════════════════════════════════════════════════════════════
        // LOGGING
        // ════════════════════════════════════════════════════════════
        services.AddSingleton<ILog, ConsoleLog>(); // Oder: SerilogAdapter, etc.

        // ════════════════════════════════════════════════════════════
        // CONFIGURATION
        // ════════════════════════════════════════════════════════════
        services.AddSingleton<ServerConfiguration>(_ => new ServerConfiguration
        {
            Port = 7777,
            TickRate = 20,
            MaxConnections = 1000,
            ConnectionTimeout = TimeSpan.FromSeconds(30)
        });

        // ════════════════════════════════════════════════════════════
        // NETWORKING
        // ════════════════════════════════════════════════════════════
        services.AddSingleton<NetworkServer>(sp =>
        {
            ServerConfiguration config = sp.GetRequiredService<ServerConfiguration>();
            ILog log = sp.GetRequiredService<ILog>();

            return new NetworkServer(log, config.Port);
        });

        // ════════════════════════════════════════════════════════════
        // GAME SYSTEMS
        // ════════════════════════════════════════════════════════════
        services.AddSingleton<ZoneManager>();
        services.AddSingleton<MessageRouter>();

        services.AddSingleton<GameServer>(sp =>
        {
            NetworkServer networkServer = sp.GetRequiredService<NetworkServer>();
            MessageRouter messageRouter = sp.GetRequiredService<MessageRouter>();
            ZoneManager zoneManager = sp.GetRequiredService<ZoneManager>();
            ILog log = sp.GetRequiredService<ILog>();
            ServerConfiguration config = sp.GetRequiredService<ServerConfiguration>();

            var gameServer = new GameServer(networkServer, messageRouter, zoneManager, sp, log)
            {
                TargetTickRate = config.TickRate
            };

            return gameServer;
        });

        // ════════════════════════════════════════════════════════════
        // SERVICES (Business Logic)
        // ════════════════════════════════════════════════════════════

        // Authentication
        services.AddSingleton<IAuthenticationService, AuthenticationService.AuthenticationService>();

        // Player
        services.AddSingleton<IPlayerService, Player.Service.PlayerService>();

        //Broadcast
        services.AddSingleton<IBroadcastService, BroadcastService>();

        //AsyncTask
        services.AddSingleton<IAsyncTaskService, AsyncTask.Services.AsyncTaskService>();

        //Zone
        services.AddSingleton<IZoneService, ZoneService>();

        //Entity
        services.AddSingleton<IEntityService, Entities.Services.EntityService>();

        //Movement
        //services.AddSingleton<IMovementService, MovementService>();

        // TODO:  Weitere Services
        // services.AddSingleton<IChatService, ChatService>();
        // services.AddSingleton<ICombatService, CombatService>();
        // services.AddSingleton<IInventoryService, InventoryService>();
        // services.AddSingleton<IGuildService, GuildService>();
        // services.AddSingleton<IPartyService, PartyService>();
        // services.AddSingleton<IAuctionService, AuctionService>();
        // services.AddSingleton<IMailService, MailService>();

        // ════════════════════════════════════════════════════════════
        // HANDLERS
        // ════════════════════════════════════════════════════════════
        services.AddSingleton<ConnectionHandler>();
        services.AddSingleton<ZoneHandler>();
        services.AddSingleton<ZoneEventHandler>();
        services.AddSingleton<MovementHandler>();
        services.AddSingleton<CombatHandler>();
        services.AddSingleton<ChatHandler>();
        services.AddSingleton<PingHandler>();
        // TODO: Future handlers
        // services.AddSingleton<InventoryHandler>();
        // services.AddSingleton<SocialHandler>();
        // services.AddSingleton<AdminHandler>();

        return services;
    }

    /// <summary>
    ///     Register all handler in MessageRouter.
    /// </summary>
    private static void RegisterHandlers(IServiceProvider serviceProvider, MessageRouter router)
    {
        ILog log = serviceProvider.GetRequiredService<ILog>();

        Type[] handlerTypes =
        [
            typeof(ConnectionHandler),
            typeof(ZoneHandler),
            typeof(ZoneEventHandler),
            typeof(MovementHandler),
            typeof(CombatHandler),
            typeof(ChatHandler),
            typeof(PingHandler)
            // TODO: Future handlers
            // typeof(InventoryHandler),
            // typeof(SocialHandler),
            // typeof(AdminHandler),
        ];

        foreach (Type handlerType in handlerTypes)
        {
            var handler = (ICategoryHandler)serviceProvider.GetRequiredService(handlerType);
            router.RegisterHandler(handler);
            log.Info("Registered handler:  {Handler} for category {Category}",
                handlerType.Name, handler.Category);
        }
    }
}

/// <summary>
///     Server-Configuration
/// </summary>
public class ServerConfiguration
{
    public int Port { get; init; } = SharedConstants.DefaultPort;
    public int TickRate { get; init; } = SharedConstants.TickRate;
    public int MaxConnections { get; init; } = SharedConstants.MaxConnection;

    public TimeSpan ConnectionTimeout { get; init; } =
        TimeSpan.FromSeconds(SharedConstants.TimeToConnectionDeadInSeconds);
}
