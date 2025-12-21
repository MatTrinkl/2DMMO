using Mmo.Server.MessageRouting.Handler;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handlers;

/// <summary>
///     Handler for Chat messages (Category 4, range 400-499).
///     Handles chat messages, whispers, channel operations.
/// </summary>
public class ChatHandler : BaseCategoryHandler
{
    public ChatHandler(ILog log) : base(log)
    {
    }

    /// <inheritdoc />
    public override MessageCategory Category => MessageCategory.Chat;

    /// <inheritdoc />
    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for specific Chat message types
        // Example:
        // Register(MessageType.ChatMessage, HandleChatMessage);
        // Register(MessageType.ChatWhisper, HandleChatWhisper);
        // Register(MessageType.ChatChannelJoin, HandleChatChannelJoin);
    }

    // TODO: Add handler methods
    // Example:
    // private void HandleChatMessage(MessageContext ctx, ChatMessageMessage msg) { }
}
