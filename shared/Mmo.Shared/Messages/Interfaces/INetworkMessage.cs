using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages.Interfaces;

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
