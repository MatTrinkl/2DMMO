using Mmo.Server.Networking;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Server.Handlers.Base;

/// <summary>
///     Interface for category handlers.
/// </summary>
public interface ICategoryHandler
{
    /// <summary>The category that this handler processes.</summary>
    MessageCategory Category { get; }

    /// <summary>Checks if this handler can process a specific MessageType.</summary>
    bool CanHandle(MessageType type);

    /// <summary>Processes a message (SYNCHRONOUS!).</summary>
    void Handle(MessageContext ctx, MessageType type, INetworkMessage message);
}
