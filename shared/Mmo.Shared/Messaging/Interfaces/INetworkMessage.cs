using Mmo.Shared.Messaging.Enums;

namespace Mmo.Shared.Messaging.Interfaces;

/// <summary>
///     This Interface guarantees the use of the Message Type for serialization and deserialization.
/// </summary>
public interface INetworkMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    MessageType Type { get; }
}
