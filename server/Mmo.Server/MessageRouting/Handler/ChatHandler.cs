using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handler;

/// <summary>
///     Handler for Chat (MessageCategory.Chat, range 400-499).
///     Handles all chat-related messages (1-to-1, zone, guild, global, etc.).
/// </summary>
public class ChatHandler : BaseCategoryHandler
{
    public ChatHandler(ILog log) : base(log)
    {
    }

    public override MessageCategory Category => MessageCategory.Chat;

    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for chat messages
        // Example:
        // Register(MessageType.ChatMessage, HandleChatMessage);
        // Register(MessageType.ChatWhisper, HandleWhisper);
    }

    // Example handler method structure:
    // private void HandleChatMessage(MessageContext ctx, ChatMessage msg)
    // {
    //     if (!RequireInGame(ctx)) return;
    //     if (!RequireNotMuted(ctx)) return;
    //     
    //     var chatService = ctx.GetService<IChatService>();
    //     // Handler logic here - delegate to service
    // }
}
