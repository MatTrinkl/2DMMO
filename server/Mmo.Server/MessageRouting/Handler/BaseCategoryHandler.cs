using Mmo.Server.MessageRouting.Interfaces;
using Mmo.Server.Messages;
using Mmo.Server.Network.Interfaces;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.MessageRouting.Handler;

/// <summary>
///     Basisklasse für alle Category-Handler.
///     Jeder Handler verarbeitet eine MessageCategory (100er-Block).
///     Verwendet O(1) Array-Lookup für Sub-Routing.
///     WICHTIG:
///     - Handler-Methoden sind SYNCHRON (void, nicht async)
///     - Für async Operations:  ctx.RunAsync() verwenden
///     - Senden nur über ctx.Send() (queued für Output-Phase)
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
    ///     Die Kategorie die dieser Handler verarbeitet.
    /// </summary>
    public abstract MessageCategory Category { get; }

    // ═══════════════════════════════════════════════════════════════
    // ICategoryHandler IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Prüft ob dieser Handler einen bestimmten MessageType verarbeiten kann.
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
            ctx.GetService<IBroadcastService>().SendError(ctx.Connection,"INTERNAL_ERROR", "An error occurred processing your request", null, null);
        }
    }

    /// <summary>
    ///     Wird von Subklassen überschrieben um Handler zu registrieren.
    /// </summary>
    protected abstract void RegisterHandlers();

    // ═══════════════════════════════════════════════════════════════
    // REGISTRATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Registriert einen Handler für einen bestimmten MessageType.
    /// </summary>
    /// <typeparam name="TMessage">Der Message-Typ. </typeparam>
    /// <param name="type">Der MessageType (muss in dieser Kategorie sein!).</param>
    /// <param name="handler">Die Handler-Methode.</param>
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
    ///     Prüft ob der Spieler authentifiziert ist.
    ///     Sendet automatisch Error wenn nicht.
    /// </summary>
    protected bool RequireAuthenticated(MessageContext ctx)
    {
        if (ctx.IsAuthenticated) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection,"NOT_AUTHENTICATED", "You must be logged in", null, null);
        return false;

    }

    /// <summary>
    ///     Prüft ob der Spieler einen Charakter hat.
    ///     Sendet automatisch Error wenn nicht.
    /// </summary>
    protected bool RequireCharacter(MessageContext ctx)
    {
        if (ctx.HasCharacter) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection,"NO_CHARACTER", "You must select a character first", null, null);
        return false;

    }

    /// <summary>
    ///     Prüft ob der Spieler authentifiziert ist UND einen Charakter hat.
    ///     Sendet automatisch Error wenn nicht.
    /// </summary>
    protected bool RequireInGame(MessageContext ctx) => RequireAuthenticated(ctx) && RequireCharacter(ctx);

    /// <summary>
    ///     Prüft ob der Spieler Game Master ist.
    ///     Sendet automatisch Error wenn nicht.
    /// </summary>
    protected bool RequireGameMaster(MessageContext ctx)
    {
        if (ctx.IsGameMaster) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection,"PERMISSION_DENIED", "This action requires Game Master privileges", null, null);
        return false;

    }

    /// <summary>
    ///     Prüft ob der Spieler Admin ist.
    ///     Sendet automatisch Error wenn nicht.
    /// </summary>
    protected bool RequireAdmin(MessageContext ctx)
    {
        if (ctx.IsAdmin) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection,"PERMISSION_DENIED", "This action requires Admin privileges", null, null);
        return false;

    }

    /// <summary>
    ///     Prüft ob der Spieler NICHT gemutet ist.
    ///     Sendet automatisch Error wenn gemutet.
    /// </summary>
    protected bool RequireNotMuted(MessageContext ctx)
    {
        if (!ctx.IsMuted) return true;
        ctx.GetService<IBroadcastService>().SendError(ctx.Connection,"MUTED", "You are muted and cannot perform this action", null, null);
        return false;

    }
}
