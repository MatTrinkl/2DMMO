using Mmo.Server.MessageRouting.Interfaces;
using Mmo.Server.Messages;
using Mmo.Server.Network.Interfaces;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.MessageRouting.Handler;

/// <summary>
///     Base class for all category handlers.
///     Each handler processes one MessageCategory (100-block).
///     Uses O(1) array lookup for sub-routing.
///     IMPORTANT:
///     - Handler methods are SYNCHRONOUS (void, not async)
///     - For async operations: use ctx.RunAsync()
///     - Send only via ctx.Send() (queued for Output phase)
/// </summary>
public abstract class BaseCategoryHandler : ICategoryHandler
{
    private readonly Action<MessageContext, INetworkMessage>?[] _handlers =
        new Action<MessageContext, INetworkMessage>?[100];

    private readonly ILog _log;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    protected BaseCategoryHandler(ILog log)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        RegisterHandlers();
    }

    // ═══════════════════════════════════════════════════════════════
    // ABSTRACT MEMBERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     The category this handler processes.
    /// </summary>
    public abstract MessageCategory Category { get; }

    // ═══════════════════════════════════════════════════════════════
    // ICategoryHandler IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Checks if this handler can handle a specific MessageType.
    /// </summary>
    public bool CanHandle(MessageType type)
    {
        int index = (ushort)type % 100;
        return index < 100 && _handlers[index] != null;
    }

    /// <summary>
    ///     Verarbeitet eine Message.
    /// </summary>
    public void Handle(MessageContext ctx, MessageType type, INetworkMessage message)
    {
        int index = (ushort)type % 100;
        Action<MessageContext, INetworkMessage>? handler = _handlers[index];

        if (handler == null)
        {
            _log.Warn("No handler registered for {MessageType} in {Handler}", type, GetType().Name);
            return;
        }

        try
        {
            handler(ctx, message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error in handler for {MessageType} in {Handler}", type, GetType().Name);
            ctx.GetService<IBroadcastService>().SendError(ctx.Connection, "INTERNAL_ERROR",
                "An error occurred processing your request", null, null);
        }
    }

    /// <summary>
    ///     Called by subclasses to register handlers.
    /// </summary>
    protected abstract void RegisterHandlers();

    // ═══════════════════════════════════════════════════════════════
    // REGISTRATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Registers a handler for a specific MessageType.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="type">The MessageType (must be in this category!).</param>
    /// <param name="handler">The handler method.</param>
    protected void Register<TMessage>(MessageType type, Action<MessageContext, TMessage> handler)
        where TMessage : INetworkMessage
    {
        // Validierung: MessageType muss zu dieser Kategorie gehören
        int expectedCategory = (int)Category;
        int actualCategory = (ushort)type / 100;

        if (actualCategory != expectedCategory)
            throw new InvalidOperationException(
                $"MessageType {type} ({(ushort)type}) belongs to category {actualCategory}, " +
                $"but this handler is for category {expectedCategory} ({Category})");

        int index = (ushort)type % 100;

        if (_handlers[index] != null)
            throw new InvalidOperationException(
                $"Handler for MessageType {type} already registered in {GetType().Name}");

        _handlers[index] = (ctx, msg) => handler(ctx, (TMessage)msg);

        _log.Debug("Registered handler for {MessageType} in {Handler}", type, GetType().Name);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Checks if the player is authenticated.
    ///     Automatically sends error if not.
    /// </summary>
    protected bool RequireAuthenticated(MessageContext ctx)
    {
        if (ctx.IsAuthenticated) return true;
        ctx.GetService<IBroadcastService>()
            .SendError(ctx.Connection, "NOT_AUTHENTICATED", "You must be logged in", null, null);
        return false;
    }

    /// <summary>
    ///     Checks if the player has a character.
    ///     Automatically sends error if not.
    /// </summary>
    protected bool RequireCharacter(MessageContext ctx)
    {
        if (ctx.HasCharacter) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection, "NO_CHARACTER",
            "You must select a character first", null, null);
        return false;
    }

    /// <summary>
    ///     Checks if the player is authenticated AND has a character.
    ///     Automatically sends error if not.
    /// </summary>
    protected bool RequireInGame(MessageContext ctx) => RequireAuthenticated(ctx) && RequireCharacter(ctx);

    /// <summary>
    ///     Checks if the player is a Game Master.
    ///     Automatically sends error if not.
    /// </summary>
    protected bool RequireGameMaster(MessageContext ctx)
    {
        if (ctx.IsGameMaster) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection, "PERMISSION_DENIED",
            "This action requires Game Master privileges", null, null);
        return false;
    }

    /// <summary>
    ///     Checks if the player is an Admin.
    ///     Automatically sends error if not.
    /// </summary>
    protected bool RequireAdmin(MessageContext ctx)
    {
        if (ctx.IsAdmin) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection, "PERMISSION_DENIED",
            "This action requires Admin privileges", null, null);
        return false;
    }

    /// <summary>
    ///     Checks if the player is NOT muted.
    ///     Automatically sends error if muted.
    /// </summary>
    protected bool RequireNotMuted(MessageContext ctx)
    {
        if (!ctx.IsMuted) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection, "MUTED",
            "You are muted and cannot perform this action", null, null);
        return false;
    }
}
