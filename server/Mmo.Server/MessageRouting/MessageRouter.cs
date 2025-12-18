using Mmo.Server.Handlers.Base;
using Mmo.Server.Networking;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.MessageRouting;

/// <summary>
///     Routes incoming messages to the appropriate CategoryHandler.
///     Uses O(1) array lookup based on MessageCategory.
/// </summary>
public sealed class MessageRouter
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════
    
    private readonly ICategoryHandler?[] _handlers = new ICategoryHandler?[50];
    private readonly ILog _log;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public MessageRouter(ILog log)
    {
        _log = log;
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Registers a handler for its category.
    ///     Each category can only have one handler.
    /// </summary>
    /// <param name="handler">The category handler to register.</param>
    /// <exception cref="InvalidOperationException">If a handler is already registered for this category.</exception>
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
    ///     Routes a message to the appropriate handler based on its type.
    ///     SYNCHRONOUS - no async/await!
    ///     The handler is determined by calculating the category from the message type
    ///     using integer division (MessageType / 100).
    /// </summary>
    /// <param name="ctx">The message context containing connection and player information.</param>
    /// <param name="type">The message type.</param>
    /// <param name="message">The message to route.</param>
    public void Route(MessageContext ctx, MessageType type, INetworkMessage message)
    {
        // O(1) category calculation
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
