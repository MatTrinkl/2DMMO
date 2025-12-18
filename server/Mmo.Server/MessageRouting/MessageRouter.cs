using Mmo.Server.Handlers.Base;
using Mmo.Server.Networking;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.MessageRouting;

/// <summary>
///     Routet eingehende Messages an den zuständigen CategoryHandler.
///     Verwendet O(1) Array-Lookup basierend auf der MessageCategory.
/// </summary>
public sealed class MessageRouter
{
    private readonly ICategoryHandler?[] _handlers = new ICategoryHandler?[50];
    private readonly ILog _log;

    public MessageRouter(ILog log)
    {
        _log = log;
    }

    /// <summary>
    ///     Registriert einen Handler für seine Kategorie.
    /// </summary>
    public void RegisterHandler(ICategoryHandler handler)
    {
        int index = (int)handler.Category;

        if (_handlers[index] != null)
            throw new InvalidOperationException(
                $"Handler for category {handler.Category} already registered.");

        _handlers[index] = handler;
        _log.Debug("Registered handler for category {Category}", handler.Category);
    }

    /// <summary>
    ///     Routet eine Message an den zuständigen Handler.
    ///     SYNCHRON - kein async/await!
    /// </summary>
    public void Route(MessageContext ctx, MessageType type, INetworkMessage message)
    {
        // O(1) Kategorie-Berechnung
        int categoryIndex = (ushort)type / 100;

        if (categoryIndex >= _handlers.Length)
        {
            _log.Warn("Invalid message category index: {Index} for type {Type}", categoryIndex, type);
            return;
        }

        ICategoryHandler? handler = _handlers[categoryIndex];

        if (handler == null)
        {
            _log.Warn("No handler registered for category {Category} (type:  {Type})",
                (MessageCategory)categoryIndex, type);
            return;
        }

        if (!handler.CanHandle(type))
        {
            _log.Warn("Handler {Handler} cannot handle message type {Type}",
                handler.GetType().Name, type);
            return;
        }

        try
        {
            handler.Handle(ctx, type, message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error handling message {Type} in {Handler}",
                type, handler.GetType().Name);

            ctx.SendError("INTERNAL_ERROR", "An error occurred processing your request.");
        }
    }
}
