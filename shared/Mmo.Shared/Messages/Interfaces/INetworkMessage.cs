using Mmo.Shared.Enums.Messages;

namespace Mmo.Shared.Interfaces;

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
