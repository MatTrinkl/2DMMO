using Mmo.Server.Networking;
using Mmo.Shared. Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Server. Handlers.Base;

/// <summary>
///     Interface für Category-Handler.
/// </summary>
public interface ICategoryHandler
{
    /// <summary>Die Kategorie die dieser Handler verarbeitet.</summary>
    MessageCategory Category { get; }

    /// <summary>Prüft ob dieser Handler einen bestimmten MessageType verarbeiten kann. </summary>
    bool CanHandle(MessageType type);

    /// <summary>Verarbeitet eine Message (SYNCHRON!).</summary>
    void Handle(MessageContext ctx, MessageType type, INetworkMessage message);
}
