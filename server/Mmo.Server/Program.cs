using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.GameLoop;
using Mmo.Server.Handlers.Base;
using Mmo.Server.Logging;
using Mmo.Server.MessageRouting;
using Mmo.Server.MessageRouting.MessageHandler;
using Mmo.Server.Networking;
using Mmo.Server.Services.Authentication;
using Mmo.Server.Services.Player;
using Mmo.Server.Zones;
using Mmo.Shared.Interfaces;

namespace Mmo.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("  MMO Server Starting...");
        Console.WriteLine("═══════════════════════════════════════════");

        // ════════════════════════════════════════════════════════════
        // 1. DI-Container konfigurieren
        // ════════════════════════════════════════════════════════════
        ServiceCollection services = ConfigureServices();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // ════════════════════════════════════════════════════════════
        // 2. Services aus DI holen
        // ════════════════════════════════════════════════════════════
        ILog log = serviceProvider.GetRequiredService<ILog>();
        NetworkServer networkServer = serviceProvider.GetRequiredService<NetworkServer>();
        GameServer gameServer = serviceProvider.GetRequiredService<GameServer>();
        MessageRouter messageRouter = serviceProvider.GetRequiredService<MessageRouter>();

        // ════════════════════════════════════════════════════════════
        // 3. Handler registrieren
        // ════════════════════════════════════════════════════════════
        RegisterHandlers(serviceProvider, messageRouter);

        // ════════════════════════════════════════════════════════════
        // 4. Server starten
        // ════════════════════════════════════════════════════════════
        networkServer.Start();
        gameServer.Start();

        log.Info("Server is running.  Press Ctrl+C to stop.");

        // ════════════════════════════════════════════════════════════
        // 5. Graceful Shutdown
        // ════════════════════════════════════════════════════════════
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            log.Info("Shutdown signal received.. .");
            cts.Cancel();
        };

        // Warten bis Ctrl+C
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

        // Dispose ServiceProvider (ruft Dispose auf allen Services auf)
        if (serviceProvider is IDisposable disposable) disposable.Dispose();

        log.Info("Server stopped.  Goodbye!");
    }

    /// <summary>
    ///     Konfiguriert alle Services für den DI-Container.
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
        services.AddSingleton<ServerConfiguration>(sp =>
        {
            return new ServerConfiguration
            {
                Port = 7777,
                TickRate = 20,
                MaxConnections = 1000,
                ConnectionTimeout = TimeSpan.FromSeconds(30)
            };
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
        services.AddSingleton<IAuthenticationService, AuthenticationService>();

        // Player
        services.AddSingleton<IPlayerService, PlayerService>();

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
        // services.AddSingleton<MovementHandler>();
        // services. AddSingleton<CombatHandler>();
        // services.AddSingleton<ChatHandler>();
        // services.AddSingleton<InventoryHandler>();
        // services.AddSingleton<SocialHandler>();
        // services.AddSingleton<AdminHandler>();

        return services;
    }

    /// <summary>
    ///     Registriert alle Handler beim MessageRouter.
    /// </summary>
    private static void RegisterHandlers(IServiceProvider serviceProvider, MessageRouter router)
    {
        ILog log = serviceProvider.GetRequiredService<ILog>();

        // Alle Handler aus DI holen und registrieren
        Type[] handlerTypes = new[]
        {
            typeof(ConnectionHandler)
            // typeof(MovementHandler),
            // typeof(CombatHandler),
            // typeof(ChatHandler),
            // typeof(InventoryHandler),
            // typeof(SocialHandler),
            // typeof(AdminHandler),
        };

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
///     Server-Konfiguration.
/// </summary>
public class ServerConfiguration
{
    public int Port { get; set; } = 7777;
    public int TickRate { get; set; } = 20;
    public int MaxConnections { get; set; } = 1000;
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
